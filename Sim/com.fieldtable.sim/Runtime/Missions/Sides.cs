using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    /// <summary>
    /// One side the player can command (ledger round 18): the Marines, the Army, the Navy SEALs, or Air Force
    /// Pararescue. Each goes out as it really would: who's in it, what they carry, what they're called, and which
    /// missions fit the work they do.
    /// </summary>
    public sealed class Side
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }

        /// <summary>One of them, and several: "Marine" and "Marines", "soldier" and "soldiers".</summary>
        public string Member { get; internal set; }
        public string Members { get; internal set; }

        /// <summary>What the whole group is called: a squad, or a team.</summary>
        public string Group { get; internal set; }

        /// <summary>The group's radio callsign, and higher's.</summary>
        public string Callsign { get; internal set; }
        public string Higher { get; internal set; }

        /// <summary>Down or dead before the group can't fight on: about half.</summary>
        public int LoseAt { get; internal set; }

        /// <summary>The group's line in a brief: who comes.</summary>
        public string Line { get; internal set; }

        /// <summary>The start states that fit what this side does for real. Custom play can pick any.</summary>
        public IReadOnlyList<string> Missions { get; internal set; }

        /// <summary>How long their CASEVAC takes, when they bring their own helicopters. Null uses the usual.</summary>
        public int? CasevacMin { get; internal set; }
        public int? CasevacMax { get; internal set; }

        /// <summary>Everyone in the group, in column order: id, unit kind, team, callsigns, and where they stand in the column.</summary>
        internal (string id, string def, string team, string[] calls, float back, float side)[] Column { get; set; }

        /// <summary>How deep the column is, from the point man back to the last in line.</summary>
        public float ColumnDepth => Column.Max(c => c.back) + 6f;

        /// <summary>How many go out, not counting the interpreter.</summary>
        public int Size => Column.Count(c => c.def != "terp");
    }

    public static class Sides
    {
        public const string Marines = "marines", Army = "army", Seals = "seals", Pararescue = "pjs";

        static readonly string[] Everything = { StartStates.Bridge, StartStates.Raid, StartStates.Crossroads, StartStates.Crew, StartStates.Clear, StartStates.SupplyAmbush, StartStates.Hill, StartStates.DelayAdvance, StartStates.ObservationPost };

        static string[] Team(string team, params string[] rest) => new[] { team + " " + rest[0] }.Concat(rest.Skip(1).Select(r => team + " " + r)).ToArray();

        public static readonly IReadOnlyList<Side> All = new[]
        {
            // The 13-Marine squad the Corps settled on in 2025: a squad leader and three fire teams of four, plus the
            // platoon's corpsman. Each team's rifleman has a specialty: Alpha's is the marksman with the M38, Bravo's
            // carries the Carl Gustaf, and Charlie's flies the drone.
            new Side
            {
                Id = Marines, Name = "Marines", Member = "Marine", Members = "Marines", Group = "squad", Callsign = "Raider 2", Higher = "Raider 6", LoseAt = 7,
                Line = "13 Marines in three fire teams plus Doc and an interpreter",
                Missions = Everything,
                Column = new[]
                {
                    ("alpha-tl", "tl", "Alpha", Team("Alpha", "TL", "team leader", "lead"), 0f, 0f),
                    ("alpha-ar", "ar", "Alpha", Team("Alpha", "AR", "automatic rifleman", "gunner"), 7f, -7f),
                    ("alpha-aar", "aar", "Alpha", Team("Alpha", "AAR", "assistant gunner"), 14f, -9f),
                    ("alpha-rf", "mk", "Alpha", new[] { "Alpha rifleman", "Alpha marksman", "marksman" }, 7f, 7f),
                    ("lead", "sl", "", new[] { "Lead", "SL", "squad leader" }, 25f, 0f),
                    ("doc", "doc", "", new[] { "Doc", "corpsman" }, 30f, 5f),
                    ("terp", "terp", "", new[] { "Interpreter", "terp" }, 30f, -5f),
                    ("bravo-tl", "tl", "Bravo", Team("Bravo", "TL", "team leader", "lead"), 45f, 0f),
                    ("bravo-ar", "ar", "Bravo", Team("Bravo", "AR", "automatic rifleman", "gunner"), 52f, -7f),
                    ("bravo-aar", "aar", "Bravo", Team("Bravo", "AAR", "assistant gunner"), 59f, -9f),
                    ("bravo-rf", "cg", "Bravo", new[] { "Bravo rifleman", "Gustaf", "Carl Gustaf", "Gustaf gunner" }, 52f, 7f),
                    ("charlie-tl", "tl", "Charlie", Team("Charlie", "TL", "team leader", "lead"), 70f, 0f),
                    ("charlie-ar", "ar", "Charlie", Team("Charlie", "AR", "automatic rifleman", "gunner"), 77f, -7f),
                    ("charlie-aar", "aar", "Charlie", Team("Charlie", "AAR", "assistant gunner"), 84f, -9f),
                    ("charlie-rf", "dr", "Charlie", new[] { "Charlie rifleman", "drone operator" }, 77f, 7f),
                },
            },
            // The Army's 9-soldier squad: a squad leader and two fire teams of four (team leader, automatic rifleman,
            // grenadier, rifleman), with what the platoon really sends along: its medic and an M240 machine gun team.
            // Alpha's rifleman flies the Black Hornet.
            new Side
            {
                Id = Army, Name = "Army", Member = "soldier", Members = "soldiers", Group = "squad", Callsign = "Bulldog 2", Higher = "Bulldog 6", LoseAt = 6,
                Line = "a 9-soldier squad in two fire teams, plus a machine gun team, the platoon medic, and an interpreter",
                Missions = Everything,
                Column = new[]
                {
                    ("alpha-tl", "a-tl", "Alpha", Team("Alpha", "TL", "team leader", "lead"), 0f, 0f),
                    ("alpha-ar", "a-ar", "Alpha", Team("Alpha", "AR", "automatic rifleman", "SAW"), 7f, -7f),
                    ("alpha-gr", "a-gr", "Alpha", Team("Alpha", "grenadier", "203"), 14f, -9f),
                    ("alpha-rf", "a-dr", "Alpha", new[] { "Alpha rifleman", "drone operator" }, 7f, 7f),
                    ("lead", "a-sl", "", new[] { "Lead", "SL", "squad leader" }, 25f, 0f),
                    ("doc", "a-med", "", new[] { "Doc", "medic" }, 30f, 5f),
                    ("terp", "terp", "", new[] { "Interpreter", "terp" }, 30f, -5f),
                    ("guns-mg", "a-mg", "Guns", new[] { "Guns gunner", "machine gunner", "240 gunner", "gun team leader" }, 42f, -4f),
                    ("guns-ag", "a-ag", "Guns", new[] { "Guns AG", "assistant gunner", "Guns assistant" }, 42f, 4f),
                    ("bravo-tl", "a-tl", "Bravo", Team("Bravo", "TL", "team leader", "lead"), 55f, 0f),
                    ("bravo-ar", "a-ar", "Bravo", Team("Bravo", "AR", "automatic rifleman", "SAW"), 62f, -7f),
                    ("bravo-gr", "a-gr", "Bravo", Team("Bravo", "grenadier", "203"), 69f, -9f),
                    ("bravo-rf", "a-rf", "Bravo", new[] { "Bravo rifleman" }, 62f, 7f),
                },
            },
            // A SEAL squad of 8, half a platoon, each with a specialty on top of being a shooter. The officer in charge
            // is Lead. Raids, ambushes, and watching from close in; not holding ground against a platoon.
            new Side
            {
                Id = Seals, Name = "Navy SEALs", Member = "SEAL", Members = "SEALs", Group = "squad", Callsign = "Trident 2", Higher = "Trident 6", LoseAt = 4,
                Line = "a SEAL squad of 8, each with a specialty, plus an interpreter",
                Missions = new[] { StartStates.Raid, StartStates.ObservationPost, StartStates.SupplyAmbush, StartStates.Crew },
                Column = new[]
                {
                    ("alpha-pt", "s-pt", "Alpha", new[] { "Alpha point", "point man", "point" }, 0f, 0f),
                    ("alpha-br", "s-br", "Alpha", new[] { "Alpha breacher", "breacher" }, 7f, -6f),
                    ("alpha-com", "s-com", "Alpha", new[] { "Alpha comms", "communicator", "comms", "radioman" }, 14f, -8f),
                    ("alpha-mg", "s-mg", "Alpha", new[] { "Alpha gunner", "machine gunner", "gunner" }, 7f, 6f),
                    ("lead", "s-oic", "", new[] { "Lead", "OIC", "officer in charge", "boss" }, 24f, 0f),
                    ("terp", "terp", "", new[] { "Interpreter", "terp" }, 29f, -5f),
                    ("bravo-sn", "s-sn", "Bravo", new[] { "Bravo sniper", "sniper" }, 40f, 0f),
                    ("bravo-med", "s-med", "Bravo", new[] { "Bravo medic", "Doc", "medic", "corpsman" }, 47f, -6f),
                    ("bravo-jtac", "s-jtac", "Bravo", new[] { "Bravo JTAC", "JTAC" }, 47f, 6f),
                },
            },
            // Pararescue: a Combat Rescue Officer leading four PJs in two pairs, brought in by their own rescue
            // helicopters, which wait nearby. Paramedics first, shooters when they have to be.
            new Side
            {
                Id = Pararescue, Name = "Air Force Pararescue", Member = "PJ", Members = "PJs", Group = "team", Callsign = "Guardian 2", Higher = "Guardian 6", LoseAt = 3,
                Line = "a Combat Rescue Officer and 4 PJs in two pairs, plus an interpreter",
                Missions = new[] { StartStates.Crew },
                CasevacMin = 5, CasevacMax = 10,
                Column = new[]
                {
                    ("alpha-1", "pj", "Alpha", new[] { "Alpha one", "Alpha lead" }, 0f, 0f),
                    ("alpha-2", "pj", "Alpha", new[] { "Alpha two" }, 7f, -6f),
                    ("lead", "pj-cro", "", new[] { "Lead", "CRO", "rescue officer", "team leader" }, 16f, 0f),
                    ("terp", "terp", "", new[] { "Interpreter", "terp" }, 21f, -5f),
                    ("bravo-1", "pj", "Bravo", new[] { "Bravo one", "Bravo lead" }, 30f, 0f),
                    ("bravo-2", "pj", "Bravo", new[] { "Bravo two" }, 37f, -6f),
                },
            },
        };

        public static Side Find(string id) => All.FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));

        /// <summary>The side a force plays as, from its faction; the Marines when it isn't one of ours.</summary>
        public static Side Of(Force force) => Find(force?.Faction) ?? All[0];
    }
}
