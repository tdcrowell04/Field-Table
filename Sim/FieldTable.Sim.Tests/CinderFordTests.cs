using System;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    /// <summary>Checks the Cinder Ford map file against the map from Triston's saved mission.</summary>
    public class CinderFordTests
    {
        const float Standing = 1.7f;

        [Fact]
        public void IsEightSquaresByFourHundredMetersEach()
        {
            var map = CinderFord();
            Assert.Equal(8, map.Columns);
            Assert.Equal(6, map.Rows);
            Assert.Equal(800f, map.WidthMeters);
            Assert.Equal(600f, map.HeightMeters);
        }

        [Fact]
        public void TheBridgeIsInE3WhereRouteSlateCrossesTheCreek()
        {
            var map = CinderFord();
            var bridge = map.FindLandmark("the bridge");
            Assert.Equal("Dornhof bridge", bridge.Label);
            Assert.Equal("E3", map.SquareOf(bridge.Center).ToString());
            Assert.Equal("bridge", map.TerrainAt(bridge.Center).Id);
            Assert.Equal("creek", map.TerrainAt(At(4.5f, 2.6f)).Id);
            Assert.Equal("creek", map.TerrainAt(At(4.47f, 1.0f)).Id);
        }

        [Fact]
        public void TheRoadTheTrailTheWallAndTheHedgeAreWhereTheyShouldBe()
        {
            var map = CinderFord();
            Assert.Equal("road", map.TerrainAt(At(1f, 1.4f)).Id);
            Assert.Equal("road", map.TerrainAt(At(6f, 2.527f)).Id);
            Assert.Equal("trail", map.TerrainAt(At(1.2f, 4.8f)).Id);
            Assert.Equal("wall", map.TerrainAt(At(6f, 2.9f)).Id);
            Assert.Equal("hedge", map.TerrainAt(At(6.5f, 3.5f)).Id);
            Assert.Equal("marsh", map.TerrainAt(At(4f, 4.5f)).Id);
            Assert.Equal("forest", map.TerrainAt(At(1f, 5f)).Id);
            Assert.Equal("rocky", map.TerrainAt(At(7.5f, 0.5f)).Id);
        }

        [Theory]
        [InlineData("building 1", 572.5f, 180f)]
        [InlineData("bldg 2", 642.5f, 190f)]
        [InlineData("3", 557.5f, 245f)]
        [InlineData("building 4", 682.5f, 250f)]
        [InlineData("the farmhouse", 342.5f, 260f)]
        public void FindsEveryBuildingByNumberOrName(string name, float x, float y)
        {
            var map = CinderFord();
            var b = map.FindLandmark(name);
            Assert.NotNull(b);
            Assert.Equal("building", b.Kind);
            Assert.Equal(x, b.Center.X, 2);
            Assert.Equal(y, b.Center.Y, 2);
            Assert.Equal("building", map.TerrainAt(b.Center).Id);
        }

        [Fact]
        public void TheHillsRiseToTheirHeights()
        {
            var map = CinderFord();
            Assert.InRange(map.GroundAt(At(1.1f, 0.9f)), 24f, 25f);
            Assert.InRange(map.GroundAt(At(7.1f, 5f)), 34f, 35f);
            Assert.Equal(0f, map.GroundAt(At(5.5f, 2.6f)));
            Assert.Equal(0f, map.GroundAt(new Vec2(799.5f, 599.5f)));
        }

        [Fact]
        public void KnowsItsNamedPlaces()
        {
            var map = CinderFord();
            foreach (var name in new[] { "Hill 142", "Hill 151", "Pine Wood", "Dornhof", "Route Slate", "Slate", "Kessel Creek", "Mill Trail", "the wall", "East Hedgerow", "PL Iron", "Quarry", "Creek Reeds", "North Fields", "Lower Fields", "Pasture" })
                Assert.NotNull(map.FindLandmark(name));
            Assert.Null(map.FindLandmark("building 6"));
        }

        [Fact]
        public void FromBuilding3YouCanSeeTheBridge()
        {
            // Where the squad leader sat at the end of the real mission: "bldg 3, eyes on bridge".
            var map = CinderFord();
            var from = map.FindLandmark("building 3").Center;
            var bridge = map.FindLandmark("the bridge").Center;
            Assert.True(Sight.Trace(map, from, Standing, bridge, Standing).Clear);
        }

        [Fact]
        public void FromTheTopOfHill151YouSeeOverTheHedgeToBuilding4()
        {
            var map = CinderFord();
            Assert.True(Sight.Trace(map, At(7.1f, 5f), Standing, At(6.825f, 2.62f), Standing).Clear);
        }

        [Fact]
        public void Hill151BlocksSightAcrossItsFoot()
        {
            var map = CinderFord();
            Assert.False(Sight.Trace(map, At(6.15f, 5f), Standing, At(7.99f, 5f), Standing).Clear);
        }

        [Fact]
        public void PineWoodHidesSomeoneDeepInside()
        {
            var map = CinderFord();
            Assert.False(Sight.Trace(map, At(3f, 4.8f), Standing, At(1f, 4.8f), Standing).Clear);
        }

        [Fact]
        public void RejectsABrokenMapWithAPlainMessage()
        {
            var terrains = GameData.FromJson(Read("terrain.json")).Terrains;
            var bad = MapData.FromJson(@"{ ""name"": ""Bad"", ""columns"": 2, ""rows"": 2, ""baseTerrain"": ""grass"",
                ""features"": [ { ""kind"": ""area"", ""label"": ""Lava Lake"", ""terrain"": ""lava"", ""rect"": [0, 0, 1, 1] } ] }");
            var e = Assert.Throws<FormatException>(() => bad.Build(terrains));
            Assert.Contains("Lava Lake", e.Message);
            Assert.Contains("lava", e.Message);
        }
    }
}
