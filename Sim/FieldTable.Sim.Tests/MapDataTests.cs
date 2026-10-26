using System;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class MapDataTests
    {
        const string Small = @"{
  ""name"": ""Test Ford"", ""columns"": 3, ""rows"": 2, ""squareSize"": 100, ""baseTerrain"": ""grass"",
  ""features"": [
    { ""kind"": ""hill"", ""label"": ""Hill 60"", ""terrain"": ""scrub"", ""rect"": [0, 0, 1, 1], ""height"": 60 },
    { ""kind"": ""line"", ""label"": ""Route Tin"", ""terrain"": ""road"", ""width"": 6, ""points"": [[0, 1.5], [3, 1.5]] },
    { ""kind"": ""line"", ""label"": ""Mud Creek"", ""aliases"": [""the creek""], ""terrain"": ""creek"", ""width"": 10, ""points"": [[1.5, 0], [1.5, 2]] },
    { ""kind"": ""line"", ""label"": ""Tin bridge"", ""terrain"": ""bridge"", ""width"": 5, ""points"": [[1.4, 1.5], [1.6, 1.5]] },
    { ""kind"": ""building"", ""label"": ""building 1"", ""aliases"": [""bldg 1""], ""terrain"": ""building"", ""center"": [2.5, 0.5], ""size"": [14, 10] },
    { ""kind"": ""control"", ""label"": ""PL Lead"", ""points"": [[2, 0], [2, 2]] }
  ]
}";

        static MapGrid Build(string json) => MapData.FromJson(json).Build(GameData.FromJson(Read("terrain.json")).Terrains);

        [Fact]
        public void PaintsEveryKindOfFeature()
        {
            var map = Build(Small);
            Assert.Equal(3, map.Columns);
            Assert.Equal(2, map.Rows);
            Assert.InRange(map.GroundAt(new Vec2(50, 50)), 59f, 60f);
            Assert.Equal("scrub", map.TerrainAt(new Vec2(50, 50)).Id);
            Assert.Equal("road", map.TerrainAt(new Vec2(50, 150)).Id);
            Assert.Equal("creek", map.TerrainAt(new Vec2(150, 50)).Id);
            Assert.Equal("bridge", map.TerrainAt(new Vec2(150, 150)).Id);
            Assert.Equal("building", map.TerrainAt(new Vec2(250, 50)).Id);
        }

        [Fact]
        public void KnowsItsNamedPlaces()
        {
            var map = Build(Small);
            Assert.Equal("Mud Creek", map.FindLandmark("the creek").Label);
            Assert.Equal(new Vec2(250, 50), map.FindLandmark("bldg 1").Center);
            Assert.Equal("control", map.FindLandmark("PL Lead").Kind);
            Assert.Null(map.FindLandmark("building 2"));
        }

        [Fact]
        public void RejectsABrokenMapWithAPlainMessage()
        {
            var e = Assert.Throws<FormatException>(() => Build(@"{ ""name"": ""Bad"", ""columns"": 2, ""rows"": 2, ""baseTerrain"": ""grass"",
                ""features"": [ { ""kind"": ""area"", ""label"": ""Lava Lake"", ""terrain"": ""lava"", ""rect"": [0, 0, 1, 1] } ] }"));
            Assert.Contains("Lava Lake", e.Message);
            Assert.Contains("lava", e.Message);
        }
    }
}
