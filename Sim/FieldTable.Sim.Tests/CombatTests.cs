using System.Linq;
using Xunit;

namespace FieldTable.Sim.Tests
{
    public class CombatTests
    {
        [Fact]
        public void TwoSidesInTheOpenTradeFireAndSomeoneGetsHit()
        {
            var s = new Scene(seed: 4);
            var m = s.Add("reyes", "rf", s.Blue, 100, 300);
            var e = s.Add("pkm", "en-pkm", s.Red, 250, 300);
            for (int t = 0; t < 4; t++) s.Sim.RunTurn();
            Assert.NotEmpty(s.Events<ShotFired>().Where(x => x.Shooter == m));
            Assert.NotEmpty(s.Events<ShotFired>().Where(x => x.Shooter == e));
            Assert.True(m.Ammo < 210);
            Assert.True(m.Condition != Condition.Ok || e.Condition != Condition.Ok);
            Assert.NotEmpty(s.Events<DiceRolled>());
        }

        [Fact]
        public void CoverAndLyingDownMakeYouHarderToHit()
        {
            var s = new Scene(paint: map => map.FillRect(240, 290, 20, 20, "building"));
            var shooter = s.Add("m", "rf", s.Blue, 100, 300);
            var open = s.Add("a", "en-rf", s.Red, 250, 400);
            var inside = s.Add("b", "en-rf", s.Red, 250, 300, posture: Posture.Kneeling);
            var gun = Scene.Data.FindWeapon("m27");
            float pOpen = s.Sim.HitChance(shooter, open, gun);
            float pInside = s.Sim.HitChance(shooter, inside, gun);
            Assert.True(pInside < pOpen * 0.5f);
            open.Posture = Posture.Prone;
            Assert.True(s.Sim.HitChance(shooter, open, gun) < pOpen);
        }

        [Fact]
        public void CloserIsEasierToHit()
        {
            var s = new Scene();
            var shooter = s.Add("m", "rf", s.Blue, 100, 300);
            var near = s.Add("a", "en-rf", s.Red, 150, 300);
            var far = s.Add("b", "en-rf", s.Red, 500, 300);
            var gun = Scene.Data.FindWeapon("m27");
            Assert.True(s.Sim.HitChance(shooter, near, gun) > 2f * s.Sim.HitChance(shooter, far, gun));
        }

        [Fact]
        public void BeingShotAtPinsYouDownAndItFades()
        {
            // Past the rifle's reach but inside the machine gun's, so the duel can't end it early.
            var s = new Scene(seed: 2);
            var m = s.Add("m", "rf", s.Blue, 100, 300);
            var e = s.Add("pkm", "en-pkm", s.Red, 750, 300);
            m.Order = null;
            s.Sim.RunTurn();
            Assert.True(m.Suppression > 0.2f || m.Condition != Condition.Ok);
            s.Sim.RemoveUnitForTest(e);
            s.Sim.RunTurn();
            Assert.True(m.Suppression < 0.05f);
        }

        [Fact]
        public void NobodyShootsCiviliansOnTheirOwn()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 300);
            s.Add("farmer", "civ", s.Civ, 150, 300);
            for (int t = 0; t < 2; t++) s.Sim.RunTurn();
            Assert.Empty(s.Events<ShotFired>());
        }

        [Fact]
        public void OutOfRangeMeansNoShooting()
        {
            var s = new Scene();
            s.Add("m", "doc", s.Blue, 50, 300);
            s.Add("e", "en-rf", s.Red, 750, 300);
            s.Sim.RunTurn();
            Assert.Empty(s.Events<ShotFired>());
        }

        [Fact]
        public void SomeoneDownDiesWithoutTreatmentAndLivesWithDoc()
        {
            var s = new Scene();
            var hurt = s.Add("lund", "rf", s.Blue, 100, 300);
            var alone = s.Add("kim", "rf", s.Blue, 100, 500);
            var doc = s.Add("doc", "doc", s.Blue, 150, 300);
            doc.Callsigns.Add("Doc");
            s.Sim.KnockDownForTest(hurt);
            s.Sim.KnockDownForTest(alone);
            Assert.True(s.Order(doc, Verbs.TreatCasualty, target: "lund").Ok);
            for (int t = 0; t < 16; t++) s.Sim.RunTurn();
            Assert.Equal(Condition.Down, hurt.Condition);
            Assert.True(hurt.Stabilized);
            Assert.Equal(Condition.Dead, alone.Condition);
            Assert.Contains(s.Events<UnitDied>(), e => e.Unit == alone && e.Cause == "died of wounds");
            Assert.Contains(s.Events<RadioCall>(), e => e.Text == "Doc: lund is stable.");
        }

        [Fact]
        public void AGrenadeHitsWhoeverIsNearIncludingFriends()
        {
            var s = new Scene(seed: 9);
            var thrower = s.Add("tl", "tl", s.Blue, 100, 300);
            var friend = s.Add("too-close", "rf", s.Blue, 121, 300);
            var enemy = s.Add("e", "en-rf", s.Red, 121, 302);
            Assert.True(s.Order(thrower, Verbs.ThrowGrenade, "too-close").Ok);
            thrower.Order.Point = new Vec2(121, 301);
            s.Sim.RunTurn();
            Assert.Single(s.Events<BlastAt>());
            Assert.Contains(s.Events<DiceRolled>(), d => d.Actor == "too-close" && d.Reason.StartsWith("caught in the blast"));
            Assert.Contains(s.Events<DiceRolled>(), d => d.Actor == "e" && d.Reason.StartsWith("caught in the blast"));
            Assert.Equal(1, thrower.Grenades);
        }

        [Fact]
        public void FiringGivesTheShooterAway()
        {
            // Prone in reeds 400 m off, he'd take over a minute to spot. Once he opens up, he's given away.
            var s = new Scene(seed: 3, paint: map => map.FillRect(480, 250, 40, 100, "marsh"));
            s.Add("m", "rf", s.Blue, 100, 300);
            s.Add("e", "en-rf", s.Red, 485, 300, posture: Posture.Prone);
            s.Sim.RunTurn();
            Assert.NotNull(s.Sim.Fog.For(s.Blue).About("e"));
            Assert.Contains(s.Events<RadioCall>(), r => r.Text.Contains("Taking fire!"));
        }

        [Fact]
        public void TheEnemyPullsBackWhenHalfOfThemAreDown()
        {
            var s = new Scene(seed: 5);
            s.Add("m", "rf", s.Blue, 100, 300);
            var a = s.Add("a", "en-rf", s.Red, 500, 300);
            var b = s.Add("b", "en-rf", s.Red, 500, 320);
            s.Sim.KnockDownForTest(b);
            s.Sim.LookAround();
            s.Sim.RunTurn();
            Assert.True(a.Position.X > 500f || a.Condition != Condition.Ok);
        }

        [Fact]
        public void TheSameSeedPlaysTheSameFight()
        {
            string Play()
            {
                var s = new Scene(seed: 21);
                s.Add("m", "ar", s.Blue, 100, 300);
                s.Add("n", "rf", s.Blue, 100, 320);
                s.Add("e", "en-pkm", s.Red, 300, 300);
                s.Add("f", "en-rf", s.Red, 300, 330);
                for (int t = 0; t < 3; t++) s.Sim.RunTurn();
                return string.Join("\n", s.Sim.Log.Events.Select(x => x.Step + " " + x.Describe()));
            }
            Assert.Equal(Play(), Play());
        }
    }
}
