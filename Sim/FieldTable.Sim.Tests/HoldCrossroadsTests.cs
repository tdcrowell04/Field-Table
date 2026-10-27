using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class HoldCrossroadsTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static Mission Make(ulong seed, string goal = null) => StartStates.Create(StartStates.Crossroads, Scene.Data, names.Value, seed, goal);

        static void Turns(Mission m, int n) { for (int i = 0; i < n && m.Outcome == null; i++) { m.Sim.RunTurn(); m.CheckOutcome(); } }

        [Theory]
        [InlineData(HoldCrossroads.Probe)]
        [InlineData(HoldCrossroads.TwoSides)]
        [InlineData(HoldCrossroads.PinAndFlank)]
        public void TheSquadStartsAtTheCrossroadsAndTheEnemyIsStillOnItsWay(string goal)
        {
            for (ulong seed = 1; seed <= 5; seed++)
            {
                var m = Make(seed, goal);
                Assert.Equal(StartStates.Crossroads, m.Kind);
                Assert.Equal("the crossroads", m.Objective.Label);
                var village = m.Sim.Map.Landmarks.Single(l => l.Kind == "area" && l.Note == "village" && l.Contains(m.Objective.Center));
                Assert.True(m.Sim.Map.Landmarks.Count(l => l.Kind == "building" && village.Contains(l.Center)) >= 4);
                var lead = m.Squad.First(u => u.Id == "lead");
                Assert.True(Vec2.Distance(lead.Position, m.Objective.Center) <= 40f, "Seed " + seed + ": Lead starts " + Vec2.Distance(lead.Position, m.Objective.Center) + " m out.");
                Assert.Equal("onit", m.MainStatus);
                var enemy = m.Sim.Units.Where(u => u.Force == m.Enemy).ToList();
                Assert.InRange(enemy.Count, 14, 17);
                Assert.All(enemy, e => Assert.True(e.Gone));
                Assert.Contains("between turn 15 and turn 25", m.Brief[4]);
            }
        }

        [Theory]
        [InlineData(HoldCrossroads.Probe)]
        [InlineData(HoldCrossroads.TwoSides)]
        [InlineData(HoldCrossroads.PinAndFlank)]
        public void TheyArriveAndCloseIn(string goal)
        {
            var m = Make(2, goal);
            var c = m.Objective.Center;
            // Nobody shoots, so it's only about how they move.
            foreach (var u in m.Sim.Units) { u.Ammo = 0; u.LauncherAmmo = 0; u.LauncherAp = 0; u.Grenades = 0; }
            Turns(m, 5);
            var enemy = m.Sim.Units.Where(u => u.Force == m.Enemy).ToList();
            Assert.Contains(enemy, e => !e.Gone);
            Turns(m, 10);
            float nearest = enemy.Where(e => !e.Gone).Min(e => Vec2.Distance(e.Position, c));
            Assert.True(nearest <= 120f, goal + ": nearest attacker is " + nearest + " m out by turn " + m.Sim.Clock.Turn + ".");
        }

        [Fact]
        public void AProbeShootsThenPullsBackBeforeThePush()
        {
            var m = Make(3, HoldCrossroads.Probe);
            var c = m.Objective.Center;
            foreach (var u in m.Sim.Units) { u.Ammo = 0; u.LauncherAmmo = 0; u.LauncherAp = 0; u.Grenades = 0; }
            var probe = m.Sim.Units.Where(u => u.Force == m.Enemy).Take(3).ToList();
            float closest = float.MaxValue, after = 0f;
            int pulledBack = -1;
            for (int t = 0; t < 14 && m.Outcome == null && (pulledBack < 0 || m.Sim.Clock.Turn - pulledBack < 2); t++)
            {
                Turns(m, 1);
                if (probe[0].Gone) continue;
                float d = probe.Min(u => Vec2.Distance(u.Position, c));
                if (pulledBack < 0) closest = System.Math.Min(closest, d);
                else after = System.Math.Max(after, d);
                if (pulledBack < 0 && probe.Any(u => u.Order?.Verb == Verbs.Withdraw)) pulledBack = m.Sim.Clock.Turn;
            }
            Assert.True(pulledBack > 0);
            Assert.InRange(closest, 120f, 320f);
            Assert.True(after > closest + 100f, "The probe went no farther back than " + after + " m, from " + closest + " m.");
        }

        [Fact]
        public void StillHoldingWhenReliefComesWins()
        {
            var m = Make(1);
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy).ToList()) m.Sim.RemoveUnitForTest(e);
            Turns(m, 30);
            Assert.Equal("success", m.Outcome);
            Assert.StartsWith("Relief is here, on turn ", m.OutcomeText);
            Assert.InRange(m.Sim.Clock.Turn, HoldCrossroads.ReliefFirst + 1, HoldCrossroads.ReliefLast + 1);
            Assert.False(m.Secondary.Single(o => o.Id == "never-lost").IsBroken(m));
        }

        [Fact]
        public void TheEnemyHoldingItTwoTurnsInARowLoses()
        {
            var m = Make(1);
            var c = m.Objective.Center;
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy).Skip(1).ToList()) m.Sim.RemoveUnitForTest(e);
            var holder = m.Sim.Units.Single(u => u.Force == m.Enemy);
            m.Enemy.Plan = null;
            holder.Gone = false;
            holder.Ammo = 0;
            holder.Position = c;
            foreach (var u in m.Sim.Units.Where(u => u.Force == m.Marines)) u.Position = c + new Vec2(0f, 80f + u.Position.X % 7f);
            foreach (var u in m.Sim.Units) { u.Ammo = 0; u.LauncherAmmo = 0; u.LauncherAp = 0; u.Grenades = 0; }
            Turns(m, 1);
            Assert.Null(m.Outcome);
            Assert.True(m.Secondary.Single(o => o.Id == "never-lost").IsBroken(m));
            Turns(m, 1);
            Assert.Equal("failure", m.Outcome);
            Assert.Equal("The enemy took the crossroads.", m.OutcomeText);
        }
    }
}
