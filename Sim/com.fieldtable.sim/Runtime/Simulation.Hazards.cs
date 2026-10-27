using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    /// <summary>A hidden bomb by the road. It goes off when anyone steps within reach of it.</summary>
    public sealed class Ied
    {
        public Vec2 Position { get; }
        public bool Found { get; internal set; }
        public bool Blown { get; internal set; }
        public Ied(Vec2 position) { Position = position; }
    }

    /// <summary>What the director can switch in that lives in the simulation itself: IEDs, and a team's radio going dead.</summary>
    public sealed partial class Simulation
    {
        /// <summary>An IED goes off when someone comes this close, and a Marine walking past this close may spot it.</summary>
        public const float IedTrigger = 3f, IedSpotted = 15f;

        /// <summary>A Marine walking (not running) within sight of an IED spots it this often, each second.</summary>
        public const float IedSpotChance = 0.05f;

        /// <summary>With its radio down, a team only gets orders the squad leader can shout, from this close.</summary>
        public const float ShoutingDistance = 50f;

        readonly List<Ied> ieds = new List<Ied>();
        public IReadOnlyList<Ied> Ieds => ieds;

        // Which teams have a dead radio, and the last turn it stays dead.
        readonly Dictionary<string, int> radioDown = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        internal Ied PlaceIed(Vec2 at)
        {
            var ied = new Ied(at);
            ieds.Add(ied);
            return ied;
        }

        /// <summary>Once a second: spotting IEDs, and setting them off.</summary>
        void IedTick()
        {
            foreach (var ied in ieds.Where(i => !i.Blown).ToList())
            {
                var stepOn = units.FirstOrDefault(x => !x.Gone && !x.Def.Flying && x.Condition != Condition.Dead && x.CarriedBy == null && Vec2.Distance(x.Position, ied.Position) <= IedTrigger);
                if (stepOn != null)
                {
                    ied.Blown = true;
                    var bomb = Weapon("ied");
                    if (bomb != null) Explode(null, bomb, ied.Position, ap: false);
                    Record(new IedWentOff(ied, stepOn));
                    var heard = units.FirstOrDefault(m => ReferenceEquals(m.Force, PlayerForce) && m.CanAct && !m.Def.Flying);
                    if (heard != null) Radio(heard, Callsign(heard) + ": IED! IED at " + Map.SquareOf(ied.Position) + "!", "casualty", ied.Position);
                    continue;
                }
                if (ied.Found) continue;
                foreach (var m in units)
                {
                    if (!ReferenceEquals(m.Force, PlayerForce) || !m.CanAct || m.Def.Flying || m.Speed == MoveSpeed.Run && m.IsMoving) continue;
                    if (Vec2.Distance(m.Position, ied.Position) > IedSpotted) continue;
                    if (!Sight.Trace(Map, m.Position, m.EyeHeight, ied.Position, 0.2f).Clear) continue;
                    if (Roll(100, m.Name, "spotting something wrong by the road (5%)") > Math.Round(IedSpotChance * 100)) continue;
                    ied.Found = true;
                    Record(new IedFound(ied, m));
                    Radio(m, Callsign(m) + ": Possible IED at " + Map.SquareOf(ied.Position) + "! Everyone hold, watch your step.", "info", ied.Position);
                    break;
                }
            }
        }

        /// <summary>A team's radio goes dead for a few turns.</summary>
        internal void KnockOutRadio(string team, int turns)
        {
            radioDown[team] = Clock.Turn + turns - 1;
            var lead = SquadLeader();
            if (lead != null) Radio(lead, "Lead: No answer from " + team + ". Their radio's down.", "info", units.FirstOrDefault(u => string.Equals(u.Team, team, StringComparison.OrdinalIgnoreCase) && u.CanAct)?.Position);
        }

        /// <summary>The teams whose radios are down right now.</summary>
        public IEnumerable<string> RadiosDown => radioDown.Where(kv => Clock.Turn <= kv.Value).Select(kv => kv.Key);

        Unit SquadLeader() => units.FirstOrDefault(x => ReferenceEquals(x.Force, PlayerForce) && x.CanAct && x.IsCalled("Lead"));

        /// <summary>True when this Marine can't hear the net or be heard on it: his team's radio is down and Lead is too far to shout.</summary>
        internal bool OffTheNet(Unit u)
        {
            if (u == null || !ReferenceEquals(u.Force, PlayerForce) || string.IsNullOrEmpty(u.Team)) return false;
            if (!radioDown.TryGetValue(u.Team, out int until) || Clock.Turn > until) return false;
            var lead = SquadLeader();
            return lead == null || Vec2.Distance(lead.Position, u.Position) > ShoutingDistance;
        }

        /// <summary>Once a second: a radio that's come back says so.</summary>
        void RadioTick()
        {
            foreach (var team in radioDown.Where(kv => Clock.Turn > kv.Value).Select(kv => kv.Key).ToList())
            {
                radioDown.Remove(team);
                var tl = units.FirstOrDefault(u => string.Equals(u.Team, team, StringComparison.OrdinalIgnoreCase) && u.CanAct);
                if (tl != null) Radio(tl, team + ": Radio check, Lead. We're back up.");
            }
        }
    }
}
