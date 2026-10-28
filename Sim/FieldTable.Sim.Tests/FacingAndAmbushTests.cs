using System.Linq;
using Xunit;

namespace FieldTable.Sim.Tests
{
    public class FacingAndAmbushTests
    {
        static Unit[] Team(Scene s, string name, float x, float y) => new[]
        {
            s.Add(name.ToLowerInvariant() + "-tl", "tl", s.Blue, x, y, name),
            s.Add(name.ToLowerInvariant() + "-ar", "ar", s.Blue, x, y + 6, name),
            s.Add(name.ToLowerInvariant() + "-aar", "aar", s.Blue, x, y + 12, name),
            s.Add(name.ToLowerInvariant() + "-rf", "rf", s.Blue, x, y + 18, name),
        };

        [Theory]
        [InlineData("north", 0f, 45f)]
        [InlineData("East", 90f, 45f)]
        [InlineData("NE", 45f, 45f)]
        [InlineData("north and east", 45f, 90f)]
        [InlineData("North and East", 45f, 90f)]
        [InlineData("south and east", 135f, 90f)]
        [InlineData("the south and East", 135f, 90f)]
        [InlineData("north and west", 315f, 90f)]
        [InlineData("north-east", 45f, 45f)]
        [InlineData("west, north, east", 0f, 135f)]
        [InlineData("all around", 0f, 180f)]
        public void CompassWordsMakeAnArc(string words, float center, float half)
        {
            Assert.True(Directions.TryParse(words, out float c, out float h));
            Assert.Equal(center, c, 1);
            Assert.Equal(half, h, 1);
        }

        [Theory]
        [InlineData("")]
        [InlineData("up the hill")]
        [InlineData("left")]
        public void WordsThatArentDirectionsDont(string words) => Assert.False(Directions.TryParse(words, out _, out _));

        [Fact]
        public void OverwatchFacingNorthAndEastFromAPlace()
        {
            var s = new Scene();
            var a = Team(s, "Alpha", 100, 300);
            Assert.All(s.Sim.GiveOrder(s.Blue, "Alpha", new Order { Verb = Verbs.Overwatch, Where = "C4", Facing = "north and east" }), c => Assert.True(c.Ok));
            for (int t = 0; t < 4 && !s.Events<RadioCall>().Any(r => r.Kind == "fire"); t++) s.Sim.RunTurn();
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "Alpha: Set. Overwatch north and east.");
            Assert.All(a, u => Assert.Equal("C4", s.Map.SquareOf(u.Position).ToString()));
            Assert.All(a, u => Assert.Equal(45f, u.Facing, 1));
        }

        [Fact]
        public void OverwatchShootsWhatIsInItsArcFirst()
        {
            var s = new Scene();
            var m = s.Add("m", "ar", s.Blue, 300, 300);
            var west = s.Add("west", "en-rf", s.Red, 230, 300);
            var east = s.Add("east", "en-rf", s.Red, 420, 300);
            west.Ammo = east.Ammo = 0;
            for (int i = 0; i < 10; i++) s.Sim.LookAround();
            Assert.True(s.Sim.Give(m, new Order { Verb = Verbs.Overwatch, Facing = "east" }).Ok);
            s.Sim.RunTurn();
            var first = s.Events<ShotFired>().First(e => e.Shooter == m);
            Assert.True(Vec2.Distance(first.To, east.Position) < Vec2.Distance(first.To, west.Position));
        }

