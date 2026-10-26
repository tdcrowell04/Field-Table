using System.Linq;
using Xunit;

namespace FieldTable.Sim.Tests
{
    public class FogOfWarTests
    {
        static readonly UnitDef Rifleman = new UnitDef { Id = "rifleman", Name = "Rifleman", Ammo = 210 };

        sealed class Scene
        {
            public Simulation Sim;
            public Force Blue, Red, Civ;
            public Unit Watcher, Enemy;
        }

        static Scene Make(MapGrid map, Vec2 watcher, Vec2 enemy, Posture enemyPosture = Posture.Standing)
        {
            var s = new Scene { Sim = new Simulation(map, 3) };
            s.Blue = s.Sim.AddForce(new Force { Id = "blue", Name = "Raider 2" });
            s.Red = s.Sim.AddForce(new Force { Id = "red", Name = "Enemy" });
            s.Civ = s.Sim.AddForce(new Force { Id = "civ", Name = "Civilians", Civilian = true });
            s.Watcher = s.Sim.AddUnit(new Unit("alpha-tl", "Alpha TL", Rifleman, s.Blue, watcher));
            s.Enemy = s.Sim.AddUnit(new Unit("pkm", "PKM gunner", Rifleman, s.Red, enemy) { Posture = enemyPosture });
            return s;
        }

        static void Look(Simulation sim, int seconds)
        {
            for (int i = 0; i < seconds; i++) sim.LookAround();
        }

        [Fact]
        public void SpotsSomeoneStandingInTheOpenWithinSeconds()
        {
            var s = Make(MapGridTests.Map(), new Vec2(100, 300), new Vec2(300, 300));
            Look(s.Sim, 1);
            Assert.Null(s.Sim.Fog.For(s.Blue).About("pkm"));

            Look(s.Sim, 1);
            var c = s.Sim.Fog.For(s.Blue).About("pkm");
            Assert.NotNull(c);
            Assert.True(c.Visible);
            Assert.Equal(new Vec2(300, 300), c.LastKnownPosition);

            var e = s.Sim.Log.Events.OfType<ContactSpotted>().Single(x => x.Force == s.Blue);
            Assert.Equal("Alpha TL spots PKM gunner at D4.", e.Describe());
        }

        [Fact]
        public void SomeoneProneInReedsFarOffStaysHiddenForAMinute()
        {
            var map = MapGridTests.Map();
            map.FillRect(480, 250, 40, 100, "reeds");
            var s = Make(map, new Vec2(100, 300), new Vec2(485, 300), Posture.Prone);
            Look(s.Sim, 60);
            Assert.Null(s.Sim.Fog.For(s.Blue).About("pkm"));
        }

        [Fact]
        public void MovingIsEasierToSpotThanHoldingStill()
        {
            var s = Make(MapGridTests.Map(), new Vec2(100, 300), new Vec2(600, 300));
            float still = FogOfWar.SpotRate(s.Sim.Map, s.Watcher, s.Enemy);
            s.Enemy.IsMoving = true;
            float moving = FogOfWar.SpotRate(s.Sim.Map, s.Watcher, s.Enemy);
            Assert.Equal(still * 2f, moving, 4);
        }

        [Fact]
        public void PinnedDownOrDownedObserversSpotLessOrNotAtAll()
        {
            var s = Make(MapGridTests.Map(), new Vec2(100, 300), new Vec2(600, 300));
            float normal = FogOfWar.SpotRate(s.Sim.Map, s.Watcher, s.Enemy);
            s.Watcher.SetSuppression(0.5f);
            Assert.Equal(normal * 0.5f, FogOfWar.SpotRate(s.Sim.Map, s.Watcher, s.Enemy), 4);
            s.Watcher.Condition = Condition.Down;
            Assert.Equal(0f, FogOfWar.SpotRate(s.Sim.Map, s.Watcher, s.Enemy));
        }

        [Fact]
        public void NobodySpotsAPersonPastAThousandMeters()
        {
            var map = new MapGrid("Long", 1200, 100, 1f, 100f, MapGridTests.Terrains(), "open");
            var s = Make(map, new Vec2(50, 50), new Vec2(1100, 50));
            Assert.Equal(0f, FogOfWar.SpotRate(map, s.Watcher, s.Enemy));
        }

        [Fact]
        public void ALostContactIsRememberedWhereItWasLastSeen()
        {
            var map = MapGridTests.Map();
            map.FillRect(400, 250, 20, 100, "building");
            var s = Make(map, new Vec2(100, 300), new Vec2(300, 300));
            Look(s.Sim, 3);
            Assert.True(s.Sim.Fog.For(s.Blue).About("pkm").Visible);

            s.Enemy.Position = new Vec2(500, 300);
            Look(s.Sim, 1);
            var c = s.Sim.Fog.For(s.Blue).About("pkm");
            Assert.True(c.Known);
            Assert.False(c.Visible);
            Assert.Equal(new Vec2(300, 300), c.LastKnownPosition);
            var lost = s.Sim.Log.Events.OfType<ContactLost>().Single(x => x.Force == s.Blue);
            Assert.Equal("Raider 2 loses sight of PKM gunner, last seen at D4.", lost.Describe());
        }

        [Fact]
        public void EachForceOnlyKnowsAboutOthersAndSeesCiviliansToo()
        {
            var s = Make(MapGridTests.Map(), new Vec2(100, 300), new Vec2(300, 300));
            s.Sim.AddUnit(new Unit("farmer", "Farmer", Rifleman, s.Civ, new Vec2(150, 300)));
            Look(s.Sim, 3);
            var blue = s.Sim.Fog.For(s.Blue);
            Assert.Null(blue.About("alpha-tl"));
            Assert.NotNull(blue.About("farmer"));
            Assert.NotNull(s.Sim.Fog.For(s.Red).About("alpha-tl"));
            Assert.Equal(2, blue.Contacts.Count());
        }

        [Fact]
        public void LookingHappensOnItsOwnDuringATurn()
        {
            var s = Make(MapGridTests.Map(), new Vec2(100, 300), new Vec2(300, 300));
            s.Sim.RunTurn();
            Assert.True(s.Sim.Fog.For(s.Blue).About("pkm").Visible);
            Assert.Contains(s.Sim.Log.Events, e => e is ContactSpotted);
        }
    }
}
