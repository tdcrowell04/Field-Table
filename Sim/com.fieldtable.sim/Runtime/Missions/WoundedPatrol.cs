using System;
using System.Collections.Generic;
using System.Linq;
using static FieldTable.Sim.MissionKit;

namespace FieldTable.Sim
{
    /// <summary>
    /// Pull out the wounded patrol (ledger round 19, for Pararescue): a patrol took casualties and is pinned about 300 m
    /// from a landing zone, three of them hurt, one badly, and one dead. Get to them, treat the hurt, and carry them to
    /// the landing zone while the patrol holds the enemy off, before the badly hurt one bleeds out. The enemy keeps the
    /// patrol pinned, works round toward the landing zone, or falls back once the helicopters show.
    /// </summary>
    public static class WoundedPatrol
    {
        public const string Pin = "pin", Round = "round", FallBack = "fallback";
        public static readonly IReadOnlyList<string> Goals = new[] { Pin, Round, FallBack };

        static readonly Dictionary<string, (string said, string truth)> GoalWords = Words(
            (Pin, "they'll keep the patrol pinned", "keep the patrol pinned"),
            (Round, "they're working round to the landing zone", "work round to the landing zone"),
            (FallBack, "they'll pull back when the helicopters come", "fall back once the helicopters showed"));

        /// <summary>The badly hurt one bleeds out in this many minutes, untreated.</summary>
        public const int BadlyHurtMinutes = 25;

        /// <summary>The landing zone is about this far from the patrol.</summary>
        public const float LzOut = 300f;

        /// <summary>The enemy lies this far from the patrol, on their side.</summary>
        const float EnemyNear = 150f, EnemyFar = 250f;

        /// <summary>Within this of the patrol, the rescuers have reached them and the walking wounded go with them.</summary>
        public const float LinkUp = 10f;

        public const int TurnLimit = 30;

