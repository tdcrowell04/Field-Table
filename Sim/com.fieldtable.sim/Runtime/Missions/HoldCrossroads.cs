using System;
using System.Collections.Generic;
using System.Linq;
using static FieldTable.Sim.MissionKit;

namespace FieldTable.Sim
{
    /// <summary>
    /// Hold the crossroads (ledger round 16): the squad holds a crossroads in a small village until relief comes, on a
    /// turn a die picks between 15 and 25 that nobody tells you. The enemy probes and then pushes, comes from two sides
    /// at once, or pins you with a machine gun while the rest work around.
    /// </summary>
    public static class HoldCrossroads
    {
        public const string Probe = "probe", TwoSides = "two sides", PinAndFlank = "pin";
        public static readonly IReadOnlyList<string> Goals = new[] { Probe, TwoSides, PinAndFlank };

        static readonly Dictionary<string, (string said, string truth)> GoalWords = Words(
            (Probe, "they're only testing you, and the real push comes after", "test you first and push after"),
            (TwoSides, "they'll come at you from two sides at once", "come at you from two sides at once"),
            (PinAndFlank, "a machine gun will pin you while the rest go around", "pin you down and go around"));

        /// <summary>Relief comes at the end of a turn between these, picked by a die and never told.</summary>
        public const int ReliefFirst = 15, ReliefLast = 25;

        /// <summary>A side holds the crossroads with a fighter this close to the crossing and none of the other side's.</summary>
        public const float HoldWithin = 40f;

        /// <summary>The attack breaks off once this much of it is down.</summary>
        public const float Breaking = 0.6f;

        /// <summary>
        /// The enemy pulls back to this far from the crossing, creeps up to this far out under cover before going in,
        /// and a probe fires from this far.
        /// </summary>
        const float StartOut = 520f, GatherOut = 140f, ProbeOut = 220f;

