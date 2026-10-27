using System;
using System.Collections.Generic;
using System.Linq;
using static FieldTable.Sim.MissionKit;

namespace FieldTable.Sim
{
    /// <summary>
    /// The bridge seizure start state (Cinder Ford): a fresh map with water, a bridge, and a village; the squad comes in
    /// along the road from one side, the enemy holds the far side, and civilians are in the village.
    /// </summary>
    public static class BridgeSeizure
    {
        /// <summary>How many enemy soldiers there can be: their goal picks the number within this range.</summary>
        public const int MinEnemy = 3, MaxEnemy = 9;

        /// <summary>Makes the mission. A die picks the enemy's goal unless one is asked for (hold, delay, blow, or ambush).</summary>
        public static Mission Create(GameData data, NamePools names, ulong seed, string goal = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (names == null) throw new ArgumentNullException(nameof(names));
            // A map whose water runs so close to an edge that the squad has no room behind the bridge gets made again.
            var (mapData, map) = MakeMap(data, names, seed, "Bridge seizure " + seed,
                r => { r.Require.Add(MapFeatures.Bridges); r.Require.Add(MapFeatures.Villages); },
                (_, m) => { var (v, b, w) = PickBridge(m); return RoomBehind(m, b, w, v); });
            var (village, bridge, water) = PickBridge(map);
            var rng = new Dice(seed, 11);

            var mission = NewMission(StartStates.Bridge, data, mapData, map, seed);
            var sim = mission.Sim;

            Vec2 villageCenter = village.Center;
            mission.Objective = bridge;
            Vec2 b0 = bridge.Center;

            // Which way the water runs at the bridge, and which bank the village is on.
            Vec2 flow = Direction(water.Points, b0);
            Vec2 across = new Vec2(-flow.Y, flow.X);
            float enemySide = Bank(water.Points, villageCenter);
            if (Math.Abs(enemySide) < 0.5f) enemySide = 1f;
            enemySide = Math.Sign(enemySide);

            // The enemy's goal picks who's there and where. It sets up first, so the squad can start out of its sight.
            float goalRoll = Next(rng);
            goal ??= EnemyPlan.Goals[Math.Min(EnemyPlan.Goals.Count - 1, (int)(goalRoll * EnemyPlan.Goals.Count))];
            var (enemy, plan) = MakeEnemy(mission.Enemy, data, map, rng, bridge, across, water.Points, enemySide, goal);
            // The squad starts on the near bank, as near 300 m from the bridge as it can out of the enemy's sight.
            var formation = StartOutOfSight(sim, b0, p => Bank(water.Points, p) * enemySide <= -5f, enemy, StartDistance, StartNearest,
                b0 - across * (enemySide * StartDistance), lead => Bank(water.Points, lead) * enemySide <= -20f);
            AddSquad(sim, mission.Marines, data, names, rng, formation);
            foreach (var e in enemy) sim.AddUnit(e);
            AddCivilians(sim, mission.Civilians, data, rng, BuildingsIn(map, village), 3 + (int)(Next(rng) * 3));

            string title = "Operation " + Pick(names.Routes, rng) + " Ford";
            mission.Title = title;
            string bridgeGrid = map.SquareOf(Inside(map, b0)).ToString();
            string startGrid = map.SquareOf(formation[0].pos).ToString();
            mission.Brief.Add("Task: seize and hold " + bridge.Label + " (" + bridgeGrid + ") so the platoon can cross.");
            mission.Brief.Add("Intent: the bridge intact and usable, and nobody in " + village.Label + " hurt.");
            mission.Brief.Add("Enemy: unknown strength, on the far bank of " + water.Label + " near " + village.Label + ". " + Hint(map, plan, enemy));
            mission.Roe = StandingRoe;
            mission.Brief.Add("Civilians: in " + village.Label + ". " + RoeLine(mission.Roe));
            int back = (int)Math.Round(Vec2.Distance(formation[0].pos, b0) / 50f) * 50;
            mission.Brief.Add(SquadLine("at " + startGrid + ", about " + back + " m from the bridge"));
            mission.MainObjective = "Seize and hold " + bridge.Label + " (" + bridgeGrid + ").";
            mission.DoneWord = "Seized";
            mission.WinText = "A Marine within " + Mission.OnObjective + " m of " + bridge.Label + " and no enemy still fighting within " + Mission.HoldRadius + " m, at the end of two turns in a row.";
            mission.LoseText = LoseLine(mission);
            mission.OutOfTimeText = "Out of time. The platoon couldn't wait any longer for the bridge.";
            mission.Judge = Mission.HoldTwoTurns(bridge.Label + " is seized and held.");

            // The secondary objectives come from the commander's intent, plus bringing everyone home.
            mission.Secondary.Add(new SecondaryObjective("civilians", "No civilians hurt", m => m.AnyoneHurt(m.Civilians)));
            mission.Secondary.Add(new SecondaryObjective("bridge", "The bridge left standing", m => m.BridgeBlown));

            // The director, with what it needs to place things: the banks, and the road over the bridge.
            var road = map.Landmarks.Where(l => l.Kind == "line" && l.Label.StartsWith("Route ", StringComparison.Ordinal)).OrderBy(l => DistanceTo(b0, l.Points)).FirstOrDefault();
            mission.Director = new Director(mission, data, new Dice(seed, 13), new DirectorSetup
            {
                Focus = b0,
                FocusWord = "the bridge",
                FarSide = p => Bank(water.Points, p) * enemySide > 10f,
                FarWord = "the far bank",
                NearSide = p => Bank(water.Points, p) * enemySide <= -10f,
                NearWord = "the near bank",
                Road = road?.Points,
            });
            mission.Secondary.Add(EveryMarineHome());
            return mission;
        }

