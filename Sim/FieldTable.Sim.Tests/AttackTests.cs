using System.Linq;
using Xunit;

namespace FieldTable.Sim.Tests
{
    public class AttackTests
    {
        /// <summary>A squad dug in at one end, an enemy group 250 m off that knows where they are.</summary>
        static (Scene s, System.Collections.Generic.List<Unit> enemy, Vec2 objective) Setup(int riflemen, bool gun = true)
        {
            var s = new Scene(paint: m => { m.FillRect(380, 150, 30, 30, "scrub"); m.FillRect(380, 420, 30, 30, "scrub"); m.FillRect(250, 270, 20, 20, "rocky"); });
            var held = new Vec2(400, 300);
            var blue = new[] { s.Add("a", "rf", s.Blue, held.X, held.Y, posture: Posture.Prone), s.Add("b", "rf", s.Blue, held.X + 4, held.Y + 4, posture: Posture.Prone) };
            foreach (var u in blue) u.Ammo = 0;
            var enemy = new System.Collections.Generic.List<Unit>();
            if (gun) enemy.Add(s.Add("pkm", "en-pkm", s.Red, 150, 300));
            for (int i = 0; i < riflemen; i++) enemy.Add(s.Add("r" + i, i == 0 ? "en-sl" : i == 1 ? "en-rpg" : "en-rf", s.Red, 150 + (i % 3) * 4, 290 + (i / 3) * 6));
            foreach (var u in enemy) u.Ammo = 0;
            foreach (var x in blue) { var k = s.Sim.Fog.For(s.Red).Track(x); k.Known = true; k.LastKnownPosition = x.Position; k.LastSeenStep = 0; }
            return (s, enemy, held);
        }

        static void Seconds(Scene s, Attack a, int n)
        {
            for (int i = 0; i < n; i++)
            {
                a.Tick();
                for (int k = 0; k < SimClock.StepsPerSecond; k++) s.Sim.StepForTest();
            }
        }

        [Fact]
        public void TheGunHoldsThemDownWhileAThirdSwingsRoundTheSide()
        {
            var (s, enemy, held) = Setup(9);
            var attack = new Attack(s.Sim, enemy, "attack 1", held, s.Blue);
            attack.Tick();
            Assert.Equal(Verbs.Suppress, enemy.Single(u => u.Id == "pkm").Order?.Verb);
            Assert.Equal(3, attack.FlankParty.Count);
            Assert.Contains(attack.FlankParty, u => u.Def.Id == "en-rpg");
            Seconds(s, attack, 90);
            // The flank is well off the straight line in; the main party isn't.
            float Off(Unit u) => System.Math.Abs(u.Position.Y - held.Y);
            Assert.True(attack.FlankParty.Where(u => u.CanAct).Average(Off) >= 45f, "The flank stayed on the straight line: " + attack.FlankParty.Average(Off) + " m off.");
        }

        [Fact]
        public void TheyStopInCoverOnTheWayRatherThanWalkingStraightIn()
        {
            var (s, enemy, held) = Setup(4, gun: false);
            var attack = new Attack(s.Sim, enemy, "attack 1", held, s.Blue);
            // Smoke goes out first over the open ground, then the rush.
            Seconds(s, attack, 3);
            var first = enemy.Where(u => u.Order != null && u.Order.Verb == Verbs.Bound).ToList();
            Assert.NotEmpty(first);
            // The first rush goes about one leg on, to the rocks, not all the way to the objective.
            Assert.All(first, u => Assert.True(Vec2.Distance(u.Order.Point.Value, held) > 100f));
            Assert.Empty(attack.FlankParty);
        }

        [Fact]
        public void SmokeGoesOutBeforeARushAcrossOpenGround()
        {
            var s = new Scene();
            var held = new Vec2(400, 300);
            var blue = s.Add("a", "rf", s.Blue, held.X, held.Y, posture: Posture.Prone);
            blue.Ammo = 0;
            var enemy = new System.Collections.Generic.List<Unit> { s.Add("sl", "en-sl", s.Red, 250, 300), s.Add("r1", "en-rf", s.Red, 254, 300), s.Add("r2", "en-rf", s.Red, 250, 306) };
            foreach (var u in enemy) u.Ammo = 0;
            var k = s.Sim.Fog.For(s.Red).Track(blue); k.Known = true; k.LastKnownPosition = blue.Position; k.LastSeenStep = 0;
            var attack = new Attack(s.Sim, enemy, "attack 1", held, s.Blue);
            Seconds(s, attack, 5);
            Assert.Contains(s.Events<SmokeAt>(), e => e.Thrower.Force == s.Red);
            // Their smoke isn't called on our net.
            Assert.DoesNotContain(s.Events<RadioCall>(), r => r.Text.EndsWith("Smoke out."));
        }
    }
}
