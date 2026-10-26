using System;
using Xunit;

namespace FieldTable.Sim.Tests
{
    public class GameDataTests
    {
        const string Sample = @"{
  ""terrains"": [
    { ""id"": ""open"", ""name"": ""Open ground"", ""moveFactor"": 1.0 },
    { ""id"": ""water"", ""name"": ""River"", ""moveFactor"": 0.0 },
    { ""id"": ""building"", ""name"": ""Building"", ""moveFactor"": 0.5, ""cover"": 0.8, ""blocksSight"": true }
  ],
  ""units"": [
    { ""id"": ""rifleman"", ""name"": ""Rifleman"", ""weapon"": ""M27"", ""ammo"": 210 },
    { ""id"": ""pkm-gunner"", ""name"": ""Machine gunner"", ""weapon"": ""PKM"", ""ammo"": 400, ""walkSpeed"": 1.2 }
  ]
}";

        [Fact]
        public void LoadsTerrainsAndUnits()
        {
            var data = GameData.FromJson(Sample);
            Assert.Equal(3, data.Terrains.Count);
            Assert.True(data.Terrains[2].BlocksSight);
            Assert.Equal(0.8f, data.Terrains[2].Cover);
            var pkm = data.FindUnit("pkm-gunner");
            Assert.Equal(400, pkm.Ammo);
            Assert.Equal(1.2f, pkm.WalkSpeed);
            Assert.Equal(3.5f, pkm.RunSpeed);
        }

        [Fact]
        public void LoadedTerrainBuildsAMap()
        {
            var data = GameData.FromJson(Sample);
            var map = new MapGrid("From data", 100, 100, 1f, 50f, data.Terrains, "open");
            map.SetTerrain(5, 5, "building");
            Assert.Equal("Building", map.TerrainAt(5, 5).Name);
        }

        [Theory]
        [InlineData(@"{ ""terrains"": [ { ""id"": ""open"" }, { ""id"": ""open"" } ], ""units"": [] }")]
        [InlineData(@"{ ""terrains"": [ { ""name"": ""No id"" } ], ""units"": [] }")]
        [InlineData(@"{ ""terrains"": [ { ""id"": ""x"", ""cover"": 2 } ], ""units"": [] }")]
        [InlineData(@"{ ""terrains"": [], ""units"": [ { ""id"": ""u"", ""walkSpeed"": 0 } ] }")]
        [InlineData(@"{ ""terrains"": [], ""units"": [], ""typo"": 1 }")]
        [InlineData(@"{ ""terrains"": null, ""units"": [] }")]
        [InlineData(@"not json")]
        [InlineData(@"")]
        public void RejectsBadDataWithAPlainMessage(string json)
        {
            var e = Assert.Throws<FormatException>(() => GameData.FromJson(json));
            Assert.False(string.IsNullOrWhiteSpace(e.Message));
        }

        [Fact]
        public void SaysWhichUnitIsMissing()
        {
            var data = GameData.FromJson(Sample);
            var e = Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => data.FindUnit("tank"));
            Assert.Contains("tank", e.Message);
        }
    }
}
