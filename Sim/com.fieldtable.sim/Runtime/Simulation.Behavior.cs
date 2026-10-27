using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    public sealed partial class Simulation
    {
        /// <summary>Carry more than this many kilograms into deep water and you go under.</summary>
        public const float LoadToSwim = 15f;

        /// <summary>What's left after dropping gear to swim: uniform, rifle, a little ammo.</summary>
        public const float StrippedLoad = 12f;

        /// <summary>Seconds in deep water, carrying too much, before someone drowns.</summary>
        public const float SecondsToDrown = 20f;

        /// <summary>Minutes someone who's down lasts without treatment.</summary>
        public const int MinutesToBleedOut = 15;

        /// <summary>Seconds to treat someone: a corpsman, and anyone else.</summary>
        public const float MedicSeconds = 60f, BuddySeconds = 180f;

        /// <summary>Minutes someone hit in the chest or belly lasts once down without treatment.</summary>
        public const int MinutesToBleedOutTorso = 10;

        /// <summary>A helmet stops a hit to the head on this or less, out of 20.</summary>
        public const int HelmetSaves = 3;

        const float SuppressionFadePerSecond = 0.08f;

        /// <summary>Someone with nothing to shoot at looks for a target again this often, in seconds.</summary>
        const float LookAgain = 0.5f;
        const float PinnedAt = 0.8f;

        readonly Dictionary<string, WeaponDef> weapons = new Dictionary<string, WeaponDef>();

        /// <summary>The force the player commands. Its units don't run the enemy brain.</summary>
        public Force PlayerForce { get; set; }

        public void AddWeapons(IEnumerable<WeaponDef> list)
        {
            foreach (var w in list) weapons[w.Id] = w;
        }

        WeaponDef Weapon(string id) => id != null && weapons.TryGetValue(id, out var w) ? w : null;

        // ---------- one step for everyone ----------

        void StepUnits(float dt)
        {
            foreach (var u in units)
            {
                if (u.Condition == Condition.Dead || u.Gone) { u.IsMoving = false; continue; }
                if (u.Def.Flying) { FlyTick(u, dt); Move(u, dt); continue; }
                BleedOut(u);
                if (!u.CanAct) { u.IsMoving = false; continue; }
                if (u.Order == null && !ReferenceEquals(u.Force, PlayerForce) && !PlanBrain(u, dt)) Brain(u);
                if (u.Order == null && u.Def.Interpreter && ReferenceEquals(u.Force, PlayerForce)) StayWithLead(u);
                Think(u, dt);
                Move(u, dt);
            }
            // Casualties being carried go where their carriers go.
            foreach (var u in units)
            {
                var c = u.CarriedBy;
                if (c == null) continue;
                if (!c.CanAct || u.Gone || u.Condition == Condition.Dead) { u.CarriedBy = null; continue; }
                u.Position = Clamp(c.Position + new Vec2(0.5f, 0.6f));
                u.Facing = c.Facing;
            }
            foreach (var u in units) Fight(u, dt);
            if (Clock.Step % SimClock.StepsPerSecond == 0) EverySecond();
            foreach (var u in units)
            {
                u.SetSuppression(u.Suppression - SuppressionFadePerSecond * dt);
                u.IsFiring = Clock.Step - u.FiredAtStep < 6;
                if (u.Emote != null && Clock.Step >= u.EmoteUntil) u.Emote = null;
            }
        }

        // ---------- orders ----------

        void Think(Unit u, float dt)
        {
            var o = u.Order;
            if (o == null)
            {
                // No orders: get down when the rounds come close.
                if (u.Suppression >= 0.5f && u.Posture == Posture.Standing && !Map.TerrainAt(u.Position).SeeOut)
                    u.Posture = Posture.Kneeling;
                if (u.Suppression >= 0.7f) u.Posture = Posture.Prone;
                return;
            }
            switch (o.Verb)
            {
                case Verbs.Hold:
                    u.Path = null;
                    Finish(u, null);
                    break;

                case Verbs.ChangePosture:
                    u.Posture = ParsePosture(o.Posture) ?? u.Posture;
                    u.Path = null;
                    Finish(u, null);
                    break;

                case Verbs.Observe:
                    u.Path = null;
                    if (o.Point != null) u.Facing = Bearing(u.Position, o.Point.Value);
                    break;

                case Verbs.Follow:
                    if (o.TargetUnit == null || o.TargetUnit.Condition == Condition.Dead) { Finish(u, null); break; }
                    if (Clock.Step % (5 * SimClock.StepsPerSecond) == 0)
                    {
                        float d = Vec2.Distance(u.Position, o.TargetUnit.Position);
                        if (d > 8f)
                        {
                            u.Path = Paths.Find(u.Position, o.TargetUnit.Position, o.Swim);
                            u.PathIndex = 0;
                        }
                        else u.Path = null;
                    }
                    break;

                case Verbs.TreatCasualty:
                    var patient = o.TargetUnit;
                    if (patient == null || patient.Condition == Condition.Dead || patient.Condition == Condition.Ok || (patient.Condition == Condition.Down && patient.Stabilized && patient.Wounds == 0))
                    {
                        Finish(u, null);
                        break;
                    }
                    if (Vec2.Distance(u.Position, patient.Position) <= 2.5f)
                    {
                        u.Path = null;
                        u.Posture = Posture.Kneeling;
                        u.TaskSeconds += dt;
                        if (u.TaskSeconds >= (u.Def.Medic ? MedicSeconds : BuddySeconds))
                        {
                            if (patient.Condition == Condition.Down) patient.Stabilized = true;
                            else { patient.Wounds = 0; patient.Condition = Condition.Ok; }
                            Record(new UnitTreated(u, patient));
                            // Patching up doesn't fix a bad arm or leg: it still shoots at half, or still can't walk.
                            string state = patient.Condition == Condition.Down ? "stable." : patient.LegHit ? "patched up, but can't walk." : patient.ArmHit ? "patched up. Arm's still bad." : "patched up and good to go.";
                            Radio(u, (u.Def.Medic ? "Doc" : u.Name) + ": " + patient.Name + " is " + state, "info", patient.Position);
                            Finish(u, null);
                        }
                    }
                    else if (Clock.Step % (5 * SimClock.StepsPerSecond) == 0 || u.Path == null)
                    {
                        u.Path = Paths.Find(u.Position, patient.Position, o.Swim);
                        u.PathIndex = 0;
                    }
                    break;

                case Verbs.ThrowGrenade:
                    if (o.Point != null && u.Grenades > 0)
                    {
                        u.Grenades--;
                        Blast(u, Weapon("grenade") ?? new WeaponDef { Id = "grenade", Name = "Hand grenade", Accuracy = 0.45f, Blast = 5f, Suppress = 0.6f, Lethality = 0.5f, Range = 30f }, o.Point.Value);
                    }
                    Finish(u, null);
                    break;

                case Verbs.Drop:
                    if (u.Load > StrippedLoad)
                    {
                        u.Load = StrippedLoad;
                        Radio(u, u.Name + ": Dropped my gear.");
                    }
                    Finish(u, null);
                    break;

                case Verbs.Emote:
                    u.Emote = string.IsNullOrWhiteSpace(o.Kind) ? "wave" : o.Kind.Trim().ToLowerInvariant();
                    u.EmoteUntil = Clock.Step + 10 * SimClock.StepsPerSecond;
                    u.Path = null;
                    Finish(u, null);
                    break;

                case Verbs.Report:
                    Finish(u, null);
                    break;

                default:
                    ThinkMore(u, o, dt);
                    break;
            }
        }

        void Finish(Unit u, string radio)
        {
            var o = u.Order;
            if (o == null) return;
            o.Done = true;
            u.Order = null;
            if (radio != null) Radio(u, radio);
        }

        void Arrive(Unit u)
        {
            var o = u.Order;
            if (o == null) return;
            switch (o.Verb)
            {
                case Verbs.Move:
                    if (ParsePosture(o.Posture) is Posture p) u.Posture = p;
                    else if (u.Speed == MoveSpeed.Crawl) u.Posture = Posture.Prone;
                    Finish(u, null);
                    TeamSet(u, o);
                    break;
                case Verbs.Withdraw:
                    u.Posture = Posture.Prone;
                    Finish(u, null);
                    TeamSet(u, o);
                    break;
                case Verbs.TakeCover:
                    u.Posture = Posture.Kneeling;
                    Finish(u, null);
                    break;
                case Verbs.Hide:
                    u.Posture = Posture.Prone;
                    Finish(u, null);
                    break;
                case Verbs.Assault:
                    u.Posture = Posture.Kneeling;
                    Finish(u, null);
                    TeamSet(u, o);
                    break;
                case Verbs.Bound:
                    o.Arrived = true;
                    u.Posture = Posture.Kneeling;
                    if (o.Group == 0) Finish(u, null);
                    break;
                case Verbs.Overwatch:
                    u.Posture = Posture.Kneeling;
                    break;
                case Verbs.Carry:
                    SetDown(u, o);
                    break;
            }
        }

        /// <summary>When the last of a team finishes the same move, its leader reports set.</summary>
        void TeamSet(Unit u, Order done)
        {
            if (!ReferenceEquals(u.Force, PlayerForce) || done.Group == 0) return;
            if (units.Any(x => x.Order != null && x.Order.Group == done.Group)) return;
            string who = string.IsNullOrEmpty(u.Team) ? u.Name : u.Team;
            Radio(u, who + ": Set" + (string.IsNullOrWhiteSpace(done.Where) ? "." : " at " + done.Where + "."), "set");
        }

        // ---------- the enemy's own brain ----------

        /// <summary>
        /// A simple coded enemy: holds its ground and fights from cover, gets down under fire, and pulls back
        /// when it's hurt and pinned or when its side has lost half its people. The in-game AI steers this later.
        /// </summary>
        void Brain(Unit u)
        {
            if (u.Force.Civilian)
            {
                if (u.Suppression > 0.2f && !Map.TerrainAt(u.Position).SeeOut) u.Posture = Posture.Prone;
                return;
            }
            var side = units.Where(x => ReferenceEquals(x.Force, u.Force)).ToList();
            float lost = side.Count(x => !x.CanAct) / (float)Math.Max(1, side.Count);
            if ((u.Wounds >= 1 && u.Suppression >= 0.6f) || lost >= BreakingPoint(u.Force))
            {
                var threats = Fog.For(u.Force).Contacts.Where(c => AreHostile(u.Force, c.Unit.Force)).Select(c => c.LastKnownPosition).ToList();
                if (threats.Count > 0)
                {
                    Vec2 away = u.Position - Average(threats);
                    float len = away.Length;
                    Vec2 dir = len > 0.1f ? away * (1f / len) : new Vec2(0, -1);
                    var o = new Order { Verb = Verbs.Withdraw, Speed = "run" };
                    o.Point = Clamp(u.Position + dir * 200f);
                    var path = Paths.Find(u.Position, o.Point.Value, false);
                    if (path != null)
                    {
                        u.Order = o;
                        u.Path = path;
                        u.PathIndex = 0;
                        u.Speed = MoveSpeed.Run;
                        return;
                    }
                }
            }
            if (u.Suppression >= 0.4f) u.Posture = Posture.Prone;
        }

        // ---------- moving ----------

        void Move(Unit u, float dt)
        {
            if (u.Def.Flying) { Fly(u, dt); return; }
            if (u.CarriedBy != null || (u.LegHit && u.Path != null))
            {
                u.Path = null;
                u.IsMoving = false;
                return;
            }
            if (u.Path == null || u.PathIndex >= u.Path.Count)
            {
                u.IsMoving = false;
                u.Path = null;
                CheckWater(u, dt);
                return;
            }
            if (u.Suppression >= PinnedAt && u.Speed != MoveSpeed.Crawl)
            {
                // Pinned: nobody gets up and walks into that.
                u.IsMoving = false;
                u.Posture = Posture.Prone;
                CheckWater(u, dt);
                return;
            }

            float speed = u.Speed == MoveSpeed.Run ? u.Def.RunSpeed : u.Speed == MoveSpeed.Crawl ? u.Def.CrawlSpeed : u.Def.WalkSpeed;
            if (u.Speed == MoveSpeed.Crawl) u.Posture = Posture.Prone;
            else u.Posture = Posture.Standing;
            var here = Map.TerrainAt(u.Position);
            float factor = here.MoveFactor;
            if (u.Condition == Condition.Wounded) factor *= 0.6f;
            // Carrying a casualty: half speed with a helper, a quarter alone, and no running.
            // A prisoner who can walk is only led, at a walk.
            if (Carrying(u) is Unit load)
            {
                speed = u.Def.WalkSpeed;
                if (!Walks(load)) factor *= CarryHelpers(u, load) > 0 ? 0.5f : 0.25f;
            }
            factor *= 1f - 0.5f * u.Suppression;
            float budget = Math.Max(0.02f, speed * factor) * dt;

            while (budget > 0f && u.PathIndex < u.Path.Count)
            {
                Vec2 target = u.Path[u.PathIndex];
                Vec2 delta = target - u.Position;
                float dist = delta.Length;
                if (dist < 0.01f) { u.PathIndex++; continue; }
                // Climbing slows you down.
                float climb = Map.InBounds(target) ? Math.Max(0f, Map.GroundAt(target) - Map.GroundAt(u.Position)) / Math.Max(dist, 1f) : 0f;
                float step = Math.Min(dist, budget / (1f + 3f * climb));
                // A budget too small to move on rounds the step to zero; stop rather than loop forever.
                if (step <= 0f) break;
                u.Facing = Bearing(u.Position, target);
                u.Position = u.Position + delta * (step / dist);
                budget -= step * (1f + 3f * climb);
                if (step >= dist - 0.001f) u.PathIndex++;
            }
            u.IsMoving = true;
            CheckWater(u, dt);
            if (u.Path == null) return;
            if (u.PathIndex >= u.Path.Count)
            {
                u.Path = null;
                u.IsMoving = false;
                Arrive(u);
            }
        }

        /// <summary>True for a prisoner on his own two feet: led, not carried.</summary>
        static bool Walks(Unit u) => u.Detained && u.Condition != Condition.Down && u.Condition != Condition.Dead && !u.LegHit;

        void CheckWater(Unit u, float dt)
        {
            if (!Map.TerrainAt(u.Position).Deep) { u.DeepSeconds = 0f; return; }
            if (u.DeepSeconds == 0f && u.Load > LoadToSwim)
            {
                Record(new UnitInTrouble(u, "is going under"));
                Radio(u, u.Name + " is going under!", "casualty");
            }
            u.DeepSeconds += dt;
            if (u.Load > LoadToSwim && u.DeepSeconds >= SecondsToDrown) Kill(u, "drowned");
        }

        // ---------- fighting ----------

        void Fight(Unit u, float dt)
        {
            if (!u.CanAct || u.Def.Weapon == null || u.Asleep) return;
            u.FireCooldown -= dt;
            if (u.FireCooldown > 0f) return;
            if (u.Suppression >= PinnedAt) return;
            if (u.IsMoving && u.Speed == MoveSpeed.Run) return;
            if (Map.TerrainAt(u.Position).Deep) return;
            if (u.Order != null && (u.Order.Verb == Verbs.TreatCasualty || u.Order.Verb == Verbs.Hide || u.Order.Verb == Verbs.FlyDrone || u.Order.Verb == Verbs.Carry)) return;
            // An ambush holds its fire until it's sprung, and the engineer doesn't stop setting his charges to shoot.
            var plan = u.Force.Plan;
            if (plan != null && (plan.HoldingFire || plan.HoldFire?.Invoke(u) == true || (ReferenceEquals(u, plan.Engineer) && !plan.ChargesSet))) return;

            Unit target = null;
            Vec2? area = null;
            var o = u.Order;
            if (o != null && (o.Verb == Verbs.Fire || o.Verb == Verbs.Suppress))
            {
                if (o.TargetUnit != null && o.TargetUnit.Condition != Condition.Dead && !o.TargetUnit.Gone)
                {
                    if (Sight.Between(Map, u, o.TargetUnit).SeenBy(u)) target = o.TargetUnit;
                    else if (o.Verb == Verbs.Suppress) area = Fog.For(u.Force).About(o.TargetUnit.Id)?.LastKnownPosition ?? o.TargetUnit.Position;
                }
                else if (o.Point != null) area = o.Point;
            }
            if (target == null && area == null) target = AutoTarget(u);
            // Nothing to shoot at: look again in half a second rather than every step.
            if (target == null && area == null) { u.FireCooldown = LookAgain; return; }

            Vec2 aim = target?.Position ?? area.Value;
            float range = Vec2.Distance(u.Position, aim);
            bool ordered = o != null && (o.Verb == Verbs.Fire || o.Verb == Verbs.Suppress);
            string at = target?.Name ?? o?.Target ?? Map.SquareOf(aim).ToString();
            var launcher = Weapon(u.Def.Launcher);
            if (launcher != null && range >= 30f && range <= launcher.Range && !(o != null && o.Fired))
            {
                // An order that names HE or AP fires one of those rounds, then keeps on with the rifle;
                // otherwise the launcher is used when it's worth it.
                string asked = o != null && !o.Fired ? (o.Kind ?? "").Trim().ToUpperInvariant() : "";
                bool wantAp = asked == "AP", wantHe = asked == "HE";
                bool worth = wantAp || wantHe || WorthABlast(u, aim, target);
                if (worth)
                {
                    // AP for someone behind a wall or inside a building, HE for people in the open.
                    bool ap = wantAp || (!wantHe && Map.TerrainAt(aim).Cover >= 0.5f);
                    if (ap && u.LauncherAp <= 0) ap = false;
                    if (!ap && u.LauncherAmmo <= 0) ap = u.LauncherAp > 0;
                    if (ap ? u.LauncherAp > 0 : u.LauncherAmmo > 0)
                    {
                        if (ap) u.LauncherAp--; else u.LauncherAmmo--;
                        if (ordered) FirePosition(u, o, (o.Verb == Verbs.Suppress ? "Suppressing " : "Engaging ") + at + ".");
                        if (wantAp || wantHe) o.Fired = true;
                        u.FireCooldown = launcher.Cycle;
                        Blast(u, launcher, aim, ap);
                        return;
                    }
                }
            }

            var gun = Weapon(u.Def.Weapon);
            if (gun == null || range > gun.Range) { u.FireCooldown = LookAgain; return; }
            if (u.Ammo < gun.Burst)
            {
                if (u.Ammo > 0) { u.Ammo = 0; Radio(u, u.Name + ": I'm black on ammo!", "info"); }
                u.FireCooldown = LookAgain;
                return;
            }
            u.Ammo -= gun.Burst;
            if (ordered) FirePosition(u, o, (o.Verb == Verbs.Suppress ? "Suppressing " : "Engaging ") + at + ".");
            u.FireCooldown = gun.Cycle;
            u.FiredAtStep = Clock.Step;
            Fog.Reveal(this, u, gun);

            if (target != null)
            {
                float p = HitChance(u, target, gun);
                int roll = Roll(100, u.Name, "a burst at " + target.Name + " (" + Pct(p) + ")");
                bool hit = roll <= Math.Round(p * 100);
                Record(new ShotFired(u, u.Position, target.Position, gun.Id, hit));
                Suppress(target.Position, gun.Suppress, 5f, u);
                if (hit)
                {
                    // One roll to hit; then how many rounds of the burst hit, each with its own wound roll.
                    int rounds = gun.Burst > 1 ? Roll(gun.Burst, u.Name, "rounds that hit " + target.Name, true) : 1;
                    for (int i = 0; i < rounds && target.Condition != Condition.Dead; i++) Wound(target, gun, u);
                }
            }
            else
            {
                Record(new ShotFired(u, u.Position, area.Value, gun.Id, false));
                foreach (var v in units.Where(x => x.CanAct && !x.Def.Flying && AreHostile(u.Force, x.Force) && Vec2.Distance(x.Position, area.Value) <= 6f).ToList())
                {
                    float p = HitChance(u, v, gun) * 0.3f;
                    if (Roll(100, u.Name, "suppressing fire near " + v.Name + " (" + Pct(p) + ")") <= Math.Round(p * 100)) Wound(v, gun, u);
                }
                Suppress(area.Value, gun.Suppress, 10f, u);
            }
        }

        /// <summary>The nearest enemy this unit's side has spotted and this unit can see, within its weapon's range.</summary>
        Unit AutoTarget(Unit u)
        {
            var gun = Weapon(u.Def.Weapon);
            float reach = Math.Max(gun?.Range ?? 0f, Weapon(u.Def.Launcher)?.Range ?? 0f);
            var knowledge = Fog.For(u.Force);
            // On overwatch, anything in the watched area comes first.
            Vec2? watch = u.Order != null && u.Order.Verb == Verbs.Overwatch ? u.Order.Watch : null;
            Unit best = null;
            float bestD = float.MaxValue;
            foreach (var t in units)
            {
                if (!t.CanAct || t.Def.Flying || !AreHostile(u.Force, t.Force)) continue;
                var c = knowledge.About(t.Id);
                if (c == null || !c.Visible) continue;
                float d = Vec2.Distance(u.Position, t.Position);
                if (d > reach) continue;
                float score = watch != null && Vec2.Distance(t.Position, watch.Value) <= OverwatchArea ? d - 10000f : d;
                if (score >= bestD) continue;
                if (!Sight.Between(Map, u, t).SeenBy(u)) continue;
                best = t;
                bestD = score;
            }
            return best;
        }

        bool WorthABlast(Unit u, Vec2 aim, Unit target)
        {
            if (target != null && Map.TerrainAt(target.Position).Cover >= 0.5f) return true;
            int near = units.Count(x => x.CanAct && AreHostile(u.Force, x.Force) && Vec2.Distance(x.Position, aim) <= 8f);
            if (near >= 2) return true;
            // Don't drop a grenade on your own people.
            return false;
        }

        /// <summary>Chance one burst hits, from range, posture, cover, movement, how pinned the shooter is, and the light.</summary>
        public float HitChance(Unit shooter, Unit target, WeaponDef gun)
        {
            float d = Vec2.Distance(shooter.Position, target.Position);
            float p = gun.Accuracy * (float)Math.Pow(100.0 / Math.Max(d, 25f), 0.8);
            p = Math.Min(p, 0.9f);
            p *= target.Posture == Posture.Prone ? 0.5f : target.Posture == Posture.Kneeling ? 0.75f : 1f;
            float cover = Map.TerrainAt(target.Position).Cover;
            if (target.Posture == Posture.Standing) cover *= 0.5f;
            // Inside the same building it's room to room: the walls don't stand between them.
            if (d <= 25f && Map.TerrainAt(target.Position).SeeOut && Map.TerrainAt(shooter.Position).SeeOut) cover = 0f;
            p *= 1f - cover;
            if (target.IsMoving) p *= 0.7f;
            if (shooter.IsMoving) p *= 0.4f;
            if (shooter.ArmHit) p *= 0.5f;
            p *= 1f - 0.7f * shooter.Suppression;
            var sight = Sight.Between(Map, shooter, target);
            p *= 1f - sight.Obscured;
            if (Light != LightLevel.Day && !shooter.Def.Thermal)
                p *= shooter.Def.NightVision ? 0.7f : (Light == LightLevel.Night ? 0.3f : 0.6f);
            return Math.Max(0.01f, Math.Min(0.9f, p));
        }

        /// <summary>
        /// A grenade or rocket. HE: the full blast, but a wall or building between you and it protects you.
        /// AP: a small blast that punches through walls, so cover doesn't help whoever is behind the spot it hits.
        /// </summary>
        void Blast(Unit u, WeaponDef w, Vec2 aim, bool ap = false)
        {
            u.FiredAtStep = Clock.Step;
            Fog.Reveal(this, u, w);
            float d = Vec2.Distance(u.Position, aim);
            float p = Math.Min(0.9f, w.Accuracy * (float)Math.Pow(100.0 / Math.Max(d, 25f), 0.5));
            if (u.ArmHit) p *= 0.5f;
            Vec2 at = aim;
            string round = w.Name + (w.ApBlast > 0f ? (ap ? " AP" : " HE") : "");
            if (Roll(100, u.Name, round + " at " + Map.SquareOf(Clamp(aim)) + " (" + Pct(p) + ")", true) > Math.Round(p * 100))
            {
                // A miss lands 5 to 15 meters off, in one of eight directions.
                int dir = Roll(8, u.Name, "where it lands");
                int off = 4 + Roll(10, u.Name, "how far off");
                double ang = dir * Math.PI / 4;
                at = Clamp(aim + new Vec2((float)Math.Cos(ang), (float)Math.Sin(ang)) * off);
            }
            Record(new ShotFired(u, u.Position, at, w.Id + (ap ? "-ap" : ""), true));
            Explode(u, w, at, ap);
        }

        /// <summary>A round goes off where it landed: everyone inside the blast rolls.</summary>
        internal void Explode(Unit u, WeaponDef w, Vec2 at, bool ap)
        {
            var blast = new BlastAt(u, at, w, ap);
            Record(blast);
            float radius = blast.Radius;
            foreach (var v in units.Where(x => x.Condition != Condition.Dead && !x.Gone && !x.Def.Flying && Vec2.Distance(x.Position, at) <= radius).ToList())
            {
                float dist = Vec2.Distance(v.Position, at);
                float cover = 0f;
                if (!ap)
                {
                    var here = Map.TerrainAt(v.Position);
                    if (here.SeeOut && !Map.TerrainAt(at).SeeOut) cover = 0.8f;
                    else if (!Sight.Trace(Map, at, 0.3f, v.Position, Postures.Height(v.Posture) * 0.6f).Clear) continue;
                    else cover = here.Cover * 0.5f;
                }
                float chance = (1f - dist / radius) * (1f - cover) * (v.Posture == Posture.Prone ? 0.6f : 1f);
                if (chance <= 0f) continue;
                if (Roll(100, v.Name, "caught in the blast (" + Pct(chance) + ")", true) <= Math.Round(chance * 100)) Wound(v, w, u);
            }
            Suppress(at, w.Suppress, w.Blast * 2f, null);
        }

        void Suppress(Vec2 at, float amount, float radius, Unit shooter)
        {
            foreach (var v in units)
            {
                if (v.Condition == Condition.Dead || v.Gone || v.Def.Flying || (shooter != null && ReferenceEquals(v.Force, shooter.Force))) continue;
                float d = Vec2.Distance(v.Position, at);
                if (d > radius) continue;
                v.SetSuppression(v.Suppression + amount * (d <= 2f ? 1f : 0.6f));
            }
        }

        /// <summary>
        /// A hit, by where it lands (a d20): 1–2 head, 3–9 chest, 10–12 belly, 13–16 arm, 17–20 leg.
        /// A helmet now and then stops a head hit, and plates stop half the chest hits.
        /// </summary>
        internal void Wound(Unit v, WeaponDef w, Unit by)
        {
            if (v.Condition == Condition.Dead || v.Gone) return;
            if (by != null && ReferenceEquals(by.Force, v.Force))
            {
                Record(new FriendlyFire(by, v));
                if (ReferenceEquals(v.Force, PlayerForce)) Radio(by, Callsign(by) + ": Check fire! Check fire! " + v.Name + " is hit by our own fire.", "casualty", v.Position);
            }
            int where = Roll(20, v.Name, "where the hit lands", true);
            bool stopped = false;
            if (where <= 2 && v.Def.Helmet) stopped = Roll(20, v.Name, "the helmet (stops it on " + HelmetSaves + " or less)", true) <= HelmetSaves;
            else if (where >= 3 && where <= 9 && v.Def.Plate) stopped = Roll(2, v.Name, "the plate (stops it on a 1)", true) == 1;
            HitIn(v, w, where, stopped);
        }

        /// <summary>What a hit does, from where it landed (1 to 20) and whether a helmet or plate stopped it.</summary>
        internal void HitIn(Unit v, WeaponDef w, int where, bool stopped)
        {
            if (v.Condition == Condition.Dead || v.Gone) return;
            bool ours = ReferenceEquals(v.Force, PlayerForce);
            if (where <= 2)
            {
                if (stopped)
                {
                    Saved(v, ours ? v.Name + ": Helmet took it! I'm good." : null, "is saved by the helmet");
                    return;
                }
                Kill(v, "killed by " + w.Name + ", hit in the head");
                return;
            }
            if (where <= 12)
            {
                bool chest = where <= 9;
                if (chest && stopped)
                {
                    Saved(v, ours ? v.Name + ": Plate took it! I'm good." : null, "is saved by the plate");
                    return;
                }
                if (v.Condition == Condition.Down) { Kill(v, "killed by " + w.Name); return; }
                v.Wounds++;
                Down(v, w.Name + (chest ? ", hit in the chest" : ", hit in the belly"), MinutesToBleedOutTorso);
                return;
            }
            bool arm = where <= 16;
            if (v.Condition == Condition.Down) return;
            if (v.Condition == Condition.Wounded)
            {
                v.Wounds++;
                Down(v, w.Name + (arm ? ", hit again in the arm" : ", hit again in the leg"), MinutesToBleedOut);
                return;
            }
            v.Wounds++;
            v.Condition = Condition.Wounded;
            if (arm) v.ArmHit = true;
            else
            {
                v.LegHit = true;
                v.Path = null;
                v.IsMoving = false;
                if (v.Order != null && Verbs.NeedsLegs(v.Order.Verb)) v.Order = null;
            }
            Record(new UnitWounded(v, w.Name + (arm ? ", hit in the arm" : ", hit in the leg")));
            if (ours) Radio(v, v.Name + ": I'm hit! " + (arm ? "My arm." : "My leg, I can't walk."), "casualty");
        }

        /// <summary>A helmet or plate stopped it: knocked flat and pinned, not hurt.</summary>
        void Saved(Unit v, string radio, string what)
        {
            v.SetSuppression(1f);
            v.Posture = Posture.Prone;
            Record(new UnitInTrouble(v, what));
            if (radio != null && v.Condition != Condition.Down) Radio(v, radio, "info");
        }

        void Down(Unit v, string cause, int minutes = MinutesToBleedOut)
        {
            if (v.Condition == Condition.Down || v.Condition == Condition.Dead) return;
            v.Condition = Condition.Down;
            v.DownSince = Clock.Step;
            v.BleedMinutes = minutes;
            v.Stabilized = false;
            v.Posture = Posture.Prone;
            v.Path = null;
            v.Order = null;
            v.IsMoving = false;
            Record(new UnitDown(v, cause));
            if (ReferenceEquals(v.Force, PlayerForce))
                Radio(v, (string.IsNullOrEmpty(v.Team) ? "Lead" : v.Team) + ": Man down! " + v.Name + " is down.", "casualty");
            else if (!v.Force.Civilian) Shout(Shouter(v.Force, v.Position), "Man down! Help him!");
        }

        void Kill(Unit v, string cause)
        {
            if (v.Condition == Condition.Dead) return;
            v.Condition = Condition.Dead;
            v.Posture = Posture.Prone;
            v.Path = null;
            v.Order = null;
            v.IsMoving = false;
            Record(new UnitDied(v, cause));
        }

        void BleedOut(Unit u)
        {
            if (u.Condition == Condition.Down && !u.Stabilized && Clock.Step - u.DownSince >= u.BleedMinutes * 60 * SimClock.StepsPerSecond)
                Kill(u, "died of wounds");
        }

        internal void KnockDownForTest(Unit u) => Down(u, "a test");
        internal void RemoveUnitForTest(Unit u) => units.Remove(u);

        /// <summary>A call on the net. Important ones carry a kind and a place, so the map can mark them.</summary>
        internal void Radio(Unit from, string text, string kind = null, Vec2? at = null, string speaker = null)
        {
            // A team whose radio is down isn't heard on the net, unless Lead is close enough to hear them shout.
            if (OffTheNet(from)) return;
            Record(new RadioCall(from, text, kind, kind != null ? at ?? from?.Position : null, speaker));
        }

        // ---------- calling out contacts ----------

        /// <summary>A contact out of sight this long counts as coming back when it's seen again.</summary>
        public const int SecondsGoneToCallAgain = 30;

        /// <summary>The square each team last called each contact in.</summary>
        readonly Dictionary<string, GridRef> called = new Dictionary<string, GridRef>();

        /// <summary>
        /// A Marine calls out a contact on the net ("Taking fire!" when it's shooting, "Contact" when it's just seen).
        /// Each team calls a contact once, and again only if it moves to another square or comes back after
        /// being out of sight a while.
        /// </summary>
        internal void CallContact(Unit spotter, Unit target, bool takingFire, bool wasKnown, int lastSeenStep)
        {
            if (!ReferenceEquals(spotter.Force, PlayerForce)) return;
            // What the drone sees, its operator's team calls in.
            bool drone = spotter.Def.Flying && spotter.Operator != null;
            var caller = drone ? spotter.Operator : spotter;
            string who = string.IsNullOrEmpty(caller.Team) ? caller.Name : caller.Team;
            GridRef square = Map.SquareOf(target.Position);
            string key = who + "|" + target.Id;
            bool cameBack = wasKnown && Clock.Step - lastSeenStep >= SecondsGoneToCallAgain * SimClock.StepsPerSecond;
            if (called.TryGetValue(key, out GridRef last) && last.Equals(square) && !cameBack) return;
            called[key] = square;
            string kind = target.Def.Name ?? "Enemy";
            string lower = char.ToLowerInvariant(kind[0]) + kind.Substring(1);
            Radio(caller, who + (takingFire ? ": Taking fire! " + kind : drone ? ": Contact on the drone, " + lower : ": Contact, " + lower) + " at " + square + ".", "contact", target.Position);
        }

        static float Bearing(Vec2 a, Vec2 b) => (float)(Math.Atan2(b.X - a.X, -(b.Y - a.Y)) * 180.0 / Math.PI + 360.0) % 360f;

        static string Pct(float p) => Math.Round(p * 100) + "%";
    }
}
