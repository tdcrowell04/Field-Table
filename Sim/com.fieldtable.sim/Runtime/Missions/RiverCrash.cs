using System;
using System.Collections.Generic;
using System.Linq;
using static FieldTable.Sim.MissionKit;

namespace FieldTable.Sim
{
    /// <summary>
    /// Crash in the river (ledger round 19, for Pararescue): a helicopter went down in a river. Two of its crew are on a
    /// sandbar, two are in the water, and two of the four are hurt; the current carries the ones in the water downstream
    /// until someone reaches them. Get to them (wade or swim; drop the heavy gear first), treat them, and get them out
    /// before the enemy who heard the crash arrives, on a turn a die picks. The enemy comes straight for the crash,
    /// searches the banks first, or watches the crash from cover and waits for the rescuers.
    /// </summary>
    public static class RiverCrash
    {
        public const string Straight = "straight", Banks = "banks", Watch = "watch";
        public static readonly IReadOnlyList<string> Goals = new[] { Straight, Banks, Watch };

        static readonly Dictionary<string, (string said, string truth)> GoalWords = Words(
            (Straight, "they're coming straight for the crash", "come straight for the crash"),
            (Banks, "they're searching the riverbanks", "search the riverbanks first"),
            (Watch, "they're waiting at the river for whoever comes", "watch the crash and wait for the rescuers"));

        /// <summary>The enemy arrives at the end of a turn between these, picked by a die and never told.</summary>
        public const int ArriveFirst = 6, ArriveLast = 12;

        /// <summary>Minutes in the water before the hurt one and the other go under, unless someone reaches them first.</summary>
        public const int HurtInWater = 10, WellInWater = 20;

        /// <summary>How fast the current carries someone in the water, in meters a second.</summary>
        public const float Current = 0.3f;

        /// <summary>Within this, a rescuer has hold of someone in the water, or has reached the sandbar.</summary>
        public const float Reach = 3f, OnTheBar = 12f;

        /// <summary>A soldier this close to a crew member on dry ground, with no rescuer within the second, takes him.</summary>
        public const float CaptureReach = 4f, RescuersStopIt = 30f;

        const float StartDistance = 300f, StartNearest = 200f;