        /// <summary>Makes the mission. A die picks the enemy's goal unless one is asked for (probe, two sides, or pin).</summary>
        public static Mission Create(GameData data, NamePools names, ulong seed, string goal = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (names == null) throw new ArgumentNullException(nameof(names));
            var (mapData, map) = MakeMap(data, names, seed, "Hold the crossroads " + seed, r =>
            {
                r.Crossroads = true;
                r.Exclude.Add(MapFeatures.Rivers);
                r.Exclude.Add(MapFeatures.Bridges);
                r.Exclude.Add(MapFeatures.Towns);
                r.Exclude.Add(MapFeatures.Creeks);
            }, (_, m) => FindCrossing(m) is Vec2 x && Settlement(m, x) is Landmark v && v.Contains(x) && BuildingsIn(m, v).Count() >= 4);
            Vec2 c = FindCrossing(map) ?? new Vec2(map.WidthMeters / 2f, map.HeightMeters / 2f);
            var village = Settlement(map, c);
            var routes = map.Landmarks.Where(l => l.Kind == "line" && l.Label.StartsWith("Route ", StringComparison.Ordinal)).OrderBy(l => DistanceTo(c, l.Points)).Take(2).ToList();
            var rng = new Dice(seed, 11);

            var mission = NewMission(StartStates.Crossroads, data, mapData, map, seed);
            var sim = mission.Sim;
            var crossroads = AddPoint(mapData, map, "the crossroads", c, "where " + string.Join(" and ", routes.Select(r => r.Label)) + " cross", "crossroads", "the crossing", "crossing", "the intersection");
            mission.Objective = crossroads;

            // The four ways out of the crossroads along the roads. The squad came in along one of them; the enemy comes by the others.
            var arms = new List<Vec2>();
            foreach (var r in routes) { var d = Direction(r.Points, c); arms.Add(d); arms.Add(d * -1f); }
            if (arms.Count < 4) arms = new List<Vec2> { new Vec2(0, -1), new Vec2(0, 1), new Vec2(1, 0), new Vec2(-1, 0) };
            Vec2 home = arms[(int)(Next(rng) * arms.Count) % arms.Count];
            var formation = Toward(map, c + home * 10f, c - home * 100f);
            AddSquad(sim, mission.Marines, data, names, rng, formation);
            var squad = sim.Units.Where(u => ReferenceEquals(u.Force, mission.Marines) && !u.Gone).ToList();

            float goalRoll = Next(rng);
            goal ??= Goals[Math.Min(Goals.Count - 1, (int)(goalRoll * Goals.Count))];
            // They've come to take it, so they keep at it until 60% of them are down.
            var plan = new EnemyPlan(goal, crossroads, Goals, GoalWords) { Breaks = Breaking };
            var others = Shuffle(arms.Where(a => Vec2.Distance(a, home) > 0.5f).ToList(), rng);
            Vec2 main = others[0];
            // The second way in: another road for two sides, and for working around, the road off to one side of the gun.
            var aside = others.Where(a => Math.Abs(Dot(a, main)) < 0.5f).ToList();
            Vec2 second = aside.Count > 0 ? aside[0] : others[Math.Min(1, others.Count - 1)];
            if (goal == TwoSides) second = others[Math.Min(1, others.Count - 1)];

            var made = new List<Unit>();
            var numbered = new Dictionary<string, int>();
            var groups = new List<Group>();
            var taken = new List<Vec2>(squad.Select(u => u.Position));
            Group NewGroup(string role, Vec2 way, string[] kinds, int go)
            {
                var g = new Group { Role = role, Way = way, GoTurn = go, Team = "attack " + (groups.Count + 1) };
                Vec2 start = OutAlong(map, c, way, StartOut);
                foreach (var kind in kinds)
                {
                    Vec2 at = RingSpot(map, rng, start, 0f, 60f, p => !Seen(map, squad, p), taken, (p, t) => t.Cover + t.Concealment) ?? Standable(map, start, taken);
                    taken.Add(at);
                    var name = kind == "en-pkm" ? "PKM gunner" : kind == "en-rpg" ? "RPG gunner" : "Rifleman";
                    // They aren't on the map yet: they arrive here when their group sets out.
                    var u = Fighter(data, mission.Enemy, made, numbered, kind, name, at, Posture.Kneeling, c);
                    u.Gone = true;
                    u.Team = g.Team;
                    g.Men.Add(u);
                }
                groups.Add(g);
                return g;
            }
            string[] Riflemen(int n) => Enumerable.Repeat("en-rf", n).ToArray();
            int first = Between(rng, 2, 4);
            switch (goal)
            {
                case Probe:
                {
                    var probe = NewGroup("probe", main, Riflemen(3), first);
                    int rest = Between(rng, 11, 14);
                    var push = NewGroup("assault", main, new[] { "en-pkm", "en-pkm", "en-rpg" }.Concat(Riflemen(rest - 3)).ToArray(), first + Between(rng, 4, 6));
                    probe.Joins = push;
                    break;
                }
                case TwoSides:
                {
                    int total = Between(rng, 14, 17), half = total / 2;
                    var a = NewGroup("assault", main, new[] { "en-pkm", "en-rpg" }.Concat(Riflemen(half - 2)).ToArray(), first);
                    var b = NewGroup("assault", second, new[] { "en-pkm", "en-rpg" }.Concat(Riflemen(total - half - 2)).ToArray(), first);
                    a.WaitFor = b; b.WaitFor = a;
                    break;
                }
                default:
                {
                    var gun = NewGroup("fire", main, new[] { "en-pkm", "en-pkm", "en-rpg" }.Concat(Riflemen(Between(rng, 2, 3))).ToArray(), first);
                    var around = NewGroup("assault", second, new[] { "en-rpg" }.Concat(Riflemen(Between(rng, 8, 10))).ToArray(), first);
                    around.WaitFor = gun;
                    break;
                }
            }
            foreach (var u in made) { u.Force.Plan = plan; sim.AddUnit(u); }
            foreach (var g in groups) PlanGroup(map, rng, c, squad, g);

            var homes = village != null ? BuildingsIn(map, village).ToList() : new List<Landmark>();
            AddCivilians(sim, mission.Civilians, data, rng, homes, Between(rng, 3, 5));

            // Relief, on a turn nobody is told.
            int relief = Between(rng, ReliefFirst, ReliefLast);
            mission.TurnLimit = ReliefLast + 5;
            bool heldBefore = false, everLost = false;
            mission.Judge = m =>
            {
                bool ours = Holders(m, m.Marines), theirs = Holders(m, m.Enemy);
                bool enemyHolds = theirs && !ours;
                bool lost = enemyHolds && heldBefore;
                heldBefore = enemyHolds;
                everLost |= enemyHolds;
                if (lost) return ("failure", "The enemy took the crossroads.");
                if (m.Sim.Clock.Turn > relief) return ("success", "Relief is here, on turn " + relief + ". The crossroads held.");
                return null;
            };
            mission.Status = m => Holders(m, m.Marines) ? "onit" : "notyet";
            mission.OnItWord = "Holding";
            mission.NotYetWord = "Not held";
            mission.DoneWord = "Held";
            plan.EverySecond = side => Attack(sim, plan, groups, c, mission.Marines);

            mission.Title = "Operation " + Pick(names.Routes, rng) + " Post";
            string grid = map.SquareOf(c).ToString();
            string villageName = village?.Label ?? "the village";
            mission.Brief.Add("Task: hold the crossroads in " + villageName + " (" + grid + ") until relief arrives.");
            mission.Brief.Add("Intent: the crossroads open for the convoy behind the relief, and nobody in " + villageName + " hurt.");
            mission.Brief.Add("Enemy: expected from the " + Compass(main) + ". Strength unknown. " + Hint(goal, second));
            mission.Roe = StandingRoe;
            mission.Brief.Add("Civilians: in " + villageName + ". " + RoeLine(mission.Roe));
            mission.Brief.Add(SquadLine("at the crossroads, having come in from the " + Compass(home)) + " Relief comes up the same road, some time between turn " + ReliefFirst + " and turn " + ReliefLast + ".");
            mission.MainObjective = "Hold the crossroads (" + grid + ") until relief arrives.";
            mission.WinText = "Still holding the crossroads when relief arrives, at the end of a turn between " + ReliefFirst + " and " + ReliefLast + " that nobody tells you. Holding it means a Marine who can fight within " + HoldWithin + " m of the crossing.";
            mission.LoseText = Mission.CombatIneffective + " Marines down, or the enemy holding the crossroads (a fighter within " + HoldWithin + " m and no Marine) at the end of two turns in a row.";

            mission.Secondary.Add(new SecondaryObjective("civilians", "No villagers hurt", m => m.AnyoneHurt(m.Civilians)));
            mission.Secondary.Add(new SecondaryObjective("never-lost", "The crossroads never lost", m => everLost));
            mission.Director = new Director(mission, data, new Dice(seed, 13), new DirectorSetup
            {
                Focus = c,
                FocusWord = "the crossroads",
                FarSide = p => Vec2.Distance(p, c) > 200f && Dot(p - c, home) < 0f,
                FarWord = "the enemy's side of the crossroads",
                NearSide = p => Dot(p - c, home) > 30f,
                NearWord = "the road the squad came in on",
                AddIns = new[] { Director.Reinforcements, Director.CiviliansRun, Director.Sniper, Director.PullBack, Director.RadioDown },
                Slow = () => true,
                SlowWhy = "Fits because the first attack has been beaten off.",
            });
            mission.Secondary.Add(EveryMarineHome());
            return mission;
        }

