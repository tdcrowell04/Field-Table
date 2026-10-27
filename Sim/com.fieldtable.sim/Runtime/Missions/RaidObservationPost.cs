using System;
using System.Collections.Generic;
using System.Linq;
using static FieldTable.Sim.MissionKit;

namespace FieldTable.Sim
{
    /// <summary>
    /// Raid the observation post (ledger round 17): an enemy post on high ground is spotting for their artillery. Get
    /// in, wreck its radio and scopes by searching it, and get back past the start line before their quick reaction
    /// force arrives, on a turn a die picks between 8 and 14. The post is sleepy and easy to surprise, has a patrol out,
    /// or expected you and set a trap.
    /// </summary>
    public static class RaidObservationPost
    {
        public const string Sleepy = "sleepy", Patrol = "patrol", Trap = "trap";
        public static readonly IReadOnlyList<string> Goals = new[] { Sleepy, Patrol, Trap };

        static readonly Dictionary<string, (string said, string truth)> GoalWords = Words(
            (Sleepy, "half of them up there are asleep", "sleep at their post"),
            (Patrol, "they keep a patrol out around the hill", "keep a patrol out"),
            (Trap, "they knew you were coming and they're waiting for you", "set a trap for you"));

        /// <summary>Their quick reaction force arrives at the end of a turn between these, picked by a die and never told.</summary>
        public const int QrfFirst = 8, QrfLast = 14;

        /// <summary>Sleepers wake when a Marine gets this close, shots land near them, or someone awake at the post sees a Marine.</summary>
        public const float WakeWithin = 25f;

        /// <summary>The squad starts about this far from the post, and never nearer than the least; the patrol walks this far out.</summary>
        const float StartDistance = 400f, StartNearest = 250f, PatrolOut = 180f;

