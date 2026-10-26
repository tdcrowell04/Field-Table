using System;
using System.Collections.Generic;

namespace FieldTable.Sim
{
    /// <summary>
    /// The whole fight: map, forces, units, clock, dice, and log.
    /// Runs one WEGO turn at a time. Orders and verbs plug in at later pieces.
    /// </summary>
    public sealed class Simulation
    {
        readonly List<Force> forces = new List<Force>();
        readonly List<Unit> units = new List<Unit>();
        readonly Dice dice;

        public MapGrid Map { get; }
        public SimClock Clock { get; }
        public EventLog Log { get; } = new EventLog();
        public ulong Seed { get; }

        public IReadOnlyList<Force> Forces => forces;
        public IReadOnlyList<Unit> Units => units;

        public Simulation(MapGrid map, ulong seed, int turnSeconds = SimClock.DefaultTurnSeconds)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            Seed = seed;
            dice = new Dice(seed);
            Clock = new SimClock(turnSeconds);
        }

        public Force AddForce(Force force)
        {
            if (force == null) throw new ArgumentNullException(nameof(force));
            if (FindForce(force.Id) != null) throw new ArgumentException("A force called " + force.Id + " is already in the fight.");
            forces.Add(force);
            return force;
        }

        public Unit AddUnit(Unit unit)
        {
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            if (FindUnit(unit.Id) != null) throw new ArgumentException("A unit called " + unit.Id + " is already in the fight.");
            if (!forces.Contains(unit.Force)) throw new ArgumentException("Add " + unit.Force.Id + " to the fight before its units.");
            if (!Map.InBounds(unit.Position)) throw new ArgumentException(unit.Id + " starts off the map.");
            units.Add(unit);
            return unit;
        }

        public Force FindForce(string id) => forces.Find(f => f.Id == id);
        public Unit FindUnit(string id) => units.Find(u => u.Id == id);

        /// <summary>
        /// Rolls a die through the one seeded roller and writes it to the log.
        /// Every chance in the game goes through here, so a turn replays exactly.
        /// </summary>
        public int Roll(int sides, string actor, string reason, bool bigMoment = false)
        {
            int result = dice.Roll(sides);
            Log.Add(new DiceRolled(actor, reason, sides, result, bigMoment), Clock);
            return result;
        }

        /// <summary>Plays out one full turn for every unit at once, then moves to the next turn.</summary>
        public void RunTurn()
        {
            Log.Add(new TurnStarted(), Clock);
            int steps = Clock.StepsPerTurn;
            for (int i = 0; i < steps; i++)
            {
                StepOnce();
                Clock.Advance();
            }
            Log.Add(new TurnEnded(), Clock);
            Clock.NextTurn();
        }

        void StepOnce()
        {
            // Movement, fire, and verbs plug in here in later pieces.
        }
    }
}
