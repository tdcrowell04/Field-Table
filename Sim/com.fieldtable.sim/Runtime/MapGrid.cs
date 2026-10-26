using System;
using System.Collections.Generic;

namespace FieldTable.Sim
{
    /// <summary>A position on the map in meters. X runs west to east, Y runs north to south.</summary>
    public readonly struct Vec2 : IEquatable<Vec2>
    {
        public readonly float X;
        public readonly float Y;

        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);
        public static Vec2 operator *(Vec2 a, float s) => new Vec2(a.X * s, a.Y * s);

        public float Length => (float)Math.Sqrt(X * X + Y * Y);
        public static float Distance(Vec2 a, Vec2 b) => (a - b).Length;

        public bool Equals(Vec2 other) => X.Equals(other.X) && Y.Equals(other.Y);
        public override bool Equals(object obj) => obj is Vec2 v && Equals(v);
        public override int GetHashCode() => (X.GetHashCode() * 397) ^ Y.GetHashCode();
        public override string ToString() => "(" + X + ", " + Y + ")";
    }

    /// <summary>
    /// One kind of ground, loaded from data so each setting and mod can bring its own.
    /// </summary>
    public sealed class TerrainDef
    {
        public string Id { get; set; }
        public string Name { get; set; }

        /// <summary>Movement speed multiplier: 1 is open ground, 0 means nobody can walk here.</summary>
        public float MoveFactor { get; set; } = 1f;

        /// <summary>How much this ground protects someone in it from fire, from 0 (none) to 1 (full).</summary>
        public float Cover { get; set; }

        /// <summary>How tall the things growing or built here are, in meters. 0 for flat ground.</summary>
        public float Height { get; set; }

        /// <summary>
        /// How much each meter of this blocks sight below its height. 1 is solid (a wall);
        /// 0.04 means about 25 meters of it hides what's behind (dense pine).
        /// </summary>
        public float SightBlock { get; set; }

        /// <summary>How well someone in this ground is hidden from 0 (not at all) to 1 (fully), when it's at least as tall as they are.</summary>
        public float Concealment { get; set; }

        /// <summary>True when someone inside can see out and be seen, like through a building's windows.</summary>
        public bool SeeOut { get; set; }

        /// <summary>Water too deep to wade. Crossing takes a bridge, a boat, or a swim, and a swim in full kit can drown you.</summary>
        public bool Deep { get; set; }

        public bool Passable => MoveFactor > 0f;
    }

    /// <summary>
    /// A map reference like "G4": a letter for the column (west to east) and a number for the row (north to south).
    /// </summary>
    public readonly struct GridRef : IEquatable<GridRef>
    {
        /// <summary>Zero-based column: A is 0.</summary>
        public readonly int Column;

        /// <summary>Zero-based row: 1 is 0.</summary>
        public readonly int Row;

        public GridRef(int column, int row)
        {
            Column = column;
            Row = row;
        }

        /// <summary>Reads "G4" or "g4". Doesn't check the square is on any particular map.</summary>
        public static bool TryParse(string text, out GridRef grid)
        {
            grid = default;
            if (string.IsNullOrWhiteSpace(text)) return false;
            string t = text.Trim().ToUpperInvariant();
            if (t.Length < 2 || t[0] < 'A' || t[0] > 'Z') return false;
            if (!int.TryParse(t.Substring(1), System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int row) || row < 1) return false;
            grid = new GridRef(t[0] - 'A', row - 1);
            return true;
        }

        public override string ToString() => ((char)('A' + Column)).ToString() + (Row + 1);

        public bool Equals(GridRef other) => Column == other.Column && Row == other.Row;
        public override bool Equals(object obj) => obj is GridRef g && Equals(g);
        public override int GetHashCode() => (Column * 397) ^ Row;
    }

    /// <summary>A named place on the map that orders can point at: "building 4", "Hill 151", "Route Slate".</summary>
    public sealed class Landmark
    {
        public string Label { get; }
        public string Kind { get; }
        public IReadOnlyList<string> Aliases { get; }
        public string Note { get; }

        /// <summary>Where an order to go "to" this place aims, in meters.</summary>
        public Vec2 Center { get; }

        /// <summary>The area it covers, in meters.</summary>
        public Vec2 Min { get; }
        public Vec2 Max { get; }

        /// <summary>For roads, creeks, walls, and other lines: the points along it, in meters. Empty for areas.</summary>
        public IReadOnlyList<Vec2> Points { get; }

        public Landmark(string label, string kind, IList<string> aliases, string note, Vec2 center, Vec2 min, Vec2 max, IList<Vec2> points)
        {
            Label = label;
            Kind = kind;
            Aliases = new List<string>(aliases ?? new string[0]);
            Note = note ?? "";
            Center = center;
            Min = min;
            Max = max;
            Points = new List<Vec2>(points ?? new Vec2[0]);
        }

        public bool Contains(Vec2 p) => p.X >= Min.X && p.X <= Max.X && p.Y >= Min.Y && p.Y <= Max.Y;

        public bool IsCalled(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            string n = name.Trim();
            if (string.Equals(Label, n, StringComparison.OrdinalIgnoreCase)) return true;
            foreach (var a in Aliases)
                if (string.Equals(a, n, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }

    /// <summary>
    /// The ground the fight happens on: a fine grid of terrain cells with ground height,
    /// the coarser lettered squares players use for grid references, and named landmarks.
    /// </summary>
    public sealed class MapGrid
    {
        readonly byte[] cells;
        readonly float[] ground;
        readonly List<TerrainDef> terrains;
        readonly Dictionary<string, byte> terrainIndex = new Dictionary<string, byte>();
        readonly List<Landmark> landmarks = new List<Landmark>();

        public string Name { get; }

        /// <summary>Map size in cells.</summary>
        public int Width { get; }
        public int Height { get; }

        /// <summary>Meters per cell.</summary>
        public float CellSize { get; }

        /// <summary>Meters per lettered grid square.</summary>
        public float SquareSize { get; }

        public IReadOnlyList<Landmark> Landmarks => landmarks;

        public MapGrid(string name, int width, int height, float cellSize, float squareSize, IList<TerrainDef> terrainTable, string fillTerrain)
        {
            if (width < 1 || height < 1) throw new ArgumentOutOfRangeException(nameof(width), "A map needs at least one cell.");
            if (cellSize <= 0f) throw new ArgumentOutOfRangeException(nameof(cellSize));
            if (squareSize < cellSize) throw new ArgumentOutOfRangeException(nameof(squareSize), "A grid square can't be smaller than a cell.");
            if (terrainTable == null || terrainTable.Count == 0) throw new ArgumentException("A map needs at least one kind of terrain.", nameof(terrainTable));
            if (terrainTable.Count > 256) throw new ArgumentException("A map can use at most 256 kinds of terrain.", nameof(terrainTable));

            Name = name;
            Width = width;
            Height = height;
            CellSize = cellSize;
            SquareSize = squareSize;
            terrains = new List<TerrainDef>(terrainTable);
            for (int i = 0; i < terrains.Count; i++) terrainIndex[terrains[i].Id] = (byte)i;

            if (Columns > 26) throw new ArgumentException("A map can be at most 26 grid squares wide (A to Z).");

            cells = new byte[width * height];
            ground = new float[width * height];
            byte fill = IndexOf(fillTerrain);
            for (int i = 0; i < cells.Length; i++) cells[i] = fill;
        }

        public float WidthMeters => Width * CellSize;
        public float HeightMeters => Height * CellSize;

        /// <summary>Number of lettered columns and numbered rows, counting a partial square at the edge.</summary>
        public int Columns => (int)Math.Ceiling(WidthMeters / SquareSize);
        public int Rows => (int)Math.Ceiling(HeightMeters / SquareSize);

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
        public bool InBounds(Vec2 p) => p.X >= 0f && p.Y >= 0f && p.X < WidthMeters && p.Y < HeightMeters;

        public TerrainDef TerrainAt(int x, int y)
        {
            if (!InBounds(x, y)) throw new ArgumentOutOfRangeException(nameof(x), "That cell is off the map.");
            return terrains[cells[y * Width + x]];
        }

        public TerrainDef TerrainAt(Vec2 p)
        {
            CellOf(p, out int x, out int y);
            return TerrainAt(x, y);
        }

        /// <summary>Height of the ground itself in meters, not counting buildings or trees.</summary>
        public float GroundAt(int x, int y)
        {
            if (!InBounds(x, y)) throw new ArgumentOutOfRangeException(nameof(x), "That cell is off the map.");
            return ground[y * Width + x];
        }

        public float GroundAt(Vec2 p)
        {
            CellOf(p, out int x, out int y);
            return GroundAt(x, y);
        }

        public void SetTerrain(int x, int y, string terrainId)
        {
            if (!InBounds(x, y)) throw new ArgumentOutOfRangeException(nameof(x), "That cell is off the map.");
            cells[y * Width + x] = IndexOf(terrainId);
        }

        /// <summary>Paints a rectangle of cells, clipped to the map.</summary>
        public void FillRect(int x, int y, int w, int h, string terrainId)
        {
            byte t = IndexOf(terrainId);
            int x0 = Math.Max(0, x), y0 = Math.Max(0, y);
            int x1 = Math.Min(Width, x + w), y1 = Math.Min(Height, y + h);
            for (int cy = y0; cy < y1; cy++)
                for (int cx = x0; cx < x1; cx++)
                    cells[cy * Width + cx] = t;
        }

        /// <summary>Paints every cell whose center is inside the polygon (points in meters).</summary>
        public void FillPolygon(IList<Vec2> points, string terrainId)
        {
            if (points == null || points.Count < 3) throw new ArgumentException("An area needs at least 3 points.", nameof(points));
            byte t = IndexOf(terrainId);
            Bounds(points, 0f, out int x0, out int y0, out int x1, out int y1);
            for (int cy = y0; cy <= y1; cy++)
                for (int cx = x0; cx <= x1; cx++)
                    if (InsidePolygon(points, CellCenter(cx, cy)))
                        cells[cy * Width + cx] = t;
        }

        /// <summary>Paints a line of the given width (meters) along the points (meters). Never thinner than one cell.</summary>
        public void FillLine(IList<Vec2> points, float width, string terrainId)
        {
            if (points == null || points.Count < 2) throw new ArgumentException("A line needs at least 2 points.", nameof(points));
            byte t = IndexOf(terrainId);
            float r = Math.Max(width, CellSize) / 2f;
            for (int i = 0; i + 1 < points.Count; i++)
            {
                var seg = new[] { points[i], points[i + 1] };
                Bounds(seg, r, out int x0, out int y0, out int x1, out int y1);
                for (int cy = y0; cy <= y1; cy++)
                    for (int cx = x0; cx <= x1; cx++)
                        if (DistanceToSegment(CellCenter(cx, cy), seg[0], seg[1]) <= r)
                            cells[cy * Width + cx] = t;
            }
        }

        /// <summary>
        /// Raises a rounded hill filling the ellipse inside the rectangle (meters), peaking at its middle.
        /// Optionally paints the hill's ground.
        /// </summary>
        public void RaiseHill(Vec2 min, Vec2 max, float peak, string terrainId)
        {
            byte? t = terrainId == null ? (byte?)null : IndexOf(terrainId);
            float cxm = (min.X + max.X) / 2f, cym = (min.Y + max.Y) / 2f;
            float rx = (max.X - min.X) / 2f, ry = (max.Y - min.Y) / 2f;
            if (rx <= 0f || ry <= 0f) throw new ArgumentException("A hill needs a width and a depth.");
            Bounds(new[] { min, max }, 0f, out int x0, out int y0, out int x1, out int y1);
            for (int cy = y0; cy <= y1; cy++)
                for (int cx = x0; cx <= x1; cx++)
                {
                    Vec2 c = CellCenter(cx, cy);
                    float dx = (c.X - cxm) / rx, dy = (c.Y - cym) / ry;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (d >= 1f) continue;
                    int i = cy * Width + cx;
                    ground[i] += peak * (float)(Math.Cos(Math.PI * d) + 1.0) / 2f;
                    if (t.HasValue) cells[i] = t.Value;
                }
        }

        /// <summary>
        /// Raises (or with a negative height, lowers) the ground along a line, highest in the middle of its width
        /// and easing to nothing at the edges: a ridge, or a gully. Optionally paints the middle of it.
        /// </summary>
        public void RaiseAlong(IList<Vec2> points, float width, float height, string terrainId, float paintWidth = 0f)
        {
            if (points == null || points.Count < 2) throw new ArgumentException("A ridge or gully needs at least 2 points.", nameof(points));
            if (width <= 0f) throw new ArgumentException("A ridge or gully needs a width above 0.");
            byte? t = terrainId == null ? (byte?)null : IndexOf(terrainId);
            float half = width / 2f;
            float paintHalf = Math.Max(paintWidth, CellSize) / 2f;
            Bounds(points, half, out int x0, out int y0, out int x1, out int y1);
            for (int cy = y0; cy <= y1; cy++)
                for (int cx = x0; cx <= x1; cx++)
                {
                    Vec2 c = CellCenter(cx, cy);
                    float d = float.MaxValue;
                    for (int i = 0; i + 1 < points.Count; i++) d = Math.Min(d, DistanceToSegment(c, points[i], points[i + 1]));
                    if (d >= half) continue;
                    int idx = cy * Width + cx;
                    ground[idx] += height * (float)(Math.Cos(Math.PI * d / half) + 1.0) / 2f;
                    if (t.HasValue && (paintWidth <= 0f || d <= paintHalf)) cells[idx] = t.Value;
                }
        }

        public void AddLandmark(Landmark landmark)
        {
            if (landmark == null) throw new ArgumentNullException(nameof(landmark));
            landmarks.Add(landmark);
        }

        /// <summary>Finds a landmark by its label or any alias, ignoring case. Null if there's none.</summary>
        public Landmark FindLandmark(string name) => landmarks.Find(l => l.IsCalled(name));

        public void CellOf(Vec2 p, out int x, out int y)
        {
            x = (int)Math.Floor(p.X / CellSize);
            y = (int)Math.Floor(p.Y / CellSize);
        }

        public Vec2 CellCenter(int x, int y) => new Vec2((x + 0.5f) * CellSize, (y + 0.5f) * CellSize);

        public bool IsOnMap(GridRef g) => g.Column >= 0 && g.Row >= 0 && g.Column < Columns && g.Row < Rows;

        /// <summary>The lettered square a position falls in.</summary>
        public GridRef SquareOf(Vec2 p)
        {
            if (!InBounds(p)) throw new ArgumentOutOfRangeException(nameof(p), "That position is off the map.");
            return new GridRef((int)Math.Floor(p.X / SquareSize), (int)Math.Floor(p.Y / SquareSize));
        }

        /// <summary>The middle of a lettered square, kept on the map for a partial square at the edge.</summary>
        public Vec2 SquareCenter(GridRef g)
        {
            if (!IsOnMap(g)) throw new ArgumentOutOfRangeException(nameof(g), "Grid " + g + " is off the map.");
            float x0 = g.Column * SquareSize, y0 = g.Row * SquareSize;
            float x1 = Math.Min(x0 + SquareSize, WidthMeters), y1 = Math.Min(y0 + SquareSize, HeightMeters);
            return new Vec2((x0 + x1) / 2f, (y0 + y1) / 2f);
        }

        public bool SquareContains(GridRef g, Vec2 p) => InBounds(p) && SquareOf(p).Equals(g);

        void Bounds(IList<Vec2> pts, float pad, out int x0, out int y0, out int x1, out int y1)
        {
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (var p in pts)
            {
                minX = Math.Min(minX, p.X); minY = Math.Min(minY, p.Y);
                maxX = Math.Max(maxX, p.X); maxY = Math.Max(maxY, p.Y);
            }
            x0 = Math.Max(0, (int)Math.Floor((minX - pad) / CellSize));
            y0 = Math.Max(0, (int)Math.Floor((minY - pad) / CellSize));
            x1 = Math.Min(Width - 1, (int)Math.Floor((maxX + pad) / CellSize));
            y1 = Math.Min(Height - 1, (int)Math.Floor((maxY + pad) / CellSize));
        }

        static bool InsidePolygon(IList<Vec2> poly, Vec2 p)
        {
            bool inside = false;
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                Vec2 a = poly[i], b = poly[j];
                if ((a.Y > p.Y) != (b.Y > p.Y) && p.X < (b.X - a.X) * (p.Y - a.Y) / (b.Y - a.Y) + a.X)
                    inside = !inside;
            }
            return inside;
        }

        internal static float DistanceToSegment(Vec2 p, Vec2 a, Vec2 b)
        {
            Vec2 ab = b - a;
            float len2 = ab.X * ab.X + ab.Y * ab.Y;
            if (len2 <= 0f) return Vec2.Distance(p, a);
            float t = ((p.X - a.X) * ab.X + (p.Y - a.Y) * ab.Y) / len2;
            t = Math.Max(0f, Math.Min(1f, t));
            return Vec2.Distance(p, a + ab * t);
        }

        byte IndexOf(string terrainId)
        {
            if (terrainId == null || !terrainIndex.TryGetValue(terrainId, out byte i))
                throw new ArgumentException("Unknown terrain: " + terrainId, nameof(terrainId));
            return i;
        }
    }
}
