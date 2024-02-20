namespace WriteProgressPlus.Components;

internal class TimeKeeper
{
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

    public bool ShouldDisplay()
    {
        if (DateTime.Now - LastDisplayed > UpdatePeriod)
        {
            LastDisplayed = DateTime.Now;
            return true;
        }
        else return false;
    }
}
