using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class DownedCrewTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static Mission Make(ulong seed, string goal = null) => StartStates.Create(StartStates.Crew, Scene.Data, names.Value, seed, goal);

        static void Turns(Mission m, int n) { for (int i = 0; i < n && m.Outcome == null; i++) { m.Sim.RunTurn(); m.CheckOutcome(); } }

        static Unit Pilot(Mission m) => m.Sim.Units.First(u => u.Id == "pilot");
        static Unit Chief(Mission m) => m.Sim.Units.First(u => u.Id == "crew-chief");

        static void NoEnemy(Mission m)
        {
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy).ToList()) m.Sim.RemoveUnitForTest(e);
        }

        [Theory]
        [InlineData(DownedCrew.Race)]
        [InlineData(DownedCrew.Bait)]
        [InlineData(DownedCrew.Search)]
        public void TwoCrewHidingNearTheWreckThePilotHurt(string goal)
        {
            for (ulong seed = 1; seed <= 5; seed++)
            {
                var m = Make(seed, goal);
                Assert.Equal(StartStates.Crew, m.Kind);
                Assert.Equal("the wreck", m.Objective.Label);
                var w = m.Objective.Center;
                var pilot = Pilot(m);
                var chief = Chief(m);
                Assert.Equal(Condition.Down, pilot.Condition);
                Assert.True(pilot.LegHit);
                Assert.Equal(Condition.Ok, chief.Condition);
                Assert.True(pilot.Passenger && chief.Passenger);
                Assert.False(m.Sim.AreHostile(m.Marines, pilot.Force));
                Assert.True(m.Sim.AreHostile(m.Enemy, pilot.Force));
                Assert.InRange(Vec2.Distance(pilot.Position, w), 30f, 160f);
                Assert.True(m.Squad.All(u => Vec2.Distance(u.Position, w) >= 200f), "Seed " + seed + ": a Marine starts too near the wreck.");
                Assert.Equal("notyet", m.MainStatus);
                Assert.DoesNotContain(m.Squad, u => u.Name == pilot.Name.Substring(3) || u.Name == chief.Name.Substring(4));
            }
        }

        [Fact]
        public void TheirSurvivalRadioCallsWhenAMarineIsClose()
        {
            var m = Make(2, DownedCrew.Bait);
            NoEnemy(m);
            var chief = Chief(m);
            var alpha = m.Squad.First(u => u.Id == "alpha-tl");
            alpha.Position = chief.Position + new Vec2(0f, 120f);
            Turns(m, 1);
            var call = Assert.Single(m.Sim.Log.Events.OfType<RadioCall>(), r => r.Text.Contains("on the survival radio"));
            Assert.Contains(m.Sim.Map.SquareOf(chief.Position).ToString(), call.Text);
            Assert.Contains("can't walk", call.Text);
            Assert.True(m.Sim.Fog.For(m.Marines).About(chief.Id).Known);
        }

        [Fact]
        public void TreatThePilotCallCasevacAndBothGetOut()
        {
            var m = Make(1, DownedCrew.Race);
            NoEnemy(m);
            var pilot = Pilot(m);
            var doc = m.Squad.First(u => u.Def.Medic);
            var lead = m.Squad.First(u => u.Id == "lead");
            doc.Position = pilot.Position + new Vec2(2f, 0f);
            lead.Position = pilot.Position + new Vec2(-3f, 0f);
            Turns(m, 1);
            Assert.Equal("onit", m.MainStatus);
            Assert.True(m.Sim.Give(doc, new Order { Verb = Verbs.TreatCasualty, Target = pilot.Name }).Ok);
            var call = m.Sim.Give(lead, new Order { Verb = Verbs.Request, Kind = "CASEVAC", Where = m.Sim.Coordinates(pilot.Position + new Vec2(-8f, 0f)) });
            Assert.True(call.Ok, call.Problem);
            Turns(m, 35);
            Assert.True(pilot.Stabilized);
            Assert.Equal("success", m.Outcome);
            Assert.Equal("Both crew are out on the CASEVAC.", m.OutcomeText);
            Assert.True(pilot.Evacuated && Chief(m).Evacuated);
            Assert.False(m.Secondary.Single(o => o.Id == "crew").IsBroken(m));
        }

        [Fact]
        public void AMarineCanCarryThePilot()
        {
            var m = Make(1, DownedCrew.Race);
            NoEnemy(m);
            var pilot = Pilot(m);
            var marine = m.Squad.First(u => u.Id == "bravo-rf");
            marine.Position = pilot.Position + new Vec2(2f, 0f);
            m.Sim.LookAround();
            m.Sim.Fog.Spot(m.Sim, m.Marines, pilot, marine);
            var check = m.Sim.Give(marine, new Order { Verb = Verbs.Carry, Target = pilot.Name, Where = m.Sim.Coordinates(pilot.Position + new Vec2(0f, 40f)) });
            Assert.True(check.Ok, check.Problem);
            // Alone, a carrier goes at a quarter of a walk: 40 m takes a few minutes.
            Turns(m, 6);
            Assert.Null(pilot.CarriedBy);
            Assert.InRange(Vec2.Distance(pilot.Position, pilot.Start + new Vec2(0f, 40f)), 0f, 6f);
            // The crew chief keeps with him.
            Assert.InRange(Vec2.Distance(Chief(m).Position, pilot.Position), 0f, 10f);
        }

        [Fact]
        public void TheEnemyGettingToThemFirstTakesThemAndLoses()
        {
            var m = Make(3, DownedCrew.Race);
            NoEnemy(m);
            var pilot = Pilot(m);
            var chief = Chief(m);
            var e1 = new Unit("e1", "Rifleman 9", Scene.Data.FindUnit("en-rf"), m.Enemy, pilot.Position + new Vec2(1f, 1f)) { Ammo = 0 };
            var e2 = new Unit("e2", "Rifleman 10", Scene.Data.FindUnit("en-rf"), m.Enemy, chief.Position + new Vec2(1f, -1f)) { Ammo = 0 };
            m.Sim.AddUnit(e1);
            m.Sim.AddUnit(e2);
            chief.Ammo = 0;
            Turns(m, 1);
            Assert.True(pilot.Captured && chief.Captured);
            Assert.Equal("failure", m.Outcome);
            Assert.Equal("The enemy captured the crew.", m.OutcomeText);
        }

        [Fact]
        public void TheRaceHeadsStraightForTheWreck()
        {
            var m = Make(1, DownedCrew.Race);
            var w = m.Objective.Center;
            foreach (var u in m.Sim.Units) { u.Ammo = 0; u.LauncherAmmo = 0; u.LauncherAp = 0; u.Grenades = 0; }
            var enemy = m.Sim.Units.Where(u => u.Force == m.Enemy).ToList();
            float before = enemy.Min(e => Vec2.Distance(e.Position, w));
            Turns(m, 4);
            float after = enemy.Min(e => Vec2.Distance(e.Position, w));
            Assert.True(after < before - 150f || after < 30f, "Nearest went from " + before + " to " + after + " m.");
        }

        [Fact]
        public void TheBaitHoldsFireUntilSomeoneComesToTheWreck()
        {
            var m = Make(2, DownedCrew.Bait);
            Turns(m, 3);
            Assert.DoesNotContain(m.Sim.Log.Events.OfType<ShotFired>(), s => s.Shooter.Force == m.Enemy);
            Assert.False(m.Enemy.Plan.Sprung);
            m.Squad.First(u => u.Id == "alpha-tl").Position = m.Objective.Center + new Vec2(3f, 3f);
            Turns(m, 1);
            Assert.True(m.Enemy.Plan.Sprung);
        }
    }
}
