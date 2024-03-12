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
    public readonly int LineWidth;
    public readonly Elements Visible;

    public BarInput(string? formattedItem, Counter counter, string activity, int remainingTime, int lineWidth, Elements visible)
    {
        FormattedItem = Regex.Replace((formattedItem ?? string.Empty), @"[\r\n]", "", RegexOptions.None, TimeSpan.FromMilliseconds(500));
        Counter = counter;
        Activity = activity;
        RemainingTime = remainingTime;
        LineWidth = lineWidth;
        Visible = visible;
    }
}
