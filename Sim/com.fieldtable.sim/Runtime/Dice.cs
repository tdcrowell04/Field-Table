namespace FieldTable.Sim
{
    /// <summary>
    /// The one dice roller every roll in the game goes through.
    /// Uses PCG32 so the same seed gives the same rolls on every PC,
    /// which is what lets any turn be replayed exactly.
    /// </summary>
    public sealed class Dice
    {
        const ulong Multiplier = 6364136223846793005UL;

        ulong state;
        readonly ulong increment;

        public Dice(ulong seed, ulong stream = 54UL)
        {
            increment = (stream << 1) | 1UL;
            state = 0UL;
            NextUInt();
            state += seed;
            NextUInt();
        }

        /// <summary>The next raw 32-bit value.</summary>
        public uint NextUInt()
        {
            ulong old = state;
            state = unchecked(old * Multiplier + increment);
            uint xorShifted = (uint)(((old >> 18) ^ old) >> 27);
            int rot = (int)(old >> 59);
            return (xorShifted >> rot) | (xorShifted << ((-rot) & 31));
        }

        /// <summary>Rolls one die with the given number of sides: 1 to sides, every face equally likely.</summary>
        public int Roll(int sides)
        {
            if (sides < 2) throw new System.ArgumentOutOfRangeException(nameof(sides), "A die needs at least 2 sides.");
            uint bound = (uint)sides;
            uint threshold = (0u - bound) % bound;
            while (true)
            {
                uint r = NextUInt();
                if (r >= threshold) return (int)(r % bound) + 1;
            }
        }
    }
}
