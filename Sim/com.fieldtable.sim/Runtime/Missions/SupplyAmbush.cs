using System;
using System.Collections.Generic;
using System.Linq;
using static FieldTable.Sim.MissionKit;

namespace FieldTable.Sim
{
    /// <summary>
    /// Ambush a supply party (ledger round 16): fighters carrying crates of weapons on foot along a trail across the map.
    /// Intel isn't sure: they could come from either end, come late, and shepherds use the trail too. The enemy walks
    /// straight in, sends a scout ahead, or comes with more men than intel said.
    /// </summary>
    public static class SupplyAmbush
    {
        public const string Straight = "straight", Scout = "scout", MoreMen = "more";
        public static readonly IReadOnlyList<string> Goals = new[] { Straight, Scout, MoreMen };

        static readonly Dictionary<string, (string said, string truth)> GoalWords = Words(
            (Straight, "they walk straight down the trail", "walk straight down the trail"),
            (Scout, "a scout goes ahead of them", "send a scout ahead"),
            (MoreMen, "there are more of them than you think", "come with more men than intel said"));

        /// <summary>Intel's best guess at the turn they come. They really come between the second two, picked by a die.</summary>
        public const int Expected = 6, EarliestTurn = 5, LatestTurn = 11;

        /// <summary>A scout walks this far ahead of the party.</summary>
        public const float ScoutAhead = 150f;

        /// <summary>The squad starts about this far from the middle of the trail.</summary>
        const float StartDistance = 220f, StartNearest = 140f;

        static readonly string[] Crates = { "a crate of rifles", "a crate of RPG rounds", "a box of grenades" };

