using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class SidesTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static Mission Make(string kind, string side, ulong seed = 1, MissionOptions options = null) => StartStates.Create(kind, Scene.Data, names.Value, seed, null, side, options);

        [Theory]
        [InlineData(Sides.Marines, 14, "Marine", "Raider 2", 7)]
        [InlineData(Sides.Army, 12, "soldier", "Bulldog 2", 6)]
        [InlineData(Sides.Seals, 8, "SEAL", "Trident 2", 4)]
        [InlineData(Sides.Pararescue, 5, "PJ", "Guardian 2", 3)]
        public void EachSideGoesOutAsItReallyWould(string side, int size, string member, string callsign, int loseAt)
        {
            var m = Make(StartStates.Crew, side);
            Assert.Equal(side, m.Side.Id);
            Assert.Equal(size, m.Squad.Count());
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "Lead"));
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "interpreter"));
            Assert.Equal(callsign, m.Marines.Name);
            Assert.Equal(loseAt, m.LoseAt);
            Assert.Contains("Every " + member + " comes home alive", m.Secondary.Select(o => o.Text));
            Assert.Contains(loseAt + " " + m.Side.Members + " down", m.LoseText);
            Assert.All(m.Sim.Units.Where(u => u.Force == m.Marines), u => Assert.True(u.Def != null));
            Assert.Contains(m.Brief, l => l.StartsWith("You: " + m.Side.Line));
        }

        [Fact]
        public void TheMarinesHaveTheirMarksmanAndTheCarlGustaf()
        {
            var m = Make(StartStates.Bridge, Sides.Marines);
            Assert.Equal("m38", m.Sim.UnitsCalled(m.Marines, "marksman").Single().Def.Weapon);
            Assert.Equal("m3e1", m.Sim.UnitsCalled(m.Marines, "Gustaf").Single().Def.Launcher);
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "drone operator"));
            Assert.Equal(4, m.Sim.UnitsCalled(m.Marines, "Alpha").Count);
        }

        [Fact]
        public void TheArmyBringsAMachineGunTeamAndTheMedic()
        {
            var m = Make(StartStates.Crossroads, Sides.Army);
            var guns = m.Sim.UnitsCalled(m.Marines, "Guns");
            Assert.Equal(2, guns.Count);
            Assert.Contains(guns, u => u.Def.Weapon == "m240");
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "Doc"));
            Assert.Equal(2, m.Squad.Count(u => u.Def.Launcher == "m320"));
            Assert.Equal("blackhornet", m.Squad.Single(u => u.Def.Drone != null).Def.Drone);
            Assert.Contains("6 soldiers down", m.LoseText);
            Assert.Equal("soldier on it", Make(StartStates.Hill, Sides.Army).OnItWord);
        }

        [Fact]
        public void TheSealsAreAllSuppressedWithASpecialtyEach()
        {
            var m = Make(StartStates.Raid, Sides.Seals);
            Assert.All(m.Squad, u => Assert.True(m.Sim.Units.Count > 0 && Scene.Data.FindWeapon(u.Def.Weapon).Suppressed, u.Name + " carries " + u.Def.Weapon));
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "breacher"));
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "sniper"));
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "JTAC"));
            Assert.Equal(3, m.Squad.Count(u => u.Def.Radio));
            Assert.All(m.Squad, u => Assert.True(u.Def.Thermal));
        }

        [Fact]
        public void TheBreacherGetsThroughInHalfTheTime()
        {
            var s = new Scene(paint: m => m.FillRect(300, 200, 1, 200, "wall"));
            var br = s.Add("br", "s-br", s.Blue, 297, 300);
            var rf = s.Add("rf", "rf", s.Blue, 297, 330);
            Assert.True(s.Order(br, Verbs.Breach, s.Sim.Coordinates(new Vec2(300.5f, 300.5f))).Ok);
            Assert.True(s.Order(rf, Verbs.Breach, s.Sim.Coordinates(new Vec2(300.5f, 330.5f))).Ok);
            for (int i = 0; i < 13 * SimClock.StepsPerSecond; i++) s.Sim.StepForTest();
            Assert.Contains(s.Events<BreachMade>(), b => b.Unit == br);
            Assert.DoesNotContain(s.Events<BreachMade>(), b => b.Unit == rf);
        }

        [Fact]
        public void AParamedicWorksFasterAndKeepsTheDownFromBleedingOut()
        {
            var s = new Scene();
            var pj = s.Add("pj", "pj", s.Blue, 100, 300);
            var hurt = s.Add("hurt", "rf", s.Blue, 101, 300);
            s.Sim.KnockDownForTest(hurt);
            int since = hurt.DownSince;
            Assert.True(s.Order(pj, Verbs.TreatCasualty, target: "hurt").Ok);
            for (int i = 0; i < 20 * SimClock.StepsPerSecond; i++) s.Sim.StepForTest();
            Assert.True(hurt.DownSince > since, "The bleeding clock should pause under a paramedic's hands.");
            for (int i = 0; i < 25 * SimClock.StepsPerSecond; i++) s.Sim.StepForTest();
            Assert.True(hurt.Stabilized, "A paramedic stabilizes in 40 seconds, not 60.");
        }

        [Fact]
        public void PararescueGetsItsOwnHelicoptersSooner()
        {
            var m = Make(StartStates.Crew, Sides.Pararescue, 2);
            var lead = m.Squad.Single(u => u.Id == "lead");
            Assert.True(m.Sim.Give(lead, new Order { Verb = Verbs.Request, Target = "CASEVAC", Where = m.Sim.Map.SquareOf(lead.Position).ToString() }).Ok);
            m.Sim.RunTurn();
            Assert.NotNull(m.Sim.Evac);
            int minutes = (m.Sim.Evac.ArriveStep - 0) / (60 * SimClock.StepsPerSecond);
            Assert.InRange(minutes, 5, 10);
            Assert.All(m.Squad, u => Assert.True(u.Def.Medic));
            // Not every PJ is "Doc" on the net.
            Assert.DoesNotContain(m.Sim.Log.Events.OfType<RadioCall>(), r => r.Text.StartsWith("Doc: "));
        }

        [Fact]
        public void EachSidePlaysTheMissionsThatFitIt()
        {
            Assert.Equal(9, Sides.Find(Sides.Marines).Missions.Count);
            Assert.Equal(9, Sides.Find(Sides.Army).Missions.Count);
            Assert.Equal(new[] { StartStates.Raid, StartStates.ObservationPost, StartStates.SupplyAmbush, StartStates.Crew }, Sides.Find(Sides.Seals).Missions);
            Assert.Equal(new[] { StartStates.Crew }, Sides.Find(Sides.Pararescue).Missions);
            for (ulong seed = 1; seed <= 5; seed++) Assert.Equal(StartStates.Crew, Make(StartStates.Random, Sides.Pararescue, seed).Kind);
        }

        [Fact]
        public void CustomNumbersChangeTheEnemyTheClockAndTheLosingCount()
        {
            var m = Make(StartStates.Crossroads, Sides.Marines, 1, new MissionOptions { Enemy = 10, TurnLimit = 30, LoseAt = 5, ArrivalTurn = 18 });
            Assert.Equal(10, m.Sim.Units.Count(u => u.Force == m.Enemy));
            Assert.Equal(30, m.TurnLimit);
            Assert.Equal(5, m.LoseAt);
            Assert.StartsWith("5 Marines down", m.LoseText);
            Assert.NotNull(m.Options);
            // The leaders with the radios are kept when the enemy is cut down.
            Assert.Contains(m.Sim.Units, u => u.Force == m.Enemy && u.Def.Radio);
        }

        [Fact]
        public void CustomTurnsMoveTheDelaysWinningTurnAndTheBrief()
        {
            var m = Make(StartStates.DelayAdvance, Sides.Army, 2, new MissionOptions { Enemy = 12, TurnLimit = 15 });
            Assert.Equal(15, m.TurnLimit);
            Assert.Contains("until turn 15.", m.MainObjective);
            Assert.Contains("The end of turn 15 ", m.WinText);
            Assert.Contains(m.Brief, l => l == "Custom numbers: 12 enemy soldiers in all, 15 turns.");
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy).ToList()) m.Sim.RemoveUnitForTest(e);
            for (int i = 0; i < 16 && m.Outcome == null; i++) { m.Sim.RunTurn(); m.CheckOutcome(); }
            Assert.Equal("success", m.Outcome);
            Assert.StartsWith("Turn 15,", m.OutcomeText);
        }

        [Fact]
        public void CustomCanAddEnemyAndLetAnySidePlayAnyMission()
        {
            var m = Make(StartStates.Bridge, Sides.Pararescue, 3, new MissionOptions { Enemy = 20 });
            Assert.Equal(StartStates.Bridge, m.Kind);
            Assert.Equal(Sides.Pararescue, m.Side.Id);
            var enemy = m.Sim.Units.Where(u => u.Force == m.Enemy).ToList();
            Assert.Equal(20, enemy.Count);
            Assert.Contains(enemy, u => u.Id.StartsWith("en-custom-"));
            Assert.All(enemy.Where(u => u.Id.StartsWith("en-custom-")), u => Assert.StartsWith("Rifleman ", u.Name));
            Assert.Equal(enemy.Count, enemy.Select(u => u.Name).Distinct().Count());
        }

        [Fact]
        public void TheEnemyIsTheKansianArmy()
        {
            var m = Make(StartStates.Bridge, Sides.Marines);
            Assert.Equal("Kansian Army", m.Enemy.Name);
            Assert.Equal("Marine on it", m.OnItWord);
            Assert.All(m.Sim.Units.Where(u => u.Force == m.Enemy), u => Assert.StartsWith("Kansian ", u.Def.Name));
            Assert.Contains(m.Brief, l => l.Contains("Kansian soldiers don't speak English"));
        }
    }
}
