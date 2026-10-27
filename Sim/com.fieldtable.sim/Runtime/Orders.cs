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
        public const string Bound = "bound";
        public const string Overwatch = "overwatch";
        public const string ThrowSmoke = "throw smoke";
        public const string Clear = "clear";
        public const string Breach = "breach";
        public const string FlyDrone = "fly drone";
        public const string Talk = "talk";
        public const string Search = "search";
        public const string Carry = "carry";
        public const string Detain = "detain";
        public const string ShareAmmo = "share ammo";
        public const string Request = "request";

        public static readonly IReadOnlyList<string> Built = new[]
        {
            Move, Hold, Fire, Suppress, TakeCover, ChangePosture, Assault, Withdraw, Follow,
            Observe, Report, TreatCasualty, ThrowGrenade, Hide, Drop, Emote,
            Bound, Overwatch, ThrowSmoke, Clear, Breach, FlyDrone, Talk, Search, Carry, Detain, ShareAmmo, Request,
        };

        /// <summary>Verbs that need the unit to walk somewhere. A Marine hit in the leg can't do these.</summary>
        internal static bool NeedsLegs(string verb) =>
            verb == Move || verb == Assault || verb == Withdraw || verb == Follow || verb == TakeCover || verb == Hide ||
            verb == TreatCasualty || verb == Bound || verb == Clear || verb == Breach || verb == Talk || verb == Search ||
            verb == Carry || verb == Detain;
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

        /// <summary>
        /// What kind: for emotes, "dance", "salute", "wave", and so on; for a launcher, "HE" or "AP";
        /// for a request, what's asked for, like "CASEVAC".
        /// </summary>
        public string Kind { get; set; }

        internal Vec2? Point;
        internal Unit TargetUnit;
        internal bool Done;
        internal int Group;

        // Working state for verbs that take more than one step.
        internal Vec2? Watch;
        internal Landmark Building;
        internal int Half;
        internal bool Arrived, LegDone, Fired, Announced;
        internal float LegMeters;
        internal Vec2 LastPos;
        internal List<Vec2> SavedPath;
        internal int SavedIndex;
        internal int Phase;

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
