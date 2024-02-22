namespace WriteProgressPlus.Components;

/// <summary>
/// Controls how often a progress bar can update, keeps track of iteration times and calculates ETA.
/// </summary>
internal class TimeKeeper
{
    /// <summary>
    /// Minimum time between updates of progress bar.
    /// <para/>
    /// Set to the same values as in <see href="https://github.com/PowerShell/PowerShell/pull/2822">PR #2822</see> - 200ms
    /// </summary>
    /// <remarks>
    /// More information can be found at
    /// <seealso cref="PowershellVersionDifferences.IsThrottlingBuiltIn"/>.
    /// </remarks>
    public static readonly TimeSpan UpdatePeriod = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// How many elements should be considered when calculating ETA
    /// </summary>
    public const int CalculationLength = 50;

    public DateTime StartTime { get; }

    public DateTime LastDisplayed { get; set; }

    private TimeBuffer Buffer { get; }

    private TimeKeeper(int calculationLength)
    {
        StartTime = DateTime.Now;
        Buffer = new(calculationLength);
        // Make sure the first iteration can be displayed
        LastDisplayed = StartTime - UpdatePeriod.Multiply(5);
    }

    public TimeKeeper() : this(CalculationLength)
    { }

    /// <inheritdoc cref="TimeBuffer.AddTime()"/>
    public void AddTime() => Buffer.AddTime();

    /// <inheritdoc cref="TimeBuffer.CalculateMovingAverageTime()"/>
    public TimeSpan GetAverage() => Buffer.CalculateMovingAverageTime();

    /// <summary>
    /// Ensure that the progress bar won't be updated too often, which reduces performance.
    /// Updates that come too fast should be ignored.
    /// </summary>
    /// <returns></returns>
    public bool UpdatedPermitted()
    {
        TimeSpan timePassed = DateTime.Now - LastDisplayed;
        if (timePassed <= UpdatePeriod)
        {
            return false;
        }
        LastDisplayed = DateTime.Now;
        return true;
    }
}
