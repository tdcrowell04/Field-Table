using System;
using System.Collections.Generic;
using System.Linq;
using static FieldTable.Sim.MissionKit;

namespace FieldTable.Sim
{
    /// <summary>
    /// The compound raid (ledger round 16): a walled farm, a wanted man known by a code name, his family, and a pickup
    /// point to bring him to. The enemy guards him and fights, slips him out the back, or he isn't there at all and the
    /// family can say where he went.
    /// </summary>
    public static class CompoundRaid
    {
        public const string Guard = "guard", Slip = "slip", Gone = "gone";
        public static readonly IReadOnlyList<string> Goals = new[] { Guard, Slip, Gone };

        static readonly Dictionary<string, (string said, string truth)> GoalWords = Words(
            (Guard, "they'll fight for him at the farm", "fight for him at the farm"),
            (Slip, "they'll get him out the back if you come", "slip him out the back"),
            (Gone, "he left the farm days ago", "hide him somewhere else"));

        /// <summary>The squad starts about this far from the farm, and never nearer than the least.</summary>
        const float StartDistance = 300f, StartNearest = 150f;

        /// <summary>He slips out once a Marine is this close to the farm, or the guards see one or get shot at.</summary>
        public const float SlipWhenWithin = 150f;

        /// <summary>A Marine this close to his body identifies him, and a prisoner this close to the pickup point is there.</summary>
        public const float IdentifyWithin = 5f, OnPickup = 15f;