        public static Mission Create(GameData data, NamePools names, ulong seed, string goal = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (names == null) throw new ArgumentNullException(nameof(names));
            // A stretch of river well in from the edges, with a few hundred meters downstream still on the map.
            (Landmark river, int at)? Site(MapGrid m)
            {
                var river = m.Landmarks.Where(l => l.Kind == "line" && l.Label.EndsWith(" River", StringComparison.Ordinal)).FirstOrDefault();
                if (river == null || river.Points.Count < 4) return null;
                // Well away from any bridge, or the crew could walk out.
                var bridges = m.Landmarks.Where(l => l.Kind == "line" && l.Label.EndsWith(" bridge", StringComparison.Ordinal)).Select(l => l.Center).ToList();
                for (int i = 1; i < river.Points.Count - 2; i++)
                {
                    var p = river.Points[i];
                    if (p.X > 200f && p.Y > 200f && p.X < m.WidthMeters - 200f && p.Y < m.HeightMeters - 200f && m.TerrainAt(p).Deep
                        && bridges.All(b => Vec2.Distance(b, p) >= 150f)) return (river, i);
                }
                return null;
            }
            var (mapData, map) = MakeMap(data, names, seed, "River crash " + seed, r => r.Require.Add(MapFeatures.Rivers), (_, m) => Site(m) != null);
            var site = Site(map) ?? throw new InvalidOperationException("No river for the crash.");
            var river = site.river;
            Vec2 crash = river.Points[site.at];
            Vec2 down = Unit(river.Points[site.at + 1] - crash);
            Vec2 across = new Vec2(-down.Y, down.X);
            // The sandbar: a little gravel and scrub in the middle of the river, with the wreck on its downstream end.
            var bar = new List<Vec2>();
            for (int i = 0; i < 10; i++) bar.Add(crash + Turn(new Vec2(0f, -1f), i * Math.PI / 5) * 1f + down * (float)(Math.Cos(i * Math.PI / 5) * 7.0) + across * (float)(Math.Sin(i * Math.PI / 5) * 3.5));
            mapData.Features.Add(AreaFeature(mapData, "scrub", bar, "the sandbar", "sandbar", "the gravel bar", "the island"));
            var wreckPts = new List<Vec2>();
            for (int i = 0; i < 8; i++) wreckPts.Add(crash + down * 5f + Turn(new Vec2(0f, -2.5f), i * Math.PI / 4));
            mapData.Features.Add(AreaFeature(mapData, "rubble", wreckPts, "the wreck", "wreck", "the helicopter", "helicopter", "the crash"));
            map = mapData.Build(data.Terrains);
            var sandbar = map.Landmarks.First(l => l.Label == "the sandbar");
            var rng = new Dice(seed, 11);

            var mission = NewMission(StartStates.River, data, mapData, map, seed);
            var sim = mission.Sim;
            mission.Objective = sandbar;
            var crewForce = sim.AddForce(new Force { Id = "aircrew", Name = "Saber 1-1" });
            sim.SetHostile(mission.Marines, crewForce, false);
            float goalRoll = Next(rng);
            goal ??= Goals[Math.Min(Goals.Count - 1, (int)(goalRoll * Goals.Count))];
            var plan = new EnemyPlan(goal, sandbar, Goals, GoalWords);

            // The crew: two on the sandbar (the pilot can't walk), two in the water just downstream (the gunner's hurt).
            var free = Shuffle(names.People.ToList(), rng);
            int n = 0;
            string Name(string rank) => rank + " " + (n < free.Count ? free[n++] : "Airman " + (++n));
            Unit Add(string id, string rank, Vec2 at, string[] calls)
            {
                var u = new Unit(id, Name(rank), data.FindUnit("aircrew"), crewForce, at) { Posture = Posture.Prone, Passenger = true };
                u.Callsigns.AddRange(calls.Concat(new[] { "crew", "Saber" }));
                sim.AddUnit(u);
                return u;
            }
            var pilot = Add("pilot", "Capt", Standable(map, crash, Array.Empty<Vec2>()), new[] { "pilot", "the pilot" });
            var copilot = Add("copilot", "Lt", Standable(map, crash - down * 3f, new[] { pilot.Position }), new[] { "copilot", "the copilot" });
            Vec2 InTheWater(float downstream, float side)
            {
                for (float d = downstream; d < downstream + 40f; d += 2f)
                {
                    var p = Inside(map, crash + down * d + across * side);
                    if (map.TerrainAt(p).Deep) return p;
                }
                return Inside(map, crash + down * (downstream + 12f));
            }
            var chief = Add("crew-chief", "SSgt", InTheWater(14f, 2f), new[] { "crew chief", "the crew chief" });
            var gunner = Add("gunner", "SrA", InTheWater(20f, -2f), new[] { "gunner", "the gunner" });
            pilot.Condition = Condition.Wounded; pilot.LegHit = true; pilot.Wounds = 1;
            gunner.Condition = Condition.Wounded; gunner.ArmHit = true; gunner.Wounds = 1;
            var crew = new List<Unit> { pilot, copilot, chief, gunner };
            var inWater = new Dictionary<Unit, float> { { chief, WellInWater * 60f }, { gunner, HurtInWater * 60f } };
            var held = new HashSet<Unit>();

            // The radio and codes, in the wreck: searching it pulls them out.
            var codes = sim.PlaceGear("the radio and codes", crash + down * 5f);

            // The enemy: they come in from one side of the river, on a turn nobody's told.
            int side = Next(rng) < 0.5f ? 1 : -1;
            Vec2 theirWay = across * side;
            int arrive = ArrivalTurn(mission, rng, ArriveFirst, ArriveLast);
            Vec2 from = EdgeToward(map, crash, theirWay);
            var enemy = new List<Unit>();
            var numbered = new Dictionary<string, int>();
            int count = Between(rng, 6, 8);
            var taken = new List<Vec2>();
            for (int i = 0; i < count; i++)
            {
                string def = i == 0 ? "en-pkm" : i == 1 ? "en-rpg" : "en-rf";
                var at = Standable(map, Inside(map, from + down * ((i % 3) * 5f - 5f) - theirWay * (i / 3 * 5f)), taken);
                taken.Add(at);
                var u = Fighter(data, mission.Enemy, enemy, numbered, def, def == "en-pkm" ? "PKM gunner" : def == "en-rpg" ? "RPG gunner" : "Rifleman", at, Posture.Standing, crash);
                u.Gone = true;
            }
            Organize(data, enemy);
            foreach (var u in enemy) { u.Force.Plan = plan; sim.AddUnit(u); }

            // The near bank, opposite the crash: where the crew make for once someone has them.
            Vec2 nearBank = Inside(map, crash - theirWay * 30f);
            for (float d = 6f; d < 80f; d += 3f) { var p = Inside(map, crash - theirWay * d); if (!map.TerrainAt(p).Deep) { nearBank = Inside(map, p - theirWay * 6f); break; } }

            // The rescuers start on that bank, out of sight of where the enemy will come from. (They head for the bank
            // opposite the crash: there's no walking route to the sandbar itself.)
            var formation = StartOutOfSight(sim, nearBank, p => Dot(p - crash, theirWay) < -80f && !map.TerrainAt(p).Deep, new List<Unit>(), StartDistance, StartNearest, Inside(map, crash - theirWay * StartDistance));
            AddSquad(sim, mission.Marines, data, names, rng, formation);

            // The water: the current carries anyone nobody has hold of; too long in it and they go under. Once a rescuer
            // has them, or reaches the sandbar, the crew make for the near bank with them.
            bool reached = false;
            // A crew member whose way couldn't be found waits a little before trying again, so turns stay quick.
            var retryAt = new Dictionary<Unit, int>();
            bool Swim(Unit u, Vec2 to)
            {
                if (retryAt.TryGetValue(u, out int at) && sim.Clock.Step < at) return false;
                bool ok = sim.Give(u, new Order { Verb = Verbs.Move, Where = sim.Coordinates(to), Swim = true }).Ok;
                if (!ok) retryAt[u] = sim.Clock.Step + 10 * SimClock.StepsPerSecond;
                return ok;
            }
            sim.OnSecond += () =>
            {
                var ours = mission.Squad.Where(u => u.CanAct).ToList();
                foreach (var u in inWater.Keys.ToList())
                {
                    if (u.Gone || u.Condition == Condition.Dead) { inWater.Remove(u); continue; }
                    if (!sim.Map.TerrainAt(u.Position).Deep) { inWater.Remove(u); continue; }
                    if (!held.Contains(u) && ours.Any(m => Vec2.Distance(m.Position, u.Position) <= Reach))
                    {
                        held.Add(u);
                        reached = true;
                        sim.Radio(u, crewForce.Name + ": You've got me! Get me to the bank!", "casualty", u.Position, u.Name);
                    }
                    if (held.Contains(u))
                    {
                        if (u.Order == null) Swim(u, nearBank);
                        continue;
                    }
                    inWater[u] -= 1f;
                    if (inWater[u] <= 0f) { sim.GoUnder(u); sim.Radio(null, mission.Side.Higher + ": We've lost " + u.Name + " in the water.", "casualty", u.Position, mission.Side.Higher); continue; }
                    // Downstream along the river, staying in the water; off the map, they're gone.
                    var next = u.Position + down * Current;
                    if (!sim.Map.InBounds(next) || next.X < 2f || next.Y < 2f || next.X > sim.Map.WidthMeters - 2f || next.Y > sim.Map.HeightMeters - 2f) { sim.GoUnder(u); continue; }
                    if (sim.Map.TerrainAt(next).Deep) u.Position = next;
                }
                // On the sandbar: once a rescuer is there, the copilot swims for the near bank; the pilot needs carrying.
                if (ours.Any(m => Vec2.Distance(m.Position, crash) <= OnTheBar))
                {
                    reached = true;
                    if (copilot.CanAct && copilot.Order == null && Vec2.Distance(copilot.Position, nearBank) > 8f && !copilot.Gone)
                        Swim(copilot, nearBank);
                }
                // Ashore on the near bank, the crew who can walk keep with the rescuers there, the way the downed crew do.
                if (reached)
                    foreach (var u in crew.Where(u => u.CanAct && !u.Gone && u.Order == null && !u.LegHit && !sim.Map.TerrainAt(u.Position).Deep && Dot(u.Position - crash, theirWay) < 0f))
                    {
                        var with = ours.Where(m => !sim.Map.TerrainAt(m.Position).Deep).OrderBy(m => Vec2.Distance(m.Position, u.Position)).FirstOrDefault();
                        if (with != null && Vec2.Distance(with.Position, u.Position) > 8f) Swim(u, Standable(sim.Map, with.Position + new Vec2(2.5f, -2.5f), new[] { with.Position }));
                    }
                // Taken: a soldier right on a crew member out of the water, with no rescuer close enough to stop it.
                foreach (var u in crew)
                {
                    if (u.Gone || u.Condition == Condition.Dead || sim.Map.TerrainAt(u.Position).Deep) continue;
                    var taker = sim.Units.FirstOrDefault(e => ReferenceEquals(e.Force, mission.Enemy) && e.CanAct && Vec2.Distance(e.Position, u.Position) <= CaptureReach);
                    if (taker != null && !ours.Any(m => Vec2.Distance(m.Position, u.Position) <= RescuersStopIt)) sim.Capture(u, taker);
                }
            };

            // The enemy: they arrive, then come straight on, search the banks, or set up to watch and wait.
            bool came = false, sprung = goal != Watch;
            Attack attack = null;
            var bankStops = new List<Vec2>();
            for (int k = -2; k <= 2; k++) bankStops.Add(Inside(map, crash + down * (k * 80f) + theirWay * 35f));
            int stop = 0;
            plan.HoldFire = u => !sprung;
            plan.EverySecond = their =>
            {
                if (!came)
                {
                    if (sim.Clock.Turn <= arrive) return;
                    came = true;
                    foreach (var u in enemy) u.Gone = false;
                    sim.Radio(null, mission.Side.Higher + ": Drone feed shows trucks stopping at " + sim.Map.SquareOf(from) + ". They heard the crash.", "info", from, mission.Side.Higher);
                    if (goal == Watch)
                    {
                        // Into cover on their bank with a view of the crash, and wait.
                        for (int i = 0; i < enemy.Count; i++)
                        {
                            var spot = RingSpot(sim.Map, rng, crash + theirWay * 90f, 0f, 60f, p => !sim.Map.TerrainAt(p).Deep, taken, (p, t) => t.Cover + t.Concealment + (Sight.Trace(sim.Map, p, 0.4f, crash, 1f).Clear ? 2f : 0f)) ?? Inside(sim.Map, crash + theirWay * 90f);
                            taken.Add(spot);
                            Go(sim, enemy[i], spot, Verbs.Move, MoveSpeed.Run);
                        }
                    }
                    return;
                }
                var up = enemy.Where(u => u.CanAct).ToList();
                if (up.Count == 0) return;
                var knows = sim.Fog.For(mission.Enemy);
                var target = knows.Contacts.Where(k => k.Visible && ReferenceEquals(k.Unit.Force, mission.Marines) && k.Unit.CanAct && !k.Unit.Def.Flying)
                    .OrderBy(k => Vec2.Distance(k.LastKnownPosition, up[0].Position)).FirstOrDefault();
                if (goal == Watch && !sprung)
                {
                    // They open up once rescuers are at the crash, close to them, or shooting at them.
                    bool atCrash = mission.Squad.Any(m => m.CanAct && Vec2.Distance(m.Position, crash) <= 40f);
                    bool close = mission.Squad.Any(m => m.CanAct && up.Any(e => Vec2.Distance(e.Position, m.Position) <= Simulation.AmbushFound));
                    if (atCrash || close || up.Any(e => e.Wounds > 0 || e.Suppression >= 0.3f))
                    {
                        sprung = true;
                        plan.Sprung = true;
                        sim.Shout(up[0], "Now! Fire on them!", always: true);
                    }
                    else { foreach (var u in up.Where(u => u.Order == null)) u.Posture = Posture.Prone; return; }
                }
                if (target != null)
                {
                    if (attack == null) attack = new Attack(sim, enemy, "river", target.LastKnownPosition, mission.Marines);
                    else attack.Retarget(target.LastKnownPosition);
                    attack.Tick();
                    return;
                }
                if (goal == Watch || up.Any(u => u.Order != null && u.Order.Verb != Verbs.Suppress)) return;
                // Nobody seen: straight to the crash, or along the bank first.
                Vec2 to = goal == Banks && stop < bankStops.Count ? bankStops[stop++] : Inside(sim.Map, crash + theirWay * 25f);
                TeamOrder(sim, enemy, "river", to, Verbs.Move);
            };

            mission.TurnLimit = Mission.DefaultTurnLimit;
            mission.Judge = m =>
            {
                var lost = crew.FirstOrDefault(u => u.Condition == Condition.Dead || u.Captured);
                if (lost != null) return ("failure", lost.Captured ? lost.Name + " was captured." : lost.Name + " drowned.");
                if (crew.All(u => u.Evacuated)) return ("success", "All four crew are out on the helicopters.");
                return null;
            };
            mission.Status = m => reached ? "onit" : "notyet";
            mission.NotYetWord = "Not reached";
            mission.OnItWord = "Reached";
            mission.DoneWord = "Out";

            mission.Title = "Operation " + Pick(names.Routes, rng) + " Current";
            mission.Brief.Add("Task: " + crewForce.Name + " went down in " + river.Label + " at " + map.SquareOf(crash) + ". Two crew are on a sandbar, the pilot unable to walk; two are in the water downstream, and the gunner is hurt. Get all four out on a CASEVAC.");
            mission.Brief.Add("Intent: everyone out of the water before the current takes them. The hurt one in the water has about " + HurtInWater + " minutes, the other about " + WellInWater + ".");
            mission.Brief.Add("Water: deep. Drop your heavy gear before you swim, and order a swim to cross. Carry the pilot across with a swim order too.");
            mission.Brief.Add("Enemy: they'll have heard the crash; trucks are expected sometime between turn " + ArriveFirst + " and " + ArriveLast + ". " + Hints[goal]);
            mission.Roe = StandingRoe;
            mission.Brief.Add(RoeLine(mission.Roe));
            mission.Brief.Add(SquadLine(mission, "on the near bank, about " + (int)(Math.Round(Vec2.Distance(formation[0].pos, crash) / 50f) * 50) + " m from the crash") + " Your helicopters wait nearby: request a CASEVAC to a landing zone you pick.");
            mission.MainObjective = "Get all four crew out of the river and onto the helicopters.";
            mission.WinText = "All four crew taken out on a CASEVAC from a landing zone you pick.";
            mission.LoseText = LoseLine(mission, "any of the crew drowns or is captured.");
            mission.OutOfTimeText = "Out of time. The crew are still out there.";

            mission.Secondary.Add(new SecondaryObjective("codes", "The radio and codes pulled out of the wreck (search it)", m => !codes.Wrecked) { AtEnd = true });
            mission.Director = new Director(mission, data, new Dice(seed, 13), new DirectorSetup
            {
                Focus = crash,
                FocusWord = "the crash",
                FarSide = p => Dot(p - crash, theirWay) > 60f,
                FarWord = "the far bank",
                NearSide = p => Dot(p - crash, theirWay) < -60f,
                NearWord = "the near bank",
                AddIns = new[] { Director.Sniper, Director.RadioDown },
                Slow = () => !reached,
            });
            mission.Secondary.Add(EveryoneHome(mission));
            return mission;
        }

        static readonly Dictionary<string, string> Hints = new Dictionary<string, string>
        {
            { Straight, "Locals say a truck left the village fast when it came down." },
            { Banks, "A fisherman says soldiers walk the riverbanks most days." },
            { Watch, "The crash site has gone quiet on the drone feed." },
        };
    }
}
