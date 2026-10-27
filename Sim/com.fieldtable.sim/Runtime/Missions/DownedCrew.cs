using System;
using System.Collections.Generic;
using System.Linq;
using static FieldTable.Sim.MissionKit;

namespace FieldTable.Sim
{
    /// <summary>
    /// The downed helicopter crew (ledger round 16): a helicopter went down, and its two crew are hiding near the wreck,
    /// the pilot hurt. Find them, treat him, and get both out on a CASEVAC from a landing zone the player picks. The
    /// enemy races you to the wreck, waits at it as bait, or searches the farms nearby.
    /// </summary>
    public static class DownedCrew
    {
        public const string Race = "race", Bait = "bait", Search = "search";
        public static readonly IReadOnlyList<string> Goals = new[] { Race, Bait, Search };

        static readonly Dictionary<string, (string said, string truth)> GoalWords = Words(
            (Race, "they're racing you to the wreck", "race you to the wreck"),
            (Bait, "they're waiting for you at the wreck", "wait for you at the wreck"),
            (Search, "they're searching the farms for the crew", "search the farms for the crew"));

        /// <summary>The crew's survival radio picks up a Marine this close, and the crew go with the squad once one is this close.</summary>
        public const float RadioReach = 150f, LinkUp = 10f;

        /// <summary>A fighter this close to a crew member, with no Marine within the second distance, takes him prisoner.</summary>
        public const float CaptureReach = 4f, MarinesStopIt = 30f;

        /// <summary>The pilot is down and bleeding: this many minutes before he dies untreated.</summary>
        public const int PilotMinutes = 30;

        /// <summary>The squad starts about this far from the wreck, and never nearer than the least.</summary>
        const float StartDistance = 400f, StartNearest = 250f;

