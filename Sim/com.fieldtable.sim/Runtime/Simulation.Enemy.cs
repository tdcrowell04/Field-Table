using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    /// <summary>
    /// What the enemy is out to do, which picks who's there and steers how they fight: hold the objective, delay you,
    /// blow it up, or ambush you on it.
    /// </summary>
    public sealed class EnemyPlan
    {
        public const string Hold = "hold", Delay = "delay", Blow = "blow", Ambush = "ambush";
        public static readonly IReadOnlyList<string> Goals = new[] { Hold, Delay, Blow, Ambush };

        public string Goal { get; }

        /// <summary>The place the plan is about: the bridge.</summary>
        public Landmark Objective { get; }

        /// <summary>Ambush: true once they've opened up.</summary>
        public bool Sprung { get; internal set; }

        /// <summary>Delay: true while they're falling back to their next spot, and how many of them were hurt last time they did.</summary>
        public bool FallingBack { get; internal set; }
        internal int HurtSeen;

        /// <summary>Blow: the fighter with the charges, the spot he fires them from, and how far along he is.</summary>
        public Unit Engineer { get; set; }
        public Vec2 FiringPoint { get; set; }
        public float ChargeSeconds { get; internal set; }
        public bool ChargesSet => ChargeSeconds >= Simulation.ChargeTime;
        public bool Blown { get; internal set; }

        public EnemyPlan(string goal, Landmark objective)
        {
            if (!Goals.Contains(goal)) throw new ArgumentException("Unknown enemy goal: " + goal + ".", nameof(goal));
            Goal = goal;
            Objective = objective ?? throw new ArgumentNullException(nameof(objective));
        }

        /// <summary>True while the plan keeps them from shooting: an ambush that hasn't been sprung.</summary>
        public bool HoldingFire => Goal == Ambush && !Sprung;
    }

    /// <summary>How a side with a plan thinks: the ambush waiting, the delay falling back, the engineer at his charges.</summary>
    public sealed partial class Simulation
    {
        /// <summary>Blow: seconds to set the charges, and how close the attackers get before he fires them.</summary>
        public const float ChargeTime = 240f, BlowWhenWithin = 100f;

        /// <summary>Ambush: they open up once someone's on the bridge, this close to one of them, or they're shot at.</summary>
        public const float AmbushFound = 35f;

        /// <summary>Delay: they fall back once an attacker they know of gets this close, and go this much farther back.</summary>
        public const float DelayWithin = 200f, DelayBound = 200f;

        /// <summary>Hold: a side out to hold its ground falls back only once this much of it is lost (others at half).</summary>
        const float HoldBreaks = 0.66f;

        /// <summary>Gives a side with a plan its own way of thinking for this step. False leaves the usual brain to it.</summary>
        bool PlanBrain(Unit u, float dt)
        {
            var plan = u.Force.Plan;
            if (plan == null) return false;
            if (plan.Goal == EnemyPlan.Blow && ReferenceEquals(u, plan.Engineer)) { EngineerBrain(u, plan, dt); return true; }
            if (plan.Goal == EnemyPlan.Ambush && !plan.Sprung)
            {
                // Lying low and still until it's time.
                u.Posture = Posture.Prone;
                return true;
            }
            return false;
        }

        /// <summary>How much of a side has to be lost before it pulls back.</summary>
        float BreakingPoint(Force f) => f.Plan != null && f.Plan.Goal == EnemyPlan.Hold ? HoldBreaks : 0.5f;

        /// <summary>Once a second: springing the ambush, and sending the delay back.</summary>
        void ThinkPlans()
        {
            foreach (var plan in units.Select(u => u.Force.Plan).Where(p => p != null).Distinct().ToList())
            {
                var side = units.Where(u => ReferenceEquals(u.Force.Plan, plan)).ToList();
                var force = side[0].Force;
                if (plan.Goal == EnemyPlan.Ambush && !plan.Sprung && SpringAmbush(plan, side)) plan.Sprung = true;
                if (plan.Goal == EnemyPlan.Delay) ThinkDelay(plan, force, side);
            }
        }

        bool SpringAmbush(EnemyPlan plan, List<Unit> side)
        {
            var bridge = plan.Objective;
            foreach (var x in units)
            {
                if (!x.CanAct || x.Def.Flying || !AreHostile(side[0].Force, x.Force)) continue;
                if (OnBridge(bridge, x.Position)) return true;
                if (side.Any(e => e.CanAct && Vec2.Distance(e.Position, x.Position) <= AmbushFound)) return true;
            }
            return side.Any(e => e.Wounds > 0 || !e.CanAct || e.Suppression >= 0.3f);
        }

        static bool OnBridge(Landmark bridge, Vec2 p)
        {
            if (bridge.Points.Count < 2) return Vec2.Distance(p, bridge.Center) <= Mission.OnObjective;
            return MapGrid.DistanceToSegment(p, bridge.Points[0], bridge.Points[bridge.Points.Count - 1]) <= 5f;
        }

        void ThinkDelay(EnemyPlan plan, Force force, List<Unit> side)
        {
            var fighting = side.Where(u => u.CanAct).ToList();
            if (plan.FallingBack)
            {
                if (fighting.All(u => u.Order == null)) plan.FallingBack = false;
                return;
            }
            var known = Fog.For(force).Contacts.Where(c => AreHostile(force, c.Unit.Force) && !c.Unit.Def.Flying).Select(c => c.LastKnownPosition).ToList();
            bool close = known.Any(p => fighting.Any(u => Vec2.Distance(u.Position, p) <= DelayWithin));
            // Each new casualty sends them back once.
            int hurtNow = side.Count(u => u.Wounds > 0 || !u.CanAct);
            bool hurt = hurtNow > plan.HurtSeen;
            plan.HurtSeen = hurtNow;
            if (!close && !hurt) return;
            bool any = false;
            foreach (var u in fighting)
            {
                Vec2 away = u.Position - plan.Objective.Center;
                float len = away.Length;
                Vec2 dir = len > 0.1f ? away * (1f / len) : new Vec2(0, -1);
                Vec2 to = Clamp(u.Position + dir * DelayBound);
                if (Vec2.Distance(to, u.Position) < 40f) continue; // backed up against the edge: stay and fight
                var path = Paths.Find(u.Position, to, false);
                if (path == null) continue;
                u.Order = new Order { Verb = Verbs.Withdraw, Speed = "run", Point = to };
                u.Path = path;
                u.PathIndex = 0;
                u.Speed = MoveSpeed.Run;
                any = true;
            }
            plan.FallingBack = any;
        }

        // ---------- the engineer and his charges ----------

        void EngineerBrain(Unit u, EnemyPlan plan, float dt)
        {
            if (plan.Blown) { u.Posture = u.Suppression >= 0.4f ? Posture.Prone : Posture.Kneeling; return; }
            if (u.Suppression >= PinnedAt) { u.Posture = Posture.Prone; return; }
            if (!plan.ChargesSet)
            {
                Vec2 at = plan.Objective.Center;
                if (Vec2.Distance(u.Position, at) > 3f)
                {
                    if (u.Path == null) { u.Path = Paths.Find(u.Position, at, false); u.PathIndex = 0; u.Speed = MoveSpeed.Walk; }
                    return;
                }
                u.Path = null;
                u.Posture = Posture.Kneeling;
                plan.ChargeSeconds += dt;
                return;
            }
            if (Vec2.Distance(u.Position, plan.FiringPoint) > 2f)
            {
                if (u.Path == null) { u.Path = Paths.Find(u.Position, plan.FiringPoint, false); u.PathIndex = 0; u.Speed = MoveSpeed.Run; }
                return;
            }
            u.Path = null;
            u.Posture = Posture.Prone;
            if (Clock.Step % SimClock.StepsPerSecond != 0) return;
            var known = Fog.For(u.Force).Contacts.Where(c => AreHostile(u.Force, c.Unit.Force) && !c.Unit.Def.Flying && c.Unit.CanAct);
            if (known.Any(c => Vec2.Distance(c.LastKnownPosition, plan.Objective.Center) <= BlowWhenWithin)) BlowBridge(u, plan);
        }

        /// <summary>
        /// The charges go off: a big blast on the bridge, and the span drops into the water. Its ends over land become rubble.
        /// </summary>
        internal void BlowBridge(Unit by, EnemyPlan plan)
        {
            if (plan.Blown) return;
            plan.Blown = true;
            var bridge = plan.Objective;
            var charges = Weapon("demo");
            if (charges != null) Explode(by, charges, bridge.Center, ap: false);
            Vec2 a = bridge.Points.Count >= 2 ? bridge.Points[0] : bridge.Center, b = bridge.Points.Count >= 2 ? bridge.Points[bridge.Points.Count - 1] : bridge.Center;
            Vec2 along = b - a;
            float len = along.Length;
            along = len > 0.01f ? along * (1f / len) : new Vec2(1, 0);
            var across = new Vec2(-along.Y, along.X);
            var changed = new List<(int x, int y, string terrain)>();
            int x0 = (int)Math.Floor((Math.Min(a.X, b.X) - 8f) / Map.CellSize), x1 = (int)Math.Ceiling((Math.Max(a.X, b.X) + 8f) / Map.CellSize);
            int y0 = (int)Math.Floor((Math.Min(a.Y, b.Y) - 8f) / Map.CellSize), y1 = (int)Math.Ceiling((Math.Max(a.Y, b.Y) + 8f) / Map.CellSize);
            for (int y = Math.Max(0, y0); y <= Math.Min(Map.Height - 1, y1); y++)
                for (int x = Math.Max(0, x0); x <= Math.Min(Map.Width - 1, x1); x++)
                {
                    if (Map.TerrainAt(x, y).Id != "bridge") continue;
                    Vec2 c = Map.CellCenter(x, y);
                    // Over water if the water runs under it on either side; otherwise it's an end on land.
                    string under = "rubble";
                    foreach (float side in new[] { 1f, -1f })
                    {
                        Vec2 q = c + across * (8f * side);
                        if (!Map.InBounds(q)) continue;
                        var t = Map.TerrainAt(q);
                        if (t.Deep || t.Id == "creek") { under = t.Id; break; }
                    }
                    Map.SetTerrain(x, y, under);
                    changed.Add((x, y, under));
                }
            Paths.Refresh(new Vec2(x0 * Map.CellSize, y0 * Map.CellSize), new Vec2((x1 + 1) * Map.CellSize, (y1 + 1) * Map.CellSize));
            Record(new BridgeBlown(by, bridge, changed));
            var heard = units.FirstOrDefault(x => ReferenceEquals(x.Force, PlayerForce) && x.CanAct && !x.Def.Flying);
            if (heard != null) Radio(heard, Callsign(heard) + ": They blew the bridge! " + Capital(bridge.Label) + " is down.", "info", bridge.Center);
        }
    }
}