        /// <summary>Makes the mission. A die picks the enemy's goal unless one is asked for (sleepy, patrol, or trap).</summary>
        public static Mission Create(GameData data, NamePools names, ulong seed, string goal = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (names == null) throw new ArgumentNullException(nameof(names));
            Landmark High(MapGrid m) => m.Landmarks.Where(l => l.Kind == "hill").OrderBy(l => l.Label)
                .FirstOrDefault(h => m.GroundAt(h.Center) >= 15f && m.GroundAt(h.Center) <= 90f && !AtEdge(m, h.Center, 250f) && !InSettlement(m, h.Center, 40f) && !m.TerrainAt(h.Center).Deep);
            var (mapData, map) = MakeMap(data, names, seed, "Observation post " + seed, r => r.Require.Add(MapFeatures.Hills), (_, m) => High(m) != null);
            var hill = High(map) ?? map.Landmarks.Where(l => l.Kind == "hill").OrderByDescending(l => map.GroundAt(l.Center)).First();
            Vec2 post = Standable(map, hill.Center, Array.Empty<Vec2>());
            var rng = new Dice(seed, 11);

            var mission = NewMission(StartStates.ObservationPost, data, mapData, map, seed);
            var sim = mission.Sim;
            var postMark = AddPoint(mapData, map, "the post", post, "their observation post on " + hill.Label, "post", "the observation post", "OP", "the OP");
            mission.Objective = postMark;
            var radio = sim.PlaceGear("the post's radio", post + new Vec2(1.5f, 0f));
            var scopes = sim.PlaceGear("the post's scopes", post + new Vec2(-1.5f, 0.5f));

            float goalRoll = Next(rng);
            goal ??= Goals[Math.Min(Goals.Count - 1, (int)(goalRoll * Goals.Count))];
            var plan = new EnemyPlan(goal, postMark, Goals, GoalWords);
            var made = new List<Unit>();
            var numbered = new Dictionary<string, int>();
            var taken = new List<Vec2>();
            var atPost = new List<Vec2> { radio.Position, scopes.Position };
            Unit AtPost(string def, string name, Posture posture)
            {
                // Round the gear, a few meters apart.
                var at = Standable(map, post + Turn(new Vec2(0f, -6f), atPost.Count * Math.PI / 2.5), atPost);
                atPost.Add(at);
                taken.Add(at);
                return Fighter(data, mission.Enemy, made, numbered, def, name, at, posture, post + new Vec2(0f, 100f));
            }
            // The post: an observer, two riflemen on security (one leads it and carries their own radio), and on a busy
            // post a machine gun.
            AtPost("en-rf", "Rifleman", Posture.Kneeling);
            AtPost("en-rf", "Observer", Posture.Prone);
            AtPost("en-rf", "Rifleman", Posture.Kneeling);
            if (goal != Sleepy) AtPost("en-pkm", "PKM gunner", Posture.Prone);
            var postMen = made.ToList();
            var patrol = new List<Unit>();
            var ambush = new List<Unit>();

            // Where the squad will come from: the side of the hill with the most room, and the start line in front of it.
            var sides = Enumerable.Range(0, 8).Select(k => Turn(new Vec2(0f, -1f), k * Math.PI / 4)).ToList();
            Vec2 from = sides.OrderByDescending(d => Vec2.Distance(post, EdgeToward(map, post, d)) + Next(rng) * 60f).First();
            if (goal == Patrol)
            {
                // A loop round the hill, a few hundred meters long, walked again and again.
                for (int i = 0; i < 3; i++)
                {
                    var at = Standable(map, post + from * PatrolOut + new Vec2(4f * i, 0f), taken);
                    taken.Add(at);
                    patrol.Add(Fighter(data, mission.Enemy, made, numbered, "en-rf", "Rifleman", at, Posture.Standing, post));
                }
            }
            else if (goal == Trap)
            {
                // Lying low on the slope the squad will come up, holding fire until it's close.
                int n = Between(rng, 5, 6);
                for (int i = 0; i < n; i++)
                {
                    string def = i == 0 ? "en-pkm" : i == 1 ? "en-rpg" : "en-rf";
                    var at = RingSpot(map, rng, post + from * 120f, 0f, 70f, p => !map.TerrainAt(p).SeeOut && Dot(p - post, from) > 50f, taken,
                        (p, t) => t.Concealment * 2f + t.Cover) ?? Standable(map, post + from * 120f, taken);
                    ambush.Add(Fighter(data, mission.Enemy, made, numbered, def, def == "en-pkm" ? "PKM gunner" : def == "en-rpg" ? "RPG gunner" : "Rifleman", at, Posture.Prone, post + from * 400f));
                }
            }
            // Their quick reaction force: a squad that comes in from the far side when its turn comes.
            int qrfTurn = Between(rng, QrfFirst, QrfLast);
            Vec2 qrfFrom = EdgeToward(map, post, from * -1f);
            var qrf = new List<Unit>();
            foreach (var def in new[] { "en-rf", "en-rf", "en-pkm", "en-rf", "en-rpg", "en-rf", "en-rf", "en-rf" })
            {
                var u = Fighter(data, mission.Enemy, made, numbered, def, def == "en-pkm" ? "PKM gunner" : def == "en-rpg" ? "RPG gunner" : "Rifleman", qrfFrom, Posture.Standing, post);
                u.Gone = true;
                u.Team = "reaction force";
                qrf.Add(u);
            }
            Organize(data, made, postMen.Concat(patrol).Concat(ambush).ToList(), qrf);
            foreach (var u in made) u.Force.Plan = plan;

            var onTheHill = made.Except(qrf).ToList();
            var formation = StartOutOfSight(sim, post, p => Dot(p - post, from) > 150f && !InSettlement(map, p), onTheHill, StartDistance, StartNearest, Inside(map, post + from * StartDistance));
            AddSquad(sim, mission.Marines, data, names, rng, formation);
            foreach (var u in made) sim.AddUnit(u);
            if (goal == Sleepy) foreach (var u in postMen.Skip(1).Take(2)) { u.Asleep = true; u.Posture = Posture.Prone; }
            var village = Settlement(map, post);
            if (village != null) AddCivilians(sim, mission.Civilians, data, rng, BuildingsIn(map, village), Between(rng, 2, 3));

            // The start line, a little in front of where the squad starts, square to the way to the post.
            Vec2 lead = formation[0].pos;
            Vec2 toPost = Unit(post - lead);
            Vec2 lineAt = lead + toPost * 12f;
            Vec2 across = new Vec2(-toPost.Y, toPost.X);
            Vec2 la = EdgeToward(map, lineAt, across), lb = EdgeToward(map, lineAt, across * -1f);
            mapData.Features.Add(new MapFeature
            {
                Kind = "control", Label = "the start line", Note = "get back past it", Aliases = new List<string> { "start line", "the line of departure" },
                Points = new List<float[]> { new[] { la.X / mapData.SquareSize, la.Y / mapData.SquareSize }, new[] { lb.X / mapData.SquareSize, lb.Y / mapData.SquareSize } },
            });
            map.AddLandmark(new Landmark("the start line", "control", new[] { "start line", "the line of departure" }, "get back past it", (la + lb) * 0.5f,
                new Vec2(Math.Min(la.X, lb.X), Math.Min(la.Y, lb.Y)), new Vec2(Math.Max(la.X, lb.X), Math.Max(la.Y, lb.Y)), new[] { la, lb }));
            bool Back(Vec2 p) => Dot(p - lineAt, toPost) < 0f;

            // The post's ways: sleepers waking, the patrol walking its loop, the trap holding fire, and the reaction force.
            bool sprung = false, qrfCame = false;
            var loop = Enumerable.Range(0, 6).Select(k => Standable(map, Inside(map, post + Turn(from, k * Math.PI / 3) * PatrolOut), Array.Empty<Vec2>())).ToList();
            int leg = 0;
            plan.HoldFire = u => goal == Trap && ambush.Contains(u) && !sprung;
            plan.EverySecond = side =>
            {
                var marines = mission.Squad.Where(u => u.CanAct).ToList();
                var knows = sim.Fog.For(mission.Enemy);
                // Sleepers wake.
                if (side.Any(u => u.Asleep))
                {
                    bool seen = side.Any(u => !u.Asleep && u.CanAct && postMen.Contains(u)) && knows.Contacts.Any(k => k.Visible && ReferenceEquals(k.Unit.Force, mission.Marines));
                    foreach (var u in side.Where(u => u.Asleep))
                        if (seen || u.Suppression > 0f || u.Wounds > 0 || marines.Any(m => Vec2.Distance(m.Position, u.Position) <= WakeWithin))
                        {
                            u.Asleep = false;
                            u.Posture = Posture.Kneeling;
                        }
                }
                // The patrol walks its loop until it meets someone.
                if (patrol.Count > 0 && !patrol.Any(u => u.CanAct && knows.Contacts.Any(k => k.Visible && ReferenceEquals(k.Unit.Force, mission.Marines))))
                {
                    var walking = patrol.Where(u => u.CanAct).ToList();
                    if (walking.Count > 0 && walking.All(u => u.Order == null))
                    {
                        leg = (leg + 1) % loop.Count;
                        TeamOrder(sim, patrol, "patrol", loop[leg], Verbs.Move);
                    }
                }
                // The trap springs once someone's close, at the post, or shooting at them.
                if (goal == Trap && !sprung)
                {
                    bool close = marines.Any(m => Vec2.Distance(m.Position, post) <= 30f || ambush.Any(e => e.CanAct && Vec2.Distance(e.Position, m.Position) <= Simulation.AmbushFound));
                    if (close || ambush.Any(e => e.Wounds > 0 || !e.CanAct || e.Suppression >= 0.3f))
                    {
                        sprung = true;
                        plan.Sprung = true;
                        sim.Shout(ambush.FirstOrDefault(e => e.CanAct), "Now! Fire!", always: true);
                    }
                }
                // The reaction force: it comes in off the far edge, makes for the post, then goes after the Marines.
                if (!qrfCame && sim.Clock.Turn > qrfTurn)
                {
                    qrfCame = true;
                    for (int i = 0; i < qrf.Count; i++) { qrf[i].Gone = false; qrf[i].Position = Standable(sim.Map, qrfFrom + new Vec2((i % 3) * 4f, (i / 3) * 4f), Array.Empty<Vec2>()); }
                    sim.Radio(null, "Raider 6: Drone feed shows " + qrf.Count + " soldiers getting out of trucks at " + sim.Map.SquareOf(qrfFrom) + ". That'll be their reaction force.", "info", qrfFrom, "Raider 6");
                    TeamOrder(sim, qrf, "reaction force", post, Verbs.Move);
                }
                if (qrfCame)
                {
                    var up = qrf.Where(u => u.CanAct).ToList();
                    if (up.Count == 0 || up.Any(u => u.Order != null && u.Order.Verb != Verbs.Suppress)) return;
                    var target = knows.Contacts.Where(k => ReferenceEquals(k.Unit.Force, mission.Marines) && k.Unit.CanAct && !k.Unit.Def.Flying)
                        .OrderBy(k => Vec2.Distance(k.LastKnownPosition, up[0].Position)).FirstOrDefault();
                    Vec2 to = target?.LastKnownPosition ?? post;
                    if (Vec2.Distance(to, up[0].Position) < 20f) return;
                    var guns = up.Where(u => u.Def.Id == "en-pkm").ToList();
                    if (target != null) SuppressKnown(sim, guns, to, mission.Marines);
                    TeamOrder(sim, qrf, "reaction force", to, target != null ? Verbs.Bound : Verbs.Move, null, up.Except(target != null ? guns : new List<Unit>()).ToList());
                }
            };

            mission.Judge = m =>
            {
                if (!radio.Wrecked || !scopes.Wrecked) return null;
                var home = m.Squad.Where(u => u.Condition != Condition.Dead && !u.Evacuated).ToList();
                return home.All(u => Back(u.Position)) ? ("success", "The post's radio and scopes are wrecked, and the squad is back past the start line.") : ((string, string)?)null;
            };
            mission.Status = m => radio.Wrecked && scopes.Wrecked ? "onit" : "notyet";
            mission.OnItWord = "Wrecked; get back";
            mission.DoneWord = "Done";

            mission.Title = "Operation " + Pick(names.Routes, rng) + " Blind";
            string grid = map.SquareOf(post).ToString();
            mission.Brief.Add("Task: raid their observation post on " + hill.Label + " (" + grid + "), wreck its radio and scopes (search the post), and get back past the start line.");
            mission.Brief.Add("Intent: their artillery blind, and a prisoner if you can get one.");
            mission.Brief.Add("Enemy: a few soldiers at the post. Their reaction force will come once they know, sometime between turn " + QrfFirst + " and turn " + QrfLast + ". " + Hints[goal]);
            mission.Roe = StandingRoe;
            mission.Brief.Add("Civilians: " + (village != null ? "in " + village.Label + ". " : "none known nearby. ") + RoeLine(mission.Roe));
            int back = (int)Math.Round(Vec2.Distance(lead, post) / 50f) * 50;
            mission.Brief.Add(SquadLine("at " + map.SquareOf(lead) + ", about " + back + " m from the post, just behind the start line"));
            mission.MainObjective = "Wreck the post's radio and scopes, then get back past the start line.";
            mission.WinText = "The post's radio and scopes wrecked (a Marine searches within " + Simulation.SearchReach + " m of them), and every Marine still alive back past the start line at the end of a turn.";
            mission.LoseText = LoseLine(mission);
            mission.OutOfTimeText = "Out of time. Their artillery can still see.";

            mission.Secondary.Add(new SecondaryObjective("prisoner", "A prisoner taken", m => !m.Sim.Units.Any(u => ReferenceEquals(u.Force, m.Enemy) && u.Detained && !u.Captured)) { AtEnd = true });
            var road = map.Landmarks.Where(l => l.Kind == "line" && l.Label.StartsWith("Route ", StringComparison.Ordinal)).OrderBy(l => DistanceTo(post, l.Points)).FirstOrDefault();
            mission.Director = new Director(mission, data, new Dice(seed, 13), new DirectorSetup
            {
                Focus = post,
                FocusWord = "the post",
                FarSide = p => Dot(p - post, from) < -60f,
                FarWord = "the far side of the hill",
                NearSide = p => Dot(p - post, from) > 60f,
                NearWord = "the squad's side of the hill",
                Road = road?.Points,
                AddIns = new[] { Director.CiviliansRun, Director.Sniper, Director.Ied, Director.PullBack, Director.RadioDown },
            });
            mission.Secondary.Add(EveryMarineHome());
            return mission;
        }

        static readonly Dictionary<string, string> Hints = new Dictionary<string, string>
        {
            { Sleepy, "They've been up there a week without trouble." },
            { Patrol, "Patrols have been seen on the slopes." },
            { Trap, "A shepherd says they've been busy on the slope facing us." },
        };
    }
}
