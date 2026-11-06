using System.Collections.Generic;
using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    /// <summary>The bugs the first agent playthrough found (playthroughs.md, run 1), each kept fixed.</summary>
    public class RunOneFixesTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static Unit[] Team(Scene s, string name, float x, float y) => new[]
        {
            s.Add(name.ToLowerInvariant() + "-tl", "tl", s.Blue, x, y, name),
            s.Add(name.ToLowerInvariant() + "-ar", "ar", s.Blue, x, y + 6, name),
            s.Add(name.ToLowerInvariant() + "-aar", "aar", s.Blue, x, y + 12, name),
            s.Add(name.ToLowerInvariant() + "-rf", "rf", s.Blue, x, y + 18, name),
        };

        [Fact]
        public void APlaceWithANoteInBracketsIsStillThatPlace()
        {
            var m = StartStates.Create(StartStates.ObservationPost, Scene.Data, names.Value, 1, null, "marines");
            var a = m.Squad.First(u => u.Team == "Alpha");
            var c = m.Sim.Check(a, new Order { Verb = Verbs.Move, Where = "the post (F5)" });
            Assert.True(c.Ok, c.Problem);
            // When only the note makes sense, the note is the place.
            var d = m.Sim.Check(a, new Order { Verb = Verbs.Move, Where = "the far slope (D3)" });
            Assert.True(d.Ok, d.Problem);
        }

        [Fact]
        public void AContactWithANoteInBracketsIsStillThatContact()
        {
            var s = new Scene();
            var a = s.Add("a", "rf", s.Blue, 100, 300, "Alpha");
            var b = s.Add("b", "rf", s.Blue, 110, 300, "Alpha");
            var c = s.Sim.Give(a, new Order { Verb = Verbs.Follow, Target = b.Name + " (friend, on our side)" });
            Assert.True(c.Ok, c.Problem);
            Assert.Same(b, a.Order.TargetUnit);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ATeamOrderInTheSameCallLeavesAMemberWithHisOwn(bool memberFirst)
        {
            var s = new Scene();
            var a = Team(s, "Alpha", 100, 300);
            var own = (a[3].Name, new Order { Verb = Verbs.Observe, Where = "C2" });
            var team = ("Alpha", new Order { Verb = Verbs.Move, Where = "C4" });
            var checks = s.Sim.GiveOrders(s.Blue, memberFirst ? new List<(string, Order)> { own, team } : new List<(string, Order)> { team, own });
            Assert.All(checks.SelectMany(x => x), c => Assert.True(c.Ok, c.Problem));
            Assert.Equal(Verbs.Observe, a[3].Order.Verb);
            Assert.All(a.Take(3), u => Assert.Equal(Verbs.Move, u.Order.Verb));
            // Only checking leaves everyone as they were, and shows the same split.
            var s2 = new Scene();
            var a2 = Team(s2, "Alpha", 100, 300);
            var preview = s2.Sim.GiveOrders(s2.Blue, new List<(string, Order)> { team, own }, give: false);
            Assert.Equal(3, preview[0].Count);
            Assert.All(a2, u => Assert.Null(u.Order));
        }

        [Fact]
        public void ATeamFollowsATeam()
        {
            var s = new Scene();
            var a = Team(s, "Alpha", 100, 300);
            var b = Team(s, "Bravo", 100, 400);
            Assert.All(s.Sim.GiveOrder(s.Blue, "Bravo", new Order { Verb = Verbs.Follow, Target = "Alpha" }), c => Assert.True(c.Ok, c.Problem));
            Assert.All(b, u => Assert.Same(a[0], u.Order.TargetUnit));
        }

        [Fact]
        public void ToldAgainWhileCarryingThemTheyKeepCarryingToThePlace()
        {
            var s = new Scene();
            var cas = s.Add("cas", "rf", s.Blue, 150, 215, "Alpha");
            var carrier = s.Add("carrier", "aar", s.Blue, 152, 215, "Alpha");
            s.Sim.KnockDownForTest(cas);
            var order = new Order { Verb = Verbs.Carry, Target = cas.Name, Where = "B2" };
            Assert.True(s.Sim.Give(carrier, order.Copy()).Ok);
            for (int i = 0; i < 200 && !ReferenceEquals(cas.CarriedBy, carrier); i++) s.Sim.StepForTest();
            Assert.Same(carrier, cas.CarriedBy);
            for (int i = 0; i < 50; i++) s.Sim.StepForTest();
            Assert.True(s.Sim.Give(carrier, order.Copy()).Ok);
            for (int t = 0; t < 8 && carrier.Order != null; t++) s.Sim.RunTurn();
            Assert.Contains(s.Events<RadioCall>(), r => r.Text.EndsWith(cas.Name + " is at B2."));
            Assert.Equal("B2", s.Map.SquareOf(cas.Position).ToString());
        }

        [Fact]
        public void TheInterpreterStaysPutWhenToldToHold()
        {
            var m = StartStates.Create(StartStates.ObservationPost, Scene.Data, names.Value, 34567, null, "marines");
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy).ToList()) m.Sim.RemoveUnitForTest(e);
            var terp = m.Sim.Units.First(u => u.Def.Interpreter && u.Force == m.Marines);
            var lead = m.Squad.First(u => u.Id == "lead");
            Assert.True(m.Sim.Give(terp, new Order { Verb = Verbs.Hold }).Ok);
            var at = terp.Position;
            Assert.True(m.Sim.Give(lead, new Order { Verb = Verbs.Move, Where = m.Sim.Coordinates(lead.Position + new Vec2(0f, -120f)) }).Ok);
            for (int t = 0; t < 3; t++) m.Sim.RunTurn();
            Assert.True(Vec2.Distance(terp.Position, at) < 3f, "The interpreter moved " + Vec2.Distance(terp.Position, at) + " m.");
            Assert.True(Vec2.Distance(lead.Position, at) > 60f);
        }

        [Fact]
        public void SmokePastAThrowGoesAsFarAsItCanTowardTheSpot()
        {
            var s = new Scene();
            var tl = s.Add("tl", "tl", s.Blue, 100, 300, "Alpha");
            tl.Smokes = 1;
            var c = s.Sim.Give(tl, new Order { Verb = Verbs.ThrowSmoke, Where = s.Sim.Coordinates(new Vec2(200f, 300f)) });
            Assert.True(c.Ok, c.Problem);
            Assert.Contains(c.Warnings, w => w.Contains("past a throw"));
            Assert.InRange(Vec2.Distance(tl.Order.Point.Value, tl.Position), 30f, Simulation.ThrowRange);
            Assert.True(tl.Order.Point.Value.X > tl.Position.X + 30f);
        }

        [Fact]
        public void HitBySomeoneNobodySeesTheTeamCallsTakingFireAndFromWhere()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 300, "Alpha");
            var e = s.Add("e", "en-rf", s.Red, 600, 300);
            s.Sim.Wound(m, Scene.Data.FindWeapon("m4"), e);
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "Alpha: Taking fire from the east! Can't see the shooter.");
            // Once every 30 seconds, not every round.
            s.Sim.Wound(m, Scene.Data.FindWeapon("m4"), e);
            Assert.Single(s.Events<RadioCall>(), r => r.Text.Contains("Can't see the shooter"));
        }
    }
}
