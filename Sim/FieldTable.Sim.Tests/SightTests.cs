using Xunit;

namespace FieldTable.Sim.Tests
{
    public class SightTests
    {
        const float Standing = 1.7f, Prone = 0.4f;

        static SightResult Look(MapGrid map, float x0, float y0, float h0, float x1, float y1, float h1) =>
            Sight.Trace(map, new Vec2(x0, y0), h0, new Vec2(x1, y1), h1);

        [Fact]
        public void SeesAcrossOpenGround()
        {
            var r = Look(MapGridTests.Map(), 50, 300, Standing, 750, 300, Prone);
            Assert.True(r.Clear);
            Assert.Equal(0f, r.Obscured);
        }

        [Fact]
        public void ALowWallHidesSomeoneProneBehindItButNotSomeoneStanding()
        {
            var map = MapGridTests.Map();
            map.FillRect(198, 250, 1, 100, "wall");
            Assert.False(Look(map, 100, 300, Standing, 200.5f, 300, Prone).Clear);
            Assert.True(Look(map, 100, 300, Standing, 200.5f, 300, Standing).Clear);
        }

        [Fact]
        public void ABuildingBlocksSight()
        {
            var map = MapGridTests.Map();
            map.FillRect(300, 290, 14, 20, "building");
            Assert.False(Look(map, 100, 300, Standing, 500, 300, Standing).Clear);
        }

        [Fact]
        public void SomeoneInsideABuildingSeesOutItsWindowsAndCanBeSeen()
        {
            var map = MapGridTests.Map();
            map.FillRect(100, 293, 14, 14, "building");
            Assert.True(Look(map, 107, 300, Standing, 500, 300, Standing).Clear);
            Assert.True(Look(map, 500, 300, Standing, 107, 300, Standing).Clear);
        }

        [Fact]
        public void BeingInsideOneBuildingDoesntLetYouSeeThroughAnother()
        {
            var map = MapGridTests.Map();
            map.FillRect(100, 293, 14, 14, "building");
            map.FillRect(300, 290, 14, 20, "building");
            Assert.False(Look(map, 107, 300, Standing, 500, 300, Standing).Clear);
        }

        [Fact]
        public void AFewMetersOfPineThinTheViewAndThirtyHideIt()
        {
            var map = MapGridTests.Map();
            map.FillRect(300, 250, 15, 100, "forest");
            var thin = Look(map, 100, 300, Standing, 500, 300, Standing);
            Assert.True(thin.Clear);
            Assert.InRange(thin.Obscured, 0.5f, 0.7f);

            map.FillRect(300, 250, 30, 100, "forest");
            Assert.False(Look(map, 100, 300, Standing, 500, 300, Standing).Clear);
        }

        [Fact]
        public void AHillBlocksSightAndTheTopSeesBothSides()
        {
            var map = MapGridTests.Map();
            map.RaiseHill(new Vec2(300, 200), new Vec2(500, 400), 30f, null);
            Assert.InRange(map.GroundAt(new Vec2(400, 300)), 29f, 30f);
            Assert.False(Look(map, 100, 300, Standing, 700, 300, Standing).Clear);
            Assert.True(Look(map, 400, 300, Standing, 100, 300, Standing).Clear);
            Assert.True(Look(map, 400, 300, Standing, 700, 300, Standing).Clear);
        }

        [Fact]
        public void NoSightOffTheMap()
        {
            Assert.False(Look(MapGridTests.Map(), 100, 300, Standing, 900, 300, Standing).Clear);
        }
    }
}
