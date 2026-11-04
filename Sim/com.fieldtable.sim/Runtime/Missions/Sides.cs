using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    /// <summary>
    /// One side the player can command (ledger round 18): the Marines, the Army, the Navy SEALs, or Air Force
    /// Pararescue, each going out as it really would. Sides come from a data file (Data/sides.json), so a custom
    /// faction is a file, not code (ledger round 19: easily moddable).
    /// </summary>
    public sealed class Side
    {
        public string Id { get; set; }
        public string Name { get; set; }

        /// <summary>One of them, and several: "Marine" and "Marines", "soldier" and "soldiers".</summary>
        public string Member { get; set; }
        public string Members { get; set; }

        /// <summary>What the whole group is called: a squad, or a team.</summary>
        public string Group { get; set; } = "squad";

        /// <summary>The group's radio callsign, and higher's.</summary>
        public string Callsign { get; set; }
        public string Higher { get; set; }

        /// <summary>Down or dead before the group can't fight on: about half.</summary>
        public int LoseAt { get; set; }

        /// <summary>The group's line in a brief: who comes.</summary>
        public string Line { get; set; }

        /// <summary>The start states that fit what this side does for real. Custom play can pick any.</summary>
        public List<string> Missions { get; set; } = new List<string>();

        /// <summary>How long their CASEVAC takes, when they bring their own helicopters. Null uses the usual.</summary>
        public int? CasevacMin { get; set; }
        public int? CasevacMax { get; set; }

        /// <summary>How the figures are drawn: "ground" (camo that follows the ground), "ocp", or "multicam".</summary>
        public string Look { get; set; } = "ground";

        /// <summary>Everyone in the group, in column order.</summary>
        public List<ColumnSlot> Column { get; set; } = new List<ColumnSlot>();

        /// <summary>How deep the column is, from the point man back to the last in line.</summary>
        public float ColumnDepth => Column.Max(c => c.Back) + 6f;

        /// <summary>How many go out, not counting the interpreter.</summary>
        public int Size => Column.Count(c => c.Def != "terp");
    }

    /// <summary>One place in a side's column: who stands there, what kind, which team, what they answer to, and where in the column.</summary>
    public sealed class ColumnSlot
    {
        public string Id { get; set; }
        public string Def { get; set; }
        public string Team { get; set; } = "";
        public List<string> Calls { get; set; } = new List<string>();

        /// <summary>Meters back from the point man, and to the right of the way ahead.</summary>
        public float Back { get; set; }
        public float Side { get; set; }
    }

    public static class Sides
    {
        public const string Marines = "marines", Army = "army", Seals = "seals", Pararescue = "pjs";

        /// <summary>The side a force plays as. Null for a force that isn't one of ours.</summary>
        public static Side Of(Force force) => force?.Side;
    }
}
