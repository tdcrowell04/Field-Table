using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    /// <summary>One add-in the director could switch on now: its id, and what it is in plain words for the in-game AI.</summary>
    public sealed class AddInOption
    {
        public string Id { get; }
        public string What { get; }
        public AddInOption(string id, string what) { Id = id; What = what; }
    }

    /// <summary>
    /// The director (ledger round 14): it watches the mission and offers the add-ins that fit what's happening right
    /// now. The in-game AI picks which one, if any; the code sets the limits and makes it happen. No add-ins before
    /// turn 3, at most 3 a mission, each kind once, and at least 2 turns apart.
    /// </summary>
    public sealed class Director
    {
        public const int FirstTurn = 3, MostPerMission = 3, TurnsBetween = 2;

        public const string Reinforcements = "reinforcements", CiviliansRun = "civilians", Sniper = "sniper", Ied = "ied", PullBack = "pullback", RadioDown = "radio";

        readonly Mission m;
        readonly GameData data;
        readonly Dice rng;
        readonly IReadOnlyList<Vec2> water;
        readonly float enemySide;
        readonly IReadOnlyList<Vec2> road;
        readonly List<(string id, int turn)> used = new List<(string, int)>();

        /// <summary>The add-ins switched on so far, and the turn each came in.</summary>
        public IReadOnlyList<(string id, int turn)> Used => used;

        internal Director(Mission mission, GameData data, Dice rng, IReadOnlyList<Vec2> water, float enemySide, IReadOnlyList<Vec2> road)
        {
            m = mission; this.data = data; this.rng = rng; this.water = water; this.enemySide = enemySide; this.road = road;
        }

        Simulation Sim => m.Sim;
        Vec2 Bridge => m.Objective.Center;
        IEnumerable<Unit> Marines => m.Squad.Where(u => u.CanAct);
        bool OnEnemyBank(Vec2 p) => BridgeSeizure.Bank(water, p) * enemySide > 10f;

        /// <summary>The add-ins that fit what's happening now, within the limits. Empty when none do.</summary>
        public List<AddInOption> Options()
        {
            var list = new List<AddInOption>();
            int turn = Sim.Clock.Turn;
            if (m.Outcome != null || turn < FirstTurn || used.Count >= MostPerMission) return list;
            if (used.Count > 0 && turn - used[used.Count - 1].turn < TurnsBetween) return list;
            bool Done(string id) => used.Any(x => x.id == id);
            var marines = Marines.ToList();
            if (marines.Count == 0) return list;
            var enemy = Sim.Units.Where(u => ReferenceEquals(u.Force, m.Enemy) && !u.Gone).ToList();

            int enemyLost = enemy.Count(u => !u.CanAct);
            if (!Done(Reinforcements) && turn >= 8 && !m.MarineOnObjective && (turn >= 12 || enemyLost >= 2))
                list.Add(new AddInOption(Reinforcements, "Enemy reinforcements: 2 to 4 more fighters arrive on the far bank and head for the bridge. Fits because the squad is taking a while."));

            var lastTurn = Sim.Log.Events.Where(e => e.Turn == turn - 1).ToList();
            var landed = lastTurn.OfType<ShotFired>().Select(e => e.To).Concat(lastTurn.OfType<BlastAt>().Select(e => e.At)).ToList();
            if (!Done(CiviliansRun) && Scared().Any())
                list.Add(new AddInOption(CiviliansRun, "Civilians near the shooting panic and run out into the open."));

            if (!Done(Sniper) && turn <= 15 && marines.Any(u => Vec2.Distance(u.Position, u.Start) >= 60f))
                list.Add(new AddInOption(Sniper, "An enemy sniper on the far bank with a long view of the squad's way in."));

            if (!Done(Ied) && turn <= 10 && road != null && IedSpot() != null)
                list.Add(new AddInOption(Ied, "An IED hidden by the road on the near bank, short of the bridge, where the squad hasn't been yet."));

            var plan = m.Enemy.Plan;
            if (!Done(PullBack) && (plan == null || plan.Goal != EnemyPlan.Delay) && marines.Any(u => OnEnemyBank(u.Position)) && enemy.Any(u => u.CanAct))
                list.Add(new AddInOption(PullBack, "The enemy sees Marines on its own bank, feels flanked, and pulls back from the bridge."));

            if (!Done(RadioDown) && Teams().Any())
                list.Add(new AddInOption(RadioDown, "One fire team's radio goes dead for 3 turns: it only gets orders Lead can shout from within 50 m."));
            return list;

            IEnumerable<Unit> Scared() => Sim.Units.Where(c => ReferenceEquals(c.Force, m.Civilians) && c.CanAct && c.Order == null && landed.Any(p => Vec2.Distance(p, c.Position) <= 120f));
        }

        IEnumerable<string> Teams() => Marines.Where(u => !string.IsNullOrEmpty(u.Team)).GroupBy(u => u.Team).Where(g => g.Count() >= 2).Select(g => g.Key);

        /// <summary>Switches an add-in on, if it still fits. Returns false when it doesn't.</summary>
        public bool Apply(string id)
        {
            var option = Options().FirstOrDefault(o => o.Id == id);
            if (option == null) return false;
            switch (id)
            {
                case Reinforcements: BringReinforcements(); break;
                case CiviliansRun: CiviliansPanic(); break;
                case Sniper: PlaceSniper(); break;
                case Ied: Sim.PlaceIed(IedSpot().Value); break;
                case PullBack: EnemyPullsBack(); break;
                case RadioDown:
                    var teams = Teams().ToList();
                    Sim.KnockOutRadio(teams[Pick(teams.Count)], 3);
                    break;
            }
            used.Add((id, Sim.Clock.Turn));
            Sim.Record(new AddInSwitchedOn(id, option.What));
            return true;
        }

        int Pick(int n) => Math.Min(n - 1, (int)(BridgeSeizure.Next(rng) * n));

        /// <summary>A spot on the far bank in this ring around the bridge, scored by what the caller wants. Null if none fits.</summary>
        Vec2? FarBankSpot(float near, float far, Func<Vec2, TerrainDef, float> score)
        {
            var map = Sim.Map;
            Vec2? best = null;
            float bestScore = float.MinValue;
            for (int k = 0; k < 200; k++)
            {
                float r = near + (far - near) * BridgeSeizure.Next(rng);
                double a = BridgeSeizure.Next(rng) * Math.PI * 2;
                var p = new Vec2(Bridge.X + (float)Math.Cos(a) * r, Bridge.Y + (float)Math.Sin(a) * r);
                if (!map.InBounds(p) || !OnEnemyBank(p)) continue;
                var t = map.TerrainAt(p);
                if (t.Deep || !t.Passable || t.SeeOut) continue;
                float s = score(p, t) + BridgeSeizure.Next(rng) * 0.2f;
                if (s > bestScore) { bestScore = s; best = p; }
            }
            return best;
        }

        string NextName(string kind) =>
            kind + " " + (Sim.Units.Count(u => u.Name != null && u.Name.StartsWith(kind + " ", StringComparison.Ordinal)) + 1);

        void BringReinforcements()
        {
            var map = Sim.Map;
            int count = 2 + Pick(3);
            Vec2 drop = FarBankSpot(330f, 450f, (p, t) => t.Id == "road" || t.Id == "trail" ? 1f : 0f) ?? BridgeSeizure.Inside(map, Bridge);
            for (int i = 0; i < count; i++)
            {
                bool rpg = i == 3;
                var at = BridgeSeizure.Standable(map, drop + new Vec2(i * 4f, i % 2 * 4f), Sim.Units.Select(u => u.Position));
                var u = new Unit("enemy-r" + (i + 1), NextName(rpg ? "RPG gunner" : "Rifleman"), data.FindUnit(rpg ? "en-rpg" : "en-rf"), m.Enemy, at);
                Sim.AddUnit(u);
                var to = FarBankSpot(50f, 120f, (p, t) => t.Cover + (Sight.Trace(map, p, 1.1f, Bridge, 1f).Clear ? 1f : 0f)) ?? Bridge;
                if (!Sim.Give(u, new Order { Verb = Verbs.Move, Where = Sim.Coordinates(to), Speed = "run" }).Ok) Send(u, Verbs.Move, Bridge - at, 200f);
            }
            Sim.Radio(null, "Raider 6: Drone feed shows a truck dropping off " + count + " fighters at " + map.SquareOf(drop) + " on the far side.", "info", drop, "Raider 6");
        }

        void CiviliansPanic()
        {
            var map = Sim.Map;
            int turn = Sim.Clock.Turn;
            var lastTurn = Sim.Log.Events.Where(e => e.Turn == turn - 1).ToList();
            var landed = lastTurn.OfType<ShotFired>().Select(e => e.To).Concat(lastTurn.OfType<BlastAt>().Select(e => e.At)).ToList();
            var scared = Sim.Units.Where(c => ReferenceEquals(c.Force, m.Civilians) && c.CanAct && c.Order == null && landed.Any(p => Vec2.Distance(p, c.Position) <= 120f))
                .OrderBy(c => landed.Min(p => Vec2.Distance(p, c.Position))).Take(1 + Pick(2)).ToList();
            foreach (var c in scared) Send(c, Verbs.Move, c.Position - Bridge, 350f);
            var seen = scared.FirstOrDefault(c => Sim.Fog.For(m.Marines).About(c.Id)?.Visible == true);
            var who = Marines.FirstOrDefault();
            if (seen != null && who != null) Sim.Radio(who, (string.IsNullOrEmpty(who.Team) ? who.Name : who.Team) + ": Civilians running at " + map.SquareOf(seen.Position) + "! Watch your fire!", "info", seen.Position);
        }

        /// <summary>
        /// Sends someone running about this far in roughly this direction: straight on if there's a dry way, otherwise a
        /// little to either side, or not so far. False if nowhere works.
        /// </summary>
        bool Send(Unit u, string verb, Vec2 toward, float far)
        {
            float len = toward.Length;
            Vec2 dir = len > 0.1f ? toward * (1f / len) : new Vec2(0, 1);
            foreach (float d in new[] { far, far * 0.7f, far * 0.4f })
                foreach (float turn in new[] { 0f, 0.5f, -0.5f, 1f, -1f })
                {
                    var r = new Vec2(dir.X * (float)Math.Cos(turn) - dir.Y * (float)Math.Sin(turn), dir.X * (float)Math.Sin(turn) + dir.Y * (float)Math.Cos(turn));
                    var to = BridgeSeizure.Inside(Sim.Map, u.Position + r * d);
                    if (Sim.Map.TerrainAt(to).Deep) continue;
                    if (Sim.Give(u, new Order { Verb = verb, Where = Sim.Coordinates(to), Speed = "run" }).Ok) return true;
                }
            return false;
        }

        void PlaceSniper()
        {
            var map = Sim.Map;
            Vec2 approach = BridgeSeizure.Inside(map, Bridge + (Bridge - FarSide()) * 1f);
            Vec2 at = FarBankSpot(250f, 450f, (p, t) => t.Concealment + map.GroundAt(p) / 20f + (Sight.Trace(map, p, 0.4f, approach, 1.7f).Clear ? 2f : 0f)) ?? BridgeSeizure.Inside(map, FarSide());
            var u = new Unit("enemy-s1", NextName("Sniper"), data.FindUnit("en-snp"), m.Enemy, at) { Posture = Posture.Prone };
            u.Facing = BridgeSeizure.Bearing(at, Bridge);
            Sim.AddUnit(u);
        }

        /// <summary>A spot well out on the far bank, straight back from the bridge.</summary>
        Vec2 FarSide()
        {
            var map = Sim.Map;
            for (int k = 1; k <= 8; k++)
            {
                double a = BridgeSeizure.Next(rng) * Math.PI * 2;
                var p = BridgeSeizure.Inside(map, Bridge + new Vec2((float)Math.Cos(a), (float)Math.Sin(a)) * 250f);
                if (OnEnemyBank(p)) return p;
            }
            return Bridge;
        }

        /// <summary>A spot by the road on the near bank, 70 to 150 m from the bridge, nearer the bridge than any Marine has got.</summary>
        Vec2? IedSpot()
        {
            if (road == null) return null;
            float closest = Marines.Select(u => Vec2.Distance(u.Position, Bridge)).DefaultIfEmpty(float.MaxValue).Min();
            if (closest < 150f) return null;
            Vec2? best = null;
            float bestD = float.MaxValue;
            for (int i = 0; i + 1 < road.Count; i++)
                for (int k = 0; k <= 20; k++)
                {
                    var p = road[i] + (road[i + 1] - road[i]) * (k / 20f);
                    if (!Sim.Map.InBounds(p) || OnEnemyBank(p) || BridgeSeizure.Bank(water, p) * enemySide > -10f) continue;
                    float d = Vec2.Distance(p, Bridge);
                    if (d < 70f || d > 150f || d > closest - 30f) continue;
                    // The farther from the bridge, the sooner they reach it: take the one nearest 110 m.
                    if (Math.Abs(d - 110f) < bestD) { bestD = Math.Abs(d - 110f); best = p + new Vec2(1.5f, 1.5f); }
                }
            return best;
        }

        void EnemyPullsBack()
        {
            var marines = Marines.ToList();
            var going = Sim.Units.Where(u => ReferenceEquals(u.Force, m.Enemy) && u.CanAct && !marines.Any(x => Vec2.Distance(x.Position, u.Position) <= 60f)).ToList();
            foreach (var u in going) Send(u, Verbs.Withdraw, u.Position - Bridge, 200f);
            if (going.Count > 0) Sim.Shout(going[0], "They're behind us! Pull back!", always: true);
        }
    }
}
