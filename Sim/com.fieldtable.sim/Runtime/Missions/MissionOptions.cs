using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    /// <summary>
    /// Custom numbers for a mission (ledger round 18, item 6): Triston's option to adjust a mission's numbers to make
    /// it play custom. Anything left null keeps the mission's own numbers. Custom play also lets any side play any
    /// mission, whether or not it fits.
    /// </summary>
    public sealed class MissionOptions
    {
        /// <summary>How many enemy soldiers are in the mission, on the map and still to come.</summary>
        public int? Enemy { get; set; }

        /// <summary>The turn time runs out, or, where the mission's end is a turn, that turn.</summary>
        public int? TurnLimit { get; set; }

        /// <summary>How many down or dead before the group can't fight on.</summary>
        public int? LoseAt { get; set; }

        /// <summary>The turn something arrives that a die would otherwise pick: relief, the reaction force, the supply party.</summary>
        public int? ArrivalTurn { get; set; }

        public bool Any => Enemy != null || TurnLimit != null || LoseAt != null || ArrivalTurn != null;

        /// <summary>The enemy's number, once the mission is made: added to or cut down to what was asked.</summary>
        internal void ApplyAfter(Mission m, GameData data)
        {
            if (TurnLimit is int turns && turns > 0 && turns != m.TurnLimit)
            {
                int old = m.TurnLimit;
                m.TurnLimit = turns;
                string Turn(string text) => (text ?? "").Replace("turn " + old + " ", "turn " + turns + " ").Replace("turn " + old + ".", "turn " + turns + ".").Replace("turn " + old + ",", "turn " + turns + ",");
                m.LoseText = Turn(m.LoseText);
                m.WinText = Turn(m.WinText);
                m.MainObjective = Turn(m.MainObjective);
                for (int i = 0; i < m.Brief.Count; i++) m.Brief[i] = Turn(m.Brief[i]);
            }
            if (LoseAt is int lose && lose > 0 && lose != m.LoseAt)
            {
                int old = m.LoseAt;
                m.LoseAt = lose;
                m.LoseText = (m.LoseText ?? "").Replace(old + " " + m.Side.Members + " down", lose + " " + m.Side.Members + " down");
            }
            if (Enemy is int want && want >= 0) ScaleEnemy(m, data, want);
            var set = new List<string>();
            if (Enemy is int e) set.Add(e + " enemy soldiers in all");
            if (TurnLimit is int t) set.Add(t + " turns");
            if (LoseAt is int l) set.Add(l + " " + m.Side.Members + " down to lose");
            if (ArrivalTurn is int a) set.Add("they arrive on turn " + a);
            if (set.Count > 0) m.Brief.Add("Custom numbers: " + string.Join(", ", set) + ".");
        }

        static void ScaleEnemy(Mission m, GameData data, int want)
        {
            var sim = m.Sim;
            var enemy = sim.Units.Where(u => ReferenceEquals(u.Force, m.Enemy) && !u.Def.Flying).ToList();
            if (want < enemy.Count)
            {
                // Cut from the back: plain soldiers first, never the leaders with the radios, the wanted man, the
                // engineer with the charges, or the porters with the crates.
                bool Plain(Unit u) => !u.Def.Radio && u.Def.Id != "en-hvt" && u.Def.Id != "en-eng" && u.Def.Id != "en-porter" && !ReferenceEquals(u, m.Enemy.Plan?.Engineer);
                var cut = Enumerable.Reverse(enemy).Where(Plain).Take(enemy.Count - want).ToList();
                if (cut.Count < enemy.Count - want) cut.AddRange(Enumerable.Reverse(enemy).Where(u => !cut.Contains(u) && u.Def.Id != "en-hvt").Take(enemy.Count - want - cut.Count));
                foreach (var u in cut) sim.RemoveFromTheFight(u);
            }
            else if (want > enemy.Count)
            {
                // More riflemen, beside the group that has the most already, sharing its place, team, and whether it's on the map yet.
                var rifle = data.FindUnit("en-rf");
                var template = enemy.Where(u => u.Def.Id == "en-rf").GroupBy(u => u.Team).OrderByDescending(g => g.Count()).FirstOrDefault()?.Last() ?? enemy.LastOrDefault();
                if (rifle == null || template == null) return;
                int n = enemy.Count(u => u.Name != null && u.Name.StartsWith("Rifleman ", StringComparison.Ordinal));
                for (int i = enemy.Count; i < want; i++)
                {
                    var at = MissionKit.Standable(sim.Map, template.Position + new Vec2(2f + (i % 3) * 2f, (i % 2) * 3f - 1.5f), sim.Units.Select(u => u.Position));
                    var u = new Unit("en-custom-" + (i + 1), "Rifleman " + (++n), rifle, m.Enemy, at) { Team = template.Team, Gone = template.Gone, Posture = template.Posture, Facing = template.Facing };
                    sim.AddUnit(u);
                }
            }
        }
    }
}
