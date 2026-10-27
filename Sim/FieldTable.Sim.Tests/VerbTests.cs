using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class VerbTests
    {
        static Unit[] Team(Scene s, string name, float x, float y) => new[]
        {
            s.Add(name.ToLowerInvariant() + "-tl", "tl", s.Blue, x, y, name),
            s.Add(name.ToLowerInvariant() + "-ar", "ar", s.Blue, x, y + 6, name),
            s.Add(name.ToLowerInvariant() + "-aar", "aar", s.Blue, x, y + 12, name),
            s.Add(name.ToLowerInvariant() + "-rf", "rf", s.Blue, x, y + 18, name),
        };

        static void Building(MapGrid m, string label, int x, int y, int w, int h)
        {
            m.FillRect(x, y, w, h, "building");
            m.AddLandmark(new Landmark(label, "building", null, null, new Vec2(x + w / 2f, y + h / 2f), new Vec2(x, y), new Vec2(x + w, y + h), null));
        }

        [Fact]
        public void BoundingHalvesTakeTurnsAndTheTeamReportsSet()
        {
            var s = new Scene();
            s.Sim.RecordFrames(s.Blue);
            var a = Team(s, "Alpha", 100, 300);
            var checks = s.Sim.GiveOrder(s.Blue, "Alpha", new Order { Verb = "Bound", Where = "E4" });
            Assert.All(checks, c => Assert.True(c.Ok));
            s.Sim.RunTurn();
            // Somewhere in the turn one half rushes while the other half is still.
            bool turns = s.Sim.Frames.Any(f =>
            {
                var moving = f.Units.Where(u => u.Moving).Select(u => u.Id).ToList();
                return moving.Count == 2 && (moving.SequenceEqual(new[] { "alpha-tl", "alpha-aar" }) || moving.SequenceEqual(new[] { "alpha-ar", "alpha-rf" }));
            });
            Assert.True(turns);
            for (int t = 0; t < 6 && a.Any(u => u.Order != null); t++) s.Sim.RunTurn();
            Assert.All(a, u => Assert.Null(u.Order));
            Assert.All(a, u => Assert.Equal("E4", s.Map.SquareOf(u.Position).ToString()));
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "Alpha: Set at E4.");
        }

        [Fact]
        public void OverwatchShootsWhatIsInTheWatchedAreaFirst()
        {
            var s = new Scene();
            var m = s.Add("m", "ar", s.Blue, 100, 300);
            var near = s.Add("near", "en-rf", s.Red, 160, 380);
            var far = s.Add("far", "en-rf", s.Red, 420, 300);
            near.Ammo = far.Ammo = 0;
            for (int i = 0; i < 10; i++) s.Sim.LookAround();
            Assert.True(s.Sim.Give(m, new Order { Verb = Verbs.Overwatch, Target = "E4" }).Ok);
            s.Sim.RunTurn();
            var first = s.Events<ShotFired>().First(e => e.Shooter == m);
            Assert.Equal(far.Id, s.Sim.Units.First(u => Vec2.Distance(u.Position, first.To) < 30f && u.Force == s.Red).Id);
            Assert.NotNull(m.Order);
        }

        [Fact]
        public void ClearingABuildingFindsWhoIsInsideAndReportsClear()
        {
            var s = new Scene(paint: m => Building(m, "building 1", 300, 290, 10, 8));
            var a = Team(s, "Alpha", 260, 280);
            var civ = s.Add("farmer", "civ", s.Civ, 305, 294);
            Assert.Null(s.Sim.Fog.For(s.Blue).About(civ.Id));
            Assert.All(s.Sim.GiveOrder(s.Blue, "Alpha", new Order { Verb = Verbs.Clear, Where = "building 1" }), c => Assert.True(c.Ok));
            for (int t = 0; t < 3 && a.Any(u => u.Order != null); t++) s.Sim.RunTurn();
            Assert.NotNull(s.Sim.Fog.For(s.Blue).About(civ.Id));
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "Alpha: Building 1 clear.");
        }

        [Fact]
        public void ABreachOpensAGapInAWall()
        {
            var s = new Scene(paint: m => m.FillRect(300, 200, 1, 200, "wall"));
            var m = s.Add("m", "rf", s.Blue, 280, 300);
            Assert.True(s.Order(m, Verbs.Breach, "D4").Ok);
            Assert.Equal("wall", s.Map.TerrainAt(new Vec2(300.5f, 300.5f)).Id);
            s.Sim.RunTurn();
            Assert.Equal("rubble", s.Map.TerrainAt(new Vec2(300.5f, 300.5f)).Id);
            Assert.Contains(s.Events<BreachMade>(), b => b.Unit == m);
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "m: Breach is open.");
        }

        [Fact]
        public void TheDroneFliesOutSeesOverAWallAndComesHome()
        {
            var s = new Scene(paint: m => m.FillRect(300, 0, 2, 600, "wall"));
            var op = s.Add("op", "dr", s.Blue, 100, 300, "Charlie");
            var drone = s.Add("drone", "quadcopter", s.Blue, 100, 300);
            drone.Operator = op;
            drone.Gone = true;
            var e = s.Add("e", "en-rf", s.Red, 400, 300, posture: Posture.Prone);
            Assert.Null(s.Sim.Fog.For(s.Blue).About(e.Id));
            Assert.True(s.Order(op, Verbs.FlyDrone, "D4").Ok);
            s.Sim.RunTurn();
            Assert.False(drone.Gone);
            Assert.True(Vec2.Distance(drone.Position, s.Map.SquareCenter(new GridRef(3, 3))) < 1f);
            Assert.NotNull(s.Sim.Fog.For(s.Blue).About(e.Id));
            Assert.NotNull(op.Order);
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "op: Drone's up.");
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "Charlie: Contact on the drone, enemy rifleman at E4.");
            Assert.True(s.Sim.Give(op, new Order { Verb = Verbs.FlyDrone, Where = "back" }).Ok);
            s.Sim.RunTurn();
            Assert.True(drone.Gone);
            Assert.Null(op.Order);
            Assert.Equal(14, s.Sim.UnitsCalled(s.Blue, "squad").Count + 13);
        }

        [Fact]
        public void TalkingToACivilianTellsTheSquadWhatTheySaw()
        {
            // A building blocks the Marine's view of the gun, but not the farmer's.
            var s = new Scene(paint: m => m.FillRect(260, 285, 20, 30, "building"));
            var m = s.Add("m", "rf", s.Blue, 200, 300);
            var civ = s.Add("farmer", "civ", s.Civ, 230, 340);
            var e = s.Add("e", "en-pkm", s.Red, 500, 300);
            e.Ammo = 0;
            for (int i = 0; i < 10; i++) s.Sim.LookAround();
            Assert.Null(s.Sim.Fog.For(s.Blue).About(e.Id));
            Assert.NotNull(s.Sim.Fog.For(s.Civ).About(e.Id));
            // The gun has since moved off; only the farmer knows where it was.
            s.Sim.RemoveUnitForTest(e);
            Assert.True(s.Sim.Give(m, new Order { Verb = Verbs.Talk, Target = "farmer" }).Ok);
            s.Sim.RunTurn();
            Assert.NotNull(s.Sim.Fog.For(s.Blue).About(e.Id));
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "m: farmer says they saw a machine gunner at F4.");
        }

        [Fact]
        public void ACarefulSearchFindsSomeoneHidingClose()
        {
            var s = new Scene(paint: m => m.FillRect(280, 280, 40, 40, "marsh"));
            var m = s.Add("m", "rf", s.Blue, 250, 300);
            var e = s.Add("e", "en-rf", s.Red, 285, 300, posture: Posture.Prone);
            e.Ammo = 0;
            Assert.Null(s.Sim.Fog.For(s.Blue).About(e.Id));
            Assert.True(s.Sim.Give(m, new Order { Verb = Verbs.Search }).Ok);
            s.Sim.RunTurn();
            Assert.NotNull(s.Sim.Fog.For(s.Blue).About(e.Id));
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "m: Search done. Found a rifleman at C4.");
        }

        [Fact]
        public void SearchingADownedEnemyTakesTheirWeapon()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 250, 300);
            var e = s.Add("e", "en-rf", s.Red, 270, 300);
            s.Sim.KnockDownForTest(e);
            for (int i = 0; i < 3; i++) s.Sim.LookAround();
            Assert.True(s.Sim.Give(m, new Order { Verb = Verbs.Search, Target = "e" }).Ok);
            s.Sim.RunTurn();
            Assert.Equal(0, e.Ammo);
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "m: Searched e. Took their AK rifle and ammo.");
        }

        [Fact]
        public void TwoMarinesCarryACasualtyFasterThanOne()
        {
            float Carried(int carriers)
            {
                var s = new Scene();
                var cas = s.Add("cas", "rf", s.Blue, 100, 300);
                s.Sim.KnockDownForTest(cas);
                for (int i = 0; i < carriers; i++)
                {
                    var c = s.Add("c" + i, "rf", s.Blue, 101 + i, 302);
                    Assert.True(s.Sim.Give(c, new Order { Verb = Verbs.Carry, Target = "cas", Where = "G4" }).Ok);
                }
                s.Sim.RunTurn();
                Assert.NotNull(cas.CarriedBy);
                return cas.Position.X - 100;
            }
            float alone = Carried(1), together = Carried(2);
            Assert.True(together > alone * 1.6f, "alone " + alone + " m, together " + together + " m");
        }

        [Fact]
        public void ACarriedCasualtyIsSetDownWhereTheyWereTold()
        {
            var s = new Scene();
            var cas = s.Add("cas", "rf", s.Blue, 100, 350);
            s.Sim.KnockDownForTest(cas);
            var a = s.Add("a", "rf", s.Blue, 105, 350);
            var b = s.Add("b", "rf", s.Blue, 105, 352);
            s.Sim.Give(a, new Order { Verb = Verbs.Carry, Target = "cas", Where = "B4" });
            s.Sim.Give(b, new Order { Verb = Verbs.Carry, Target = "cas", Where = "B4" });
            for (int t = 0; t < 4 && (a.Order != null || b.Order != null); t++) s.Sim.RunTurn();
            Assert.Null(cas.CarriedBy);
            Assert.Equal("B4", s.Map.SquareOf(cas.Position).ToString());
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "a: cas is at B4.");
        }

        [Fact]
        public void DetainingACivilianWorksButNotSomeoneStillFighting()
        {
            var s = new Scene();
            var m = s.Add("m", "rf", s.Blue, 250, 300);
            var civ = s.Add("farmer", "civ", s.Civ, 270, 300);
            var e = s.Add("gunman", "en-rf", s.Red, 270, 330);
            e.Ammo = 0;
            for (int i = 0; i < 3; i++) s.Sim.LookAround();
            var no = s.Sim.Give(m, new Order { Verb = Verbs.Detain, Target = "gunman" });
            Assert.False(no.Ok);
            Assert.Equal("gunman is still fighting, Lead.", no.Problem);
            Assert.True(s.Sim.Give(m, new Order { Verb = Verbs.Detain, Target = "farmer" }).Ok);
            s.Sim.RunTurn();
            Assert.True(civ.Detained);
            Assert.False(civ.CanAct);
            Assert.Contains(s.Events<UnitDetained>(), d => d.Unit == civ);
        }

        [Fact]
        public void SharingAmmoEvensOutTheTeam()
        {
            var s = new Scene();
            var a = Team(s, "Alpha", 100, 300);
            a[0].Ammo = 30;
            a[1].Ammo = 660;
            a[2].Ammo = 420;
            a[3].Ammo = 30;
            s.Sim.GiveOrder(s.Blue, "Alpha", new Order { Verb = Verbs.ShareAmmo });
            s.Sim.RunTurn();
            float[] fraction = a.Select(u => u.Ammo / (float)u.Def.Ammo).ToArray();
            Assert.All(fraction, f => Assert.InRange(f, fraction[0] - 0.01f, fraction[0] + 0.01f));
            Assert.Contains(s.Events<RadioCall>(), r => r.Text.StartsWith("Alpha: Ammo's spread out."));
        }

        [Fact]
        public void CasevacTakesTheCasualtiesAtTheLandingZone()
        {
            var s = new Scene();
            var lead = s.Add("lead", "sl", s.Blue, 340, 340);
            var doc = s.Add("doc", "doc", s.Blue, 345, 340);
            var hurt = new[] { s.Add("h1", "rf", s.Blue, 350, 345), s.Add("h2", "rf", s.Blue, 352, 350) };
            foreach (var h in hurt) s.Sim.KnockDownForTest(h);
            var leg = s.Add("leg", "rf", s.Blue, 355, 352);
            s.Sim.HitIn(leg, Scene.Data.FindWeapon("ak"), 18, false);
            Assert.True(s.Sim.Give(lead, new Order { Verb = Verbs.Request, Target = "CASEVAC", Where = "D4" }).Ok);
            s.Sim.RunTurn();
            var nine = s.Events<RadioCall>().First(r => r.Text.StartsWith("Doc: CASEVAC 9-line."));
            Assert.Contains("1: LZ D4.", nine.Text);
            Assert.Contains("3: 2 urgent, 0 priority, 1 routine.", nine.Text);
            Assert.Contains("5: 3 litter, 0 ambulatory.", nine.Text);
            Assert.NotNull(s.Sim.Evac);
            Assert.False(s.Sim.Give(lead, new Order { Verb = Verbs.Request, Target = "CASEVAC", Where = "D4" }).Ok);
            // Doc keeps them alive until the bird comes.
            foreach (var h in hurt) h.Stabilized = true;
            for (int t = 0; t < Simulation.CasevacMaxMinutes + 5 && s.Sim.Evac != null; t++) s.Sim.RunTurn();
            Assert.Null(s.Sim.Evac);
            Assert.All(hurt.Append(leg), h => Assert.True(h.Evacuated && h.Gone));
            Assert.False(lead.Gone);
            Assert.Contains(s.Events<RadioCall>(), r => r.Text == "Dustoff: Leaving with 3 casualties.");
        }

        [Fact]
        public void ARequestForSomethingElseIsNotCoveredYet()
        {
            var s = new Scene();
            var lead = s.Add("lead", "sl", s.Blue, 300, 300);
            var check = s.Sim.Give(lead, new Order { Verb = Verbs.Request, Kind = "QRF" });
            Assert.False(check.Ok);
            Assert.StartsWith("not covered", check.Problem);
        }

        [Fact]
        public void TheEnemyIsADifferentMixEachMission()
        {
            var names = NamePools.FromJson(Read("names.json"));
            bool noPkm = false, noRpg = false;
            for (ulong seed = 1; seed <= 20; seed++)
            {
                var m = BridgeSeizure.Create(Scene.Data, names, seed);
                var enemy = m.Sim.Units.Where(u => u.Force == m.Enemy).ToList();
                Assert.InRange(enemy.Count, BridgeSeizure.MinEnemy, BridgeSeizure.MaxEnemy);
                Assert.Equal(enemy.Count, enemy.Select(u => u.Name).Distinct().Count());
                noPkm |= enemy.All(u => u.Def.Id != "en-pkm");
                noRpg |= enemy.All(u => u.Def.Id != "en-rpg");
            }
            Assert.True(noPkm || noRpg);
        }
    }
}
