using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WriteProgressPlus.Components;
internal readonly record struct BarInput
{
    public readonly string FormattedItem;
    public readonly Counter Counter;
    public readonly string Activity;
    public readonly int RemainingTime;
    public readonly Size BufferSize;
    public readonly Elements Visible;
    public int LineWidth => BufferSize.Width;
    public int LineHeight => BufferSize.Height;

    public BarInput(string? formattedItem, Counter counter, string activity, int remainingTime, Size buffer, Elements visible)
    {
        FormattedItem = Regex.Replace((formattedItem ?? string.Empty), @"[\r\n]", "", RegexOptions.None, TimeSpan.FromMilliseconds(500));
        Counter = counter;
        Activity = activity;
        RemainingTime = remainingTime;
        BufferSize = buffer;
        Visible = visible;
    }

    /// <summary>
    /// If time remaining is requested by <see cref="Visible"/> and remaining time is positive returns it. Otherwise returns -1;
    /// </summary>
    internal int VisibleRemainingTime
    {
        get
        {
            if (Visible.HasFlag(Elements.TimeRemaining) && RemainingTime >= 0)
            {
                return RemainingTime;
            }
            return -1;
        }
    }

    /// <summary>
    /// If percent is requested by <see cref="Visible"/> and precent is positive or zero returns it. Otherwise returns -1;
    /// </summary>
    internal int VisiblePercent
    {
        get
        {
            if (Visible.HasFlag(Elements.Percentage))
            {
                return RemainingTime;
            }
            return Counter.Percent;
        }
    }
}
