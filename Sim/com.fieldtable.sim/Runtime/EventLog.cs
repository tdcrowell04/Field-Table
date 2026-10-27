using System.Collections.Generic;

namespace FieldTable.Sim
{
    /// <summary>Something that happened, stamped with when it happened.</summary>
    public abstract class SimEvent
    {
        public int Step { get; internal set; }
        public int Turn { get; internal set; }

        /// <summary>A plain-English line for the combat log.</summary>
        public abstract string Describe();
    }

    public sealed class TurnStarted : SimEvent
    {
        public override string Describe() => "Turn " + Turn + " starts.";
    }

    public sealed class TurnEnded : SimEvent
    {
        public override string Describe() => "Turn " + Turn + " ends.";
    }

    public sealed class DiceRolled : SimEvent
    {
        public string Actor { get; }
        public string Reason { get; }
        public int Sides { get; }
        public int Result { get; }

        /// <summary>
        /// True for the big moments the player rolls (an attack, a skill check, a called shot).
        /// The rest roll themselves on screen.
        /// </summary>
        public bool BigMoment { get; }

        public DiceRolled(string actor, string reason, int sides, int result, bool bigMoment)
        {
            Actor = actor;
            Reason = reason;
            Sides = sides;
            Result = result;
            BigMoment = bigMoment;
        }

        public override string Describe() => Actor + " rolls d" + Sides + " for " + Reason + ": " + Result + ".";
    }

    /// <summary>A force has just spotted a unit that isn't theirs.</summary>
    public sealed class ContactSpotted : SimEvent
    {
        public Force Force { get; }
        public Unit Target { get; }
        public Unit Spotter { get; }
        public GridRef Square { get; }

        public ContactSpotted(Force force, Unit target, Unit spotter, GridRef square)
        {
            Force = force;
            Target = target;
            Spotter = spotter;
            Square = square;
        }

        public override string Describe() => Spotter.Name + " spots " + Target.Name + " at " + Square + ".";
    }

    /// <summary>A force has lost sight of a unit it had spotted. It still knows where it last was.</summary>
    public sealed class ContactLost : SimEvent
    {
        public Force Force { get; }
        public Unit Target { get; }
        public GridRef LastSquare { get; }

        public ContactLost(Force force, Unit target, GridRef lastSquare)
        {
            Force = force;
            Target = target;
            LastSquare = lastSquare;
        }

        public override string Describe() => Force.Name + " loses sight of " + Target.Name + ", last seen at " + LastSquare + ".";
    }

    /// <summary>A burst, a grenade, or a rocket. Drawn as a tracer on the table.</summary>
    public sealed class ShotFired : SimEvent
    {
        public Unit Shooter { get; }
        public Vec2 From { get; }
        public Vec2 To { get; }
        public string Weapon { get; }
        public bool Hit { get; }

        public ShotFired(Unit shooter, Vec2 from, Vec2 to, string weapon, bool hit)
        {
            Shooter = shooter; From = from; To = to; Weapon = weapon; Hit = hit;
        }

        public override string Describe() => Shooter.Name + " fires " + Weapon + (Hit ? " and hits." : ".");
    }

    /// <summary>A grenade or rocket going off.</summary>
    /// <summary>The enemy blew the bridge: the cells that changed, and what each is now.</summary>
    public sealed class BridgeBlown : SimEvent
    {
        public Unit By { get; }
        public Landmark Bridge { get; }
        public IReadOnlyList<(int x, int y, string terrain)> Cells { get; }

        public BridgeBlown(Unit by, Landmark bridge, IReadOnlyList<(int x, int y, string terrain)> cells) { By = by; Bridge = bridge; Cells = cells; }
        public override string Describe() => By.Name + " blows " + Bridge.Label + ".";
    }

    public sealed class BlastAt : SimEvent
    {
        public Unit Shooter { get; }
        public Vec2 At { get; }
        public WeaponDef Weapon { get; }

        /// <summary>True for an armor-piercing round: a small blast that punches through walls.</summary>
        public bool Ap { get; }
        public float Radius { get; }

        public BlastAt(Unit shooter, Vec2 at, WeaponDef weapon, bool ap = false)
        {
            Shooter = shooter; At = at; Weapon = weapon; Ap = ap;
            Radius = ap && weapon.ApBlast > 0f ? weapon.ApBlast : weapon.Blast;
        }

        public override string Describe() => Weapon.Name + (Ap ? " (AP)" : "") + " from " + Shooter.Name + " goes off.";
    }

    public sealed class UnitWounded : SimEvent
    {
        public Unit Unit { get; }
        public string Cause { get; }
        public UnitWounded(Unit unit, string cause) { Unit = unit; Cause = cause; }
        public override string Describe() => Unit.Name + " is wounded by " + Cause + ".";
    }

    public sealed class UnitDown : SimEvent
    {
        public Unit Unit { get; }
        public string Cause { get; }
        public UnitDown(Unit unit, string cause) { Unit = unit; Cause = cause; }
        public override string Describe() => Unit.Name + " is down, hit by " + Cause + ".";
    }

