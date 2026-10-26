namespace FieldTable.Sim
{
    /// <summary>
    /// Game time, counted in whole steps so it never drifts.
    /// One step is a tenth of a second.
    /// </summary>
    public sealed class SimClock
    {
        public const int StepsPerSecond = 10;
        public const int DefaultTurnSeconds = 60;

        public int Step { get; private set; }
        public int Turn { get; private set; } = 1;
        public int TurnSeconds { get; private set; }

        public SimClock(int turnSeconds = DefaultTurnSeconds)
        {
            SetTurnLength(turnSeconds);
        }

        public int StepsPerTurn => TurnSeconds * StepsPerSecond;
        public double Seconds => (double)Step / StepsPerSecond;

        /// <summary>Turn length is a setting. A change takes effect from the next turn.</summary>
        public void SetTurnLength(int seconds)
        {
            if (seconds < 1) throw new System.ArgumentOutOfRangeException(nameof(seconds), "A turn must be at least 1 second.");
            TurnSeconds = seconds;
        }

        internal void Advance() => Step++;
        internal void NextTurn() => Turn++;
    }
}
