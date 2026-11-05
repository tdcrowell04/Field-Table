using System;
using System.Collections.Generic;
using System.Linq;
using static FieldTable.Sim.MissionKit;

namespace FieldTable.Sim
{
    /// <summary>
    /// The collapsed building (ledger round 19, for Pararescue): a strike brought down a house in a village with people
    /// inside, a family and one of ours who was with them. Find where they're trapped (search the rubble), dig them out,
    /// treat them, and get everyone out while a Kansian patrol works through the village. The patrol is passing
    /// through, searching house to house, or set up on the far side waiting for whoever comes.
    /// </summary>
    public static class CollapsedBuilding
    {
        public const string Passing = "passing", Searching = "searching", Waiting = "waiting";
        public static readonly IReadOnlyList<string> Goals = new[] { Passing, Searching, Waiting };

        static readonly Dictionary<string, (string said, string truth)> GoalWords = Words(
            (Passing, "they're only passing through", "pass through the village"),
            (Searching, "they're searching the houses", "search the village house to house"),
            (Waiting, "they're waiting for whoever comes to the rubble", "wait on the far side for the rescuers"));

        /// <summary>A rescuer searching within this of someone trapped hears them.</summary>
        public const float HearWithin = 10f;

        /// <summary>A rescuer this close, standing still, digs; it takes this long to get someone out.</summary>
        public const float DigWithin = 3f;
        public const int DigSeconds = 60;

        const float StartDistance = 300f, StartNearest = 200f;