        public static Mission Create(GameData data, NamePools names, ulong seed, string goal = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (names == null) throw new ArgumentNullException(nameof(names));
            // The patrol in some cover, with open ground about 300 m off for the helicopters.
            (Vec2 patrol, Vec2 lz)? Site(MapGrid m)
            {
                var d = new Dice(seed, 31);
                var paths = new Pathfinder(m);
                for (int i = 0; i < 12; i++)
                {
                    var lz = OpenSite(m, d, 15f, 15f, 150f, 80f);
                    if (lz == null) continue;
                    var spot = RingSpot(m, d, lz.Value, LzOut - 40f, LzOut + 40f, p => p.X > 120f && p.Y > 120f && p.X < m.WidthMeters - 120f && p.Y < m.HeightMeters - 120f && !InSettlement(m, p),
                        new List<Vec2>(), (p, t) => t.Cover + t.Concealment * 0.5f);
                    // The casualties have to be carried there: it needs a dry way from the patrol to the landing zone.
                    if (spot != null && m.TerrainAt(spot.Value).Cover >= 0.1f && paths.Find(spot.Value, lz.Value, false) != null) return (spot.Value, lz.Value);
                }
                return null;
            }
            var (mapData, map) = MakeMap(data, names, seed, "Wounded patrol " + seed, r => { }, (_, m) => Site(m) != null);
            var (where, lzAt) = Site(map) ?? (new Vec2(map.WidthMeters / 2f, map.HeightMeters / 2f), new Vec2(map.WidthMeters / 2f, map.HeightMeters / 2f + LzOut));
            var lzMark = AddPoint(mapData, map, "the LZ", lzAt, "landing zone", "LZ", "landing zone", "the landing zone");
            var rng = new Dice(seed, 11);

            var mission = NewMission(StartStates.Patrol, data, mapData, map, seed);
            var sim = mission.Sim;
            var patrolForce = sim.AddForce(new Force { Id = "patrol", Name = "Havoc 3" });
            sim.SetHostile(mission.Marines, patrolForce, false);
            float goalRoll = Next(rng);
            goal ??= Goals[Math.Min(Goals.Count - 1, (int)(goalRoll * Goals.Count))];

            // The patrol: two still fighting, three hurt (one badly), and one dead.
            var free = Shuffle(names.People.ToList(), rng);
            int n = 0;
            string Name(string rank) => rank + " " + (n < free.Count ? free[n++] : "Soldier " + (++n));
            var taken = new List<Vec2>();
            Unit Add(string id, string def, string rank, string[] calls)
            {
                var at = Standable(map, where + Turn(new Vec2(0f, -4f - 2f * (taken.Count / 3)), taken.Count * Math.PI / 3), taken);
                taken.Add(at);
                var u = new Unit(id, Name(rank), data.FindUnit(def), patrolForce, at) { Posture = Posture.Prone, Team = "Havoc" };
                u.Callsigns.AddRange(calls);
                sim.AddUnit(u);
                return u;
            }
            var leader = Add("havoc-tl", "a-tl", "Sgt", new[] { "patrol leader", "Havoc" });
            var gunner = Add("havoc-ar", "a-ar", "Spc", new[] { "Havoc gunner" });
            var bad = Add("havoc-1", "a-gr", "Spc", new[] { "Havoc", "casualty" });
            var leg = Add("havoc-2", "a-rf", "PFC", new[] { "Havoc", "casualty" });
            var arm = Add("havoc-3", "a-rf", "Spc", new[] { "Havoc", "casualty" });
            var dead = Add("havoc-4", "a-rf", "PFC", new[] { "Havoc", "the fallen" });
            bad.Condition = Condition.Down; bad.DownSince = 0; bad.BleedMinutes = BadlyHurtMinutes; bad.Wounds = 1;
            leg.Condition = Condition.Wounded; leg.LegHit = true; leg.Wounds = 1;
            arm.Condition = Condition.Wounded; arm.ArmHit = true; arm.Wounds = 1;
            dead.Condition = Condition.Dead;
            var hurt = new List<Unit> { bad, leg, arm };
            foreach (var u in hurt.Append(dead)) u.Passenger = true;

            // The enemy: in cover on their side of the patrol, knowing where the patrol is.
            Vec2 enemyWay = Unit(where - lzAt);
            var enemy = new List<Unit>();
            var numbered = new Dictionary<string, int>();
            int count = Between(rng, 6, 8);
            for (int i = 0; i < count; i++)
            {
                string def = i == 0 ? "en-pkm" : i == 1 ? "en-rpg" : "en-rf";
                var at = RingSpot(map, rng, where + enemyWay * ((EnemyNear + EnemyFar) / 2f), 0f, 70f, p => Dot(p - where, enemyWay) >= EnemyNear * 0.8f, taken, (p, t) => t.Cover + t.Concealment)
                    ?? Standable(map, Inside(map, where + enemyWay * EnemyFar), taken);
                taken.Add(at);
                Fighter(data, mission.Enemy, enemy, numbered, def, def == "en-pkm" ? "PKM gunner" : def == "en-rpg" ? "RPG gunner" : "Rifleman", at, Posture.Prone, where);
            }
            Organize(data, enemy);
            var plan = new EnemyPlan(goal, lzMark, Goals, GoalWords);
            foreach (var u in enemy) { u.Force.Plan = plan; sim.AddUnit(u); }
            foreach (var x in sim.Units.Where(x => ReferenceEquals(x.Force, patrolForce) && x.Condition != Condition.Dead))
            {
                var k = sim.Fog.For(mission.Enemy).Track(x);
                k.Known = true; k.LastKnownPosition = x.Position; k.LastSeenStep = 0;
            }

            // The patrol has been talking to us on the radio, so the rescuers know where each of them is.
            foreach (var x in sim.Units.Where(x => ReferenceEquals(x.Force, patrolForce)))
            {
                var k = sim.Fog.For(mission.Marines).Track(x);
                k.Known = true; k.LastKnownPosition = x.Position; k.LastSeenStep = 0;
            }

            // The rescuers: put down at the landing zone, out of the enemy's sight.
            var formation = StartOutOfSight(sim, where, p => Vec2.Distance(p, lzAt) <= 80f, enemy, Vec2.Distance(where, lzAt), Vec2.Distance(where, lzAt) - 60f, lzAt);
            AddSquad(sim, mission.Marines, data, names, rng, formation);

            // The walking wounded go with the rescuers once they've got there.
            bool linked = false;
            sim.OnSecond += () =>
            {
                var ours = mission.Squad.Where(u => u.CanAct).ToList();
                if (!linked && ours.Any(m => Vec2.Distance(m.Position, where) <= LinkUp)) linked = true;
                if (linked && arm.CanAct && arm.Order == null && !arm.Gone)
                {
                    Unit with = hurt.Where(h => h != arm && !h.Gone && h.CarriedBy != null).Select(h => h.CarriedBy).FirstOrDefault()
                        ?? ours.OrderBy(m => Vec2.Distance(m.Position, arm.Position)).FirstOrDefault();
                    if (with != null && Vec2.Distance(with.Position, arm.Position) > 6f) Go(sim, arm, Standable(sim.Map, with.Position + new Vec2(2.5f, 2.5f), new[] { with.Position }), Verbs.Move, MoveSpeed.Walk);
                }
            };

            // How the enemy fights: keeps the patrol's heads down; some go for the landing zone; or they pull back for the helicopters.
            var gun = enemy.Where(u => u.Def.Id == "en-pkm").ToList();
            var flankers = goal == Round ? enemy.Where(u => u.Def.Id != "en-pkm" && !u.Def.Radio).Take(4).ToList() : new List<Unit>();
            Attack toLz = null;
            bool gone = false;
            plan.EverySecond = side =>
            {
                if (gone) return;
                int turn = sim.Clock.Turn;
                if (goal == FallBack && sim.Evac != null && sim.Evac.ArriveStep - sim.Clock.Step <= 2 * 60 * SimClock.StepsPerSecond)
                {
                    gone = true;
                    plan.FallingBack = true;
                    sim.Shout(side.FirstOrDefault(u => u.CanAct), "Helicopters! Pull back!", always: true);
                    foreach (var u in side.Where(u => u.CanAct))
                        Go(sim, u, OutAlong(sim.Map, u.Position, enemyWay, 250f), Verbs.Withdraw, MoveSpeed.Run);
                    return;
                }
                // Every 20 seconds, the gun and half the rest fire on the patrol (or the rescuers, if they're nearer).
                if (sim.Clock.Step % (20 * SimClock.StepsPerSecond) < SimClock.StepsPerSecond)
                {
                    var shooters = gun.Concat(side.Where(u => !gun.Contains(u) && !flankers.Contains(u)).Where((u, i) => i % 2 == 0)).Where(u => u.CanAct && (u.Order == null || u.Order.Verb == Verbs.Suppress)).ToList();
                    SuppressKnown(sim, shooters, where, patrolForce);
                }
                if (goal == Round && turn >= 3)
                {
                    toLz ??= new Attack(sim, flankers, "round", lzAt, mission.Marines, flank: false);
                    toLz.Tick();
                }
            };

            mission.Objective = lzMark;
            mission.TurnLimit = TurnLimit;
            mission.Judge = m =>
            {
                var lost = hurt.FirstOrDefault(u => u.Condition == Condition.Dead);
                if (lost != null) return ("failure", lost.Name + " died before we got him out.");
                if (hurt.All(u => u.Evacuated)) return ("success", "All three hurt are out on the helicopters.");
                return null;
            };
            mission.Status = m => linked ? "onit" : "notyet";
            mission.NotYetWord = "Not reached";
            mission.OnItWord = "With them";
            mission.DoneWord = "Out";

            mission.Title = "Operation " + Pick(names.Routes, rng) + " Lift";
            string grid = map.SquareOf(where).ToString();
            mission.Brief.Add("Task: a patrol, Havoc 3, is pinned at " + grid + " with three hurt, one badly, and one dead. Get to them, treat them, and carry them to the LZ at " + map.SquareOf(lzAt) + " for the helicopters.");
            mission.Brief.Add("Intent: every casualty out alive before the badly hurt one bleeds out (about " + BadlyHurtMinutes + " minutes).");
            mission.Brief.Add("Patrol: two still fighting and holding the enemy off. They speak English.");
            mission.Brief.Add("Enemy: " + count + " or so in cover off to the " + Compass(enemyWay) + " of the patrol, with a machine gun. " + Hints[goal]);
            mission.Roe = StandingRoe;
            mission.Brief.Add(RoeLine(mission.Roe));
            mission.Brief.Add(SquadLine(mission, "at the LZ, about " + (int)(Math.Round(Vec2.Distance(where, lzAt) / 50f) * 50) + " m from the patrol") + " Your helicopters wait nearby: request a CASEVAC to the LZ when you're ready.");
            mission.MainObjective = "Get the patrol's three hurt out alive on the helicopters.";
            mission.WinText = "All three hurt taken out on a CASEVAC from the LZ. Carry the two who can't walk; the third walks with you once you reach them.";
            mission.LoseText = LoseLine(mission, "one of the hurt dies.");
            mission.OutOfTimeText = "Out of time. The patrol is still out there.";

            mission.Secondary.Add(new SecondaryObjective("fallen", "The patrol's dead brought out too", m => !dead.Evacuated) { AtEnd = true });
            mission.Director = new Director(mission, data, new Dice(seed, 13), new DirectorSetup
            {
                Focus = where,
                FocusWord = "the patrol",
                FarSide = p => Dot(p - where, enemyWay) > 60f,
                FarWord = "the enemy's side of the patrol",
                NearSide = p => Dot(p - where, enemyWay) < -60f,
                NearWord = "the LZ side",
                AddIns = new[] { Director.Reinforcements, Director.Sniper, Director.RadioDown },
                Slow = () => !linked,
            });
            mission.Secondary.Add(EveryoneHome(mission));
            return mission;
        }

        static readonly Dictionary<string, string> Hints = new Dictionary<string, string>
        {
            { Pin, "Havoc says they've been at it for an hour and aren't moving." },
            { Round, "Havoc saw some of them moving off toward the LZ." },
            { FallBack, "Havoc says they pulled back the last time a helicopter came over." },
        };
    }
}