        /// <summary>Makes the mission. A die picks the enemy's goal unless one is asked for (race, bait, or search).</summary>
        public static Mission Create(GameData data, NamePools names, ulong seed, string goal = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (names == null) throw new ArgumentNullException(nameof(names));
            // Open ground for the wreck, with farms within a few hundred meters.
            Vec2? Site(MapGrid m)
            {
                var d = new Dice(seed, 21);
                for (int i = 0; i < 6; i++)
                {
                    var s = OpenSite(m, d, 10f, 10f, 250f, 100f);
                    if (s != null && m.Landmarks.Any(l => l.Kind == "building" && Vec2.Distance(l.Center, s.Value) is float k && k >= 120f && k <= 400f)) return s;
                }
                return null;
            }
            var (mapData, map) = MakeMap(data, names, seed, "Downed crew " + seed, r => r.Require.Add(MapFeatures.Villages), (_, m) => Site(m) != null);
            Vec2 w = Site(map) ?? OpenSite(map, new Dice(seed, 21), 10f, 10f, 150f, 40f) ?? new Vec2(map.WidthMeters / 2f, map.HeightMeters / 2f);
            var rng = new Dice(seed, 11);

            // The wreck: a scorched scatter of wreckage on open ground.
            var scatter = new List<Vec2>();
            for (int i = 0; i < 10; i++) scatter.Add(w + Turn(new Vec2(0f, -(5f + 4f * Next(rng))), i * Math.PI / 5));
            mapData.Features.Add(AreaFeature(mapData, "rubble", scatter, "the wreck", "wreck", "the crash site", "crash site", "the helicopter", "helicopter"));
            map = mapData.Build(data.Terrains);
            var wreck = map.Landmarks.First(l => l.Label == "the wreck");

            var mission = NewMission(StartStates.Crew, data, mapData, map, seed);
            var sim = mission.Sim;
            mission.Objective = wreck;
            var crewForce = sim.AddForce(new Force { Id = "aircrew", Name = "Dagger 2-1", Faction = "marines" });
            sim.SetHostile(mission.Marines, crewForce, false);

            // The enemy comes from the side away from the squad.
            Vec2 enemyWay = Unit(Turn(new Vec2(0f, -1f), Next(rng) * Math.PI * 2));
            Vec2 TowardEnemy(Vec2 p) => p - w;
            float goalRoll = Next(rng);
            goal ??= Goals[Math.Min(Goals.Count - 1, (int)(goalRoll * Goals.Count))];
            var plan = new EnemyPlan(goal, wreck, Goals, GoalWords);

            // The crew: hiding together in cover 40 to 150 m from the wreck, the pilot down and bleeding slowly.
            var taken = new List<Vec2>();
            Vec2 hide = RingSpot(map, rng, w, 40f, 150f, p => !map.TerrainAt(p).SeeOut, taken,
                (p, t) => t.Concealment * 2f + t.Cover + (Sight.Trace(map, w, 1.7f, p, 0.4f).Clear ? 0f : 1f)) ?? Inside(map, w + enemyWay * -60f);

            var enemy = new List<Unit>();
            var numbered = new Dictionary<string, int>();
            Vec2 far = OutAlong(map, w, enemyWay, 520f);
            switch (goal)
            {
                case Race:
                {
                    // They set out from well back on their side, all at once.
                    int count = Between(rng, 5, 7);
                    for (int i = 0; i < count; i++)
                    {
                        var at = RingSpot(map, rng, far, 0f, 60f, null, taken, (p, t) => t.Cover + t.Concealment) ?? Standable(map, far, taken);
                        Fighter(data, mission.Enemy, enemy, numbered, i == 0 ? "en-pkm" : "en-rf", i == 0 ? "PKM gunner" : "Rifleman", at, Posture.Kneeling, w);
                    }
                    break;
                }
                case Bait:
                {
                    // Lying low around the wreck with eyes on it, holding fire until someone comes to it.
                    int count = Between(rng, 6, 8);
                    for (int i = 0; i < count; i++)
                    {
                        string def = i == 0 ? "en-pkm" : i == 1 ? "en-rpg" : "en-rf";
                        var at = RingSpot(map, rng, w, 80f, 200f, p => Vec2.Distance(p, hide) > 60f, taken, (p, t) =>
                            t.Concealment + t.Cover * 0.5f + (Sight.Trace(map, p, 0.4f, w, 1f).Clear ? 2f : 0f) + (Dot(TowardEnemy(p), enemyWay) > 0f ? 1f : 0f)) ?? Standable(map, w + enemyWay * 120f, taken);
                        Fighter(data, mission.Enemy, enemy, numbered, def, def == "en-pkm" ? "PKM gunner" : def == "en-rpg" ? "RPG gunner" : "Rifleman", at, Posture.Prone, w);
                    }
                    break;
                }
                default:
                {
                    // In pairs among the farms on their side of the wreck.
                    int count = Between(rng, 6, 8);
                    var farms = map.Landmarks.Where(l => l.Kind == "building" && Vec2.Distance(l.Center, w) <= 450f).OrderByDescending(l => Dot(l.Center - w, enemyWay)).ToList();
                    for (int i = 0; i < count; i++)
                    {
                        Vec2 near = farms.Count > 0 ? farms[(i / 2) % farms.Count].Center : far;
                        var at = Standable(map, near + new Vec2(0f, 7f + 3f * (i % 2)), taken);
                        taken.Add(at);
                        Fighter(data, mission.Enemy, enemy, numbered, "en-rf", "Rifleman", at, Posture.Standing, w);
                    }
                    break;
                }
            }
            foreach (var u in enemy) u.Force.Plan = plan;

            var formation = StartOutOfSight(sim, w, p => Dot(TowardEnemy(p), enemyWay) < -100f && !InSettlement(map, p), enemy, StartDistance, StartNearest, Inside(map, w - enemyWay * StartDistance));
            AddSquad(sim, mission.Marines, data, names, rng, formation);
            foreach (var u in enemy) sim.AddUnit(u);

            // The crew's names, from those no Marine has.
            var free = Shuffle(names.People.Where(n => !sim.Units.Any(u => u.Name == n)).DefaultIfEmpty("Hale").ToList(), rng);
            var pilot = new Unit("pilot", "Lt " + free[0], data.FindUnit("aircrew"), crewForce, hide) { Posture = Posture.Prone, Passenger = true };
            pilot.Callsigns.AddRange(new[] { "pilot", "the pilot", "crew", "Dagger" });
            var chief = new Unit("crew-chief", "Sgt " + free[Math.Min(1, free.Count - 1)], data.FindUnit("aircrew"), crewForce, Standable(map, hide + new Vec2(2f, 0f), new[] { hide })) { Posture = Posture.Prone, Passenger = true };
            chief.Callsigns.AddRange(new[] { "crew chief", "the crew chief", "crew", "Dagger" });
            sim.AddUnit(pilot);
            sim.AddUnit(chief);
            pilot.BleedMinutes = PilotMinutes;
            pilot.Condition = Condition.Down;
            pilot.DownSince = 0;
            pilot.LegHit = true;
            pilot.Wounds = 1;
            var crew = new List<Unit> { pilot, chief };

            var village = Settlement(map, w);
            if (village != null) AddCivilians(sim, mission.Civilians, data, rng, BuildingsIn(map, village), Between(rng, 3, 5));

            // The crew: their survival radio, going with the squad once found, and being taken if the enemy gets to them first.
            bool heard = false, linked = false;
            sim.OnSecond += () =>
            {
                var marines = mission.Squad.Where(u => u.CanAct).ToList();
                var talker = crew.FirstOrDefault(u => u.CanAct) ?? crew.FirstOrDefault(u => !u.Gone && u.Condition != Condition.Dead);
                if (!heard && talker != null)
                {
                    var near = marines.FirstOrDefault(m => Vec2.Distance(m.Position, talker.Position) <= RadioReach);
                    if (near != null)
                    {
                        heard = true;
                        foreach (var u in crew.Where(u => !u.Gone)) sim.Fog.Spot(sim, mission.Marines, u, near);
                        string where = sim.Map.TerrainAt(talker.Position).Name.ToLowerInvariant();
                        int off = (int)Math.Round(Vec2.Distance(talker.Position, w) / 10f) * 10;
                        sim.Radio(talker, crewForce.Name + ": Raider, " + crewForce.Name + ", on the survival radio. We see you. We're in the " + where + " at "
                            + sim.Map.SquareOf(talker.Position) + ", about " + off + " m " + Compass(talker.Position - w) + " of the wreck." + (pilot.Condition == Condition.Down ? " " + pilot.Name + " is hit and can't walk." : ""), "casualty", talker.Position, chief.Name);
                    }
                }
                if (!linked && marines.Any(m => crew.Any(u => !u.Gone && Vec2.Distance(m.Position, u.Position) <= LinkUp))) linked = true;
                foreach (var u in crew)
                {
                    if (u.Gone || u.Condition == Condition.Dead) continue;
                    // Taken prisoner: a fighter right on him, and no Marine close enough to stop it.
                    var taker = sim.Units.FirstOrDefault(e => ReferenceEquals(e.Force, mission.Enemy) && e.CanAct && Vec2.Distance(e.Position, u.Position) <= CaptureReach);
                    if (taker != null && !marines.Any(m => Vec2.Distance(m.Position, u.Position) <= MarinesStopIt))
                    {
                        sim.Radio(u, crewForce.Name + ": They've found us! They're taking " + (u == pilot ? "the pilot" : "me") + "!", "casualty", u.Position, u.Name);
                        sim.Capture(u, taker);
                    }
                }
                // Once found, the crew chief keeps with the pilot, or whoever is carrying him, or the nearest Marine.
                if (linked && chief.CanAct && chief.Order == null)
                {
                    Unit with = pilot.CarriedBy ?? (pilot.Gone || pilot.Condition == Condition.Dead ? marines.OrderBy(m => Vec2.Distance(m.Position, chief.Position)).FirstOrDefault() : pilot);
                    if (with != null && Vec2.Distance(with.Position, chief.Position) > 6f) Go(sim, chief, Standable(sim.Map, with.Position + new Vec2(2.5f, 2.5f), new[] { with.Position }), Verbs.Move, MoveSpeed.Run);
                }
            };
            switch (goal)
            {
                case Race: plan.EverySecond = Hunt(mission, crew, enemy, w, true); break;
                case Search: plan.EverySecond = Hunt(mission, crew, enemy, w, false); break;
                default:
                    // The bait holds fire until someone comes to the wreck, gets close to one of them, or shoots.
                    bool sprung = false;
                    plan.HoldFire = u => !sprung;
                    plan.Think = (u, dt) => { if (sprung) return false; u.Posture = Posture.Prone; return true; };
                    plan.EverySecond = side =>
                    {
                        if (sprung) return;
                        bool come = sim.Units.Any(x => ReferenceEquals(x.Force, mission.Marines) && x.CanAct && !x.Def.Flying && (Vec2.Distance(x.Position, w) <= 30f || side.Any(e => e.CanAct && Vec2.Distance(e.Position, x.Position) <= Simulation.AmbushFound)));
                        if (!come && !side.Any(e => e.Wounds > 0 || !e.CanAct || e.Suppression >= 0.3f)) return;
                        sprung = true;
                        plan.Sprung = true;
                        sim.Shout(side.FirstOrDefault(e => e.CanAct), "Now! Fire on them!", always: true);
                    };
                    break;
            }

            mission.Judge = m =>
            {
                var alive = crew.Where(u => u.Condition != Condition.Dead && !u.Captured).ToList();
                if (alive.Count == 0) return ("failure", crew.All(u => u.Captured) ? "The enemy captured the crew." : crew.All(u => u.Condition == Condition.Dead) ? "The crew were killed." : "The crew were lost, one captured and one killed.");
                if (alive.All(u => u.Evacuated)) return ("success", alive.Count == crew.Count ? "Both crew are out on the CASEVAC." : alive[0].Name + " is out on the CASEVAC.");
                return null;
            };
            mission.Status = m => linked ? "onit" : "notyet";
            mission.NotYetWord = "Not found";
            mission.OnItWord = "Found";
            mission.DoneWord = "Out";

            mission.Title = "Operation " + Pick(names.Routes, rng) + " Rope";
            string wreckGrid = map.SquareOf(w).ToString();
            var lead = formation[0].pos;
            mission.Brief.Add("Task: find the crew of " + crewForce.Name + ", a helicopter down at " + wreckGrid + ", and get them out on a CASEVAC.");
            mission.Brief.Add("Intent: both crew home alive.");
            mission.Brief.Add("Crew: two, last heard on their survival radio near the wreck. The pilot is hurt. They speak English and carry carbines.");
            mission.Brief.Add("Enemy: fighters will have seen it come down; strength unknown. " + Hints[goal]);
            mission.Roe = StandingRoe;
            mission.Brief.Add("Civilians: " + (village != null ? "in " + village.Label + ". " : "on the farms nearby. ") + RoeLine(mission.Roe));
            int back = (int)Math.Round(Vec2.Distance(lead, w) / 50f) * 50;
            mission.Brief.Add(SquadLine("at " + map.SquareOf(lead) + ", about " + back + " m from the wreck"));
            mission.MainObjective = "Find the crew and get them out on a CASEVAC.";
            mission.WinText = "Every crew member still alive and free taken out on a CASEVAC. Request one to a landing zone you pick; anyone within " + Simulation.CasevacLoadRadius + " m of it when it lands gets on.";
            mission.LoseText = LoseLine(mission, "both crew captured or killed.");
            mission.OutOfTimeText = "Out of time. The crew are still out there.";

            mission.Secondary.Add(new SecondaryObjective("crew", "Both crew alive", m => crew.Any(u => u.Condition == Condition.Dead || u.Captured)));
            mission.Secondary.Add(new SecondaryObjective("civilians", "No civilians hurt", m => m.AnyoneHurt(m.Civilians)));
            var road = map.Landmarks.Where(l => l.Kind == "line" && l.Label.StartsWith("Route ", StringComparison.Ordinal)).OrderBy(l => DistanceTo(w, l.Points)).FirstOrDefault();
            mission.Director = new Director(mission, data, new Dice(seed, 13), new DirectorSetup
            {
                Focus = w,
                FocusWord = "the wreck",
                FarSide = p => Dot(TowardEnemy(p), enemyWay) > 60f,
                FarWord = "the far side of the wreck",
                NearSide = p => Dot(TowardEnemy(p), enemyWay) < -60f,
                NearWord = "the squad's side of the wreck",
                Road = road?.Points,
                Slow = () => !linked,
            });
            mission.Secondary.Add(EveryMarineHome());
            return mission;
        }

