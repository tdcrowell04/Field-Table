using System.Linq;
using Xunit;

namespace FieldTable.Sim.Tests
{
    public class WoundTests
    {
        static WeaponDef Rifle => Scene.Data.FindWeapon("ak");

        [Fact]
        public void AHelmetNowAndThenStopsAHeadShot()
        {
            var s = new Scene();
            var m = s.Add("reyes", "rf", s.Blue, 100, 300);
            s.Sim.HitIn(m, Rifle, 1, stopped: true);
            Assert.Equal(Condition.Ok, m.Condition);
            Assert.Equal(1f, m.Suppression);
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "reyes: Helmet took it! I'm good.");
            s.Sim.HitIn(m, Rifle, 2, stopped: false);
            Assert.Equal(Condition.Dead, m.Condition);
        }

        [Fact]
        public void PlatesStopChestHitsButNotBellyHits()
        {
            var s = new Scene();
            var a = s.Add("a", "rf", s.Blue, 100, 300);
            var b = s.Add("b", "rf", s.Blue, 100, 320);
            var c = s.Add("c", "rf", s.Blue, 100, 340);
            s.Sim.HitIn(a, Rifle, 5, stopped: true);
            s.Sim.HitIn(b, Rifle, 5, stopped: false);
            s.Sim.HitIn(c, Rifle, 11, stopped: false);
            Assert.Equal(Condition.Ok, a.Condition);
            Assert.Equal(Condition.Down, b.Condition);
            Assert.Equal(Condition.Down, c.Condition);
            Assert.Equal(Simulation.MinutesToBleedOutTorso, b.BleedMinutes);
            Assert.Equal(Simulation.MinutesToBleedOutTorso, c.BleedMinutes);
        }

        [Fact]
        public void TheEnemyHasNoHelmetOrPlates()
        {
            Assert.False(Scene.Data.FindUnit("en-rf").Helmet);
            Assert.False(Scene.Data.FindUnit("en-rf").Plate);
            Assert.True(Scene.Data.FindUnit("rf").Helmet);
            Assert.True(Scene.Data.FindUnit("doc").Plate);
        }

