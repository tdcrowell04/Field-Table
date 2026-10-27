using System;
using System.Collections.Generic;
using System.Linq;
using static FieldTable.Sim.MissionKit;

namespace FieldTable.Sim
{
    /// <summary>
    /// Delay the advance (ledger round 17): an enemy platoon is coming up a road toward the battalion. Slow it down
    /// without getting caught, falling back through three lines marked across the map, until turn 20. The enemy pushes
    /// straight up the road, sends scouts first and then the rest, or holds you on the road with a few while the rest
    /// go round through the woods.
    /// </summary>
    public static class DelayAdvance
    {
        public const string Push = "push", Scouts = "scouts", GoRound = "round";
        public static readonly IReadOnlyList<string> Goals = new[] { Push, Scouts, GoRound };

        static readonly Dictionary<string, (string said, string truth)> GoalWords = Words(
            (Push, "the whole platoon is coming straight up the road", "push straight up the road"),
            (Scouts, "scouts go first and the rest follow", "send scouts first"),
            (GoRound, "a few will hold you on the road while the rest go round", "hold you on the road and go round"));

        /// <summary>The battalion is in place at the end of this turn, and the lines are this far along the road from the enemy's end.</summary>
        public const int HoldUntil = 20;
        static readonly float[] LineAt = { 0.4f, 0.58f, 0.76f };

        /// <summary>No line given up before this turn, for the extra.</summary>
        public const int EarliestLineLost = 6;

        /// <summary>Scouts walk this far ahead of the rest; going round keeps this far off the road.</summary>
        const float ScoutsAhead = 300f, RoundOut = 230f;

