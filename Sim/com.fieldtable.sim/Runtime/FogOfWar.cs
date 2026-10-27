using System;
using System.Collections.Generic;

namespace FieldTable.Sim
{
    /// <summary>How much light there is to see by.</summary>
    public enum LightLevel
    {
        Day,
        Twilight,
        Night,
    }

    /// <summary>What one force knows about one unit that isn't theirs.</summary>
    public sealed class Contact
    {
        public Unit Unit { get; }

        /// <summary>True once someone in the force has spotted it. Stays true after it drops out of sight.</summary>
        public bool Known { get; internal set; }

        /// <summary>True while someone in the force can see it right now.</summary>
        public bool Visible { get; internal set; }

        /// <summary>Where the force last saw it. Only updates while it's visible.</summary>
        public Vec2 LastKnownPosition { get; internal set; }
        public int LastSeenStep { get; internal set; }

        /// <summary>Progress toward spotting it, 0 to 1. Builds while it's in view, fades when it isn't.</summary>
        public float Progress { get; internal set; }

        internal Contact(Unit unit) => Unit = unit;
    }

    /// <summary>Everything one force knows about everyone else.</summary>
    public sealed class ForceKnowledge
    {
        readonly Dictionary<string, Contact> contacts = new Dictionary<string, Contact>();

        public Force Force { get; }

        internal ForceKnowledge(Force force) => Force = force;

        /// <summary>Only units this force has spotted.</summary>
        public IEnumerable<Contact> Contacts
        {
            get
            {
                foreach (var c in contacts.Values)
                    if (c.Known) yield return c;
            }
        }

        /// <summary>What this force knows about a unit, or null if it has never spotted it.</summary>
        public Contact About(string unitId) => contacts.TryGetValue(unitId, out var c) && c.Known ? c : null;

        internal Contact Track(Unit unit)
        {
            if (!contacts.TryGetValue(unit.Id, out var c))
            {
                c = new Contact(unit);
                contacts[unit.Id] = c;
            }
            return c;
        }
    }

    /// <summary>
    /// Fog of war: each force only knows what its own units have seen.
    /// Spotting builds up over time instead of rolling dice: a target standing in the open
    /// at 100 meters is spotted in about half a second, at 600 meters in about 6 seconds,
    /// and much slower when it's prone, still, or hidden in cover.
    /// Each observer looks with whatever they have (eyes, NODs, a thermal) and the best one counts.
    /// </summary>
    public sealed class FogOfWar
    {
        /// <summary>Farthest the naked eye spots a person, in meters, by light.</summary>
        public const float DayEyeRange = 1000f;
        public const float TwilightEyeRange = 400f;
        public const float NightEyeRange = 50f;

        /// <summary>Farthest NODs spot a person, at twilight or night. They're no help in daylight.</summary>
        public const float NightVisionRange = 300f;

        /// <summary>NODs spot more slowly than eyes in daylight: less detail, no depth.</summary>
        const float NightVisionSpeed = 0.6f;

        /// <summary>Farthest a thermal spots a person, day or night.</summary>
        public const float ThermalRange = 1500f;

        /// <summary>A thermal sees body heat through reeds and grass, so concealment counts for less.</summary>
        const float ThermalConcealment = 0.3f;

        /// <summary>How fast unfinished spotting fades once the target is out of view, per second.</summary>
        const float FadePerSecond = 0.5f;

        readonly Dictionary<string, ForceKnowledge> byForce = new Dictionary<string, ForceKnowledge>();

        public ForceKnowledge For(Force force)
        {
            if (force == null) throw new ArgumentNullException(nameof(force));
            if (!byForce.TryGetValue(force.Id, out var k))
            {
                k = new ForceKnowledge(force);
                byForce[force.Id] = k;
            }
            return k;
        }

        public static float EyeRange(LightLevel light)
        {
            switch (light)
            {
                case LightLevel.Twilight: return TwilightEyeRange;
                case LightLevel.Night: return NightEyeRange;
                default: return DayEyeRange;
            }
        }

        /// <summary>How fast one observer is spotting one target, as progress per second. 0 when it can't.</summary>
        public static float SpotRate(MapGrid map, Unit observer, Unit target, LightLevel light = LightLevel.Day)
        {
            if (!observer.CanAct) return 0f;
            float d = Vec2.Distance(observer.Position, target.Position);

            float eye = EyeRange(light);
            float nods = observer.Def.NightVision && light != LightLevel.Day ? NightVisionRange : 0f;
            float thermal = observer.Def.Thermal ? ThermalRange : 0f;
            if (d > Math.Max(eye, Math.Max(nods, thermal))) return 0f;

            SightResult sight = Sight.Between(map, observer, target);
            if (!sight.Clear) return 0f;

            double secondsToSpot = 0.5 * Math.Pow(Math.Max(d, 10f) / 100.0, 1.4);
            float baseRate = (float)(1.0 / secondsToSpot);
            baseRate *= Postures.Visibility(target.Posture);
            if (target.IsMoving) baseRate *= 2f;
            baseRate *= 1f - sight.Obscured;
            baseRate *= 1f - observer.Suppression;
            float concealment = ConcealmentAt(map, target);

            float best = 0f;
            if (d <= eye) best = Math.Max(best, baseRate * (1f - concealment));
            if (d <= nods) best = Math.Max(best, baseRate * NightVisionSpeed * (1f - concealment));
            if (d <= thermal) best = Math.Max(best, baseRate * (1f - concealment * ThermalConcealment));
            return Math.Max(0f, best);
        }