        /// <summary>Where the two roads cross, if they do.</summary>
        static Vec2? FindCrossing(MapGrid map)
        {
            var routes = map.Landmarks.Where(l => l.Kind == "line" && l.Label.StartsWith("Route ", StringComparison.Ordinal)).ToList();
            for (int i = 0; i < routes.Count; i++)
                for (int j = i + 1; j < routes.Count; j++)
                {
                    var a = routes[i].Points;
                    var b = routes[j].Points;
                    for (int s = 0; s + 1 < a.Count; s++)
                        for (int t = 0; t + 1 < b.Count; t++)
                            if (Cross(a[s], a[s + 1], b[t], b[t + 1]) is Vec2 x) return x;
                }
            return null;
        }

        static Vec2? Cross(Vec2 a, Vec2 b, Vec2 c, Vec2 d)
        {
            float den = (b.X - a.X) * (d.Y - c.Y) - (b.Y - a.Y) * (d.X - c.X);
            if (Math.Abs(den) < 1e-6f) return null;
            float t = ((c.X - a.X) * (d.Y - c.Y) - (c.Y - a.Y) * (d.X - c.X)) / den;
            float u = ((c.X - a.X) * (b.Y - a.Y) - (c.Y - a.Y) * (b.X - a.X)) / den;
            if (t < 0f || t > 1f || u < 0f || u > 1f) return null;
            return a + (b - a) * t;
        }

        /// <summary>True while a side has someone who can fight within reach of the crossing.</summary>
        static bool Holders(Mission m, Force side) =>
            m.Sim.Units.Any(u => ReferenceEquals(u.Force, side) && u.CanAct && !u.Def.Flying && !u.Def.Interpreter && Vec2.Distance(u.Position, m.Objective.Center) <= HoldWithin);

