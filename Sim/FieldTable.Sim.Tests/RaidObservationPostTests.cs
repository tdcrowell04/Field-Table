using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class RaidObservationPostTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static Mission Make(ulong seed, string goal = null) => StartStates.Create(StartStates.ObservationPost, Scene.Data, names.Value, seed, goal);

        static void Turns(Mission m, int n) { for (int i = 0; i < n && m.Outcome == null; i++) { m.Sim.RunTurn(); m.CheckOutcome(); } }

        static void Quiet(Mission m)
        {
            foreach (var u in m.Sim.Units) { u.Ammo = 0; u.LauncherAmmo = 0; u.LauncherAp = 0; u.Grenades = 0; }
        }

        [Theory]
        [InlineData(RaidObservationPost.Sleepy)]
        [InlineData(RaidObservationPost.Patrol)]
        [InlineData(RaidObservationPost.Trap)]
        public void APostWithItsGearAStartLineAndAReactionForceToCome(string goal)
        {
            for (ulong seed = 1; seed <= 4; seed++)
            {
                var m = Make(seed, goal);
                var map = m.Sim.Map;
                Assert.Equal(StartStates.ObservationPost, m.Kind);
                Assert.Equal("the post", m.Objective.Label);
                Assert.Equal(2, m.Sim.GearOnTheGround.Count);
                Assert.All(m.Sim.GearOnTheGround, g => Assert.True(Vec2.Distance(g.Position, m.Objective.Center) <= 3f));
                Assert.Equal("control", map.FindLandmark("the start line").Kind);
                var enemy = m.Sim.Units.Where(u => u.Force == m.Enemy).ToList();
                Assert.Equal(8, enemy.Count(u => u.Gone && u.Team == "reaction force"));
                Assert.Equal(goal == RaidObservationPost.Sleepy ? 2 : 0, enemy.Count(u => u.Asleep));
                Assert.All(m.Squad, u => Assert.True(Vec2.Distance(u.Position, m.Objective.Center) >= 200f));
                Assert.Equal("Squad leader 1", enemy.First(u => u.Def.Radio && !u.Gone).Name);
                Assert.Equal("Squad leader 2", enemy.First(u => u.Def.Radio && u.Gone).Name);
            }
        }

        [Fact]
        public void SleepersSeeNothingUntilSomeoneComesClose()
        {
            var m = Make(3, RaidObservationPost.Sleepy);
            Quiet(m);
            var sleeper = m.Sim.Units.First(u => u.Asleep);
            var marine = m.Squad.First(u => u.Id == "alpha-tl");
            marine.Position = sleeper.Position + new Vec2(40f, 0f);
            Assert.Equal(0f, FogOfWar.SpotRate(m.Sim.Map, sleeper, marine));
            // The others at the post are awake: once they see him, everyone's up.
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy && !u.Asleep && !u.Gone).ToList()) m.Sim.RemoveUnitForTest(e);
            Turns(m, 1);
            Assert.True(sleeper.Asleep);
            marine.Position = sleeper.Position + new Vec2(15f, 0f);
            Turns(m, 1);
            Assert.False(sleeper.Asleep);
        }

        [Fact]
        public void WreckTheGearAndGetBackToWin()
        {
            var m = Make(1, RaidObservationPost.Patrol);
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy).ToList()) m.Sim.RemoveUnitForTest(e);
            var marine = m.Squad.First(u => u.Id == "bravo-tl");
            marine.Position = m.Objective.Center + new Vec2(2f, 2f);
            Assert.True(m.Sim.Give(marine, new Order { Verb = Verbs.Search, Where = m.Sim.Coordinates(m.Objective.Center) }).Ok);
            Turns(m, 1);
            Assert.All(m.Sim.GearOnTheGround, g => Assert.True(g.Wrecked));
            Assert.Null(m.Outcome);
            Assert.Equal("onit", m.MainStatus);
            marine.Position = marine.Start;
            Turns(m, 1);
            Assert.Equal("success", m.Outcome);
            Assert.True(m.Secondary.Single(o => o.Id == "prisoner").IsBroken(m));
        }

        [Fact]
        public void TheReactionForceComesOnItsTurnWithAWarning()
        {
            var m = Make(2, RaidObservationPost.Patrol);
            Quiet(m);
            var qrf = m.Sim.Units.Where(u => u.Team == "reaction force").ToList();
            int came = -1;
            for (int t = 0; t < 16 && came < 0; t++)
            {
                Turns(m, 1);
                if (qrf.Any(u => !u.Gone)) came = m.Sim.Clock.Turn - 1;
            }
            Assert.InRange(came, RaidObservationPost.QrfFirst + 1, RaidObservationPost.QrfLast + 1);
            Assert.Contains(m.Sim.Log.Events.OfType<RadioCall>(), r => r.Text.StartsWith("Raider 6: Drone feed shows 8 soldiers getting out of trucks"));
        }

        [Fact]
        public void TheTrapHoldsFireUntilTheSquadIsClose()
        {
            var m = Make(2, RaidObservationPost.Trap);
            Turns(m, 2);
            Assert.False(m.Enemy.Plan.Sprung);
            var ambusher = m.Sim.Units.First(u => u.Force == m.Enemy && u.Def.Id == "en-rpg");
            m.Squad.First(u => u.Id == "alpha-tl").Position = ambusher.Position + new Vec2(20f, 0f);
            Turns(m, 1);
            Assert.True(m.Enemy.Plan.Sprung);
        }
    }
}