        /// <summary>Makes the mission. A die picks the enemy's goal unless one is asked for (straight, scout, or more).</summary>
        public static Mission Create(GameData data, NamePools names, ulong seed, string goal = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (names == null) throw new ArgumentNullException(nameof(names));
            var (mapData, map) = MakeMap(data, names, seed, "Supply ambush " + seed, r => { r.Exclude.Add(MapFeatures.Rivers); r.Exclude.Add(MapFeatures.Bridges); }, null);
            var rng = new Dice(seed, 11);

            // The trail: a footpath across the map from one edge to the other, going round anything it can't cross.
            bool northSouth = Next(rng) < 0.5f;
            float at0 = 0.3f + 0.4f * Next(rng), at1 = 0.3f + 0.4f * Next(rng);
            Vec2 a = northSouth ? new Vec2(map.WidthMeters * at0, 2f) : new Vec2(2f, map.HeightMeters * at0);
            Vec2 b = northSouth ? new Vec2(map.WidthMeters * at1, map.HeightMeters - 2f) : new Vec2(map.WidthMeters - 2f, map.HeightMeters * at1);
            var route = new Pathfinder(map).Find(a, b, false) ?? new List<Vec2> { a, b };
            // A route's waypoints leave out where it starts, so the trail starts from the edge itself.
            var trail = new List<Vec2> { a };
            foreach (var p in route)
                if (Vec2.Distance(p, trail[trail.Count - 1]) >= 20f) trail.Add(p);
            if (Vec2.Distance(trail[trail.Count - 1], route[route.Count - 1]) > 0.5f) trail.Add(route[route.Count - 1]);
            string trailName = Pick(names.Woods.Where(n => !map.Landmarks.Any(l => l.Label.StartsWith(n, StringComparison.Ordinal))).ToList(), rng) + " Trail";
            // It goes under water, yards, roads, and buildings, and over fields, hedges, and woods.
            var under = new HashSet<string> { "river", "creek", "lake", "yard", "road", "trail", "bridge", "cliff", "building" };
            int index = mapData.Features.FindIndex(f => f.Terrain != null && under.Contains(f.Terrain));
            var path0 = LineFeature(mapData, "trail", 2f, trail, trailName, "a footpath");
            path0.Aliases.AddRange(new[] { "the trail", "trail" });
            mapData.Features.Insert(index < 0 ? mapData.Features.Count : index, path0);
            map = mapData.Build(data.Terrains);
            var trailMark = map.Landmarks.First(l => l.Label == trailName);
            Vec2 middle = PointAlong(trail, 0.5f);

            var mission = NewMission(StartStates.SupplyAmbush, data, mapData, map, seed);
            var sim = mission.Sim;
            mission.Objective = trailMark;

            // Where they come from and when: intel's guess is one end; a die says whether it's right.
            bool fromA = Next(rng) < 0.5f;
            bool intelRight = Next(rng) < 0.6f;
            var path = fromA ? trail : Enumerable.Reverse(trail).ToList();
            Vec2 expectedFrom = intelRight ? path[0] : path[path.Count - 1];
            int arrive = Between(rng, EarliestTurn, LatestTurn);

            float goalRoll = Next(rng);
            goal ??= Goals[Math.Min(Goals.Count - 1, (int)(goalRoll * Goals.Count))];
            var plan = new EnemyPlan(goal, trailMark, Goals, GoalWords);
            var party = new List<Unit>();
            var numbered = new Dictionary<string, int>();
            var kinds = new List<string> { "en-porter", "en-porter" };
            if (goal == MoreMen) { kinds.Add("en-pkm"); kinds.Add("en-rpg"); kinds.AddRange(Enumerable.Repeat("en-rf", Between(rng, 4, 6))); }
            else kinds.AddRange(Enumerable.Repeat("en-rf", Between(rng, 2, 4)));
            // Riflemen lead and follow; the porters walk in the middle.
            var file = kinds.Where(k => k != "en-porter").Take(1).Concat(kinds.Where(k => k == "en-porter")).Concat(kinds.Where(k => k != "en-porter").Skip(1)).ToList();
            int crate = 0;
            foreach (var kind in file)
            {
                string name = kind == "en-porter" ? "Porter" : kind == "en-pkm" ? "PKM gunner" : kind == "en-rpg" ? "RPG gunner" : "Rifleman";
                var u = Fighter(data, mission.Enemy, party, numbered, kind, name, path[0], Posture.Standing, path[Math.Min(1, path.Count - 1)]);
                if (kind == "en-porter") u.Cargo = Crates[crate++ % Crates.Length];
                u.Gone = true;
            }
            Unit scout = null;
            if (goal == Scout)
            {
                scout = Fighter(data, mission.Enemy, party, numbered, "en-rf", "Scout", path[0], Posture.Standing, path[Math.Min(1, path.Count - 1)]);
                scout.Gone = true;
            }
            Organize(data, party);
            foreach (var u in party) { u.Force.Plan = plan; }

            var formation = StartOutOfSight(sim, middle, p => DistanceTo(p, trail) >= 100f && !InSettlement(map, p), new List<Unit>(), StartDistance, StartNearest, Inside(map, middle + Turn(Unit(trail[trail.Count - 1] - trail[0]), Math.PI / 2) * StartDistance));
            AddSquad(sim, mission.Marines, data, names, rng, formation);
            foreach (var u in party) sim.AddUnit(u);

            // Shepherds walk the trail first, half the time, from either end.
            var shepherds = new List<Unit>();
            int shepherdsCome = -1;
            List<Vec2> shepherdPath = null;
            if (Next(rng) < 0.5f)
            {
                shepherdsCome = Math.Max(2, arrive - Between(rng, 1, 3));
                shepherdPath = Next(rng) < 0.5f ? trail : Enumerable.Reverse(trail).ToList();
                for (int i = 0; i < 2; i++)
                {
                    var u = new Unit("shepherd-" + (i + 1), "Shepherd " + (i + 1), data.FindUnit("civ"), mission.Civilians, shepherdPath[0]) { Gone = true };
                    sim.AddUnit(u);
                    shepherds.Add(u);
                }
            }
            var village = Settlement(map, middle);
            if (village != null) AddCivilians(sim, mission.Civilians, data, rng, BuildingsIn(map, village), Between(rng, 2, 4), 1);

            // The party: on the trail at their turn, walking it in file; once ambushed the riflemen fight and the porters
            // run for whichever end is nearer. A scout who sees Marines turns them all back.
            var walk = new Dictionary<Unit, List<Vec2>>();
            bool came = false, contact = false, turnedBack = false;
            var dest = new Dictionary<Unit, Vec2>();
            void Walk(Unit u, List<Vec2> along, MoveSpeed speed)
            {
                int i = Nearest(along, u.Position);
                var rest = along.Skip(i + 1).ToList();
                if (rest.Count == 0) return;
                dest[u] = rest[rest.Count - 1];
                u.Order = new Order { Verb = Verbs.Move, Speed = speed == MoveSpeed.Run ? "run" : "walk", Point = dest[u] };
                u.Path = rest;
                u.PathIndex = 0;
                u.Speed = speed;
                walk[u] = along;
            }
            plan.EverySecond = side =>
            {
                int turn = sim.Clock.Turn;
                if (!came && turn >= arrive)
                {
                    came = true;
                    // In file, 6 m apart, coming in off the edge; a scout comes first, well ahead.
                    var file2 = party.Where(u => u != scout).ToList();
                    for (int i = 0; i < file2.Count; i++) { file2[i].Gone = false; file2[i].Position = PointAt(path, 6f * (file2.Count - 1 - i)); Walk(file2[i], path, MoveSpeed.Walk); }
                    if (scout != null) { scout.Gone = false; scout.Position = PointAt(path, 6f * file2.Count + ScoutAhead); Walk(scout, path, MoveSpeed.Walk); }
                }
                if (!came) return;
                var knows = sim.Fog.For(mission.Enemy);
                if (!turnedBack && scout != null && scout.CanAct && !contact && knows.Contacts.Any(k => k.Visible && ReferenceEquals(k.Unit.Force, mission.Marines) && !k.Unit.Def.Flying))
                {
                    // The scout has seen them: everyone turns round and goes back the way they came.
                    turnedBack = true;
                    plan.FallingBack = true;
                    sim.Shout(scout, "Soldiers! Go back, go back!", always: true);
                    var back = Enumerable.Reverse(path).ToList();
                    foreach (var u in party.Where(u => u.CanAct && !u.Gone)) Walk(u, back, MoveSpeed.Run);
                }
                if (!contact && party.Any(u => !u.Gone && (u.Wounds > 0 || u.Condition != Condition.Ok || u.Suppression >= 0.3f)))
                {
                    contact = true;
                    plan.Sprung = true;
                    foreach (var u in party.Where(u => u.CanAct && !u.Gone))
                    {
                        if (u.Cargo == null) { u.Order = null; u.Path = null; continue; }
                        // A porter runs for the nearer end of the trail.
                        int i = Nearest(trail, u.Position);
                        var along = Length(trail, i) < Length(trail, trail.Count - 1) - Length(trail, i) ? Enumerable.Reverse(trail).ToList() : trail;
                        Walk(u, along, MoveSpeed.Run);
                    }
                }
                foreach (var u in party.Where(u => u.CanAct && !u.Gone))
                {
                    if (dest.TryGetValue(u, out Vec2 d) && Vec2.Distance(u.Position, d) <= 8f && AtEdge(sim.Map, u.Position, 10f)) { sim.LeaveMap(u); continue; }
                    // Anyone who's stopped (pinned, or brought up short) gets going again if they still should be.
                    bool going = !contact || u.Cargo != null;
                    if (going && u.Order == null && u.Suppression < 0.5f && !u.LegHit && walk.TryGetValue(u, out var w)) Walk(u, w, contact || turnedBack ? MoveSpeed.Run : MoveSpeed.Walk);
                }
            };
            if (shepherds.Count > 0)
            {
                bool out_ = false;
                sim.OnSecond += () =>
                {
                    if (!out_ && sim.Clock.Turn >= shepherdsCome)
                    {
                        out_ = true;
                        for (int i = 0; i < shepherds.Count; i++)
                        {
                            var u = shepherds[i];
                            u.Gone = false;
                            u.Position = PointAt(shepherdPath, 4f * (shepherds.Count - 1 - i));
                            u.Path = shepherdPath.Skip(Nearest(shepherdPath, u.Position) + 1).ToList();
                            u.PathIndex = 0;
                            u.Order = new Order { Verb = Verbs.Move, Speed = "walk", Point = shepherdPath[shepherdPath.Count - 1] };
                        }
                    }
                    if (!out_) return;
                    foreach (var u in shepherds.Where(u => u.CanAct && !u.Gone))
                    {
                        if (Vec2.Distance(u.Position, shepherdPath[shepherdPath.Count - 1]) <= 8f) sim.LeaveMap(u);
                        else if (u.Order == null && u.Suppression < 0.2f)
                        {
                            u.Path = shepherdPath.Skip(Nearest(shepherdPath, u.Position) + 1).ToList();
                            u.PathIndex = 0;
                            u.Order = new Order { Verb = Verbs.Move, Speed = "walk", Point = shepherdPath[shepherdPath.Count - 1] };
                        }
                    }
                };
            }

            var porters = party.Where(u => u.Cargo != null).ToList();
            int crates = porters.Count;
            mission.Judge = m =>
            {
                var gone = m.Sim.Log.Events.OfType<UnitLeftMap>().Select(e => e.Unit).FirstOrDefault(u => porters.Contains(u) && u.Cargo != null);
                if (gone != null) return ("failure", Capital(gone.Cargo ?? "the weapons") + " got " + (turnedBack || Vec2.Distance(gone.Position, path[0]) < 20f ? "away." : "through."));
                if (m.Sim.Log.Events.OfType<CargoTaken>().Count(e => porters.Contains(e.From)) >= crates) return ("success", "The party is stopped and the weapons are taken.");
                return null;
            };
            mission.Status = m => contact ? "onit" : "notyet";
            mission.NotYetWord = "Waiting";
            mission.OnItWord = "Sprung";
            mission.DoneWord = "Taken";

            mission.Title = "Operation " + Pick(names.Routes, rng) + " Snare";
            Vec2 lead = formation[0].pos;
            string ends = map.SquareOf(trail[0]) + " to " + map.SquareOf(trail[trail.Count - 1]);
            mission.Brief.Add("Task: ambush a party carrying weapons along " + trailName + " (" + ends + "), stop it, and take the weapons.");
            mission.Brief.Add("Intent: the weapons off the trail, and one of them alive to question.");
            mission.Brief.Add("Enemy: intel says 4 to 6 soldiers on foot with crates, expected about turn " + Expected + " from the " + Compass(expectedFrom - middle) + " end of the trail. Intel isn't sure: they could come from the other end, come late, or turn out to be shepherds. " + Hints[goal]);
            mission.Roe = StandingRoe;
            mission.Brief.Add("Civilians: shepherds use the trail" + (village != null ? ", and there are villagers in " + village.Label : "") + ". " + RoeLine(mission.Roe));
            int back = (int)Math.Round(DistanceTo(lead, trail) / 50f) * 50;
            mission.Brief.Add(SquadLine("at " + map.SquareOf(lead) + ", about " + back + " m from the trail"));
            mission.MainObjective = "Stop the party and take the weapons.";
            mission.WinText = "Every crate the party carries taken: search the porters, or search where they fell.";
            mission.LoseText = LoseLine(mission, "a crate gets off the map.");
            mission.OutOfTimeText = "Out of time. The weapons are still out there.";

            mission.Secondary.Add(new SecondaryObjective("civilians", "No civilians hurt (positive ID first)", m => m.AnyoneHurt(m.Civilians)));
            mission.Secondary.Add(new SecondaryObjective("prisoner", "One of them taken alive to question", m => !party.Any(u => u.Detained && !u.Captured)) { AtEnd = true });
            Vec2 side = Unit(lead - middle);
            mission.Director = new Director(mission, data, new Dice(seed, 13), new DirectorSetup
            {
                Focus = middle,
                FocusWord = "the trail",
                FarSide = p => Dot(p - middle, side) < -60f,
                FarWord = "the far side of the trail",
                AddIns = new[] { Director.CiviliansRun, Director.RadioDown },
            });
            mission.Secondary.Add(EveryMarineHome());
            return mission;
        }