        /// <summary>The village, the bridge nearest it, and the water that bridge crosses.</summary>
        static (Landmark village, Landmark bridge, Landmark water) PickBridge(MapGrid map)
        {
            var village = map.Landmarks.First(l => l.Kind == "area" && (l.Note == "village" || l.Note == "town"));
            var bridge = map.Landmarks.Where(l => l.Kind == "line" && l.Label.EndsWith(" bridge", StringComparison.Ordinal))
                .OrderBy(b => Vec2.Distance(b.Center, village.Center)).First();
            var water = map.Landmarks.Where(l => l.Kind == "line" && (l.Label.EndsWith(" River", StringComparison.Ordinal) || l.Label.EndsWith(" Creek", StringComparison.Ordinal)))
                .OrderBy(l => DistanceTo(bridge.Center, l.Points)).First();
            return (village, bridge, water);
        }

        /// <summary>True when there's room on the near bank for the squad to start about 300 m from the bridge, column and all.</summary>
        static bool RoomBehind(MapGrid map, Landmark bridge, Landmark water, Landmark village)
        {
            Vec2 b0 = bridge.Center, flow = Direction(water.Points, b0), across = new Vec2(-flow.Y, flow.X);
            float side = Math.Sign(Bank(water.Points, village.Center));
            if (side == 0f) side = 1f;
            for (int k = -4; k <= 4; k++)
            {
                double turn = k * 0.25;
                Vec2 back = across * -side;
                Vec2 dir = new Vec2((float)(back.X * Math.Cos(turn) - back.Y * Math.Sin(turn)), (float)(back.X * Math.Sin(turn) + back.Y * Math.Cos(turn)));
                Vec2 lead = b0 + dir * StartDistance;
                if (Fits(map, lead, b0) && Bank(water.Points, lead) * side < -20f) return true;
            }
            return false;
        }

        /// <summary>Where the squad would like to start: this far from the bridge, out of the enemy's sight, and never nearer than the least.</summary>
        const float StartDistance = 300f, StartNearest = 150f;

