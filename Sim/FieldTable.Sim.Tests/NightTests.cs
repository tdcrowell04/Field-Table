using Xunit;

namespace FieldTable.Sim.Tests
{
    public class NightTests
    {
        static readonly UnitDef Eyes = new UnitDef { Id = "eyes", Name = "No gear" };
        static readonly UnitDef Nods = new UnitDef { Id = "nods", Name = "NODs", NightVision = true };
        static readonly UnitDef Thermal = new UnitDef { Id = "thermal", Name = "Thermal", Thermal = true };
        static readonly UnitDef Target = new UnitDef { Id = "target", Name = "Target" };

        static readonly Force Blue = new Force { Id = "blue", Name = "Raider 2" };
        static readonly Force Red = new Force { Id = "red", Name = "Enemy" };

        static float Rate(MapGrid map, UnitDef gear, float distance, LightLevel light, Posture posture = Posture.Standing, float targetX = -1)
        {
            var observer = new Unit("o", "Observer", gear, Blue, new Vec2(100, 300));
            var target = new Unit("t", "Target", Target, Red, new Vec2(targetX >= 0 ? targetX : 100 + distance, 300)) { Posture = posture };
            return FogOfWar.SpotRate(map, observer, target, light);
        }

        [Fact]
        public void AtNightTheNakedEyeOnlyReachesFiftyMeters()
        {
            var map = MapGridTests.Map();
            Assert.True(Rate(map, Eyes, 40, LightLevel.Night) > 0f);
            Assert.Equal(0f, Rate(map, Eyes, 60, LightLevel.Night));
        }

        [Fact]
        public void AtTwilightTheNakedEyeReachesFourHundredMeters()
        {
            var map = MapGridTests.Map();
            Assert.True(Rate(map, Eyes, 390, LightLevel.Twilight) > 0f);
            Assert.Equal(0f, Rate(map, Eyes, 410, LightLevel.Twilight));
        }

        [Fact]
        public void NodsReachThreeHundredMetersAtNightAndDontHelpByDay()
        {
            var map = MapGridTests.Map();
            Assert.True(Rate(map, Nods, 290, LightLevel.Night) > 0f);
            Assert.Equal(0f, Rate(map, Nods, 310, LightLevel.Night));
            Assert.Equal(Rate(map, Eyes, 290, LightLevel.Day), Rate(map, Nods, 290, LightLevel.Day));
        }

        [Fact]
        public void NodsSpotMoreSlowlyThanEyesInDaylight()
        {
            var map = MapGridTests.Map();
            Assert.Equal(Rate(map, Eyes, 200, LightLevel.Day) * 0.6f, Rate(map, Nods, 200, LightLevel.Night), 4);
        }

        [Fact]
        public void AThermalSeesFarAtNight()
        {
            var map = MapGridTests.Map();
            Assert.True(Rate(map, Thermal, 650, LightLevel.Night) > 0f);
        }

        [Fact]
        public void AThermalSeesSomeoneInReedsBetterThanEyes()
        {
            var map = MapGridTests.Map();
            map.FillRect(480, 250, 40, 100, "reeds");
            float eyes = Rate(map, Eyes, 0, LightLevel.Day, Posture.Prone, 485);
            float thermal = Rate(map, Thermal, 0, LightLevel.Day, Posture.Prone, 485);
            Assert.True(thermal > eyes * 2f);
        }

        [Fact]
        public void AThermalCantSeeThroughABuilding()
        {
            var map = MapGridTests.Map();
            map.FillRect(300, 290, 14, 20, "building");
            Assert.Equal(0f, Rate(map, Thermal, 400, LightLevel.Night));
        }

        [Fact]
        public void MarinesWithNodsSpotAnEnemyDuringANightTurn()
        {
            var sim = new Simulation(MapGridTests.Map(), 5) { Light = LightLevel.Night };
            var blue = sim.AddForce(new Force { Id = "blue", Name = "Raider 2" });
            var red = sim.AddForce(new Force { Id = "red", Name = "Enemy" });
            sim.AddUnit(new Unit("alpha-tl", "Alpha TL", Nods, blue, new Vec2(100, 300)));
            sim.AddUnit(new Unit("pkm", "PKM gunner", Target, red, new Vec2(300, 300)));
            sim.RunTurn();
            Assert.True(sim.Fog.For(blue).About("pkm").Visible);
            Assert.Null(sim.Fog.For(red).About("alpha-tl"));
        }
    }
}
