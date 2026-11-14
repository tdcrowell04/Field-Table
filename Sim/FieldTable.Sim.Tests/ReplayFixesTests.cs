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

        // Run 2: "check the two wounded Kansians at F5" got "Nobody's hit, Lead." A place stands in for the name.
        [Fact]
        public void AMedicToldToCheckTheWoundedAtAPlaceFindsTheEnemyDownThere()
        {
            var s = new Scene();
            var doc = s.Add("doc", "doc", s.Blue, 100, 300, "HQ");
            var e = s.Add("e", "en-rf", s.Red, 150, 300);
            e.Ammo = 0;
            s.Sim.KnockDownForTest(e);
            s.Sim.Fog.Spot(s.Sim, s.Blue, e, doc);
            var c = s.Sim.Give(doc, new Order { Verb = Verbs.TreatCasualty, Where = s.Sim.Coordinates(new Vec2(155f, 305f)) });
            Assert.True(c.Ok, c.Problem);
            Assert.Same(e, doc.Order.TargetUnit);
            // And an enemy nobody knows about isn't found.
            var s2 = new Scene();
            var doc2 = s2.Add("doc", "doc", s2.Blue, 100, 300, "HQ");
            var e2 = s2.Add("e", "en-rf", s2.Red, 150, 300);
            s2.Sim.KnockDownForTest(e2);
            Assert.False(s2.Sim.Give(doc2, new Order { Verb = Verbs.TreatCasualty, Where = s2.Sim.Coordinates(new Vec2(155f, 305f)) }).Ok);
        }

        // Runs 2 and 3: a team order took the drone operator off the drone five times.
        [Fact]
        public void ATeamOrderLeavesTheDroneOperatorFlying()
        {
            var m = StartStates.Create(StartStates.Random, Scene.Data, NamePools.FromJson(Read("names.json")), 38545, null, "marines");
            var op = m.Squad.First(u => m.Sim.DroneOf(u) != null);
            Assert.True(m.Sim.Give(op, new Order { Verb = Verbs.FlyDrone, Where = "the post" }).Ok);
            for (int i = 0; i < 300; i++) m.Sim.StepForTest();
            var checks = m.Sim.GiveOrder(m.Marines, op.Team, new Order { Verb = Verbs.Move, Where = "C3" });
            Assert.DoesNotContain(checks, c => ReferenceEquals(c.Unit, op));
            Assert.Equal(Verbs.FlyDrone, op.Order.Verb);
            // Named himself, he does what he's told.
            Assert.True(m.Sim.Give(op, new Order { Verb = Verbs.Move, Where = "C3" }).Ok);
        }
    }
}
