using System.Linq;
using Xunit;

namespace FieldTable.Sim.Tests
{
    public class MovementTests
    {
        [Fact]
        public void AMarineWalksAboutEightyMetersInAMinute()
        {
            var s = new Scene();
            var m = s.Add("reyes", "rf", s.Blue, 100, 350);
            Assert.True(s.Order(m, Verbs.Move, "G4").Ok);
            s.Sim.RunTurn();
            Assert.InRange(m.Position.X - 100, 78f, 86f);
            Assert.InRange(m.Position.Y, 349f, 351f);
        }

        [Fact]
        public void RunningCoversMoreThanTwiceTheGround()
        {
            var s = new Scene();
            var walker = s.Add("w", "rf", s.Blue, 50, 200);
            var runner = s.Add("r", "rf", s.Blue, 50, 400);
            s.Order(walker, Verbs.Move, "H3");
            s.Order(runner, Verbs.Move, "H5", speed: "run");
            s.Sim.RunTurn();
            Assert.True(runner.Position.X - 50 > 2f * (walker.Position.X - 50));
        }

        [Fact]
        public void PlowedFieldsAreSlowerThanGrass()
        {
            var s = new Scene(paint: m => m.FillRect(0, 350, 800, 100, "plowed"));
            var grass = s.Add("g", "rf", s.Blue, 50, 200);
            var mud = s.Add("p", "rf", s.Blue, 50, 400);
            s.Order(grass, Verbs.Move, "H3");
            s.Order(mud, Verbs.Move, "H5");
            s.Sim.RunTurn();
            Assert.True(mud.Position.X < grass.Position.X - 10f);
        }

        [Fact]
        public void WithoutSwimmingTheRouteGoesAroundDeepWater()
        {
            var s = new Scene(paint: m => m.FillRect(300, 150, 60, 300, "lake"));
            s.Sim.RecordFrames(s.Blue);
            var m = s.Add("m", "rf", s.Blue, 250, 300);
            var check = s.Order(m, Verbs.Move, "E4", speed: "run");
            Assert.True(check.Ok);
            for (int t = 0; t < 4 && m.Order != null; t++)
            {
                s.Sim.RunTurn();
                Assert.All(s.Sim.Frames, f => Assert.False(s.Map.TerrainAt(new Vec2(f.Units[0].X, f.Units[0].Y)).Deep));
            }
            Assert.True(s.Map.SquareOf(m.Position).ToString() == "E4");
        }

        [Fact]
        public void AFullyKittedMarineToldToSwimGoesUnderAndDrowns()
        {
            var s = new Scene(paint: m => m.FillRect(300, 0, 120, 600, "river"));
            var m = s.Add("lund", "rf", s.Blue, 290, 300);
            var check = s.Order(m, Verbs.Move, "E4", swim: true);
            Assert.True(check.Ok);
            Assert.Contains(check.Warnings, w => w.Contains("too deep to swim in full kit"));
            s.Sim.RunTurn();
            s.Sim.RunTurn();
            Assert.Equal(Condition.Dead, m.Condition);
            Assert.Contains(s.Events<UnitDied>(), e => e.Unit == m && e.Cause == "drowned");
            Assert.Contains(s.Events<RadioCall>(), e => e.Text == "lund is going under!");
        }

        [Fact]
        public void DroppingGearFirstLetsHimSwimAcross()
        {
            var s = new Scene(paint: m => m.FillRect(300, 0, 40, 600, "river"));
            var m = s.Add("ortiz", "rf", s.Blue, 290, 300);
            Assert.True(s.Order(m, Verbs.Drop).Ok);
            s.Sim.RunTurn();
            Assert.Equal(Simulation.StrippedLoad, m.Load);
            Assert.True(s.Order(m, Verbs.Move, "D4", swim: true).Ok);
            for (int t = 0; t < 6 && m.Order != null; t++) s.Sim.RunTurn();
            Assert.Equal(Condition.Ok, m.Condition);
            Assert.True(m.Position.X > 340f);
        }