    public sealed class UnitDied : SimEvent
    {
        public Unit Unit { get; }
        public string Cause { get; }
        public UnitDied(Unit unit, string cause) { Unit = unit; Cause = cause; }
        public override string Describe() => Unit.Name + " " + Cause + ".";
    }

    public sealed class UnitTreated : SimEvent
    {
        public Unit Medic { get; }
        public Unit Patient { get; }
        public UnitTreated(Unit medic, Unit patient) { Medic = medic; Patient = patient; }
        public override string Describe() => Medic.Name + " treats " + Patient.Name + ".";
    }

    /// <summary>Something going wrong for one unit, like going under in deep water.</summary>
    public sealed class UnitInTrouble : SimEvent
    {
        public Unit Unit { get; }
        public string What { get; }
        public UnitInTrouble(Unit unit, string what) { Unit = unit; What = what; }
        public override string Describe() => Unit.Name + " " + What + ".";
    }

    /// <summary>A line on the radio net. Written from templates now; the in-game AI can voice them later.</summary>
    /// <summary>A unit hurt by its own side's fire.</summary>
    public sealed class FriendlyFire : SimEvent
    {
        public Unit Shooter { get; }
        public Unit Victim { get; }
        public FriendlyFire(Unit shooter, Unit victim) { Shooter = shooter; Victim = victim; }
        public override string Describe() => Victim.Name + " is hit by friendly fire from " + Shooter.Name + ".";
    }

    public sealed class SmokeAt : SimEvent
    {
        public Unit Thrower { get; }
        public SmokeCloud Cloud { get; }
        public SmokeAt(Unit thrower, SmokeCloud cloud) { Thrower = thrower; Cloud = cloud; }
        public override string Describe() => Thrower.Name + " throws smoke.";
    }

    public sealed class BreachMade : SimEvent
    {
        public Unit Unit { get; }
        public Vec2 At { get; }
        public float Radius { get; }
        public BreachMade(Unit unit, Vec2 at, float radius) { Unit = unit; At = at; Radius = radius; }
        public override string Describe() => Unit.Name + " opens a breach.";
    }

    public sealed class UnitSurrendered : SimEvent
    {
        public Unit Unit { get; }
        public UnitSurrendered(Unit unit) { Unit = unit; }
        public override string Describe() => Unit.Name + " gives up.";
    }

    /// <summary>A fighter shouted, and the Marines heard it. Understood when the interpreter was in earshot.</summary>
    public sealed class ShoutHeard : SimEvent
    {
        public Unit By { get; }
        public string Words { get; }
        public bool Understood { get; }
        public ShoutHeard(Unit by, string words, bool understood) { By = by; Words = words; Understood = understood; }
        public override string Describe() => By.Name + " shouts" + (Understood ? ": " + Words : ".");
    }

    public sealed class UnitDetained : SimEvent
    {
        public Unit Unit { get; }
        public Unit By { get; }
        public UnitDetained(Unit unit, Unit by) { Unit = unit; By = by; }
        public override string Describe() => By.Name + " detains " + Unit.Name + ".";
    }

    public sealed class UnitEvacuated : SimEvent
    {
        public Unit Unit { get; }
        public UnitEvacuated(Unit unit) { Unit = unit; }
        public override string Describe() => Unit.Name + " is evacuated.";
    }

    /// <summary>The CASEVAC vehicle or helicopter arriving at the landing zone, or leaving it.</summary>
    public sealed class CasevacMoved : SimEvent
    {
        public Vec2 Lz { get; }
        public bool Arrived { get; }
        public bool Helicopter { get; }
        public CasevacMoved(Vec2 lz, bool arrived, bool helicopter) { Lz = lz; Arrived = arrived; Helicopter = helicopter; }
        public override string Describe() => (Helicopter ? "The CASEVAC helicopter " : "The CASEVAC vehicle ") + (Arrived ? "arrives." : "leaves.");
    }

    public sealed class RadioCall : SimEvent
    {
        public Unit From { get; }
        public string Text { get; }

        /// <summary>Where the call is about, for a marker on the map, or null for calls that don't need one.</summary>
        public Vec2? At { get; }

        /// <summary>What kind of news it is: contact, casualty, set, fire (a fire position), smoke, lz, or info. Null with no marker.</summary>
        public string Kind { get; }

        /// <summary>
        /// Whose words these really are when the caller passes on what someone said (a villager, a prisoner, a shout),
        /// so the AI can give them their own voice. Null for a Marine's own call.
        /// </summary>
        public string Speaker { get; }

        public RadioCall(Unit from, string text, string kind = null, Vec2? at = null, string speaker = null) { From = from; Text = text; Kind = kind; At = at; Speaker = speaker; }
        public override string Describe() => Text;
    }

    /// <summary>Everything that happened, in order. Nothing is ever removed.</summary>
    public sealed class EventLog
    {
        readonly List<SimEvent> events = new List<SimEvent>();

        public IReadOnlyList<SimEvent> Events => events;

        internal void Add(SimEvent e, SimClock clock)
        {
            e.Step = clock.Step;
            e.Turn = clock.Turn;
            events.Add(e);
        }
    }
}
