using System;
using System.Linq;
using Xunit;

namespace FieldTable.Sim.Tests
{
    public class SimulationTests
    {
        static readonly UnitDef Rifleman = new UnitDef { Id = "rifleman", Name = "Rifleman", Weapon = "M27", Ammo = 210 };

        static Simulation NewSim(ulong seed = 7, int turnSeconds = 60)
        {
            var sim = new Simulation(MapGridTests.Map(), seed, turnSeconds);
            sim.AddForce(new Force { Id = "blue", Name = "Raider 2", Faction = "marines" });
            sim.AddForce(new Force { Id = "red", Name = "Enemy", Faction = "insurgents" });
            sim.AddForce(new Force { Id = "civ", Name = "Civilians", Faction = "civilians", Civilian = true });
            return sim;
        }

        [Fact]
        public void ATurnIsOneMinuteByDefault()
        {
            var clock = new SimClock();
            Assert.Equal(60, clock.TurnSeconds);
            Assert.Equal(600, clock.StepsPerTurn);
        }

        [Fact]
        public void RunningATurnAdvancesTheClockOneTurn()
        {
            var sim = NewSim();
            sim.RunTurn();
            Assert.Equal(600, sim.Clock.Step);
            Assert.Equal(60.0, sim.Clock.Seconds);
            Assert.Equal(2, sim.Clock.Turn);
            Assert.IsType<TurnStarted>(sim.Log.Events[0]);
            Assert.IsType<TurnEnded>(sim.Log.Events[1]);
            Assert.Equal(1, sim.Log.Events[1].Turn);
        }

        [Fact]
        public void TurnLengthIsASetting()
        {
            var sim = NewSim(turnSeconds: 30);
            sim.RunTurn();
            Assert.Equal(300, sim.Clock.Step);
            sim.Clock.SetTurnLength(90);
            sim.RunTurn();
            Assert.Equal(1200, sim.Clock.Step);
            Assert.Throws<ArgumentOutOfRangeException>(() => sim.Clock.SetTurnLength(0));
        }

        [Fact]
        public void EveryRollGoesInTheLog()
        {
            var sim = NewSim();
            int r = sim.Roll(20, "Alpha TL", "a shot at the gunner", bigMoment: true);
            var e = Assert.IsType<DiceRolled>(sim.Log.Events.Single());
            Assert.Equal(r, e.Result);
            Assert.Equal(20, e.Sides);
            Assert.True(e.BigMoment);
            Assert.Equal("Alpha TL rolls d20 for a shot at the gunner: " + r + ".", e.Describe());
        }

        [Fact]
        public void SameSeedReplaysTheSameFight()
        {
            string Play(ulong seed)
            {
                var sim = NewSim(seed);
                for (int t = 0; t < 3; t++)
                {
                    sim.Roll(20, "Alpha", "spot");
                    sim.RunTurn();
                    sim.Roll(6, "Bravo", "suppression");
                }
                return string.Join("\n", sim.Log.Events.Select(e => e.Step + " " + e.Describe()));
            }

            Assert.Equal(Play(11), Play(11));
            Assert.NotEqual(Play(11), Play(12));
        }

        [Fact]
        public void ForcesKnowWhoTheirEnemiesAre()
        {
            var sim = NewSim();
            Force blue = sim.FindForce("blue"), red = sim.FindForce("red"), civ = sim.FindForce("civ");
            Assert.True(blue.IsHostileTo(red));
            Assert.False(blue.IsHostileTo(blue));
            Assert.False(blue.IsHostileTo(civ));
            Assert.False(civ.IsHostileTo(red));
        }

        [Fact]
        public void VersusWorksWithBothPlayersOnTheSameFaction()
        {
            var a = new Force { Id = "p1", Faction = "marines" };
            var b = new Force { Id = "p2", Faction = "marines" };
            Assert.True(a.IsHostileTo(b));
        }

        [Fact]
        public void UnitsJoinAForceInTheFight()
        {
            var sim = NewSim();
            var blue = sim.FindForce("blue");
            var u = sim.AddUnit(new Unit("alpha-tl", "Alpha TL", Rifleman, blue, new Vec2(100f, 500f)) { Team = "Alpha" });
            Assert.Same(u, sim.FindUnit("alpha-tl"));
            Assert.Equal(210, u.Ammo);
            Assert.True(u.CanAct);

            Assert.Throws<ArgumentException>(() => sim.AddUnit(new Unit("alpha-tl", "Copy", Rifleman, blue, new Vec2(1f, 1f))));
            Assert.Throws<ArgumentException>(() => sim.AddUnit(new Unit("lost", "Lost", Rifleman, blue, new Vec2(900f, 1f))));
            var stranger = new Force { Id = "green" };
            Assert.Throws<ArgumentException>(() => sim.AddUnit(new Unit("g1", "G1", Rifleman, stranger, new Vec2(1f, 1f))));
            Assert.Throws<ArgumentException>(() => sim.AddForce(new Force { Id = "blue" }));
        }

        [Fact]
        public void SuppressionStaysBetweenZeroAndOne()
        {
            var u = new Unit("u", "U", Rifleman, new Force { Id = "blue" }, new Vec2(1f, 1f));
            u.SetSuppression(1.7f);
            Assert.Equal(1f, u.Suppression);
            u.SetSuppression(-2f);
            Assert.Equal(0f, u.Suppression);
        }

        [Fact]
        public void DownedUnitsCantAct()
        {
            var u = new Unit("u", "U", Rifleman, new Force { Id = "blue" }, new Vec2(1f, 1f));
            u.Condition = Condition.Wounded;
            Assert.True(u.CanAct);
            u.Condition = Condition.Down;
            Assert.False(u.CanAct);
        }
    }
}
