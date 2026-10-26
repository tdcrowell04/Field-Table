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

        /// <summary>"success", "failure", or null while it's still going.</summary>
        public string Outcome { get; private set; }
        public string OutcomeText { get; private set; }

        /// <summary>Marines down or dead before the squad can't fight on.</summary>
        public const int CombatIneffective = 7;

        /// <summary>Turns before the platoon can't wait any longer.</summary>
        public const int TurnLimit = 40;

        /// <summary>Checks, from what's really true, whether the mission has ended. Call after each turn.</summary>
        public void CheckOutcome()
        {
            if (Outcome != null) return;
            var marines = Sim.Units.Where(u => ReferenceEquals(u.Force, Marines)).ToList();
            Vec2 b = Objective.Center;
            bool onBridge = marines.Any(u => u.CanAct && Vec2.Distance(u.Position, b) <= 15f);
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
        }

        bool heldLastCheck;

        /// <summary>No enemy still fighting within this many meters, and the bridge is held.</summary>
        public const float HoldRadius = 250f;
    }
}
