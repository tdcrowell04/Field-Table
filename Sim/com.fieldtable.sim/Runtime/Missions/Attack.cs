using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    /// <summary>
    /// How the enemy attacks a position (ledger round 19, item 12 (a): smarter attackers). Their machine guns hold the
    /// defenders down; the rest move cover to cover by bounds instead of walking straight in; a party swings round the
    /// side while the guns fire; and before a rush across open ground, someone throws smoke. Every attacking group in
    /// every mission uses this, so they all fight the same way.
    /// </summary>
    public sealed class Attack
    {
        /// <summary>How far each rush goes before stopping in cover.</summary>
        public const float Leg = 60f;

        /// <summary>Within this of where they're going, they make the last rush.</summary>
        public const float LastRush = 70f;

        /// <summary>How far out to the side the flanking party swings, and how far short of the objective it turns in.</summary>
        public const float FlankOut = 90f, FlankShort = 60f;

        /// <summary>A group this big or bigger sends a party round the side: a third of those moving.</summary>
        public const int FlankFrom = 5;

        /// <summary>The main party waits this long, at most, for the flank to get into place before its last rush.</summary>
        public const int WaitForFlankSeconds = 120;

        readonly Simulation sim;
        readonly Force targets;
        readonly List<Unit> men;
        readonly string team;
        Vec2 objective;
        Party main, flank;
        bool started;

        sealed class Party
        {
            public string Name;
            public List<Unit> Men = new List<Unit>();
            public Vec2? Turn;          // where the flank turns in, before the objective
            public bool Turned;
            public Vec2? Stop;          // the cover they're rushing to now
            public bool Smoked;         // smoke thrown for this rush
            public int WaitedSince = -1;
            public bool LastRushMade;
        }

        /// <summary>Starts an attack by these men on a spot, against a side they know about.</summary>
        public Attack(Simulation sim, List<Unit> men, string team, Vec2 objective, Force targets, bool flank = true)
        {
            this.sim = sim;
            this.men = men;
            this.team = team;
            this.objective = objective;
            this.targets = targets;
            WithFlank = flank;
        }

        /// <summary>False when the group should go straight at it, like one already coming from the side.</summary>
        public bool WithFlank { get; }

        /// <summary>The party swinging round the side, for tests and the debrief; empty when there isn't one.</summary>
        public IReadOnlyList<Unit> FlankParty => flank?.Men ?? (IReadOnlyList<Unit>)Array.Empty<Unit>();

        /// <summary>True once the last rush is over (they've overrun it), or everyone who can still move is there.</summary>
        public bool Done => Overran || Movers().All(u => Vec2.Distance(u.Position, objective) <= Mission.OnObjective * 1.5f);

        /// <summary>True once the main party's last rush onto the objective has finished.</summary>
        public bool Overran { get; private set; }

        /// <summary>Where they're attacking.</summary>
        public Vec2 Objective => objective;

        /// <summary>Moves the objective, like a delaying squad that's fallen back.</summary>
        public void Retarget(Vec2 to)
        {
            if (Vec2.Distance(to, objective) < 30f) return;
            objective = to;
            foreach (var p in Parties()) { p.Stop = null; p.Smoked = false; }
        }

        IEnumerable<Party> Parties() => new[] { main, flank }.Where(p => p != null);

        List<Unit> Movers() => men.Where(u => u.CanAct && !u.Gone && u.Def.Id != "en-pkm").ToList();

        /// <summary>Once a second while the attack goes on.</summary>
        public void Tick()
        {
            var up = men.Where(u => u.CanAct && !u.Gone).ToList();
            if (up.Count == 0) return;
            if (!started) Start(up);
            // Anyone who joined since it started (like a probe falling back into the push) goes with the main party.
            foreach (var u in up.Where(u => u.Def.Id != "en-pkm" && !Parties().Any(p => p.Men.Contains(u)))) { main.Men.Add(u); u.Team = main.Name; }

            // The guns keep the defenders' heads down the whole time, from where they are.
            var guns = up.Where(u => u.Def.Id == "en-pkm" && (u.Order == null || u.Order.Verb != Verbs.Suppress)).ToList();
            MissionKit.SuppressKnown(sim, guns, objective, targets);

            // The flank gets into place before the main party's last rush, so they hit together.
            bool flankReady = flank == null || flank.Turned || flank.Men.All(u => !u.CanAct);
            foreach (var p in Parties()) Move(p, p == main && !flankReady);
        }

        void Start(List<Unit> up)
        {
            started = true;
            var movers = up.Where(u => u.Def.Id != "en-pkm").ToList();
            main = new Party { Name = team };
            if (WithFlank && movers.Count >= FlankFrom)
            {
                // A third goes round the side with more cover; the RPG goes with them if there is one.
                int n = Math.Max(2, movers.Count / 3);
                var going = movers.OrderByDescending(u => u.Def.Id == "en-rpg").ThenBy(u => u.Name).Take(n).ToList();
                Vec2 from = Average(movers.Select(u => u.Position));
                Vec2 d = MissionKit.Unit(objective - from);
                var across = new Vec2(-d.Y, d.X);
                Vec2 Spot(float s) => MissionKit.Inside(sim.Map, objective - d * FlankShort + across * (FlankOut * s));
                float Cover(Vec2 p) => Ring(p, 30f).Max(q => sim.Map.TerrainAt(q).Cover + sim.Map.TerrainAt(q).Concealment);
                Vec2 turn = Cover(Spot(1f)) >= Cover(Spot(-1f)) ? Spot(1f) : Spot(-1f);
                flank = new Party { Name = team + " flank", Men = going, Turn = BestCover(turn, 30f, going[0].Position) ?? turn };
                main.Men = movers.Except(going).ToList();
            }
            else main.Men = movers;
            foreach (var p in Parties()) foreach (var u in p.Men) u.Team = p.Name;
        }

        void Move(Party p, bool holdShort)
        {
            var up = p.Men.Where(u => u.CanAct && !u.Gone).ToList();
            if (up.Count == 0) return;
            // Still on a rush: let it finish. Pinned men stay down until it eases.
            if (up.Any(u => u.Order != null && u.Order.Verb != Verbs.Suppress && u.Order.Verb != Verbs.ThrowSmoke)) return;
            if (p.LastRushMade) { if (p == main) Overran = true; return; }
            var able = up.Where(u => u.Suppression < 0.6f).ToList();
            if (able.Count == 0) return;
            Vec2 at = Average(able.Select(u => u.Position));

            // The flank first goes round to where it turns in.
            Vec2 goal = p.Turn != null && !p.Turned ? p.Turn.Value : objective;
            if (p.Turn != null && !p.Turned && Vec2.Distance(at, p.Turn.Value) <= 25f) { p.Turned = true; goal = objective; }

            float left = Vec2.Distance(at, goal);
            if (goal.Equals(objective) && left <= LastRush)
            {
                if (holdShort)
                {
                    // Wait for the flank, but not forever.
                    if (p.WaitedSince < 0) p.WaitedSince = sim.Clock.Step;
                    if (sim.Clock.Step - p.WaitedSince < WaitForFlankSeconds * SimClock.StepsPerSecond) return;
                }
                Rush(p, able, at, objective);
                p.LastRushMade = able.Any(u => u.Order != null);
                return;
            }
            // Next, the best cover about one rush on toward where they're going.
            Vec2 dir = MissionKit.Unit(goal - at);
            Vec2 ahead = at + dir * Math.Min(Leg, left);
            var stop = BestCover(ahead, 25f, at) ?? ahead;
            Rush(p, able, at, stop);
        }

        void Rush(Party p, List<Unit> able, Vec2 at, Vec2 to)
        {
            // Smoke first, when the way there is open and they know where the defenders are.
            if (!p.Smoked && Open(at, to) && KnownWithin(at, 350f) is Vec2 enemy)
            {
                var thrower = able.FirstOrDefault(u => u.Smokes > 0);
                if (thrower != null)
                {
                    Vec2 toward = MissionKit.Unit(enemy - thrower.Position);
                    var spot = MissionKit.Inside(sim.Map, thrower.Position + toward * 25f);
                    if (sim.Give(thrower, new Order { Verb = Verbs.ThrowSmoke, Where = sim.Coordinates(spot) }).Ok) { p.Smoked = true; return; }
                }
            }
            p.Smoked = false;
            p.Stop = to;
            MissionKit.TeamOrder(sim, p.Men, p.Name, to, Verbs.Bound, null, able);
            // A rush that couldn't be ordered (no way there) goes straight for the objective instead.
            if (able.All(u => u.Order == null) && !to.Equals(objective)) MissionKit.TeamOrder(sim, p.Men, p.Name, objective, Verbs.Bound, null, able);
        }

        /// <summary>True when most of the way is ground that hides nobody.</summary>
        bool Open(Vec2 a, Vec2 b)
        {
            int open = 0, n = 6;
            for (int i = 1; i <= n; i++)
            {
                var t = sim.Map.TerrainAt(a + (b - a) * (i / (float)n));
                if (t.Cover < 0.25f && t.Concealment < 0.25f) open++;
            }
            return open >= n / 2 && Vec2.Distance(a, b) >= 25f;
        }

        Vec2? KnownWithin(Vec2 at, float range)
        {
            var knows = sim.Fog.For(men[0].Force);
            var c = knows.Contacts.Where(k => k.Known && ReferenceEquals(k.Unit.Force, targets) && k.Unit.CanAct && !k.Unit.Def.Flying && Vec2.Distance(k.LastKnownPosition, at) <= range)
                .OrderBy(k => Vec2.Distance(k.LastKnownPosition, at)).FirstOrDefault();
            return c?.LastKnownPosition;
        }

        /// <summary>
        /// The spot with the most cover within reach of a point, that can be walked to from where they are. Only the best
        /// few get a route looked for, to keep turns quick.
        /// </summary>
        Vec2? BestCover(Vec2 around, float reach, Vec2 from)
        {
            var ranked = Ring(around, reach)
                .Select(p => (p, t: sim.Map.TerrainAt(p)))
                .Where(x => !x.t.Deep && x.t.Passable)
                .Select(x => (x.p, score: x.t.Cover * 2f + x.t.Concealment - Vec2.Distance(x.p, around) * 0.01f))
                .OrderByDescending(x => x.score).Take(4);
            foreach (var (p, _) in ranked)
                if (sim.Paths.Find(from, p, false) != null) return p;
            return null;
        }

        IEnumerable<Vec2> Ring(Vec2 c, float reach)
        {
            yield return MissionKit.Inside(sim.Map, c);
            for (float r = 8f; r <= reach; r += 8f)
                for (int a = 0; a < 12; a++)
                    yield return MissionKit.Inside(sim.Map, c + new Vec2((float)Math.Cos(a * Math.PI / 6) * r, (float)Math.Sin(a * Math.PI / 6) * r));
        }

        static Vec2 Average(IEnumerable<Vec2> ps)
        {
            var l = ps.ToList();
            return new Vec2(l.Average(p => p.X), l.Average(p => p.Y));
        }
    }
}
