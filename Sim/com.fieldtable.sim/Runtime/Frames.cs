using System.Collections.Generic;

namespace FieldTable.Sim
{
    /// <summary>Every unit at one moment of a turn.</summary>
    public sealed class Frame
    {
        public int Step { get; }
        public List<UnitFrame> Units { get; } = new List<UnitFrame>();
        public Frame(int step) => Step = step;
    }

    /// <summary>One unit at one moment: where, how, and whether the viewing side can see it.</summary>
    public readonly struct UnitFrame
    {
        public readonly string Id;
        public readonly float X, Y, Facing;
        public readonly Posture Posture;
        public readonly Condition Condition;
        public readonly bool Moving, Firing, Seen, Detained;
        public readonly string Emote;

        public UnitFrame(Unit u, bool seen)
        {
            Id = u.Id;
            X = u.Position.X;
            Y = u.Position.Y;
            Facing = u.Facing;
            Posture = u.Posture;
            Condition = u.Condition;
            Moving = u.IsMoving;
            Firing = u.IsFiring;
            Emote = u.Emote;
            Seen = seen;
            Detained = u.Detained;
        }
    }
}
