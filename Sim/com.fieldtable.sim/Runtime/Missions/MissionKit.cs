using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    /// <summary>
    /// What every start state shares: the forces, the squad and how it forms up, starting out of the enemy's sight,
    /// putting people where they belong, and the small map helpers.
    /// </summary>
    internal static class MissionKit
    {
        /// <summary>The map every start state uses: this many 100 m squares each way, a kilometer square.</summary>
        public const int MapSquares = 10;

        /// <summary>How many maps to try for one that suits a start state before taking the last one.</summary>
        public const int MapTries = 12;

        /// <summary>Makes maps from the seed (and seeds after it) until one suits the start state, or the tries run out.</summary>
        public static (MapData data, MapGrid map) MakeMap(GameData data, NamePools names, ulong seed, string name, Action<MapRequest> setup, Func<MapData, MapGrid, bool> suits)
        {
            MapData mapData = null;
            MapGrid map = null;
            for (int attempt = 0; attempt < MapTries; attempt++)
            {
                var request = new MapRequest { Seed = seed + (ulong)attempt * 0x9E3779B97F4A7C15UL, Name = name, Columns = MapSquares, Rows = MapSquares };
                setup?.Invoke(request);
                try
                {
                    mapData = MapGenerator.Generate(request, names);
                }
                catch (InvalidOperationException) when (attempt + 1 < MapTries)
                {
                    continue;
                }
                map = mapData.Build(data.Terrains);
                if (suits == null || suits(mapData, map)) break;
            }
            return (mapData, map);
        }

        /// <summary>The simulation and its three sides: the Marines, the enemy, and the civilians.</summary>
        public static Mission NewMission(string kind, GameData data, MapData mapData, MapGrid map, ulong seed)
        {
            var sim = new Simulation(map, seed);
            sim.AddWeapons(data.Weapons);
            var mission = new Mission { Kind = kind, Sim = sim, MapData = mapData };
            mission.Marines = sim.AddForce(new Force { Id = "marines", Name = "Raider 2", Faction = "marines" });
            mission.Enemy = sim.AddForce(new Force { Id = "enemy", Name = "Enemy", Faction = "insurgents" });
            // The villagers and the fighters don't speak English, so talking to them takes the interpreter.
            mission.Enemy.Language = "local";
            mission.Civilians = sim.AddForce(new Force { Id = "civilians", Name = "Civilians", Faction = "civilians", Civilian = true, Language = "local" });
            sim.PlayerForce = mission.Marines;
            sim.RecordFrames(mission.Marines);
            return mission;
        }

        /// <summary>The usual last secondary objective, and the rules of engagement most missions start with.</summary>
        public static SecondaryObjective EveryMarineHome() => new SecondaryObjective("marines", "Every Marine comes home alive", m => m.Squad.Any(u => u.Condition == Condition.Dead));
        public const string StandingRoe = "Positive ID before engaging.";

        /// <summary>How the mission is lost, for the brief: too many Marines down, or out of time, and anything the start state adds.</summary>
        public static string LoseLine(Mission m, string more = null) =>
            Mission.CombatIneffective + " Marines down, " + (more == null ? "or " : "") + "past turn " + m.TurnLimit + (more == null ? "." : ", or " + more);

        public static string RoeLine(string roe) => "Standing ROE: " + roe.Substring(0, 1).ToLowerInvariant() + roe.Substring(1);

        // ---------- the squad ----------

        /// <summary>The squad's line in a brief, where it starts.</summary>
        public static string SquadLine(string where) => "You: 13 Marines in three fire teams plus Doc and an interpreter, " + where + ". The villagers and the fighters don't speak English.";

        public static void AddSquad(Simulation sim, Force force, GameData data, NamePools names, Dice rng, List<(Vec2 pos, float facing)> formation)
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
            // The interpreter goes with the squad leader. He isn't a Marine, and he carries no weapon.
            int terp = slots.Count;
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

        // Where each Marine stands, in the order AddSquad fills them: meters back from the lead and to the right of the way ahead.
        static readonly (float back, float side)[] Slots =
        {
            (0f, 0f), (7f, -7f), (14f, -9f), (7f, 7f),       // Alpha in a wedge: TL, AR, AAR, rifleman
            (25f, 0f), (30f, 5f), (30f, -5f),                 // the squad leader, Doc, and the interpreter
            (45f, 0f), (52f, -7f), (59f, -9f), (52f, 7f),     // Bravo
            (70f, 0f), (77f, -7f), (84f, -9f), (77f, 7f),     // Charlie
        };

        /// <summary>How deep the column is, from Alpha's point man back to Charlie's last Marine.</summary>
        const float ColumnDepth = 90f;

        /// <summary>How far a Marine who'd start in the enemy's sight can shift to a hidden spot, and what that costs a start.</summary>
        const float TuckRange = 30f, TuckCost = 25f;

        /// <summary>
        /// Where the squad starts: fire teams in wedges facing what they're going for, one behind another (Alpha, then the
        /// squad leader, Doc, and the interpreter, then Bravo and Charlie), on ground the start state allows, where the
        /// fewest can be seen by the enemy, as near the distance it wants as that allows and never nearer than its least.
        /// It needs a dry route to the target. Anyone the enemy could still see shifts to the nearest hidden spot within
        /// 30 m, so the wedges loosen to fit the cover. The lead's own spot can be held to a stricter rule than the rest.
        /// </summary>
        public static List<(Vec2 pos, float facing)> StartOutOfSight(Simulation sim, Vec2 target, Func<Vec2, bool> allowed, List<Unit> enemy, float distance, float nearest, Vec2 fallback, Func<Vec2, bool> leadAllowed = null)
        {
            var map = sim.Map;
            leadAllowed ??= allowed;
            var hidden = new HiddenSpots(map, enemy, c => allowed(c) && Vec2.Distance(c, target) >= nearest);

            // Every spot the lead could start on, nearest the wanted distance first.
            var spots = new List<(float near, Vec2 lead)>();
            for (float y = 20f; y < map.HeightMeters - 20f; y += 40f)
                for (float x = 20f; x < map.WidthMeters - 20f; x += 40f)
                {
                    var lead = new Vec2(x, y);
                    if (!Fits(map, lead, target) || !leadAllowed(lead)) continue;
                    float d = Vec2.Distance(lead, target);
                    var t = map.TerrainAt(lead);
                    if (d < nearest || t.Deep || !t.Passable) continue;
                    spots.Add((Math.Abs(d - distance), lead));
                }
            spots.Sort((a, b) => a.near.CompareTo(b.near));

            // How good a start is: how far from the wanted distance, plus a little for each Marine who has to shift to
            // cover and a lot for each one the enemy would see anyway. Spots get scored only until the next one couldn't do better.
            float Score(Vec2 lead, float near)
            {
                float score = near;
                foreach (var (spot, _) in Toward(map, lead, target))
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
                if (sim.Paths.Find(top.lead, target, false) != null) return Tuck(map, Toward(map, top.lead, target), enemy, hidden);
            }
            return Toward(map, Inside(map, fallback), target);
        }

        /// <summary>True when the whole column, from the lead back, fits on the map instead of getting squashed against an edge.</summary>
        public static bool Fits(MapGrid map, Vec2 lead, Vec2 target)
        {
            Vec2 tail = lead - (target - lead) * (ColumnDepth / Math.Max(1f, Vec2.Distance(target, lead)));
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
        /// The spots the start state allows where no enemy can see a man standing, every 8 m. Each spot is only worked out
        /// the first time it's asked about, to keep the phone quick.
        /// </summary>
        sealed class HiddenSpots
        {
            const float Cell = 8f;
            readonly MapGrid map;
            readonly List<Unit> enemy;
            readonly Func<Vec2, bool> allowed;
            readonly sbyte[,] known;   // 0 not worked out yet, 1 hidden, -1 not

            public HiddenSpots(MapGrid map, List<Unit> enemy, Func<Vec2, bool> allowed)
            {
                this.map = map; this.enemy = enemy; this.allowed = allowed;
                known = new sbyte[(int)(map.WidthMeters / Cell), (int)(map.HeightMeters / Cell)];
            }

            static Vec2 Center(int cx, int cy) => new Vec2((cx + 0.5f) * Cell, (cy + 0.5f) * Cell);

            bool Hidden(int cx, int cy)
            {
                if (known[cx, cy] == 0)
                {
                    var c = Center(cx, cy);
                    var t = map.TerrainAt(c);
                    bool ok = allowed(c) && t.Passable && !t.Deep && !t.SeeOut && t.MoveFactor >= 0.3f && !enemy.Any(e => Sees(map, e, c));
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

        /// <summary>The squad in its column of wedges, lead at a spot, facing a target.</summary>
        public static List<(Vec2, float)> Toward(MapGrid map, Vec2 lead, Vec2 target)
        {
            Vec2 f = target - lead;
            float len = f.Length;
            f = len > 0.01f ? f * (1f / len) : new Vec2(0, -1);
            var right = new Vec2(-f.Y, f.X);
            float facing = Bearing(lead, target);
            var spots = new List<(Vec2, float)>();
            foreach (var (back, side) in Slots)
                spots.Add((Standable(map, Inside(map, lead - f * back + right * side), spots.Select(x => x.Item1)), facing));
            return spots;
        }

        /// <summary>The nearest spot someone can stand, not in water or a building and not on top of anyone, within 30 m.</summary>
        public static Vec2 Standable(MapGrid map, Vec2 p, IEnumerable<Vec2> taken)
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
        public static bool Sees(MapGrid map, Unit e, Vec2 p) =>
            Vec2.Distance(e.Position, p) <= FogOfWar.DayEyeRange && Sight.Trace(map, e.Position, Postures.Height(e.Posture), p, Postures.Height(Posture.Standing)).Clear;

        // ---------- putting people where they belong ----------

        /// <summary>
        /// The best of 300 random spots in a ring around a point, where the start state allows, not on deep water and not
        /// within 15 m of anyone already placed; scored by the caller, with a little chance thrown in. Null if none fits.
        /// </summary>
        public static Vec2? RingSpot(MapGrid map, Dice rng, Vec2 around, float near, float far, Func<Vec2, bool> allowed, List<Vec2> taken, Func<Vec2, TerrainDef, float> score)
        {
            Vec2? best = null;
            float bestScore = float.MinValue;
            for (int k = 0; k < 300; k++)
            {
                float r = near + (far - near) * Next(rng);
                double a = Next(rng) * Math.PI * 2;
                var p = new Vec2(around.X + (float)Math.Cos(a) * r, around.Y + (float)Math.Sin(a) * r);
                if (!map.InBounds(p) || (allowed != null && !allowed(p))) continue;
                var t = map.TerrainAt(p);
                if (t.Deep || !t.Passable) continue;
                if (taken.Any(q => Vec2.Distance(q, p) < 15f)) continue;
                float s = score(p, t) + Next(rng) * 0.2f;
                if (s > bestScore) { bestScore = s; best = p; }
            }
            if (best != null) taken.Add(best.Value);
            return best;
        }

        /// <summary>A fighter of a kind, named and numbered by kind ("Rifleman 2"), facing a spot.</summary>
        public static Unit Fighter(GameData data, Force force, List<Unit> made, Dictionary<string, int> numbered, string def, string name, Vec2 at, Posture posture, Vec2 facing)
        {
            numbered[name] = numbered.TryGetValue(name, out int n) ? n + 1 : 1;
            var u = new Unit("enemy-" + (made.Count + 1), name + " " + numbered[name], data.FindUnit(def), force, at) { Posture = posture };
            u.Facing = Bearing(at, facing);
            made.Add(u);
            return u;
        }

        /// <summary>Civilians, one to a house, in the buildings given, up to a number.</summary>
        public static List<Unit> AddCivilians(Simulation sim, Force force, GameData data, Dice rng, IEnumerable<Landmark> homes, int count, int firstNumber = 1)
        {
            var shuffled = Shuffle(homes.ToList(), rng);
            var made = new List<Unit>();
            for (int i = 0; i < Math.Min(count, shuffled.Count); i++)
            {
                var u = new Unit("civilian-" + (firstNumber + i), "Civilian " + (firstNumber + i), data.FindUnit("civ"), force, shuffled[i].Center);
                sim.AddUnit(u);
                made.Add(u);
            }
            return made;
        }

        /// <summary>The village or town nearest a point, if the map has one.</summary>
        public static Landmark Settlement(MapGrid map, Vec2 near) =>
            map.Landmarks.Where(l => l.Kind == "area" && (l.Note == "village" || l.Note == "town")).OrderBy(l => Vec2.Distance(l.Center, near)).FirstOrDefault();

        public static IEnumerable<Landmark> BuildingsIn(MapGrid map, Landmark area) => map.Landmarks.Where(l => l.Kind == "building" && area.Contains(l.Center));

        // ---------- adding to the map ----------

        /// <summary>A named spot, like the pickup point, put on a map that's already in play.</summary>
        public static Landmark AddPoint(MapData mapData, MapGrid map, string label, Vec2 at, string note, params string[] aliases)
        {
            float sq = mapData.SquareSize;
            mapData.Features.Add(new MapFeature { Kind = "point", Label = label, Note = note, Aliases = aliases.ToList(), Points = new List<float[]> { new[] { at.X / sq, at.Y / sq } } });
            var mark = new Landmark(label, "point", aliases, note, at, at, at, null);
            map.AddLandmark(mark);
            return mark;
        }

        public static MapFeature LineFeature(MapData mapData, string terrain, float width, IEnumerable<Vec2> pts, string label = null, string note = null) => new MapFeature
        {
            Kind = "line", Terrain = terrain, Width = width, Label = label, Note = note, Aliases = new List<string>(),
            Points = pts.Select(p => new[] { p.X / mapData.SquareSize, p.Y / mapData.SquareSize }).ToList(),
        };

        public static MapFeature RectFeature(MapData mapData, string terrain, Vec2 min, Vec2 max, string label = null, string note = null, params string[] aliases) => new MapFeature
        {
            Kind = "area", Terrain = terrain, Label = label, Note = note, Aliases = aliases.ToList(),
            Rect = new[] { min.X / mapData.SquareSize, min.Y / mapData.SquareSize, (max.X - min.X) / mapData.SquareSize, (max.Y - min.Y) / mapData.SquareSize },
        };

        public static MapFeature AreaFeature(MapData mapData, string terrain, IEnumerable<Vec2> pts, string label = null, params string[] aliases) => new MapFeature
        {
            Kind = "area", Terrain = terrain, Label = label, Aliases = aliases.ToList(),
            Points = pts.Select(p => new[] { p.X / mapData.SquareSize, p.Y / mapData.SquareSize }).ToList(),
        };

        /// <summary>A building of a size, numbered after the map's own buildings.</summary>
        public static MapFeature BuildingFeature(MapData mapData, Vec2 center, float width, float depth)
        {
            int n = mapData.Features.Count(f => f.Kind == "building") + 1;
            string num = n.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return new MapFeature
            {
                Kind = "building", Terrain = "building", Label = "building " + num, Aliases = new List<string> { "bldg " + num, num },
                Center = new[] { center.X / mapData.SquareSize, center.Y / mapData.SquareSize }, Size = new[] { width, depth },
            };
        }

        /// <summary>
        /// The middle of a stretch of open, dry ground this big (plus 6 m around it), away from water, roads, buildings,
        /// and villages, at least this far in from every edge. Null if 200 tries find none.
        /// </summary>
        public static Vec2? OpenSite(MapGrid map, Dice rng, float halfWidth, float halfDepth, float margin, float fromSettlements = 120f)
        {
            var settlements = map.Landmarks.Where(l => l.Kind == "area" && (l.Note == "village" || l.Note == "town")).ToList();
            for (int k = 0; k < 200; k++)
            {
                var c = new Vec2(margin + (map.WidthMeters - 2 * margin) * Next(rng), margin + (map.HeightMeters - 2 * margin) * Next(rng));
                if (settlements.Any(s => Inside(s, c, fromSettlements))) continue;
                bool ok = true;
                for (float y = c.Y - halfDepth - 6f; y <= c.Y + halfDepth + 6f && ok; y += 2f)
                    for (float x = c.X - halfWidth - 6f; x <= c.X + halfWidth + 6f && ok; x += 2f)
                    {
                        var p = new Vec2(x, y);
                        ok = map.InBounds(p) && OpenGround.Contains(map.TerrainAt(p).Id);
                    }
                if (ok) return c;
            }
            return null;
        }

        static readonly HashSet<string> OpenGround = new HashSet<string> { "grass", "stubble", "plowed", "scrub", "corn", "orchard" };

        static bool Inside(Landmark area, Vec2 p, float margin) =>
            p.X >= area.Min.X - margin && p.X <= area.Max.X + margin && p.Y >= area.Min.Y - margin && p.Y <= area.Max.Y + margin;

        /// <summary>True within this many meters of the map's edge.</summary>
        public static bool AtEdge(MapGrid map, Vec2 p, float within) =>
            p.X <= within || p.Y <= within || p.X >= map.WidthMeters - within || p.Y >= map.HeightMeters - within;

        /// <summary>The spot on the map's edge straight out from a point in a direction.</summary>
        public static Vec2 EdgeToward(MapGrid map, Vec2 from, Vec2 dir)
        {
            dir = Unit(dir);
            float t = float.MaxValue;
            if (dir.X > 0.001f) t = Math.Min(t, (map.WidthMeters - 4f - from.X) / dir.X);
            if (dir.X < -0.001f) t = Math.Min(t, (4f - from.X) / dir.X);
            if (dir.Y > 0.001f) t = Math.Min(t, (map.HeightMeters - 4f - from.Y) / dir.Y);
            if (dir.Y < -0.001f) t = Math.Min(t, (4f - from.Y) / dir.Y);
            return Inside(map, from + dir * Math.Max(0f, t));
        }

        /// <summary>Sends someone on a route to a spot at a speed. False when there's no way there.</summary>
        public static bool Go(Simulation sim, Unit u, Vec2 to, string verb = Verbs.Move, MoveSpeed speed = MoveSpeed.Walk)
        {
            var path = sim.Paths.Find(u.Position, to, false);
            if (path == null) return false;
            u.Order = new Order { Verb = verb, Speed = speed == MoveSpeed.Run ? "run" : "walk", Point = to };
            u.Path = path;
            u.PathIndex = 0;
            u.Speed = speed;
            return true;
        }

        /// <summary>The usual enemy lines a prisoner says, and how the end puts a lie about them, for a set of goals.</summary>
        public static Dictionary<string, (string said, string truth)> Words(params (string goal, string said, string truth)[] list) =>
            list.ToDictionary(x => x.goal, x => (x.said, x.truth));

        // ---------- helpers ----------

        public static float Next(Dice d) => (d.NextUInt() >> 8) * (1f / 16777216f);

        public static int Between(Dice d, int lo, int hi) => lo + Math.Min(hi - lo, (int)(Next(d) * (hi - lo + 1)));

        public static List<T> Shuffle<T>(IList<T> list, Dice rng)
        {
            var copy = list.ToList();
            for (int i = copy.Count - 1; i > 0; i--)
            {
                int j = (int)(Next(rng) * (i + 1));
                var t = copy[i]; copy[i] = copy[j]; copy[j] = t;
            }
            return copy;
        }

        public static string Pick(List<string> pool, Dice rng) => pool.Count == 0 ? "Cinder" : pool[(int)(Next(rng) * pool.Count) % pool.Count];

        public static Vec2 Inside(MapGrid map, Vec2 p) =>
            new Vec2(Math.Max(3f, Math.Min(map.WidthMeters - 3f, p.X)), Math.Max(3f, Math.Min(map.HeightMeters - 3f, p.Y)));

        public static float Bearing(Vec2 a, Vec2 b) => (float)(Math.Atan2(b.X - a.X, -(b.Y - a.Y)) * 180.0 / Math.PI + 360.0) % 360f;

        public static float DistanceTo(Vec2 p, IReadOnlyList<Vec2> line)
        {
            float best = float.MaxValue;
            for (int i = 0; i + 1 < line.Count; i++) best = Math.Min(best, MapGrid.DistanceToSegment(p, line[i], line[i + 1]));
            return best;
        }

        /// <summary>The direction of the line's segment nearest a point.</summary>
        public static Vec2 Direction(IReadOnlyList<Vec2> line, Vec2 p)
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

        public static float Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Y * b.Y;

        /// <summary>A spot out along a way from a point, pulled in to 40 m short of the map's edge if that's nearer (but never under 150 m).</summary>
        public static Vec2 OutAlong(MapGrid map, Vec2 from, Vec2 way, float distance)
        {
            float room = Vec2.Distance(from, EdgeToward(map, from, way)) - 40f;
            return Inside(map, from + way * Math.Min(distance, Math.Max(150f, room)));
        }

        /// <summary>The compass word for a direction on the map, like "northeast". North is up the map.</summary>
        public static string Compass(Vec2 d)
        {
            double angle = (Math.Atan2(d.X, -d.Y) * 180.0 / Math.PI + 360.0) % 360.0;
            string[] names = { "north", "northeast", "east", "southeast", "south", "southwest", "west", "northwest" };
            return names[(int)Math.Round(angle / 45.0) % 8];
        }

        public static Vec2 Unit(Vec2 v)
        {
            float len = v.Length;
            return len > 0.01f ? v * (1f / len) : new Vec2(0, -1);
        }

        /// <summary>A point turned about another by an angle in radians.</summary>
        public static Vec2 Turn(Vec2 v, double angle) =>
            new Vec2((float)(v.X * Math.Cos(angle) - v.Y * Math.Sin(angle)), (float)(v.X * Math.Sin(angle) + v.Y * Math.Cos(angle)));
    }
}