        static bool Seen(MapGrid map, List<Unit> squad, Vec2 p) =>
            squad.Any(u => Vec2.Distance(u.Position, p) <= FogOfWar.DayEyeRange && Sight.Trace(map, u.Position, 1.7f, p, 1.1f).Clear);

        static string Hint(string goal, Vec2 second)
        {
            switch (goal)
            {
                case Probe: return "Scouts have been seen watching the village for two days.";
                case TwoSides: return "Fighters were seen gathering in more than one place.";
                default: return "A machine gun team was seen moving in, and others were seen off to the " + Compass(second) + ".";
            }
        }

        // ---------- the attack ----------

        /// <summary>
        /// One group of attackers and how far along it is: waiting to set out, on its way to where it gathers, gathered,
        /// and then going in (or, for a probe, firing a while and pulling back; for the gun, staying to fire).
        /// </summary>
        sealed class Group
        {
            public string Role, Team;
            public Vec2 Way;
            public int GoTurn;
            public Vec2 Gather;
            public Vec2 Back;
            public readonly List<Unit> Men = new List<Unit>();
            public Group WaitFor, Joins;
            public int Phase;
            public int GatheredTurn = -1;
            public bool Firing;
        }

        /// <summary>Has men fire on the Marines they know of, nearest the crossing first, shared out between them.</summary>
        static void Suppress(Simulation sim, List<Unit> men, Vec2 c, Force marines)
        {
            if (men.Count == 0) return;
            var knows = sim.Fog.For(men[0].Force);
            var targets = sim.Units.Where(x => ReferenceEquals(x.Force, marines) && x.CanAct && !x.Def.Flying && knows.About(x.Id) != null)
                .OrderBy(x => Vec2.Distance(knows.About(x.Id).LastKnownPosition, c)).ToList();
            if (targets.Count == 0) return;
            for (int i = 0; i < men.Count; i++)
            {
                var u = men[i];
                u.Order = new Order { Verb = Verbs.Suppress, Target = targets[i % targets.Count].Name, TargetUnit = targets[(i * 3) % targets.Count] };
                u.Path = null;
                u.Posture = Posture.Prone;
            }
        }

        const int Waiting = 0, Going = 1, Gathered = 2, In = 3, Done = 4;

        /// <summary>Where a group gathers: a probe and the gun where they can see the crossing, the rest in cover out of sight of it.</summary>
        static void PlanGroup(MapGrid map, Dice rng, Vec2 c, List<Unit> squad, Group g)
        {
            bool sees = g.Role == "probe" || g.Role == "fire";
            float out_ = g.Role == "probe" ? ProbeOut : g.Role == "fire" ? 300f : GatherOut;
            Vec2 near = OutAlong(map, c, g.Way, out_);
            // The probe and the gun want a view of the crossing; the rest want to be hidden from every Marine.
            g.Gather = RingSpot(map, rng, near, 0f, sees ? 50f : 70f, null, new List<Vec2>(), (p, t) =>
                t.Cover + t.Concealment * 0.5f + (sees ? (Sight.Trace(map, p, 1.1f, c, 1.5f).Clear ? 2f : 0f) : (Seen(map, squad, p) ? 0f : 3f))) ?? near;
            g.Back = OutAlong(map, c, g.Way, StartOut);
        }

