using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    public sealed partial class Simulation
    {
        Pathfinder pathfinder;
        int nextGroup;

        /// <summary>Routes over this map. Built the first time something needs one.</summary>
        public Pathfinder Paths => pathfinder ?? (pathfinder = new Pathfinder(Map));

        /// <summary>
        /// The units in a force an order means: one unit by name or callsign, a team ("Alpha"),
        /// or everyone ("squad", "all").
        /// </summary>
        public List<Unit> UnitsCalled(Force force, string name)
        {
            var mine = units.Where(u => ReferenceEquals(u.Force, force)).ToList();
            if (string.IsNullOrWhiteSpace(name)) return new List<Unit>();
            string n = name.Trim();
            if (n.Equals("squad", StringComparison.OrdinalIgnoreCase) || n.Equals("all", StringComparison.OrdinalIgnoreCase) || n.Equals("everyone", StringComparison.OrdinalIgnoreCase))
                return mine;
            var team = mine.Where(u => string.Equals(u.Team, n, StringComparison.OrdinalIgnoreCase)).ToList();
            if (team.Count > 0) return team;
            return mine.Where(u => u.IsCalled(n)).ToList();
        }

        /// <summary>
        /// Gives an order to everyone it names. A team spreads out side by side instead of piling onto one spot.
        /// Each unit's check comes back, whether or not it could do it.
        /// </summary>
        public List<OrderCheck> GiveOrder(Force force, string who, Order order)
        {
            var list = UnitsCalled(force, who);
            var results = new List<OrderCheck>();
            if (list.Count == 0)
            {
                results.Add(new OrderCheck { Ok = false, Problem = "Nobody called " + who + " in " + force.Name + "." });
                return results;
            }
            int group = ++nextGroup;
            var able = list.Where(u => u.CanAct).ToList();
            Vec2 centroid = able.Count > 0 ? Average(able.Select(u => u.Position)) : list[0].Position;
            for (int i = 0; i < list.Count; i++)
            {
                var o = order.Copy();
                o.Group = group;
                Vec2 offset = default;
                if (list.Count > 1 && able.Count > 1)
                {
                    int k = able.IndexOf(list[i]);
                    if (k >= 0) offset = Spread(k, centroid, order, list[i]);
                }
                results.Add(Give(list[i], o, offset));
            }
            return results;
        }

        /// <summary>Checks an order without carrying it out.</summary>
        public OrderCheck Check(Unit unit, Order order) => Resolve(unit, order.Copy(), default);

        /// <summary>Checks an order and, if it can be done, starts carrying it out.</summary>
        public OrderCheck Give(Unit unit, Order order) => Give(unit, order, default);

        OrderCheck Give(Unit unit, Order order, Vec2 offset)
        {
            var check = Resolve(unit, order, offset);
            if (!check.Ok) return check;
            unit.Order = order;
            unit.Path = check.Path;
            unit.PathIndex = 0;
            unit.TaskSeconds = 0f;
            unit.Swimming = order.Swim;
            unit.Speed = SpeedOf(order);
            if (unit.Speed == MoveSpeed.Crawl) unit.Posture = Posture.Prone;
            return check;
        }

        OrderCheck Resolve(Unit unit, Order order, Vec2 offset)
        {
            var check = new OrderCheck { Unit = unit };
            string verb = (order.Verb ?? "").Trim().ToLowerInvariant();
            order.Verb = verb;
            if (!unit.CanAct) return Fail(check, unit.Name + " is " + (unit.Condition == Condition.Dead ? "dead" : "down") + " and can't act.");
            if (!Verbs.Built.Contains(verb)) return Fail(check, "not covered: " + verb);

            // Where the order points, if anywhere.
            if (!string.IsNullOrWhiteSpace(order.Where))
            {
                if (!TryPlace(unit, order.Where, out Vec2 p)) return Fail(check, "Say again, where is " + order.Where + "?");
                order.Point = Clamp(p + offset);
            }
            if (!string.IsNullOrWhiteSpace(order.Target))
            {
                order.TargetUnit = FindTarget(unit, order.Target);
                if (order.TargetUnit == null && order.Point == null)
                {
                    if (TryPlace(unit, order.Target, out Vec2 p)) order.Point = p;
                    else return Fail(check, "Say again, which " + order.Target + "? We don't have eyes on that.");
                }
            }

            switch (verb)
            {
                case Verbs.Move:
                case Verbs.Assault:
                case Verbs.Observe:
                    if (order.Point == null && order.TargetUnit != null) order.Point = Clamp(order.TargetUnit.Position + offset);
                    if (order.Point == null) return Fail(check, "Say again, " + verb + " where?");
                    if (verb != Verbs.Observe && !Route(check, unit, order)) return check;
                    break;

                case Verbs.Withdraw:
                    if (order.Point == null) order.Point = Clamp(unit.Start + offset);
                    if (!Route(check, unit, order)) return check;
                    break;

                case Verbs.Follow:
                    if (order.TargetUnit == null || !ReferenceEquals(order.TargetUnit.Force, unit.Force)) return Fail(check, "Say again, follow who?");
                    if (ReferenceEquals(order.TargetUnit, unit)) return Fail(check, "Can't follow myself, Lead.");
                    break;

                case Verbs.Fire:
                case Verbs.Suppress:
                    if (unit.Def.Weapon == null) return Fail(check, unit.Name + " has no weapon.");
                    if (order.TargetUnit == null && order.Point == null) return Fail(check, "Say again, " + verb + " what?");
                    if (order.TargetUnit != null && !AreHostile(unit.Force, order.TargetUnit.Force)) check.Warnings.Add(order.TargetUnit.Name + " isn't the enemy.");
                    break;

                case Verbs.ThrowGrenade:
                    if (unit.Grenades <= 0) return Fail(check, unit.Name + " is out of grenades.");
                    if (order.Point == null && order.TargetUnit != null) order.Point = order.TargetUnit.Position;
                    if (order.Point == null) return Fail(check, "Say again, throw it where?");
                    if (Vec2.Distance(unit.Position, order.Point.Value) > 35f) return Fail(check, "Too far to throw, that's past 35 meters.");
                    break;

                case Verbs.TreatCasualty:
                    if (order.TargetUnit == null) order.TargetUnit = NearestCasualty(unit);
                    if (order.TargetUnit == null) return Fail(check, "Nobody's hit, Lead.");
                    if (order.TargetUnit.Condition == Condition.Ok) return Fail(check, order.TargetUnit.Name + " isn't hurt.");
                    if (order.TargetUnit.Condition == Condition.Dead) return Fail(check, order.TargetUnit.Name + " is gone. Nothing to treat.");
                    order.Point = order.TargetUnit.Position;
                    if (!Route(check, unit, order)) return check;
                    break;

                case Verbs.TakeCover:
                case Verbs.Hide:
                    order.Point = BestSpotNear(unit, verb == Verbs.Hide);
                    if (!Route(check, unit, order)) return check;
                    break;

                case Verbs.ChangePosture:
                    if (ParsePosture(order.Posture) == null) return Fail(check, "Say again, stand, kneel, or get down?");
                    break;
            }

            check.Ok = true;
            return check;
        }

        bool Route(OrderCheck check, Unit unit, Order order)
        {
            var path = Paths.Find(unit.Position, order.Point.Value, order.Swim);
            if (path == null)
            {
                bool wet = Paths.Find(unit.Position, order.Point.Value, true) != null;
                Fail(check, wet ? "Can't get there without swimming, Lead." : "No way to get there from here.");
                return false;
            }
            check.Path = path;
            if (order.Swim && unit.Load > LoadToSwim && PathCrossesDeep(unit.Position, path))
                check.Warnings.Add(unit.Name + ": Lead, that water's too deep to swim in full kit.");
            return true;
        }

        bool PathCrossesDeep(Vec2 start, List<Vec2> path)
        {
            Vec2 at = start;
            foreach (var p in path)
            {
                float d = Vec2.Distance(at, p);
                int n = Math.Max(1, (int)(d / 2f));
                for (int i = 1; i <= n; i++)
                    if (Map.TerrainAt(at + (p - at) * (i / (float)n)).Deep) return true;
                at = p;
            }
            return false;
        }

        /// <summary>Finds a place by friendly unit, landmark, or grid square. Lines snap to their nearest point.</summary>
        public bool TryPlace(Unit unit, string name, out Vec2 point)
        {
            point = default;
            if (string.IsNullOrWhiteSpace(name)) return false;
            var friend = units.FirstOrDefault(u => ReferenceEquals(u.Force, unit.Force) && !ReferenceEquals(u, unit) && u.IsCalled(name));
            if (friend == null && unit.Force != null) friend = UnitsCalled(unit.Force, name).FirstOrDefault(u => !ReferenceEquals(u, unit));
            if (friend != null) { point = friend.Position; return true; }

            var mark = Map.FindLandmark(name);
            if (mark != null)
            {
                point = (mark.Kind == "line" || mark.Kind == "control") && mark.Points.Count >= 2 && !mark.IsCalled("bridge")
                    && !mark.Label.EndsWith("bridge", StringComparison.OrdinalIgnoreCase)
                    ? NearestOnLine(unit.Position, mark.Points)
                    : mark.Center;
                return true;
            }
            if (GridRef.TryParse(name.Replace(" ", ""), out GridRef g) && Map.IsOnMap(g))
            {
                point = Map.SquareCenter(g);
                return true;
            }
            return false;
        }

        /// <summary>A unit an order points at: a spotted enemy, a civilian, or a friend.</summary>
        Unit FindTarget(Unit unit, string name)
        {
            var friend = units.FirstOrDefault(u => ReferenceEquals(u.Force, unit.Force) && u.IsCalled(name));
            if (friend != null) return friend;
            var known = Fog.For(unit.Force).Contacts.Select(c => c.Unit).Where(u => u.Condition != Condition.Dead).ToList();
            string n = name.Trim().ToLowerInvariant();
            var matches = known.Where(u => u.IsCalled(name) || (u.Name ?? "").ToLowerInvariant().Contains(n) || (u.Def.Name ?? "").ToLowerInvariant().Contains(n)).ToList();
            return matches.OrderBy(u => Vec2.Distance(unit.Position, Fog.For(unit.Force).About(u.Id).LastKnownPosition)).FirstOrDefault();
        }

        Unit NearestCasualty(Unit unit) =>
            units.Where(u => ReferenceEquals(u.Force, unit.Force) && !ReferenceEquals(u, unit) && (u.Condition == Condition.Wounded || (u.Condition == Condition.Down && !u.Stabilized)))
                 .OrderBy(u => Vec2.Distance(u.Position, unit.Position)).FirstOrDefault();

        /// <summary>The best place within 30 meters to take cover (or to hide), by the ground itself.</summary>
        Vec2 BestSpotNear(Unit unit, bool hide)
        {
            Vec2 best = unit.Position;
            float bestScore = Score(unit.Position) + 0.05f;
            for (int r = 4; r <= 30; r += 4)
                for (int a = 0; a < 16; a++)
                {
                    double ang = a * Math.PI / 8;
                    var p = new Vec2(unit.Position.X + (float)Math.Cos(ang) * r, unit.Position.Y + (float)Math.Sin(ang) * r);
                    if (!Map.InBounds(p)) continue;
                    var t = Map.TerrainAt(p);
                    if (t.Deep || !t.Passable) continue;
                    float s = Score(p) - r * 0.004f;
                    if (s > bestScore) { bestScore = s; best = p; }
                }
            return best;

            float Score(Vec2 p)
            {
                var t = Map.TerrainAt(p);
                return hide ? (t.Height >= Postures.Height(Posture.Prone) ? t.Concealment : t.Concealment * 0.3f) : t.Cover;
            }
        }

        Vec2 Spread(int index, Vec2 centroid, Order order, Unit unit)
        {
            if (index == 0) return default;
            Vec2 toward = order.Where != null && TryPlace(unit, order.Where, out Vec2 p) ? p - centroid : new Vec2(0, -1);
            float len = toward.Length;
            Vec2 dir = len > 0.01f ? toward * (1f / len) : new Vec2(0, -1);
            var side = new Vec2(-dir.Y, dir.X);
            int k = (index + 1) / 2;
            float sign = index % 2 == 1 ? 1f : -1f;
            return side * (6f * k * sign);
        }

        static MoveSpeed SpeedOf(Order o)
        {
            string s = (o.Speed ?? "").ToLowerInvariant();
            if (s == "run" || s == "rush" || s == "sprint") return MoveSpeed.Run;
            if (s == "crawl" || s == "low crawl") return MoveSpeed.Crawl;
            if (o.Verb == Verbs.Withdraw) return MoveSpeed.Run;
            return MoveSpeed.Walk;
        }

        internal static Posture? ParsePosture(string s)
        {
            switch ((s ?? "").Trim().ToLowerInvariant())
            {
                case "stand": case "standing": case "up": return Posture.Standing;
                case "kneel": case "kneeling": case "crouch": return Posture.Kneeling;
                case "prone": case "down": case "get down": case "lie down": return Posture.Prone;
                default: return null;
            }
        }

        Vec2 Clamp(Vec2 p) => new Vec2(Math.Max(1f, Math.Min(Map.WidthMeters - 1f, p.X)), Math.Max(1f, Math.Min(Map.HeightMeters - 1f, p.Y)));

        static Vec2 Average(IEnumerable<Vec2> pts)
        {
            float x = 0, y = 0; int n = 0;
            foreach (var p in pts) { x += p.X; y += p.Y; n++; }
            return n == 0 ? default : new Vec2(x / n, y / n);
        }

        static Vec2 NearestOnLine(Vec2 p, IReadOnlyList<Vec2> pts)
        {
            Vec2 best = pts[0];
            float bestD = float.MaxValue;
            for (int i = 0; i + 1 < pts.Count; i++)
            {
                Vec2 a = pts[i], b = pts[i + 1], ab = b - a;
                float len2 = ab.X * ab.X + ab.Y * ab.Y;
                float t = len2 <= 0f ? 0f : Math.Max(0f, Math.Min(1f, ((p.X - a.X) * ab.X + (p.Y - a.Y) * ab.Y) / len2));
                Vec2 q = a + ab * t;
                float d = Vec2.Distance(p, q);
                if (d < bestD) { bestD = d; best = q; }
            }
            return best;
        }

        static OrderCheck Fail(OrderCheck c, string problem)
        {
            c.Ok = false;
            c.Problem = problem;
            return c;
        }
    }
}
