using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    /// <summary>
    /// The bridge seizure start state (Cinder Ford): a fresh map with water, a bridge, and a village; the squad comes in
    /// along the road from one side, the enemy holds the far side, and civilians are in the village.
    /// </summary>
    public static class BridgeSeizure
    {
        /// <summary>How many enemy fighters there can be: their goal picks the number within this range.</summary>
        public const int MinEnemy = 3, MaxEnemy = 9;

        /// <summary>Makes the mission. A die picks the enemy's goal unless one is asked for (hold, delay, blow, or ambush).</summary>
        public static Mission Create(GameData data, NamePools names, ulong seed, string goal = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (names == null) throw new ArgumentNullException(nameof(names));
            // A kilometer square: room to start well back, and still inside what three-digit coordinates can name. A map
            // whose water runs so close to an edge that the squad has no room behind the bridge gets made again.
            MapData mapData = null;
            MapGrid map = null;
            Landmark village = null, bridge = null, water = null;
            for (int attempt = 0; attempt < MapTries; attempt++)
            {
                var request = new MapRequest { Seed = seed + (ulong)attempt * 0x9E3779B97F4A7C15UL, Name = "Bridge seizure " + seed, Columns = MapSquares, Rows = MapSquares };
                request.Require.Add(MapFeatures.Bridges);
                request.Require.Add(MapFeatures.Villages);
                mapData = MapGenerator.Generate(request, names);
                map = mapData.Build(data.Terrains);
                (village, bridge, water) = PickBridge(map);
                if (RoomBehind(map, bridge, water, village)) break;
            }
            var rng = new Dice(seed, 11);

            var sim = new Simulation(map, seed);
            sim.AddWeapons(data.Weapons);
            var mission = new Mission { Sim = sim, MapData = mapData };
            mission.Marines = sim.AddForce(new Force { Id = "marines", Name = "Raider 2", Faction = "marines" });
            mission.Enemy = sim.AddForce(new Force { Id = "enemy", Name = "Enemy", Faction = "insurgents" });
            // The villagers and the fighters don't speak English, so talking to them takes the interpreter.
            mission.Enemy.Language = "local";
            mission.Civilians = sim.AddForce(new Force { Id = "civilians", Name = "Civilians", Faction = "civilians", Civilian = true, Language = "local" });
            sim.PlayerForce = mission.Marines;
            sim.RecordFrames(mission.Marines);

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
            var formation = Formation(sim, b0, across, water.Points, enemySide, enemy);
            AddSquad(sim, mission.Marines, data, names, rng, formation);
            foreach (var e in enemy) sim.AddUnit(e);
            AddCivilians(sim, mission.Civilians, data, map, rng, village);

            string title = "Operation " + Pick(names.Routes, rng) + " Ford";
            mission.Title = title;
            string bridgeGrid = map.SquareOf(Inside(map, b0)).ToString();
            string startGrid = map.SquareOf(formation[0].pos).ToString();
            mission.Brief.Add("Task: seize and hold " + bridge.Label + " (" + bridgeGrid + ") so the platoon can cross.");
            mission.Brief.Add("Intent: the bridge intact and usable, and nobody in " + village.Label + " hurt.");
            mission.Brief.Add("Enemy: unknown strength, on the far bank of " + water.Label + " near " + village.Label + ". " + Hints[plan.Goal]);
            mission.Roe = "Positive ID before engaging.";
            mission.Brief.Add("Civilians: in " + village.Label + ". Standing ROE: " + mission.Roe.Substring(0, 1).ToLowerInvariant() + mission.Roe.Substring(1));
            int back = (int)Math.Round(Vec2.Distance(formation[0].pos, b0) / 50f) * 50;
            mission.Brief.Add("You: 13 Marines in three fire teams plus Doc and an interpreter, at " + startGrid + ", about " + back + " m from the bridge. The villagers and the fighters don't speak English.");
            mission.MainObjective = "Seize and hold " + bridge.Label + " (" + bridgeGrid + ").";

            // The secondary objectives come from the commander's intent, plus bringing everyone home.
            mission.Secondary.Add(new SecondaryObjective("civilians", "No civilians hurt", m => m.AnyoneHurt(m.Civilians)));
            mission.Secondary.Add(new SecondaryObjective("bridge", "The bridge left standing", m => m.BridgeBlown));

            // The director, with what it needs to place things: the banks, and the road over the bridge.
            var road = map.Landmarks.Where(l => l.Kind == "line" && l.Label.StartsWith("Route ", StringComparison.Ordinal)).OrderBy(l => DistanceTo(b0, l.Points)).FirstOrDefault();
            mission.Director = new Director(mission, data, new Dice(seed, 13), water.Points, enemySide, road?.Points);
            mission.Secondary.Add(new SecondaryObjective("marines", "Every Marine comes home alive", m => m.Squad.Any(u => u.Condition == Condition.Dead)));
            return mission;
        }

        /// <summary>The map is this many 100 m squares each way.</summary>
        public const int MapSquares = 10;

        /// <summary>How many maps to try for one with room behind the bridge.</summary>
        const int MapTries = 12;

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

        static void AddSquad(Simulation sim, Force force, GameData data, NamePools names, Dice rng, List<(Vec2 pos, float facing)> formation)
        {
            var people = Shuffle(names.People.Count > 0 ? names.People : new List<string> { "Marine" }, rng);
            int next = 0;
            string Name() => next < people.Count ? people[next++] : "Marine " + (next++ + 1);

            var slots = new List<(string id, string def, string team, string[] calls)>
            {
                ("lead", "sl", "", new[] { "Lead", "SL", "squad leader" }),
                ("doc", "doc", "", new[] { "Doc", "corpsman" }),
            };
            int terp = -1;
            foreach (var team in new[] { "Alpha", "Bravo", "Charlie" })
            {
                slots.Add((team.ToLowerInvariant() + "-tl", "tl", team, new[] { team + " TL", team + " team leader", team + " lead" }));
                slots.Add((team.ToLowerInvariant() + "-ar", "ar", team, new[] { team + " AR", team + " automatic rifleman", team + " gunner" }));
                slots.Add((team.ToLowerInvariant() + "-aar", "aar", team, new[] { team + " AAR", team + " assistant gunner" }));
                bool drone = team == "Charlie";
                slots.Add((team.ToLowerInvariant() + "-rf", drone ? "dr" : "rf", team, drone ? new[] { team + " rifleman", "drone operator" } : new[] { team + " rifleman" }));
            }
            // The interpreter goes with the squad leader. He isn't a Marine, and he carries no weapon.
            terp = slots.Count;
            slots.Add(("terp", "terp", "", new[] { "Interpreter", "terp" }));
            // Column order: Alpha leads, then the squad leader, Doc, and the interpreter, then Bravo and Charlie.
            var order = new[] { 2, 3, 4, 5, 0, 1, terp, 6, 7, 8, 9, 10, 11, 12, 13 };
            for (int i = 0; i < order.Length; i++)
            {
                var s = slots[order[i]];
                var u = new Unit(s.id, Name(), data.FindUnit(s.def), force, formation[i].pos) { Team = s.team };
                u.Callsigns.AddRange(s.calls);
                u.Facing = formation[i].facing;
                sim.AddUnit(u);
                // A Marine who flies a drone carries it in its case until it's launched.
                if (u.Def.Drone != null)
                {
                    var drone = new Unit(s.id + "-drone", "Drone", data.FindUnit(u.Def.Drone), force, u.Position) { Operator = u, Gone = true };
                    drone.Callsigns.Add("drone");
                    sim.AddUnit(drone);
                }
            }
        }

        // Where each Marine stands, in the order AddSquad fills them: meters back from the lead and to the right of the road.
        static readonly (float back, float side)[] Slots =
        {
            (0f, 0f), (7f, -7f), (14f, -9f), (7f, 7f),       // Alpha in a wedge: TL, AR, AAR, rifleman
            (25f, 0f), (30f, 5f), (30f, -5f),                 // the squad leader, Doc, and the interpreter
            (45f, 0f), (52f, -7f), (59f, -9f), (52f, 7f),     // Bravo
            (70f, 0f), (77f, -7f), (84f, -9f), (77f, 7f),     // Charlie
        };

        /// <summary>Where the squad would like to start: this far from the bridge, out of the enemy's sight.</summary>
        const float StartDistance = 300f, StartNearest = 150f;

        /// <summary>How deep the column is, from Alpha's point man back to Charlie's last Marine.</summary>
        const float ColumnDepth = 90f;

        /// <summary>How far a Marine who'd start in the enemy's sight can shift to a hidden spot, and what that costs a start.</summary>
        const float TuckRange = 30f, TuckCost = 25f;

        /// <summary>
        /// Where the squad starts, on the near bank: fire teams in wedges facing the bridge, one behind another (Alpha,
        /// then the squad leader and Doc, then Bravo and Charlie), where the fewest of them can be seen by the enemy,
        /// as near 300 m from the bridge as that allows, and never closer than 150 m. It needs a dry route to the bridge.
        /// Anyone the enemy could still see shifts to the nearest hidden spot within 30 m, so the wedges loosen to fit the cover.
        /// </summary>
        static List<(Vec2 pos, float facing)> Formation(Simulation sim, Vec2 bridge, Vec2 across, IReadOnlyList<Vec2> water, float enemySide, List<Unit> enemy)
        {
            var map = sim.Map;
            var hidden = new HiddenSpots(map, enemy, bridge, water, enemySide);

            // Every spot the lead could start on, nearest 300 m first.
            var spots = new List<(float near, Vec2 lead)>();
            for (float y = 20f; y < map.HeightMeters - 20f; y += 40f)
                for (float x = 20f; x < map.WidthMeters - 20f; x += 40f)
                {
                    var lead = new Vec2(x, y);
                    if (!Fits(map, lead, bridge) || Bank(water, lead) * enemySide > -20f) continue;
                    float d = Vec2.Distance(lead, bridge);
                    var t = map.TerrainAt(lead);
                    if (d < StartNearest || t.Deep || !t.Passable) continue;
                    spots.Add((Math.Abs(d - StartDistance), lead));
                }
            spots.Sort((a, b) => a.near.CompareTo(b.near));

            // How good a start is: how far from 300 m, plus a little for each Marine who has to shift to cover and a lot
            // for each one the enemy would see anyway. Spots get scored only until the next one couldn't do better.
            float Score(Vec2 lead, float near)
            {
                float score = near;
                foreach (var (spot, _) in Toward(map, lead, bridge))
                {
                    // Nobody starts in deep water.
                    if (map.TerrainAt(spot).Deep) return float.MaxValue / 2f;
                    if (enemy.Any(e => Sees(map, e, spot))) score += hidden.Near(spot, TuckRange).Any() ? TuckCost : 1000f;
                }
                return score;
            }
            var scored = new List<(float score, Vec2 lead)>();
            int next = 0;
            while (true)
            {
                float best = scored.Count > 0 ? scored.Min(o => o.score) : float.MaxValue;
                for (; next < spots.Count && spots[next].near < best; next++)
                {
                    float score = Score(spots[next].lead, spots[next].near);
                    scored.Add((score, spots[next].lead));
                    best = Math.Min(best, score);
                }
                if (scored.Count == 0) break;
                var top = scored.OrderBy(o => o.score).First();
                scored.Remove(top);
                if (sim.Paths.Find(top.lead, bridge, false) != null) return Tuck(map, Toward(map, top.lead, bridge), enemy, hidden);
            }
            return Toward(map, Inside(map, bridge - across * (enemySide * StartDistance)), bridge);
        }

        /// <summary>True when the whole column, from the lead back, fits on the map instead of getting squashed against an edge.</summary>
        static bool Fits(MapGrid map, Vec2 lead, Vec2 bridge)
        {
            Vec2 tail = lead - (bridge - lead) * (ColumnDepth / Math.Max(1f, Vec2.Distance(bridge, lead)));
            return lead.X >= 12f && lead.Y >= 12f && lead.X <= map.WidthMeters - 12f && lead.Y <= map.HeightMeters - 12f
                && tail.X >= 12f && tail.Y >= 12f && tail.X <= map.WidthMeters - 12f && tail.Y <= map.HeightMeters - 12f;
        }

        /// <summary>Moves each Marine the enemy could see to the nearest hidden spot within 30 m, if there is one.</summary>
        static List<(Vec2, float)> Tuck(MapGrid map, List<(Vec2 pos, float facing)> spots, List<Unit> enemy, HiddenSpots hidden)
        {
            for (int i = 0; i < spots.Count; i++)
            {
                var (pos, facing) = spots[i];
                if (!enemy.Any(e => Sees(map, e, pos))) continue;
                int me = i;
                var others = spots.Where((_, j) => j != me).Select(o => o.pos).ToList();
                foreach (var h in hidden.Near(pos, TuckRange))
                {
                    var q = Standable(map, h, others);
                    if (enemy.Any(e => Sees(map, e, q))) continue;
                    spots[i] = (q, facing);
                    break;
                }
            }
            return spots;
        }

        /// <summary>
        /// The spots on the near bank, at least 150 m from the bridge, where no enemy can see a man standing, every 8 m.
        /// Each spot is only worked out the first time it's asked about, to keep the phone quick.
        /// </summary>
        sealed class HiddenSpots
        {
            const float Cell = 8f;
            readonly MapGrid map;
            readonly List<Unit> enemy;
            readonly Vec2 bridge;
            readonly IReadOnlyList<Vec2> water;
            readonly float enemySide;
            readonly sbyte[,] known;   // 0 not worked out yet, 1 hidden, -1 not

            public HiddenSpots(MapGrid map, List<Unit> enemy, Vec2 bridge, IReadOnlyList<Vec2> water, float enemySide)
            {
                this.map = map; this.enemy = enemy; this.bridge = bridge; this.water = water; this.enemySide = enemySide;
                known = new sbyte[(int)(map.WidthMeters / Cell), (int)(map.HeightMeters / Cell)];
            }

            static Vec2 Center(int cx, int cy) => new Vec2((cx + 0.5f) * Cell, (cy + 0.5f) * Cell);

            bool Hidden(int cx, int cy)
            {
                if (known[cx, cy] == 0)
                {
                    var c = Center(cx, cy);
                    var t = map.TerrainAt(c);
                    bool ok = Bank(water, c) * enemySide <= -5f && Vec2.Distance(c, bridge) >= StartNearest
                        && t.Passable && !t.Deep && !t.SeeOut && t.MoveFactor >= 0.3f && !enemy.Any(e => Sees(map, e, c));
                    known[cx, cy] = (sbyte)(ok ? 1 : -1);
                }
                return known[cx, cy] > 0;
            }

            /// <summary>The hidden spots within range of a point, nearest first.</summary>
            public IEnumerable<Vec2> Near(Vec2 p, float range)
            {
                int r = (int)Math.Ceiling(range / Cell);
                int px = (int)(p.X / Cell), py = (int)(p.Y / Cell);
                var found = new List<Vec2>();
                for (int cx = Math.Max(0, px - r); cx <= Math.Min(known.GetLength(0) - 1, px + r); cx++)
                    for (int cy = Math.Max(0, py - r); cy <= Math.Min(known.GetLength(1) - 1, py + r); cy++)
                        if (Vec2.Distance(Center(cx, cy), p) <= range && Hidden(cx, cy)) found.Add(Center(cx, cy));
                return found.OrderBy(c => Vec2.Distance(c, p));
            }
        }

        static List<(Vec2, float)> Toward(MapGrid map, Vec2 lead, Vec2 bridge)
        {
            Vec2 f = bridge - lead;
            float len = f.Length;
            f = len > 0.01f ? f * (1f / len) : new Vec2(0, -1);
            var right = new Vec2(-f.Y, f.X);
            float facing = Bearing(lead, bridge);
            var spots = new List<(Vec2, float)>();
            foreach (var (back, side) in Slots)
                spots.Add((Standable(map, Inside(map, lead - f * back + right * side), spots.Select(x => x.Item1)), facing));
            return spots;
        }

        /// <summary>The nearest spot someone can stand, not in water or a building and not on top of anyone, within 30 m.</summary>
        internal static Vec2 Standable(MapGrid map, Vec2 p, IEnumerable<Vec2> taken)
        {
            var others = taken.ToList();
            bool Ok(Vec2 q)
            {
                if (!map.InBounds(q)) return false;
                var t = map.TerrainAt(q);
                return t.Passable && !t.Deep && !t.SeeOut && t.MoveFactor >= 0.3f && others.All(o => Vec2.Distance(o, q) >= 3f);
            }
            if (Ok(p)) return p;
            for (int r = 2; r <= 30; r += 2)
                for (int a = 0; a < 16; a++)
                {
                    var q = Inside(map, p + new Vec2((float)Math.Cos(a * Math.PI / 8), (float)Math.Sin(a * Math.PI / 8)) * r);
                    if (Ok(q)) return q;
                }
            return p;
        }

        /// <summary>True when an enemy in its spot has a clear line to a Marine standing there, within eye range by day.</summary>
        internal static bool Sees(MapGrid map, Unit e, Vec2 p) =>
            Vec2.Distance(e.Position, p) <= FogOfWar.DayEyeRange && Sight.Trace(map, e.Position, Postures.Height(e.Posture), p, Postures.Height(Posture.Standing)).Clear;

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
                Vec2 best = default;
                float bestScore = float.MinValue;
                for (int k = 0; k < 300; k++)
                {
                    float r = near + (far - near) * Next(rng);
                    double a = Next(rng) * Math.PI * 2;
                    var p = new Vec2(bridge.X + (float)Math.Cos(a) * r, bridge.Y + (float)Math.Sin(a) * r);
                    if (!map.InBounds(p) || Bank(water, p) * side < 5f) continue;
                    var t = map.TerrainAt(p);
                    if (t.Deep || !t.Passable) continue;
                    if (taken.Any(q => Vec2.Distance(q, p) < 15f)) continue;
                    // Hold digs in: cover counts most. An ambush hides: concealment counts most.
                    float score = goal == EnemyPlan.Ambush ? t.Concealment + t.Cover * 0.5f : t.Cover * (goal == EnemyPlan.Hold ? 1.5f : 1f) + t.Concealment * 0.3f;
                    score += (t.SeeOut ? 0.3f : 0f) + Next(rng) * 0.2f;
                    float eye = Postures.Height(posture);
                    if (Sight.Trace(map, p, eye, bridge, 1f).Clear) score += goal == EnemyPlan.Ambush ? 2f : 1f;
                    if (watchApproach && Sight.Trace(map, p, eye, approach, 1.7f).Clear) score += 1f;
                    if (score > bestScore) { bestScore = score; best = p; }
                }
                if (bestScore == float.MinValue) best = Inside(map, bridge + across * (side * near));
                taken.Add(best);
                return best;
            }
            var numbered = new Dictionary<string, int>();
            void Add(string def, string name, float near, float far, Posture posture, bool watchApproach)
            {
                numbered[name] = numbered.TryGetValue(name, out int n) ? n + 1 : 1;
                var p = Spot(near, far, posture, watchApproach);
                var u = new Unit("enemy-" + (made.Count + 1), name + " " + numbered[name], data.FindUnit(def), force, p) { Posture = posture };
                u.Facing = Bearing(p, bridge);
                made.Add(u);
            }
            int Between(int lo, int hi) => lo + Math.Min(hi - lo, (int)(Next(rng) * (hi - lo + 1)));

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
            foreach (var u in made) u.Force.Plan = plan;
            return (made, plan);
        }

        static float BridgeHalf(Landmark bridge) =>
            bridge.Points.Count >= 2 ? Vec2.Distance(bridge.Points[0], bridge.Points[bridge.Points.Count - 1]) / 2f : 10f;

        /// <summary>What the brief says about the enemy for each goal: a hint, never the answer.</summary>
        static readonly Dictionary<string, string> Hints = new Dictionary<string, string>
        {
            { EnemyPlan.Hold, "Fighters were seen digging in near the bridge." },
            { EnemyPlan.Delay, "A few fighters have been seen watching the road from well back." },
            { EnemyPlan.Blow, "A farmer saw men carrying heavy boxes onto the bridge at dawn." },
            { EnemyPlan.Ambush, "Locals say the fighters pulled out this morning, but nobody saw them go." },
        };

        static void AddCivilians(Simulation sim, Force force, GameData data, MapGrid map, Dice rng, Landmark village)
        {
            var homes = map.Landmarks.Where(l => l.Kind == "building" && village.Contains(l.Center)).ToList();
            int count = Math.Min(homes.Count, 3 + (int)(Next(rng) * 3));
            homes = Shuffle(homes, rng);
            for (int i = 0; i < count; i++)
            {
                var u = new Unit("civilian-" + (i + 1), "Civilian " + (i + 1), data.FindUnit("civ"), force, homes[i].Center);
                sim.AddUnit(u);
            }
        }

        // ---------- helpers ----------

        internal static float Next(Dice d) => (d.NextUInt() >> 8) * (1f / 16777216f);

        static List<T> Shuffle<T>(IList<T> list, Dice rng)
        {
            var copy = list.ToList();
            for (int i = copy.Count - 1; i > 0; i--)
            {
                int j = (int)(Next(rng) * (i + 1));
                var t = copy[i]; copy[i] = copy[j]; copy[j] = t;
            }
            return copy;
        }

        static string Pick(List<string> pool, Dice rng) => pool.Count == 0 ? "Cinder" : pool[(int)(Next(rng) * pool.Count) % pool.Count];

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

        internal static Vec2 Inside(MapGrid map, Vec2 p) =>
            new Vec2(Math.Max(3f, Math.Min(map.WidthMeters - 3f, p.X)), Math.Max(3f, Math.Min(map.HeightMeters - 3f, p.Y)));

        internal static float Bearing(Vec2 a, Vec2 b) => (float)(Math.Atan2(b.X - a.X, -(b.Y - a.Y)) * 180.0 / Math.PI + 360.0) % 360f;

        static float DistanceTo(Vec2 p, IReadOnlyList<Vec2> line)
        {
            float best = float.MaxValue;
            for (int i = 0; i + 1 < line.Count; i++) best = Math.Min(best, MapGrid.DistanceToSegment(p, line[i], line[i + 1]));
            return best;
        }

        /// <summary>The direction of the line's segment nearest a point.</summary>
        static Vec2 Direction(IReadOnlyList<Vec2> line, Vec2 p)
        {
            int best = 0;
            float bestD = float.MaxValue;
            for (int i = 0; i + 1 < line.Count; i++)
            {
                float d = MapGrid.DistanceToSegment(p, line[i], line[i + 1]);
                if (d < bestD) { bestD = d; best = i; }
            }
            Vec2 v = line[best + 1] - line[best];
            float len = v.Length;
            return len > 0f ? v * (1f / len) : new Vec2(0, 1);
        }
    }
}