        /// <summary>
        /// How hidden a unit is by the ground it's in. Full concealment when the ground is at least as tall
        /// as the unit (standing in reeds, lying in scrub), a little when it isn't.
        /// </summary>
        public static float ConcealmentAt(MapGrid map, Unit unit)
        {
            TerrainDef t = map.TerrainAt(unit.Position);
            return t.Height >= Postures.Height(unit.Posture) ? t.Concealment : t.Concealment * 0.3f;
        }

        /// <summary>
        /// Firing gives a shooter away: anyone on a hostile side with a line of sight to them spots them at once
        /// within earshot (600 m, or 150 m with a suppressor), and at night a muzzle flash shows at any range
        /// unless the weapon is suppressed.
        /// </summary>
        internal void Reveal(Simulation sim, Unit shooter, WeaponDef weapon)
        {
            float earshot = weapon.Suppressed ? 150f : 600f;
            bool flash = sim.Light != LightLevel.Day && !weapon.Suppressed;
            foreach (Force force in sim.Forces)
            {
                if (ReferenceEquals(force, shooter.Force) || !sim.AreHostile(force, shooter.Force)) continue;
                Unit spotter = null;
                foreach (Unit o in sim.Units)
                {
                    if (!ReferenceEquals(o.Force, force) || !o.CanAct) continue;
                    float d = Vec2.Distance(o.Position, shooter.Position);
                    if (d > earshot && !flash) continue;
                    if (!Sight.Between(sim.Map, o, shooter).Clear) continue;
                    spotter = o;
                    break;
                }
                if (spotter == null) continue;
                Contact c = For(force).Track(shooter);
                bool wasVisible = c.Visible, wasKnown = c.Known;
                int lastSeen = c.LastSeenStep;
                c.Known = true;
                c.Visible = true;
                c.Progress = 1f;
                c.LastKnownPosition = shooter.Position;
                c.LastSeenStep = sim.Clock.Step;
                if (!wasVisible)
                {
                    sim.Record(new ContactSpotted(force, shooter, spotter, sim.Map.SquareOf(shooter.Position)));
                    sim.CallContact(spotter, shooter, true, wasKnown, lastSeen);
                }
            }
        }

        internal void Update(Simulation sim, float seconds)
        {
            foreach (Force force in sim.Forces)
            {
                ForceKnowledge knowledge = For(force);
                foreach (Unit target in sim.Units)
                {
                    if (ReferenceEquals(target.Force, force)) continue;

                    float best = 0f;
                    Unit spotter = null;
                    foreach (Unit observer in sim.Units)
                    {
                        if (!ReferenceEquals(observer.Force, force)) continue;
                        float r = SpotRate(sim.Map, observer, target, sim.Light);
                        if (r > best)
                        {
                            best = r;
                            spotter = observer;
                        }
                    }

                    Contact c = knowledge.Track(target);
                    if (best > 0f)
                    {
                        if (c.Visible)
                        {
                            c.LastKnownPosition = target.Position;
                            c.LastSeenStep = sim.Clock.Step;
                            continue;
                        }
                        c.Progress = Math.Min(1f, c.Progress + best * seconds);
                        if (c.Progress >= 1f)
                        {
                            bool wasKnown = c.Known;
                            int lastSeen = c.LastSeenStep;
                            c.Known = true;
                            c.Visible = true;
                            c.LastKnownPosition = target.Position;
                            c.LastSeenStep = sim.Clock.Step;
                            sim.Record(new ContactSpotted(force, target, spotter, sim.Map.SquareOf(target.Position)));
                            if (sim.AreHostile(force, target.Force)) sim.CallContact(spotter, target, false, wasKnown, lastSeen);
                        }
                    }
                    else
                    {
                        if (c.Visible)
                        {
                            c.Visible = false;
                            c.Progress = 0f;
                            sim.Record(new ContactLost(force, target, sim.Map.SquareOf(c.LastKnownPosition)));
                        }
                        else
                        {
                            c.Progress = Math.Max(0f, c.Progress - FadePerSecond * seconds);
                        }
                    }
                }
            }
        }
    }
}
