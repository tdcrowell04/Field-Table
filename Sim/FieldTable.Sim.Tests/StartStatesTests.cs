using System.Linq;
using Xunit;
using static FieldTable.Sim.Tests.TestData;

namespace FieldTable.Sim.Tests
{
    public class StartStatesTests
    {
        static readonly System.Lazy<NamePools> names = new System.Lazy<NamePools>(() => NamePools.FromJson(Read("names.json")));

        [Fact]
        public void SixMissionsEachWithAWayToWinAndLose()
        {
            Assert.Equal(new[] { "bridge", "raid", "crossroads", "crew", "clear", "ambush" }, StartStates.All.Select(s => s.Id));
            foreach (var state in StartStates.All)
            {
                var m = StartStates.Create(state.Id, Scene.Data, names.Value, 7);
                Assert.Equal(state.Id, m.Kind);
                Assert.False(string.IsNullOrEmpty(m.Title));
                Assert.True(m.Brief.Count >= 5, state.Id);
                Assert.False(string.IsNullOrEmpty(m.MainObjective));
                Assert.False(string.IsNullOrEmpty(m.WinText));
                Assert.False(string.IsNullOrEmpty(m.LoseText));
                Assert.NotNull(m.Objective);
                Assert.NotNull(m.Director);
                Assert.NotNull(m.Enemy.Plan);
                Assert.Equal("Every Marine comes home alive", m.Secondary.Last().Text);
                Assert.Equal(14, m.Squad.Count());
                // The enemy is an army (ledger round 17, (a)): every side has a squad leader with a radio, and names run in order.
                var enemy = m.Sim.Units.Where(u => u.Force == m.Enemy).ToList();
                Assert.Contains(enemy, u => u.Def.Id == "en-sl" && u.Def.Radio && u.Name.StartsWith("Squad leader "));
                foreach (var kind in enemy.Select(u => System.Text.RegularExpressions.Regex.Match(u.Name, @"^(.*) \d+$")).Where(x => x.Success).GroupBy(x => x.Groups[1].Value))
                    Assert.Equal(Enumerable.Range(1, kind.Count()).Select(n => kind.Key + " " + n), kind.Select(x => x.Value));
            }
        }

        [Fact]
        public void RandomPicksAMissionFromTheSeed()
        {
            var kinds = Enumerable.Range(1, 12).Select(i => StartStates.Create(StartStates.Random, Scene.Data, names.Value, (ulong)i).Kind).ToList();
            Assert.True(kinds.Distinct().Count() >= 3, string.Join(",", kinds));
            Assert.Equal(kinds[4], StartStates.Create(null, Scene.Data, names.Value, 5).Kind);
            Assert.Throws<System.ArgumentException>(() => StartStates.Create("nope", Scene.Data, names.Value, 1));
        }
    }
}