        static readonly Dictionary<string, string> Hints = new Dictionary<string, string>
        {
            { Race, "A drone saw a truck heading for the crash." },
            { Bait, "The crash site has gone quiet." },
            { Search, "Farmers say armed men are going door to door." },
        };

        /// <summary>
        /// Race and search: they look for the crew, at the wreck and the cover around it (race) or farm by farm (search),
        /// in pairs. Anyone who sees a crew member goes for him with the nearest friend. Once they see Marines, they stop
        /// searching and fight.
        /// </summary>
        static Action<List<Unit>> Hunt(Mission mission, List<Unit> crew, List<Unit> enemy, Vec2 wreck, bool race)
        {
            var sim = mission.Sim;
            var map = sim.Map;
            var rng = new Dice(sim.Seed, 23);
            // The places to look: the wreck first, then cover around it; or the farms, nearest the wreck first.
            var places = new List<Vec2>();
            if (race)
            {
                places.Add(wreck);
                var spots = new List<Vec2>();
                for (int i = 0; i < 6; i++)
                    if (RingSpot(map, rng, wreck, 30f, 150f, p => !map.TerrainAt(p).SeeOut, spots, (p, t) => t.Concealment + t.Cover) is Vec2 s) places.Add(s);
            }
            else places.AddRange(map.Landmarks.Where(l => l.Kind == "building" && Vec2.Distance(l.Center, wreck) <= 450f).OrderBy(l => Vec2.Distance(l.Center, wreck)).Select(l => l.Center));
            if (places.Count == 0) places.Add(wreck);
            var next = new Dictionary<Unit, int>();
            for (int i = 0; i < enemy.Count; i++) next[enemy[i]] = (i / 2) % places.Count;
            bool fighting = false;
            return side =>
            {
                var knows = sim.Fog.For(side[0].Force);
                if (!fighting && knows.Contacts.Any(k => k.Visible && ReferenceEquals(k.Unit.Force, mission.Marines) && !k.Unit.Def.Flying))
                {
                    fighting = true;
                    foreach (var u in side.Where(u => u.CanAct && u.Order != null && u.Order.Verb == Verbs.Move)) { u.Order = null; u.Path = null; }
                }
                // Anyone who can see a crew member goes for him, with his partner.
                var seen = crew.FirstOrDefault(c => !c.Gone && c.Condition != Condition.Dead && knows.About(c.Id)?.Visible == true);
                if (seen != null && !mission.Squad.Any(m => m.CanAct && Vec2.Distance(m.Position, seen.Position) <= MarinesStopIt * 2f))
                {
                    foreach (var u in side.Where(u => u.CanAct && !u.Def.Id.Equals("en-pkm")).OrderBy(u => Vec2.Distance(u.Position, seen.Position)).Take(2))
                        if (u.Order == null || u.Order.Point == null || Vec2.Distance(u.Order.Point.Value, seen.Position) > 3f) Go(sim, u, seen.Position, Verbs.Move, MoveSpeed.Run);
                    return;
                }
                if (fighting) return;
                // Otherwise, on to the next place to look once they've had a look around this one.
                foreach (var u in side.Where(u => u.CanAct && u.Order == null))
                {
                    if (!next.TryGetValue(u, out int k)) continue;
                    if (Vec2.Distance(u.Position, places[k]) <= 15f)
                    {
                        if (u.TaskSeconds < 30f) { u.TaskSeconds += 1f; continue; }
                        u.TaskSeconds = 0f;
                        k = next[u] = (k + 1) % places.Count;
                    }
                    Go(sim, u, Standable(map, places[k] + new Vec2(0f, 7f), Array.Empty<Vec2>()));
                }
            };
        }
    }
}