        /// <summary>Makes the mission. A die picks the enemy's goal unless one is asked for (guard, slip, or gone).</summary>
        public static Mission Create(GameData data, NamePools names, ulong seed, string goal = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (names == null) throw new ArgumentNullException(nameof(names));
            // A map with a village somewhere, and open ground for the farm well in from the edges.
            Vec2? Site(MapGrid m) => OpenSite(m, new Dice(seed, 21), 34f, 30f, 250f);
            var (mapData, map) = MakeMap(data, names, seed, "Compound raid " + seed, r => r.Require.Add(MapFeatures.Villages), (_, m) => Site(m) != null);
            Vec2 c = Site(map) ?? OpenSite(map, new Dice(seed, 21), 34f, 30f, 150f, 40f) ?? new Vec2(map.WidthMeters / 2f, map.HeightMeters / 2f);
            var rng = new Dice(seed, 11);

            // The squad comes in from the side with the most room. The gate faces it; a gap in the back wall leads out the far side.
            var sides = new[] { new Vec2(0, -1), new Vec2(1, 0), new Vec2(0, 1), new Vec2(-1, 0) };
            Vec2 front = sides.OrderByDescending(d => Vec2.Distance(c, EdgeToward(map, c, d)) + Next(rng) * 50f).First();
            Vec2 right = new Vec2(-front.Y, front.X);
            bool northSouth = Math.Abs(front.Y) > 0.5f;
            float hw = 18f + 7f * Next(rng), hd = 15f + 5f * Next(rng);
            Vec2 At(float along, float across) => c + front * along + right * across;
            // The farmhouse runs along the back wall with room to walk behind it; the back gap is off to one side of it.
            float houseAt = (hw - 9f) * (2f * Next(rng) - 1f);
            float side = houseAt >= 0f ? -1f : 1f;
            float gap = side * (hw - 3f - (hw - 11f + Math.Abs(houseAt)) * Next(rng));
            string farmName = Pick(names.Places.Where(n => !map.Landmarks.Any(l => l.Label.StartsWith(n, StringComparison.Ordinal))).ToList(), rng) + " farm";

            var corners = new[] { At(hd, -hw), At(hd, hw), At(-hd, hw), At(-hd, -hw) };
            Vec2 min = new Vec2(corners.Min(p => p.X), corners.Min(p => p.Y)), max = new Vec2(corners.Max(p => p.X), corners.Max(p => p.Y));
            mapData.Features.Add(RectFeature(mapData, "yard", min, max, farmName, "walled farm", "the farm", "farm", "the compound", "compound"));
            mapData.Features.Add(LineFeature(mapData, "wall", 1f, new[] { At(hd, -hw), At(hd, -2f) }));
            mapData.Features.Add(LineFeature(mapData, "wall", 1f, new[] { At(hd, 2f), At(hd, hw), At(-hd, hw), At(-hd, gap + 1f) }));
            mapData.Features.Add(LineFeature(mapData, "wall", 1f, new[] { At(-hd, gap - 1f), At(-hd, -hw), At(hd, -hw) }));
            // The shed sits in a front corner.
            Vec2 houseCenter = At(-hd + 8f, houseAt);
            var house = BuildingFeature(mapData, houseCenter, northSouth ? 14f : 10f, northSouth ? 10f : 14f);
            mapData.Features.Add(house);
            float shedSide = Next(rng) < 0.5f ? -1f : 1f;
            Vec2 shedCenter = At(hd - 5f, shedSide * (hw - 6f));
            var shed = BuildingFeature(mapData, shedCenter, northSouth ? 6f : 5f, northSouth ? 5f : 6f);
            mapData.Features.Add(shed);
            map = mapData.Build(data.Terrains);
            var farm = map.Landmarks.First(l => l.Label == farmName);
            var houseMark = map.Landmarks.First(l => l.Label == house.Label);

            var mission = NewMission(StartStates.Raid, data, mapData, map, seed);
            var sim = mission.Sim;
            mission.Objective = farm;

            // The enemy's goal picks who's where. It sets up first, so the squad can start out of its sight.
            float goalRoll = Next(rng);
            goal ??= Goals[Math.Min(Goals.Count - 1, (int)(goalRoll * Goals.Count))];
            var plan = new EnemyPlan(goal, farm, Goals, GoalWords);
            var made = new List<Unit>();
            var numbered = new Dictionary<string, int>();
            void AddGuard(string def, string name, Vec2 at, Posture posture, Vec2 facing) => Fighter(data, mission.Enemy, made, numbered, def, name, at, posture, facing);
            // Spots inside the walls: along the front wall looking out, along the back, by the shed, by the house.
            var spots = new List<(Vec2 at, Vec2 facing)>
            {
                (At(hd - 2.5f, -hw * 0.55f), At(hd + 100f, -hw * 0.55f)),
                (At(hd - 2.5f, hw * 0.55f), At(hd + 100f, hw * 0.55f)),
                (At(-hd + 1.8f, gap), At(-hd - 100f, gap)),
                (At(hd - 2.5f, -shedSide * 6f), At(hd + 100f, 0f)),
                (At(0f, -shedSide * (hw - 4f)), At(0f, -shedSide * (hw + 100f))),
                (At(-hd + 14.5f, houseAt), At(hd + 100f, houseAt)),
            };
            var codeName = Pick(names.Ridges, rng);
            Unit hvt;
            Landmark hideout = null;
            if (goal == Gone)
            {
                // He's in a house somewhere else, nearest 450 m from the farm, with a guard or two; one or two left at the farm.
                hideout = map.Landmarks.Where(l => l.Kind == "building" && !farm.Contains(l.Center) && Vec2.Distance(l.Center, c) >= 300f)
                    .OrderBy(l => Math.Abs(Vec2.Distance(l.Center, c) - 450f)).FirstOrDefault();
                Vec2 hide = hideout?.Center ?? RingSpot(map, rng, c, 350f, 500f, null, new List<Vec2>(), (p, t) => t.Concealment) ?? At(-300f, 0f);
                hvt = new Unit("hvt", codeName, data.FindUnit("en-hvt"), mission.Enemy, hide) { Posture = Posture.Kneeling };
                int withHim = Between(rng, 1, 2), atFarm = Between(rng, 1, 2);
                for (int i = 0; i < withHim; i++) AddGuard("en-rf", "Rifleman", hide + new Vec2(2f * (i + 1), 1.5f), Posture.Kneeling, c);
                for (int i = 0; i < atFarm; i++) AddGuard("en-rf", "Rifleman", spots[i].at, Posture.Standing, spots[i].facing);
            }
            else
            {
                hvt = new Unit("hvt", codeName, data.FindUnit("en-hvt"), mission.Enemy, houseCenter + new Vec2(1.5f, -1f)) { Posture = Posture.Kneeling };
                int count = goal == Guard ? Between(rng, 4, 6) : Between(rng, 3, 5);
                for (int i = 0; i < count; i++)
                {
                    bool gun = i == 0 && goal == Guard && Next(rng) < 0.5f;
                    var (at, facing) = spots[i % spots.Count];
                    // Lookouts stand to see over the wall; the rest wait kneeling.
                    var posture = gun ? Posture.Prone : i < 3 ? Posture.Standing : Posture.Kneeling;
                    AddGuard(gun ? "en-pkm" : "en-rf", gun ? "PKM gunner" : "Rifleman", gun ? At(hd - 3f, 0f) : at, posture, facing);
                }
            }
            hvt.Callsigns.AddRange(new[] { "the wanted man", "wanted man", "HVT", "the target" });
            hvt.Facing = Bearing(hvt.Position, At(hd + 100f, 0f));
            var enemy = new List<Unit> { hvt };
            enemy.AddRange(made);
            foreach (var u in enemy) u.Force.Plan = plan;

            var formation = StartOutOfSight(sim, c, p => Dot(p - c, front) >= 100f && !InSettlement(map, p), enemy, StartDistance, StartNearest, At(StartDistance, 0f));
            AddSquad(sim, mission.Marines, data, names, rng, formation);
            foreach (var u in enemy) sim.AddUnit(u);

            // His family, in the farmhouse. When he's gone, they know where.
            int familySize = Between(rng, 2, 4);
            var family = new List<Unit>();
            for (int i = 0; i < familySize; i++)
            {
                var u = new Unit("civilian-" + (i + 1), "Civilian " + (i + 1), data.FindUnit("civ"), mission.Civilians, houseCenter + new Vec2(-3f + 2f * i, 1.5f));
                if (hideout != null) u.Knows = codeName + " left the farm two days ago and is staying in " + hideout.Label + " (" + map.SquareOf(hideout.Center) + ").";
                else if (goal == Gone) u.Knows = codeName + " left the farm two days ago, out toward " + map.SquareOf(hvt.Position) + ".";
                sim.AddUnit(u);
                family.Add(u);
            }
            var village = Settlement(map, c);
            if (village != null) AddCivilians(sim, mission.Civilians, data, rng, BuildingsIn(map, village).Where(b => hideout == null || b != hideout), Between(rng, 2, 3), familySize + 1);

            // The pickup point: back where the squad starts, on a road or track if there's one close.
            Vec2 lead = formation[0].pos;
            Vec2 pickupAt = RingSpot(map, rng, lead, 30f, 120f, p => Dot(p - c, front) >= Dot(lead - c, front) + 20f && !map.TerrainAt(p).SeeOut, new List<Vec2>(),
                (p, t) => t.Id == "road" || t.Id == "trail" ? 1f : t.MoveFactor * 0.5f) ?? lead;
            var pickup = AddPoint(mapData, map, "the pickup point", pickupAt, "where the vehicles wait", "pickup point", "pickup", "PZ");

            // How it ends: he's brought in, he's identified dead, or he gets away.
            bool identified = false;
            sim.OnSecond += () =>
            {
                if (identified || hvt.Condition != Condition.Dead || hvt.Gone) return;
                var by = mission.Squad.FirstOrDefault(u => u.CanAct && Vec2.Distance(u.Position, hvt.Position) <= IdentifyWithin);
                if (by == null) return;
                identified = true;
                sim.Radio(by, (string.IsNullOrEmpty(by.Team) ? by.Name : by.Team) + ": It's " + codeName + ". He's dead.", "info", hvt.Position);
            };
            // Gone: he lies low where he is, whatever happens at the farm.
            if (goal == Gone) plan.Think = (u, dt) => u == hvt;
            if (goal == Slip) plan.EverySecond = SlipOut(mission, plan, hvt, c, EdgeToward(map, c, front * -1f));
            mission.Judge = m =>
            {
                if (hvt.Gone && !hvt.Detained) return ("failure", codeName + " got away.");
                if (identified) return ("success", codeName + " is dead, and the squad identified his body.");
                if (hvt.Detained && Vec2.Distance(hvt.Position, pickup.Center) <= OnPickup) return ("success", codeName + " is detained and at the pickup point.");
                return null;
            };
            mission.Status = m => hvt.Detained ? "onit" : "notyet";
            mission.OnItWord = "Detained";
            mission.DoneWord = "Brought in";

            mission.Title = "Operation " + Pick(names.Routes, rng) + " Latch";
            string farmGrid = map.SquareOf(c).ToString(), pickupGrid = map.SquareOf(pickup.Center).ToString();
            mission.Brief.Add("Task: detain the man code-named " + codeName + " at " + farmName + " (" + farmGrid + ") and bring him to the pickup point (" + pickupGrid + ").");
            mission.Brief.Add("Intent: " + codeName + " alive to question, and his family unhurt.");
            mission.Brief.Add("Enemy: armed men are with him; strength unknown. " + Hints[goal]);
            mission.Roe = StandingRoe;
            mission.Brief.Add("Civilians: his family lives at the farm" + (village != null ? ", and there are villagers in " + village.Label : "") + ". " + RoeLine(mission.Roe));
            int back = (int)Math.Round(Vec2.Distance(lead, c) / 50f) * 50;
            mission.Brief.Add(SquadLine("at " + map.SquareOf(lead) + ", about " + back + " m from the farm"));
            mission.MainObjective = "Detain " + codeName + " and bring him to the pickup point (" + pickupGrid + ").";
            mission.WinText = codeName + " detained and brought within " + OnPickup + " m of the pickup point. If he's killed, a Marine has to reach his body to identify him.";
            mission.LoseText = LoseLine(mission, codeName + " gets off the map.");
            mission.OutOfTimeText = "Out of time. " + codeName + " is still out there.";

            mission.Secondary.Add(new SecondaryObjective("alive", codeName + " taken alive", m => hvt.Condition == Condition.Dead));
            mission.Secondary.Add(new SecondaryObjective("family", "His family unhurt", m => m.AnyoneHurt(family)));
            var road = map.Landmarks.Where(l => l.Kind == "line" && l.Label.StartsWith("Route ", StringComparison.Ordinal)).OrderBy(l => DistanceTo(c, l.Points)).FirstOrDefault();
            mission.Director = new Director(mission, data, new Dice(seed, 13), new DirectorSetup
            {
                Focus = c,
                FocusWord = "the farm",
                FarSide = p => Dot(p - c, front) < -60f,
                FarWord = "the far side of the farm",
                NearSide = p => Dot(p - c, front) > 60f,
                NearWord = "the squad's side of the farm",
                Road = road?.Points,
                Slow = () => !hvt.Detained && hvt.Condition != Condition.Dead,
            });
            mission.Secondary.Add(EveryMarineHome());
            return mission;
        }

