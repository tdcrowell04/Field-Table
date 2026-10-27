using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class SupplyAmbushTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static Mission Make(ulong seed, string goal = null) => StartStates.Create(StartStates.SupplyAmbush, Scene.Data, names.Value, seed, goal);

        static void Turns(Mission m, int n) { for (int i = 0; i < n && m.Outcome == null; i++) { m.Sim.RunTurn(); m.CheckOutcome(); } }

        static System.Collections.Generic.List<Unit> Party(Mission m) => m.Sim.Units.Where(u => u.Force == m.Enemy).ToList();

        /// <summary>Runs turns until the party is on the map.</summary>
        static void UntilTheyCome(Mission m)
        {
            for (int i = 0; i < 15 && Party(m).All(u => u.Gone); i++) Turns(m, 1);
            Assert.Contains(Party(m), u => !u.Gone);
        }

        static void Quiet(Mission m)
        {
            foreach (var u in m.Sim.Units) { u.Ammo = 0; u.LauncherAmmo = 0; u.LauncherAp = 0; u.Grenades = 0; }
        }

        [Theory]
        [InlineData(SupplyAmbush.Straight, 4, 6)]
        [InlineData(SupplyAmbush.Scout, 5, 7)]
        [InlineData(SupplyAmbush.MoreMen, 8, 10)]
        public void ATrailAcrossTheMapAndAPartyStillToCome(string goal, int least, int most)
        {
            for (ulong seed = 1; seed <= 5; seed++)
            {
                var m = Make(seed, goal);
                var map = m.Sim.Map;
                Assert.Equal(StartStates.SupplyAmbush, m.Kind);
                var trail = m.Objective;
                Assert.EndsWith(" Trail", trail.Label);
                Assert.True(trail.Points.Count >= 2);
                Assert.True(MissionKit.AtEdge(map, trail.Points[0], 5f) && MissionKit.AtEdge(map, trail.Points[trail.Points.Count - 1], 5f), "Seed " + seed + ": the trail doesn't run edge to edge.");
                var party = Party(m);
                Assert.InRange(party.Count, least, most);
                Assert.All(party, u => Assert.True(u.Gone));
                Assert.Equal(2, party.Count(u => u.Cargo != null));
                Assert.All(m.Squad, u => Assert.True(MissionKit.DistanceTo(u.Position, trail.Points) >= 80f));
                Assert.True(m.Secondary.Single(o => o.Id == "prisoner").AtEnd);
            }
        }

        [Fact]
        public void LeftAloneTheyWalkTheTrailAndGetThrough()
        {
            var m = Make(1, SupplyAmbush.Straight);
            Quiet(m);
            // The squad stays well away from the trail.
            foreach (var u in m.Sim.Units.Where(u => u.Force == m.Marines)) u.Position = u.Start;
            UntilTheyCome(m);
            Assert.All(Party(m), u => Assert.NotNull(u.Order));
            Turns(m, 25);
            Assert.Equal("failure", m.Outcome);
            Assert.EndsWith("got through.", m.OutcomeText);
        }

        [Fact]
        public void StopThemAndSearchTheBodiesToWin()
        {
            var m = Make(2, SupplyAmbush.Straight);
            UntilTheyCome(m);
            var party = Party(m);
            foreach (var u in party) u.Condition = Condition.Dead;
            var porters = party.Where(u => u.Cargo != null).ToList();
            Vec2 between = (porters[0].Position + porters[1].Position) * 0.5f;
            var marine = m.Squad.First(u => u.Id == "bravo-tl");
            marine.Position = between + new Vec2(1f, 0f);
            var check = m.Sim.Give(marine, new Order { Verb = Verbs.Search, Where = m.Sim.Coordinates(between) });
            Assert.True(check.Ok, check.Problem);
            Turns(m, 1);
            Assert.All(porters, p => Assert.Null(p.Cargo));
            Assert.Equal("success", m.Outcome);
            Assert.Equal("The party is stopped and the weapons are taken.", m.OutcomeText);
            Assert.True(m.Secondary.Single(o => o.Id == "prisoner").IsBroken(m));
        }

        [Fact]
        public void AmbushedThePortersRunForTheNearerEnd()
        {
            var m = Make(1, SupplyAmbush.Straight);
            Quiet(m);
            UntilTheyCome(m);
            // Well down the trail first, so the nearer end isn't right behind them.
            Turns(m, 4);
            var porter = Party(m).First(u => u.Cargo != null);
            porter.SetSuppression(0.5f);
            var rifleman = Party(m).First(u => u.Cargo == null);
            Turns(m, 1);
            Assert.True(m.Enemy.Plan.Sprung);
            Assert.Equal("onit", m.MainStatus);
            Assert.Equal(MoveSpeed.Run, porter.Speed);
            Assert.Null(rifleman.Order);
        }

        [Fact]
        public void AScoutWhoSeesMarinesTurnsThemAllBack()
        {
            var m = Make(2, SupplyAmbush.Scout);
            Quiet(m);
            UntilTheyCome(m);
            var scout = Party(m).Single(u => u.Name.StartsWith("Scout"));
            var marine = m.Squad.First(u => u.Id == "alpha-tl");
            marine.Position = scout.Position + new Vec2(25f, 0f);
            Turns(m, 1);
            Assert.True(m.Enemy.Plan.FallingBack);
            marine.Position = marine.Start;
            Turns(m, 20);
            Assert.Equal("failure", m.Outcome);
            Assert.EndsWith("got away.", m.OutcomeText);
        }
    }
}