        /// <summary>Makes the mission. A die picks the enemy's goal unless one is asked for (push, scouts, or round).</summary>
        public static Mission Create(GameData data, NamePools names, ulong seed, string goal = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (names == null) throw new ArgumentNullException(nameof(names));
            // A road from one edge of the map to the other, with no river to cross.
            Landmark Road(MapGrid m) => m.Landmarks.Where(l => l.Kind == "line" && l.Label.StartsWith("Route ", StringComparison.Ordinal) && l.Points.Count >= 2
                && AtEdge(m, Inside(m, l.Points[0]), 30f) && AtEdge(m, Inside(m, l.Points[l.Points.Count - 1]), 30f)).OrderBy(l => l.Label).FirstOrDefault();
            var (mapData, map) = MakeMap(data, names, seed, "Delay the advance " + seed, r =>
            {
                r.Require.Add(MapFeatures.Roads);
                r.Exclude.Add(MapFeatures.Rivers);
                r.Exclude.Add(MapFeatures.Bridges);
            }, (_, m) => Road(m) != null);
            var road = Road(map) ?? map.Landmarks.First(l => l.Kind == "line" && l.Label.StartsWith("Route ", StringComparison.Ordinal));
            var rng = new Dice(seed, 11);

            // Which end they come from; the battalion is at the other.
            bool fromStart = Next(rng) < 0.5f;
            var path = (fromStart ? road.Points : road.Points.Reverse()).Select(p => Inside(map, p)).ToList();
            float total = Length(path, path.Count - 1);

            var mission = NewMission(StartStates.DelayAdvance, data, mapData, map, seed);
            var sim = mission.Sim;
            mission.Objective = road;

            // The three lines, straight across the map where they cross the road.
            var lines = new List<(Vec2 at, Vec2 ahead, Landmark mark)>();
            for (int i = 0; i < LineAt.Length; i++)
            {
                Vec2 at = PointAt(path, total * LineAt[i]);
                Vec2 ahead = Unit(PointAt(path, total * LineAt[i] + 20f) - PointAt(path, total * LineAt[i] - 20f));
                Vec2 across = new Vec2(-ahead.Y, ahead.X);
                Vec2 a = EdgeToward(map, at, across), b = EdgeToward(map, at, across * -1f);
                string label = "Phase Line " + (i + 1);
                mapData.Features.Add(new MapFeature
                {
                    Kind = "control", Label = label, Note = "fall back past it", Aliases = new List<string> { "PL " + (i + 1), "PL" + (i + 1), "line " + (i + 1) },
                    Points = new List<float[]> { new[] { a.X / mapData.SquareSize, a.Y / mapData.SquareSize }, new[] { b.X / mapData.SquareSize, b.Y / mapData.SquareSize } },
                });
                var mark = new Landmark(label, "control", new[] { "PL " + (i + 1), "PL" + (i + 1), "line " + (i + 1) }, "fall back past it", (a + b) * 0.5f,
                    new Vec2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)), new Vec2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y)), new[] { a, b });
                map.AddLandmark(mark);
                lines.Add((at, ahead, mark));
            }
            bool Past(Vec2 p, int line) => Dot(p - lines[line].at, lines[line].ahead) > 0f;

            // The platoon: three squads of nine (a machine gun and an RPG in each) and its leader with the radio.
            float goalRoll = Next(rng);
            goal ??= Goals[Math.Min(Goals.Count - 1, (int)(goalRoll * Goals.Count))];
            var plan = new EnemyPlan(goal, road, Goals, GoalWords);
            var made = new List<Unit>();
            var numbered = new Dictionary<string, int>();
            var squads = new List<List<Unit>>();
            for (int sq = 0; sq < 3; sq++)
            {
                var squad = new List<Unit>();
                foreach (var def in new[] { "en-rf", "en-rf", "en-pkm", "en-rf", "en-rpg", "en-rf", "en-rf", "en-rf", "en-rf" })
                {
                    var u = Fighter(data, mission.Enemy, made, numbered, def, def == "en-pkm" ? "PKM gunner" : def == "en-rpg" ? "RPG gunner" : "Rifleman", path[0], Posture.Standing, path[1]);
                    u.Gone = true;
                    squad.Add(u);
                }
                squads.Add(squad);
            }
            var platoonLeader = new Unit("enemy-pl", "Platoon leader", data.FindUnit("en-sl"), mission.Enemy, path[0]) { Gone = true };
            made.Add(platoonLeader);
            squads[0].Add(platoonLeader);
            Organize(data, made);
            string[] teams = { "1st squad", "2nd squad", "3rd squad" };
            for (int sq = 0; sq < 3; sq++) foreach (var u in squads[sq]) u.Team = teams[sq];
            // Scouts: two from the 1st squad go well ahead.
            var scouts = goal == Scouts ? squads[0].Where(u => u.Def.Id == "en-rf").Take(2).ToList() : new List<Unit>();
            foreach (var u in scouts) { u.Team = "scouts"; squads[0].Remove(u); }
            foreach (var u in made) u.Force.Plan = plan;

            // The squad starts at the first line, by the road, facing the way they'll come.
            Vec2 first = lines[0].at;
            var formation = Toward(map, Standable(map, first + lines[0].ahead * 15f, Array.Empty<Vec2>()), first - lines[0].ahead * 100f);
            AddSquad(sim, mission.Marines, data, names, rng, formation);
            foreach (var u in made) sim.AddUnit(u);
            var village = Settlement(map, lines[1].at);
            if (village != null) AddCivilians(sim, mission.Civilians, data, rng, BuildingsIn(map, village), Between(rng, 2, 4));

            // Going round: off to whichever side has more cover, parallel to the road.
            float Cover(float side) => Enumerable.Range(1, 9).Sum(k => { var p = PointAt(path, total * k / 10f); var q = Inside(map, p + new Vec2(-Dir(path, total * k / 10f).Y, Dir(path, total * k / 10f).X) * (side * RoundOut)); return map.TerrainAt(q).Concealment + map.TerrainAt(q).Cover; });
            float roundSide = Cover(1f) >= Cover(-1f) ? 1f : -1f;
            List<Vec2> RoundRoute()
            {
                var pts = new List<Vec2>();
                for (int k = 1; k <= 10; k++)
                {
                    float d = total * k / 10f;
                    var dir = Dir(path, d);
                    var off = k == 10 ? 0f : roundSide * RoundOut * (float)Math.Sin(Math.PI * Math.Min(1f, k / 8f));
                    pts.Add(Inside(map, PointAt(path, d) + new Vec2(-dir.Y, dir.X) * off));
                }
                return pts;
            }
            var round = RoundRoute();

            // Each group's way forward: a waypoint at a time, by bounds once they've met the squad. Each squad keeps to
            // its own side of the road so they don't all bunch on it.
            var arrive = new[] { 1, 2, 3 };
            var group = new Dictionary<string, (List<Unit> men, List<Vec2> route, int next, int arriveTurn)>();
            float[] lane = { 0f, -35f, 35f };
            for (int sq = 0; sq < 3; sq++)
            {
                var route = new List<Vec2>();
                for (int k = 1; k <= 12; k++)
                {
                    float d = total * k / 12f;
                    var dir = Dir(path, d);
                    route.Add(Standable(map, Inside(map, PointAt(path, d) + new Vec2(-dir.Y, dir.X) * lane[sq]), Array.Empty<Vec2>()));
                }
                if (goal == GoRound && sq > 0) route = round.Select(p => Standable(map, Inside(map, p + new Vec2(lane[sq] * 0.5f, lane[sq] * 0.5f)), Array.Empty<Vec2>())).ToList();
                group[teams[sq]] = (squads[sq], route, 0, arrive[sq] + (goal == Scouts ? 3 : 0));
            }
            if (scouts.Count > 0) group["scouts"] = (scouts, group[teams[0]].route.ToList(), 0, 1);
            bool scoutsFound = false;
            var exitPoint = path[path.Count - 1];
            var came = new HashSet<string>();
            // A group whose order didn't take (no way there) waits a bit and tries the next waypoint, instead of
            // searching for a route every second.
            var waitUntil = new Dictionary<string, int>();
            plan.EverySecond = side =>
            {
                int turn = sim.Clock.Turn;
                // Half of them down, and the rest give up the advance: their own sense pulls them back.
                if (!plan.FallingBack && made.Count(u => !u.CanAct && !u.Gone) >= made.Count * 0.5f)
                {
                    plan.FallingBack = true;
                    foreach (var u in made.Where(u => u.CanAct)) { u.Order = null; u.Path = null; }
                }
                var knows = sim.Fog.For(mission.Enemy);
                if (scouts.Count > 0 && !scoutsFound && came.Contains("scouts") && (scouts.Any(s => s.CanAct && knows.Contacts.Any(k => k.Visible && ReferenceEquals(k.Unit.Force, mission.Marines))) || scouts.Any(s => !s.Gone && (!s.CanAct || s.Suppression > 0.3f))))
                {
                    // The scouts have found them: the rest come up now.
                    scoutsFound = true;
                    foreach (var t in teams) group[t] = (group[t].men, group[t].route, group[t].next, Math.Min(group[t].arriveTurn, turn));
                }
                foreach (var name in group.Keys.ToList())
                {
                    var (men, route, next, arriveTurn) = group[name];
                    if (turn < arriveTurn) continue;
                    // They come in off the edge at the road when it's their turn.
                    if (came.Add(name))
                        for (int i = 0; i < men.Count; i++)
                        {
                            men[i].Gone = false;
                            men[i].Position = Standable(sim.Map, path[0] + new Vec2((i % 3) * 4f, (i / 3) * 4f), Array.Empty<Vec2>());
                        }
                    var up = men.Where(u => u.CanAct).ToList();
                    if (up.Count == 0 || plan.FallingBack) continue;
                    // Through the last waypoint: they walk off the battalion's edge and are past everything.
                    foreach (var u in up.Where(u => Vec2.Distance(u.Position, exitPoint) <= 15f)) sim.LeaveMap(u);
                    if (up.Any(u => u.Order != null && u.Order.Verb != Verbs.Suppress)) continue;
                    // Scouts stop once they've found the squad, and watch.
                    if (name == "scouts" && scoutsFound) continue;
                    if (waitUntil.TryGetValue(name, out int until) && sim.Clock.Step < until) continue;
                    bool contact = up.Any(u => knows.Contacts.Any(k => k.Visible && ReferenceEquals(k.Unit.Force, mission.Marines) && Vec2.Distance(k.LastKnownPosition, u.Position) <= 450f));
                    // On to the next waypoint once the group is past (or near) the one it was going for.
                    Vec2 centre = new Vec2(up.Average(u => u.Position.X), up.Average(u => u.Position.Y));
                    while (next < route.Count - 1 && Vec2.Distance(centre, route[next]) < 40f) next++;
                    group[name] = (men, route, next, arriveTurn);
                    // One look for a way there for the whole group first; with none, skip that waypoint for now.
                    var leader = up.OrderBy(u => Vec2.Distance(u.Position, centre)).First();
                    if (sim.Paths.Find(leader.Position, route[next], false) == null) { Stuck(name); continue; }
                    if (contact)
                    {
                        // The machine gun fires on the squad; the rest go on by bounds.
                        var guns = up.Where(u => u.Def.Id == "en-pkm" && (u.Order == null)).ToList();
                        SuppressKnown(sim, guns, centre, mission.Marines);
                        var going = up.Where(u => u.Def.Id != "en-pkm" && u.Suppression < 0.6f).ToList();
                        if (going.Count > 0) TeamOrder(sim, men, name, route[next], Verbs.Bound, null, going);
                        if (going.Count > 0 && going.All(u => u.Order == null)) Stuck(name);
                        // The gun catches up once the others are well ahead.
                        foreach (var g in up.Where(u => u.Def.Id == "en-pkm" && going.Count > 0 && going.Min(x => Vec2.Distance(x.Position, u.Position)) > 120f)) Go(sim, g, route[next], Verbs.Move, MoveSpeed.Run);
                    }
                    else
                    {
                        TeamOrder(sim, men, name, route[next], Verbs.Move);
                        if (up.All(u => u.Order == null)) Stuck(name);
                    }
                }
            };
            void Stuck(string name)
            {
                var (men, route, next, arriveTurn) = group[name];
                group[name] = (men, route, Math.Min(route.Count - 1, next + 1), arriveTurn);
                waitUntil[name] = sim.Clock.Step + 10 * SimClock.StepsPerSecond;
            }

            // How it ends.
            bool earlyLoss = false;
            mission.TurnLimit = HoldUntil;
            mission.Judge = m =>
            {
                var enemy = m.Sim.Units.Where(u => ReferenceEquals(u.Force, m.Enemy)).ToList();
                var left = new HashSet<Unit>(m.Sim.Log.Events.OfType<UnitLeftMap>().Select(e => e.Unit));
                bool through = enemy.Any(u => (u.CanAct && Past(u.Position, 2)) || left.Contains(u));
                if (m.Sim.Clock.Turn <= EarliestLineLost - 1 && enemy.Any(u => u.CanAct && Past(u.Position, 0))) earlyLoss = true;
                if (through) return ("failure", "They crossed Phase Line 3.");
                if (m.Sim.Clock.Turn > HoldUntil) return ("success", "Turn " + HoldUntil + ", and they never crossed Phase Line 3. The battalion is in place.");
                return null;
            };
            mission.Status = m =>
            {
                var enemy = m.Sim.Units.Where(u => ReferenceEquals(u.Force, m.Enemy) && u.CanAct).ToList();
                return enemy.Any(u => Past(u.Position, 0)) ? "onit" : "notyet";
            };
            mission.NotYetWord = "Holding";
            mission.OnItWord = "Falling back";
            mission.DoneWord = "Held";

            mission.Title = "Operation " + Pick(names.Routes, rng) + " Brake";
            string Grid(int i) => map.SquareOf(lines[i].at).ToString();
            mission.Brief.Add("Task: slow an enemy platoon coming up " + road.Label + " from the " + Compass(path[0] - lines[1].at) + " until the battalion is in place at the end of turn " + HoldUntil + ". Fall back through Phase Lines 1, 2, and 3 (where they cross the road: " + Grid(0) + ", " + Grid(1) + ", " + Grid(2) + ").");
            mission.Brief.Add("Intent: buy time without getting caught; don't give up a line before turn " + EarliestLineLost + ".");
            mission.Brief.Add("Enemy: a platoon, about 30, with machine guns and RPGs. " + Hints[goal]);
            mission.Roe = StandingRoe;
            mission.Brief.Add("Civilians: " + (village != null ? "in " + village.Label + ". " : "none known nearby. ") + RoeLine(mission.Roe));
            mission.Brief.Add(SquadLine("at Phase Line 1 by " + road.Label + ", " + Grid(0)));
            mission.MainObjective = "Keep them short of Phase Line 3 until turn " + HoldUntil + ".";
            mission.WinText = "The end of turn " + HoldUntil + " with no enemy soldier who can fight past Phase Line 3.";
            mission.LoseText = Mission.CombatIneffective + " Marines down, or an enemy soldier past Phase Line 3 at the end of a turn.";

            mission.Secondary.Add(new SecondaryObjective("lines", "No line given up before turn " + EarliestLineLost, m => earlyLoss));
            mission.Director = new Director(mission, data, new Dice(seed, 13), new DirectorSetup
            {
                Focus = lines[1].at,
                FocusWord = "the road",
                FarSide = p => !Past(p, 0) && Vec2.Distance(p, lines[0].at) > 150f,
                FarWord = "the enemy's side of Phase Line 1",
                NearSide = p => Past(p, 2),
                NearWord = "the battalion's side",
                AddIns = new[] { Director.Reinforcements, Director.CiviliansRun, Director.Sniper, Director.PullBack, Director.RadioDown },
                Slow = () => true,
                SlowWhy = "Fits because the advance has stalled.",
            });
            mission.Secondary.Add(EveryMarineHome());
            return mission;
        }

        static readonly Dictionary<string, string> Hints = new Dictionary<string, string>
        {
            { Push, "They've been moving fast and in the open." },
            { Scouts, "They've been careful so far." },
            { GoRound, "Their last attack went round our flank." },
        };

        static float Length(List<Vec2> line, int upTo)
        {
            float total = 0f;
            for (int i = 0; i < upTo && i + 1 < line.Count; i++) total += Vec2.Distance(line[i], line[i + 1]);
            return total;
        }

        static Vec2 PointAt(List<Vec2> line, float meters)
        {
            meters = Math.Max(0f, meters);
            for (int i = 0; i + 1 < line.Count; i++)
            {
                float seg = Vec2.Distance(line[i], line[i + 1]);
                if (meters <= seg && seg > 0f) return line[i] + (line[i + 1] - line[i]) * (meters / seg);
                meters -= seg;
            }
            return line[line.Count - 1];
        }

        static Vec2 Dir(List<Vec2> line, float meters) => Unit(PointAt(line, meters + 15f) - PointAt(line, meters - 15f));
    }
}
