using System.Linq;
using Xunit;

namespace FieldTable.Sim.Tests
{
    public class DiceTests
    {
        [Fact]
        public void SameSeedGivesSameRolls()
        {
            var a = new Dice(1234);
            var b = new Dice(1234);
            for (int i = 0; i < 1000; i++) Assert.Equal(a.Roll(20), b.Roll(20));
        }

        [Fact]
        public void DifferentSeedsGiveDifferentRolls()
        {
            var a = new Dice(1);
            var b = new Dice(2);
            var ra = Enumerable.Range(0, 50).Select(_ => a.Roll(100)).ToArray();
            var rb = Enumerable.Range(0, 50).Select(_ => b.Roll(100)).ToArray();
            Assert.NotEqual(ra, rb);
        }

        [Theory]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(8)]
        [InlineData(10)]
        [InlineData(12)]
        [InlineData(20)]
        [InlineData(100)]
        public void RollsStayOnTheDieAndHitEveryFace(int sides)
        {
            var dice = new Dice(99);
            var counts = new int[sides + 1];
            int rolls = sides * 2000;
            for (int i = 0; i < rolls; i++)
            {
                int r = dice.Roll(sides);
                Assert.InRange(r, 1, sides);
                counts[r]++;
            }
            // Every face should land close to its fair share (2000), well inside random wobble.
            for (int face = 1; face <= sides; face++) Assert.InRange(counts[face], 1700, 2300);
        }

        [Fact]
        public void RollsMatchThePinnedSequence()
        {
            // Pinned so a change to the roller (which would break saved replays) gets caught.
            var dice = new Dice(42);
            var rolls = Enumerable.Range(0, 10).Select(_ => dice.Roll(20)).ToArray();
            Assert.Equal(PinnedD20Seed42, rolls);
        }

        internal static readonly int[] PinnedD20Seed42 = { 4, 18, 5, 16, 16, 7, 6, 6, 15, 5 };

        [Fact]
        public void MatchesThePublishedPcg32Reference()
        {
            // First outputs of the PCG32 reference program (seed 42, stream 54).
            var dice = new Dice(42, 54);
            uint[] expected = { 0xa15c02b7, 0x7b47f409, 0xba1d3330, 0x83d2f293, 0xbfa4784b, 0xcbed606e };
            foreach (uint e in expected) Assert.Equal(e, dice.NextUInt());
        }

        [Fact]
        public void ADieNeedsAtLeastTwoSides()
        {
            var dice = new Dice(1);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => dice.Roll(1));
        }
    }
}
