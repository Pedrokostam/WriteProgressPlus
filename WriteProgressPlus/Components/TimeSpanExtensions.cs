using System.Runtime.CompilerServices;

namespace WriteProgressPlus.Components;

public static class TimeSpanExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan Multiply(this TimeSpan ts, int multiplier) => TimeSpan.FromTicks(ts.Ticks * multiplier);
}
