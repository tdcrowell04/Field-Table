using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class TakeTheHillTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static Mission Make(ulong seed, string goal = null) => StartStates.Create(StartStates.Hill, Scene.Data, names.Value, seed, goal);

        static void Turns(Mission m, int n) { for (int i = 0; i < n && m.Outcome == null; i++) { m.Sim.RunTurn(); m.CheckOutcome(); } }

        static void Quiet(Mission m)
        {
            foreach (var u in m.Sim.Units) { u.Ammo = 0; u.LauncherAmmo = 0; u.LauncherAp = 0; u.Grenades = 0; }
        }

        [Theory]
        [InlineData(TakeTheHill.HoldOn)]
        [InlineData(TakeTheHill.FallBack)]
        [InlineData(TakeTheHill.Counter)]
        public void ASquadDugInOnAHillAboveTheRoad(string goal)
        {
            for (ulong seed = 1; seed <= 5; seed++)
            {
                var m = Make(seed, goal);
                var map = m.Sim.Map;
                Assert.Equal(StartStates.Hill, m.Kind);
                Assert.Equal("hill", m.Objective.Kind);
                Vec2 top = m.Objective.Center;
                Assert.Contains(map.Landmarks, l => l.Kind == "line" && l.Label.StartsWith("Route ") && MissionKit.DistanceTo(top, l.Points) <= 350f);
                var enemy = m.Sim.Units.Where(u => u.Force == m.Enemy).ToList();
                Assert.InRange(enemy.Count, 6, 9);
                int onTop = enemy.Count(e => Vec2.Distance(e.Position, top) <= 50f);
                Assert.True(goal == TakeTheHill.Counter ? onTop == 3 : onTop == enemy.Count, "Seed " + seed + ": " + onTop + " of " + enemy.Count + " on top.");
                Assert.Single(enemy, e => e.Cargo == "their radio" && e.Def.Radio);
                Assert.All(m.Squad, u => Assert.True(Vec2.Distance(u.Position, top) >= 200f));
            }
        }

        [Fact]
        public void TheFallBackGoesOverTheTopWhenMarinesComeClose()
        {
            var m = Make(1, TakeTheHill.FallBack);
            Quiet(m);
            Vec2 top = m.Objective.Center;
            var lead = m.Squad.First(u => u.Id == "lead");
            lead.Position = top + (lead.Position - top) * (140f / Vec2.Distance(lead.Position, top));
            Turns(m, 3);
            Assert.True(m.Enemy.Plan.FallingBack);
            Assert.True(m.Sim.Units.Where(u => u.Force == m.Enemy && u.CanAct).All(e => Vec2.Distance(e.Position, top) >= 70f));
        }

        [Fact]
        public void TheCounterattackComesOnceMarinesAreOnTop()
        {
            var m = Make(2, TakeTheHill.Counter);
            Quiet(m);
            Vec2 top = m.Objective.Center;
            var woods = m.Sim.Units.Where(u => u.Force == m.Enemy && Vec2.Distance(u.Position, top) > 100f).ToList();
            Assert.InRange(woods.Count, 5, 6);
            float before = woods.Min(u => Vec2.Distance(u.Position, top));
            Turns(m, 1);
            Assert.False(m.Enemy.Plan.Sprung);
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy && !woods.Contains(u)).ToList()) m.Sim.RemoveUnitForTest(e);
            m.Squad.First(u => u.Id == "alpha-tl").Position = top;
            Turns(m, 2);
            Assert.True(m.Enemy.Plan.Sprung);
            Assert.True(woods.Min(u => Vec2.Distance(u.Position, top)) < before - 40f);
        }

        [Fact]
        public void HoldTheTopTwoTurnsToWinAndSearchTheLeaderForTheRadio()
        {
            var m = Make(3, TakeTheHill.HoldOn);
            var leader = m.Sim.Units.Single(u => u.Force == m.Enemy && u.Def.Radio);
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy).ToList())
                if (e == leader) e.Condition = Condition.Dead; else m.Sim.RemoveUnitForTest(e);
            var marine = m.Squad.First(u => u.Id == "alpha-tl");
            marine.Position = leader.Position + new Vec2(1f, 0f);
            Assert.True(m.Sim.Give(marine, new Order { Verb = Verbs.Search, Where = m.Sim.Coordinates(leader.Position) }).Ok);
            m.Squad.First(u => u.Id == "alpha-ar").Position = m.Objective.Center;
            Turns(m, 3);
            Assert.Equal("success", m.Outcome);
            Assert.EndsWith(" is taken and held.", m.OutcomeText);
            Assert.Null(leader.Cargo);
            Assert.False(m.Secondary.Single(o => o.Id == "radio").IsBroken(m));
        }
    }
}