        static readonly Dictionary<string, string> Hints = new Dictionary<string, string>
        {
            { Straight, "They've used this trail for months without trouble." },
            { Scout, "They've lost men to ambushes before." },
            { MoreMen, "They've used this trail for months without trouble." },
        };

        static string Capital(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);

        static int Nearest(List<Vec2> line, Vec2 p)
        {
            int best = 0;
            float bestD = float.MaxValue;
            for (int i = 0; i < line.Count; i++)
            {
                float d = Vec2.Distance(line[i], p);
                if (d < bestD) { bestD = d; best = i; }
            }
            return best;
        }

        /// <summary>How far along a line its i-th point is.</summary>
        static float Length(List<Vec2> line, int upTo)
        {
            float total = 0f;
            for (int i = 0; i < upTo && i + 1 < line.Count; i++) total += Vec2.Distance(line[i], line[i + 1]);
            return total;
        }

        /// <summary>The point this far along a line from its start.</summary>
        static Vec2 PointAt(List<Vec2> line, float meters)
        {
            for (int i = 0; i + 1 < line.Count; i++)
            {
                float seg = Vec2.Distance(line[i], line[i + 1]);
                if (meters <= seg && seg > 0f) return line[i] + (line[i + 1] - line[i]) * (meters / seg);
                meters -= seg;
            }
            return line[line.Count - 1];
        }

        static Vec2 PointAlong(List<Vec2> line, float t) => PointAt(line, Length(line, line.Count - 1) * t);
    }
}