        [Fact]
        public void AChestWoundBleedsOutInTenMinutes()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 300);
            s.Sim.HitIn(m, Rifle, 6, stopped: false);
            for (int t = 0; t < 9; t++) s.Sim.RunTurn();
            Assert.Equal(Condition.Down, m.Condition);
            s.Sim.RunTurn();
            s.Sim.RunTurn();
            Assert.Equal(Condition.Dead, m.Condition);
        }

        [Fact]
        public void AnArmWoundHalvesHisAim()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 300);
            var e = s.Add("e", "en-rf", s.Red, 300, 300);
            float before = s.Sim.HitChance(m, e, Scene.Data.FindWeapon("m27"));
            s.Sim.HitIn(m, Rifle, 14, stopped: false);
            Assert.Equal(Condition.Wounded, m.Condition);
            Assert.True(m.ArmHit);
            Assert.Equal(before * 0.5f, s.Sim.HitChance(m, e, Scene.Data.FindWeapon("m27")), 3);
        }

        [Fact]
        public void ALegWoundMeansHeHasToBeCarried()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 300);
            s.Sim.HitIn(m, Rifle, 18, stopped: false);
            Assert.True(m.LegHit);
            var check = s.Order(m, Verbs.Move, "G4");
            Assert.False(check.Ok);
            Assert.Equal("m: Can't walk, Lead. I need to be carried.", check.Problem);
            Assert.True(s.Order(m, Verbs.Fire, "G4").Ok);
        }

        [Fact]
        public void HitAgainWhileWoundedPutsHimDownAndHitInTheChestWhileDownKillsHim()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 300);
            s.Sim.HitIn(m, Rifle, 14, stopped: false);
            s.Sim.HitIn(m, Rifle, 18, stopped: false);
            Assert.Equal(Condition.Down, m.Condition);
            Assert.Equal(Simulation.MinutesToBleedOut, m.BleedMinutes);
            s.Sim.HitIn(m, Rifle, 15, stopped: false);
            Assert.Equal(Condition.Down, m.Condition);
            s.Sim.HitIn(m, Rifle, 4, stopped: false);
            Assert.Equal(Condition.Dead, m.Condition);
        }

        [Fact]
        public void AMachineGunHitRollsForHowManyRoundsHit()
        {
            for (ulong seed = 1; seed <= 20; seed++)
            {
                var s = new Scene(seed);
                s.Add("m", "rf", s.Blue, 100, 300);
                s.Add("pkm", "en-pkm", s.Red, 160, 300);
                s.Sim.RunTurn();
                var rounds = s.Events<DiceRolled>().FirstOrDefault(r => r.Reason == "rounds that hit m");
                if (rounds == null) continue;
                Assert.Equal(10, rounds.Sides);
                Assert.True(s.Events<DiceRolled>().Count(r => r.Actor == "m" && r.Reason == "where the hit lands") >= 1);
                return;
            }
            Assert.Fail("The machine gun never hit in 20 tries.");
        }

        [Fact]
        public void AnHEBlastIsStoppedByAWallButAPOneGoesThrough()
        {
            var s = new Scene(paint: m => m.FillRect(300, 200, 1, 200, "wall"));
            var tl = s.Add("tl", "tl", s.Blue, 100, 300);
            var open = s.Add("open", "en-rf", s.Red, 298, 260);
            var behind = s.Add("behind", "en-rf", s.Red, 302, 300);
            var m320 = Scene.Data.FindWeapon("m320");
            s.Sim.Explode(tl, m320, new Vec2(298, 299), ap: false);
            var rolls = s.Events<DiceRolled>().Where(r => r.Reason.StartsWith("caught in the blast")).Select(r => r.Actor).ToList();
            Assert.DoesNotContain("behind", rolls);
            Assert.DoesNotContain("open", rolls);

            var s2 = new Scene(paint: m => m.FillRect(300, 200, 1, 200, "wall"));
            var tl2 = s2.Add("tl", "tl", s2.Blue, 100, 300);
            s2.Add("behind", "en-rf", s2.Red, 301, 300);
            s2.Add("far", "en-rf", s2.Red, 296, 300);
            s2.Sim.Explode(tl2, m320, new Vec2(299.5f, 300), ap: true);
            var apRolls = s2.Events<DiceRolled>().Where(r => r.Reason.StartsWith("caught in the blast")).Select(r => r.Actor).ToList();
            Assert.Contains("behind", apRolls);
            Assert.DoesNotContain("far", apRolls);
            Assert.True(s2.Events<BlastAt>().Single().Ap);
            Assert.Equal(2f, s2.Events<BlastAt>().Single().Radius);
        }

        [Fact]
        public void AnHEBlastInTheOpenReachesEveryoneClose()
        {
            var s = new Scene();
            var tl = s.Add("tl", "tl", s.Blue, 100, 300);
            s.Add("a", "en-rf", s.Red, 300, 302);
            s.Add("b", "en-rf", s.Red, 303, 300);
            s.Sim.Explode(tl, Scene.Data.FindWeapon("m320"), new Vec2(300, 300), ap: false);
            var rolls = s.Events<DiceRolled>().Where(r => r.Reason.StartsWith("caught in the blast")).Select(r => r.Actor).ToList();
            Assert.Contains("a", rolls);
            Assert.Contains("b", rolls);
        }

        [Fact]
        public void AskingForAPFiresOneAPRound()
        {
            var s = new Scene(paint: m => m.FillRect(195, 290, 12, 10, "building"));
            var tl = s.Add("tl", "tl", s.Blue, 100, 295);
            s.Add("e", "en-rf", s.Red, 200, 295);
            for (int i = 0; i < 5; i++) s.Sim.LookAround();
            Assert.True(s.Sim.Give(tl, new Order { Verb = Verbs.Fire, Target = "e", Kind = "AP" }).Ok);
            s.Sim.RunTurn();
            Assert.Equal(2, tl.LauncherAp);
            Assert.Contains(s.Events<ShotFired>(), e => e.Shooter == tl && e.Weapon == "m320-ap");
        }

        [Fact]
        public void SmokeBlindsEyesButNotThermals()
        {
            var s = new Scene();
            var rf = s.Add("rf", "rf", s.Blue, 100, 300);
            var tl = s.Add("tl", "tl", s.Blue, 100, 320);
            var e = s.Add("e", "en-rf", s.Red, 400, 310);
            Assert.True(FogOfWar.SpotRate(s.Map, rf, e) > 0f);
            s.Map.Smoke.Add(new SmokeCloud { Center = new Vec2(250, 310), Radius = 15f, Height = 8f, UntilStep = 9999 });
            var sight = Sight.Between(s.Map, rf, e);
            Assert.True(sight.Smoke);
            Assert.False(sight.SeenBy(rf));
            Assert.True(sight.SeenBy(tl));
            Assert.Equal(0f, FogOfWar.SpotRate(s.Map, rf, e));
            Assert.True(FogOfWar.SpotRate(s.Map, tl, e) > 0f);
        }

        [Fact]
        public void OneSmokeIsEnoughForATeam()
        {
            var s = new Scene();
            var team = new[] { s.Add("a", "tl", s.Blue, 100, 300, "Alpha"), s.Add("b", "ar", s.Blue, 100, 306, "Alpha"), s.Add("c", "rf", s.Blue, 100, 312, "Alpha") };
            s.Sim.GiveOrder(s.Blue, "Alpha", new Order { Verb = Verbs.ThrowSmoke });
            s.Sim.RunTurn();
            Assert.Single(s.Map.Smoke);
            Assert.Equal(2, team.Sum(u => u.Smokes));
        }

        [Fact]
        public void SomeoneDownDoesNotSayTheyAreGood()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 300);
            s.Sim.KnockDownForTest(m);
            s.Sim.HitIn(m, Rifle, 5, stopped: true);
            Assert.Equal(Condition.Down, m.Condition);
            Assert.DoesNotContain(s.Events<RadioCall>(), r => r.Text.Contains("I'm good"));
        }

        [Fact]
        public void ThrownSmokeHangsForAMinuteAndAHalf()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 100, 300);
            Assert.True(s.Sim.Give(m, new Order { Verb = Verbs.ThrowSmoke, Where = "B4" }).Ok == false);
            var check = s.Sim.Give(m, new Order { Verb = Verbs.ThrowSmoke });
            Assert.True(check.Ok);
            s.Sim.RunTurn();
            Assert.Equal(0, m.Smokes);
            Assert.Single(s.Map.Smoke);
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "m: Smoke out.");
            s.Sim.RunTurn();
            Assert.Empty(s.Map.Smoke);
        }
    }
}
