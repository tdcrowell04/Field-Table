using System.Collections.Generic;
using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    /// <summary>What the replay of run 1 found (playthroughs.md, run 1b), kept fixed.</summary>
    public class ReplayFixesTests
    {
        // Boone "stalled" 115 m short of the post: the post's slope climbs about 70 m in 85 m, and he was walking up it
        // at the pace a climb like that takes. The preview now says how long a move will take, so it shows up front.
        [Fact]
        public void ASteepClimbShowsAsSlowBeforeAnyoneSetsOff()
        {
            var m = StartStates.Create(StartStates.Random, Scene.Data, NamePools.FromJson(Read("names.json")), 38545, null, "marines");
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy).ToList()) m.Sim.RemoveUnitForTest(e);
            var boone = m.Squad.First(u => u.Team == "Charlie");
            boone.Position = new Vec2(440f, 360f);
            var order = new Order { Verb = Verbs.Search, Where = "the post" };
            var c = m.Sim.Check(boone, order);
            Assert.True(c.Ok, c.Problem);
            float up = m.Sim.TravelSeconds(boone, order, c.Path);
            float flat = Pathfinder.Length(boone.Position, c.Path) / boone.Def.WalkSpeed;
            Assert.True(up > 2.5f * flat, "The climb should show as much slower: " + up + " s against " + flat + " s on the flat.");
            // And the estimate is about what it really takes.
            Assert.True(m.Sim.Give(boone, order).Ok);
            float t = 0f;
            while (boone.Order != null && boone.Path != null && t < 600f) { m.Sim.StepForTest(); t += 1f / SimClock.StepsPerSecond; }
            Assert.InRange(t, up * 0.7f, up * 1.3f);
        }
    }
}
