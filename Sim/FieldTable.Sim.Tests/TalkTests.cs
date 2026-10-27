using System.Linq;
using Xunit;

namespace FieldTable.Sim.Tests
{
    public class TalkTests
    {
        static Scene Local()
        {
            var s = new Scene();
            s.Red.Language = "local";
            s.Civ.Language = "local";
            return s;
        }

        [Fact]
        public void AVillagerWhoDoesntSpeakEnglishNeedsTheInterpreter()
        {
            var s = Local();
            var m = s.Add("reyes", "rf", s.Blue, 100, 300);
            var c = s.Add("farmer", "civ", s.Civ, 104, 300);
            for (int i = 0; i < 5; i++) s.Sim.LookAround();
            Assert.True(s.Order(m, Verbs.Talk, target: "farmer").Ok);
            s.Sim.RunTurn();
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "reyes: farmer doesn't speak English. I need the interpreter here.");
        }

        [Fact]
        public void WithTheInterpreterThereTheyTalkThroughHim()
        {
            var s = Local();
            var m = s.Add("reyes", "rf", s.Blue, 100, 300);
            s.Add("terp", "terp", s.Blue, 102, 302);
            s.Add("farmer", "civ", s.Civ, 104, 300);
            var e = s.Add("e", "en-rf", s.Red, 200, 300);
            for (int i = 0; i < 5; i++) s.Sim.LookAround();
            Assert.True(s.Order(m, Verbs.Talk, target: "farmer").Ok);
            s.Sim.RunTurn();
            var said = Assert.Single(s.Events<RadioCall>(), r => r.Text.StartsWith("reyes: farmer, through the interpreter,"));
            Assert.Equal("farmer", said.Speaker);
        }

        [Fact]
        public void APrisonerRefusesLiesOrTellsTheTruthButTheSquadOnlyGetsHisWord()
        {
            bool refused = false, lied = false, truth = false;
            for (ulong seed = 1; seed <= 40 && !(refused && lied && truth); seed++)
            {
                var s = new Scene(seed);
                var m = s.Add("reyes", "rf", s.Blue, 100, 300);
                var p = s.Add("prisoner", "en-rf", s.Red, 103, 300);
                // His two friends are behind a building, out of the Marine's sight, and with two of three still up they stay put.
                var friend = s.Add("friend", "en-pkm", s.Red, 450, 120);
                s.Add("friend2", "en-rf", s.Red, 455, 126);
                s.Map.FillRect(425, 95, 10, 50, "building");
                s.Sim.KnockDownForTest(p);
                for (int i = 0; i < 5; i++) s.Sim.LookAround();
                s.Red.Plan = new EnemyPlan(EnemyPlan.Blow, new Landmark("bridge", "line", null, null, new Vec2(500, 300), new Vec2(490, 300), new Vec2(510, 300), new[] { new Vec2(490, 300), new Vec2(510, 300) }));
                Assert.True(s.Order(m, Verbs.Talk, target: "prisoner").Ok);
                s.Sim.RunTurn();
                var said = Assert.Single(s.Events<RadioCall>(), r => r.Text.StartsWith("reyes: prisoner"));
                if (said.Text.Contains("won't say anything")) refused = true;
                else if (said.Text.Contains("2 soldiers at E2") && said.Text.Contains("rigged to blow")) truth = true;
                else lied = true;
                Assert.Equal("prisoner", said.Speaker);
                // His word isn't a sighting.
                var k = s.Sim.Fog.For(s.Blue).About("friend");
                Assert.True(k == null || !k.Known, "friend known: " + k?.Known + " visible " + k?.Visible + " at " + friend.Position + " seen step " + k?.LastSeenStep + " text " + said.Text);
            }
            Assert.True(refused && lied && truth);
        }

