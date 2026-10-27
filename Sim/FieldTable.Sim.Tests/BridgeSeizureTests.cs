using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class BridgeSeizureTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static Mission Make(ulong seed) => BridgeSeizure.Create(Scene.Data, names.Value, seed);

        [Fact]
        public void SetsUpTheSquadTheEnemyAndCivilians()
        {
            for (ulong seed = 1; seed <= 12; seed++)
            {
                var m = Make(seed);
                var units = m.Sim.Units;
                Assert.Equal(14, m.Squad.Count());
                var drone = Assert.Single(units, u => u.Force == m.Marines && u.Def.Flying);
                Assert.True(drone.Gone);
                Assert.Equal("charlie-rf", drone.Operator.Id);
                Assert.InRange(units.Count(u => u.Force == m.Enemy), BridgeSeizure.MinEnemy, BridgeSeizure.MaxEnemy);
                Assert.InRange(units.Count(u => u.Force == m.Civilians), 1, 5);
                foreach (var u in units.Where(u => !u.Gone))
                {
                    var t = m.Sim.Map.TerrainAt(u.Position);
                    Assert.False(t.Deep, "Seed " + seed + ": " + u.Name + " starts in deep water.");
                    Assert.True(t.Passable);
                }
                foreach (var c in units.Where(u => u.Force == m.Civilians))
                    Assert.Equal("building", m.Sim.Map.TerrainAt(c.Position).Id);
                Assert.Equal(5, m.Brief.Count);
                Assert.Contains(m.Objective.Label, m.Brief[0]);
            }
        }

        [Fact]
        public void TheSquadAndTheEnemyStartOnOppositeBanks()
        {
            for (ulong seed = 1; seed <= 12; seed++)
            {
                var m = Make(seed);
                var map = m.Sim.Map;
                Vec2 b = m.Objective.Center;
                // Every Marine needs to cross the bridge (or swim) to reach any enemy: no dry route that avoids the bridge.
                var lead = m.Sim.Units.First(u => u.Id == "lead");
                foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy))
                {
                    var path = m.Sim.Paths.Find(lead.Position, e.Position, false);
                    Assert.NotNull(path);
                    float nearest = path.Min(p => Vec2.Distance(p, b));
                    Assert.True(nearest < 60f || Vec2.Distance(lead.Position, b) < Vec2.Distance(lead.Position, e.Position), "Seed " + seed);
                }
                Assert.True(m.Sim.Units.Where(u => u.Force == m.Marines).All(u => Vec2.Distance(u.Position, b) > 30f), "Seed " + seed + ": a Marine starts on the bridge.");
            }
        }

        [Fact]
        public void EveryoneKnowsTheirNames()
        {
            var m = Make(3);
            Assert.Equal(4, m.Sim.UnitsCalled(m.Marines, "Alpha").Count);
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "Doc"));
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "Lead"));
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "Charlie AR"));
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "drone operator"));
            Assert.Equal(14, m.Sim.UnitsCalled(m.Marines, "squad").Count);
            var name = m.Sim.Units.First(u => u.Id == "bravo-tl").Name;
            Assert.Single(m.Sim.UnitsCalled(m.Marines, name));
        }

        [Fact]
        public void TheSameSeedSetsUpTheSameMission()
        {
            string Setup(ulong seed)
            {
                var m = Make(seed);
                return m.Title + "|" + string.Join(";", m.Sim.Units.Select(u => u.Name + "@" + u.Position)) + "|" + string.Join(" ", m.Brief);
            }
            Assert.Equal(Setup(7), Setup(7));
            Assert.NotEqual(Setup(7), Setup(8));
        }

        [Fact]
        public void AFewTurnsPlayWithoutOrders()
        {
            var m = Make(5);
            for (int t = 0; t < 3; t++)
            {
                m.Sim.RunTurn();
                m.CheckOutcome();
            }
            Assert.Equal(4, m.Sim.Clock.Turn);
            Assert.NotEmpty(m.Sim.Frames);
            Assert.Null(m.Outcome);
        }

        [Fact]
        public void ATurnFinishesOnTheMapThatOnceFroze()
        {
            // Seed 55555 once left a step so small it rounded to nothing, and the turn never ended.
            var m = Make(55555);
            string b = m.Objective.Label;
            m.Sim.GiveOrder(m.Marines, "Alpha", new Order { Verb = Verbs.Move, Where = b, Speed = "run" });
            m.Sim.GiveOrder(m.Marines, "Bravo", new Order { Verb = Verbs.Move, Where = b });
            m.Sim.GiveOrder(m.Marines, "Charlie", new Order { Verb = Verbs.TakeCover });
            var turn = System.Threading.Tasks.Task.Run(() => m.Sim.RunTurn());
            Assert.True(turn.Wait(System.TimeSpan.FromSeconds(30)), "The turn never finished.");
            Assert.Equal(2, m.Sim.Clock.Turn);
        }

        [Fact]
        public void TheSecondaryObjectivesComeFromTheIntent()
        {
            var m = Make(3);
            Assert.Equal(new[] { "civilians", "bridge", "marines" }, m.Secondary.Select(o => o.Id));
            Assert.All(m.Secondary, o => Assert.False(o.IsBroken(m)));
            Assert.StartsWith("Seize and hold " + m.Objective.Label, m.MainObjective);
            Assert.Equal("Positive ID before engaging.", m.Roe);
        }

        [Fact]
        public void HurtingACivilianBreaksThatObjectiveForGood()
        {
            var m = Make(3);
            var civ = m.Sim.Units.First(u => u.Force == m.Civilians);
            m.Sim.KnockDownForTest(civ);
            civ.Condition = Condition.Ok;
            Assert.True(m.Secondary.First(o => o.Id == "civilians").IsBroken(m));
            Assert.False(m.Secondary.First(o => o.Id == "marines").IsBroken(m));
        }

        [Fact]
        public void AMarineKilledBreaksBringingEveryoneHome()
        {
            var m = Make(3);
            var marine = m.Sim.Units.First(u => u.Force == m.Marines);
            m.Sim.KnockDownForTest(marine);
            Assert.False(m.Secondary.First(o => o.Id == "marines").IsBroken(m));
            marine.Condition = Condition.Dead;
            Assert.True(m.Secondary.First(o => o.Id == "marines").IsBroken(m));
            Assert.False(m.Secondary.First(o => o.Id == "bridge").IsBroken(m));
        }

        [Fact]
        public void TakingTheBridgeWithNoEnemyLeftWins()
        {
            var m = Make(6);
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy).ToList()) m.Sim.RemoveUnitForTest(e);
            var lead = m.Sim.Units.First(u => u.Id == "lead");
            lead.Position = m.Objective.Center;
            m.CheckOutcome();
            Assert.Null(m.Outcome);
            m.Sim.RunTurn();
            m.CheckOutcome();
            Assert.Equal("success", m.Outcome);
        }

        [Fact]
        public void LosingSevenMarinesLoses()
        {
            var m = Make(6);
            foreach (var u in m.Sim.Units.Where(u => u.Force == m.Marines).Take(7).ToList()) m.Sim.KnockDownForTest(u);
            m.CheckOutcome();
            Assert.Equal("failure", m.Outcome);
        }
    }
}
