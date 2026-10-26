using System;
using System.Collections.Generic;

namespace FieldTable.Sim
{
    /// <summary>The verbs the simulation can carry out so far. The rest of verbs.md comes in as each gets built.</summary>
    public static class Verbs
    {
        public const string Move = "move";
        public const string Hold = "hold";
        public const string Fire = "fire";
        public const string Suppress = "suppress";
        public const string TakeCover = "take cover";
        public const string ChangePosture = "change posture";
        public const string Assault = "assault";
        public const string Withdraw = "withdraw";
        public const string Follow = "follow";
        public const string Observe = "observe";
        public const string Report = "report";
        public const string TreatCasualty = "treat casualty";
        public const string ThrowGrenade = "throw grenade";
        public const string Hide = "hide";
        public const string Drop = "drop";
        public const string Emote = "emote";

        public static readonly IReadOnlyList<string> Built = new[]
        {
            Move, Hold, Fire, Suppress, TakeCover, ChangePosture, Assault, Withdraw, Follow,
            Observe, Report, TreatCasualty, ThrowGrenade, Hide, Drop, Emote,
        };
    }

    /// <summary>
    /// One order to one unit: a verb plus its blanks. The order reader fills these in from what the player typed;
    /// the simulation checks them and carries them out.
    /// </summary>
    public sealed class Order
    {
        public string Verb { get; set; }

        /// <summary>Where: a landmark ("building 4"), a grid ("G4"), or a unit ("Bravo").</summary>
        public string Where { get; set; }

        /// <summary>Who or what: an enemy the force has spotted, a casualty, a unit to follow.</summary>
        public string Target { get; set; }

        /// <summary>How: "walk", "run", or "crawl".</summary>
        public string Speed { get; set; }

        /// <summary>How: "stand", "kneel", or "prone".</summary>
        public string Posture { get; set; }

        /// <summary>How: true lets the route go through deep water.</summary>
        public bool Swim { get; set; }

        /// <summary>For emotes: "dance", "salute", "wave", and so on.</summary>
        public string Kind { get; set; }

        internal Vec2? Point;
        internal Unit TargetUnit;
        internal bool Done;
        internal int Group;

        public Order Copy() => new Order { Verb = Verb, Where = Where, Target = Target, Speed = Speed, Posture = Posture, Swim = Swim, Kind = Kind };
    }

    /// <summary>What the simulation thinks of an order before it's carried out.</summary>
    public sealed class OrderCheck
    {
        public Unit Unit { get; internal set; }
        public bool Ok { get; internal set; }

        /// <summary>Why it can't be done, in a unit's words, when Ok is false.</summary>
        public string Problem { get; internal set; }

        /// <summary>Things a Marine would flag before going, like swimming in full kit.</summary>
        public List<string> Warnings { get; } = new List<string>();

        /// <summary>The route the unit will take, if it moves.</summary>
        public List<Vec2> Path { get; internal set; }
    }
}
