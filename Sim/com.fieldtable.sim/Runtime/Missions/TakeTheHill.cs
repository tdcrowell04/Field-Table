using System;
using System.Collections.Generic;
using System.Linq;
using static FieldTable.Sim.MissionKit;

namespace FieldTable.Sim
{
    /// <summary>
    /// Take the hill (ledger round 17): an enemy squad is dug in on a hill that looks down the road the battalion needs.
    /// Take it and hold it. The enemy holds to the last, falls back to a second position on the far slope, or lets you
    /// take it and counterattacks from the woods.
    /// </summary>
    public static class TakeTheHill
    {
        public const string HoldOn = "hold", FallBack = "fallback", Counter = "counter";
        public static readonly IReadOnlyList<string> Goals = new[] { HoldOn, FallBack, Counter };

        static readonly Dictionary<string, (string said, string truth)> GoalWords = Words(
            (HoldOn, "they'll hold the hill to the last man", "hold the hill to the last"),
            (FallBack, "they'll fall back over the top to a second position", "fall back to a second position"),
            (Counter, "they'll let you have the top and hit you from the woods", "let you take it and counterattack"));

        /// <summary>The fall back goes once a Marine is this close to the top, or two of them are down.</summary>
        public const float FallBackWithin = 150f;

        /// <summary>Holding to the last: they only pull back once this much of them is down.</summary>
        const float LastMan = 0.9f;

        /// <summary>The squad starts about this far from the top, and never nearer than the least.</summary>
        const float StartDistance = 400f, StartNearest = 250f;

