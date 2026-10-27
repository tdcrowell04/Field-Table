using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class ClearVillageTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static Mission Make(ulong seed, string goal = null) => StartStates.Create(StartStates.Clear, Scene.Data, names.Value, seed, goal);

        static void Turns(Mission m, int n) { for (int i = 0; i < n && m.Outcome == null; i++) { m.Sim.RunTurn(); m.CheckOutcome(); } }

        static bool In(Landmark b, Unit u) => b.Contains(u.Position);

        [Theory]
        [InlineData(ClearVillage.HouseToHouse)]
        [InlineData(ClearVillage.RiggedDoor)]
        [InlineData(ClearVillage.SlipOut)]
        public void HigherMarksFourToSixHousesWithFightersInSomeAndFamiliesInOthers(string goal)
        {
            for (ulong seed = 1; seed <= 5; seed++)
            {
                var m = Make(seed, goal);
                Assert.Equal(StartStates.Clear, m.Kind);
                var village = m.Objective;
                Assert.InRange(m.Marked.Count, 4, 6);
                Assert.All(m.Marked, b => Assert.True(village.Contains(b.Center)));
                var enemy = m.Sim.Units.Where(u => u.Force == m.Enemy).ToList();
                Assert.All(enemy, e => Assert.Contains(m.Marked, b => In(b, e)));
                var held = m.Marked.Where(b => enemy.Any(e => In(b, e))).ToList();
                Assert.Equal(goal == ClearVillage.HouseToHouse ? 3 : goal == ClearVillage.RiggedDoor ? 1 : held.Count, held.Count);
                Assert.Contains(m.Marked, b => !held.Contains(b) && m.Sim.Units.Any(c => c.Force == m.Civilians && In(b, c)));
                Assert.All(m.Squad, u => Assert.True(Vec2.Distance(u.Position, village.Center) >= 150f));
                Assert.Equal(goal == ClearVillage.RiggedDoor, m.Sim.Ieds.Count == 1);
                foreach (var b in m.Marked) Assert.Contains(b.Label.Substring(9), m.Brief[0]);
            }
        }

        [Fact]
        public void ClearingEveryMarkedHouseWins()
        {
            var m = Make(1, ClearVillage.HouseToHouse);
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy).ToList()) m.Sim.RemoveUnitForTest(e);
            var clearers = m.Squad.Where(u => u.CanAct).Take(m.Marked.Count).ToList();
            for (int i = 0; i < m.Marked.Count; i++)
            {
                var b = m.Marked[i];
                clearers[i].Position = new Vec2(b.Min.X - 3f, b.Center.Y);
                if (i == m.Marked.Count - 1) continue;
                Assert.True(m.Sim.Give(clearers[i], new Order { Verb = Verbs.Clear, Where = b.Label }).Ok);
            }
            Turns(m, 3);
            Assert.Null(m.Outcome);
            Assert.Equal("onit", m.MainStatus);
            Assert.True(m.Sim.Give(clearers[m.Marked.Count - 1], new Order { Verb = Verbs.Clear, Where = m.Marked[m.Marked.Count - 1].Label }).Ok);
            Turns(m, 3);
            Assert.Equal("success", m.Outcome);
            Assert.Equal("Every marked building is clear.", m.OutcomeText);
            Assert.False(m.Secondary.Single(o => o.Id == "standing").IsBroken(m));
        }

        [Fact]
        public void TheRiggedDoorBlowsTheHouseOpen()
        {
            var m = Make(3, ClearVillage.RiggedDoor);
            var ied = Assert.Single(m.Sim.Ieds);
            var b = m.Marked.Single(h => Vec2.Distance(h.Center, ied.Position) < 20f && ied.Position.X >= h.Min.X - 1.5f && ied.Position.X <= h.Max.X + 1.5f && ied.Position.Y >= h.Min.Y - 1.5f && ied.Position.Y <= h.Max.Y + 1.5f);
            Assert.DoesNotContain(m.Sim.Units, u => u.Force == m.Enemy && In(b, u));
            m.Squad.First(u => u.Id == "alpha-rf").Position = ied.Position + new Vec2(0.5f, 0.5f);
            Turns(m, 1);
            Assert.True(ied.Blown);
            Assert.True(m.Secondary.Single(o => o.Id == "standing").IsBroken(m));
        }

        [Fact]
        public void HouseToHouseALoneFighterFallsBackToAnotherHouse()
        {
            var m = Make(2, ClearVillage.HouseToHouse);
            foreach (var u in m.Sim.Units) { u.Ammo = 0; u.LauncherAmmo = 0; u.LauncherAp = 0; u.Grenades = 0; }
            var enemy = m.Sim.Units.Where(u => u.Force == m.Enemy).ToList();
            var held = m.Marked.Where(b => enemy.Any(e => In(b, e))).ToList();
            var b0 = held.First(b => enemy.Count(e => In(b, e)) >= 2);
            var inside = enemy.Where(e => In(b0, e)).ToList();
            // Down to one, with a Marine at the door.
            foreach (var e in inside.Skip(1)) m.Sim.RemoveUnitForTest(e);
            var last = inside[0];
            m.Squad.First(u => u.Id == "alpha-tl").Position = new Vec2(b0.Min.X - 4f, b0.Center.Y);
            Turns(m, 2);
            Assert.False(In(b0, last), last.Name + " is still in " + b0.Label + ".");
            Assert.Contains(held, b => b != b0 && In(b, last) || last.Order != null);
        }

        [Fact]
        public void SlipOutTheyAllLeaveWhenMarinesComeClose()
        {
            var m = Make(1, ClearVillage.SlipOut);
            foreach (var u in m.Sim.Units) { u.Ammo = 0; u.LauncherAmmo = 0; u.LauncherAp = 0; u.Grenades = 0; }
            var lead = m.Squad.First(u => u.Id == "lead");
            Vec2 toward = m.Objective.Center - lead.Position;
            lead.Position = m.Objective.Center - toward * (140f / toward.Length);
            Turns(m, 12);
            Assert.All(m.Sim.Units.Where(u => u.Force == m.Enemy), e => Assert.True(e.Gone, e.Name + " is still at " + e.Position));
            Assert.Contains(m.Sim.Log.Events.OfType<UnitLeftMap>(), e => e.Unit.Force == m.Enemy);
        }
    }
}
