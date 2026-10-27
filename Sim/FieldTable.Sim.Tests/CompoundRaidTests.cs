using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class CompoundRaidTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static Mission Make(ulong seed, string goal = null) => StartStates.Create(StartStates.Raid, Scene.Data, names.Value, seed, goal);

        static Unit Hvt(Mission m) => m.Sim.Units.First(u => u.Id == "hvt");

        static void Turns(Mission m, int n) { for (int i = 0; i < n && m.Outcome == null; i++) { m.Sim.RunTurn(); m.CheckOutcome(); } }

        [Theory]
        [InlineData(CompoundRaid.Guard)]
        [InlineData(CompoundRaid.Slip)]
        [InlineData(CompoundRaid.Gone)]
        public void AWalledFarmHisFamilyAndAPickupPoint(string goal)
        {
            for (ulong seed = 1; seed <= 6; seed++)
            {
                var m = Make(seed, goal);
                var map = m.Sim.Map;
                Assert.Equal(StartStates.Raid, m.Kind);
                Assert.Equal(goal, m.Enemy.Plan.Goal);
                Assert.Equal(14, m.Squad.Count());
                var farm = m.Objective;
                Assert.EndsWith(" farm", farm.Label);
                Assert.True(m.Sim.TryPlace(m.Squad.First(), "the compound", out _));
                // Walls all round, with the gate and the back gap to get in and out by.
                int walls = 0;
                for (float y = farm.Min.Y - 1f; y <= farm.Max.Y + 1f; y += 1f)
                    for (float x = farm.Min.X - 1f; x <= farm.Max.X + 1f; x += 1f)
                        if (map.TerrainAt(new Vec2(x, y)).Id == "wall") walls++;
                float around = 2f * (farm.Max.X - farm.Min.X + farm.Max.Y - farm.Min.Y);
                Assert.InRange(walls, around * 0.85f, around * 1.6f);
                var hvt = Hvt(m);
                if (goal == CompoundRaid.Gone) Assert.False(farm.Contains(hvt.Position));
                else Assert.Equal("building", map.TerrainAt(hvt.Position).Id);
                var family = m.Sim.Units.Where(u => u.Force == m.Civilians && farm.Contains(u.Position)).ToList();
                Assert.InRange(family.Count, 2, 4);
                Assert.All(family, f => Assert.Equal("building", map.TerrainAt(f.Position).Id));
                Assert.Equal(goal == CompoundRaid.Gone, family.All(f => f.Knows != null));
                var pickup = map.FindLandmark("the pickup point");
                Assert.Equal("point", pickup.Kind);
                Assert.NotNull(m.Sim.Paths.Find(hvt.Position, pickup.Center, false));
                foreach (var u in m.Sim.Units.Where(u => !u.Gone)) Assert.False(map.TerrainAt(u.Position).Deep);
                Assert.True(m.Squad.All(u => Vec2.Distance(u.Position, farm.Center) >= 100f), "Seed " + seed + ": a Marine starts too near the farm.");
                Assert.Contains(hvt.Name, m.Brief[0]);
            }
        }

        [Fact]
        public void WhenHesNotThereHisFamilySaysWhereHeWent()
        {
            var m = Make(3, CompoundRaid.Gone);
            var hvt = Hvt(m);
            var talker = m.Squad.First(u => u.Id == "alpha-rf");
            var terp = m.Sim.Units.First(u => u.Def.Interpreter);
            var wife = m.Sim.Units.First(u => u.Force == m.Civilians && m.Objective.Contains(u.Position));
            talker.Position = wife.Position + new Vec2(2f, 0f);
            terp.Position = wife.Position + new Vec2(-2f, 0f);
            // The interpreter keeps with Lead, so Lead comes too.
            m.Squad.First(u => u.Id == "lead").Position = wife.Position + new Vec2(0f, 3f);
            // She's indoors, so the talker has to have gone in and seen her.
            m.Sim.Fog.Spot(m.Sim, m.Marines, wife, talker);
            Assert.True(m.Sim.Give(talker, new Order { Verb = Verbs.Talk, Target = wife.Name }).Ok);
            m.Sim.RunTurn();
            var calls = m.Sim.Log.Events.OfType<RadioCall>().Select(r => r.Text).ToList();
            Assert.True(calls.Any(t => t.Contains(hvt.Name + " left the farm two days ago") && t.Contains(m.Sim.Map.SquareOf(hvt.Start).ToString())), string.Join(" | ", calls));
            // He stays in his hideout, whatever happens at the farm.
            Assert.Equal(hvt.Start, hvt.Position);
        }

        [Fact]
        public void DetainHimAndWalkHimToThePickupPointToWin()
        {
            var m = Make(1, CompoundRaid.Guard);
            var hvt = Hvt(m);
            // Everyone else at the farm is out of it, and he gives up.
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy && u != hvt).ToList()) m.Sim.RemoveUnitForTest(e);
            hvt.Surrendered = true;
            var marine = m.Squad.First(u => u.Id == "alpha-tl");
            marine.Position = hvt.Position + new Vec2(3f, 0f);
            m.Sim.Fog.Spot(m.Sim, m.Marines, hvt, marine);
            Assert.True(m.Sim.Give(marine, new Order { Verb = Verbs.Detain, Target = hvt.Name }).Ok);
            Turns(m, 1);
            Assert.True(hvt.Detained);
            Assert.Null(m.Outcome);
            Assert.Equal("onit", m.MainStatus);
            var walk = m.Sim.Give(marine, new Order { Verb = Verbs.Carry, Target = hvt.Name, Where = "the pickup point" });
            Assert.True(walk.Ok, walk.Problem);
            Turns(m, 12);
            Assert.Equal("success", m.Outcome);
            Assert.Equal(hvt.Name + " is detained and at the pickup point.", m.OutcomeText);
            Assert.False(m.Secondary.Single(o => o.Id == "alive").IsBroken(m));
        }

        [Fact]
        public void APrisonerWhoCanWalkIsLedAtAWalkNotCarried()
        {
            var s = new Scene();
            var m = s.Add("reyes", "rf", s.Blue, 100, 300);
            var p = s.Add("p", "en-rf", s.Red, 102, 300);
            p.Detained = true;
            for (int i = 0; i < 3; i++) s.Sim.LookAround();
            Assert.True(s.Order(m, Verbs.Carry, target: "p", where: "300 300").Ok);
            for (int i = 0; i < 100; i++) s.Sim.StepForTest();
            // Ten seconds at a walk: about 14 m, where a casualty carried alone would only go a quarter of that.
            float led = Vec2.Distance(p.Position, new Vec2(102, 300));
            Assert.InRange(led, 10f, 16f);
            Assert.Same(m, p.CarriedBy);
        }

        [Fact]
        public void SlipGetsHimOutTheBackWhenTheSquadComesClose()
        {
            var m = Make(2, CompoundRaid.Slip);
            var hvt = Hvt(m);
            var lead = m.Squad.First(u => u.Id == "lead");
            Turns(m, 1);
            Assert.Null(hvt.Order);
            // Nobody shoots, so it's only about him going.
            foreach (var u in m.Sim.Units) { u.Ammo = 0; u.LauncherAmmo = 0; u.LauncherAp = 0; u.Grenades = 0; }
            // A Marine creeps up within 150 m: he's off out the back, and keeps going until he's off the map.
            Vec2 toward = m.Objective.Center - lead.Position;
            lead.Position = m.Objective.Center - toward * (140f / toward.Length);
            Turns(m, 1);
            Assert.NotNull(hvt.Order);
            Assert.True(m.Enemy.Plan.FallingBack);
            foreach (var u in m.Squad) u.Position = u.Start;
            Turns(m, 10);
            Assert.True(hvt.Gone);
            Assert.Equal("failure", m.Outcome);
            Assert.Equal(hvt.Name + " got away.", m.OutcomeText);
        }

        [Fact]
        public void KilledHeStillCountsOnceAMarineIdentifiesHim()
        {
            var m = Make(1, CompoundRaid.Guard);
            var hvt = Hvt(m);
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy && u != hvt).ToList()) m.Sim.RemoveUnitForTest(e);
            hvt.Condition = Condition.Dead;
            Turns(m, 1);
            Assert.Null(m.Outcome);
            m.Squad.First(u => u.Id == "alpha-tl").Position = hvt.Position + new Vec2(2f, 0f);
            Turns(m, 1);
            Assert.Equal("success", m.Outcome);
            Assert.True(m.Secondary.Single(o => o.Id == "alive").IsBroken(m));
            Assert.Contains(m.Sim.Log.Events.OfType<RadioCall>(), r => r.Text == "Alpha: It's " + hvt.Name + ". He's dead.");
        }
    }
}
