using System.Collections.ObjectModel;
namespace WriteProgressPlus.Components;

/// <summary>
/// Simple implementation of a circular buffer.
/// Stores recent TimeEntries and can calculate moving average.
/// </summary>
public class TimeBuffer
{
    /// <summary>
    /// Value returned when calculation is not possible.
    /// -1 second is understood by WriteProgress as no time remaining specified.
    /// </summary>
    private readonly TimeSpan FallbackValue = TimeSpan.FromSeconds(-1);

    /// <summary>
    /// Upper limit to avoid having too large buffers.
    /// </summary>
    public static readonly int MaxCalculationLength = 5000;

    /// <summary>
    /// Lower limit to avoid having too small buffers.
    /// </summary>
    public static readonly int MinCalculationLength = 50;

    /// <summary>
    /// Actual container for data.
    /// </summary>
    private readonly TimeEntry[] _timeEntries;

    /// <summary>
    /// How many elements the buffer can store.
    /// </summary>
    public int BufferLength => _timeEntries.Length;

    /// <summary>
    /// How many elements were inserted.
    /// </summary>
    private int CurrentIndex = 0;

    /// <summary>
    /// Where the LATEST element was put.
    /// </summary>
    private int LatestIndex = 0;

    /// <summary>
    /// Where the OLDEST element was put.
    /// </summary>
    private int OldestIndex = 0;

    /// <summary>
    /// Where to put the NEXT element.
    /// </summary>
    private int InsertionIndex => CurrentIndex % BufferLength;

    /// <summary>
    /// Most recent time entry.
    /// </summary>
    private TimeEntry LatestTimeEntry => _timeEntries[LatestIndex];

    /// <summary>
    /// Least recent time entry still in buffer.
    /// </summary>
    private TimeEntry OldestTimeEntry => _timeEntries[OldestIndex];

    public TimeBuffer(int calculationLength)
    {
        var bufferLength = Math.Min(Math.Max(MinCalculationLength, calculationLength), MaxCalculationLength);
        _timeEntries = new TimeEntry[bufferLength];
        // set the first element to a non-natural value - this will make sure
        // that the first actual element won't be skipped due to having the same value.
        // P.S. No need to set every element of the array, as the only public way to add an element is through AddTime
        // which will always start at zero.
        _timeEntries[0] = TimeEntry.MinValue;

    }
    /// <summary>
    /// Adds current datetime and given iteration to time buffer.
    /// </summary>
    public void AddTime(int iteration) => AddTime(new TimeEntry(DateTime.UtcNow, iteration));

    /// <summary>
    /// Adds given datetime and iteration to time buffer.
    /// </summary>
    public void AddTime(DateTime time, int iteration) => AddTime(new TimeEntry(time, iteration));

    /// <summary>
    /// Adds given datetime and iteration to time buffer.
    /// </summary>
    public void AddTime(TimeEntry entry)
    {
        if (_timeEntries[LatestIndex].Iteration == entry.Iteration)
        {
            // If the iteration of new entry is the same as the latest entriy's, do not add it.
            return;
        }
        _timeEntries[InsertionIndex] = entry;
        LatestIndex = InsertionIndex;

        CurrentIndex++;

        // if CurrentIndex exceeds length of the array
        // the next element will overwrite something
        // so the oldest index should become InsertionIndex
        if (CurrentIndex > BufferLength)
        {
            OldestIndex = InsertionIndex;
        }
    }

    // Public view of buffer, limited to non-empty elements.
    public ICollection<TimeEntry> TimeEntries
    {
        get
        {
            if (CurrentIndex < BufferLength)
            {
                return _timeEntries.Take(CurrentIndex).ToList();
            }
            return new ReadOnlyCollection<TimeEntry>(_timeEntries);
        }
    }

    /// <summary>
    /// Calculates estimated time to completion.
    /// </summary>
    /// <returns></returns>
    public TimeSpan CalculateMovingAverageTime()
    {
#if NET6_0
        return TimeSpan.MinValue;
#endif
        var iterationSpan = LatestTimeEntry.Iteration - OldestTimeEntry.Iteration;
        if (iterationSpan <= 0)
        {
            // Nothing to calculate, since we have not proceeded with iteration.
            // Or we have negative iteration, which is worse.
            return FallbackValue;
        }
        TimeSpan timeSpan = LatestTimeEntry.Time - OldestTimeEntry.Time;
        var timePerIterationMilliseconds = timeSpan.TotalSeconds / iterationSpan;
        return TimeSpan.FromSeconds(timePerIterationMilliseconds);
    }
}
