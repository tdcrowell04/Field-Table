using System;
using System.Collections.Generic;
using Xunit;

namespace FieldTable.Sim.Tests
{
    public class MapGridTests
    {
        internal static List<TerrainDef> Terrains() => new List<TerrainDef>
        {
            new TerrainDef { Id = "open", Name = "Open ground", MoveFactor = 1f },
            new TerrainDef { Id = "water", Name = "River", MoveFactor = 0f },
            new TerrainDef { Id = "building", Name = "Building", MoveFactor = 0.5f, Cover = 0.8f, Height = 6f, SightBlock = 1f, Concealment = 0.5f, SeeOut = true },
            new TerrainDef { Id = "wall", Name = "Low wall", MoveFactor = 0.3f, Cover = 0.8f, Height = 1.2f, SightBlock = 1f },
            new TerrainDef { Id = "forest", Name = "Pine", MoveFactor = 0.5f, Height = 15f, SightBlock = 0.04f, Concealment = 0.5f },
            new TerrainDef { Id = "reeds", Name = "Reeds", MoveFactor = 0.4f, Height = 2f, SightBlock = 0.1f, Concealment = 0.6f },
        };

        // 800 m by 600 m, 1 m cells, 100 m grid squares: columns A to H, rows 1 to 6.
        internal static MapGrid Map() => new MapGrid("Test", 800, 600, 1f, 100f, Terrains(), "open");

        [Fact]
        public void CountsGridSquares()
        {
            var map = Map();
            Assert.Equal(8, map.Columns);
            Assert.Equal(6, map.Rows);
        }

        [Fact]
        public void CountsAPartialSquareAtTheEdge()
        {
            var map = new MapGrid("Odd", 850, 600, 1f, 100f, Terrains(), "open");
            Assert.Equal(9, map.Columns);
            Assert.Equal(new Vec2(825f, 50f), map.SquareCenter(new GridRef(8, 0)));
        }

        [Theory]
        [InlineData("G4", 6, 3)]
        [InlineData("g4", 6, 3)]
        [InlineData(" A1 ", 0, 0)]
        [InlineData("H6", 7, 5)]
        public void ReadsGridReferences(string text, int column, int row)
        {
            Assert.True(GridRef.TryParse(text, out var g));
            Assert.Equal(new GridRef(column, row), g);
        }

        [Theory]
        [InlineData("")]
        [InlineData("4G")]
        [InlineData("G")]
        [InlineData("G0")]
        [InlineData("G-1")]
        [InlineData("G4.5")]
        [InlineData(null)]
        public void RejectsBadGridReferences(string text)
        {
            Assert.False(GridRef.TryParse(text, out _));
        }

        [Fact]
        public void WritesGridReferencesBack()
        {
            Assert.Equal("G4", new GridRef(6, 3).ToString());
        }

        [Fact]
        public void KnowsWhichSquaresAreOnTheMap()
        {
            var map = Map();
            Assert.True(map.IsOnMap(new GridRef(7, 5)));
            Assert.False(map.IsOnMap(new GridRef(8, 0)));
            Assert.False(map.IsOnMap(new GridRef(0, 6)));
        }

        [Fact]
        public void FindsTheSquareAPositionIsIn()
        {
            var map = Map();
            Assert.Equal("G4", map.SquareOf(new Vec2(650f, 350f)).ToString());
            Assert.Equal("A1", map.SquareOf(new Vec2(0f, 0f)).ToString());
            Assert.Equal("B1", map.SquareOf(new Vec2(100f, 99.9f)).ToString());
        }

        [Fact]
        public void FiresIntoASquareStayInThatSquare()
        {
            // Fairness rule 1: a grid is the grid the player sees.
            var map = Map();
            var g4 = new GridRef(6, 3);
            Vec2 center = map.SquareCenter(g4);
            Assert.Equal(new Vec2(650f, 350f), center);
            Assert.True(map.SquareContains(g4, center));
            Assert.False(map.SquareContains(g4, new Vec2(700f, 350f)));
        }

        [Fact]
        public void PaintsAndReadsTerrain()
        {
            var map = Map();
            map.FillRect(300, 0, 40, 600, "water");
            map.SetTerrain(10, 10, "building");
            Assert.Equal("water", map.TerrainAt(new Vec2(320.5f, 100f)).Id);
            Assert.False(map.TerrainAt(320, 5).Passable);
            Assert.Equal("building", map.TerrainAt(10, 10).Id);
            Assert.Equal(6f, map.TerrainAt(10, 10).Height);
            Assert.Equal("open", map.TerrainAt(0, 0).Id);
        }

        [Fact]
        public void ClipsPaintToTheMap()
        {
            var map = Map();
            map.FillRect(790, 590, 50, 50, "water");
            Assert.Equal("water", map.TerrainAt(799, 599).Id);
        }

        [Fact]
        public void RefusesUnknownTerrain()
        {
            var map = Map();
            Assert.Throws<ArgumentException>(() => map.SetTerrain(0, 0, "lava"));
            Assert.Throws<ArgumentException>(() => new MapGrid("Bad", 10, 10, 1f, 10f, Terrains(), "lava"));
        }

        [Fact]
        public void RefusesPositionsOffTheMap()
        {
            var map = Map();
            Assert.False(map.InBounds(new Vec2(800f, 10f)));
            Assert.False(map.InBounds(new Vec2(-0.1f, 10f)));
            Assert.Throws<ArgumentOutOfRangeException>(() => map.SquareOf(new Vec2(800f, 10f)));
            Assert.Throws<ArgumentOutOfRangeException>(() => map.TerrainAt(800, 0));
        }

        [Fact]
        public void RefusesMapsWiderThanZ()
        {
            Assert.Throws<ArgumentException>(() => new MapGrid("Wide", 2700, 100, 1f, 100f, Terrains(), "open"));
        }
    }
}
