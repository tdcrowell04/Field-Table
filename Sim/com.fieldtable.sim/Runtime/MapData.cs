using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FieldTable.Sim
{
    /// <summary>
    /// A map as written in a data file. Positions are in grid squares (like the prototype's maps),
    /// widths and heights in meters. Features are painted in the order they're listed.
    /// </summary>
    public sealed class MapData
    {
        public string Name { get; set; }
        public int Columns { get; set; }
        public int Rows { get; set; }

        /// <summary>Meters per lettered grid square.</summary>
        public float SquareSize { get; set; } = 100f;

        /// <summary>Meters per terrain cell.</summary>
        public float CellSize { get; set; } = 1f;

        /// <summary>The ground everywhere a feature doesn't cover.</summary>
        public string BaseTerrain { get; set; }

        public List<MapFeature> Features { get; set; } = new List<MapFeature>();

        public static MapData FromJson(string json)
        {
            MapData data;
            try
            {
                data = JsonConvert.DeserializeObject<MapData>(json, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                });
            }
            catch (JsonException e)
            {
                throw new FormatException("The map file isn't valid: " + e.Message, e);
            }
            if (data == null) throw new FormatException("The map file is empty.");
            return data;
        }

        /// <summary>Builds the playable map, using the terrain kinds from the game data.</summary>
        public MapGrid Build(IList<TerrainDef> terrains)
        {
            if (Columns < 1 || Rows < 1) throw new FormatException("Map " + Name + " needs at least one column and one row.");
            if (CellSize <= 0f || SquareSize < CellSize) throw new FormatException("Map " + Name + " needs a cell size above 0 and grid squares at least that big.");
            if (Features == null) throw new FormatException("Map " + Name + " has no feature list.");

            int width = (int)Math.Round(Columns * SquareSize / CellSize);
            int height = (int)Math.Round(Rows * SquareSize / CellSize);
            MapGrid map;
            try
            {
                map = new MapGrid(Name, width, height, CellSize, SquareSize, terrains, BaseTerrain);
            }
            catch (ArgumentException e)
            {
                throw new FormatException("Map " + Name + ": " + e.Message, e);
            }

            foreach (var f in Features)
            {
                try
                {
                    f.Apply(map, SquareSize);
                }
                catch (ArgumentException e)
                {
                    throw new FormatException("Map " + Name + ", feature " + (f.Label ?? "(no label)") + ": " + e.Message, e);
                }
            }
            return map;
        }
    }

    /// <summary>
    /// One thing on a map. Kinds:
    /// "area" (a rect or polygon of terrain), "hill" (a rounded hill in a rect),
    /// "line" (a road, creek, wall, or hedge along points), "building" (a center and a size in meters),
    /// and "control" (a phase line or boundary with no terrain of its own).
    /// </summary>
    public sealed class MapFeature
    {
        public string Kind { get; set; }
        public string Label { get; set; }
        public List<string> Aliases { get; set; }
        public string Note { get; set; }
        public string Terrain { get; set; }

        /// <summary>x, y, width, depth in grid squares.</summary>
        public float[] Rect { get; set; }

        /// <summary>Points in grid squares.</summary>
        public List<float[]> Points { get; set; }

        /// <summary>Line width in meters.</summary>
        public float Width { get; set; }

        /// <summary>Hill height in meters.</summary>
        public float Height { get; set; }

        /// <summary>Building center in grid squares.</summary>
        public float[] Center { get; set; }

        /// <summary>Building width and depth in meters.</summary>
        public float[] Size { get; set; }

        internal void Apply(MapGrid map, float square)
        {
            switch (Kind)
            {
                case "area":
                    if (Rect != null)
                    {
                        RectMeters(square, out Vec2 min, out Vec2 max);
                        map.FillPolygon(new[] { min, new Vec2(max.X, min.Y), max, new Vec2(min.X, max.Y) }, Terrain);
                        Mark(map, "area", (min + max) * 0.5f, min, max, null);
                    }
                    else
                    {
                        var pts = PointsMeters(square, 3);
                        map.FillPolygon(pts, Terrain);
                        Extent(pts, out Vec2 min, out Vec2 max);
                        Mark(map, "area", (min + max) * 0.5f, min, max, pts);
                    }
                    break;

                case "hill":
                    {
                        RectMeters(square, out Vec2 min, out Vec2 max);
                        if (Height <= 0f) throw new ArgumentException("A hill needs a height above 0.");
                        map.RaiseHill(min, max, Height, Terrain);
                        Mark(map, "hill", (min + max) * 0.5f, min, max, null);
                    }
                    break;

                case "line":
                    {
                        var pts = PointsMeters(square, 2);
                        if (Width <= 0f) throw new ArgumentException("A line needs a width above 0.");
                        map.FillLine(pts, Width, Terrain);
                        Extent(pts, out Vec2 min, out Vec2 max);
                        Mark(map, "line", Midpoint(pts), min, max, pts);
                    }
                    break;

                case "building":
                    {
                        if (Center == null || Center.Length != 2) throw new ArgumentException("A building needs a center: [x, y].");
                        if (Size == null || Size.Length != 2 || Size[0] <= 0f || Size[1] <= 0f) throw new ArgumentException("A building needs a size: [width, depth] in meters.");
                        var c = new Vec2(Center[0] * square, Center[1] * square);
                        var half = new Vec2(Size[0] / 2f, Size[1] / 2f);
                        Vec2 min = c - half, max = c + half;
                        map.FillPolygon(new[] { min, new Vec2(max.X, min.Y), max, new Vec2(min.X, max.Y) }, Terrain);
                        Mark(map, "building", c, min, max, null);
                    }
                    break;

                case "control":
                    {
                        var pts = PointsMeters(square, 2);
                        Extent(pts, out Vec2 min, out Vec2 max);
                        Mark(map, "control", Midpoint(pts), min, max, pts);
                    }
                    break;

                default:
                    throw new ArgumentException("Unknown kind: " + Kind + ". Use area, hill, line, building, or control.");
            }
        }

        void Mark(MapGrid map, string kind, Vec2 center, Vec2 min, Vec2 max, IList<Vec2> pts)
        {
            if (string.IsNullOrWhiteSpace(Label)) return;
            map.AddLandmark(new Landmark(Label, kind, Aliases, Note, center, min, max, pts));
        }

        void RectMeters(float square, out Vec2 min, out Vec2 max)
        {
            if (Rect == null || Rect.Length != 4 || Rect[2] <= 0f || Rect[3] <= 0f)
                throw new ArgumentException("Needs a rect: [x, y, width, depth] in grid squares.");
            min = new Vec2(Rect[0] * square, Rect[1] * square);
            max = new Vec2((Rect[0] + Rect[2]) * square, (Rect[1] + Rect[3]) * square);
        }

        List<Vec2> PointsMeters(float square, int least)
        {
            if (Points == null || Points.Count < least) throw new ArgumentException("Needs at least " + least + " points.");
            var list = new List<Vec2>();
            foreach (var p in Points)
            {
                if (p == null || p.Length != 2) throw new ArgumentException("Each point needs [x, y].");
                list.Add(new Vec2(p[0] * square, p[1] * square));
            }
            return list;
        }

        static void Extent(IList<Vec2> pts, out Vec2 min, out Vec2 max)
        {
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            foreach (var p in pts)
            {
                x0 = Math.Min(x0, p.X); y0 = Math.Min(y0, p.Y);
                x1 = Math.Max(x1, p.X); y1 = Math.Max(y1, p.Y);
            }
            min = new Vec2(x0, y0);
            max = new Vec2(x1, y1);
        }

        /// <summary>The point halfway along a line.</summary>
        static Vec2 Midpoint(IList<Vec2> pts)
        {
            float total = 0f;
            for (int i = 0; i + 1 < pts.Count; i++) total += Vec2.Distance(pts[i], pts[i + 1]);
            float half = total / 2f;
            for (int i = 0; i + 1 < pts.Count; i++)
            {
                float seg = Vec2.Distance(pts[i], pts[i + 1]);
                if (seg >= half && seg > 0f) return pts[i] + (pts[i + 1] - pts[i]) * (half / seg);
                half -= seg;
            }
            return pts[pts.Count - 1];
        }
    }
}
