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
        /// <summary>How many enemy fighters hold the far bank: a random number in this range each mission.</summary>
        public const int MinEnemy = 5, MaxEnemy = 9;

        public static Mission Create(GameData data, NamePools names, ulong seed)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (names == null) throw new ArgumentNullException(nameof(names));
            var request = new MapRequest { Seed = seed, Name = "Bridge seizure " + seed };
            request.Require.Add(MapFeatures.Bridges);
            request.Require.Add(MapFeatures.Villages);
            var mapData = MapGenerator.Generate(request, names);
            var map = mapData.Build(data.Terrains);
            var rng = new Dice(seed, 11);

            var sim = new Simulation(map, seed);
            sim.AddWeapons(data.Weapons);
            var mission = new Mission { Sim = sim, MapData = mapData };
            mission.Marines = sim.AddForce(new Force { Id = "marines", Name = "Raider 2", Faction = "marines" });
            mission.Enemy = sim.AddForce(new Force { Id = "enemy", Name = "Enemy", Faction = "insurgents" });
            mission.Civilians = sim.AddForce(new Force { Id = "civilians", Name = "Civilians", Faction = "civilians", Civilian = true });
            sim.PlayerForce = mission.Marines;
            sim.RecordFrames(mission.Marines);

            var village = map.Landmarks.First(l => l.Kind == "area" && (l.Note == "village" || l.Note == "town"));
            Vec2 villageCenter = village.Center;
            var bridges = map.Landmarks.Where(l => l.Kind == "line" && l.Label.EndsWith(" bridge", StringComparison.Ordinal)).ToList();
            var bridge = bridges.OrderBy(b => Vec2.Distance(b.Center, villageCenter)).First();
            mission.Objective = bridge;
            Vec2 b0 = bridge.Center;

            // Which way the water runs at the bridge, and which bank the village is on.
            var water = map.Landmarks.Where(l => l.Kind == "line" && (l.Label.EndsWith(" River", StringComparison.Ordinal) || l.Label.EndsWith(" Creek", StringComparison.Ordinal)))
                .OrderBy(l => DistanceTo(b0, l.Points)).First();
            Vec2 flow = Direction(water.Points, b0);
            Vec2 across = new Vec2(-flow.Y, flow.X);
            float enemySide = Bank(water.Points, villageCenter);
            if (Math.Abs(enemySide) < 0.5f) enemySide = 1f;
            enemySide = Math.Sign(enemySide);

            // The enemy sets up first, so the squad can start out of its sight.
            var enemy = MakeEnemy(mission.Enemy, data, map, rng, b0, across, water.Points, enemySide);
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
            mission.Brief.Add("Enemy: unknown strength. Fighters reported near " + village.Label + " on the far bank of " + water.Label + ".");
            mission.Roe = "Positive ID before engaging.";
            mission.Brief.Add("Civilians: in " + village.Label + ". Standing ROE: " + mission.Roe.Substring(0, 1).ToLowerInvariant() + mission.Roe.Substring(1));
            int back = (int)Math.Round(Vec2.Distance(formation[0].pos, b0) / 50f) * 50;
            mission.Brief.Add("You: 13 Marines in three fire teams plus Doc, at " + startGrid + ", about " + back + " m from the bridge.");
            mission.MainObjective = "Seize and hold " + bridge.Label + " (" + bridgeGrid + ").";

            // The secondary objectives come from the commander's intent, plus bringing everyone home.
            mission.Secondary.Add(new SecondaryObjective("civilians", "No civilians hurt", m => m.AnyoneHurt(m.Civilians)));
            // Nothing can damage the bridge yet, so this one holds until explosives that can are built.
            mission.Secondary.Add(new SecondaryObjective("bridge", "The bridge left standing", m => false));
            mission.Secondary.Add(new SecondaryObjective("marines", "Every Marine comes home alive", m => m.Squad.Any(u => u.Condition == Condition.Dead)));
            return mission;
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
            foreach (var team in new[] { "Alpha", "Bravo", "Charlie" })
            {
                slots.Add((team.ToLowerInvariant() + "-tl", "tl", team, new[] { team + " TL", team + " team leader", team + " lead" }));
                slots.Add((team.ToLowerInvariant() + "-ar", "ar", team, new[] { team + " AR", team + " automatic rifleman", team + " gunner" }));
                slots.Add((team.ToLowerInvariant() + "-aar", "aar", team, new[] { team + " AAR", team + " assistant gunner" }));
                bool drone = team == "Charlie";
                slots.Add((team.ToLowerInvariant() + "-rf", drone ? "dr" : "rf", team, drone ? new[] { team + " rifleman", "drone operator" } : new[] { team + " rifleman" }));
            }
            // Column order: Alpha leads, then the squad leader and Doc, then Bravo and Charlie.
            var order = new[] { 2, 3, 4, 5, 0, 1, 6, 7, 8, 9, 10, 11, 12, 13 };
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
            (25f, 0f), (30f, 5f),                             // the squad leader and Doc
            (45f, 0f), (52f, -7f), (59f, -9f), (52f, 7f),     // Bravo
            (70f, 0f), (77f, -7f), (84f, -9f), (77f, 7f),     // Charlie
        };

        /// <summary>Where the squad would like to start: this far from the bridge, out of the enemy's sight.</summary>
        const float StartDistance = 300f, StartNearest = 150f;

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
            var options = new List<(float score, Vec2 lead)>();
            float best = float.MaxValue;
            for (float y = 20f; y < map.HeightMeters - 20f; y += 40f)
                for (float x = 20f; x < map.WidthMeters - 20f; x += 40f)
                {
                    var lead = new Vec2(x, y);
                    if (Bank(water, lead) * enemySide > -20f) continue;
                    float d = Vec2.Distance(lead, bridge);
                    var t = map.TerrainAt(lead);
                    if (d < StartNearest || t.Deep || !t.Passable) continue;
                    float score = Math.Abs(d - StartDistance);
                    foreach (var (spot, _) in Toward(map, lead, bridge))
                    {
                        if (enemy.Any(e => Sees(map, e, spot))) score += hidden.Near(spot, TuckRange).Any() ? TuckCost : 1000f;
                        if (score > best + 1000f) break;
                    }
                    best = Math.Min(best, score);
                    options.Add((score, lead));
                }
            foreach (var (_, lead) in options.OrderBy(o => o.score))
                if (sim.Paths.Find(lead, bridge, false) != null) return Tuck(map, Toward(map, lead, bridge), enemy, hidden);
            return Toward(map, Inside(map, bridge - across * (enemySide * StartDistance)), bridge);
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

        /// <summary>The nearest spot someone can stand, not in water or a building and not on top of anyone, within 12 m.</summary>
        static Vec2 Standable(MapGrid map, Vec2 p, IEnumerable<Vec2> taken)
        {
            var others = taken.ToList();
            bool Ok(Vec2 q)
            {
                if (!map.InBounds(q)) return false;
                var t = map.TerrainAt(q);
                return t.Passable && !t.Deep && !t.SeeOut && t.MoveFactor >= 0.3f && others.All(o => Vec2.Distance(o, q) >= 3f);
            }
            if (Ok(p)) return p;
            for (int r = 2; r <= 12; r += 2)
                for (int a = 0; a < 16; a++)
                {
                    var q = Inside(map, p + new Vec2((float)Math.Cos(a * Math.PI / 8), (float)Math.Sin(a * Math.PI / 8)) * r);
                    if (Ok(q)) return q;
                }
            return p;
        }

        /// <summary>True when an enemy in its spot has a clear line to a Marine standing there, within eye range by day.</summary>
        static bool Sees(MapGrid map, Unit e, Vec2 p) =>
            Vec2.Distance(e.Position, p) <= FogOfWar.DayEyeRange && Sight.Trace(map, e.Position, Postures.Height(e.Posture), p, Postures.Height(Posture.Standing)).Clear;

        static List<Unit> MakeEnemy(Force force, GameData data, MapGrid map, Dice rng, Vec2 bridge, Vec2 across, IReadOnlyList<Vec2> water, float side)
        {
            var taken = new List<Vec2>();
            var made = new List<Unit>();
            void Place(string id, string name, string def, float near, float far, Posture posture)
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
                    float score = t.Cover + (t.SeeOut ? 0.3f : 0f) + t.Concealment * 0.3f + Next(rng) * 0.2f;
                    if (Sight.Trace(map, p, Postures.Height(posture), bridge, 1f).Clear) score += 1f;
                    if (score > bestScore) { bestScore = score; best = p; }
                }
                if (bestScore == float.MinValue) best = Inside(map, bridge + across * (side * near));
                taken.Add(best);
                var u = new Unit(id, name, data.FindUnit(def), force, best) { Posture = posture };
                u.Facing = Bearing(best, bridge);
                made.Add(u);
            }
            // A random 5 to 9 fighters, each a random kind: rifleman, machine gunner, or RPG gunner.
            int count = MinEnemy + (int)(Next(rng) * (MaxEnemy - MinEnemy + 1));
            var kinds = new[]
            {
                (def: "en-rf", name: "Rifleman", near: 25f, far: 90f, posture: Posture.Kneeling),
                (def: "en-pkm", name: "PKM gunner", near: 80f, far: 220f, posture: Posture.Prone),
                (def: "en-rpg", name: "RPG gunner", near: 40f, far: 130f, posture: Posture.Kneeling),
            };
            var numbered = new int[kinds.Length];
            for (int i = 0; i < count; i++)
            {
                int k = Math.Min(kinds.Length - 1, (int)(Next(rng) * kinds.Length));
                var kind = kinds[k];
                Place("enemy-" + (i + 1), kind.name + " " + (++numbered[k]), kind.def, kind.near, kind.far, kind.posture);
            }
            return made;
        }

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

        static float Next(Dice d) => (d.NextUInt() >> 8) * (1f / 16777216f);

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
        static float Bank(IReadOnlyList<Vec2> line, Vec2 p)
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

        static Vec2 Inside(MapGrid map, Vec2 p) =>
            new Vec2(Math.Max(3f, Math.Min(map.WidthMeters - 3f, p.X)), Math.Max(3f, Math.Min(map.HeightMeters - 3f, p.Y)));

        static float Bearing(Vec2 a, Vec2 b) => (float)(Math.Atan2(b.X - a.X, -(b.Y - a.Y)) * 180.0 / Math.PI + 360.0) % 360f;

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
