using System;
using System.Collections.Generic;
using System.Linq;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    /// <summary>A small fight on an open map, built from the real game data, for tests.</summary>
    sealed class Scene
    {
        static readonly Lazy<GameData> data = new Lazy<GameData>(() =>
            GameData.Combine(GameData.FromJson(Read("terrain.json")), GameData.FromJson(Read("units.json"))));

        public static GameData Data => data.Value;

        public readonly MapGrid Map;
        public readonly Simulation Sim;
        public readonly Force Blue, Red, Civ;

        public Scene(ulong seed = 1, Action<MapGrid> paint = null)
        {
            Map = new MapGrid("Test", 800, 600, 1f, 100f, Data.Terrains, "grass");
            paint?.Invoke(Map);
            Sim = new Simulation(Map, seed);
            Sim.AddWeapons(Data.Weapons);
            Blue = Sim.AddForce(new Force { Id = "blue", Name = "Raider 2", Faction = "marines" });
            Red = Sim.AddForce(new Force { Id = "red", Name = "Enemy", Faction = "insurgents" });
            Civ = Sim.AddForce(new Force { Id = "civ", Name = "Civilians", Civilian = true });
            Sim.PlayerForce = Blue;
        }

        public Unit Add(string id, string def, Force force, float x, float y, string team = "", Posture posture = Posture.Standing)
        {
            var u = new Unit(id, id, Data.FindUnit(def), force, new Vec2(x, y)) { Team = team, Posture = posture };
            return Sim.AddUnit(u);
        }

        public OrderCheck Order(Unit u, string verb, string where = null, string target = null, string speed = null, bool swim = false, string posture = null) =>
            Sim.Give(u, new Order { Verb = verb, Where = where, Target = target, Speed = speed, Swim = swim, Posture = posture });

        public IEnumerable<T> Events<T>() where T : SimEvent => Sim.Log.Events.OfType<T>();
    }
}
