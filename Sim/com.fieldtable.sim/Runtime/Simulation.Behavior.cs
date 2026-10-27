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

        const float SuppressionFadePerSecond = 0.08f;
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
                if (u.Condition == Condition.Dead) { u.IsMoving = false; continue; }
                BleedOut(u);
                if (!u.CanAct) { u.IsMoving = false; continue; }
                if (u.Order == null && !ReferenceEquals(u.Force, PlayerForce)) Brain(u);
                Think(u, dt);
                Move(u, dt);
            }
            foreach (var u in units) Fight(u, dt);
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
                            Radio(u, (u.Def.Medic ? "Doc" : u.Name) + ": " + patient.Name + " is " + (patient.Condition == Condition.Down ? "stable." : "patched up and good to go."));
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
            }
        }

        /// <summary>When the last of a team finishes the same move, its leader reports set.</summary>
        void TeamSet(Unit u, Order done)
        {
            if (!ReferenceEquals(u.Force, PlayerForce) || done.Group == 0) return;
            if (units.Any(x => x.Order != null && x.Order.Group == done.Group)) return;
            string who = string.IsNullOrEmpty(u.Team) ? u.Name : u.Team;
            Radio(u, who + ": Set" + (string.IsNullOrWhiteSpace(done.Where) ? "." : " at " + done.Where + "."));
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
            if ((u.Wounds >= 1 && u.Suppression >= 0.6f) || lost >= 0.5f)
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

        void CheckWater(Unit u, float dt)
        {
            if (!Map.TerrainAt(u.Position).Deep) { u.DeepSeconds = 0f; return; }
            if (u.DeepSeconds == 0f && u.Load > LoadToSwim)
            {
                Record(new UnitInTrouble(u, "is going under"));
                Radio(u, u.Name + " is going under!");
            }
            u.DeepSeconds += dt;
            if (u.Load > LoadToSwim && u.DeepSeconds >= SecondsToDrown) Kill(u, "drowned");
        }

        // ---------- fighting ----------

        void Fight(Unit u, float dt)
        {
            if (!u.CanAct || u.Def.Weapon == null) return;
            u.FireCooldown -= dt;
            if (u.FireCooldown > 0f) return;
            if (u.Suppression >= PinnedAt) return;
            if (u.IsMoving && u.Speed == MoveSpeed.Run) return;
            if (Map.TerrainAt(u.Position).Deep) return;
            if (u.Order != null && (u.Order.Verb == Verbs.TreatCasualty || u.Order.Verb == Verbs.Hide)) return;

            Unit target = null;
            Vec2? area = null;
            var o = u.Order;
            if (o != null && (o.Verb == Verbs.Fire || o.Verb == Verbs.Suppress))
            {
                if (o.TargetUnit != null && o.TargetUnit.Condition != Condition.Dead)
                {
                    if (Sight.Between(Map, u, o.TargetUnit).Clear) target = o.TargetUnit;
                    else if (o.Verb == Verbs.Suppress) area = Fog.For(u.Force).About(o.TargetUnit.Id)?.LastKnownPosition ?? o.TargetUnit.Position;
                }
                else if (o.Point != null) area = o.Point;
            }
            if (target == null && area == null) target = AutoTarget(u);
            if (target == null && area == null) return;

            Vec2 aim = target?.Position ?? area.Value;
            float range = Vec2.Distance(u.Position, aim);
            var launcher = Weapon(u.Def.Launcher);
            if (launcher != null && u.LauncherAmmo > 0 && range >= 30f && range <= launcher.Range && WorthABlast(u, aim, target))
            {
                u.LauncherAmmo--;
                u.FireCooldown = launcher.Cycle;
                Blast(u, launcher, aim);
                return;
            }

            var gun = Weapon(u.Def.Weapon);
            if (gun == null || range > gun.Range) return;
            if (u.Ammo < gun.Burst)
            {
                if (u.Ammo > 0) { u.Ammo = 0; Radio(u, u.Name + ": I'm black on ammo!"); }
                return;
            }
            u.Ammo -= gun.Burst;
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
                if (hit) Wound(target, gun);
            }
            else
            {
                Record(new ShotFired(u, u.Position, area.Value, gun.Id, false));
                foreach (var v in units.Where(x => x.CanAct && AreHostile(u.Force, x.Force) && Vec2.Distance(x.Position, area.Value) <= 6f).ToList())
                {
                    float p = HitChance(u, v, gun) * 0.3f;
                    if (Roll(100, u.Name, "suppressing fire near " + v.Name + " (" + Pct(p) + ")") <= Math.Round(p * 100)) Wound(v, gun);
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
            Unit best = null;
            float bestD = float.MaxValue;
            foreach (var t in units)
            {
                if (!t.CanAct || !AreHostile(u.Force, t.Force)) continue;
                var c = knowledge.About(t.Id);
                if (c == null || !c.Visible) continue;
                float d = Vec2.Distance(u.Position, t.Position);
                if (d > reach || d >= bestD) continue;
                if (!Sight.Between(Map, u, t).Clear) continue;
                best = t;
                bestD = d;
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
            p *= 1f - cover;
            if (target.IsMoving) p *= 0.7f;
            if (shooter.IsMoving) p *= 0.4f;
            p *= 1f - 0.7f * shooter.Suppression;
            var sight = Sight.Between(Map, shooter, target);
            p *= 1f - sight.Obscured;
            if (Light != LightLevel.Day && !shooter.Def.Thermal)
                p *= shooter.Def.NightVision ? 0.7f : (Light == LightLevel.Night ? 0.3f : 0.6f);
            return Math.Max(0.01f, Math.Min(0.9f, p));
        }

        void Blast(Unit u, WeaponDef w, Vec2 aim)
        {
            u.FiredAtStep = Clock.Step;
            Fog.Reveal(this, u, w);
            float d = Vec2.Distance(u.Position, aim);
            float p = Math.Min(0.9f, w.Accuracy * (float)Math.Pow(100.0 / Math.Max(d, 25f), 0.5));
            Vec2 at = aim;
            if (Roll(100, u.Name, w.Name + " at " + Map.SquareOf(Clamp(aim)) + " (" + Pct(p) + ")", true) > Math.Round(p * 100))
            {
                // A miss lands 5 to 15 meters off, in one of eight directions.
                int dir = Roll(8, u.Name, "where it lands");
                int off = 4 + Roll(10, u.Name, "how far off");
                double ang = dir * Math.PI / 4;
                at = Clamp(aim + new Vec2((float)Math.Cos(ang), (float)Math.Sin(ang)) * off);
            }
            Record(new ShotFired(u, u.Position, at, w.Id, true));
            Record(new BlastAt(u, at, w));
            foreach (var v in units.Where(x => x.Condition != Condition.Dead && Vec2.Distance(x.Position, at) <= w.Blast).ToList())
            {
                float dist = Vec2.Distance(v.Position, at);
                float cover = Map.TerrainAt(v.Position).SeeOut && !Map.TerrainAt(at).SeeOut ? 0.8f : Map.TerrainAt(v.Position).Cover * 0.5f;
                float chance = (1f - dist / w.Blast) * (1f - cover) * (v.Posture == Posture.Prone ? 0.6f : 1f);
                if (chance <= 0f) continue;
                if (Roll(100, v.Name, "caught in the blast (" + Pct(chance) + ")", true) <= Math.Round(chance * 100)) Wound(v, w);
            }
            Suppress(at, w.Suppress, w.Blast * 2f, null);
        }

        void Suppress(Vec2 at, float amount, float radius, Unit shooter)
        {
            foreach (var v in units)
            {
                if (v.Condition == Condition.Dead || (shooter != null && ReferenceEquals(v.Force, shooter.Force))) continue;
                float d = Vec2.Distance(v.Position, at);
                if (d > radius) continue;
                v.SetSuppression(v.Suppression + amount * (d <= 2f ? 1f : 0.6f));
            }
        }

        void Wound(Unit v, WeaponDef w)
        {
            if (v.Condition == Condition.Dead) return;
            int roll = Roll(20, v.Name, "how bad the hit is", true);
            if (roll <= 2 && v.Condition != Condition.Ok)
            {
                Kill(v, "killed by " + w.Name);
                return;
            }
            if (roll <= Math.Round(20 * w.Lethality) || v.Wounds >= 1)
            {
                v.Wounds++;
                Down(v, w.Name);
                return;
            }
            v.Wounds++;
            v.Condition = Condition.Wounded;
            Record(new UnitWounded(v, w.Name));
            if (ReferenceEquals(v.Force, PlayerForce)) Radio(v, v.Name + ": I'm hit!");
        }

        void Down(Unit v, string cause)
        {
            if (v.Condition == Condition.Down || v.Condition == Condition.Dead) return;
            v.Condition = Condition.Down;
            v.DownSince = Clock.Step;
            v.Stabilized = false;
            v.Posture = Posture.Prone;
            v.Path = null;
            v.Order = null;
            v.IsMoving = false;
            Record(new UnitDown(v, cause));
            if (ReferenceEquals(v.Force, PlayerForce))
                Radio(v, (string.IsNullOrEmpty(v.Team) ? "Lead" : v.Team) + ": Man down! " + v.Name + " is down.");
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
            if (u.Condition == Condition.Down && !u.Stabilized && Clock.Step - u.DownSince >= MinutesToBleedOut * 60 * SimClock.StepsPerSecond)
                Kill(u, "died of wounds");
        }

        internal void KnockDownForTest(Unit u) => Down(u, "a test");
        internal void RemoveUnitForTest(Unit u) => units.Remove(u);

        internal void Radio(Unit from, string text) => Record(new RadioCall(from, text));

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
            string who = string.IsNullOrEmpty(spotter.Team) ? spotter.Name : spotter.Team;
            GridRef square = Map.SquareOf(target.Position);
            string key = who + "|" + target.Id;
            bool cameBack = wasKnown && Clock.Step - lastSeenStep >= SecondsGoneToCallAgain * SimClock.StepsPerSecond;
            if (called.TryGetValue(key, out GridRef last) && last.Equals(square) && !cameBack) return;
            called[key] = square;
            string kind = target.Def.Name ?? "enemy";
            Radio(spotter, who + (takingFire ? ": Taking fire! " + kind : ": Contact, " + kind.ToLowerInvariant()) + " at " + square + ".");
        }

        static float Bearing(Vec2 a, Vec2 b) => (float)(Math.Atan2(b.X - a.X, -(b.Y - a.Y)) * 180.0 / Math.PI + 360.0) % 360f;

        static string Pct(float p) => Math.Round(p * 100) + "%";
    }
}
