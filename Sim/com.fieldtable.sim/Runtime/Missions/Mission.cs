using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    /// <summary>A mission ready to play: the fight, who's in it, the brief, and how it ends.</summary>
    public sealed class Mission
    {
        public string Title { get; internal set; }
        public Simulation Sim { get; internal set; }
        public MapData MapData { get; internal set; }
        public Force Marines { get; internal set; }
        public Force Enemy { get; internal set; }
        public Force Civilians { get; internal set; }
        public List<string> Brief { get; } = new List<string>();

        /// <summary>The place the mission is about, like the bridge to seize.</summary>
        public Landmark Objective { get; internal set; }

        /// <summary>The main objective in a line, like "Seize and hold Fox Creek bridge (E3)."</summary>
        public string MainObjective { get; internal set; }

        /// <summary>The rules of engagement in a line.</summary>
        public string Roe { get; internal set; }

        /// <summary>What the mission asks for on top of the main objective. They don't decide winning or losing; the end grades them.</summary>
        public List<SecondaryObjective> Secondary { get; } = new List<SecondaryObjective>();

        /// <summary>A Marine this close to the objective is on it.</summary>
        public const float OnObjective = 15f;

        /// <summary>True while a Marine who can fight is on the objective.</summary>
        public bool MarineOnObjective => Sim.Units.Any(u => ReferenceEquals(u.Force, Marines) && !u.Def.Flying && u.CanAct && OnTheObjective(u.Position));

        /// <summary>True once the enemy has blown the bridge.</summary>
        public bool BridgeBlown => Sim.Log.Events.OfType<BridgeBlown>().Any();

        /// <summary>
        /// On the bridge, within 15 m of its middle. Once it's been blown, the crossing site still counts: within 15 m of
        /// either end.
        /// </summary>
        bool OnTheObjective(Vec2 p)
        {
            if (Vec2.Distance(p, Objective.Center) <= OnObjective) return true;
            if (!BridgeBlown || Objective.Points.Count < 2) return false;
            return Vec2.Distance(p, Objective.Points[0]) <= OnObjective || Vec2.Distance(p, Objective.Points[Objective.Points.Count - 1]) <= OnObjective;
        }

        /// <summary>The Marines themselves, not counting the drone or the interpreter.</summary>
        public IEnumerable<Unit> Squad => Sim.Units.Where(u => ReferenceEquals(u.Force, Marines) && !u.Def.Flying && !u.Def.Interpreter);

        /// <summary>"success", "failure", or null while it's still going.</summary>
        public string Outcome { get; private set; }
        public string OutcomeText { get; private set; }

        /// <summary>
        /// The grade at the end. A win is S+ with every secondary objective met, then S, A, and B as fewer are met,
        /// and C with Marines killed; each friendly fire hit on a Marine drops it a grade, all the way to F. A loss is F.
        /// </summary>
        public string Rating { get; private set; }

        static readonly string[] Grades = { "S+", "S", "A", "B", "C", "F" };
        const int KilledGrade = 4;

        /// <summary>How many times Marines have been hit by their own side's fire.</summary>
        public int FriendlyFireHits => Sim.Log.Events.OfType<FriendlyFire>().Count(e => ReferenceEquals(e.Victim.Force, Marines));

        /// <summary>Marines down or dead before the squad can't fight on.</summary>
        public const int CombatIneffective = 7;

        /// <summary>Turns before the platoon can't wait any longer.</summary>
        public const int TurnLimit = 40;

        /// <summary>Checks, from what's really true, whether the mission has ended. Call after each turn.</summary>
        public void CheckOutcome()
        {
            if (Outcome != null) return;
            var marines = Squad.ToList();
            Vec2 b = Objective.Center;
            bool onBridge = MarineOnObjective;
            bool enemyNear = Sim.Units.Any(u => ReferenceEquals(u.Force, Enemy) && u.CanAct && Vec2.Distance(u.Position, b) <= HoldRadius);
            int lost = marines.Count(u => !u.CanAct);
            bool holding = onBridge && !enemyNear;
            if (lost >= CombatIneffective)
            {
                Outcome = "failure";
                OutcomeText = "The squad is combat ineffective, with " + lost + " Marines down.";
            }
            else if (holding && heldLastCheck)
            {
                Outcome = "success";
                OutcomeText = Objective.Label + " is seized and held.";
            }
            else if (Sim.Clock.Turn > TurnLimit)
            {
                Outcome = "failure";
                OutcomeText = "Out of time. The platoon couldn't wait any longer for the bridge.";
            }
            heldLastCheck = holding;
            if (Outcome != null && Rating == null) Rating = Rate();
        }

        string Rate()
        {
            if (Outcome != "success") return "F";
            int grade = Squad.Any(u => u.Condition == Condition.Dead) ? KilledGrade : Math.Min(KilledGrade - 1, Secondary.Count(o => o.IsBroken(this)));
            return Grades[Math.Min(Grades.Length - 1, grade + FriendlyFireHits)];
        }

        bool heldLastCheck;

        /// <summary>True when anyone in a force has been wounded, downed, or killed so far.</summary>
        public bool AnyoneHurt(Force force) => Sim.Log.Events.Any(e =>
            (e is UnitWounded w && ReferenceEquals(w.Unit.Force, force)) ||
            (e is UnitDown d && ReferenceEquals(d.Unit.Force, force)) ||
            (e is UnitDied x && ReferenceEquals(x.Unit.Force, force)));

        /// <summary>No enemy still fighting within this many meters, and the bridge is held.</summary>
        public const float HoldRadius = 250f;
    }

    /// <summary>One secondary objective: an id, what it asks, and how to tell, from what's really true, that it's been broken.</summary>
    public sealed class SecondaryObjective
    {
        public string Id { get; }
        public string Text { get; }
        readonly Func<Mission, bool> broken;

        public SecondaryObjective(string id, string text, Func<Mission, bool> broken)
        {
            Id = id;
            Text = text;
            this.broken = broken ?? throw new ArgumentNullException(nameof(broken));
        }

        /// <summary>True once it can't be met any more.</summary>
        public bool IsBroken(Mission mission) => broken(mission);
    }
}