        /// <summary>Once a second: moves each group on when it's time, and once the side has lost half, stops giving orders.</summary>
        static void Attack(Simulation sim, EnemyPlan plan, List<Group> groups, Vec2 c, Force marines)
        {
            if (plan.FallingBack) return;
            var all = groups.SelectMany(g => g.Men).ToList();
            if (all.Count(u => !u.CanAct && !u.Gone) >= all.Count * Breaking)
            {
                // Broken: everyone's own sense takes over, which pulls them back.
                plan.FallingBack = true;
                foreach (var u in all.Where(u => u.CanAct)) { u.Order = null; u.Path = null; }
                return;
            }
            int turn = sim.Clock.Turn;
            foreach (var g in groups)
            {
                if (g.Phase == Waiting)
                {
                    if (turn < g.GoTurn) continue;
                    foreach (var u in g.Men) u.Gone = false;
                    // They've had eyes on the crossroads: they come knowing where the squad was when they set out.
                    var knows = sim.Fog.For(g.Men[0].Force);
                    foreach (var x in sim.Units.Where(x => ReferenceEquals(x.Force, marines) && x.CanAct && !x.Def.Flying))
                    {
                        var k = knows.Track(x);
                        if (k.Known) continue;
                        k.Known = true;
                        k.LastKnownPosition = x.Position;
                        k.LastSeenStep = sim.Clock.Step;
                    }
                }
                var men = g.Men.Where(u => u.CanAct).ToList();
                if (men.Count == 0) { g.Phase = Done; continue; }
                switch (g.Phase)
                {
                    case Waiting:
                        // They've crept up under cover before anyone sees them: they show up already where they gather.
                        var spots = new List<Vec2>();
                        foreach (var u in men)
                        {
                            u.Position = Standable(sim.Map, g.Gather + Turn(new Vec2(0f, -5f - 3f * (spots.Count / 6)), spots.Count * Math.PI / 3), spots.Concat(sim.Units.Where(x => x != u && !x.Gone).Select(x => x.Position)));
                            u.Posture = Posture.Prone;
                            spots.Add(u.Position);
                        }
                        g.Phase = Gathered;
                        g.GatheredTurn = turn;
                        break;
                    case Going:
                        if (men.Any(u => u.Order != null && u.Order.Verb != Verbs.Suppress)) break;
                        g.Phase = Gathered;
                        g.GatheredTurn = turn;
                        break;
                    case Gathered:
                        if (g.Role == "fire")
                        {
                            // The gun group opens up once the group going around is ready to go in.
                            if (!g.Firing && groups.Any(o => o.WaitFor == g && o.Phase >= Gathered)) { Suppress(sim, men, c, marines); g.Firing = true; }
                            break;
                        }
                        if (g.Role == "probe")
                        {
                            // A turn or two of shooting, or the first one hurt, and back they go to join the push.
                            if (turn == g.GatheredTurn) { Suppress(sim, men, c, marines); break; }
                            bool hurt = g.Men.Any(u => u.Wounds > 0 || !u.CanAct);
                            if (turn - g.GatheredTurn < 2 && !hurt) break;
                            Send(sim, g, g.Back, Verbs.Withdraw);
                            if (g.Joins != null) { g.Joins.Men.AddRange(men); foreach (var u in men) u.Team = g.Joins.Team; }
                            g.Men.Clear();
                            g.Phase = Done;
                            break;
                        }
                        // Going in waits for the other group to be ready, or for the gun to have fired a turn.
                        var other = g.WaitFor;
                        bool ready = other == null || other.Phase == Done || (other.Phase >= Gathered && (other.Role != "fire" || turn - other.GatheredTurn >= 1));
                        if (!ready && turn - g.GatheredTurn < 3) break;
                        // The machine guns stay back and fire on the squad; the rest go in by bounds, half rushing while
                        // the other half kneels and covers them.
                        var guns = men.Where(u => u.Def.Id == "en-pkm").ToList();
                        Suppress(sim, guns, c, marines);
                        Send(sim, g, c, Verbs.Bound, null, men.Except(guns).ToList());
                        g.Phase = In;
                        break;
                    case In:
                        // Once they've all stopped, any still short of the crossing go again, as long as they aren't pinned.
                        if (men.Any(u => u.Order != null && u.Order.Verb != Verbs.Suppress)) break;
                        var shortOf = men.Where(u => Vec2.Distance(u.Position, c) > HoldWithin * 0.5f && u.Suppression < 0.5f && u.Def.Id != "en-pkm").ToList();
                        if (shortOf.Count > 0) Send(sim, g, c, Verbs.Bound, null, shortOf);
                        break;
                }
            }
        }

        /// <summary>
        /// Gives a group an order the way the squad's own team orders go, so they spread out side by side and bound by
        /// halves. Only those named go, when it isn't the whole group.
        /// </summary>
        static void Send(Simulation sim, Group g, Vec2 to, string verb, string posture = null, List<Unit> only = null)
        {
            var going = only ?? g.Men.Where(u => u.CanAct).ToList();
            var staying = g.Men.Where(u => !going.Contains(u)).ToList();
            foreach (var u in staying) u.Team = g.Team + " waiting";
            sim.GiveOrder(g.Men[0].Force, g.Team, new Order { Verb = verb, Where = sim.Coordinates(to), Posture = posture });
            foreach (var u in staying) u.Team = g.Team;
        }
    }
}