        /// <summary>Makes the mission. A die picks the enemy's goal unless one is asked for (hold, fallback, or counter).</summary>
        public static Mission Create(GameData data, NamePools names, ulong seed, string goal = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (names == null) throw new ArgumentNullException(nameof(names));
            // A hill high enough to matter but not a mountain, well in from the edges, with a road below it.
            (Landmark hill, Landmark road) Pick2(MapGrid m)
            {
                var roads = m.Landmarks.Where(l => l.Kind == "line" && l.Label.StartsWith("Route ", StringComparison.Ordinal)).ToList();
                foreach (var h in m.Landmarks.Where(l => l.Kind == "hill").OrderBy(l => l.Label))
                {
                    float height = m.GroundAt(h.Center);
                    if (height < 18f || height > 90f || AtEdge(m, h.Center, 230f) || InSettlement(m, h.Center, 40f) || m.TerrainAt(h.Center).Deep) continue;
                    var r = roads.Where(x => DistanceTo(h.Center, x.Points) <= 350f).OrderBy(x => DistanceTo(h.Center, x.Points)).FirstOrDefault();
                    if (r != null) return (h, r);
                }
                return (null, null);
            }
            var (mapData, map) = MakeMap(data, names, seed, "Take the hill " + seed,
                r => { r.Require.Add(MapFeatures.Hills); r.Require.Add(MapFeatures.Roads); }, (_, m) => Pick2(m).hill != null);
            var (hill, road) = Pick2(map);
            hill ??= map.Landmarks.Where(l => l.Kind == "hill").OrderByDescending(l => map.GroundAt(l.Center)).First();
            road ??= map.Landmarks.Where(l => l.Kind == "line" && l.Label.StartsWith("Route ", StringComparison.Ordinal)).OrderBy(l => DistanceTo(hill.Center, l.Points)).FirstOrDefault();
            Vec2 top = hill.Center;
            var rng = new Dice(seed, 11);

            var mission = NewMission(StartStates.Hill, data, mapData, map, seed);
            var sim = mission.Sim;
            mission.Objective = hill;

            // The squad comes from the road's side of the hill.
            Vec2 toRoad = road != null ? Unit(NearestOn(road.Points, top) - top) : new Vec2(0f, 1f);
            Vec2 roadNear = road != null ? NearestOn(road.Points, top) : top + toRoad * 200f;

            float goalRoll = Next(rng);
            goal ??= Goals[Math.Min(Goals.Count - 1, (int)(goalRoll * Goals.Count))];
            var plan = new EnemyPlan(goal, hill, Goals, GoalWords);
            if (goal == HoldOn) plan.Breaks = LastMan;
            var made = new List<Unit>();
            var numbered = new Dictionary<string, int>();
            var taken = new List<Vec2>();
            Vec2 approach = top + toRoad * 250f;
            // Dug in on the top: cover first, then a view down toward the road.
            void DigIn(string def, string name, float near, float far, Posture posture)
            {
                var at = RingSpot(map, rng, top, near, far, p => Dot(p - top, toRoad) > -15f && !map.TerrainAt(p).SeeOut, taken,
                    (p, t) => t.Cover * 1.5f + t.Concealment * 0.5f + (Sight.Trace(map, p, Postures.Height(posture), approach, 1.7f).Clear ? 1.5f : 0f)) ?? Standable(map, top, taken);
                Fighter(data, mission.Enemy, made, numbered, def, name, at, posture, approach);
            }
            List<Unit> woods = new List<Unit>();
            Vec2 second = default;
            switch (goal)
            {
                case HoldOn:
                {
                    DigIn("en-pkm", "PKM gunner", 10f, 35f, Posture.Prone);
                    DigIn("en-rpg", "RPG gunner", 10f, 35f, Posture.Kneeling);
                    int n = Between(rng, 6, 7);
                    for (int i = 0; i < n; i++) DigIn("en-rf", "Rifleman", 5f, 45f, Posture.Kneeling);
                    break;
                }
                case FallBack:
                {
                    DigIn("en-pkm", "PKM gunner", 10f, 35f, Posture.Prone);
                    int n = Between(rng, 5, 7);
                    for (int i = 0; i < n; i++) DigIn("en-rf", "Rifleman", 5f, 45f, Posture.Kneeling);
                    // The second position: on the far slope, out of sight of the road.
                    second = RingSpot(map, rng, top - toRoad * 130f, 0f, 40f, p => !map.TerrainAt(p).SeeOut, new List<Vec2>(),
                        (p, t) => t.Cover + t.Concealment + (Sight.Trace(map, roadNear, 1.7f, p, 1.1f).Clear ? 0f : 2f)) ?? Inside(map, top - toRoad * 130f);
                    break;
                }
                default:
                {
                    // A few on top, and the rest waiting in cover off to the side and behind, out of sight of the road.
                    for (int i = 0; i < 3; i++) DigIn("en-rf", "Rifleman", 5f, 35f, Posture.Kneeling);
                    Vec2 side = Turn(toRoad * -1f, (Next(rng) < 0.5f ? 1 : -1) * Math.PI / 3);
                    Vec2 hide = RingSpot(map, rng, top + side * 230f, 0f, 60f, p => !map.TerrainAt(p).SeeOut && Vec2.Distance(p, top) >= 180f, new List<Vec2>(),
                        (p, t) => t.Concealment * 2f + t.Cover + (Sight.Trace(map, roadNear, 1.7f, p, 1.1f).Clear ? 0f : 2f)) ?? Inside(map, top + side * 230f);
                    int before = made.Count;
                    foreach (var def in new[] { "en-rf", "en-pkm", "en-rpg", "en-rf", "en-rf" }.Concat(Next(rng) < 0.5f ? new[] { "en-rf" } : new string[0]))
                    {
                        var at = Standable(map, hide + new Vec2(4f * (made.Count - before) - 10f, 3f * ((made.Count - before) % 2)), taken);
                        taken.Add(at);
                        Fighter(data, mission.Enemy, made, numbered, def, def == "en-pkm" ? "PKM gunner" : def == "en-rpg" ? "RPG gunner" : "Rifleman", at, Posture.Prone, top);
                    }
                    woods = made.Skip(before).ToList();
                    break;
                }
            }
            Organize(data, made);
            // The squad leader's radio is worth having: searching him, or where he fell, takes it.
            foreach (var u in made.Where(u => u.Def.Radio)) u.Cargo = "their radio";
            foreach (var u in made) u.Force.Plan = plan;

            var formation = StartOutOfSight(sim, top, p => Dot(p - top, toRoad) > 60f && !InSettlement(map, p), made, StartDistance, StartNearest, Inside(map, top + toRoad * StartDistance));
            AddSquad(sim, mission.Marines, data, names, rng, formation);
            foreach (var u in made) sim.AddUnit(u);
            var village = Settlement(map, top);
            if (village != null) AddCivilians(sim, mission.Civilians, data, rng, BuildingsIn(map, village), Between(rng, 2, 4));

            // How the enemy fights for it.
            var onTop = made.Except(woods).ToList();
            if (goal == FallBack)
            {
                bool gone = false;
                plan.EverySecond = side =>
                {
                    if (gone) return;
                    bool close = mission.Squad.Any(u => u.CanAct && Vec2.Distance(u.Position, top) <= FallBackWithin);
                    if (!close && side.Count(u => !u.CanAct) < 2) return;
                    gone = true;
                    plan.FallingBack = true;
                    sim.Shout(side.FirstOrDefault(u => u.CanAct), "Back! To the second line!", always: true);
                    var going = side.Where(u => u.CanAct).ToList();
                    for (int i = 0; i < going.Count; i++)
                        Go(sim, going[i], Standable(sim.Map, second + Turn(new Vec2(0f, -5f), i * Math.PI / 4), Array.Empty<Vec2>()), Verbs.Withdraw, MoveSpeed.Run);
                };
            }
            else if (goal == Counter)
            {
                bool hit = false;
                foreach (var u in woods) u.Team = "counterattack";
                plan.EverySecond = side =>
                {
                    var men = woods.Where(u => u.CanAct).ToList();
                    if (men.Count == 0) return;
                    if (!hit)
                    {
                        if (!mission.MarineOnObjective && onTop.Count(u => u.CanAct) > 1) return;
                        hit = true;
                        plan.Sprung = true;
                        // They know where the Marines on the top are: they watched them take it.
                        var knows = sim.Fog.For(mission.Enemy);
                        foreach (var x in mission.Squad.Where(x => x.CanAct && Vec2.Distance(x.Position, top) <= 60f))
                        {
                            var k = knows.Track(x);
                            k.Known = true; k.LastKnownPosition = x.Position; k.LastSeenStep = sim.Clock.Step;
                        }
                        var guns = men.Where(u => u.Def.Id == "en-pkm").ToList();
                        SuppressKnown(sim, guns, top, mission.Marines);
                        TeamOrder(sim, woods, "counterattack", top, Verbs.Bound, null, men.Except(guns).ToList());
                        sim.Shout(men[0], "Now! Take it back!", always: true);
                        return;
                    }
                    if (men.Any(u => u.Order != null && u.Order.Verb != Verbs.Suppress)) return;
                    var shortOf = men.Where(u => u.Def.Id != "en-pkm" && Vec2.Distance(u.Position, top) > Mission.OnObjective && u.Suppression < 0.5f).ToList();
                    if (shortOf.Count > 0) TeamOrder(sim, woods, "counterattack", top, Verbs.Bound, null, shortOf);
                };
            }

            mission.Judge = Mission.HoldTwoTurns(hill.Label + " is taken and held.");
            mission.DoneWord = "Taken";
            mission.Title = "Operation " + Pick(names.Routes, rng) + " Crest";
            string grid = map.SquareOf(top).ToString();
            var lead = formation[0].pos;
            mission.Brief.Add("Task: take " + hill.Label + " (" + grid + "), which looks down " + (road?.Label ?? "the road") + ", and hold it so the battalion can use the road.");
            mission.Brief.Add("Intent: the hill ours, and their radio taken so we can listen in.");
            mission.Brief.Add("Enemy: a squad dug in on the hill; strength unknown. " + Hints[goal]);
            mission.Roe = StandingRoe;
            mission.Brief.Add("Civilians: " + (village != null ? "in " + village.Label + ". " : "none known nearby. ") + RoeLine(mission.Roe));
            int back = (int)Math.Round(Vec2.Distance(lead, top) / 50f) * 50;
            mission.Brief.Add(SquadLine("at " + map.SquareOf(lead) + ", about " + back + " m from the top"));
            mission.MainObjective = "Take " + hill.Label + " (" + grid + ") and hold it.";
            mission.WinText = "A Marine within " + Mission.OnObjective + " m of the top of " + hill.Label + " and no enemy still fighting within " + Mission.HoldRadius + " m, at the end of two turns in a row.";
            mission.LoseText = LoseLine(mission);
            mission.OutOfTimeText = "Out of time. The battalion couldn't wait any longer for the road.";

            mission.Secondary.Add(new SecondaryObjective("radio", "Their radio taken intact (search their position)", m => !m.Sim.Log.Events.OfType<CargoTaken>().Any(e => e.Cargo == "their radio")) { AtEnd = true });
            mission.Director = new Director(mission, data, new Dice(seed, 13), new DirectorSetup
            {
                Focus = top,
                FocusWord = "the hill",
                FarSide = p => Dot(p - top, toRoad) < -60f,
                FarWord = "the far side of the hill",
                NearSide = p => Dot(p - top, toRoad) > 60f,
                NearWord = "the road side of the hill",
                Road = road?.Points,
            });
            mission.Secondary.Add(EveryMarineHome());
            return mission;
        }

        static readonly Dictionary<string, string> Hints = new Dictionary<string, string>
        {
            { HoldOn, "They've been digging in up there for two days." },
            { FallBack, "Patrols saw them digging on the far slope too." },
            { Counter, "Only a few have been seen up top." },
        };

        /// <summary>The nearest point on a line to a spot.</summary>
        static Vec2 NearestOn(IReadOnlyList<Vec2> line, Vec2 p)
        {
            Vec2 best = line[0];
            float bestD = float.MaxValue;
            for (int i = 0; i + 1 < line.Count; i++)
            {
                Vec2 a = line[i], ab = line[i + 1] - a;
                float len2 = ab.X * ab.X + ab.Y * ab.Y;
                float t = len2 > 0f ? Math.Max(0f, Math.Min(1f, ((p.X - a.X) * ab.X + (p.Y - a.Y) * ab.Y) / len2)) : 0f;
                var q = a + ab * t;
                float d = Vec2.Distance(p, q);
                if (d < bestD) { bestD = d; best = q; }
            }
            return best;
        }
    }
}