        [Fact]
        public void WithoutSwimmingThereIsNoWayAcrossARiverWithNoBridge()
        {
            var s = new Scene(paint: m => m.FillRect(300, 0, 40, 600, "river"));
            var m = s.Add("m", "rf", s.Blue, 290, 300);
            var check = s.Order(m, Verbs.Move, "E4");
            Assert.False(check.Ok);
            Assert.Equal("Can't get there without swimming, Lead.", check.Problem);
        }

        [Fact]
        public void ABridgeGetsThemAcrossWithoutSwimming()
        {
            var s = new Scene(paint: m => { m.FillRect(300, 0, 40, 600, "river"); m.FillRect(295, 295, 50, 8, "bridge"); });
            var m = s.Add("m", "rf", s.Blue, 290, 200);
            Assert.True(s.Order(m, Verbs.Move, "E3").Ok);
            for (int t = 0; t < 5 && m.Order != null; t++) s.Sim.RunTurn();
            Assert.Null(m.Order);
            Assert.Equal(Condition.Ok, m.Condition);
            Assert.Equal("E3", s.Map.SquareOf(m.Position).ToString());
        }

        [Fact]
        public void ATeamSpreadsOutAndReportsSet()
        {
            var s = new Scene();
            var a = new[] { s.Add("a1", "tl", s.Blue, 100, 300, "Alpha"), s.Add("a2", "ar", s.Blue, 100, 306, "Alpha"), s.Add("a3", "aar", s.Blue, 100, 312, "Alpha"), s.Add("a4", "rf", s.Blue, 100, 318, "Alpha") };
            var checks = s.Sim.GiveOrder(s.Blue, "Alpha", new Order { Verb = Verbs.Move, Where = "C4" });
            Assert.Equal(4, checks.Count);
            Assert.All(checks, c => Assert.True(c.Ok));
            for (int t = 0; t < 3 && a.Any(u => u.Order != null); t++) s.Sim.RunTurn();
            var ends = a.Select(u => u.Position).ToList();
            for (int i = 0; i < ends.Count; i++)
                for (int j = i + 1; j < ends.Count; j++)
                    Assert.True(Vec2.Distance(ends[i], ends[j]) >= 4f);
            Assert.Contains(s.Events<RadioCall>(), e => e.Text == "Alpha: Set at C4.");
        }

        [Fact]
        public void AnUnknownPlaceGetsASayAgain()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 300);
            var check = s.Order(m, Verbs.Move, "the moon");
            Assert.False(check.Ok);
            Assert.Equal("Say again, where is the moon?", check.Problem);
        }

        [Fact]
        public void TheWoodsAtASquareMeansTheNearestPartOfThem()
        {
            var s = new Scene(paint: m => m.FillRect(240, 220, 40, 60, "forest"));
            var m = s.Add("m", "rf", s.Blue, 100, 250);
            Assert.True(s.Sim.TryPlace(m, "the woods at C3", out Vec2 p));
            Assert.Equal("forest", s.Map.TerrainAt(p).Id);
            Assert.InRange(p.X, 240f, 243f);
            Assert.True(s.Order(m, Verbs.Move, "woods in C3").Ok);
            var none = s.Order(m, Verbs.Move, "the reeds at C3");
            Assert.False(none.Ok);
            Assert.Equal("Say again, where is the reeds at C3?", none.Problem);
        }

        [Fact]
        public void AVerbThatIsNotBuiltYetGoesToTheAI()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 300);
            var check = s.Order(m, "pray");
            Assert.False(check.Ok);
            Assert.StartsWith("not covered", check.Problem);
        }

        [Fact]
        public void CrawlingIsSlowAndKeepsThemProne()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 300);
            s.Order(m, Verbs.Move, "G4", speed: "crawl");
            s.Sim.RunTurn();
            Assert.Equal(Posture.Prone, m.Posture);
            Assert.InRange(m.Position.X - 100, 12f, 22f);
        }

        [Fact]
        public void ADanceShowsForTenSeconds()
        {
            var s = new Scene();
            s.Sim.RecordFrames(s.Blue);
            var m = s.Add("m", "rf", s.Blue, 100, 300);
            Assert.True(s.Order(m, Verbs.Emote, null).Ok);
            m.Order.Kind = "dance";
            s.Sim.RunTurn();
            Assert.Contains(s.Sim.Frames, f => f.Units[0].Emote == "dance");
            Assert.Null(m.Emote);
        }
    }
}
