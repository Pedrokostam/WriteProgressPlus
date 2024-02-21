using System.Diagnostics;
using System.Management.Automation;

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
    /// <para/>
    /// Powershell 7 onwards has this behavior built in.
    /// </summary>
    /// <returns></returns>
    public bool ShouldDisplay()
    {
        //// if instance is null, the progress bar is being deleted
        //// in that case do not set last time
        //// as it may prevent next call to wripro
        //if (instance is  null)
        //{
        //    return true;
        //}
        TimeSpan timePassed = DateTime.Now - LastDisplayed;
        Debug.WriteLine(timePassed.TotalMilliseconds);
        if (timePassed <= UpdatePeriod)
        {
            return false;
        }
        LastDisplayed = DateTime.Now;
        return true;
    }
}
