

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
    public static readonly long UpdatePeriodTicks = 2_000_000; // tick is 100ns => 200ms

    /// <summary>
    /// How many elements should be considered when calculating ETA
    /// </summary>
    public const int CalculationLength = 50;

    public long LastDisplayTimeTicks { get; set; }

    private TimeBuffer Buffer { get; }

    private TimeKeeper(int calculationLength)
    {
        Buffer = new(calculationLength);
        // Make sure the first iteration can be displayed
        // .UtcNow is about 3 times faster than .Now
        LastDisplayTimeTicks = DateTime.UtcNow.Ticks - UpdatePeriodTicks * 5;
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
        long currentTicks = DateTime.UtcNow.Ticks;
        long ticksPassed = currentTicks - LastDisplayTimeTicks;
        if (ticksPassed <= UpdatePeriodTicks)
        {
            return false;
        }
        LastDisplayTimeTicks = currentTicks;
        return true;
    }
}
