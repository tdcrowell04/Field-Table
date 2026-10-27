using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class BridgeSeizureTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        static Mission Make(ulong seed) => BridgeSeizure.Create(Scene.Data, names.Value, seed);

        [Fact]
        public void SetsUpTheSquadTheEnemyAndCivilians()
        {
            for (ulong seed = 1; seed <= 12; seed++)
            {
                var m = Make(seed);
                var units = m.Sim.Units;
                Assert.Equal(14, m.Squad.Count());
                var drone = Assert.Single(units, u => u.Force == m.Marines && u.Def.Flying);
                Assert.True(drone.Gone);
                Assert.Equal("charlie-rf", drone.Operator.Id);
                Assert.InRange(units.Count(u => u.Force == m.Enemy), BridgeSeizure.MinEnemy, BridgeSeizure.MaxEnemy);
                Assert.InRange(units.Count(u => u.Force == m.Civilians), 1, 5);
                foreach (var u in units.Where(u => !u.Gone))
                {
                    var t = m.Sim.Map.TerrainAt(u.Position);
                    Assert.False(t.Deep, "Seed " + seed + ": " + u.Name + " starts in deep water.");
                    Assert.True(t.Passable);
                }
                foreach (var c in units.Where(u => u.Force == m.Civilians))
                    Assert.Equal("building", m.Sim.Map.TerrainAt(c.Position).Id);
                Assert.Equal(5, m.Brief.Count);
                Assert.Contains(m.Objective.Label, m.Brief[0]);
            }
        }

        [Fact]
        public void TheSquadAndTheEnemyStartOnOppositeBanks()
        {
            for (ulong seed = 1; seed <= 12; seed++)
            {
                var m = Make(seed);
                var map = m.Sim.Map;
                Vec2 b = m.Objective.Center;
                // A straight line from any Marine to any enemy crosses the water once (or an odd number of times, round bends).
                var water = map.Landmarks.Where(l => l.Kind == "line" && (l.Label.EndsWith(" River") || l.Label.EndsWith(" Creek")))
                    .OrderBy(l => l.Points.Zip(l.Points.Skip(1), (p, q) => MapGrid.DistanceToSegment(b, p, q)).Min()).First().Points;
                var lead = m.Sim.Units.First(u => u.Id == "lead");
                foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy))
                {
                    Assert.NotNull(m.Sim.Paths.Find(lead.Position, e.Position, false));
                    foreach (var u in m.Squad)
                        Assert.True(Crossings(u.Position, e.Position, water) % 2 == 1, "Seed " + seed + ": " + u.Id + " and " + e.Name + " start on the same bank.");
                }
                Assert.True(m.Sim.Units.Where(u => u.Force == m.Marines).All(u => Vec2.Distance(u.Position, b) > 30f), "Seed " + seed + ": a Marine starts on the bridge.");
            }
        }

        static int Crossings(Vec2 a, Vec2 b, System.Collections.Generic.IReadOnlyList<Vec2> line)
        {
            float Cross(Vec2 o, Vec2 p, Vec2 q) => (p.X - o.X) * (q.Y - o.Y) - (p.Y - o.Y) * (q.X - o.X);
            int n = 0;
            for (int i = 0; i + 1 < line.Count; i++)
            {
                Vec2 c = line[i], d = line[i + 1];
                if (Cross(a, b, c) * Cross(a, b, d) < 0f && Cross(c, d, a) * Cross(c, d, b) < 0f) n++;
            }
            return n;
        }

        [Fact]
        public void TheMapIsAKilometerSquare()
        {
            var m = Make(1);
            Assert.Equal(1000f, m.Sim.Map.WidthMeters);
            Assert.Equal(1000f, m.Sim.Map.HeightMeters);
        }

        [Fact]
        public void EveryoneKnowsTheirNames()
        {
            var m = Make(3);
            Assert.Equal(4, m.Sim.UnitsCalled(m.Marines, "Alpha").Count);
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "Doc"));
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "Lead"));
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "Charlie AR"));
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "drone operator"));
            // The whole squad, and the interpreter goes with it.
            Assert.Equal(15, m.Sim.UnitsCalled(m.Marines, "squad").Count);
            Assert.Single(m.Sim.UnitsCalled(m.Marines, "interpreter"));
            var name = m.Sim.Units.First(u => u.Id == "bravo-tl").Name;
            Assert.Single(m.Sim.UnitsCalled(m.Marines, name));
        }

        [Fact]
        public void TheSameSeedSetsUpTheSameMission()
        {
            string Setup(ulong seed)
            {
                var m = Make(seed);
                return m.Title + "|" + string.Join(";", m.Sim.Units.Select(u => u.Name + "@" + u.Position)) + "|" + string.Join(" ", m.Brief);
            }
            Assert.Equal(Setup(7), Setup(7));
            Assert.NotEqual(Setup(7), Setup(8));
        }

        [Fact]
        public void AFewTurnsPlayWithoutOrders()
        {
            var m = Make(5);
            for (int t = 0; t < 3; t++)
            {
                m.Sim.RunTurn();
                m.CheckOutcome();
            }
            Assert.Equal(4, m.Sim.Clock.Turn);
            Assert.NotEmpty(m.Sim.Frames);
            Assert.Null(m.Outcome);
        }

        [Fact]
        public void ATurnFinishesOnTheMapThatOnceFroze()
        {
            // Seed 55555 once left a step so small it rounded to nothing, and the turn never ended.
            var m = Make(55555);
            string b = m.Objective.Label;
            m.Sim.GiveOrder(m.Marines, "Alpha", new Order { Verb = Verbs.Move, Where = b, Speed = "run" });
            m.Sim.GiveOrder(m.Marines, "Bravo", new Order { Verb = Verbs.Move, Where = b });
            m.Sim.GiveOrder(m.Marines, "Charlie", new Order { Verb = Verbs.TakeCover });
            var turn = System.Threading.Tasks.Task.Run(() => m.Sim.RunTurn());
            Assert.True(turn.Wait(System.TimeSpan.FromSeconds(30)), "The turn never finished.");
            Assert.Equal(2, m.Sim.Clock.Turn);
        }

        [Fact]
        public void TheSecondaryObjectivesComeFromTheIntent()
        {
            var m = Make(3);
            Assert.Equal(new[] { "civilians", "bridge", "marines" }, m.Secondary.Select(o => o.Id));
            Assert.All(m.Secondary, o => Assert.False(o.IsBroken(m)));
            Assert.StartsWith("Seize and hold " + m.Objective.Label, m.MainObjective);
            Assert.Equal("Positive ID before engaging.", m.Roe);
        }

        [Fact]
        public void HurtingACivilianBreaksThatObjectiveForGood()
        {
            var m = Make(3);
            var civ = m.Sim.Units.First(u => u.Force == m.Civilians);
            m.Sim.KnockDownForTest(civ);
            civ.Condition = Condition.Ok;
            Assert.True(m.Secondary.First(o => o.Id == "civilians").IsBroken(m));
            Assert.False(m.Secondary.First(o => o.Id == "marines").IsBroken(m));
        }

        [Fact]
        public void AMarineKilledBreaksBringingEveryoneHome()
        {
            var m = Make(3);
            var marine = m.Sim.Units.First(u => u.Force == m.Marines);
            m.Sim.KnockDownForTest(marine);
            Assert.False(m.Secondary.First(o => o.Id == "marines").IsBroken(m));
            marine.Condition = Condition.Dead;
            Assert.True(m.Secondary.First(o => o.Id == "marines").IsBroken(m));
            Assert.False(m.Secondary.First(o => o.Id == "bridge").IsBroken(m));
        }

        [Fact]
        public void TheSquadUsuallyStartsOutOfSightAndSpreadOut()
        {
            int hidden = 0;
            for (ulong seed = 1; seed <= 20; seed++)
            {
                var m = Make(seed);
                var squad = m.Squad.ToList();
                foreach (var a in squad)
                    foreach (var b in squad)
                        if (a != b) Assert.True(Vec2.Distance(a.Position, b.Position) >= 2.5f, "Seed " + seed + ": " + a.Id + " and " + b.Id + " start on top of each other.");
                Assert.True(squad.All(u => Vec2.Distance(u.Position, m.Objective.Center) >= 120f), "Seed " + seed + ": the squad starts too close to the bridge.");
                var enemy = m.Sim.Units.Where(u => u.Force == m.Enemy).ToList();
                if (!squad.Any(u => enemy.Any(e => Sight.Trace(m.Sim.Map, e.Position, Postures.Height(e.Posture), u.Position, 1.7f).Clear && Vec2.Distance(e.Position, u.Position) <= FogOfWar.DayEyeRange))) hidden++;
            }
            Assert.True(hidden >= 19, "Only " + hidden + " of 20 maps start the squad out of sight.");
        }

        static Mission Won(ulong seed, System.Action<Mission> before = null)
        {
            var m = Make(seed);
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy).ToList()) m.Sim.RemoveUnitForTest(e);
            before?.Invoke(m);
            m.Sim.Units.First(u => u.Id == "lead").Position = m.Objective.Center;
            m.Sim.RunTurn(); m.CheckOutcome();
            m.Sim.RunTurn(); m.CheckOutcome();
            Assert.Equal("success", m.Outcome);
            return m;
        }

        [Fact]
        public void EverySecondaryObjectiveMetIsAnSPlus()
        {
            Assert.Equal("S+", Won(6).Rating);
        }

        [Fact]
        public void AHurtCivilianDropsTheRatingAndAKilledMarineMakesItAC()
        {
            Assert.Equal("S", Won(6, m => m.Sim.KnockDownForTest(m.Sim.Units.First(u => u.Force == m.Civilians))).Rating);
            Assert.Equal("C", Won(6, m => m.Sim.Units.First(u => u.Id == "alpha-rf").Condition = Condition.Dead).Rating);
        }

        [Fact]
        public void FriendlyFireDropsTheRatingAGrade()
        {
            var m = Won(6, x =>
            {
                var tl = x.Sim.Units.First(u => u.Id == "alpha-tl");
                var rf = x.Sim.Units.First(u => u.Id == "alpha-rf");
                x.Sim.Explode(tl, Scene.Data.FindWeapon("m320"), rf.Position, ap: false);
            });
            Assert.Equal(1, m.FriendlyFireHits);
            Assert.Contains(m.Sim.Log.Events.OfType<RadioCall>(), r => r.Text.Contains("Check fire! Check fire!"));
            Assert.Equal(m.Sim.Units.First(u => u.Id == "alpha-rf").Condition == Condition.Dead ? "F" : "S", m.Rating);
        }

        [Fact]
        public void EnoughFriendlyFireTakesAWinAllTheWayToF()
        {
            var m = Won(6, x =>
            {
                var tl = x.Sim.Units.First(u => u.Id == "alpha-tl");
                foreach (var id in new[] { "bravo-rf", "bravo-ar", "charlie-ar", "charlie-aar", "bravo-aar", "alpha-ar" })
                    x.Sim.Wound(x.Sim.Units.First(u => u.Id == id), Scene.Data.FindWeapon("m27"), tl);
            });
            Assert.Equal(6, m.FriendlyFireHits);
            Assert.Equal("F", m.Rating);
        }

        [Fact]
        public void ALossIsAnF()
        {
            var m = Make(6);
            foreach (var u in m.Squad.Take(7).ToList()) m.Sim.KnockDownForTest(u);
            m.CheckOutcome();
            Assert.Equal("F", m.Rating);
        }

        [Fact]
        public void TakingTheBridgeWithNoEnemyLeftWins()
        {
            var m = Make(6);
            foreach (var e in m.Sim.Units.Where(u => u.Force == m.Enemy).ToList()) m.Sim.RemoveUnitForTest(e);
            var lead = m.Sim.Units.First(u => u.Id == "lead");
            lead.Position = m.Objective.Center;
            m.CheckOutcome();
            Assert.Null(m.Outcome);
            m.Sim.RunTurn();
            m.CheckOutcome();
            Assert.Equal("success", m.Outcome);
        }

        [Fact]
        public void LosingSevenMarinesLoses()
        {
            var m = Make(6);
            foreach (var u in m.Squad.Take(7).ToList()) m.Sim.KnockDownForTest(u);
            m.CheckOutcome();
            Assert.Equal("failure", m.Outcome);
        }
    }
}
