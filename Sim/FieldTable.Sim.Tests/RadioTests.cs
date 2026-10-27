using System.Linq;
using Xunit;

namespace FieldTable.Sim.Tests
{
    public class RadioTests
    {
        [Fact]
        public void ATeamCallsAContactOnceWhileItStaysInTheSameSquare()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 350, "Alpha");
            var e = s.Add("e", "en-rf", s.Red, 450, 350);
            s.Sim.CallContact(m, e, true, false, 0);
            s.Sim.CallContact(m, e, true, true, 0);
            s.Sim.CallContact(m, e, false, true, 0);
            Assert.Equal("Alpha: Taking fire! Enemy rifleman at E4.", Assert.Single(s.Events<RadioCall>()).Text);
        }

        [Fact]
        public void ItCallsAgainWhenTheContactMovesToAnotherSquare()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 350, "Alpha");
            var e = s.Add("e", "en-rf", s.Red, 450, 350);
            s.Sim.CallContact(m, e, false, false, 0);
            e.Position = new Vec2(550, 350);
            s.Sim.CallContact(m, e, true, true, 0);
            Assert.Equal(new[] { "Alpha: Contact, enemy rifleman at E4.", "Alpha: Taking fire! Enemy rifleman at F4." }, s.Events<RadioCall>().Select(c => c.Text));
        }

        [Fact]
        public void ItCallsAgainWhenTheContactComesBackAfterAWhileButNotAfterABlink()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 350, "Alpha");
            var e = s.Add("e", "en-rf", s.Red, 450, 350);
            s.Sim.CallContact(m, e, true, false, 0);
            for (int i = 0; i < Simulation.SecondsGoneToCallAgain * SimClock.StepsPerSecond; i++) s.Sim.Clock.Advance();
            s.Sim.CallContact(m, e, true, true, s.Sim.Clock.Step - 5);
            Assert.Single(s.Events<RadioCall>());
            s.Sim.CallContact(m, e, true, true, 0);
            Assert.Equal(2, s.Events<RadioCall>().Count());
        }

        [Fact]
        public void EachTeamCallsItsOwnContacts()
        {
            var s = new Scene();
            var a = s.Add("a", "rf", s.Blue, 100, 350, "Alpha");
            var b = s.Add("b", "rf", s.Blue, 100, 380, "Bravo");
            var e = s.Add("e", "en-rf", s.Red, 450, 350);
            s.Sim.CallContact(a, e, true, false, 0);
            s.Sim.CallContact(b, e, true, true, 0);
            s.Sim.CallContact(a, e, true, true, 0);
            Assert.Equal(2, s.Events<RadioCall>().Count());
        }

        [Fact]
        public void TheEnemyDoesNotTalkOnTheMarinesNet()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 350, "Alpha");
            var e = s.Add("e", "en-rf", s.Red, 450, 350);
            s.Sim.CallContact(e, m, true, false, 0);
            Assert.Empty(s.Events<RadioCall>());
        }

        [Fact]
        public void ATeamOnOverwatchCallsItsFirePositionOnceEveryoneIsSet()
        {
            var s = new Scene();
            var a = s.Add("a", "tl", s.Blue, 100, 300, "Alpha");
            var b = s.Add("b", "ar", s.Blue, 100, 310, "Alpha");
            s.Sim.GiveOrder(s.Blue, "Alpha", new Order { Verb = Verbs.Overwatch, Target = "E4", Where = "B4" });
            s.Sim.RunTurn();
            s.Sim.RunTurn();
            var call = Assert.Single(s.Events<RadioCall>(), r => r.Kind == "fire");
            Assert.Equal("Alpha: Set. Overwatch on E4.", call.Text);
            Assert.Equal("B4", s.Map.SquareOf(call.At.Value).ToString());
        }

        [Fact]
        public void SuppressingCallsTheFirePositionOnce()
        {
            var s = new Scene();
            var a = s.Add("a", "ar", s.Blue, 100, 300, "Bravo");
            var b = s.Add("b", "rf", s.Blue, 100, 310, "Bravo");
            s.Sim.GiveOrder(s.Blue, "Bravo", new Order { Verb = Verbs.Suppress, Target = "E4" });
            s.Sim.RunTurn();
            var call = Assert.Single(s.Events<RadioCall>(), r => r.Kind == "fire");
            Assert.Equal("Bravo: Suppressing E4.", call.Text);
        }
    }
}
