using System;
using System.Collections.Generic;
using System.Linq;
using static FieldTable.Sim.MissionKit;

namespace FieldTable.Sim
{
    /// <summary>
    /// Clear the village (ledger round 16): higher marks 4 to 6 buildings to clear. Fighters hold some, families live in
    /// others. The enemy fights house to house, leaves a few behind with a rigged door, or slips out the back when the
    /// squad arrives.
    /// </summary>
    public static class ClearVillage
    {
        public const string HouseToHouse = "house", RiggedDoor = "rig", SlipOut = "slip";
        public static readonly IReadOnlyList<string> Goals = new[] { HouseToHouse, RiggedDoor, SlipOut };

        static readonly Dictionary<string, (string said, string truth)> GoalWords = Words(
            (HouseToHouse, "they'll fight for every house", "fight house to house"),
            (RiggedDoor, "most of them left, and a door is rigged to blow", "leave a few behind and rig a door"),
            (SlipOut, "they'll slip out the back when you come", "slip out the back"));

        /// <summary>They slip out once a Marine is this close to the village's middle, or they see one or get shot at.</summary>
        public const float SlipWhenWithin = 150f;

        /// <summary>A fighter falls back to another house once a Marine is in his or this close to it, and he's on his own.</summary>
        public const float PressedWithin = 10f;

        /// <summary>The squad starts about this far from the village's middle, and never nearer than the least.</summary>
        const float StartDistance = 300f, StartNearest = 180f;