        [Fact]
        public void AFacingThatIsntADirectionGetsASayAgain()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 300);
            var check = s.Sim.Check(m, new Order { Verb = Verbs.Overwatch, Facing = "up the hill" });
            Assert.False(check.Ok);
            Assert.Equal("Say again, face which way?", check.Problem);
        }

        [Fact]
        public void HoldAndMoveTurnToTheWayTheyWereTold()
        {
            var s = new Scene();
            var a = s.Add("a", "rf", s.Blue, 100, 300);
            var b = s.Add("b", "rf", s.Blue, 100, 340);
            Assert.True(s.Sim.Give(a, new Order { Verb = Verbs.Hold, Facing = "south" }).Ok);
            Assert.True(s.Sim.Give(b, new Order { Verb = Verbs.Move, Where = "B4", Facing = "west" }).Ok);
            s.Sim.RunTurn();
            Assert.Equal(180f, a.Facing, 1);
            Assert.Equal(270f, b.Facing, 1);
        }

        [Fact]
        public void HidingAtANamedPlaceGoesThereAndGetsDown()
        {
            var s = new Scene(paint: m => m.FillRect(500, 280, 30, 30, "scrub"));
            var m0 = s.Add("m", "rf", s.Blue, 420, 300);
            Assert.True(s.Sim.Give(m0, new Order { Verb = Verbs.Hide, Where = s.Sim.Coordinates(new Vec2(515, 295)) }).Ok);
            for (int t = 0; t < 4 && m0.Order != null; t++) s.Sim.RunTurn();
            Assert.Null(m0.Order);
            Assert.Equal(Posture.Prone, m0.Posture);
            Assert.True(Vec2.Distance(m0.Position, new Vec2(515, 295)) <= 40f, "Hid " + Vec2.Distance(m0.Position, new Vec2(515, 295)) + " m from the scrub.");
        }

        [Fact]
        public void AnAmbushHoldsFireUntilTheyComeClose()
        {
            var s = new Scene();
            var c = Team(s, "Charlie", 300, 300);
            var e = s.Add("e", "en-rf", s.Red, 300, 120);
            e.Ammo = 0;
            Assert.All(s.Sim.GiveOrder(s.Blue, "Charlie", new Order { Verb = Verbs.Ambush }), x => Assert.True(x.Ok));
            s.Sim.RunTurn();
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "Charlie: Set. Ambush in place, holding fire.");
            Assert.True(s.Sim.Fog.For(s.Blue).About(e.Id)?.Visible == true, "They should see him coming.");
            Assert.DoesNotContain(s.Events<ShotFired>(), x => c.Contains(x.Shooter));
            Assert.All(c, u => Assert.Equal(Posture.Prone, u.Posture));
            // Facing whoever's been reported, since they weren't told a way to face.
            Assert.All(c, u => Assert.InRange(u.Facing, 0f, 30f));
            e.Position = new Vec2(300, 275);
            s.Sim.RunTurn();
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "Charlie: Ambush! Opening fire.");
            Assert.Contains(s.Events<ShotFired>(), x => c.Contains(x.Shooter));
        }

        [Fact]
        public void AnAmbushIsSprungWhenOneOfThemIsHit()
        {
            var s = new Scene();
            var c = Team(s, "Charlie", 300, 300);
            var e = s.Add("e", "en-rf", s.Red, 300, 120);
            e.Ammo = 0;
            Assert.All(s.Sim.GiveOrder(s.Blue, "Charlie", new Order { Verb = Verbs.Ambush, Facing = "north" }), x => Assert.True(x.Ok));
            s.Sim.RunTurn();
            Assert.DoesNotContain(s.Events<ShotFired>(), x => c.Contains(x.Shooter));
            c[3].Wounds = 1;
            s.Sim.RunTurn();
            Assert.Contains(s.Events<ShotFired>(), x => c.Contains(x.Shooter));
        }

        [Fact]
        public void HitOnTheWayToTheAmbushTheyStopAndFight()
        {
            var s = new Scene();
            var c = Team(s, "Charlie", 100, 300);
            var e = s.Add("e", "en-rf", s.Red, 300, 120);
            e.Ammo = 0;
            Assert.All(s.Sim.GiveOrder(s.Blue, "Charlie", new Order { Verb = Verbs.Ambush, Where = "F4" }), x => Assert.True(x.Ok));
            for (int i = 0; i < 10; i++) s.Sim.LookAround();
            c[1].Wounds = 1;
            s.Sim.RunTurn();
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "Charlie: We're spotted before we're set. Opening fire.");
            Assert.All(c, u => Assert.Null(u.Path));
            Assert.Contains(s.Events<ShotFired>(), x => c.Contains(x.Shooter));
        }

        [Fact]
        public void AnAmbushOnAKillZoneSpringsWhenTheyWalkIntoIt()
        {
            var s = new Scene();
            var c = Team(s, "Charlie", 300, 300);
            var e = s.Add("e", "en-rf", s.Red, 500, 120);
            e.Ammo = 0;
            Assert.All(s.Sim.GiveOrder(s.Blue, "Charlie", new Order { Verb = Verbs.Ambush, Target = s.Sim.Coordinates(new Vec2(450, 200)) }), x => Assert.True(x.Ok));
            s.Sim.RunTurn();
            Assert.DoesNotContain(s.Events<ShotFired>(), x => c.Contains(x.Shooter));
            e.Position = new Vec2(455, 205);
            s.Sim.RunTurn();
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "Charlie: Ambush! Opening fire.");
        }
    }
}
