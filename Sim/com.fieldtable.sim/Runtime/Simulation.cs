using System;
using System.Collections.Generic;

namespace FieldTable.Sim
{
    /// <summary>
    /// The whole fight: map, forces, units, clock, dice, fog of war, and log.
    /// Runs one WEGO turn at a time. Orders and verbs plug in at later pieces.
    /// </summary>
    public sealed partial class Simulation
    {
        readonly List<Force> forces = new List<Force>();
        readonly List<Unit> units = new List<Unit>();
        readonly Dictionary<string, bool> hostility = new Dictionary<string, bool>();
        readonly Dice dice;

        public MapGrid Map { get; }
        public SimClock Clock { get; }
        public EventLog Log { get; } = new EventLog();
        public FogOfWar Fog { get; } = new FogOfWar();

        /// <summary>How much light there is. Night cuts how far the naked eye sees; NODs and thermals help.</summary>
        public LightLevel Light { get; set; } = LightLevel.Day;
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
        /// Sets whether two forces are enemies, both ways. The mission decides this,
        /// so civilians can be a target (a faction attacking civilians that the player has to stop).
        /// </summary>
        public void SetHostile(Force a, Force b, bool hostile)
        {
            if (a == null || b == null) throw new ArgumentNullException(a == null ? nameof(a) : nameof(b));
            if (ReferenceEquals(a, b)) throw new ArgumentException("A force can't be its own enemy.");
            hostility[PairKey(a, b)] = hostile;
        }

        /// <summary>
        /// True when two forces are enemies. Unless the mission says otherwise,
        /// different forces are enemies and civilians are no one's enemy.
        /// </summary>
        public bool AreHostile(Force a, Force b)
        {
            if (a == null || b == null || ReferenceEquals(a, b)) return false;
            if (hostility.TryGetValue(PairKey(a, b), out bool set)) return set;
            if (a.Civilian || b.Civilian) return false;
            return !string.Equals(a.Id, b.Id, StringComparison.Ordinal);
        }

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
            frames.Clear();
            if (frameViewer != null) Snapshot();
            int steps = Clock.StepsPerTurn;
            for (int i = 0; i < steps; i++)
            {
                StepOnce();
                Clock.Advance();
            }
            Log.Add(new TurnEnded(), Clock);
            Clock.NextTurn();
        }

        /// <summary>Checks who can see whom right now, as it stands. Runs on its own once a second during a turn.</summary>
        public void LookAround(float seconds = 1f) => Fog.Update(this, seconds);

        internal void Record(SimEvent e) => Log.Add(e, Clock);

        /// <summary>One step of a tenth of a second, for tests that need to hold things in place between steps.</summary>
        internal void StepForTest()
        {
            StepOnce();
            Clock.Advance();
        }

        void StepOnce()
        {
            StepUnits(1f / SimClock.StepsPerSecond);
            if (Clock.Step % SimClock.StepsPerSecond == 0) LookAround();
            if (frameViewer != null && Clock.Step % frameEvery == 0) Snapshot();
        }

        // ---------- snapshots for playing a turn back ----------

        Force frameViewer;
        int frameEvery = 5;
        readonly List<Frame> frames = new List<Frame>();

        /// <summary>The snapshots taken during the last turn, oldest first.</summary>
        public IReadOnlyList<Frame> Frames => frames;

        /// <summary>
        /// Takes a snapshot of every unit every few steps during a turn (every half second by default),
        /// marking which ones the viewing force can see, so the table can play the turn back.
        /// </summary>
        public void RecordFrames(Force viewer, int everySteps = 5)
        {
            frameViewer = viewer;
            frameEvery = Math.Max(1, everySteps);
        }

        void Snapshot()
        {
            var f = new Frame(Clock.Step);
            var knowledge = Fog.For(frameViewer);
            foreach (var u in units)
            {
                bool seen = !u.Gone && (ReferenceEquals(u.Force, frameViewer) || (knowledge.About(u.Id)?.Visible ?? false));
                f.Units.Add(new UnitFrame(u, seen));
            }
            frames.Add(f);
        }

        static string PairKey(Force a, Force b) =>
            string.CompareOrdinal(a.Id, b.Id) < 0 ? a.Id + "|" + b.Id : b.Id + "|" + a.Id;
    }
}
