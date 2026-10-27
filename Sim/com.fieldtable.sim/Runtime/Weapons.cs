namespace FieldTable.Sim
{
    /// <summary>
    /// A kind of weapon, loaded from data. Every shot is one burst: it takes Cycle seconds,
    /// uses Burst rounds, and rolls once to hit.
    /// </summary>
    public sealed class WeaponDef
    {
        public string Id { get; set; }
        public string Name { get; set; }

        /// <summary>Farthest it's used, in meters.</summary>
        public float Range { get; set; } = 500f;

        /// <summary>Rounds per burst.</summary>
        public int Burst { get; set; } = 3;

        /// <summary>Seconds between bursts.</summary>
        public float Cycle { get; set; } = 2.5f;

        /// <summary>Chance one burst hits a standing person in the open at 100 meters, 0 to 1.</summary>
        public float Accuracy { get; set; } = 0.3f;

        /// <summary>How much a burst pins down the people it lands near, 0 to 1.</summary>
        public float Suppress { get; set; } = 0.15f;

        /// <summary>Blast radius in meters for grenades and rockets. 0 for bullets.</summary>
        public float Blast { get; set; }

        /// <summary>Chance a hit puts someone down instead of wounding them, 0 to 1. (Where a hit lands decides it now; kept for mods.)</summary>
        public float Lethality { get; set; } = 0.4f;

        /// <summary>Blast radius in meters for armor-piercing (AP) rounds, which punch through walls. 0 when it has none.</summary>
        public float ApBlast { get; set; }

        /// <summary>What ammo it takes, like "5.56". Only the same ammo can be shared. Empty means only its own kind.</summary>
        public string Caliber { get; set; }

        /// <summary>Fitted with a suppressor or flash hider: quieter, and no muzzle flash at night.</summary>
        public bool Suppressed { get; set; }
    }
}