        static readonly Dictionary<string, string> Hints = new Dictionary<string, string>
        {
            { Guard, "A neighbor says armed men have been at the farm all week." },
            { Slip, "He's got away from a raid once before." },
            { Gone, "This intel is two days old." },
        };

        /// <summary>
        /// Slip: once a Marine comes close, or the guards see one or get shot at, he and the two guards nearest him walk
        /// out through the gap in the back wall and make for the far edge of the map. Anyone who reaches it is gone.
        /// </summary>
        static Action<List<Unit>> SlipOut(Mission mission, EnemyPlan plan, Unit hvt, Vec2 farm, Vec2 exit)
        {
            var sim = mission.Sim;
            List<Unit> going = null;
            return side =>
            {
                if (going == null)
                {
                    if (!hvt.CanAct) return;
                    var force = hvt.Force;
                    bool seen = sim.Fog.For(force).Contacts.Any(k => k.Visible && sim.AreHostile(force, k.Unit.Force) && !k.Unit.Def.Flying);
                    bool close = mission.Squad.Any(u => u.CanAct && Vec2.Distance(u.Position, farm) <= SlipWhenWithin);
                    bool shot = side.Any(e => e.Wounds > 0 || !e.CanAct || e.Suppression >= 0.3f);
                    if (!seen && !close && !shot) return;
                    going = new List<Unit> { hvt };
                    going.AddRange(side.Where(u => u != hvt && u.CanAct && u.Def.Id == "en-rf").OrderBy(u => Vec2.Distance(u.Position, hvt.Position)).Take(2));
                    foreach (var u in going) Go(sim, u, exit);
                    plan.FallingBack = true;
                }
                foreach (var u in going)
                {
                    if (!u.CanAct || u.Gone) continue;
                    if (AtEdge(sim.Map, u.Position, 8f)) sim.LeaveMap(u);
                    else if (u.Order == null && !u.LegHit) Go(sim, u, exit);
                }
            };
        }
    }
}
