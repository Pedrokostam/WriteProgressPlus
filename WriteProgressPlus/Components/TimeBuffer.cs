namespace WriteProgressPlus.Components;

class TimeBuffer
{
    //private readonly LinkedList<TimeSpan> timeSpans;
    private readonly TimeSpan[] timeSpans;

    private DateTime LastDataPoint = DateTime.MinValue;
    public int CurrentLength { get; private set; }
    public int MaxLength { get; }
    private int CurrentIndex = 0;
    private int InsertionIndex => CurrentIndex % MaxLength;
    public TimeBuffer(int calculationLength)
    {
        MaxLength = calculationLength > 0 ? calculationLength : 1;
        timeSpans = new TimeSpan[MaxLength];
    }

    public void AddTime() => AddTime(DateTime.Now);
    public void AddTime(DateTime time)
    {
        timeSpans[InsertionIndex] = time - LastDataPoint;
        LastDataPoint = time;
        CurrentIndex++;
    }

    public TimeSpan CalculateMovingAverageTime()
    {
        if (CurrentIndex == 0) return TimeSpan.Zero;
        if (CurrentIndex < MaxLength)
        {
            return TimeSpan.FromMilliseconds(timeSpans.Take(CurrentIndex).Average(x => x.Milliseconds));
        }
        else
        {
            return TimeSpan.FromMilliseconds(timeSpans.Average(x => x.Milliseconds));
        }
    }
}
