using System.Runtime.InteropServices;
namespace WriteProgressPlus.Components;

[StructLayout(LayoutKind.Auto)]
public readonly record struct TimeEntry
{
    public readonly DateTime Time;
    public readonly int Iteration;

    public TimeEntry(DateTime time, int iteration)
    {
        Time = time;
        Iteration = iteration;
    }
    public static TimeEntry Zero => new(DateTime.MinValue, 0);
}
