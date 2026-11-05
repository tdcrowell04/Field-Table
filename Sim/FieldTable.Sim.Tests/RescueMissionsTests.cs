using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class RescueMissionsTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static Mission Make(string kind, ulong seed, string goal = null) => StartStates.Create(kind, Scene.Data, names.Value, seed, goal, Sides.Pararescue);

        static void Turns(Mission m, int n) { for (int i = 0; i < n && m.Outcome == null; i++) { m.Sim.RunTurn(); m.CheckOutcome(); } }

        static void Quiet(Mission m)
        {
            foreach (var u in m.Sim.Units) { u.Ammo = 0; u.LauncherAmmo = 0; u.LauncherAp = 0; u.Grenades = 0; }
        }

        static void NoEnemy(Mission m)
        {
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy).ToList()) m.Sim.RemoveUnitForTest(e);
        }

        // ---------- the wounded patrol ----------

        [Theory]
        [InlineData(WoundedPatrol.Pin)]
        [InlineData(WoundedPatrol.Round)]
        [InlineData(WoundedPatrol.FallBack)]
        public void APinnedPatrolWithThreeHurtAndALandingZone(string goal)
        {
            for (ulong seed = 1; seed <= 3; seed++)
            {
                var m = Make(StartStates.Patrol, seed, goal);
                var patrol = m.Sim.Units.Where(u => u.Force.Id == "patrol").ToList();
                Assert.Equal(6, patrol.Count);
                Assert.Single(patrol, u => u.Condition == Condition.Dead);
                Assert.Single(patrol, u => u.Condition == Condition.Down);
                Assert.Equal(4, patrol.Count(u => u.Passenger));
                Assert.False(m.Sim.AreHostile(m.Marines, patrol[0].Force));
                var lz = m.Sim.Map.FindLandmark("the LZ");
                Assert.InRange(Vec2.Distance(lz.Center, patrol[0].Position), 200f, 360f);
                Assert.All(m.Squad, u => Assert.True(Vec2.Distance(u.Position, lz.Center) <= 120f, u.Name + " starts " + Vec2.Distance(u.Position, lz.Center) + " m from the LZ."));
                Assert.InRange(m.Sim.Units.Count(u => u.Force == m.Enemy), 6, 8);
                Assert.Equal(5, m.Squad.Count());
            }
        }

        [Fact]
        public void CarryTheHurtToTheLzAndTheHelicoptersTakeThem()
        {
            var m = Make(StartStates.Patrol, 2, WoundedPatrol.Pin);
            NoEnemy(m);
            var lz = m.Sim.Map.FindLandmark("the LZ").Center;
            var hurt = m.Sim.Units.Where(u => u.Force.Id == "patrol" && u.Passenger && u.Condition != Condition.Dead).ToList();
            foreach (var h in hurt) h.Position = lz + new Vec2(3f, 3f);
            var lead = m.Squad.Single(u => u.Id == "lead");
            Assert.True(m.Sim.Give(lead, new Order { Verb = Verbs.Request, Kind = "CASEVAC", Where = "the LZ" }).Ok);
            Turns(m, 14);
            Assert.Equal("success", m.Outcome);
            // The patrol's dead weren't brought: that extra is missed.
            Assert.True(m.Secondary.Single(o => o.Id == "fallen").IsBroken(m));
        }

        [Fact]
        public void TheDeadOfThePatrolCanBeCarriedOut()
        {
            var m = Make(StartStates.Patrol, 1, WoundedPatrol.Pin);
            var dead = m.Sim.Units.Single(u => u.Force.Id == "patrol" && u.Condition == Condition.Dead);
            var pj = m.Squad.First(u => u.Id == "alpha-1");
            pj.Position = dead.Position + new Vec2(1f, 0f);
            var check = m.Sim.Give(pj, new Order { Verb = Verbs.Carry, Target = dead.Name, Where = "the LZ" });
            Assert.True(check.Ok, check.Problem);
        }

        [Fact]
        public void IfTheBadlyHurtOneBleedsOutItsLost()
        {
            var m = Make(StartStates.Patrol, 3, WoundedPatrol.Pin);
            Quiet(m);
            var bad = m.Sim.Units.Single(u => u.Force.Id == "patrol" && u.Condition == Condition.Down);
            bad.BleedMinutes = 1;
            Turns(m, 3);
            Assert.Equal("failure", m.Outcome);
            Assert.EndsWith("died before we got him out.", m.OutcomeText);
        }

        // ---------- the crash in the river ----------

        [Theory]
        [InlineData(RiverCrash.Straight)]
        [InlineData(RiverCrash.Banks)]
        [InlineData(RiverCrash.Watch)]
        public void FourCrewTwoInTheWaterAndAWreckOnASandbar(string goal)
        {
            for (ulong seed = 1; seed <= 3; seed++)
            {
                var m = Make(StartStates.River, seed, goal);
                var crew = m.Sim.Units.Where(u => u.Force.Id == "aircrew").ToList();
                Assert.Equal(4, crew.Count);
                Assert.Equal(2, crew.Count(u => m.Sim.Map.TerrainAt(u.Position).Deep));
                Assert.Equal(2, crew.Count(u => u.Wounds > 0));
                Assert.All(crew, u => Assert.True(u.Passenger));
                Assert.Equal("the sandbar", m.Objective.Label);
                Assert.All(m.Sim.Map.Landmarks.Where(l => l.Label.EndsWith(" bridge")), b => Assert.True(Vec2.Distance(b.Center, m.Objective.Center) >= 150f));
                Assert.Single(m.Sim.GearOnTheGround);
                Assert.All(m.Sim.Units.Where(u => u.Force == m.Enemy), e => Assert.True(e.Gone));
            }
        }

        [Fact]
        public void TheCurrentCarriesThemOffAndTooLongInTheWaterTheyGoUnder()
        {
            var m = Make(StartStates.River, 1, RiverCrash.Straight);
            Quiet(m);
            NoEnemy(m);
            var gunner = m.Sim.Units.Single(u => u.Id == "gunner");
            var was = gunner.Position;
            Turns(m, 1);
            Assert.True(Vec2.Distance(gunner.Position, was) >= 10f, "The current should have carried him: " + Vec2.Distance(gunner.Position, was) + " m.");
            Turns(m, RiverCrash.HurtInWater + 2);
            Assert.Equal("failure", m.Outcome);
            Assert.EndsWith("drowned.", m.OutcomeText);
        }

        [Fact]
        public void ReachingSomeoneInTheWaterStopsTheCurrentAndTheyMakeForTheBank()
        {
            var m = Make(StartStates.River, 2, RiverCrash.Straight);
            Quiet(m);
            NoEnemy(m);
            var chief = m.Sim.Units.Single(u => u.Id == "crew-chief");
            var pj = m.Squad.First(u => u.Id == "alpha-1");
            pj.Load = Simulation.StrippedLoad;
            pj.Position = chief.Position + new Vec2(1f, 0f);
            Turns(m, 1);
            Assert.Contains(m.Sim.Log.Events.OfType<RadioCall>(), r => r.Text.Contains("You've got me!"));
            Turns(m, 3);
            Assert.False(m.Sim.Map.TerrainAt(chief.Position).Deep, "The crew chief should be out of the water.");
            Assert.NotEqual(Condition.Dead, chief.Condition);
        }

        // ---------- the collapsed building ----------

        [Theory]
        [InlineData(CollapsedBuilding.Passing)]
        [InlineData(CollapsedBuilding.Searching)]
        [InlineData(CollapsedBuilding.Waiting)]
        public void AFamilyAndOneOfOursUnderTheRubble(string goal)
        {
            for (ulong seed = 1; seed <= 3; seed++)
            {
                var m = Make(StartStates.Rubble, seed, goal);
                var trapped = m.Sim.Units.Where(u => u.Trapped).ToList();
                Assert.Equal(4, trapped.Count);
                Assert.All(trapped, u => Assert.True(u.Gone));
                Assert.Equal(3, trapped.Count(u => u.Force == m.Civilians));
                Assert.All(trapped, u => Assert.True(m.Objective.Contains(u.Position)));
                Assert.Equal("rubble", m.Sim.Map.TerrainAt(m.Objective.Center).Id);
                Assert.InRange(m.Sim.Units.Count(u => u.Force == m.Enemy), 4, 6);
            }
        }

        [Fact]
        public void SearchTheRubbleToHearThemThenDigThemOut()
        {
            var m = Make(StartStates.Rubble, 1, CollapsedBuilding.Passing);
            Quiet(m);
            NoEnemy(m);
            var t = m.Sim.Units.First(u => u.Trapped);
            var pj = m.Squad.First(u => u.Id == "alpha-1");
            pj.Position = t.Position + new Vec2(1.5f, 0f);
            Assert.True(m.Sim.Give(pj, new Order { Verb = Verbs.Search, Where = m.Sim.Coordinates(t.Position) }).Ok);
            Turns(m, 2);
            Assert.False(t.Trapped);
            Assert.False(t.Gone);
            Assert.Contains(m.Sim.Log.Events.OfType<RadioCall>(), r => r.Text.Contains("under the rubble"));
            Assert.Contains(m.Sim.Log.Events.OfType<RadioCall>(), r => r.Text.Contains("out."));
        }

        [Fact]
        public void AVillagerHitByOurFireBreaksThatExtraButNotTheirInjuriesFromTheCollapse()
        {
            var m = Make(StartStates.Rubble, 2, CollapsedBuilding.Passing);
            var villagers = m.Secondary.Single(o => o.Id == "villagers");
            Assert.False(villagers.IsBroken(m));
            var civ = m.Sim.Units.First(u => u.Force == m.Civilians && !u.Trapped);
            m.Sim.Wound(civ, Scene.Data.FindWeapon("m4"), m.Squad.First());
            Assert.True(villagers.IsBroken(m));
        }
    }
}
