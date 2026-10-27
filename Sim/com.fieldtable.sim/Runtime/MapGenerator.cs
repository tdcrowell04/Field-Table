using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace FieldTable.Sim
{
    /// <summary>The 15 things a generated map can have. Any map may have some, all, or only a few.</summary>
    public static class MapFeatures
    {
        public const string Hills = "hills";
        public const string Ridgelines = "ridgelines";
        public const string Rivers = "rivers";
        public const string Creeks = "creeks";
        public const string Lakes = "lakes";
        public const string Marsh = "marsh";
        public const string Forest = "forest";
        public const string Woodlots = "woodlots";
        public const string Hedgerows = "hedgerows";
        public const string Fields = "fields";
        public const string Roads = "roads";
        public const string Bridges = "bridges";
        public const string Villages = "villages";
        public const string Towns = "towns";
        public const string Gullies = "gullies";

        public static readonly IReadOnlyList<string> All = new[]
        {
            Hills, Ridgelines, Rivers, Creeks, Lakes, Marsh, Forest, Woodlots,
            Hedgerows, Fields, Roads, Bridges, Villages, Towns, Gullies,
        };
    }

    /// <summary>
    /// What a mission asks the generator for. A start state can require features (a bridge seizure needs water
    /// and a bridge), and a player can ask the game master for specifics, like a hill of a certain height.
    /// </summary>
    public sealed class MapRequest
    {
        public string Name { get; set; } = "Generated";
        public ulong Seed { get; set; }
        public int Columns { get; set; } = 8;
        public int Rows { get; set; } = 6;
        public float SquareSize { get; set; } = 100f;
        public List<string> Require { get; set; } = new List<string>();
        public List<string> Exclude { get; set; } = new List<string>();

        /// <summary>A specific hill height the player asked for, in meters. Null leaves every hill random.</summary>
        public float? HillHeight { get; set; }

        /// <summary>
        /// True when the scenario is about having no cover. Otherwise every map gets common-sense cover and avenues of
        /// approach: a covered way in to each bridge from both banks, and no big stretch of bare open ground.
        /// </summary>
        public bool OpenGround { get; set; }
    }

    /// <summary>Names for generated places, loaded from data so each setting and mod can bring its own.</summary>
    public sealed class NamePools
    {
        public List<string> Places { get; set; } = new List<string>();
        public List<string> Waters { get; set; } = new List<string>();
        public List<string> Routes { get; set; } = new List<string>();
        public List<string> Woods { get; set; } = new List<string>();
        public List<string> Ridges { get; set; } = new List<string>();

        /// <summary>Last names for people: Marines, and anyone else a mission names.</summary>
        public List<string> People { get; set; } = new List<string>();

        public static NamePools FromJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<NamePools>(json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error })
                    ?? throw new FormatException("The names file is empty.");
            }
            catch (JsonException e)
            {
                throw new FormatException("The names file isn't valid: " + e.Message, e);
            }
        }
    }

    /// <summary>
    /// Builds a fresh map for a mission from building blocks. The same request and seed always give the same map,
    /// so a mission can be replayed; a new seed gives a different one.
    /// </summary>
    public static class MapGenerator
    {
        public const float MinHillHeight = 15f;
        public const float MaxHillHeight = 150f;

        /// <summary>Real building footprints in meters, from the building kit: shed, house, farmhouse, long house, barn, warehouse.</summary>
        static readonly float[][] Kit =
        {
            new[] { 6f, 5f }, new[] { 10f, 8f }, new[] { 14f, 10f }, new[] { 18f, 9f }, new[] { 20f, 12f }, new[] { 30f, 20f },
        };

        public static MapData Generate(MapRequest request, NamePools names)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (names == null) throw new ArgumentNullException(nameof(names));
            return new Builder(request, names).Run();
        }

        /// <summary>Generates and builds the playable map in one go.</summary>
        public static MapGrid GenerateMap(MapRequest request, NamePools names, IList<TerrainDef> terrains) =>
            Generate(request, names).Build(terrains);

        /// <summary>Random numbers for the generator, from the same kind of seeded roller as the dice.</summary>
        sealed class Rng
        {
            readonly Dice dice;
            public Rng(ulong seed) => dice = new Dice(seed, 7);
            public float Next() => (dice.NextUInt() >> 8) * (1f / 16777216f);
            public float Range(float a, float b) => a + (b - a) * Next();
            public int Int(int a, int bInclusive) => a + Math.Min(bInclusive - a, (int)(Next() * (bInclusive - a + 1)));
            public bool Chance(float p) => Next() < p;
            public T Pick<T>(IList<T> list) => list[Int(0, list.Count - 1)];
        }

        sealed class WaterLine
        {
            public List<Vec2> Points;
            public float HalfWidth;
            public string Label;
            public bool Deep;
        }

        sealed class Builder
        {
            readonly MapRequest req;
            readonly NamePools names;
            readonly Rng rng;
            readonly float w, h, sq;
            readonly HashSet<string> features = new HashSet<string>();
            readonly HashSet<string> made = new HashSet<string>();
            readonly HashSet<string> usedNames = new HashSet<string>();

            readonly List<WaterLine> waterLines = new List<WaterLine>();
            readonly List<(Vec2 c, float r)> lakes = new List<(Vec2, float)>();
            readonly List<(List<Vec2> pts, float half, string label)> roads = new List<(List<Vec2>, float, string)>();
            readonly List<(Vec2 min, Vec2 max)> settlements = new List<(Vec2, Vec2)>();
            readonly List<(Vec2 min, Vec2 max)> buildings = new List<(Vec2, Vec2)>();
            readonly List<(Vec2 min, Vec2 max)> fieldPatches = new List<(Vec2, Vec2)>();

            // Features in the order they get painted: ground shape first, water and roads over the land, buildings on top.
            readonly List<MapFeature> hills = new List<MapFeature>();
            readonly List<MapFeature> ridges = new List<MapFeature>();
            readonly List<MapFeature> fields = new List<MapFeature>();
            readonly List<MapFeature> hedges = new List<MapFeature>();
            readonly List<MapFeature> woods = new List<MapFeature>();
            readonly List<MapFeature> marshes = new List<MapFeature>();
            readonly List<MapFeature> gullies = new List<MapFeature>();
            readonly List<MapFeature> water = new List<MapFeature>();
            readonly List<MapFeature> ways = new List<MapFeature>();
            readonly List<MapFeature> bridges = new List<MapFeature>();
            readonly List<MapFeature> yards = new List<MapFeature>();
            readonly List<MapFeature> structures = new List<MapFeature>();
            readonly List<MapFeature> cliffs = new List<MapFeature>();

            public Builder(MapRequest request, NamePools pools)
            {
                req = request;
                names = pools;
                if (req.Columns < 1 || req.Rows < 1 || req.Columns > 26) throw new ArgumentException("A generated map needs 1 to 26 columns and at least 1 row.");
                if (req.SquareSize <= 0f) throw new ArgumentException("Grid squares need a size above 0.");
                if (req.HillHeight.HasValue && (req.HillHeight < MinHillHeight || req.HillHeight > MaxHillHeight))
                    throw new ArgumentException("A hill can be " + MinHillHeight + " to " + MaxHillHeight + " meters high.");
                rng = new Rng(req.Seed);
                sq = req.SquareSize;
                w = req.Columns * sq;
                h = req.Rows * sq;
            }

            public MapData Run()
            {
                PickFeatures();

                // Water first, then the roads that cross it, then the places people live, then the land around them.
                bool riverRunsNorthSouth = rng.Chance(0.5f);
                if (Has(MapFeatures.Rivers)) MakeRiver(riverRunsNorthSouth);
                if (Has(MapFeatures.Creeks)) MakeCreeks(riverRunsNorthSouth);
                if (Has(MapFeatures.Roads)) MakeRoads(riverRunsNorthSouth);
                if (Has(MapFeatures.Bridges)) MakeBridges();
                if (Has(MapFeatures.Lakes)) MakeLakes();
                if (Has(MapFeatures.Towns)) MakeTown();
                if (Has(MapFeatures.Villages)) MakeVillages();
                if (Has(MapFeatures.Roads) && (Has(MapFeatures.Villages) || Has(MapFeatures.Towns))) MakeTrails();
                if (Has(MapFeatures.Hills)) MakeHills();
                if (Has(MapFeatures.Ridgelines)) MakeRidge();
                if (Has(MapFeatures.Fields)) MakeFields();
                if (Has(MapFeatures.Hedgerows)) MakeHedgerows();
                if (Has(MapFeatures.Forest)) MakeForest();
                if (Has(MapFeatures.Woodlots)) MakeWoodlots();
                if (Has(MapFeatures.Marsh)) MakeMarsh();
                if (Has(MapFeatures.Gullies)) MakeGullies();
                if (!req.OpenGround) MakeCover();
                NumberBuildings();

                if (water.Any(f => f.Terrain == "river")) made.Add(MapFeatures.Rivers);
                if (water.Any(f => f.Terrain == "creek")) made.Add(MapFeatures.Creeks);
                if (water.Any(f => f.Terrain == "lake")) made.Add(MapFeatures.Lakes);
                if (roads.Any(r => r.label.StartsWith("Route ", StringComparison.Ordinal))) made.Add(MapFeatures.Roads);
                if (bridges.Count > 0) made.Add(MapFeatures.Bridges);
                if (hills.Count > 0) made.Add(MapFeatures.Hills);
                if (ridges.Count > 0) made.Add(MapFeatures.Ridgelines);
                if (fields.Count > 0) made.Add(MapFeatures.Fields);
                if (hedges.Count > 0) made.Add(MapFeatures.Hedgerows);
                if (marshes.Count > 0) made.Add(MapFeatures.Marsh);
                if (gullies.Count > 0) made.Add(MapFeatures.Gullies);
                foreach (string f in req.Require)
                    if (!made.Contains(f)) throw new InvalidOperationException("Couldn't fit the required " + f + " on this map. Try a bigger map or a different seed.");

                var all = new List<MapFeature>();
                foreach (var list in new[] { hills, ridges, fields, hedges, woods, marshes, gullies, water, yards, ways, bridges, cliffs, structures })
                    all.AddRange(list);
                return new MapData
                {
                    Includes = MapFeatures.All.Where(made.Contains).ToList(),
                    Name = req.Name,
                    Columns = req.Columns,
                    Rows = req.Rows,
                    SquareSize = sq,
                    CellSize = 1f,
                    BaseTerrain = "grass",
                    Features = all,
                };
            }

            bool Has(string f) => features.Contains(f);

            void PickFeatures()
            {
                foreach (var f in req.Require.Concat(req.Exclude))
                    if (!MapFeatures.All.Contains(f)) throw new ArgumentException("Unknown map feature: " + f + ".");
                foreach (var f in req.Require)
                    if (req.Exclude.Contains(f)) throw new ArgumentException("A map can't both require and exclude " + f + ".");

                foreach (var f in req.Require) features.Add(f);
                if (req.HillHeight.HasValue) Add(MapFeatures.Hills);

                // A random mix: some maps get many features, some only a few.
                int target = rng.Int(3, 11);
                var open = MapFeatures.All.Where(f => !features.Contains(f) && !req.Exclude.Contains(f)).ToList();
                while (features.Count < target && open.Count > 0)
                {
                    int i = rng.Int(0, open.Count - 1);
                    features.Add(open[i]);
                    open.RemoveAt(i);
                }

                // Some features need others to make sense.
                if (Has(MapFeatures.Bridges))
                {
                    Add(MapFeatures.Roads);
                    if (!Has(MapFeatures.Rivers) && !Has(MapFeatures.Creeks))
                    {
                        bool river = req.Exclude.Contains(MapFeatures.Creeks) || (!req.Exclude.Contains(MapFeatures.Rivers) && rng.Chance(0.5f));
                        Add(river ? MapFeatures.Rivers : MapFeatures.Creeks);
                    }
                }
                if (Has(MapFeatures.Villages) || Has(MapFeatures.Towns)) Add(MapFeatures.Roads);
                if (Has(MapFeatures.Hedgerows)) Add(MapFeatures.Fields);
            }

            void Add(string f)
            {
                if (req.Exclude.Contains(f)) throw new ArgumentException("This map needs " + f + ", but the request excludes it.");
                features.Add(f);
            }

            // ---------- water ----------

            void MakeRiver(bool northSouth)
            {
                float width = rng.Range(20f, 45f);
                var pts = Meander(northSouth, 0.25f, 0.75f, 50f);
                string label = Name(names.Waters, " River");
                waterLines.Add(new WaterLine { Points = pts, HalfWidth = width / 2f, Label = label, Deep = true });
                water.Add(Line(label, "river", width, pts, "too deep to wade", "the river"));
            }

            void MakeCreeks(bool riverNorthSouth)
            {
                int count = rng.Int(1, 2);
                for (int i = 0; i < count; i++)
                {
                    // Creeks run the other way from a river when there is one, so they feed into it.
                    bool northSouth = Has(MapFeatures.Rivers) ? !riverNorthSouth : rng.Chance(0.5f);
                    float width = rng.Range(4f, 10f);
                    var pts = Meander(northSouth, 0.15f, 0.85f, 35f);
                    string label = Name(names.Waters, " Creek");
                    waterLines.Add(new WaterLine { Points = pts, HalfWidth = width / 2f, Label = label });
                    water.Add(Line(label, "creek", width, pts, "knee deep, fordable", count == 1 ? "the creek" : null));
                }
            }

            void MakeLakes()
            {
                int count = rng.Int(1, 2);
                for (int i = 0; i < count; i++)
                {
                    float r = rng.Range(30f, 150f);
                    Vec2 c;
                    while (!TryPlace(r + 15f, out c) && r > 20f) r *= 0.7f;
                    if (r <= 20f) continue;
                    lakes.Add((c, r));
                    string label = r < 70f ? Name(names.Waters, " Pond") : "Lake " + Name(names.Waters, "");
                    water.Add(Area(label, "lake", Blob(c, r)));
                }
            }

            // ---------- roads and bridges ----------

            void MakeRoads(bool riverNorthSouth)
            {
                int count = rng.Int(1, 2);
                bool needCrossing = Has(MapFeatures.Bridges) && waterLines.Count > 0;
                for (int i = 0; i < count; i++)
                {
                    // Without bridges, roads keep to one bank of a river instead of crossing it.
                    bool northSouth;
                    if (needCrossing && i == 0) northSouth = !OrientationOf(waterLines[0].Points);
                    else if (Has(MapFeatures.Rivers) && !Has(MapFeatures.Bridges)) northSouth = riverNorthSouth;
                    else northSouth = rng.Chance(0.5f);

                    List<Vec2> pts = Meander(northSouth, 0.15f, 0.85f, 25f);
                    if (Has(MapFeatures.Rivers) && !Has(MapFeatures.Bridges))
                    {
                        // Try a few times for a road that stays on one bank; skip it if the river wanders too much.
                        pts = null;
                        for (int attempt = 0; attempt < 10 && pts == null; attempt++)
                        {
                            var tryPts = KeepToOneBank(Meander(northSouth, 0.15f, 0.85f, 25f), northSouth);
                            if (!Crosses(tryPts, waterLines[0].Points, waterLines[0].HalfWidth + 6f)) pts = tryPts;
                        }
                        if (pts == null) continue;
                    }
                    string label = "Route " + Name(names.Routes, "");
                    roads.Add((pts, 3f, label));
                    ways.Add(Line(label, "road", 6f, pts, null, null));
                }
            }

            static bool OrientationOf(List<Vec2> pts) => Math.Abs(pts[pts.Count - 1].Y - pts[0].Y) > Math.Abs(pts[pts.Count - 1].X - pts[0].X);

            List<Vec2> KeepToOneBank(List<Vec2> road, bool northSouth)
            {
                var river = waterLines[0];
                float riverAt = northSouth ? river.Points.Average(p => p.X) : river.Points.Average(p => p.Y);
                float size = northSouth ? w : h;
                bool low = riverAt > size / 2f;
                float target = low ? rng.Range(0.1f, 0.3f) * (riverAt - river.HalfWidth) : riverAt + river.HalfWidth + rng.Range(0.3f, 0.7f) * (size - riverAt - river.HalfWidth);
                float shift = target - (northSouth ? road.Average(p => p.X) : road.Average(p => p.Y));
                return road.Select(p => northSouth ? new Vec2(p.X + shift, p.Y) : new Vec2(p.X, p.Y + shift)).ToList();
            }

            void MakeBridges()
            {
                int n = 0;
                var made = new List<MapFeature>();
                foreach (var road in roads)
                    foreach (var wl in waterLines)
                        for (int i = 0; i + 1 < road.pts.Count; i++)
                            for (int j = 0; j + 1 < wl.Points.Count; j++)
                            {
                                if (!Intersect(road.pts[i], road.pts[i + 1], wl.Points[j], wl.Points[j + 1], out Vec2 at)) continue;
                                Vec2 dir = Unit(road.pts[i + 1] - road.pts[i]);
                                Vec2 flow = Unit(wl.Points[j + 1] - wl.Points[j]);
                                float sin = Math.Abs(dir.X * flow.Y - dir.Y * flow.X);
                                float half = wl.HalfWidth / Math.Max(0.3f, sin) + 6f;
                                n++;
                                // Named for the water it crosses, and never the same name twice.
                                string name = wl.Label + " bridge";
                                for (int k = 2; usedNames.Contains(name); k++) name = wl.Label + " bridge " + k;
                                usedNames.Add(name);
                                made.Add(Line(name, "bridge", 5f, new List<Vec2> { at - dir * half, at + dir * half }, "on " + road.label, "bridge " + n));
                            }
                if (made.Count == 1) made[0].Aliases.AddRange(new[] { "the bridge", "bridge" });
                bridges.AddRange(made);
            }

            void MakeTrails()
            {
                int count = rng.Int(1, 2);
                for (int i = 0, tries = 0; i < count && tries < 20 && settlements.Count > 0; tries++)
                {
                    // A footpath from the edge of a village or town out to the map edge. It goes around deep water, never through it.
                    var s = rng.Pick(settlements);
                    Vec2 mid = (s.min + s.max) * 0.5f;
                    Vec2 to = EdgePoint();
                    Vec2 from = EdgeOfRect(s.min, s.max, mid, to);
                    var pts = new List<Vec2> { from };
                    for (int k = 1; k < 4; k++)
                    {
                        Vec2 p = from + (to - from) * (k / 4f);
                        pts.Add(new Vec2(p.X + rng.Range(-30f, 30f), p.Y + rng.Range(-30f, 30f)));
                    }
                    pts.Add(to);
                    if (CrossesDeepWater(pts) || pts.Skip(1).Any(p => InSettlement(p, 0f))) continue;
                    string label = Name(names.Woods, " Trail");
                    ways.Add(Line(label, "trail", 2f, pts, null, null));
                    i++;
                }
            }

            // ---------- places people live ----------

            void MakeVillages()
            {
                int count = rng.Int(1, 2);
                for (int v = 0; v < count; v++)
                {
                    // A village sits beside a road. If the spot is taken or wet, try another, a little smaller each time.
                    float scale = 1f;
                    Vec2 min = default, max = default;
                    bool placed = false;
                    for (int attempt = 0; attempt < 100 && !placed; attempt++, scale = Math.Max(0.4f, scale * 0.95f))
                    {
                        Vec2 center = attempt < 30
                            ? PointAlong(rng.Pick(roads).pts, rng.Range(0.1f, 0.9f))
                            : new Vec2(rng.Range(0f, w), rng.Range(0f, h));
                        float hw = rng.Range(70f, 120f) * scale, hd = rng.Range(60f, 100f) * scale;
                        min = new Vec2(Clamp(center.X - hw, 0, Math.Max(0, w - 2 * hw)), Clamp(center.Y - hd, 0, Math.Max(0, h - 2 * hd)));
                        max = min + new Vec2(2 * hw, 2 * hd);
                        placed = !OverlapsSettlement(min, max) && !WaterIn(min, max);
                    }
                    if (!placed) continue;
                    made.Add(MapFeatures.Villages);
                    settlements.Add((min, max));
                    string label = Name(names.Places, "");
                    yards.Add(Rect(label, "area", "yard", min, max, "village"));
                    int houses = rng.Int(4, 9);
                    for (int b = 0; b < houses * 4 && CountIn(min, max) < houses; b++)
                        TryBuilding(new Vec2(rng.Range(min.X, max.X), rng.Range(min.Y, max.Y)), rng.Int(0, 4), min, max);
                }
            }

            void MakeTown()
            {
                float half = rng.Range(120f, 190f);
                for (int attempt = 0; attempt < 100; attempt++, half = attempt < 30 ? half : Math.Max(60f, half * 0.95f))
                {
                    // A town grows along a road, but if every spot on the road is wet, it can sit off it.
                    Vec2 c = attempt < 30
                        ? PointAlong(rng.Pick(roads).pts, rng.Range(0.25f, 0.75f))
                        : new Vec2(rng.Range(0f, w), rng.Range(0f, h));
                    Vec2 min = new Vec2(Clamp(c.X - half, 0, w - 2 * half), Clamp(c.Y - half, 0, h - 2 * half));
                    Vec2 max = min + new Vec2(2 * half, 2 * half);
                    if (max.X > w || max.Y > h || OverlapsSettlement(min, max) || WaterIn(min, max)) continue;

                    made.Add(MapFeatures.Towns);
                    settlements.Add((min, max));
                    string label = Name(names.Places, "");
                    yards.Add(Rect(label, "area", "yard", min, max, "town"));

                    // A grid of streets, then buildings filling the blocks between them.
                    float block = rng.Range(55f, 75f);
                    var streetsX = new List<float>();
                    var streetsY = new List<float>();
                    for (float x = min.X + block; x < max.X - 10f; x += block) streetsX.Add(x);
                    for (float y = min.Y + block; y < max.Y - 10f; y += block) streetsY.Add(y);
                    foreach (float x in streetsX)
                    {
                        var pts = new List<Vec2> { new Vec2(x, min.Y), new Vec2(x, max.Y) };
                        roads.Add((pts, 3f, label + " street"));
                        ways.Add(Line(null, "road", 6f, pts, null, null));
                    }
                    foreach (float y in streetsY)
                    {
                        var pts = new List<Vec2> { new Vec2(min.X, y), new Vec2(max.X, y) };
                        roads.Add((pts, 3f, label + " street"));
                        ways.Add(Line(null, "road", 6f, pts, null, null));
                    }

                    var xs = new List<float> { min.X }; xs.AddRange(streetsX); xs.Add(max.X);
                    var ys = new List<float> { min.Y }; ys.AddRange(streetsY); ys.Add(max.Y);
                    for (int i = 0; i + 1 < xs.Count; i++)
                        for (int j = 0; j + 1 < ys.Count; j++)
                        {
                            Vec2 bMin = new Vec2(xs[i] + 5f, ys[j] + 5f), bMax = new Vec2(xs[i + 1] - 5f, ys[j + 1] - 5f);
                            if (bMax.X - bMin.X < 12f || bMax.Y - bMin.Y < 12f) continue;
                            int want = rng.Int(2, 6);
                            for (int k = 0; k < want * 8 && CountIn(bMin, bMax) < want; k++)
                                TryBuilding(new Vec2(rng.Range(bMin.X, bMax.X), rng.Range(bMin.Y, bMax.Y)), rng.Int(1, 5), bMin, bMax);
                        }
                    return;
                }
            }

            void TryBuilding(Vec2 center, int kitIndex, Vec2 areaMin, Vec2 areaMax)
            {
                float[] size = Kit[kitIndex];
                float bw = size[0], bd = size[1];
                if (rng.Chance(0.5f)) { float t = bw; bw = bd; bd = t; }
                Vec2 half = new Vec2(bw / 2f, bd / 2f);
                Vec2 min = center - half, max = center + half;
                if (min.X < areaMin.X || min.Y < areaMin.Y || max.X > areaMax.X || max.Y > areaMax.Y) return;
                if (min.X < 1f || min.Y < 1f || max.X > w - 1f || max.Y > h - 1f) return;
                foreach (var b in buildings)
                    if (min.X < b.max.X + 4f && max.X > b.min.X - 4f && min.Y < b.max.Y + 4f && max.Y > b.min.Y - 4f) return;
                foreach (var r in roads)
                    for (int i = 0; i + 1 < r.pts.Count; i++)
                        if (MapGrid.DistanceToSegment(center, r.pts[i], r.pts[i + 1]) < r.half + Math.Max(bw, bd) / 2f + 3f) return;
                if (NearWater(center, Math.Max(bw, bd) / 2f + 8f)) return;
                buildings.Add((min, max));
                structures.Add(new MapFeature
                {
                    Kind = "building",
                    Terrain = "building",
                    Center = new[] { center.X / sq, center.Y / sq },
                    Size = new[] { bw, bd },
                    Aliases = new List<string>(),
                });
            }

            int CountIn(Vec2 min, Vec2 max) => buildings.Count(b => b.min.X >= min.X && b.max.X <= max.X && b.min.Y >= min.Y && b.max.Y <= max.Y);

            /// <summary>Numbers buildings 1, 2, 3 from north to south, then west to east, like the prototype's maps.</summary>
            void NumberBuildings()
            {
                var ordered = structures.OrderBy(f => Math.Floor(f.Center[1] * sq / 50f)).ThenBy(f => f.Center[0]).ToList();
                for (int i = 0; i < ordered.Count; i++)
                {
                    string n = (i + 1).ToString(CultureInfo.InvariantCulture);
                    ordered[i].Label = "building " + n;
                    ordered[i].Aliases = new List<string> { "bldg " + n, n };
                }
            }

            // ---------- the land ----------

            void MakeHills()
            {
                int count = rng.Int(1, 3);
                for (int i = 0; i < count; i++)
                {
                    float height = i == 0 && req.HillHeight.HasValue ? req.HillHeight.Value : (float)Math.Round(rng.Range(MinHillHeight, MaxHillHeight));
                    float rx = 60f + height * rng.Range(1.2f, 2.2f), ry = 60f + height * rng.Range(1.2f, 2.2f);
                    Vec2 c = new Vec2(rng.Range(0f, w), rng.Range(0f, h));
                    string label = "Hill " + Math.Round(height).ToString(CultureInfo.InvariantCulture);
                    while (usedNames.Contains(label)) label += "A";
                    usedNames.Add(label);
                    hills.Add(new MapFeature
                    {
                        Kind = "hill",
                        Label = label,
                        Note = "+" + Math.Round(height).ToString(CultureInfo.InvariantCulture) + " m",
                        Terrain = rng.Chance(0.4f) ? "scrub" : null,
                        Rect = new[] { (c.X - rx) / sq, (c.Y - ry) / sq, 2 * rx / sq, 2 * ry / sq },
                        Height = height,
                    });
                }
            }

            void MakeRidge()
            {
                bool northSouth = rng.Chance(0.5f);
                var pts = Meander(northSouth, 0.15f, 0.85f, 40f);
                // A ridge doesn't always run the whole map.
                int drop = rng.Int(0, 2);
                for (int i = 0; i < drop && pts.Count > 3; i++) pts.RemoveAt(rng.Chance(0.5f) ? 0 : pts.Count - 1);
                float height = (float)Math.Round(rng.Range(20f, 90f));
                float width = rng.Range(120f, 240f);
                string label = Name(names.Ridges, " Ridge");
                ridges.Add(new MapFeature { Kind = "ridge", Label = label, Note = "+" + height + " m", Terrain = rng.Chance(0.5f) ? "scrub" : null, PaintWidth = width * 0.6f, Width = width, Height = height, Points = Squares(pts) });

                if (rng.Chance(0.5f))
                {
                    // A cliff along part of one face of the ridge, broken wherever a road, a settlement, or water crosses it.
                    float side = rng.Chance(0.5f) ? 1f : -1f;
                    var face = new List<Vec2>();
                    for (int i = 0; i < pts.Count; i++)
                    {
                        Vec2 dir = Unit(pts[Math.Min(i + 1, pts.Count - 1)] - pts[Math.Max(i - 1, 0)]);
                        Vec2 normal = new Vec2(-dir.Y, dir.X) * side;
                        face.Add(pts[i] + normal * (width * 0.22f));
                    }
                    int start = rng.Int(0, Math.Max(0, face.Count - 3));
                    var piece = Subdivide(face.Skip(start).Take(rng.Int(2, Math.Max(2, face.Count - start))).ToList(), 20f);
                    var runs = new List<List<Vec2>>();
                    var run = new List<Vec2>();
                    for (int i = 0; i + 1 < piece.Count; i++)
                    {
                        Vec2 mid = (piece[i] + piece[i + 1]) * 0.5f;
                        bool clear = !NearRoad(mid, 12f) && !InSettlement(mid, 10f) && !NearWater(mid, 5f);
                        if (clear)
                        {
                            if (run.Count == 0) run.Add(piece[i]);
                            run.Add(piece[i + 1]);
                        }
                        else if (run.Count > 0)
                        {
                            runs.Add(run);
                            run = new List<Vec2>();
                        }
                    }
                    if (run.Count > 0) runs.Add(run);
                    runs = runs.Where(r => r.Count >= 3).OrderByDescending(r => r.Count).ToList();
                    for (int i = 0; i < runs.Count; i++)
                        cliffs.Add(Line(i == 0 ? label + " cliffs" : null, "cliff", 3f, runs[i], "sheer rock", null));
                }
            }

            void MakeFields()
            {
                // Patches of farmland in a loose grid, each plowed, stubble, tall crops, or left as pasture.
                float size = rng.Range(110f, 180f);
                string[] kinds = { "plowed", "stubble", "corn", "grass" };
                for (float y = 0; y < h; y += size)
                    for (float x = 0; x < w; x += size)
                    {
                        if (!rng.Chance(0.55f)) continue;
                        Vec2 min = new Vec2(x + rng.Range(0f, 15f), y + rng.Range(0f, 15f));
                        Vec2 max = new Vec2(Math.Min(w, x + size - rng.Range(0f, 15f)), Math.Min(h, y + size - rng.Range(0f, 15f)));
                        if (max.X - min.X < 30f || max.Y - min.Y < 30f) continue;
                        string kind = rng.Pick(kinds);
                        fieldPatches.Add((min, max));
                        fields.Add(Rect(null, "area", kind, min, max, null));
                    }
            }

            void MakeHedgerows()
            {
                foreach (var f in fieldPatches)
                {
                    var corners = new[] { f.min, new Vec2(f.max.X, f.min.Y), f.max, new Vec2(f.min.X, f.max.Y) };
                    for (int i = 0; i < 4; i++)
                    {
                        if (!rng.Chance(0.4f)) continue;
                        Vec2 a = corners[i], b = corners[(i + 1) % 4];
                        Vec2 mid = (a + b) * 0.5f;
                        if (InSettlement(mid, 10f)) continue;
                        bool wall = rng.Chance(0.35f);
                        hedges.Add(Line(null, wall ? "wall" : "hedge", wall ? 1f : 3f, new List<Vec2> { a, b }, null, null));
                    }
                }
            }

            void MakeForest()
            {
                int count = rng.Int(1, 2);
                for (int i = 0, tries = 0; i < count && tries < 40; tries++)
                {
                    float r = rng.Range(110f, 260f) * (tries < 10 ? 1f : 0.5f);
                    Vec2 c = new Vec2(rng.Range(0f, w), rng.Range(0f, h));
                    if (InSettlement(c, r * 0.8f)) continue;
                    made.Add(MapFeatures.Forest);
                    woods.Add(Area(Name(names.Woods, " Wood"), "forest", Blob(c, r)));
                    i++;
                }
            }

            void MakeWoodlots()
            {
                int count = rng.Int(2, 5);
                for (int i = 0, tries = 0; i < count && tries < 60; tries++)
                {
                    Vec2 c = new Vec2(rng.Range(0f, w), rng.Range(0f, h));
                    if (InSettlement(c, 40f) || NearWater(c, 30f)) continue;
                    i++;
                    made.Add(MapFeatures.Woodlots);
                    if (rng.Chance(0.35f))
                    {
                        float hw = rng.Range(30f, 70f), hd = rng.Range(30f, 70f);
                        woods.Add(Rect(null, "area", "orchard", c - new Vec2(hw, hd), c + new Vec2(hw, hd), "orchard"));
                    }
                    else
                    {
                        woods.Add(Area(null, "forest", Blob(c, rng.Range(20f, 60f))));
                    }
                }
            }

            void MakeMarsh()
            {
                int count = rng.Int(1, 2);
                for (int i = 0, tries = 0; i < count && tries < 40; tries++)
                {
                    // Reeds grow along water when there is some.
                    Vec2 c = waterLines.Count > 0 && tries < 20
                        ? PointAlong(rng.Pick(waterLines).Points, rng.Range(0.1f, 0.9f))
                        : new Vec2(rng.Range(0f, w), rng.Range(0f, h));
                    if (InSettlement(c, 40f)) continue;
                    i++;
                    marshes.Add(Area(rng.Chance(0.5f) ? Name(names.Waters, " Reeds") : null, "marsh", Blob(c, rng.Range(40f, 110f))));
                }
            }

            void MakeGullies()
            {
                int count = rng.Int(1, 2);
                for (int i = 0, tries = 0; i < count && tries < 40; tries++)
                {
                    Vec2 a = new Vec2(rng.Range(0f, w), rng.Range(0f, h));
                    float len = rng.Range(200f, 450f) * (tries < 20 ? 1f : 0.5f), ang = rng.Range(0f, (float)(2 * Math.PI));
                    var pts = new List<Vec2> { a };
                    for (int k = 1; k <= 4; k++)
                    {
                        ang += rng.Range(-0.5f, 0.5f);
                        pts.Add(pts[k - 1] + new Vec2((float)Math.Cos(ang), (float)Math.Sin(ang)) * (len / 4f));
                    }
                    if (pts.Any(p => InSettlement(p, 15f))) continue;
                    i++;
                    float width = rng.Range(10f, 22f);
                    gullies.Add(new MapFeature { Kind = "gully", Label = Name(names.Woods, " Gully"), Note = "dry wash", Terrain = "gully", Width = width, PaintWidth = width * 0.5f, Height = rng.Range(2f, 5f), Points = Squares(pts) });
                }
            }

            // ---------- common-sense cover ----------

            // Kinds of ground that block sight or hide someone, so a person can get out of view behind or in them.
            static readonly HashSet<string> CoverTerrains = new HashSet<string> { "forest", "hedge", "wall", "corn", "marsh", "gully", "building", "yard", "cliff" };

            /// <summary>
            /// Cover where common sense says there'd be some: each bridge gets a covered way in from both banks, starting
            /// from a wood, a hedged field, or a fold in the ground about 300 m back, and then any 100 m square with
            /// nothing to hide behind nearby gets a copse or a hedge.
            /// </summary>
            void MakeCover()
            {
                foreach (var b in bridges.ToList())
                {
                    Vec2 a = new Vec2(b.Points[0][0] * sq, b.Points[0][1] * sq), z = new Vec2(b.Points[1][0] * sq, b.Points[1][1] * sq);
                    Vec2 mid = (a + z) * 0.5f, along = Unit(z - a);
                    float half = Vec2.Distance(a, z) / 2f;
                    MakeApproach(mid, along, half);
                    MakeApproach(mid, along * -1f, half);
                }
                BreakUpOpenGround();
            }

            bool Allowed(string f) => !req.Exclude.Contains(f);

            /// <summary>Cover about 300 m back from one end of a bridge, and a covered way from it toward the bridge.</summary>
            void MakeApproach(Vec2 bridge, Vec2 toBank, float half)
            {
                Vec2 bank = bridge + toBank * (half + 12f);
                Vec2 start = default;
                bool found = false;
                for (int tries = 0; tries < 16 && !found; tries++)
                {
                    float turn = rng.Range(-0.6f, 0.6f), dist = rng.Range(260f, 360f);
                    Vec2 dir = new Vec2(toBank.X * (float)Math.Cos(turn) - toBank.Y * (float)Math.Sin(turn), toBank.X * (float)Math.Sin(turn) + toBank.Y * (float)Math.Cos(turn));
                    start = bridge + dir * dist;
                    if (start.X < 40f || start.Y < 40f || start.X > w - 40f || start.Y > h - 40f) continue;
                    // It has to be on this bank: nothing wet between the bridge's end and the spot.
                    found = !Subdivide(new List<Vec2> { bank, start }, 8f).Any(p => NearWater(p, 2f));
                }
                if (!found) return;

                // Cover to start from, unless a village or town is already there.
                if (!InSettlement(start, 20f))
                {
                    var kinds = new List<string>();
                    if (Allowed(MapFeatures.Forest) || Allowed(MapFeatures.Woodlots)) kinds.Add("woods");
                    if (Allowed(MapFeatures.Hedgerows)) kinds.Add("hedged field");
                    if (Allowed(MapFeatures.Hills)) kinds.Add("fold");
                    if (kinds.Count == 0) return;
                    switch (rng.Pick(kinds))
                    {
                        case "woods":
                            woods.Add(Area(Name(names.Woods, " Wood"), "forest", Blob(start, rng.Range(45f, 75f))));
                            made.Add(Allowed(MapFeatures.Forest) ? MapFeatures.Forest : MapFeatures.Woodlots);
                            break;
                        case "hedged field":
                        {
                            float hw = rng.Range(40f, 60f), hd = rng.Range(40f, 60f);
                            Vec2 min = start - new Vec2(hw, hd), max = start + new Vec2(hw, hd);
                            if (Allowed(MapFeatures.Fields)) { fields.Add(Rect(null, "area", rng.Pick(new[] { "grass", "stubble", "plowed" }), min, max, null)); made.Add(MapFeatures.Fields); }
                            var corners = new[] { min, new Vec2(max.X, min.Y), max, new Vec2(min.X, max.Y) };
                            // A gap in one side to get in by.
                            int gap = rng.Int(0, 3);
                            for (int i = 0; i < 4; i++)
                            {
                                Vec2 c0 = corners[i], c1 = corners[(i + 1) % 4];
                                if (i == gap)
                                {
                                    hedges.Add(Line(null, "hedge", 3f, new List<Vec2> { c0, c0 + (c1 - c0) * 0.42f }, null, null));
                                    hedges.Add(Line(null, "hedge", 3f, new List<Vec2> { c0 + (c1 - c0) * 0.58f, c1 }, null, null));
                                }
                                else hedges.Add(Line(null, "hedge", 3f, new List<Vec2> { c0, c1 }, null, null));
                            }
                            made.Add(MapFeatures.Hedgerows);
                            break;
                        }
                        default:
                        {
                            // A low rise between the bridge and the spot: the ground behind it is out of sight of the river.
                            float height = (float)Math.Round(rng.Range(MinHillHeight, 22f));
                            Vec2 c = bridge + (start - bridge) * 0.62f;
                            float r = rng.Range(55f, 75f);
                            string label = "Hill " + height.ToString(CultureInfo.InvariantCulture);
                            while (usedNames.Contains(label)) label += "A";
                            usedNames.Add(label);
                            hills.Add(new MapFeature { Kind = "hill", Label = label, Note = "+" + height.ToString(CultureInfo.InvariantCulture) + " m", Rect = new[] { (c.X - r) / sq, (c.Y - r) / sq, 2 * r / sq, 2 * r / sq }, Height = height });
                            made.Add(MapFeatures.Hills);
                            break;
                        }
                    }
                }

                // The way in: from the cover toward the bridge, bending so no one can see straight down it, and stopping
                // short of the water.
                Vec2 end = bridge + Unit(start - bridge) * rng.Range(100f, 140f);
                Vec2 side = new Vec2(-(end - start).Y, (end - start).X);
                side = Unit(side);
                var route = new List<Vec2> { start };
                for (int k = 1; k < 4; k++) route.Add(start + (end - start) * (k / 4f) + side * rng.Range(-25f, 25f));
                route.Add(end);
                if (Subdivide(route, 8f).Any(p => NearWater(p, 4f) || InSettlement(p, 0f))) return;

                var wayKinds = new List<string>();
                if (Allowed(MapFeatures.Hedgerows)) { wayKinds.Add("hedgerow"); wayKinds.Add("lane"); }
                if (Allowed(MapFeatures.Forest) || Allowed(MapFeatures.Woodlots)) wayKinds.Add("tree line");
                if (Allowed(MapFeatures.Gullies)) wayKinds.Add("gully");
                if (wayKinds.Count == 0) return;
                switch (rng.Pick(wayKinds))
                {
                    case "hedgerow":
                        hedges.Add(Line(null, "hedge", 3f, route.Select(p => p + side * 6f).ToList(), null, null));
                        made.Add(MapFeatures.Hedgerows);
                        break;
                    case "lane":
                        hedges.Add(Line(null, "hedge", 3f, route.Select(p => p + side * 6f).ToList(), null, null));
                        hedges.Add(Line(null, "hedge", 3f, route.Select(p => p - side * 6f).ToList(), null, null));
                        made.Add(MapFeatures.Hedgerows);
                        break;
                    case "tree line":
                        woods.Add(Line(null, "forest", rng.Range(28f, 40f), route, null, null));
                        made.Add(Allowed(MapFeatures.Forest) ? MapFeatures.Forest : MapFeatures.Woodlots);
                        break;
                    default:
                        float width = rng.Range(10f, 18f);
                        gullies.Add(new MapFeature { Kind = "gully", Label = Name(names.Woods, " Gully"), Note = "dry wash", Terrain = "gully", Width = width, PaintWidth = width * 0.5f, Height = rng.Range(2.5f, 4.5f), Points = Squares(route) });
                        made.Add(MapFeatures.Gullies);
                        break;
                }
            }

            /// <summary>Any 100 m square with nothing to hide behind within about 70 m of its middle gets a hedge or a copse.</summary>
            void BreakUpOpenGround()
            {
                bool copses = Allowed(MapFeatures.Woodlots) || Allowed(MapFeatures.Forest), hedgeRows = Allowed(MapFeatures.Hedgerows);
                if (!copses && !hedgeRows) return;
                var cover = new List<(List<Vec2> pts, float r, bool line)>();
                foreach (var f in hills.Concat(ridges).Concat(fields).Concat(hedges).Concat(woods).Concat(marshes).Concat(gullies).Concat(yards).Concat(structures).Concat(cliffs))
                {
                    if (f.Terrain == null || !CoverTerrains.Contains(f.Terrain)) continue;
                    if (f.Points != null)
                    {
                        var pts = f.Points.Select(q => new Vec2(q[0] * sq, q[1] * sq)).ToList();
                        if (f.Kind == "area")
                        {
                            Vec2 c = new Vec2(pts.Average(q => q.X), pts.Average(q => q.Y));
                            cover.Add((new List<Vec2> { c }, pts.Max(q => Vec2.Distance(q, c)), false));
                        }
                        else cover.Add((pts, Math.Max(2f, f.Width) / 2f, true));
                    }
                    else if (f.Rect != null)
                    {
                        Vec2 c = new Vec2((f.Rect[0] + f.Rect[2] / 2f) * sq, (f.Rect[1] + f.Rect[3] / 2f) * sq);
                        cover.Add((new List<Vec2> { c }, Math.Max(f.Rect[2], f.Rect[3]) * sq / 2f, false));
                    }
                    else if (f.Center != null) cover.Add((new List<Vec2> { new Vec2(f.Center[0] * sq, f.Center[1] * sq) }, 8f, false));
                }
                float Near(Vec2 p)
                {
                    float best = float.MaxValue;
                    foreach (var (pts, r, line) in cover)
                    {
                        if (!line) { best = Math.Min(best, Vec2.Distance(p, pts[0]) - r); continue; }
                        for (int i = 0; i + 1 < pts.Count; i++) best = Math.Min(best, MapGrid.DistanceToSegment(p, pts[i], pts[i + 1]) - r);
                    }
                    return best;
                }
                // Like farmland: a hedge along a field edge of the square, sometimes two, or a copse in a corner.
                for (float y0 = 0f; y0 < h; y0 += sq)
                    for (float x0 = 0f; x0 < w; x0 += sq)
                    {
                        Vec2 p = new Vec2(x0 + sq / 2f, y0 + sq / 2f);
                        if (NearWater(p, 15f) || NearRoad(p, 8f) || InSettlement(p, 10f) || Near(p) < 70f) continue;
                        var corners = new[] { new Vec2(x0, y0), new Vec2(x0 + sq, y0), new Vec2(x0 + sq, y0 + sq), new Vec2(x0, y0 + sq) };
                        if (hedgeRows && (!copses || rng.Chance(0.65f)))
                        {
                            int first = rng.Int(0, 3), edges = rng.Chance(0.4f) ? 2 : 1;
                            for (int e = 0; e < edges; e++)
                            {
                                Vec2 a = corners[(first + e) % 4], b = corners[(first + e + 1) % 4];
                                var pts = new List<Vec2> { a + (b - a) * rng.Range(0f, 0.2f), a + (b - a) * rng.Range(0.75f, 1f) };
                                if (Subdivide(pts, 8f).Any(q => NearWater(q, 4f) || NearRoad(q, 4f) || InSettlement(q, 5f))) continue;
                                hedges.Add(Line(null, rng.Chance(0.2f) ? "wall" : "hedge", 3f, pts, null, null));
                                cover.Add((pts, 1.5f, true));
                                made.Add(MapFeatures.Hedgerows);
                            }
                        }
                        else
                        {
                            Vec2 corner = corners[rng.Int(0, 3)];
                            Vec2 c = corner + (p - corner) * rng.Range(0.15f, 0.35f);
                            if (NearWater(c, 20f) || NearRoad(c, 20f)) continue;
                            float r = rng.Range(14f, 24f);
                            woods.Add(Area(null, "forest", Blob(c, r)));
                            cover.Add((new List<Vec2> { c }, r, false));
                            made.Add(MapFeatures.Woodlots);
                        }
                    }
            }

            // ---------- helpers ----------

            /// <summary>A wandering line across the whole map, edge to edge.</summary>
            List<Vec2> Meander(bool northSouth, float lo, float hi, float wobble)
            {
                float across = northSouth ? w : h, along = northSouth ? h : w;
                int n = rng.Int(5, 8);
                float pos = rng.Range(lo, hi) * across;
                var pts = new List<Vec2>();
                for (int i = 0; i < n; i++)
                {
                    float t = -20f + (along + 40f) * i / (n - 1);
                    if (i > 0) pos = Clamp(pos + rng.Range(-wobble, wobble), across * 0.05f, across * 0.95f);
                    pts.Add(northSouth ? new Vec2(pos, t) : new Vec2(t, pos));
                }
                return pts;
            }

            List<Vec2> Blob(Vec2 c, float r)
            {
                var pts = new List<Vec2>();
                for (int i = 0; i < 12; i++)
                {
                    double a = 2 * Math.PI * i / 12;
                    float rr = r * rng.Range(0.65f, 1f);
                    pts.Add(new Vec2(c.X + (float)Math.Cos(a) * rr, c.Y + (float)Math.Sin(a) * rr));
                }
                return pts;
            }

            Vec2 EdgePoint()
            {
                switch (rng.Int(0, 3))
                {
                    case 0: return new Vec2(rng.Range(0f, w), 0f);
                    case 1: return new Vec2(rng.Range(0f, w), h - 1f);
                    case 2: return new Vec2(0f, rng.Range(0f, h));
                    default: return new Vec2(w - 1f, rng.Range(0f, h));
                }
            }

            bool TryPlace(float clearance, out Vec2 c)
            {
                for (int i = 0; i < 30; i++)
                {
                    c = new Vec2(rng.Range(clearance, w - clearance), rng.Range(clearance, h - clearance));
                    if (NearWater(c, clearance) || InSettlement(c, clearance) || NearRoad(c, clearance)) continue;
                    return true;
                }
                c = default;
                return false;
            }

            bool NearWater(Vec2 p, float margin)
            {
                foreach (var wl in waterLines)
                    for (int i = 0; i + 1 < wl.Points.Count; i++)
                        if (MapGrid.DistanceToSegment(p, wl.Points[i], wl.Points[i + 1]) < wl.HalfWidth + margin) return true;
                foreach (var l in lakes)
                    if (Vec2.Distance(p, l.c) < l.r + margin) return true;
                return false;
            }

            bool CrossesDeepWater(List<Vec2> pts)
            {
                foreach (var p in Subdivide(pts, 4f))
                {
                    foreach (var wl in waterLines)
                        if (wl.Deep)
                            for (int i = 0; i + 1 < wl.Points.Count; i++)
                                if (MapGrid.DistanceToSegment(p, wl.Points[i], wl.Points[i + 1]) < wl.HalfWidth + 2f) return true;
                    foreach (var l in lakes)
                        if (Vec2.Distance(p, l.c) < l.r + 2f) return true;
                }
                return false;
            }

            /// <summary>Where a line from the middle of a rectangle toward a point leaves the rectangle.</summary>
            static Vec2 EdgeOfRect(Vec2 min, Vec2 max, Vec2 mid, Vec2 toward)
            {
                Vec2 d = toward - mid;
                float tx = d.X > 0 ? (max.X - mid.X) / d.X : d.X < 0 ? (min.X - mid.X) / d.X : float.MaxValue;
                float ty = d.Y > 0 ? (max.Y - mid.Y) / d.Y : d.Y < 0 ? (min.Y - mid.Y) / d.Y : float.MaxValue;
                float t = Math.Min(1f, Math.Min(tx, ty));
                return mid + d * t;
            }

            bool NearRoad(Vec2 p, float margin)
            {
                foreach (var r in roads)
                    for (int i = 0; i + 1 < r.pts.Count; i++)
                        if (MapGrid.DistanceToSegment(p, r.pts[i], r.pts[i + 1]) < r.half + margin) return true;
                return false;
            }

            bool WaterIn(Vec2 min, Vec2 max)
            {
                for (float y = min.Y; y <= max.Y; y += 10f)
                    for (float x = min.X; x <= max.X; x += 10f)
                        if (NearWater(new Vec2(x, y), 5f)) return true;
                return false;
            }

            bool InSettlement(Vec2 p, float margin) =>
                settlements.Any(s => p.X > s.min.X - margin && p.X < s.max.X + margin && p.Y > s.min.Y - margin && p.Y < s.max.Y + margin);

            bool OverlapsSettlement(Vec2 min, Vec2 max) =>
                settlements.Any(s => min.X < s.max.X + 30f && max.X > s.min.X - 30f && min.Y < s.max.Y + 30f && max.Y > s.min.Y - 30f);

            string Name(List<string> pool, string suffix)
            {
                var free = pool.Where(n => !usedNames.Contains(n + suffix)).ToList();
                string name;
                if (free.Count > 0) name = rng.Pick(free) + suffix;
                else
                {
                    int k = 2;
                    string stem = pool.Count > 0 ? pool[0] : "Unnamed";
                    while (usedNames.Contains(stem + " " + k + suffix)) k++;
                    name = stem + " " + k + suffix;
                }
                usedNames.Add(name);
                return name;
            }

            List<float[]> Squares(IEnumerable<Vec2> pts) => pts.Select(p => new[] { p.X / sq, p.Y / sq }).ToList();

            MapFeature Line(string label, string terrain, float width, List<Vec2> pts, string note, string alias) => new MapFeature
            {
                Kind = "line",
                Label = label,
                Terrain = terrain,
                Width = width,
                Points = Squares(pts),
                Note = note,
                Aliases = alias == null ? new List<string>() : new List<string> { alias },
            };

            MapFeature Area(string label, string terrain, List<Vec2> pts) => new MapFeature
            {
                Kind = "area",
                Label = label,
                Terrain = terrain,
                Points = Squares(pts),
            };

            MapFeature Rect(string label, string kind, string terrain, Vec2 min, Vec2 max, string note) => new MapFeature
            {
                Kind = kind,
                Label = label,
                Terrain = terrain,
                Note = note,
                Rect = new[] { min.X / sq, min.Y / sq, (max.X - min.X) / sq, (max.Y - min.Y) / sq },
            };

            static Vec2 PointAlong(List<Vec2> pts, float t)
            {
                float total = 0f;
                for (int i = 0; i + 1 < pts.Count; i++) total += Vec2.Distance(pts[i], pts[i + 1]);
                float want = total * t;
                for (int i = 0; i + 1 < pts.Count; i++)
                {
                    float seg = Vec2.Distance(pts[i], pts[i + 1]);
                    if (want <= seg && seg > 0f) return pts[i] + (pts[i + 1] - pts[i]) * (want / seg);
                    want -= seg;
                }
                return pts[pts.Count - 1];
            }

            static Vec2 Unit(Vec2 v)
            {
                float len = v.Length;
                return len > 0f ? v * (1f / len) : new Vec2(1f, 0f);
            }

            static float Clamp(float v, float lo, float hi) => Math.Max(lo, Math.Min(hi, v));

            /// <summary>The same line with extra points so no stretch is longer than the given step.</summary>
            static List<Vec2> Subdivide(List<Vec2> pts, float step)
            {
                var result = new List<Vec2>();
                for (int i = 0; i + 1 < pts.Count; i++)
                {
                    int n = Math.Max(1, (int)Math.Ceiling(Vec2.Distance(pts[i], pts[i + 1]) / step));
                    for (int k = 0; k < n; k++) result.Add(pts[i] + (pts[i + 1] - pts[i]) * (k / (float)n));
                }
                if (pts.Count > 0) result.Add(pts[pts.Count - 1]);
                return result;
            }

            /// <summary>True when any point of a line comes within the given distance of another line.</summary>
            static bool Crosses(List<Vec2> a, List<Vec2> b, float within)
            {
                for (int i = 0; i + 1 < a.Count; i++)
                    for (int j = 0; j + 1 < b.Count; j++)
                    {
                        if (Intersect(a[i], a[i + 1], b[j], b[j + 1], out _)) return true;
                        if (MapGrid.DistanceToSegment(a[i], b[j], b[j + 1]) < within) return true;
                        if (MapGrid.DistanceToSegment(b[j], a[i], a[i + 1]) < within) return true;
                    }
                return false;
            }

            static bool Intersect(Vec2 a, Vec2 b, Vec2 c, Vec2 d, out Vec2 at)
            {
                at = default;
                float den = (b.X - a.X) * (d.Y - c.Y) - (b.Y - a.Y) * (d.X - c.X);
                if (Math.Abs(den) < 1e-6f) return false;
                float t = ((c.X - a.X) * (d.Y - c.Y) - (c.Y - a.Y) * (d.X - c.X)) / den;
                float u = ((c.X - a.X) * (b.Y - a.Y) - (c.Y - a.Y) * (b.X - a.X)) / den;
                if (t < 0f || t > 1f || u < 0f || u > 1f) return false;
                at = a + (b - a) * t;
                return true;
            }
        }
    }
}
