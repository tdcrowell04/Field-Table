using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class MapGeneratorTests
    {
        static readonly Lazy<List<TerrainDef>> terrains = new Lazy<List<TerrainDef>>(() => GameData.FromJson(Read("terrain.json")).Terrains);
        static readonly Lazy<NamePools> names = new Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static MapData Gen(ulong seed, Action<MapRequest> setup = null)
        {
            var req = new MapRequest { Seed = seed, Name = "Test " + seed };
            setup?.Invoke(req);
            return MapGenerator.Generate(req, names.Value);
        }

        static MapGrid Build(MapData data) => data.Build(terrains.Value);

        [Fact]
        public void TheSameSeedGivesTheSameMap()
        {
            Assert.Equal(Gen(11).ToJson(), Gen(11).ToJson());
            Assert.NotEqual(Gen(11).ToJson(), Gen(12).ToJson());
        }

        [Fact]
        public void EveryOneOfTheFifteenFeaturesShowsUpAndMapsVary()
        {
            var seen = new HashSet<string>();
            var counts = new List<int>();
            for (ulong seed = 1; seed <= 120; seed++)
            {
                var data = Gen(seed);
                if (seed % 10 == 0) Build(data);
                seen.UnionWith(data.Includes);
                counts.Add(data.Includes.Count);
            }
            Assert.Empty(MapFeatures.All.Except(seen));
            Assert.Contains(counts, c => c <= 4);
            Assert.Contains(counts, c => c >= 10);
        }

        [Theory]
        [InlineData("bridges")]
        [InlineData("villages")]
        [InlineData("towns")]
        [InlineData("lakes")]
        [InlineData("forest")]
        [InlineData("marsh")]
        [InlineData("gullies")]
        [InlineData("ridgelines")]
        public void ARequiredFeatureIsAlwaysThere(string feature)
        {
            for (ulong seed = 1; seed <= 40; seed++)
            {
                var data = Gen(seed, r => r.Require.Add(feature));
                Assert.Contains(feature, data.Includes);
                if (seed % 10 == 0) Build(data);
            }
        }

        [Fact]
        public void AnExcludedFeatureIsNeverThere()
        {
            for (ulong seed = 1; seed <= 40; seed++)
            {
                var data = Gen(seed, r => { r.Exclude.Add("rivers"); r.Exclude.Add("towns"); });
                Assert.DoesNotContain("rivers", data.Includes);
                Assert.DoesNotContain("towns", data.Includes);
                Assert.DoesNotContain(data.Features, f => f.Terrain == "river");
            }
        }

        [Fact]
        public void ABridgeSeizureMapHasABridgeOverWaterOnARoad()
        {
            for (ulong seed = 1; seed <= 15; seed++)
            {
                var data = Gen(seed, r => { r.Require.Add("bridges"); r.Require.Add("villages"); });
                var map = Build(data);
                var bridge = map.FindLandmark("bridge 1");
                Assert.NotNull(bridge);
                Assert.Equal("bridge", map.TerrainAt(bridge.Center).Id);
                Assert.Contains(data.Includes, f => f == "rivers" || f == "creeks");
                Assert.Contains("roads", data.Includes);
            }
        }

        [Fact]
        public void MapsGetCommonSenseCoverUnlessTheScenarioIsOpenGround()
        {
            int Cover(MapData d) => d.Features.Count(f => f.Terrain == "hedge" || f.Terrain == "forest" || f.Terrain == "wall" || f.Terrain == "gully");
            for (ulong seed = 1; seed <= 10; seed++)
            {
                var covered = Gen(seed, r => { r.Require.Add("bridges"); r.Exclude.Add("forest"); r.Exclude.Add("woodlots"); r.Exclude.Add("hedgerows"); r.Exclude.Add("gullies"); r.Exclude.Add("hills"); });
                Assert.Equal(0, Cover(covered));
                var open = Gen(seed, r => { r.Require.Add("bridges"); r.OpenGround = true; });
                var normal = Gen(seed, r => r.Require.Add("bridges"));
                Assert.True(Cover(normal) > Cover(open), "Seed " + seed + ": no cover was added.");
            }
        }

        [Fact]
        public void HillsAreRandomFromFifteenToOneHundredFiftyMeters()
        {
            var heights = new List<float>();
            for (ulong seed = 1; seed <= 80; seed++)
                heights.AddRange(Gen(seed).Features.Where(f => f.Kind == "hill").Select(f => f.Height));
            Assert.NotEmpty(heights);
            Assert.All(heights, hgt => Assert.InRange(hgt, 15f, 150f));
            Assert.Contains(heights, hgt => hgt < 50f);
            Assert.Contains(heights, hgt => hgt > 110f);
        }

        [Fact]
        public void AskingTheGameMasterForAHillGetsThatHill()
        {
            var data = Gen(5, r => r.HillHeight = 100f);
            var map = Build(data);
            var hill = map.FindLandmark("Hill 100");
            Assert.NotNull(hill);
            Assert.InRange(map.GroundAt(hill.Center), 95f, 101f);
        }

        [Fact]
        public void BuildingsAreNumberedAndNeverInWater()
        {
            for (ulong seed = 1; seed <= 15; seed++)
            {
                var data = Gen(seed, r => r.Require.Add("towns"));
                var map = Build(data);
                var blds = map.Landmarks.Where(l => l.Kind == "building").ToList();
                Assert.NotEmpty(blds);
                Assert.Equal(blds.Count, blds.Select(b => b.Label).Distinct().Count());
                Assert.NotNull(map.FindLandmark("building 1"));
                Assert.NotNull(map.FindLandmark("bldg " + blds.Count));
                foreach (var b in blds)
                {
                    Assert.Equal("building", map.TerrainAt(b.Center).Id);
                    Assert.False(map.TerrainAt(new Vec2(b.Min.X - 2, b.Center.Y)).Deep);
                }
            }
        }

        [Fact]
        public void WithoutBridgesARoadNeverRunsThroughARiver()
        {
            int checkedMaps = 0;
            for (ulong seed = 1; seed <= 150 && checkedMaps < 15; seed++)
            {
                var data = Gen(seed, r => { r.Require.Add("rivers"); r.Require.Add("roads"); r.Exclude.Add("bridges"); });
                var river = data.Features.First(f => f.Terrain == "river");
                var riverPts = river.Points.Select(p => new Vec2(p[0] * 100, p[1] * 100)).ToList();
                foreach (var road in data.Features.Where(f => f.Terrain == "road" && f.Label != null && f.Label.StartsWith("Route ")))
                {
                    var pts = road.Points.Select(p => new Vec2(p[0] * 100, p[1] * 100)).ToList();
                    for (int i = 0; i + 1 < pts.Count; i++)
                        for (float t = 0; t <= 1f; t += 0.02f)
                        {
                            Vec2 p = pts[i] + (pts[i + 1] - pts[i]) * t;
                            float d = float.MaxValue;
                            for (int j = 0; j + 1 < riverPts.Count; j++) d = Math.Min(d, MapGrid.DistanceToSegment(p, riverPts[j], riverPts[j + 1]));
                            Assert.True(d > river.Width / 2f, "Seed " + seed + ": " + road.Label + " runs through the river.");
                        }
                }
                checkedMaps++;
            }
            Assert.True(checkedMaps > 0);
        }

        [Fact]
        public void NoTwoPlacesShareAName()
        {
            for (ulong seed = 1; seed <= 60; seed++)
            {
                var map = Build(Gen(seed, r => r.Require.Add("bridges")));
                var labels = map.Landmarks.Select(l => l.Label.ToLowerInvariant()).ToList();
                Assert.Equal(labels.Count, labels.Distinct().Count());
            }
        }

        [Fact]
        public void AGeneratedMapCanBeSavedAndLoadedAgain()
        {
            var data = Gen(21, r => r.Require.Add("villages"));
            var direct = Build(data);
            var loaded = Build(MapData.FromJson(data.ToJson()));
            for (int y = 0; y < direct.Height; y += 7)
                for (int x = 0; x < direct.Width; x += 7)
                {
                    Assert.Equal(direct.TerrainAt(x, y).Id, loaded.TerrainAt(x, y).Id);
                    Assert.Equal(direct.GroundAt(x, y), loaded.GroundAt(x, y), 3);
                }
            Assert.Equal(direct.Landmarks.Count, loaded.Landmarks.Count);
        }

        [Fact]
        public void ATurnPlaysOnAGeneratedMap()
        {
            var map = Build(Gen(8));
            var sim = new Simulation(map, 8);
            var blue = sim.AddForce(new Force { Id = "blue", Name = "Raider 2" });
            var red = sim.AddForce(new Force { Id = "red", Name = "Enemy" });
            var def = new UnitDef { Id = "rifleman", Name = "Rifleman" };
            sim.AddUnit(new Unit("a", "Alpha TL", def, blue, new Vec2(50, 50)));
            sim.AddUnit(new Unit("e", "Enemy", def, red, new Vec2(750, 550)));
            sim.RunTurn();
            Assert.Equal(2, sim.Clock.Turn);
        }

        [Theory]
        [InlineData("dragons")]
        public void RejectsAnUnknownFeature(string feature)
        {
            Assert.Throws<ArgumentException>(() => Gen(1, r => r.Require.Add(feature)));
        }

        [Fact]
        public void RejectsRequestsThatCantWork()
        {
            Assert.Throws<ArgumentException>(() => Gen(1, r => { r.Require.Add("bridges"); r.Exclude.Add("roads"); }));
            Assert.Throws<ArgumentException>(() => Gen(1, r => { r.Require.Add("hills"); r.Exclude.Add("hills"); }));
            Assert.Throws<ArgumentException>(() => Gen(1, r => r.HillHeight = 200f));
        }
    }
}