        /// <summary>
        /// The enemy, set up for its goal. Hold: 6 to 9 dug in near the bridge, a machine gun or two on the approach.
        /// Delay: 3 to 5 far back with long views, who fall back as you come. Blow: an engineer setting charges on the
        /// bridge, with 4 to 6 guards. Ambush: 6 to 9 lying low with eyes on the bridge, holding fire until you're on it.
        /// </summary>
        static (List<Unit> units, EnemyPlan plan) MakeEnemy(Force force, GameData data, MapGrid map, Dice rng, Landmark bridgeMark, Vec2 across, IReadOnlyList<Vec2> water, float side, string goal)
        {
            Vec2 bridge = bridgeMark.Center;
            var plan = new EnemyPlan(goal, bridgeMark);
            // Where the attackers will come from: the near bank, back from the bridge.
            Vec2 approach = Inside(map, bridge - across * (side * (goal == EnemyPlan.Delay ? 250f : 150f)));
            var taken = new List<Vec2>();
            var made = new List<Unit>();
            Vec2 Spot(float near, float far, Posture posture, bool watchApproach)
            {
                float eye = Postures.Height(posture);
                var best = RingSpot(map, rng, bridge, near, far, p => Bank(water, p) * side >= 5f, taken, (p, t) =>
                {
                    // Hold digs in: cover counts most. An ambush hides: concealment counts most.
                    float score = goal == EnemyPlan.Ambush ? t.Concealment + t.Cover * 0.5f : t.Cover * (goal == EnemyPlan.Hold ? 1.5f : 1f) + t.Concealment * 0.3f;
                    score += t.SeeOut ? 0.3f : 0f;
                    if (Sight.Trace(map, p, eye, bridge, 1f).Clear) score += goal == EnemyPlan.Ambush ? 2f : 1f;
                    if (watchApproach && Sight.Trace(map, p, eye, approach, 1.7f).Clear) score += 1f;
                    return score;
                });
                if (best != null) return best.Value;
                var fallback = Inside(map, bridge + across * (side * near));
                taken.Add(fallback);
                return fallback;
            }
            var numbered = new Dictionary<string, int>();
            void Add(string def, string name, float near, float far, Posture posture, bool watchApproach) =>
                Fighter(data, force, made, numbered, def, name, Spot(near, far, posture, watchApproach), posture, bridge);
            int Between(int lo, int hi) => MissionKit.Between(rng, lo, hi);

            switch (goal)
            {
                case EnemyPlan.Hold:
                {
                    int count = Between(6, 9), pkm = Between(1, 2);
                    for (int i = 0; i < pkm; i++) Add("en-pkm", "PKM gunner", 80f, 220f, Posture.Prone, true);
                    Add("en-rpg", "RPG gunner", 40f, 130f, Posture.Kneeling, false);
                    while (made.Count < count) Add("en-rf", "Rifleman", 25f, 90f, Posture.Kneeling, false);
                    break;
                }
                case EnemyPlan.Delay:
                {
                    int count = Between(3, 5);
                    Add("en-pkm", "PKM gunner", 180f, 300f, Posture.Prone, true);
                    while (made.Count < count) Add("en-rf", "Rifleman", 150f, 280f, Posture.Kneeling, true);
                    break;
                }
                case EnemyPlan.Blow:
                {
                    // The engineer starts at the far end of the bridge with his charges; his firing point is in cover nearby.
                    Vec2 end = Inside(map, bridge + across * (side * (BridgeHalf(bridgeMark) + 6f)));
                    var engineer = new Unit("enemy-1", "Engineer", data.FindUnit("en-eng"), force, end) { Posture = Posture.Kneeling };
                    engineer.Facing = Bearing(end, bridge);
                    made.Add(engineer);
                    taken.Add(end);
                    plan.Engineer = engineer;
                    plan.FiringPoint = Spot(30f, 60f, Posture.Prone, false);
                    int count = Between(5, 7);
                    Add("en-pkm", "PKM gunner", 80f, 220f, Posture.Prone, true);
                    if (Next(rng) < 0.5f) Add("en-rpg", "RPG gunner", 40f, 130f, Posture.Kneeling, false);
                    while (made.Count < count) Add("en-rf", "Rifleman", 25f, 90f, Posture.Kneeling, false);
                    break;
                }
                default:
                {
                    int count = Between(6, 9), pkm = Between(1, 2), rpg = Between(1, 2);
                    for (int i = 0; i < pkm; i++) Add("en-pkm", "PKM gunner", 60f, 150f, Posture.Prone, false);
                    for (int i = 0; i < rpg; i++) Add("en-rpg", "RPG gunner", 40f, 110f, Posture.Prone, false);
                    while (made.Count < count) Add("en-rf", "Rifleman", 30f, 90f, Posture.Prone, false);
                    break;
                }
            }
            Organize(data, made);
            foreach (var u in made) u.Force.Plan = plan;
            return (made, plan);
        }

        static float BridgeHalf(Landmark bridge) =>
            bridge.Points.Count >= 2 ? Vec2.Distance(bridge.Points[0], bridge.Points[bridge.Points.Count - 1]) / 2f : 10f;

        /// <summary>What the brief says about the enemy for its goal, a hint and never the answer, with where they were seen filled in: how far past the bridge, rounded to 50 m, and the grid square.</summary>
        static string Hint(MapGrid map, EnemyPlan plan, List<Unit> enemy)
        {
            Vec2 seen = new Vec2(enemy.Average(u => u.Position.X), enemy.Average(u => u.Position.Y));
            int meters = (int)Math.Round(Vec2.Distance(seen, plan.Objective.Center) / 50f) * 50;
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, Hints[plan.Goal], meters, map.SquareOf(Inside(map, seen)));
        }

        static readonly Dictionary<string, string> Hints = new Dictionary<string, string>
        {
            { EnemyPlan.Hold, "Soldiers were seen digging in near the bridge." },
            { EnemyPlan.Delay, "A few soldiers were seen watching the road from about {0} m beyond the bridge, around {1}." },
            { EnemyPlan.Blow, "A farmer saw men carrying heavy boxes onto the bridge at dawn." },
            { EnemyPlan.Ambush, "Locals say the soldiers pulled out this morning, but nobody saw them go." },
        };

        /// <summary>
        /// How far a point is from the water's line, plus on one bank and minus on the other. It follows the bends,
        /// so a spot round a bend is still on the bank it's really on.
        /// </summary>
        internal static float Bank(IReadOnlyList<Vec2> line, Vec2 p)
        {
            int best = 0;
            float bestD = float.MaxValue;
            for (int i = 0; i + 1 < line.Count; i++)
            {
                float d = MapGrid.DistanceToSegment(p, line[i], line[i + 1]);
                if (d < bestD) { bestD = d; best = i; }
            }
            Vec2 a = line[best], v = line[best + 1] - line[best];
            float cross = v.X * (p.Y - a.Y) - v.Y * (p.X - a.X);
            return cross < 0f ? -bestD : bestD;
        }
    }
}
