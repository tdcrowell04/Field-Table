using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    /// <summary>
    /// Talking: the interpreter, questioning prisoners, fighters giving up, and the shouting you hear across the field.
    /// The code decides what each person knows and whether they tell the truth; the page's AI only words it.
    /// </summary>
    public sealed partial class Simulation
    {
        /// <summary>The interpreter has to be this close to whoever's being talked to, in meters.</summary>
        public const float InterpreterReach = 10f;

        /// <summary>How long a Marine waits at someone for the interpreter to catch up before he radios for him, in seconds.</summary>
        public const float InterpreterWait = 15f;

        /// <summary>Shouting carries this far, and the side that shouted won't again for this many seconds.</summary>
        public const float ShoutCarries = 150f;
        public const int ShoutEvery = 30;

        /// <summary>A fighter with no friend this close, pinned flat, may give up. Checked once a minute, a 1 in 3 chance.</summary>
        public const float AloneWithin = 60f;

        readonly Dictionary<Force, int> lastShout = new Dictionary<Force, int>();

        readonly List<Lie> lies = new List<Lie>();

        /// <summary>Every lie a prisoner has told this mission, for the end screen.</summary>
        public IReadOnlyList<Lie> Lies => lies;

        static bool SameLanguage(Force a, Force b) => string.Equals(a.Language, b.Language, StringComparison.OrdinalIgnoreCase);

        /// <summary>True when what a Marine says to someone gets through: same language, or the interpreter is there.</summary>
        internal bool Understood(Unit talker, Unit p) =>
            SameLanguage(talker.Force, p.Force) || talker.Def.Interpreter ||
            units.Any(i => i.Def.Interpreter && i.CanAct && ReferenceEquals(i.Force, talker.Force) && Vec2.Distance(i.Position, p.Position) <= InterpreterReach);

        /// <summary>Talk: walk over, wait for the interpreter if they need him, then 20 seconds of talking.</summary>
        void ThinkTalk(Unit u, Order o, float dt)
        {
            var p = o.TargetUnit;
            if (!Reach(u, o, p, 3f)) return;
            if (!Understood(u, p))
            {
                if (Work(u, dt, InterpreterWait))
                {
                    Radio(u, u.Name + ": " + p.Name + " doesn't speak English. I need the interpreter here.");
                    Finish(u, null);
                }
                return;
            }
            if (!Work(u, dt, TalkSeconds)) return;
            if (AreHostile(u.Force, p.Force)) Question(u, p);
            else ShareWhatTheySaw(u, p);
            Finish(u, null);
        }

        /// <summary>
        /// A prisoner, a wounded fighter, or one who gave up, questioned. A d6: 1 or 2 he won't say anything, 3 or 4 he
        /// lies, 5 or 6 he gives something true up. Either way it's only what he claims: the squad's picture of the
        /// enemy doesn't change until they see for themselves.
        /// </summary>
        void Question(Unit u, Unit p)
        {
            int roll = Roll(6, p.Name, "what " + p.Name + " gives up (1-2 nothing, 3-4 lies, 5-6 the truth)");
            string through = SameLanguage(u.Force, p.Force) ? "" : ", through the interpreter,";
            if (roll <= 2) { Radio(u, u.Name + ": " + p.Name + " won't say anything.", "info", p.Position, p.Name); return; }
            var friends = units.Where(x => ReferenceEquals(x.Force, p.Force) && x != p && x.CanAct).ToList();
            var plan = p.Force.Plan;
            string claim;
            Lie lie = null;
            if (roll >= 5)
            {
                var told = friends.GroupBy(x => Map.SquareOf(x.Position).ToString())
                    .Select(g => (g.Count() == 1 ? "one fighter" : g.Count() + " fighters") + " at " + g.Key).ToList();
                claim = told.Count == 0 ? "says he's the last of them" : (friends.Count == 1 ? "says there's " : "says there are ") + JoinAnd(told);
                string why = plan?.Said(plan.Goal);
                if (why != null) claim += ", and " + why;
            }
            else
            {
                // A lie: fighters somewhere none are (and no Marine is either), and the wrong idea of what they're up to.
                var real = new HashSet<string>(friends.Select(x => Map.SquareOf(x.Position).ToString()));
                foreach (var m in units.Where(x => ReferenceEquals(x.Force, u.Force) && !x.Gone && !x.Def.Flying)) real.Add(Map.SquareOf(m.Position).ToString());
                var squares = new List<GridRef>();
                for (int c = 0; c < Map.Columns; c++)
                    for (int r = 0; r < Map.Rows; r++)
                    {
                        var g = new GridRef(c, r);
                        if (!real.Contains(g.ToString()) && Vec2.Distance(Map.SquareCenter(g), p.Position) <= 400f) squares.Add(g);
                    }
                GridRef? where = squares.Count == 0 ? (GridRef?)null : squares[Roll(squares.Count, p.Name, "where the lie puts them", true) - 1];
                claim = where == null ? "says they've all gone" : "says the rest of them are at " + where;
                string madeUp = null;
                if (plan != null)
                {
                    var others = plan.Options.Where(g => g != plan.Goal).ToList();
                    if (others.Count > 0)
                    {
                        madeUp = others[Roll(others.Count, p.Name, "the plan he makes up", true) - 1];
                        claim += ", and " + plan.Said(madeUp);
                    }
                }
                lie = new Lie(p, where, madeUp, plan);
                lies.Add(lie);
            }
            Radio(u, u.Name + ": " + p.Name + through + " " + claim + ".", "info", p.Position, p.Name);
        }

        /// <summary>
        /// Once a second: a Marine who goes to the square a prisoner lied about, and finds nobody there, says so. A lie
        /// nobody acts on waits for the end screen.
        /// </summary>
        void LieTick()
        {
            foreach (var lie in lies)
            {
                if (lie.Exposed || lie.Square == null) continue;
                var sq = lie.Square.Value;
                var there = units.FirstOrDefault(m => ReferenceEquals(m.Force, PlayerForce) && m.CanAct && !m.Def.Flying && Map.SquareContains(sq, m.Position));
                if (there == null) continue;
                if (units.Any(x => ReferenceEquals(x.Force, lie.Prisoner.Force) && x.CanAct && Map.SquareContains(sq, x.Position))) continue;
                lie.Exposed = true;
                Record(new LieExposed(lie, there));
                Radio(there, Callsign(there) + ": Nothing at " + sq + ". " + lie.Prisoner.Name + " lied to us.", "info", Map.SquareCenter(sq));
            }
        }

        // ---------- giving up ----------

        /// <summary>Once a second: a fighter pinned flat with no friend near may give up.</summary>
        void ThinkSurrender()
        {
            foreach (var u in units.ToList())
            {
                if (!u.CanAct || u.Force.Civilian || ReferenceEquals(u.Force, PlayerForce) || u.Def.Flying) continue;
                if (u.Suppression < PinnedAt) continue;
                if (Clock.Step - u.SurrenderCheckedStep < 60 * SimClock.StepsPerSecond) continue;
                if (units.Any(x => x != u && ReferenceEquals(x.Force, u.Force) && x.CanAct && Vec2.Distance(x.Position, u.Position) <= AloneWithin)) continue;
                u.SurrenderCheckedStep = Clock.Step;
                if (Roll(6, u.Name, "whether " + u.Name + ", alone and pinned, gives up (5 or 6)", true) >= 5) Surrender(u);
            }
        }

        internal void Surrender(Unit u)
        {
            u.Surrendered = true;
            u.Order = null;
            u.Path = null;
            u.IsMoving = false;
            u.Posture = Posture.Kneeling;
            u.Emote = "surrender";
            u.EmoteUntil = int.MaxValue;
            // Weapon down: he has nothing to fight with now.
            u.Ammo = 0; u.LauncherAmmo = 0; u.LauncherAp = 0; u.Grenades = 0;
            Record(new UnitSurrendered(u));
            Shout(u, "Don't shoot! I give up!", always: true);
            var sees = units.FirstOrDefault(m => ReferenceEquals(m.Force, PlayerForce) && m.CanAct && !m.Def.Flying && Fog.For(PlayerForce).About(u.Id)?.Visible == true);
            if (sees != null) Radio(sees, Callsign(sees) + ": " + u.Name + " is giving up at " + Map.SquareOf(u.Position) + ".", "info", u.Position);
        }

        // ---------- shouting ----------

        /// <summary>
        /// A fighter shouts. Marines within earshot hear it; with the interpreter in earshot too, he says what it was.
        /// Where it came from is only as close as the grid square.
        /// </summary>
        internal void Shout(Unit e, string words, bool always = false)
        {
            if (e == null || ReferenceEquals(e.Force, PlayerForce) || e.Force.Civilian) return;
            if (!always && lastShout.TryGetValue(e.Force, out int last) && Clock.Step - last < ShoutEvery * SimClock.StepsPerSecond) return;
            var hear = units.Where(m => ReferenceEquals(m.Force, PlayerForce) && m.CanAct && !m.Def.Flying && Vec2.Distance(m.Position, e.Position) <= ShoutCarries).ToList();
            if (hear.Count == 0) return;
            lastShout[e.Force] = Clock.Step;
            var square = Map.SquareOf(e.Position);
            var terp = hear.FirstOrDefault(m => m.Def.Interpreter);
            Record(new ShoutHeard(e, words, terp != null));
            if (terp != null || SameLanguage(e.Force, PlayerForce))
            {
                var who = terp ?? hear.OrderBy(m => Vec2.Distance(m.Position, e.Position)).First();
                Radio(who, (terp != null ? "Interpreter" : Callsign(who)) + ": They're shouting \"" + words + "\" at " + square + ".", "info", Map.SquareCenter(square), "an enemy fighter");
            }
            else
            {
                var near = hear.OrderBy(m => Vec2.Distance(m.Position, e.Position)).First();
                Radio(near, Callsign(near) + ": Shouting from " + square + ". Can't make it out.", "info", Map.SquareCenter(square));
            }
        }

        /// <summary>The nearest of a side's fighters still up, to shout about something that happened near a spot.</summary>
        Unit Shouter(Force f, Vec2 near) =>
            units.Where(x => ReferenceEquals(x.Force, f) && x.CanAct).OrderBy(x => Vec2.Distance(x.Position, near)).FirstOrDefault();

        // ---------- the interpreter's standing order ----------

        /// <summary>With no orders, the interpreter sticks with the squad leader, within about 8 m.</summary>
        void StayWithLead(Unit u)
        {
            var lead = units.FirstOrDefault(x => ReferenceEquals(x.Force, u.Force) && x.CanAct && x.IsCalled("Lead"));
            if (lead == null || lead == u) return;
            float d = Vec2.Distance(u.Position, lead.Position);
            if (d <= 8f) { if (u.Path != null && d <= 5f) u.Path = null; return; }
            if (u.Path != null && Clock.Step % (5 * SimClock.StepsPerSecond) != 0) return;
            u.Path = Paths.Find(u.Position, lead.Position, false);
            u.PathIndex = 0;
            u.Speed = d > 15f ? MoveSpeed.Run : MoveSpeed.Walk;
        }
    }

    /// <summary>A lie a prisoner told: where he said his friends were (if anywhere), and the plan he made up (if any).</summary>
    public sealed class Lie
    {
        public Unit Prisoner { get; }

        /// <summary>The square he said the rest of them were in, or null when he said they'd all gone.</summary>
        public GridRef? Square { get; }

        /// <summary>The plan he made up instead of the real one, or null when the plan never came up.</summary>
        public string MadeUpGoal { get; }

        /// <summary>True once the squad acted on it and found out.</summary>
        public bool Exposed { get; internal set; }

        readonly EnemyPlan plan;

        public Lie(Unit prisoner, GridRef? square, string madeUpGoal, EnemyPlan plan = null) { Prisoner = prisoner; Square = square; MadeUpGoal = madeUpGoal; this.plan = plan; }

        /// <summary>For the end screen: what wasn't so.</summary>
        public string Summary
        {
            get
            {
                var parts = new List<string> { Square == null ? "they hadn't gone" : "nobody was at " + Square };
                if (MadeUpGoal != null && plan != null) parts.Add("they were never out to " + plan.Truth(MadeUpGoal));
                return Prisoner.Name + " lied when questioned: " + (parts.Count == 1 ? parts[0] : parts[0] + ", and " + parts[1]) + ".";
            }
        }

    }
}
