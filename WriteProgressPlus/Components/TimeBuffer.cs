using System.Collections.ObjectModel;
namespace WriteProgressPlus.Components;

/// <summary>
/// Simple implementation of a circular buffer.
/// Stores recent TimeEntries and can calculate moving average.
/// </summary>
public class TimeBuffer
{
    private readonly TimeSpan Negative = TimeSpan.FromSeconds(-1);
    /// <summary>
    /// Upper limit to avoid having too large buffers.
    /// </summary>
    public static readonly int MaxCalculationLength = 5000;

    /// <summary>
    /// Lower limit to avoid having too large buffers.
    /// </summary>
    public static readonly int MinCalculationLength = 50;

    private readonly TimeEntry[] _timeEntries;

    private DateTime LastDataPoint = DateTime.MinValue;

    public int MaxLength { get; }

    /// <summary>
    /// How many elements were inserted.
    /// </summary>
    private int CurrentIndex = 0;

    /// <summary>
    /// Where to the LAST element was put.
    /// </summary>
    private int LatestIndex = 0;
    private int OldestIndex = 0;

    /// <summary>
    /// Where to put the NEXT element.
    /// </summary>
    private int InsertionIndex => CurrentIndex % MaxLength;

    private TimeEntry LatestTimeEntry => _timeEntries[LatestIndex];
    private TimeEntry OldestTimeEntry => _timeEntries[OldestIndex];

    public TimeBuffer(int calculationLength)
    {
        MaxLength = Math.Min(Math.Max(MinCalculationLength, calculationLength), MaxCalculationLength);
        _timeEntries = new TimeEntry[MaxLength];
    }
    /// <summary>
    /// Adds current datetime to time buffer.
    /// </summary>
    public void AddTime(int iteration) => AddTime(new TimeEntry(DateTime.UtcNow, iteration));

    /// <summary>
    /// Adds given datetime to time buffer.
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
        if (CurrentIndex > MaxLength)
        {
            OldestIndex = InsertionIndex;
        }
    }

    public ICollection<TimeEntry> TimeEntries
    {
        get
        {
            if (CurrentIndex < MaxLength)
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
        var iterationSpan = LatestTimeEntry.Iteration - OldestTimeEntry.Iteration;
        if (iterationSpan <= 0)
        {
            // Nothing to calculate, since we have not proceeded with iteration.
            // Or we have negative iteration, which is worse.
            return Negative;
        }
        TimeSpan timeSpan = LatestTimeEntry.Time - OldestTimeEntry.Time;
        var timePerIterationMilliseconds = timeSpan.TotalMilliseconds / iterationSpan;
        return TimeSpan.FromMilliseconds(timePerIterationMilliseconds);
    }
}