        /// <summary>Makes the mission. A die picks the enemy's goal unless one is asked for (house, rig, or slip).</summary>
        public static Mission Create(GameData data, NamePools names, ulong seed, string goal = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (names == null) throw new ArgumentNullException(nameof(names));
            Landmark Village(MapGrid m) => m.Landmarks.Where(l => l.Kind == "area" && l.Note == "village")
                .Select(l => (l, n: BuildingsIn(m, l).Count())).Where(x => x.n >= 6).OrderByDescending(x => x.n).Select(x => x.l).FirstOrDefault();
            var (mapData, map) = MakeMap(data, names, seed, "Clear the village " + seed, r => r.Require.Add(MapFeatures.Villages), (_, m) => Village(m) != null);
            var village = Village(map) ?? Settlement(map, new Vec2(map.WidthMeters / 2f, map.HeightMeters / 2f));
            Vec2 v = village.Center;
            var rng = new Dice(seed, 11);

            var mission = NewMission(StartStates.Clear, data, mapData, map, seed);
            var sim = mission.Sim;
            mission.Objective = village;

            // Higher's marks: 4 to 6 of the village's buildings, numbered in order.
            var houses = Shuffle(BuildingsIn(map, village).ToList(), rng);
            int marks = Math.Min(houses.Count, Between(rng, 4, 6));
            var marked = houses.Take(marks).OrderBy(b => int.TryParse(Number(b), out int n) ? n : int.MaxValue).ToList();
            mission.Marked.AddRange(marked);

            float goalRoll = Next(rng);
            goal ??= Goals[Math.Min(Goals.Count - 1, (int)(goalRoll * Goals.Count))];
            var plan = new EnemyPlan(goal, village, Goals, GoalWords);
            int heldCount = goal == HouseToHouse ? 3 : goal == RiggedDoor ? 1 : Between(rng, 2, 3);
            heldCount = Math.Min(heldCount, marked.Count - 1);
            var order = Shuffle(marked, rng);
            var held = order.Take(heldCount).ToList();
            var rest = order.Skip(heldCount).ToList();
            var homes = rest.Take(Math.Max(1, Math.Min(rest.Count, Between(rng, 1, 2)))).ToList();

            var enemy = new List<Unit>();
            var numbered = new Dictionary<string, int>();
            foreach (var b in held)
            {
                int n = Between(rng, 2, 3);
                for (int i = 0; i < n; i++)
                {
                    bool gun = goal == HouseToHouse && b == held[0] && i == 0;
                    Fighter(data, mission.Enemy, enemy, numbered, gun ? "en-pkm" : "en-rf", gun ? "PKM gunner" : "Rifleman", Indoors(b, i), Posture.Kneeling, v + (b.Center - v) * 3f);
                }
            }
            Organize(data, enemy);
            foreach (var u in enemy) u.Force.Plan = plan;

            var formation = StartOutOfSight(sim, v, p => !Near(village, p, 100f) && !InSettlement(map, p), enemy, StartDistance, StartNearest, OutAlong(map, v, new Vec2(0f, 1f), StartDistance));
            AddSquad(sim, mission.Marines, data, names, rng, formation);
            foreach (var u in enemy) sim.AddUnit(u);
            Vec2 lead = formation[0].pos;
            Vec2 front = Unit(lead - v);

            // Families in some of the marked houses, and more villagers in the houses nobody marked.
            var family = new List<Unit>();
            int civ = 0;
            foreach (var b in homes)
            {
                int n = Between(rng, 1, 3);
                for (int i = 0; i < n; i++)
                {
                    civ++;
                    var u = new Unit("civilian-" + civ, "Civilian " + civ, data.FindUnit("civ"), mission.Civilians, Indoors(b, i));
                    sim.AddUnit(u);
                    family.Add(u);
                }
            }
            AddCivilians(sim, mission.Civilians, data, rng, houses.Skip(marks), Between(rng, 1, 3), civ + 1);

            // A rigged door: an IED at the door of an empty marked house, on the side the squad comes from.
            Ied rigged = null;
            if (goal == RiggedDoor)
            {
                var door = rest.Except(homes).FirstOrDefault() ?? rest.LastOrDefault();
                if (door != null) rigged = sim.PlaceIed(DoorOf(door, front));
            }

            switch (goal)
            {
                case HouseToHouse: plan.EverySecond = side => FallBack(mission, held, side); break;
                case SlipOut: plan.EverySecond = Slip(mission, v); break;
            }

            mission.Judge = m =>
            {
                var cleared = new HashSet<Landmark>(m.Sim.Log.Events.OfType<BuildingCleared>().Select(e => e.Building));
                if (!marked.All(cleared.Contains)) return null;
                if (m.Sim.Units.Any(u => u.CanAct && m.Sim.AreHostile(m.Marines, u.Force) && marked.Any(b => b.Contains(u.Position)))) return null;
                return ("success", "Every marked building is clear.");
            };
            mission.Status = m => m.Sim.Log.Events.OfType<BuildingCleared>().Any(e => marked.Contains(e.Building)) ? "onit" : "notyet";
            mission.OnItWord = "Clearing";
            mission.DoneWord = "Cleared";

            mission.Title = "Operation " + Pick(names.Routes, rng) + " Broom";
            string list = JoinAnd(marked.Select(Number).ToList());
            string grid = map.SquareOf(v).ToString();
            mission.Brief.Add("Task: clear the " + marked.Count + " buildings higher has marked in " + village.Label + " (" + grid + "): buildings " + list + ".");
            mission.Brief.Add("Intent: the village clear of enemy soldiers, its families unhurt, and the houses left standing.");
            mission.Brief.Add("Enemy: soldiers are in some of the marked buildings; strength unknown. " + Hints[goal]);
            mission.Roe = StandingRoe;
            mission.Brief.Add("Civilians: families live in some of the marked buildings. " + RoeLine(mission.Roe));
            int back = (int)Math.Round(Vec2.Distance(lead, v) / 50f) * 50;
            mission.Brief.Add(SquadLine("at " + map.SquareOf(lead) + ", about " + back + " m from " + village.Label));
            mission.MainObjective = "Clear the marked buildings in " + village.Label + ": " + list + ".";
            mission.WinText = "Every marked building cleared (a Clear order finished inside it), with nobody hostile still in any of them.";
            mission.LoseText = LoseLine(mission);
            mission.OutOfTimeText = "Out of time. Not every marked building was cleared.";

            mission.Secondary.Add(new SecondaryObjective("civilians", "No civilians hurt", m => m.AnyoneHurt(m.Civilians)));
            mission.Secondary.Add(new SecondaryObjective("standing", "No marked building blown open", m =>
                m.Sim.Log.Events.OfType<BreachMade>().Any(e => marked.Any(b => Near(b, e.At, 2f))) ||
                (rigged != null && m.Sim.Log.Events.OfType<IedWentOff>().Any(e => e.Ied == rigged))));
            var road = map.Landmarks.Where(l => l.Kind == "line" && l.Label.StartsWith("Route ", StringComparison.Ordinal)).OrderBy(l => DistanceTo(v, l.Points)).FirstOrDefault();
            mission.Director = new Director(mission, data, new Dice(seed, 13), new DirectorSetup
            {
                Focus = v,
                FocusWord = village.Label,
                FarSide = p => Dot(p - v, front) < -60f,
                FarWord = "the far side of " + village.Label,
                NearSide = p => Dot(p - v, front) > 60f && !Near(village, p, 0f),
                NearWord = "the squad's side of " + village.Label,
                Road = road?.Points,
                Slow = () => !mission.Sim.Log.Events.OfType<BuildingCleared>().Any(e => marked.Contains(e.Building)),
            });
            mission.Secondary.Add(EveryMarineHome());
            return mission;
        }

