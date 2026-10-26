using System;
using System.Collections.Generic;

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

    public enum MoveSpeed
    {
        Walk,
        Run,
        Crawl,
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

        /// <summary>A second weapon, like a grenade launcher or a rocket. Null for none.</summary>
        public string Launcher { get; set; }

        /// <summary>Rounds for the second weapon.</summary>
        public int LauncherAmmo { get; set; }

        /// <summary>Hand grenades carried.</summary>
        public int Grenades { get; set; }

        /// <summary>What they carry, in kilograms. More than 15 kg in deep water and they go under.</summary>
        public float Load { get; set; } = 25f;

        /// <summary>Trained to treat casualties quickly, like a corpsman.</summary>
        public bool Medic { get; set; }

        /// <summary>Which drawn figure shows this unit on the table.</summary>
        public string Figure { get; set; }
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

        /// <summary>Other names an order can call this unit by, like a last name or "Alpha AR".</summary>
        public List<string> Callsigns { get; } = new List<string>();

        public float Load { get; set; }
        public int LauncherAmmo { get; set; }
        public int Grenades { get; set; }
        public int Wounds { get; internal set; }

        /// <summary>When the unit went down, in steps. -1 while it's up.</summary>
        public int DownSince { get; internal set; } = -1;

        /// <summary>True once someone has treated a downed unit, which stops the clock on them.</summary>
        public bool Stabilized { get; internal set; }

        /// <summary>Where the unit started the mission. Withdrawing falls back toward it.</summary>
        public Vec2 Start { get; internal set; }

        /// <summary>The order the unit is carrying out, or null for none.</summary>
        public Order Order { get; internal set; }

        /// <summary>The route the unit is walking, and how far along it is.</summary>
        public List<Vec2> Path { get; internal set; }
        public int PathIndex { get; internal set; }
        public MoveSpeed Speed { get; internal set; } = MoveSpeed.Walk;
        public bool Swimming { get; internal set; }
        internal float DeepSeconds;
        internal float FireCooldown;
        internal float TaskSeconds;

        /// <summary>True for a moment after the unit fires. Shows the muzzle flash on the table.</summary>
        public bool IsFiring { get; internal set; }
        internal int FiredAtStep = -1000;

        /// <summary>An emote playing right now, like "dance", or null.</summary>
        public string Emote { get; internal set; }
        internal int EmoteUntil;

        public Unit(string id, string name, UnitDef def, Force force, Vec2 position)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("A unit needs an id.", nameof(id));
            Id = id;
            Name = name;
            Def = def ?? throw new ArgumentNullException(nameof(def));
            Force = force ?? throw new ArgumentNullException(nameof(force));
            Position = position;
            Start = position;
            Ammo = def.Ammo;
            Load = def.Load;
            LauncherAmmo = def.LauncherAmmo;
            Grenades = def.Grenades;
        }

        /// <summary>True when an order can call this unit by the given name.</summary>
        public bool IsCalled(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            string n = name.Trim();
            if (string.Equals(Id, n, StringComparison.OrdinalIgnoreCase) || string.Equals(Name, n, StringComparison.OrdinalIgnoreCase)) return true;
            foreach (var c in Callsigns)
                if (string.Equals(c, n, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public bool CanAct => Condition == Condition.Ok || Condition == Condition.Wounded;

        public void SetSuppression(float value) => Suppression = Math.Max(0f, Math.Min(1f, value));
    }
}
