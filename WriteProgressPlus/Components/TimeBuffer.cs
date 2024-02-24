using System.Collections.ObjectModel;

namespace WriteProgressPlus.Components;

/// <summary>
/// Simple implementation of a circular buffer.
/// Stores recent TimeSpans and can calculate moving average.
/// </summary>
public class TimeBuffer
{
    /// <summary>
    /// Upper limit to avoid having too large buffers.
    /// </summary>
    public static readonly int MaxCalculationLength = 5000;

    /// <summary>
    /// Lower limit to avoid having too large buffers.
    /// </summary>
    public static readonly int MinCalculationLength = 50;

    private readonly TimeSpan[] timeSpans;

    private DateTime LastDataPoint = DateTime.MinValue;

    public int MaxLength { get; }

    /// <summary>
    /// How many elements were inserted.
    /// </summary>
    private int CurrentIndex = 0;

    /// <summary>
    /// Where to put the NEXT element.
    /// </summary>
    private int InsertionIndex => CurrentIndex % MaxLength;

    public TimeBuffer(int calculationLength)
    {
        MaxLength = Math.Min(Math.Max(MinCalculationLength, calculationLength), MaxCalculationLength);
        timeSpans = new TimeSpan[MaxLength];
    }
    /// <summary>
    /// Adds current datetime to time buffer.
    /// </summary>
    public void AddTime() => AddTime(DateTime.Now);

    /// <summary>
    /// Adds given datetime to time buffer.
    /// </summary>
    public void AddTime(DateTime time)
    {
        //If LastDataPoint is at its starting value, instead of adding time to the buffer, set LastDataPoint to its value.
        if (LastDataPoint == DateTime.MinValue)
        {
            LastDataPoint = time;
        }
        else
        {
            timeSpans[InsertionIndex] = time - LastDataPoint;
            LastDataPoint = time;
            CurrentIndex++;
        }
    }

    public ICollection<TimeSpan> TimeSpans
    {
        get
        {
            if (CurrentIndex < MaxLength)
            {
                return timeSpans.Take(CurrentIndex).ToList();
            }
            return new ReadOnlyCollection<TimeSpan>(timeSpans);
        }
    }

    /// <summary>
    /// Calculates estimated time to completion.
    /// </summary>
    /// <returns></returns>
    public TimeSpan CalculateMovingAverageTime()
    {
        // If we haven't filled the whole buffer, take only until CurrentIndex,
        // otherwise take the whole length
        int end = CurrentIndex < MaxLength ? CurrentIndex : MaxLength;
        if (end == 0)
        {
            // Won't be able to divide by zero
            return TimeSpan.Zero;
        }

        long tickSum = 0;
        for (int i = 0; i < end; i++)
        {
            tickSum += timeSpans[i].Ticks;
        }
        long tickMean = tickSum / end;
        
        return TimeSpan.FromTicks(tickMean);
    }
}
