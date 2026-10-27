using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class DirectorTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static Mission Make(ulong seed = 5, string goal = "hold") => BridgeSeizure.Create(Scene.Data, names.Value, seed, goal);

        static void Turns(Mission m, int n) { for (int i = 0; i < n; i++) { m.Sim.RunTurn(); m.CheckOutcome(); } }

        [Fact]
        public void NothingBeforeTurnThreeThenOneAtATimeAndNeverMoreThanThree()
        {
            var m = Make();
            Assert.Empty(m.Director.Options());
            Turns(m, 1);
            Assert.Empty(m.Director.Options());
            Turns(m, 1);
            Assert.Equal(3, m.Sim.Clock.Turn);
            Assert.Contains(m.Director.Options(), o => o.Id == Director.RadioDown);
            Assert.True(m.Director.Apply(Director.RadioDown));
            Assert.DoesNotContain(m.Director.Options(), o => o.Id == Director.RadioDown);
            Turns(m, 1);
            Assert.Empty(m.Director.Options());
            Turns(m, 1);
            Assert.False(m.Director.Apply(Director.RadioDown));
        }

        [Fact]
        public void ADeadRadioMeansShoutingDistanceUntilItComesBack()
        {
            var m = Make();
            Turns(m, 2);
            Assert.True(m.Director.Apply(Director.RadioDown));
            string team = m.Sim.RadiosDown.Single();
            Assert.Contains(m.Sim.Log.Events.OfType<RadioCall>(), r => r.Text == "Lead: No answer from " + team + ". Their radio's down.");
            var lead = m.Sim.Units.First(u => u.Id == "lead");
            var member = m.Squad.First(u => u.Team == team);
            member.Position = lead.Position + new Vec2(80, 0);
            var check = m.Sim.Give(member, new Order { Verb = Verbs.Hold });
            Assert.False(check.Ok);
            Assert.Contains("radio's down", check.Problem);
            member.Position = lead.Position + new Vec2(20, 0);
            Assert.True(m.Sim.Give(member, new Order { Verb = Verbs.Hold }).Ok);
            Turns(m, 4);
            Assert.Empty(m.Sim.RadiosDown);
            Assert.Contains(m.Sim.Log.Events.OfType<RadioCall>(), r => r.Text == team + ": Radio check, Lead. We're back up.");
        }

        [Fact]
        public void ReinforcementsComeWhenTheSquadIsSlow()
        {
            var m = Make();
            Turns(m, 11);
            Assert.Equal(12, m.Sim.Clock.Turn);
            int before = m.Sim.Units.Count(u => u.Force == m.Enemy);
            Assert.True(m.Director.Apply(Director.Reinforcements));
            int added = m.Sim.Units.Count(u => u.Force == m.Enemy) - before;
            Assert.InRange(added, 2, 4);
            Assert.Contains(m.Sim.Log.Events.OfType<RadioCall>(), r => r.Text.StartsWith("Raider 6: Drone feed shows a truck dropping off " + added + " soldiers"));
            Assert.All(m.Sim.Units.Where(u => u.Force == m.Enemy).Skip(before), u => Assert.NotNull(u.Order));
        }

        [Fact]
        public void NoReinforcementsOnceNobodyIsLeftWithARadio()
        {
            var m = Make();
            Turns(m, 11);
            var radios = m.Sim.Units.Where(u => u.Force == m.Enemy && u.Def.Radio).ToList();
            Assert.NotEmpty(radios);
            foreach (var r in radios) r.Condition = Condition.Dead;
            Assert.DoesNotContain(m.Director.Options(), o => o.Id == Director.Reinforcements);
        }

        [Fact]
        public void ASniperOnlyOnceTheSquadIsMoving()
        {
            var m = Make();
            Turns(m, 2);
            Assert.DoesNotContain(m.Director.Options(), o => o.Id == Director.Sniper);
            var lead = m.Sim.Units.First(u => u.Id == "lead");
            lead.Position = lead.Start + (m.Objective.Center - lead.Start) * (70f / Vec2.Distance(lead.Start, m.Objective.Center));
            Assert.True(m.Director.Apply(Director.Sniper));
            var sniper = Assert.Single(m.Sim.Units, u => u.Def.Id == "en-snp");
            Assert.InRange(Vec2.Distance(sniper.Position, m.Objective.Center), 200f, 460f);
        }

        [Fact]
        public void AnIedByTheRoadGoesOffUnderWhoeverStepsOnIt()
        {
            for (ulong seed = 1; seed <= 10; seed++)
            {
                var m = Make(seed);
                Turns(m, 2);
                if (!m.Director.Options().Any(o => o.Id == Director.Ied)) continue;
                Assert.True(m.Director.Apply(Director.Ied));
                var ied = Assert.Single(m.Sim.Ieds);
                Assert.InRange(Vec2.Distance(ied.Position, m.Objective.Center), 65f, 155f);
                var victim = m.Sim.Units.First(u => u.Id == "alpha-rf");
                victim.Position = ied.Position;
                Turns(m, 1);
                Assert.True(ied.Blown);
                Assert.Contains(m.Sim.Log.Events.OfType<BlastAt>(), b => b.Weapon.Id == "ied");
                Assert.Contains(m.Sim.Log.Events.OfType<RadioCall>(), r => r.Text.Contains("IED! IED at"));
                return;
            }
            Assert.Fail("No map in 10 had a road for an IED.");
        }

        [Fact]
        public void MarinesOnTheFarBankMakeTheEnemyPullBack()
        {
            var m = Make();
            Turns(m, 2);
            Assert.DoesNotContain(m.Director.Options(), o => o.Id == Director.PullBack);
            var lead = m.Sim.Units.First(u => u.Id == "lead");
            var far = m.Sim.Units.Where(u => u.Force == m.Enemy).OrderByDescending(u => Vec2.Distance(u.Position, m.Objective.Center)).First();
            lead.Position = far.Position + (far.Position - m.Objective.Center) * (100f / Vec2.Distance(far.Position, m.Objective.Center));
            Assert.Contains(m.Director.Options(), o => o.Id == Director.PullBack);
            Assert.True(m.Director.Apply(Director.PullBack));
            Assert.Contains(m.Sim.Units, u => u.Force == m.Enemy && u.Order != null && u.Order.Verb == Verbs.Withdraw);
        }

        [Fact]
        public void CiviliansRunWhenTheShootingGetsClose()
        {
            var m = Make();
            Turns(m, 1);
            var civ = m.Sim.Units.First(u => u.Force == m.Civilians);
            m.Sim.Explode(m.Sim.Units.First(u => u.Id == "alpha-tl"), Scene.Data.FindWeapon("m320"), civ.Position + new Vec2(60, 0), ap: false);
            Turns(m, 1);
            Assert.Contains(m.Director.Options(), o => o.Id == Director.CiviliansRun);
            Assert.True(m.Director.Apply(Director.CiviliansRun));
            Assert.Contains(m.Sim.Units, u => u.Force == m.Civilians && u.Order != null && u.Order.Verb == Verbs.Move);
        }
    }
}