        public static Mission Create(GameData data, NamePools names, ulong seed, string goal = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (names == null) throw new ArgumentNullException(nameof(names));
            // A house on the edge of a village of 5 or more, well in from the map's edges.
            (Landmark village, Landmark house)? Site(MapGrid m)
            {
                foreach (var v in m.Landmarks.Where(l => l.Kind == "area" && (l.Note == "village" || l.Note == "town")).OrderByDescending(l => BuildingsIn(m, l).Count()))
                {
                    var houses = BuildingsIn(m, v).ToList();
                    if (houses.Count < 5) continue;
                    var house = houses.Where(h => h.Center.X > 150f && h.Center.Y > 150f && h.Center.X < m.WidthMeters - 150f && h.Center.Y < m.HeightMeters - 150f)
                        .OrderByDescending(h => (h.Max.X - h.Min.X) * (h.Max.Y - h.Min.Y)).FirstOrDefault();
                    if (house != null) return (v, house);
                }
                return null;
            }
            var (mapData, map) = MakeMap(data, names, seed, "Collapsed building " + seed, r => r.Require.Add(MapFeatures.Villages), (_, m) => Site(m) != null);
            var site = Site(map) ?? throw new InvalidOperationException("No village with room for the collapse.");
            var house0 = site.house;
            // The house comes down: its footprint, and a little more, is rubble.
            mapData.Features.Add(RectFeature(mapData, "rubble", house0.Min - new Vec2(2f, 2f), house0.Max + new Vec2(2f, 2f), "the collapsed building", "rubble", "the rubble", "the collapse", "the fallen house", house0.Label));
            map = mapData.Build(data.Terrains);
            var village = map.Landmarks.First(l => l.Label == site.village.Label);
            var rubble = map.Landmarks.First(l => l.Label == "the collapsed building");
            Vec2 c = rubble.Center;
            var rng = new Dice(seed, 11);

            var mission = NewMission(StartStates.Rubble, data, mapData, map, seed);
            var sim = mission.Sim;
            mission.Objective = rubble;
            var ally = sim.AddForce(new Force { Id = "liaison", Name = "Atlas 4" });
            sim.SetHostile(mission.Marines, ally, false);
            float goalRoll = Next(rng);
            goal ??= Goals[Math.Min(Goals.Count - 1, (int)(goalRoll * Goals.Count))];
            var plan = new EnemyPlan(goal, rubble, Goals, GoalWords);

            // Under the rubble: a family of three and our liaison, each somewhere in the pile, off the table until dug out.
            var free = Shuffle(names.People.ToList(), rng);
            Vec2 Spot()
            {
                float w = Math.Max(2f, rubble.Max.X - rubble.Min.X - 4f), h = Math.Max(2f, rubble.Max.Y - rubble.Min.Y - 4f);
                return new Vec2(rubble.Min.X + 2f + w * (float)Next(rng), rubble.Min.Y + 2f + h * (float)Next(rng));
            }
            var trapped = new List<Unit>();
            Unit Trap(Unit u, Condition condition, bool leg)
            {
                u.Gone = true; u.Trapped = true; u.Passenger = true; u.Posture = Posture.Prone;
                u.Condition = condition; u.LegHit = leg; u.Wounds = condition == Condition.Ok && !leg ? 0 : 1;
                if (condition == Condition.Down) u.BleedMinutes = 30;
                sim.AddUnit(u);
                trapped.Add(u);
                return u;
            }
            var father = Trap(new Unit("family-1", "Civilian 1", data.FindUnit("civ"), mission.Civilians, Spot()), Condition.Down, false);
            var mother = Trap(new Unit("family-2", "Civilian 2", data.FindUnit("civ"), mission.Civilians, Spot()), Condition.Wounded, true);
            var child = Trap(new Unit("family-3", "Civilian 3", data.FindUnit("civ"), mission.Civilians, Spot()), Condition.Ok, false);
            var ours0 = Trap(new Unit("liaison", "Sgt " + (free.Count > 0 ? free[0] : "Hale"), data.FindUnit("a-rf"), ally, Spot()), Condition.Down, false);
            ours0.Callsigns.AddRange(new[] { "Atlas", "liaison", "the liaison" });
            var family = new List<Unit> { father, mother, child };

            // The rest of the village, in their houses.
            AddCivilians(sim, mission.Civilians, data, rng, BuildingsIn(map, village).Where(b => Vec2.Distance(b.Center, c) > 15f), Between(rng, 3, 5), 4);

            // The patrol: 4 to 6, coming in from the far side.
            Vec2 theirWay = Unit(Turn(new Vec2(0f, -1f), Next(rng) * Math.PI * 2));
            var enemy = new List<Unit>();
            var numbered = new Dictionary<string, int>();
            int count = Between(rng, 4, 6);
            Vec2 from = OutAlong(map, c, theirWay, 350f);
            var taken = new List<Vec2>();
            for (int i = 0; i < count; i++)
            {
                string def = i == 0 ? "en-pkm" : "en-rf";
                var at = goal == Waiting
                    ? RingSpot(map, rng, c + theirWay * 110f, 0f, 60f, p => !InSettlement(map, p, 0f) || map.TerrainAt(p).Cover > 0.3f, taken, (p, t) => t.Cover + t.Concealment + (Sight.Trace(map, p, 0.4f, c, 1f).Clear ? 2f : 0f)) ?? Standable(map, Inside2(map, c + theirWay * 110f), taken)
                    : Standable(map, Inside2(map, from + new Vec2((i % 3) * 4f, (i / 3) * 4f)), taken);
                taken.Add(at);
                var u = Fighter(data, mission.Enemy, enemy, numbered, def, def == "en-pkm" ? "PKM gunner" : "Rifleman", at, goal == Waiting ? Posture.Prone : Posture.Standing, c);
                if (goal != Waiting) u.Gone = true;
            }
            Organize(data, enemy);
            foreach (var u in enemy) { u.Force.Plan = plan; sim.AddUnit(u); }

            var formation = StartOutOfSight(sim, c, p => Dot(p - c, theirWay) < -100f && !InSettlement(map, p), goal == Waiting ? enemy : new List<Unit>(), StartDistance, StartNearest, Inside2(map, c - theirWay * StartDistance));
            AddSquad(sim, mission.Marines, data, names, rng, formation);

            // Finding and digging: a rescuer searching near someone trapped hears them; standing beside them, digs them out.
            var found = new HashSet<Unit>();
            var dug = new Dictionary<Unit, float>();
            var outOrder = new List<Unit>();
            sim.OnSecond += () =>
            {
                var ours = mission.Squad.Where(u => u.CanAct).ToList();
                foreach (var t in trapped.Where(t => t.Trapped))
                {
                    if (!found.Contains(t))
                    {
                        var hearer = ours.FirstOrDefault(m => Vec2.Distance(m.Position, t.Position) <= HearWithin && (m.Order?.Verb == Verbs.Search || Vec2.Distance(m.Position, t.Position) <= DigWithin));
                        if (hearer == null) continue;
                        found.Add(t);
                        string who = t == ours0 ? "one of ours" : "someone";
                        sim.Radio(hearer, (string.IsNullOrEmpty(hearer.Team) ? hearer.Name : hearer.Team) + ": " + (who == "one of ours" ? "We can hear one of ours" : "We can hear someone") + " under the rubble, " + sim.Coordinates(t.Position) + ". Digging.", "casualty", t.Position);
                    }
                    var digger = ours.FirstOrDefault(m => Vec2.Distance(m.Position, t.Position) <= DigWithin && !m.IsMoving);
                    if (digger == null) continue;
                    dug[t] = (dug.TryGetValue(t, out float s) ? s : 0f) + 1f;
                    if (dug[t] < DigSeconds) continue;
                    // Out: on the table now, hurt as they were. Anyone down starts bleeding from here.
                    t.Trapped = false;
                    t.Gone = false;
                    t.Position = Standable(sim.Map, t.Position, sim.Units.Where(x => x != t && !x.Gone).Select(x => x.Position));
                    if (t.Condition == Condition.Down) t.DownSince = sim.Clock.Step;
                    outOrder.Add(t);
                    sim.Fog.Spot(sim, mission.Marines, t, digger);
                    sim.Radio(digger, (string.IsNullOrEmpty(digger.Team) ? digger.Name : digger.Team) + ": We've got " + (t == ours0 ? t.Name : "a villager") + " out." + (t.Condition == Condition.Down ? " Badly hurt." : t.LegHit ? " Can't walk." : ""), "casualty", t.Position);
                }
                // Those out who can walk keep with the rescuers.
                foreach (var t in trapped.Where(t => !t.Trapped && t.CanAct && !t.Gone && t.Order == null && !t.LegHit))
                {
                    var with = ours.OrderBy(m => Vec2.Distance(m.Position, t.Position)).FirstOrDefault();
                    if (with != null && Vec2.Distance(with.Position, t.Position) > 6f) Go(sim, t, Standable(sim.Map, with.Position + new Vec2(-2.5f, 2.5f), new[] { with.Position }), Verbs.Move, MoveSpeed.Walk);
                }
            };

            // The patrol: through the village, house to house, or waiting with a view of the rubble.
            var houses = BuildingsIn(map, village).OrderBy(b => Vec2.Distance(b.Center, from)).Select(b => b.Center).ToList();
            var route = goal == Passing ? new List<Vec2> { c + theirWay * 40f, c - theirWay * 60f, Inside2(map, c - theirWay * 320f) } : houses;
            int come = Between(rng, 2, 4), stop = 0;
            bool came = goal == Waiting, sprung = goal != Waiting;
            Attack attack = null;
            plan.HoldFire = u => !sprung;
            plan.EverySecond = their =>
            {
                if (!came)
                {
                    if (sim.Clock.Turn < come) return;
                    came = true;
                    foreach (var u in enemy) u.Gone = false;
                }
                var up = enemy.Where(u => u.CanAct).ToList();
                if (up.Count == 0) return;
                if (!sprung)
                {
                    // Waiting: they open up once rescuers are at the rubble a while, close to them, or shooting at them.
                    bool atRubble = mission.Squad.Any(m => m.CanAct && Vec2.Distance(m.Position, c) <= 15f && !m.IsMoving);
                    bool close = mission.Squad.Any(m => m.CanAct && up.Any(e => Vec2.Distance(e.Position, m.Position) <= Simulation.AmbushFound));
                    if (atRubble || close || up.Any(e => e.Wounds > 0 || e.Suppression >= 0.3f))
                    {
                        sprung = true;
                        plan.Sprung = true;
                        sim.Shout(up[0], "Now! Fire on them!", always: true);
                    }
                    return;
                }
                var knows = sim.Fog.For(mission.Enemy);
                var target = knows.Contacts.Where(k => k.Visible && ReferenceEquals(k.Unit.Force, mission.Marines) && k.Unit.CanAct && !k.Unit.Def.Flying)
                    .OrderBy(k => Vec2.Distance(k.LastKnownPosition, up[0].Position)).FirstOrDefault();
                if (target != null)
                {
                    if (attack == null) attack = new Attack(sim, enemy, "patrol", target.LastKnownPosition, mission.Marines, flank: up.Count >= Attack.FlankFrom);
                    else attack.Retarget(target.LastKnownPosition);
                    attack.Tick();
                    return;
                }
                if (goal == Waiting || up.Any(u => u.Order != null && u.Order.Verb != Verbs.Suppress)) return;
                if (stop >= route.Count) return;
                TeamOrder(sim, enemy, "patrol", route[stop++], Verbs.Move);
            };

            mission.TurnLimit = Mission.DefaultTurnLimit;
            mission.Judge = m =>
            {
                // Everyone trapped has to come out alive for the win, so a death there ends it.
                var lost = trapped.FirstOrDefault(u => u.Condition == Condition.Dead);
                if (lost != null) return ("failure", (lost == ours0 ? lost.Name : "A villager") + " died before we got everyone out.");
                if (trapped.All(u => u.Evacuated)) return ("success", "Everyone from the rubble is out on the helicopters.");
                return null;
            };
            mission.Status = m => found.Count > 0 ? "onit" : "notyet";
            mission.NotYetWord = "Not found";
            mission.OnItWord = "Digging";
            mission.DoneWord = "Out";

            mission.Title = "Operation " + Pick(names.Routes, rng) + " Spade";
            string grid = map.SquareOf(c).ToString();
            mission.Brief.Add("Task: a strike brought down a house in " + village.Label + " (" + grid + ") with a family and our liaison, " + ours0.Name + " of Atlas 4, inside. Find them, dig them out, and get everyone out on a CASEVAC.");
            mission.Brief.Add("Intent: everyone under the rubble out alive.");
            mission.Brief.Add("Rubble: search it to hear where they are; anyone standing still right beside someone trapped digs, about " + DigSeconds + " seconds each.");
            mission.Brief.Add("Enemy: a Kansian patrol of a few is in the area. " + Hints[goal]);
            mission.Roe = StandingRoe;
            mission.Brief.Add("Civilians: the rest of " + village.Label + " is in their houses. " + RoeLine(mission.Roe));
            mission.Brief.Add(SquadLine(mission, "at " + map.SquareOf(formation[0].pos) + ", about " + (int)(Math.Round(Vec2.Distance(formation[0].pos, c) / 50f) * 50) + " m from the rubble") + " Your helicopters wait nearby: request a CASEVAC to a landing zone you pick.");
            mission.MainObjective = "Dig out everyone under the rubble and get them out on the helicopters.";
            mission.WinText = "Everyone who was under the rubble taken out alive on a CASEVAC from a landing zone you pick.";
            mission.LoseText = LoseLine(mission, "anyone from the rubble dies.");
            mission.OutOfTimeText = "Out of time. People are still under the rubble.";

            mission.Secondary.Add(new SecondaryObjective("villagers", "No villager hurt by our fire", m => m.Sim.Log.Events.OfType<HitBy>().Any(e => ReferenceEquals(e.Shooter.Force, m.Marines) && ReferenceEquals(e.Victim.Force, m.Civilians))));
            mission.Secondary.Add(new SecondaryObjective("family", "The family out before our own", m => outOrder.Contains(ours0) && family.Any(f => !outOrder.Contains(f) || outOrder.IndexOf(f) > outOrder.IndexOf(ours0))) { AtEnd = true });
            mission.Director = new Director(mission, data, new Dice(seed, 13), new DirectorSetup
            {
                Focus = c,
                FocusWord = "the rubble",
                FarSide = p => Dot(p - c, theirWay) > 60f,
                FarWord = "the far side of " + village.Label,
                NearSide = p => Dot(p - c, theirWay) < -60f,
                NearWord = "the near side of " + village.Label,
                AddIns = new[] { Director.Reinforcements, Director.CiviliansRun, Director.Sniper, Director.RadioDown },
                Slow = () => found.Count == 0,
            });
            mission.Secondary.Add(EveryoneHome(mission));
            return mission;
        }

        static Vec2 Inside2(MapGrid map, Vec2 p) => MissionKit.Inside(map, p);

        static readonly Dictionary<string, string> Hints = new Dictionary<string, string>
        {
            { Passing, "Villagers saw them on the road this morning, heading through." },
            { Searching, "Villagers say soldiers have been going door to door." },
            { Waiting, "Nobody's come near the rubble since it fell, though people heard voices from it." },
        };
    }
}
