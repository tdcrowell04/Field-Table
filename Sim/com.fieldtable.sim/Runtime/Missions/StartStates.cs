using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    /// <summary>One start state the player can pick: its id and its name on the menu.</summary>
    public sealed class StartState
    {
        public string Id { get; }
        public string Name { get; }
        internal Func<GameData, NamePools, ulong, string, Mission> Make { get; }

        internal StartState(string id, string name, Func<GameData, NamePools, ulong, string, Mission> make)
        {
            Id = id; Name = name; Make = make;
        }
    }

    /// <summary>
    /// Every mission the game can make (ledger round 16). The player picks one, or Random lets a die pick (ledger
    /// round 16, item 9 (a)).
    /// </summary>
    public static class StartStates
    {
        public const string Bridge = "bridge", Raid = "raid", Crossroads = "crossroads", Crew = "crew", Clear = "clear", SupplyAmbush = "ambush", Random = "random";

        public static readonly IReadOnlyList<StartState> All = new[]
        {
            new StartState(Bridge, "Bridge seizure", BridgeSeizure.Create),
            new StartState(Raid, "Compound raid", CompoundRaid.Create),
            new StartState(Crossroads, "Hold the crossroads", HoldCrossroads.Create),
            new StartState(Crew, "Downed helicopter crew", DownedCrew.Create),
            new StartState(Clear, "Clear the village", ClearVillage.Create),
            new StartState(SupplyAmbush, "Ambush a supply party", Sim.SupplyAmbush.Create),
        };

        public static StartState Find(string id) => All.FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Makes a mission of the kind asked for, or one picked by the seed for Random (or nothing). A goal can be asked
        /// for too; otherwise a die picks what the enemy is out to do.
        /// </summary>
        public static Mission Create(string id, GameData data, NamePools names, ulong seed, string goal = null)
        {
            StartState state;
            if (string.IsNullOrEmpty(id) || string.Equals(id, Random, StringComparison.OrdinalIgnoreCase))
            {
                var d = new Dice(seed, 17);
                state = All[(int)(MissionKit.Next(d) * All.Count) % All.Count];
            }
            else state = Find(id) ?? throw new ArgumentException("Unknown mission: " + id + ".", nameof(id));
            return state.Make(data, names, seed, goal);
        }
    }
}
