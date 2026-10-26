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

        /// <summary>How much this ground protects someone in it, from 0 (none) to 1 (full).</summary>
        public float Cover { get; set; }

        public bool BlocksSight { get; set; }

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

    /// <summary>
    /// The ground the fight happens on: a fine grid of terrain cells, plus the coarser
    /// lettered squares players use for grid references.
    /// </summary>
    public sealed class MapGrid
    {
        readonly byte[] cells;
        readonly List<TerrainDef> terrains;
        readonly Dictionary<string, byte> terrainIndex = new Dictionary<string, byte>();

        public string Name { get; }

        /// <summary>Map size in cells.</summary>
        public int Width { get; }
        public int Height { get; }

        /// <summary>Meters per cell.</summary>
        public float CellSize { get; }

        /// <summary>Meters per lettered grid square.</summary>
        public float SquareSize { get; }

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

        byte IndexOf(string terrainId)
        {
            if (terrainId == null || !terrainIndex.TryGetValue(terrainId, out byte i))
                throw new ArgumentException("Unknown terrain: " + terrainId, nameof(terrainId));
            return i;
        }
    }
}
