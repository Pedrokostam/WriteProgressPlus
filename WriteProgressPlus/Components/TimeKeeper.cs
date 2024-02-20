namespace WriteProgressPlus.Components;

internal class TimeKeeper
{
    /// <summary>
    /// Minimum time between updates of progress bar. 
    /// </summary>
    public static readonly TimeSpan UpdatePeriod = TimeSpan.FromMilliseconds(100);

    public const int CalculationLength = 50;

    public DateTime StartTime { get; }

    public DateTime LastDisplayed { get; set; }

    private TimeBuffer Buffer { get; }

    public int DatapointCount => Buffer.CurrentLength;

    public TimeKeeper(int calculationLength)
    {
        StartTime = DateTime.Now;
        Buffer = new(calculationLength);
        LastDisplayed = StartTime - UpdatePeriod.Multiply(5);
    }

    public TimeKeeper() : this(CalculationLength)
    { }

    public void AddTime() => Buffer.AddTime();

    public TimeSpan GetAverage() => Buffer.CalculateMovingAverageTime();

    /// <summary>
    /// Ensure that the progress bar won't be updated too often, which reduces performance.
    /// Updates that come too fast should be ignored.
    /// <para/>
    /// Powershell 7 onwards has this behavior built in.
    /// </summary>
    /// <returns></returns>
    public bool ShouldDisplay()
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
