using System;

namespace FieldTable.Sim
{
    public enum Posture
    {
        Standing,
        Kneeling,
        Prone,
    }

    public static class Postures
    {
        /// <summary>Eye height, and how tall a target is, for each posture, in meters.</summary>
        public static float Height(Posture p)
        {
            switch (p)
            {
                case Posture.Kneeling: return 1.1f;
                case Posture.Prone: return 0.4f;
                default: return 1.7f;
            }
        }

        /// <summary>How easy a target in this posture is to spot, compared with standing.</summary>
        public static float Visibility(Posture p)
        {
            switch (p)
            {
                case Posture.Kneeling: return 0.6f;
                case Posture.Prone: return 0.25f;
                default: return 1f;
            }
        }
    }

    public enum Condition
    {
        Ok,
        Wounded,
        Down,
        Dead,
    }

    /// <summary>
    /// A kind of unit, loaded from data: a Marine rifleman, an enemy machine gunner, a civilian.
    /// Each setting and mod brings its own.
    /// </summary>
    public sealed class UnitDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Weapon { get; set; }

        /// <summary>Speeds on open ground, in meters per second.</summary>
        public float WalkSpeed { get; set; } = 1.4f;
        public float RunSpeed { get; set; } = 3.5f;
        public float CrawlSpeed { get; set; } = 0.3f;

        /// <summary>Rounds carried at the start of a mission.</summary>
        public int Ammo { get; set; }

        /// <summary>Carries night vision goggles (NODs).</summary>
        public bool NightVision { get; set; }

        /// <summary>Carries a thermal sight.</summary>
        public bool Thermal { get; set; }
    }

    /// <summary>
    /// One side in the fight. Two forces can share a faction, so versus works with
    /// both players on the same faction. Who is hostile to whom is set by the mission
    /// (see Simulation.SetHostile).
    /// </summary>
    public sealed class Force
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Faction { get; set; }
        public bool Civilian { get; set; }
    }

    /// <summary>One figure on the table: a single Marine, enemy, or civilian.</summary>
    public sealed class Unit
    {
        public string Id { get; }
        public string Name { get; set; }
        public UnitDef Def { get; }
        public Force Force { get; }

        /// <summary>The team inside the force, like "Alpha". Empty for loners.</summary>
        public string Team { get; set; } = "";

        public Vec2 Position { get; set; }

        /// <summary>Which way the unit faces, in degrees clockwise from north.</summary>
        public float Facing { get; set; }

        public Posture Posture { get; set; } = Posture.Standing;

        /// <summary>True while the unit is on the move this step. Moving draws the eye.</summary>
        public bool IsMoving { get; set; }

        public Condition Condition { get; set; } = Condition.Ok;
        public int Ammo { get; set; }

        /// <summary>How pinned down the unit is, from 0 (not at all) to 1 (can't act).</summary>
        public float Suppression { get; private set; }

        public Unit(string id, string name, UnitDef def, Force force, Vec2 position)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("A unit needs an id.", nameof(id));
            Id = id;
            Name = name;
            Def = def ?? throw new ArgumentNullException(nameof(def));
            Force = force ?? throw new ArgumentNullException(nameof(force));
            Position = position;
            Ammo = def.Ammo;
        }

        public bool CanAct => Condition == Condition.Ok || Condition == Condition.Wounded;

        public void SetSuppression(float value) => Suppression = Math.Max(0f, Math.Min(1f, value));
    }
}
