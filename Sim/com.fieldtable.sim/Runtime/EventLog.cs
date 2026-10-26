using System.Collections.Generic;

namespace FieldTable.Sim
{
    /// <summary>Something that happened, stamped with when it happened.</summary>
    public abstract class SimEvent
    {
        public int Step { get; internal set; }
        public int Turn { get; internal set; }

        /// <summary>A plain-English line for the combat log.</summary>
        public abstract string Describe();
    }

    public sealed class TurnStarted : SimEvent
    {
        public override string Describe() => "Turn " + Turn + " starts.";
    }

    public sealed class TurnEnded : SimEvent
    {
        public override string Describe() => "Turn " + Turn + " ends.";
    }

    public sealed class DiceRolled : SimEvent
    {
        public string Actor { get; }
        public string Reason { get; }
        public int Sides { get; }
        public int Result { get; }

        /// <summary>
        /// True for the big moments the player rolls (an attack, a skill check, a called shot).
        /// The rest roll themselves on screen.
        /// </summary>
        public bool BigMoment { get; }

        public DiceRolled(string actor, string reason, int sides, int result, bool bigMoment)
        {
            Actor = actor;
            Reason = reason;
            Sides = sides;
            Result = result;
            BigMoment = bigMoment;
        }

        public override string Describe() => Actor + " rolls d" + Sides + " for " + Reason + ": " + Result + ".";
    }

    /// <summary>Everything that happened, in order. Nothing is ever removed.</summary>
    public sealed class EventLog
    {
        readonly List<SimEvent> events = new List<SimEvent>();

        public IReadOnlyList<SimEvent> Events => events;

        internal void Add(SimEvent e, SimClock clock)
        {
            e.Step = clock.Step;
            e.Turn = clock.Turn;
            events.Add(e);
        }
    }
}
