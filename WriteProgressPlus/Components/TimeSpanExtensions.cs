using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace WriteProgressPlus.Components;
public static class TimeSpanExtensions
{
    public static TimeSpan Multiply(this TimeSpan ts, int multiplier) => TimeSpan.FromTicks(ts.Ticks * multiplier);
}