        [Fact]
        public void ALieComesOutWhenTheSquadGoesToCheckAndAtTheEnd()
        {
            for (ulong seed = 1; seed <= 60; seed++)
            {
                var s = new Scene(seed);
                var m = s.Add("reyes", "rf", s.Blue, 100, 300, "Alpha");
                var p = s.Add("prisoner", "en-rf", s.Red, 103, 300);
                s.Add("friend", "en-pkm", s.Red, 450, 120);
                s.Add("friend2", "en-rf", s.Red, 455, 126);
                s.Map.FillRect(425, 95, 10, 50, "building");
                s.Red.Plan = new EnemyPlan(EnemyPlan.Hold, new Landmark("bridge", "line", null, null, new Vec2(500, 300), new Vec2(490, 300), new Vec2(510, 300), new[] { new Vec2(490, 300), new Vec2(510, 300) }));
                s.Sim.KnockDownForTest(p);
                for (int i = 0; i < 5; i++) s.Sim.LookAround();
                Assert.True(s.Order(m, Verbs.Talk, target: "prisoner").Ok);
                s.Sim.RunTurn();
                var lie = s.Sim.Lies.SingleOrDefault();
                if (lie == null || lie.Square == null) continue;
                Assert.False(lie.Exposed);
                Assert.Matches(@"^prisoner lied when questioned: nobody was at [A-H]\d, and they were never out to (only slow you down|blow the bridge|ambush you on the bridge)\.$", lie.Summary);
                // Falling for it: a Marine goes to the square he named, and nobody's there.
                m.Position = s.Map.SquareCenter(lie.Square.Value);
                s.Sim.RunTurn();
                Assert.True(lie.Exposed);
                Assert.Contains(s.Events<RadioCall>(), r => r.Text == "Alpha: Nothing at " + lie.Square + ". prisoner lied to us.");
                return;
            }
            Assert.Fail("No prisoner lied about a square in 60 tries.");
        }

        [Fact]
        public void AFighterAloneAndPinnedMayGiveUp()
        {
            for (ulong seed = 1; seed <= 20; seed++)
            {
                var s = new Scene(seed);
                s.Add("m", "rf", s.Blue, 100, 300);
                var e = s.Add("e", "en-rf", s.Red, 250, 300);
                for (int t = 0; t < 6 && !e.Surrendered; t++)
                {
                    for (int k = 0; k < 600; k++) { e.SetSuppression(1f); s.Sim.StepForTest(); }
                }
                if (!e.Surrendered) continue;
                Assert.False(e.CanAct);
                Assert.Equal(0, e.Ammo);
                Assert.Equal("surrender", e.Emote);
                Assert.Contains(s.Events<UnitSurrendered>(), x => x.Unit == e);
                return;
            }
            Assert.Fail("Nobody gave up in 20 tries.");
        }

        [Fact]
        public void ShoutsCarryButOnlyTheInterpreterCanSayWhatTheyWere()
        {
            var s = Local();
            var m = s.Add("m", "rf", s.Blue, 100, 300, "Alpha");
            var e = s.Add("e", "en-rf", s.Red, 200, 300);
            var f = s.Add("f", "en-rf", s.Red, 205, 305);
            s.Sim.KnockDownForTest(e);
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "Alpha: Shouting from C4. Can't make it out.");

            var s2 = Local();
            s2.Add("m", "rf", s2.Blue, 100, 300, "Alpha");
            s2.Add("terp", "terp", s2.Blue, 100, 306);
            var e2 = s2.Add("e", "en-rf", s2.Red, 200, 300);
            s2.Add("f", "en-rf", s2.Red, 205, 305);
            s2.Sim.KnockDownForTest(e2);
            Assert.Contains(s2.Events<RadioCall>(), r => r.Text == "Interpreter: They're shouting \"Man down! Help him!\" at C4.");
        }

        [Fact]
        public void TheInterpreterSticksWithTheSquadLeader()
        {
            var s = new Scene();
            var lead = s.Add("lead", "sl", s.Blue, 100, 300);
            lead.Callsigns.Add("Lead");
            var terp = s.Add("terp", "terp", s.Blue, 100, 340);
            Assert.True(s.Order(lead, Verbs.Move, "D4").Ok);
            for (int i = 0; i < 4; i++) s.Sim.RunTurn();
            Assert.True(Vec2.Distance(terp.Position, lead.Position) <= 12f, "terp " + terp.Position + " lead " + lead.Position + " path " + (terp.Path == null ? "none" : terp.Path.Count.ToString()));
        }
    }
}
