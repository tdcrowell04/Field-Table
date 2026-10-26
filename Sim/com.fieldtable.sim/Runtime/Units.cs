using System;

namespace FieldTable.Sim
{
    public enum Posture
    {
        Standing,
        Kneeling,
        Prone,
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
    }

    /// <summary>
    /// One side in the fight. Two forces can share a faction, so versus works with
    /// both players on the same faction.
    /// </summary>
    public sealed class Force
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Faction { get; set; }

        /// <summary>Civilians are never anyone's enemy.</summary>
        public bool Civilian { get; set; }

        public bool IsHostileTo(Force other)
        {
            if (other == null || ReferenceEquals(this, other)) return false;
            if (Civilian || other.Civilian) return false;
            return !string.Equals(Id, other.Id, StringComparison.Ordinal);
        }
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
