using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class DelayAdvanceTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static Mission Make(ulong seed, string goal = null) => StartStates.Create(StartStates.DelayAdvance, Scene.Data, names.Value, seed, goal);

        static void Turns(Mission m, int n) { for (int i = 0; i < n && m.Outcome == null; i++) { m.Sim.RunTurn(); m.CheckOutcome(); } }

        static void Quiet(Mission m)
        {
            foreach (var u in m.Sim.Units) { u.Ammo = 0; u.LauncherAmmo = 0; u.LauncherAp = 0; u.Grenades = 0; }
        }

        [Theory]
        [InlineData(DelayAdvance.Push)]
        [InlineData(DelayAdvance.Scouts)]
        [InlineData(DelayAdvance.GoRound)]
        public void ThreeLinesAcrossTheRoadAndAPlatoonOnItsWay(string goal)
        {
            for (ulong seed = 1; seed <= 4; seed++)
            {
                var m = Make(seed, goal);
                var map = m.Sim.Map;
                Assert.Equal(StartStates.DelayAdvance, m.Kind);
                var lines = Enumerable.Range(1, 3).Select(i => map.FindLandmark("Phase Line " + i)).ToList();
                Assert.All(lines, l => Assert.Equal("control", l.Kind));
                Assert.True(m.Sim.TryPlace(m.Squad.First(), "PL 2", out _));
                var enemy = m.Sim.Units.Where(u => u.Force == m.Enemy).ToList();
                Assert.Equal(28, enemy.Count);
                Assert.All(enemy, e => Assert.True(e.Gone));
                Assert.Equal(4, enemy.Count(e => e.Def.Radio));
                var lead = m.Squad.First(u => u.Id == "lead");
                Assert.True(MissionKit.DistanceTo(lead.Position, lines[0].Points) <= 45f, "Seed " + seed + ": Lead starts " + MissionKit.DistanceTo(lead.Position, lines[0].Points) + " m from Phase Line 1.");
                Assert.Equal(DelayAdvance.HoldUntil, m.TurnLimit);
            }
        }

        [Fact]
        public void LeftAloneTheyWalkPastTheLastLine()
        {
            var m = Make(1, DelayAdvance.Push);
            Quiet(m);
            Turns(m, 20);
            Assert.Equal("failure", m.Outcome);
            Assert.Equal("They crossed Phase Line 3.", m.OutcomeText);
        }

        [Fact]
        public void StillShortOfTheLastLineAtTurnTwentyWins()
        {
            var m = Make(2, DelayAdvance.Push);
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy).ToList()) m.Sim.RemoveUnitForTest(e);
            Turns(m, 25);
            Assert.Equal("success", m.Outcome);
            Assert.Equal(DelayAdvance.HoldUntil + 1, m.Sim.Clock.Turn);
            Assert.False(m.Secondary.Single(o => o.Id == "lines").IsBroken(m));
        }

        [Fact]
        public void ScoutsComeFirstAndTheRestFollowOnceTheyFindTheSquad()
        {
            var m = Make(3, DelayAdvance.Scouts);
            Quiet(m);
            Turns(m, 1);
            var on = m.Sim.Units.Where(u => u.Force == m.Enemy && !u.Gone).ToList();
            Assert.Equal(2, on.Count);
            Assert.All(on, u => Assert.Equal("scouts", u.Team));
            Turns(m, 6);
            Assert.True(m.Sim.Units.Count(u => u.Force == m.Enemy && !u.Gone) > 2);
        }
    }
}
