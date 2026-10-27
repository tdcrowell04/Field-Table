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
            // A drone is only ever called by name, never as part of the squad or a team.
            if (n.Equals("squad", StringComparison.OrdinalIgnoreCase) || n.Equals("all", StringComparison.OrdinalIgnoreCase) || n.Equals("everyone", StringComparison.OrdinalIgnoreCase))
                return mine.Where(u => !u.Def.Flying).ToList();
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
            order.Verb = (order.Verb ?? "").Trim().ToLowerInvariant();
            var teams = list.Select(u => u.Team).Distinct().ToList();
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
                // Bounding: teams take turns when it's more than one team; inside a team, every other Marine.
                if (o.Verb == Verbs.Bound) o.Half = teams.Count > 1 ? teams.IndexOf(list[i].Team) % 2 : Math.Max(0, able.IndexOf(list[i])) % 2;
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
            if (unit.Def.Flying) return Fail(check, "Tell " + (unit.Operator?.Name ?? "the drone operator") + " to fly it there, Lead.");
            if (unit.Detained) return Fail(check, unit.Name + " is detained.");
            if (unit.Gone) return Fail(check, unit.Name + " isn't here any more.");
            if (!unit.CanAct) return Fail(check, unit.Name + " is " + (unit.Condition == Condition.Dead ? "dead" : "down") + " and can't act.");
            if (!Verbs.Built.Contains(verb)) return Fail(check, "not covered: " + verb);
            if (unit.LegHit && Verbs.NeedsLegs(verb)) return Fail(check, unit.Name + ": Can't walk, Lead. I need to be carried.");

            // A request names what it asks for, not a place or a person; flying the drone home names no place.
            if (verb == Verbs.Request && string.IsNullOrWhiteSpace(order.Kind) && !string.IsNullOrWhiteSpace(order.Target)) { order.Kind = order.Target; order.Target = null; }
            if (verb == Verbs.FlyDrone && order.Where != null && RecallWords.Contains(order.Where.Trim().ToLowerInvariant())) order.Where = null;

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

                case Verbs.Bound:
                    if (order.Point == null && order.TargetUnit != null) order.Point = Clamp(order.TargetUnit.Position + offset);
                    if (order.Point == null) return Fail(check, "Say again, bound where?");
                    if (!Route(check, unit, order)) return check;
                    order.LastPos = unit.Position;
                    break;

                case Verbs.Overwatch:
                    {
                        // Where is the spot to watch from (optional); target is what to watch.
                        Vec2? watch = order.TargetUnit?.Position;
                        if (watch == null && !string.IsNullOrWhiteSpace(order.Target) && TryPlace(unit, order.Target, out Vec2 wp)) watch = wp;
                        if (string.IsNullOrWhiteSpace(order.Where)) order.Point = null;
                        order.Watch = watch;
                        if (order.Point != null && !Route(check, unit, order)) return check;
                        break;
                    }

                case Verbs.ThrowSmoke:
                    if (unit.Smokes <= 0) return Fail(check, unit.Name + " is out of smoke.");
                    if (order.Point == null && order.TargetUnit != null) order.Point = order.TargetUnit.Position;
                    if (order.Point == null) order.Point = SmokeSpot(unit);
                    if (Vec2.Distance(unit.Position, order.Point.Value) > ThrowRange) return Fail(check, "Too far to throw, that's past 35 meters.");
                    break;

                case Verbs.Clear:
                    order.Building = BuildingAt(order.Point ?? order.TargetUnit?.Position);
                    if (order.Building == null) return Fail(check, "Say again, which building?");
                    order.Point = order.Building.Center;
                    if (!Route(check, unit, order)) return check;
                    break;

                case Verbs.Breach:
                    {
                        if (order.Point == null) return Fail(check, "Say again, breach where?");
                        // A grid square means anywhere in it; a named place means right there.
                        GridRef? square = GridRef.TryParse((order.Where ?? "").Replace(" ", ""), out GridRef g) && Map.IsOnMap(g) ? g : (GridRef?)null;
                        var bp = FindBreachPoint(order.Point.Value, unit.Position, square);
                        if (bp == null) return Fail(check, "Nothing to breach there, Lead.");
                        order.Watch = bp;
                        order.Point = bp;
                        if (!Route(check, unit, order)) return check;
                        break;
                    }

                case Verbs.FlyDrone:
                    if (DroneOf(unit) == null) return Fail(check, unit.Name + " doesn't have a drone.");
                    break;

                case Verbs.Talk:
                    if (order.TargetUnit == null || ReferenceEquals(order.TargetUnit.Force, unit.Force)) return Fail(check, "Say again, talk to who?");
                    if (order.TargetUnit.CanAct && AreHostile(unit.Force, order.TargetUnit.Force)) return Fail(check, order.TargetUnit.Name + " is fighting us, Lead. Detain them first.");
                    order.Point = order.TargetUnit.Position;
                    if (!Route(check, unit, order)) return check;
                    break;

                case Verbs.Search:
                    if (order.TargetUnit != null && !ReferenceEquals(order.TargetUnit.Force, unit.Force))
                    {
                        if (order.TargetUnit.CanAct && AreHostile(unit.Force, order.TargetUnit.Force)) return Fail(check, order.TargetUnit.Name + " is still fighting, Lead.");
                        order.Point = order.TargetUnit.Position;
                    }
                    else
                    {
                        order.TargetUnit = null;
                        if (order.Point == null) order.Point = unit.Position;
                    }
                    if (Vec2.Distance(unit.Position, order.Point.Value) > 1f && !Route(check, unit, order)) return check;
                    break;

                case Verbs.Carry:
                    {
                        var cas = order.TargetUnit;
                        if (cas == null || !ReferenceEquals(cas.Force, unit.Force) || ReferenceEquals(cas, unit) || cas.Def.Flying) return Fail(check, "Say again, carry who?");
                        if (cas.Condition == Condition.Dead) return Fail(check, cas.Name + " is gone, Lead.");
                        if (!(cas.Condition == Condition.Down || cas.LegHit)) return Fail(check, cas.Name + " can walk, Lead.");
                        if (string.IsNullOrWhiteSpace(order.Where))
                        {
                            if (Evac == null) return Fail(check, "Carry " + cas.Name + " where, Lead?");
                            order.Point = Evac.Lz;
                            order.Where = "the LZ";
                        }
                        if (Paths.Find(cas.Position, order.Point.Value, order.Swim) == null) return Fail(check, "No way to carry " + cas.Name + " there from here.");
                        var toCasualty = Paths.Find(unit.Position, cas.Position, order.Swim);
                        if (toCasualty == null) return Fail(check, "No way to get to " + cas.Name + " from here.");
                        check.Path = toCasualty;
                        break;
                    }

                case Verbs.Detain:
                    {
                        var p = order.TargetUnit;
                        if (p == null || ReferenceEquals(p.Force, unit.Force)) return Fail(check, "Say again, detain who?");
                        if (p.Detained) return Fail(check, p.Name + " is already detained.");
                        if (p.Condition == Condition.Dead) return Fail(check, p.Name + " is dead, Lead.");
                        if (AreHostile(unit.Force, p.Force) && p.CanAct && !(p.Condition == Condition.Wounded && p.Suppression >= PinnedAt)) return Fail(check, p.Name + " is still fighting, Lead.");
                        order.Point = p.Position;
                        if (!Route(check, unit, order)) return check;
                        break;
                    }

                case Verbs.Request:
                    {
                        string what = (order.Kind ?? "").Trim().ToLowerInvariant();
                        if (!(what.Contains("casevac") || what.Contains("medevac") || what.Contains("dustoff"))) return Fail(check, "not covered: request " + (what.Length == 0 ? "something" : what));
                        if (Evac != null) return Fail(check, "CASEVAC is already on the way, Lead.");
                        if (order.Point == null) return Fail(check, "Say again, where's the LZ?");
                        break;
                    }
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
            return TryGround(unit, name, out point);
        }

        // Everyday words for kinds of ground, and the terrain each means.
        static readonly Dictionary<string, string[]> GroundWords = new Dictionary<string, string[]>
        {
            { "woods", new[] { "forest" } }, { "wood", new[] { "forest" } }, { "forest", new[] { "forest" } }, { "trees", new[] { "forest", "orchard" } },
            { "treeline", new[] { "forest" } }, { "tree line", new[] { "forest" } }, { "pines", new[] { "forest" } }, { "orchard", new[] { "orchard" } },
            { "field", new[] { "stubble", "plowed", "corn", "grass" } }, { "fields", new[] { "stubble", "plowed", "corn", "grass" } },
            { "crops", new[] { "corn" } }, { "corn", new[] { "corn" } }, { "cornfield", new[] { "corn" } }, { "tall crops", new[] { "corn" } },
            { "reeds", new[] { "marsh" } }, { "marsh", new[] { "marsh" } }, { "swamp", new[] { "marsh" } },
            { "scrub", new[] { "scrub" } }, { "brush", new[] { "scrub" } }, { "bushes", new[] { "scrub" } },
            { "hedge", new[] { "hedge" } }, { "hedgerow", new[] { "hedge" } }, { "wall", new[] { "wall" } }, { "stone wall", new[] { "wall" } },
            { "rocks", new[] { "rocky" } }, { "gully", new[] { "gully" } }, { "ditch", new[] { "gully" } }, { "wash", new[] { "gully" } },
            { "road", new[] { "road" } }, { "trail", new[] { "trail" } }, { "path", new[] { "trail" } }, { "creek", new[] { "creek" } }, { "stream", new[] { "creek" } },
            { "building", new[] { "building" } }, { "buildings", new[] { "building" } }, { "house", new[] { "building" } }, { "farmyard", new[] { "yard" } }, { "yard", new[] { "yard" } },
            { "cliff", new[] { "cliff" } }, { "bridge", new[] { "bridge" } },
        };

        /// <summary>
        /// A kind of ground in a grid square, like "the woods at C3": the patch of it in that square nearest the unit.
        /// </summary>
        bool TryGround(Unit unit, string name, out Vec2 point)
        {
            point = default;
            var m = System.Text.RegularExpressions.Regex.Match(name.Trim().ToLowerInvariant(), @"^(?:the\s+)?(.+?)\s+(?:in|at|on|of)\s+([a-z]\s?\d{1,2})$");
            if (!m.Success || !GroundWords.TryGetValue(m.Groups[1].Value.Trim(), out string[] kinds)) return false;
            if (!GridRef.TryParse(m.Groups[2].Value.Replace(" ", ""), out GridRef g) || !Map.IsOnMap(g)) return false;
            Vec2 c = Map.SquareCenter(g);
            float half = Map.SquareSize / 2f;
            float best = float.MaxValue;
            bool found = false;
            for (float y = c.Y - half + 1f; y < c.Y + half; y += 2f)
                for (float x = c.X - half + 1f; x < c.X + half; x += 2f)
                {
                    var p = new Vec2(x, y);
                    if (!Map.InBounds(p) || Array.IndexOf(kinds, Map.TerrainAt(p).Id) < 0) continue;
                    float d = Vec2.Distance(p, unit.Position);
                    if (d < best) { best = d; point = p; found = true; }
                }
            return found;
        }

        /// <summary>A unit an order points at: a spotted enemy, a civilian, or a friend.</summary>
        Unit FindTarget(Unit unit, string name)
        {
            var friend = units.FirstOrDefault(u => ReferenceEquals(u.Force, unit.Force) && u.IsCalled(name));
            if (friend != null) return friend;
            var known = Fog.For(unit.Force).Contacts.Select(c => c.Unit).Where(u => u.Condition != Condition.Dead && !u.Gone).ToList();
            // An exact name wins over a partial one, so "Rifleman 1" never means "Rifleman 12".
            var exact = known.Where(u => u.IsCalled(name)).OrderBy(u => Vec2.Distance(unit.Position, Fog.For(unit.Force).About(u.Id).LastKnownPosition)).FirstOrDefault();
            if (exact != null) return exact;
            string n = name.Trim().ToLowerInvariant();
            var matches = known.Where(u => u.IsCalled(name) || (u.Name ?? "").ToLowerInvariant().Contains(n) || (u.Def.Name ?? "").ToLowerInvariant().Contains(n)).ToList();
            return matches.OrderBy(u => Vec2.Distance(unit.Position, Fog.For(unit.Force).About(u.Id).LastKnownPosition)).FirstOrDefault();
        }

        static readonly HashSet<string> RecallWords = new HashSet<string> { "back", "home", "land", "me", "here", "base" };

        /// <summary>The building at or nearest a point, within 12 meters.</summary>
        Landmark BuildingAt(Vec2? p)
        {
            if (p == null) return null;
            Vec2 at = p.Value;
            return Map.Landmarks.Where(l => l.Kind == "building")
                .Select(l => (l, d: l.Contains(at) ? 0f : Vec2.Distance(l.Center, at)))
                .Where(x => x.d <= 12f).OrderBy(x => x.d).Select(x => x.l).FirstOrDefault();
        }

        /// <summary>Where to pop smoke when no spot is named: 15 meters toward the nearest known enemy, or at their feet.</summary>
        Vec2 SmokeSpot(Unit unit)
        {
            var near = Fog.For(unit.Force).Contacts.Where(c => AreHostile(unit.Force, c.Unit.Force) && c.Unit.CanAct)
                .OrderBy(c => Vec2.Distance(c.LastKnownPosition, unit.Position)).FirstOrDefault();
            if (near == null) return unit.Position;
            Vec2 to = near.LastKnownPosition - unit.Position;
            float len = to.Length;
            return len < 0.1f ? unit.Position : Clamp(unit.Position + to * (Math.Min(15f, len) / len));
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
            Vec2 dest = order.Where != null && TryPlace(unit, order.Where, out Vec2 d) ? d : unit.Position;
            // Side by side if there's dry ground there; otherwise in file behind the first, like on a bridge.
            foreach (var candidate in new[] { side * (6f * k * sign), dir * (-6f * index) })
                if (Standable(dest + candidate)) return candidate;
            return default;
        }

        bool Standable(Vec2 p)
        {
            if (!Map.InBounds(p)) return false;
            var t = Map.TerrainAt(p);
            return t.Passable && !t.Deep && t.MoveFactor >= 0.3f;
        }

        static MoveSpeed SpeedOf(Order o)
        {
            string s = (o.Speed ?? "").ToLowerInvariant();
            if (s == "run" || s == "rush" || s == "sprint") return MoveSpeed.Run;
            if (s == "crawl" || s == "low crawl") return MoveSpeed.Crawl;
            if (o.Verb == Verbs.Withdraw || o.Verb == Verbs.Bound) return MoveSpeed.Run;
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
