using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    /// <summary>
    /// The verbs past the first 16: bound, overwatch, throw smoke, clear, breach, fly drone, talk, search,
    /// carry, detain, share ammo, and request (CASEVAC).
    /// </summary>
    public sealed partial class Simulation
    {
        /// <summary>How far one half of a team rushes before the other half takes its turn, in meters.</summary>
        public const float BoundLeg = 50f;

        /// <summary>On overwatch, anything this close to the watched spot is shot first.</summary>
        public const float OverwatchArea = 100f;

        /// <summary>Hand-thrown things (grenades, smoke) reach this far.</summary>
        public const float ThrowRange = 35f;

        /// <summary>Smoke: how wide, how tall, and how long it hangs.</summary>
        public const float SmokeRadius = 10f, SmokeHeight = 8f;
        public const int SmokeSeconds = 90;

        /// <summary>Seconds of work: clearing each 25 square meters of a building, breaching, talking, searching, detaining, sharing ammo, launching a drone.</summary>
        public const float RoomSeconds = 10f, BreachSeconds = 20f, TalkSeconds = 20f, SearchSeconds = 20f, DetainSeconds = 30f, ShareSeconds = 30f, LaunchSeconds = 15f;

        /// <summary>How long a drone stays up before its battery brings it home, in minutes.</summary>
        public const int DroneMinutes = 20;

        /// <summary>CASEVAC: minutes until it arrives (a die decides within these), how long it waits, and how close casualties must be to be loaded.</summary>
        public const int CasevacMinMinutes = 15, CasevacMaxMinutes = 30, CasevacWaitSeconds = 180;
        public const float CasevacLoadRadius = 25f;

        // Which half of each bounding group is moving.
        readonly Dictionary<int, int> boundMoving = new Dictionary<int, int>();

        // Groups that have already reported, so a team calls "clear" or "set" once.
        readonly HashSet<int> reported = new HashSet<int>();

        // Groups that have already popped smoke.
        readonly HashSet<int> smoked = new HashSet<int>();

        /// <summary>A CASEVAC on its way or on the ground, or null.</summary>
        public Casevac Evac { get; private set; }

        void ThinkMore(Unit u, Order o, float dt)
        {
            switch (o.Verb)
            {
                case Verbs.Bound: ThinkBound(u, o); break;
                case Verbs.Overwatch:
                    if (u.Path != null) break;
                    if (o.Watch != null) u.Facing = Bearing(u.Position, o.Watch.Value);
                    o.Arrived = true;
                    FirePosition(u, o, "Set. Overwatch on " + (o.Target ?? o.Where ?? "our sector") + ".");
                    break;
                case Verbs.ThrowSmoke: ThrowSmoke(u, o); break;
                case Verbs.Clear: ThinkClear(u, o, dt); break;
                case Verbs.Breach: ThinkBreach(u, o, dt); break;
                case Verbs.FlyDrone: ThinkDrone(u, o, dt); break;
                case Verbs.Talk: ThinkTalk(u, o, dt); break;
                case Verbs.Search: ThinkSearch(u, o, dt); break;
                case Verbs.Carry: ThinkCarry(u, o); break;
                case Verbs.Detain:
                    if (Reach(u, o, o.TargetUnit, 2f) && Work(u, dt, DetainSeconds))
                    {
                        var p = o.TargetUnit;
                        p.Detained = true;
                        p.Order = null;
                        p.Path = null;
                        p.IsMoving = false;
                        p.Posture = Posture.Kneeling;
                        Record(new UnitDetained(p, u));
                        Radio(u, u.Name + ": " + p.Name + " is detained.", "info", p.Position);
                        Finish(u, null);
                    }
                    break;
                case Verbs.ShareAmmo: ThinkShareAmmo(u, o, dt); break;
                case Verbs.Request: CallCasevac(u, o); break;
            }
        }

        /// <summary>Walks to within reach of someone, refreshing the route every few seconds. True once there.</summary>
        bool Reach(Unit u, Order o, Unit who, float reach)
        {
            if (who == null || who.Gone || who.Condition == Condition.Dead && o.Verb != Verbs.Search) { Finish(u, null); return false; }
            if (Vec2.Distance(u.Position, who.Position) <= reach) { u.Path = null; return true; }
            if (u.Path == null || Clock.Step % (5 * SimClock.StepsPerSecond) == 0)
            {
                u.Path = Paths.Find(u.Position, who.Position, o.Swim);
                u.PathIndex = 0;
            }
            return false;
        }

        /// <summary>Counts time spent kneeling at a task. True once it's done.</summary>
        bool Work(Unit u, float dt, float seconds)
        {
            u.Path = null;
            u.Posture = Posture.Kneeling;
            u.TaskSeconds += dt;
            return u.TaskSeconds >= seconds;
        }

        /// <summary>When everyone in a group has finished, the group reports once.</summary>
        void GroupDone(Unit u, Order o, string text, string kind = null, Vec2? at = null)
        {
            if (o.Group == 0) { Radio(u, text, kind, at); return; }
            if (units.Any(x => x.Order != null && x.Order.Group == o.Group)) return;
            if (reported.Add(o.Group)) Radio(u, text, kind, at);
        }

        string Callsign(Unit u) => string.IsNullOrEmpty(u.Team) ? u.Name : u.Team;

        /// <summary>
        /// A fire position: the first time a group set on overwatch is all in place, or opens fire on an order, it calls it,
        /// and the call marks where they're firing from.
        /// </summary>
        void FirePosition(Unit u, Order o, string text)
        {
            if (!ReferenceEquals(u.Force, PlayerForce) || o.Announced) return;
            if (o.Group == 0) { o.Announced = true; Radio(u, u.Name + ": " + text, "fire", u.Position); return; }
            var group = units.Where(x => x.CanAct && x.Order != null && x.Order.Group == o.Group).ToList();
            if (o.Verb == Verbs.Overwatch && group.Any(x => !x.Order.Arrived)) return;
            foreach (var x in group) x.Order.Announced = true;
            o.Announced = true;
            var at = new Vec2(group.Average(x => x.Position.X), group.Average(x => x.Position.Y));
            Radio(u, Callsign(u) + ": " + text, "fire", at);
        }

        // ---------- bound: one half rushes while the other covers ----------

        void ThinkBound(Unit u, Order o)
        {
            if (o.Group == 0) return; // alone, it's a rush along the route
            var members = units.Where(x => x.CanAct && x.Order != null && x.Order.Group == o.Group).ToList();
            if (members.All(x => x.Order.Arrived))
            {
                foreach (var x in members) Finish(x, null);
                GroupDone(u, o, Callsign(u) + ": Set" + (string.IsNullOrWhiteSpace(o.Where) ? "." : " at " + o.Where + "."), "set");
                return;
            }
            if (o.Arrived) return;
            if (!boundMoving.TryGetValue(o.Group, out int moving)) boundMoving[o.Group] = moving = 0;
            // When the moving half has all finished its rush (or arrived), the other half goes.
            if (members.Where(x => x.Order.Half == moving).All(x => x.Order.LegDone || x.Order.Arrived))
            {
                moving = 1 - moving;
                boundMoving[o.Group] = moving;
                foreach (var x in members.Where(x => x.Order.Half == moving)) { x.Order.LegDone = false; x.Order.LegMeters = 0f; x.Order.LastPos = x.Position; }
                if (members.Where(x => x.Order.Half == moving).All(x => x.Order.Arrived)) return;
            }
            if (o.Half == moving && !o.LegDone)
            {
                if (u.Path == null && o.SavedPath != null) { u.Path = o.SavedPath; u.PathIndex = o.SavedIndex; o.SavedPath = null; o.LastPos = u.Position; }
                o.LegMeters += Vec2.Distance(u.Position, o.LastPos);
                o.LastPos = u.Position;
                if (o.LegMeters >= BoundLeg) { Pause(u, o); o.LegDone = true; }
            }
            else if (u.Path != null) Pause(u, o);
        }

        void Pause(Unit u, Order o)
        {
            if (u.Path != null) { o.SavedPath = u.Path; o.SavedIndex = u.PathIndex; }
            u.Path = null;
            u.IsMoving = false;
            u.Posture = Posture.Kneeling;
        }

        // ---------- smoke ----------

        void ThrowSmoke(Unit u, Order o)
        {
            // One smoke screens a team; the first Marine with one throws it.
            if (u.Smokes > 0 && o.Point != null && (o.Group == 0 || smoked.Add(o.Group)))
            {
                u.Smokes--;
                var cloud = new SmokeCloud { Center = Clamp(o.Point.Value), Radius = SmokeRadius, Height = SmokeHeight, UntilStep = Clock.Step + SmokeSeconds * SimClock.StepsPerSecond };
                Map.Smoke.Add(cloud);
                Record(new SmokeAt(u, cloud));
                Radio(u, u.Name + ": Smoke out.", "smoke", cloud.Center);
            }
            Finish(u, null);
        }

        void EverySecond()
        {
            Map.Smoke.RemoveAll(c => c.UntilStep <= Clock.Step);
            CasevacTick();
            ThinkPlans();
            ThinkSurrender();
            IedTick();
            RadioTick();
            LieTick();
        }

        // ---------- clear a building ----------

        void ThinkClear(Unit u, Order o, float dt)
        {
            var b = o.Building;
            if (!b.Contains(u.Position))
            {
                if (u.Path == null) { u.Path = Paths.Find(u.Position, b.Center, false); u.PathIndex = 0; }
                return;
            }
            u.Path = null;
            u.Posture = Posture.Standing;
            // Going room to room, whoever is inside is found.
            foreach (var x in units)
                if (!ReferenceEquals(x.Force, u.Force) && !x.Gone && x.Condition != Condition.Dead && b.Contains(x.Position)) Fog.Spot(this, u.Force, x, u);
            if (units.Any(x => x.CanAct && AreHostile(u.Force, x.Force) && b.Contains(x.Position))) return;
            u.TaskSeconds += dt;
            float area = (b.Max.X - b.Min.X) * (b.Max.Y - b.Min.Y);
            if (u.TaskSeconds < RoomSeconds * Math.Max(1f, (float)Math.Round(area / 25f))) return;
            Finish(u, null);
            GroupDone(u, o, Callsign(u) + ": " + Capital(b.Label) + " clear.", "set", b.Center);
        }

        static string Capital(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);

        // ---------- breach a wall, hedge, or building ----------

        void ThinkBreach(Unit u, Order o, float dt)
        {
            Vec2 at = o.Watch.Value;
            if (Map.TerrainAt(at).Id == "rubble") { Finish(u, null); return; }
            if (Vec2.Distance(u.Position, at) > 3f)
            {
                if (u.Path == null) { u.Path = Paths.Find(u.Position, at, false); u.PathIndex = 0; }
                return;
            }
            if (!Work(u, dt, BreachSeconds)) return;
            const float r = 1.5f;
            Map.CellOf(at, out int cx, out int cy);
            int span = (int)Math.Ceiling(r / Map.CellSize);
            for (int y = cy - span; y <= cy + span; y++)
                for (int x = cx - span; x <= cx + span; x++)
                    if (Map.InBounds(x, y) && Vec2.Distance(Map.CellCenter(x, y), at) <= r && Breachable(Map.TerrainAt(x, y))) Map.SetTerrain(x, y, "rubble");
            pathfinder = null;
            Record(new BreachMade(u, at, r));
            Finish(u, null);
            GroupDone(u, o, Callsign(u) + ": Breach is open.", "info", at);
        }

        /// <summary>What ammo a weapon takes: its caliber, or its own id when none is set.</summary>
        string Caliber(string weapon)
        {
            var w = Weapon(weapon);
            return string.IsNullOrEmpty(w?.Caliber) ? weapon ?? "" : w.Caliber;
        }

        static bool Breachable(TerrainDef t) => t.Id == "wall" || t.Id == "hedge" || t.Id == "building" || t.Id == "cliff";

        /// <summary>
        /// Where to breach: the nearest wall, hedge, building, or cliff within 15 meters of a point, or, when a grid
        /// square was named, the one in that square nearest whoever is breaching. Null if there's nothing to breach.
        /// </summary>
        Vec2? FindBreachPoint(Vec2 near, Vec2 from, GridRef? square)
        {
            Map.CellOf(near, out int cx, out int cy);
            Vec2? best = null;
            float bestD = float.MaxValue;
            int span = (int)Math.Ceiling((square != null ? Map.SquareSize * 0.75f : 15f) / Map.CellSize);
            for (int y = cy - span; y <= cy + span; y++)
                for (int x = cx - span; x <= cx + span; x++)
                {
                    if (!Map.InBounds(x, y) || !Breachable(Map.TerrainAt(x, y))) continue;
                    var p = Map.CellCenter(x, y);
                    if (square != null && !Map.SquareContains(square.Value, p)) continue;
                    // Prefer the side facing whoever is breaching.
                    float d = square != null ? Vec2.Distance(p, from) : Vec2.Distance(p, near) + 0.5f * Vec2.Distance(p, from);
                    if (d < bestD) { bestD = d; best = p; }
                }
            return best;
        }

        // ---------- the drone ----------

        /// <summary>The drone this unit carries, if it has one.</summary>
        public Unit DroneOf(Unit op) => units.FirstOrDefault(d => d.Def.Flying && ReferenceEquals(d.Operator, op));

        void ThinkDrone(Unit u, Order o, float dt)
        {
            var d = DroneOf(u);
            if (d == null) { Finish(u, null); return; }
            u.Path = null;
            u.Posture = Posture.Kneeling;
            if (o.Point == null)
            {
                // Bring it home and pack it up.
                if (d.Gone) { u.Emote = null; Finish(u, null); return; }
                d.Path = new List<Vec2> { u.Position };
                d.PathIndex = 0;
                if (Vec2.Distance(d.Position, u.Position) <= 2f) { LandDrone(d); u.Emote = null; Radio(u, u.Name + ": Drone's down and packed."); Finish(u, null); }
                return;
            }
            u.Emote = "controller";
            u.EmoteUntil = Clock.Step + 2 * SimClock.StepsPerSecond;
            if (d.Gone)
            {
                u.TaskSeconds += dt;
                if (u.TaskSeconds < LaunchSeconds) return;
                d.Gone = false;
                d.Position = u.Position;
                d.TaskSeconds = 0f;
                Radio(u, u.Name + ": Drone's up.");
            }
            if (o.Phase == 0)
            {
                d.Path = new List<Vec2> { o.Point.Value };
                d.PathIndex = 0;
                o.Phase = 1;
            }
        }

        void LandDrone(Unit d)
        {
            d.Gone = true;
            d.Path = null;
            d.IsMoving = false;
        }

        /// <summary>The drone's battery: after its time is up, it flies home to its operator and lands.</summary>
        void FlyTick(Unit d, float dt)
        {
            d.TaskSeconds += dt;
            var op = d.Operator;
            if (d.TaskSeconds < DroneMinutes * 60f) return;
            if (d.TaskSeconds - dt < DroneMinutes * 60f && op != null) Radio(op, op.Name + ": Drone's low on battery. Bringing it home.");
            Vec2 home = op?.Position ?? d.Position;
            d.Path = new List<Vec2> { home };
            d.PathIndex = 0;
            if (Vec2.Distance(d.Position, home) <= 2f)
            {
                LandDrone(d);
                if (op != null && op.Order != null && op.Order.Verb == Verbs.FlyDrone) { op.Emote = null; Finish(op, null); }
            }
        }

        /// <summary>Drones fly straight over everything at their own speed and hover when they get there.</summary>
        void Fly(Unit d, float dt)
        {
            if (d.Path == null || d.PathIndex >= d.Path.Count) { d.Path = null; d.IsMoving = false; return; }
            Vec2 target = d.Path[d.PathIndex], delta = target - d.Position;
            float dist = delta.Length, step = d.Def.WalkSpeed * dt;
            if (dist > 0.01f) d.Facing = Bearing(d.Position, target);
            if (dist <= step) { d.Position = target; d.PathIndex++; }
            else d.Position = d.Position + delta * (step / dist);
            d.IsMoving = true;
        }

        // ---------- talk ----------

        /// <summary>Whoever was talked to tells what they've seen of the enemy, and the squad now knows it too.</summary>
        void ShareWhatTheySaw(Unit u, Unit p)
        {
            var mine = Fog.For(u.Force);
            var told = new List<string>();
            foreach (var c in Fog.For(p.Force).Contacts)
            {
                if (!AreHostile(u.Force, c.Unit.Force) || c.Unit.Gone || c.Unit.Condition == Condition.Dead) continue;
                var k = mine.Track(c.Unit);
                if (!k.Known || c.LastSeenStep > k.LastSeenStep)
                {
                    k.Known = true;
                    k.LastKnownPosition = c.LastKnownPosition;
                    k.LastSeenStep = c.LastSeenStep;
                }
                told.Add(Article(c.Unit.Def.Name ?? "fighter") + " at " + Map.SquareOf(c.LastKnownPosition));
            }
            string through = SameLanguage(u.Force, p.Force) ? "" : ", through the interpreter,";
            Radio(u, u.Name + ": " + p.Name + through + (told.Count == 0 ? " didn't see anything." : " says they saw " + JoinAnd(told) + "."), "info", p.Position, p.Name);
        }

        static string Article(string kind)
        {
            string k = kind.StartsWith("Enemy ", StringComparison.Ordinal) ? kind.Substring(6) : kind;
            k = k.ToLowerInvariant();
            return ("aeiou".IndexOf(k[0]) >= 0 ? "an " : "a ") + k;
        }

        static string JoinAnd(List<string> items) =>
            items.Count == 1 ? items[0] : string.Join(", ", items.Take(items.Count - 1)) + " and " + items[items.Count - 1];

        // ---------- search ----------

        void ThinkSearch(Unit u, Order o, float dt)
        {
            var p = o.TargetUnit;
            if (p != null)
            {
                if (!Reach(u, o, p, 2f) || !Work(u, dt, SearchSeconds)) return;
                if (AreHostile(u.Force, p.Force))
                {
                    var gun = Weapon(p.Def.Weapon);
                    bool had = p.Ammo > 0 || p.LauncherAmmo > 0 || p.LauncherAp > 0 || p.Grenades > 0;
                    p.Ammo = 0; p.LauncherAmmo = 0; p.LauncherAp = 0; p.Grenades = 0;
                    Radio(u, u.Name + ": Searched " + p.Name + "." + (had ? " Took their " + (gun?.Name ?? "weapon") + " and ammo." : " Nothing on them."), "info", p.Position);
                }
                else Radio(u, u.Name + ": Searched " + p.Name + ". Nothing on them.", "info", p.Position);
                Finish(u, null);
                return;
            }
            if (u.Path != null) return;
            if (o.Point != null && Vec2.Distance(u.Position, o.Point.Value) > 6f) { u.Path = Paths.Find(u.Position, o.Point.Value, o.Swim); u.PathIndex = 0; if (u.Path != null) return; }
            if (!Work(u, dt, SearchSeconds)) return;
            // A careful look turns up anyone close by who can be seen at all, hidden or not.
            var found = new List<string>();
            foreach (var x in units)
            {
                if (ReferenceEquals(x.Force, u.Force) || x.Gone || x.Def.Flying || x.Condition == Condition.Dead) continue;
                if (Vec2.Distance(u.Position, x.Position) > 40f || !Sight.Between(Map, u, x).SeenBy(u)) continue;
                Fog.Spot(this, u.Force, x, u);
                found.Add(Article(x.Def.Name ?? "someone") + " at " + Map.SquareOf(x.Position));
            }
            Radio(u, u.Name + ": Search done. " + (found.Count == 0 ? "Nothing here." : "Found " + JoinAnd(found) + "."), "info");
            Finish(u, null);
        }

        // ---------- carry a casualty ----------

        /// <summary>The casualty this unit is carrying, or null.</summary>
        Unit Carrying(Unit u) => units.FirstOrDefault(x => ReferenceEquals(x.CarriedBy, u));

        /// <summary>Marines helping carry: also ordered to carry this casualty, and right alongside.</summary>
        int CarryHelpers(Unit carrier, Unit casualty) =>
            units.Count(x => !ReferenceEquals(x, carrier) && x.CanAct && x.Order != null && x.Order.Verb == Verbs.Carry && ReferenceEquals(x.Order.TargetUnit, casualty) && Vec2.Distance(x.Position, carrier.Position) <= 3f);

        void ThinkCarry(Unit u, Order o)
        {
            var c = o.TargetUnit;
            if (c == null || c.Gone || c.Condition == Condition.Dead) { Finish(u, null); return; }
            if (ReferenceEquals(c.CarriedBy, u)) return;
            if (c.CarriedBy != null)
            {
                // Someone has them: help, right alongside.
                var lead = c.CarriedBy;
                if (Vec2.Distance(u.Position, lead.Position) > 2.5f && (u.Path == null || Clock.Step % (2 * SimClock.StepsPerSecond) == 0))
                {
                    u.Path = Paths.Find(u.Position, lead.Position, false);
                    u.PathIndex = 0;
                    u.Speed = MoveSpeed.Run;
                }
                else if (Vec2.Distance(u.Position, lead.Position) <= 2.5f) { u.Path = null; u.Speed = MoveSpeed.Walk; }
                return;
            }
            if (!Reach(u, o, c, 2f)) return;
            c.CarriedBy = u;
            c.Path = null;
            u.Speed = MoveSpeed.Walk;
            u.Path = Paths.Find(u.Position, o.Point.Value, false);
            u.PathIndex = 0;
            if (u.Path == null) { c.CarriedBy = null; Finish(u, u.Name + ": Can't get " + c.Name + " there, Lead."); }
        }

        /// <summary>The carrier got there: set the casualty down, and everyone helping is done too.</summary>
        void SetDown(Unit u, Order o)
        {
            var c = o.TargetUnit;
            if (c == null || !ReferenceEquals(c.CarriedBy, u)) return;
            c.CarriedBy = null;
            foreach (var x in units.Where(x => x.Order != null && x.Order.Verb == Verbs.Carry && ReferenceEquals(x.Order.TargetUnit, c)).ToList()) Finish(x, null);
            Radio(u, Callsign(u) + ": " + c.Name + " is at " + (o.Where ?? Map.SquareOf(u.Position).ToString()) + ".", "casualty", c.Position);
        }

        // ---------- share ammo ----------

        void ThinkShareAmmo(Unit u, Order o, float dt)
        {
            if (!Work(u, dt, ShareSeconds)) return;
            var group = o.Group == 0 ? new List<Unit> { u } :
                units.Where(x => x.CanAct && x.Order != null && x.Order.Group == o.Group && Vec2.Distance(x.Position, u.Position) <= 25f).ToList();
            // Ammo only goes to someone whose weapon takes it: rifle rounds by caliber, and launcher rounds between launchers.
            float main = 0f;
            foreach (var kind in group.GroupBy(x => Caliber(x.Def.Weapon)))
            {
                var same = kind.ToList();
                int total = same.Sum(x => x.Ammo), full = same.Sum(x => x.Def.Ammo);
                float share = full > 0 ? total / (float)full : 0f;
                foreach (var x in same) x.Ammo = (int)Math.Floor(x.Def.Ammo * share);
                if (same.Contains(u)) main = share;
            }
            foreach (var kind in group.Where(x => x.Def.Launcher != null).GroupBy(x => Caliber(x.Def.Launcher)))
            {
                var same = kind.ToList();
                int he = same.Sum(x => x.LauncherAmmo), ap = same.Sum(x => x.LauncherAp);
                for (int i = 0; i < same.Count; i++)
                {
                    same[i].LauncherAmmo = he / same.Count + (i < he % same.Count ? 1 : 0);
                    same[i].LauncherAp = ap / same.Count + (i < ap % same.Count ? 1 : 0);
                }
            }
            foreach (var x in group) Finish(x, null);
            GroupDone(u, o, Callsign(u) + ": Ammo's spread out. Everyone's at " + Math.Round(main * 100) + "%.");
        }

        // ---------- CASEVAC ----------

        /// <summary>Lead calls in CASEVAC; Doc fills in the 9-line from what's known.</summary>
        void CallCasevac(Unit u, Order o)
        {
            Finish(u, null);
            if (Evac != null || o.Point == null) return;
            Vec2 lz = Clamp(o.Point.Value);
            var doc = units.FirstOrDefault(x => ReferenceEquals(x.Force, u.Force) && x.CanAct && x.Def.Medic) ?? u;
            Radio(doc, (doc.Def.Medic ? "Doc" : doc.Name) + ": " + NineLine(u.Force, lz), "lz", lz);
            int minutes = CasevacMinMinutes - 1 + Roll(CasevacMaxMinutes - CasevacMinMinutes + 1, "Dustoff", "minutes until CASEVAC arrives");
            bool road = Map.TerrainAt(lz).Id == "road" || Map.TerrainAt(lz).Id == "trail";
            Evac = new Casevac { Lz = lz, Helicopter = !road, ArriveStep = Clock.Step + minutes * 60 * SimClock.StepsPerSecond };
            Radio(null, "Dustoff: Copy your 9-line. " + (road ? "Ambulance" : "Bird") + " is " + minutes + " minutes out to LZ " + Map.SquareOf(lz) + ".", "lz", lz);
        }

        /// <summary>The CASEVAC 9-line, built from what the squad knows right now.</summary>
        public string NineLine(Force force, Vec2 lz)
        {
            var hurt = units.Where(x => ReferenceEquals(x.Force, force) && !x.Gone && !x.Def.Flying && (x.Condition == Condition.Down || x.Condition == Condition.Wounded)).ToList();
            int urgent = hurt.Count(x => x.Condition == Condition.Down && !x.Stabilized);
            int priority = hurt.Count(x => x.Condition == Condition.Down && x.Stabilized);
            int routine = hurt.Count(x => x.Condition == Condition.Wounded);
            int litter = hurt.Count(x => x.Condition == Condition.Down || x.LegHit);
            var known = Fog.For(force).Contacts.Where(c => AreHostile(force, c.Unit.Force) && c.Unit.CanAct).ToList();
            string security = known.Any(c => c.Visible && Vec2.Distance(c.LastKnownPosition, lz) <= 300f) ? "X, hot LZ, armed escort needed"
                : known.Any(c => Vec2.Distance(c.LastKnownPosition, lz) <= 500f) ? "E, enemy in the area"
                : known.Count > 0 ? "P, possible enemy" : "N, no enemy known";
            var t = Map.TerrainAt(lz);
            bool trees = false, buildings = false;
            for (int a = 0; a < 16; a++)
            {
                var p = lz + new Vec2((float)Math.Cos(a * Math.PI / 8), (float)Math.Sin(a * Math.PI / 8)) * 20f;
                if (!Map.InBounds(p)) continue;
                var q = Map.TerrainAt(p);
                trees |= q.Id == "forest" || q.Id == "orchard";
                buildings |= q.Id == "building";
            }
            string obstacles = trees && buildings ? ", trees and buildings close by" : trees ? ", trees close by" : buildings ? ", buildings close by" : ", nothing in the way";
            return "CASEVAC 9-line. 1: LZ " + Map.SquareOf(lz) + ". 2: " + force.Name + " on the squad net. 3: " + urgent + " urgent, " + priority + " priority, " + routine + " routine. 4: None. 5: "
                + litter + " litter, " + (hurt.Count - litter) + " ambulatory. 6: " + security + ". 7: Smoke. 8: " + hurt.Count + " US military. 9: " + t.Name + obstacles + ".";
        }

        void CasevacTick()
        {
            var e = Evac;
            if (e == null) return;
            if (!e.Landed)
            {
                if (Clock.Step < e.ArriveStep) return;
                e.Landed = true;
                e.LandedStep = Clock.Step;
                Record(new CasevacMoved(e.Lz, true, e.Helicopter));
                Radio(null, "Dustoff: " + (e.Helicopter ? "Bird" : "Ambulance") + " is at LZ " + Map.SquareOf(e.Lz) + ". Bring them to us.", "lz", e.Lz);
            }
            // Load every casualty who can't walk out, within reach of the landing zone.
            foreach (var x in units)
            {
                if (!ReferenceEquals(x.Force, PlayerForce) || x.Gone || x.Def.Flying || x.Condition == Condition.Dead) continue;
                if (!(x.Condition == Condition.Down || x.LegHit) || Vec2.Distance(x.Position, e.Lz) > CasevacLoadRadius) continue;
                x.CarriedBy = null;
                x.Gone = true;
                x.Evacuated = true;
                x.Stabilized = true;
                x.Order = null;
                x.Path = null;
                e.Loaded++;
                Record(new UnitEvacuated(x));
            }
            if (Clock.Step - e.LandedStep < CasevacWaitSeconds * SimClock.StepsPerSecond) return;
            Record(new CasevacMoved(e.Lz, false, e.Helicopter));
            Radio(null, "Dustoff: " + (e.Loaded == 0 ? "Nobody at the LZ. Leaving." : "Leaving with " + e.Loaded + (e.Loaded == 1 ? " casualty." : " casualties.")), "lz", e.Lz);
            Evac = null;
        }
    }

    /// <summary>A CASEVAC called in: where it lands, when, whether it's a helicopter or a ground ambulance, and who it took.</summary>
    public sealed class Casevac
    {
        public Vec2 Lz { get; internal set; }
        public bool Helicopter { get; internal set; }
        public int ArriveStep { get; internal set; }
        public bool Landed { get; internal set; }
        public int LandedStep { get; internal set; }
        public int Loaded { get; internal set; }
    }
}
