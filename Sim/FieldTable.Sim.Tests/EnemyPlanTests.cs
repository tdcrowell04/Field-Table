using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class EnemyPlanTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static Mission Make(ulong seed, string goal) => BridgeSeizure.Create(Scene.Data, names.Value, seed, goal);

        static Unit Lead(Mission m) => m.Sim.Units.First(u => u.Id == "lead");

        [Theory]
        [InlineData("hold", 6, 9)]
        [InlineData("delay", 3, 5)]
        [InlineData("blow", 5, 7)]
        [InlineData("ambush", 6, 9)]
        public void EachGoalBringsItsOwnMix(string goal, int least, int most)
        {
            for (ulong seed = 1; seed <= 4; seed++)
            {
                var m = Make(seed, goal);
                var enemy = m.Sim.Units.Where(u => u.Force == m.Enemy).ToList();
                Assert.InRange(enemy.Count, least, most);
                Assert.Equal(goal, m.Enemy.Plan.Goal);
                Assert.Equal(goal == "blow", enemy.Any(u => u.Def.Id == "en-eng"));
                Assert.Contains(enemy, u => u.Def.Id == "en-pkm");
                if (goal == "delay") Assert.All(enemy, u => Assert.True(Vec2.Distance(u.Position, m.Objective.Center) >= 140f));
            }
        }

        [Fact]
        public void TheBriefHintsAtTheGoalWithoutNamingIt()
        {
            Assert.Contains("digging in", Make(1, "hold").Brief[2]);
            Assert.Contains("heavy boxes", Make(1, "blow").Brief[2]);
            Assert.Contains("nobody saw them go", Make(1, "ambush").Brief[2]);
            Assert.Contains("from well back", Make(1, "delay").Brief[2]);
        }

        [Fact]
        public void AnAmbushHoldsFireUntilSomeoneIsOnTheBridge()
        {
            var m = Make(2, "ambush");
            var lead = Lead(m);
            // He can't shoot, so he can't spring it by firing first.
            lead.Ammo = 0;
            foreach (var u in m.Squad.Where(u => u != lead).ToList()) m.Sim.RemoveUnitForTest(u);
            // Out in the open on the near bank, 60 m short of the bridge: they hold.
            Vec2 b = m.Objective.Center, near = m.Objective.Points[0];
            Vec2 back = near + (near - b) * (60f / Vec2.Distance(near, b));
            lead.Position = back;
            for (int i = 0; i < 2; i++) m.Sim.RunTurn();
            Assert.DoesNotContain(m.Sim.Log.Events.OfType<ShotFired>(), s => s.Shooter.Force == m.Enemy);
            Assert.False(m.Enemy.Plan.Sprung);
            // On the bridge: they open up.
            lead.Position = b;
            m.Sim.RunTurn();
            Assert.True(m.Enemy.Plan.Sprung);
        }

        [Fact]
        public void TheEngineerSetsHisChargesThenWaitsAtHisFiringPoint()
        {
            var m = Make(3, "blow");
            var plan = m.Enemy.Plan;
            for (int i = 0; i < 10 && !(plan.ChargesSet && Vec2.Distance(plan.Engineer.Position, plan.FiringPoint) <= 3f); i++) m.Sim.RunTurn();
            Assert.True(plan.ChargesSet);
            Assert.False(plan.Blown);
            Assert.True(Vec2.Distance(plan.Engineer.Position, plan.FiringPoint) <= 3f);
        }

        [Fact]
        public void ABlownBridgeDropsIntoTheWaterAndFailsTheObjective()
        {
            var m = Make(3, "blow");
            var plan = m.Enemy.Plan;
            var bridgeObjective = m.Secondary.Single(o => o.Id == "bridge");
            Assert.False(bridgeObjective.IsBroken(m));
            m.Sim.BlowBridge(plan.Engineer, plan);
            Assert.True(m.BridgeBlown);
            Assert.True(bridgeObjective.IsBroken(m));
            Assert.NotEqual("bridge", m.Sim.Map.TerrainAt(m.Objective.Center).Id);
            Assert.Contains(m.Sim.Log.Events.OfType<RadioCall>(), r => r.Text.Contains("They blew the bridge!"));
            Assert.Contains(m.Sim.Log.Events.OfType<BlastAt>(), e => e.Radius >= 10f);
            // The crossing site still counts as the objective: either end of what's left.
            m.Sim.Units.First(u => u.Id == "lead").Position = m.Objective.Points[0];
            Assert.True(m.MarineOnObjective);
        }

        [Fact]
        public void TheEngineerBlowsItWhenTheMarinesGetClose()
        {
            var m = Make(3, "blow");
            var plan = m.Enemy.Plan;
            for (int i = 0; i < 10 && !(plan.ChargesSet && Vec2.Distance(plan.Engineer.Position, plan.FiringPoint) <= 3f); i++) m.Sim.RunTurn();
            Assert.True(plan.ChargesSet);
            // Walk the whole squad up to the near end, where the enemy can't miss them.
            foreach (var u in m.Squad) u.Position = m.Objective.Points[0] + (m.Objective.Points[0] - m.Objective.Center) * 0.5f;
            for (int i = 0; i < 2 && !plan.Blown; i++) m.Sim.RunTurn();
            Assert.True(plan.Blown || !plan.Engineer.CanAct);
        }

        [Fact]
        public void TheDelayFallsBackWhenMarinesGetClose()
        {
            var m = Make(4, "delay");
            var enemy = m.Sim.Units.Where(u => u.Force == m.Enemy).ToList();
            var start = enemy.ToDictionary(u => u, u => Vec2.Distance(u.Position, m.Objective.Center));
            // Put the squad on the bridge and let the enemy see them.
            foreach (var u in m.Squad) u.Position = m.Objective.Center;
            m.Sim.RunTurn();
            Assert.True(enemy.Where(u => u.CanAct).Any(u => Vec2.Distance(u.Position, m.Objective.Center) > start[u] + 40f), "Nobody fell back.");
        }
    }
}