        static readonly Dictionary<string, string> Hints = new Dictionary<string, string>
        {
            { HouseToHouse, "Soldiers have been seen moving between the houses." },
            { RiggedDoor, "Most of the soldiers were seen leaving yesterday." },
            { SlipOut, "They have always pulled out before when our troops came." },
        };

        static string Number(Landmark b) => b.Label.StartsWith("building ", StringComparison.Ordinal) ? b.Label.Substring(9) : b.Label;

        static string JoinAnd(List<string> items) =>
            items.Count == 1 ? items[0] : string.Join(", ", items.Take(items.Count - 1)) + (items.Count > 2 ? "," : "") + " and " + items[items.Count - 1];

        /// <summary>A spot inside a building for its i-th person, spread a couple of meters apart.</summary>
        static Vec2 Indoors(Landmark b, int i)
        {
            float w = b.Max.X - b.Min.X, d = b.Max.Y - b.Min.Y;
            var offsets = new[] { new Vec2(-0.25f, -0.2f), new Vec2(0.25f, 0.2f), new Vec2(0.25f, -0.2f), new Vec2(-0.25f, 0.2f) };
            var o = offsets[i % offsets.Length];
            return b.Center + new Vec2(o.X * w, o.Y * d);
        }

        /// <summary>The middle of a building's wall on the side facing a direction, a meter outside it.</summary>
        static Vec2 DoorOf(Landmark b, Vec2 facing)
        {
            Vec2 half = (b.Max - b.Min) * 0.5f;
            if (Math.Abs(facing.X) * half.Y > Math.Abs(facing.Y) * half.X)
                return b.Center + new Vec2(Math.Sign(facing.X) * (half.X + 1f), 0f);
            return b.Center + new Vec2(0f, Math.Sign(facing.Y) * (half.Y + 1f));
        }

        static bool Near(Landmark area, Vec2 p, float margin) =>
            p.X >= area.Min.X - margin && p.X <= area.Max.X + margin && p.Y >= area.Min.Y - margin && p.Y <= area.Max.Y + margin;

        /// <summary>
        /// House to house: a fighter left on his own in a house a Marine is in, or right outside, runs for another house
        /// his side still holds, the one farthest from the Marines.
        /// </summary>
        static void FallBack(Mission mission, List<Landmark> held, List<Unit> side)
        {
            var marines = mission.Squad.Where(u => u.CanAct).ToList();
            foreach (var u in side.Where(u => u.CanAct && u.Order == null))
            {
                var b = held.FirstOrDefault(h => h.Contains(u.Position));
                if (b == null || !marines.Any(m => Near(b, m.Position, PressedWithin))) continue;
                if (side.Count(x => x != u && x.CanAct && b.Contains(x.Position)) > 1) continue;
                var next = held.Where(h => h != b && side.Any(x => x.CanAct && h.Contains(x.Position)))
                    .OrderByDescending(h => marines.Min(m => Vec2.Distance(m.Position, h.Center))).FirstOrDefault();
                if (next != null) Go(mission.Sim, u, Indoors(next, 3), Verbs.Move, MoveSpeed.Run);
            }
        }

        /// <summary>
        /// Slip out: once a Marine comes close to the village, or they see one or get shot at, they all make for the edge
        /// of the map on the far side from the nearest Marine. Anyone who reaches it is gone.
        /// </summary>
        static Action<List<Unit>> Slip(Mission mission, Vec2 v)
        {
            var sim = mission.Sim;
            Vec2? exit = null;
            return side =>
            {
                if (exit == null)
                {
                    var force = side[0].Force;
                    var marines = mission.Squad.Where(u => u.CanAct).ToList();
                    bool seen = sim.Fog.For(force).Contacts.Any(k => k.Visible && sim.AreHostile(force, k.Unit.Force) && !k.Unit.Def.Flying);
                    bool close = marines.Any(u => Vec2.Distance(u.Position, v) <= SlipWhenWithin);
                    bool shot = side.Any(e => e.Wounds > 0 || !e.CanAct || e.Suppression >= 0.3f);
                    if ((!seen && !close && !shot) || marines.Count == 0) return;
                    var nearest = marines.OrderBy(u => Vec2.Distance(u.Position, v)).First();
                    exit = EdgeToward(sim.Map, v, v - nearest.Position);
                    if (side[0].Force.Plan != null) side[0].Force.Plan.FallingBack = true;
                }
                foreach (var u in side.Where(u => u.CanAct && !u.Gone))
                {
                    if (AtEdge(sim.Map, u.Position, 8f)) sim.LeaveMap(u);
                    else if (u.Order == null && !u.LegHit) Go(sim, u, exit.Value, Verbs.Move, MoveSpeed.Run);
                }
            };
        }
    }
}
