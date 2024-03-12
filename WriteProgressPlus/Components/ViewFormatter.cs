using System;
using System.Collections.Generic;
using System.Text;

namespace WriteProgressPlus.Components;
public static class ViewFormatter
{
    static void FormatMinimalView(BarInput input)
    {
        int maxActivityPartWidth = input.LineWidth / 2; // half, inclusive
        int maxActivityTextWidth = maxActivityPartWidth - 1; // padding from right
        int actualActivityWidth = Math.Min(maxActivityTextWidth, input.Activity.Length);
        string activity;
        if (actualActivityWidth < input.Activity.Length)
        {
            activity = input.Activity.Substring(0, actualActivityWidth);
        }
        else
        {
            activity = input.Activity;
        }

        int actualItemCounterSpace = input.LineWidth - (activity.Length + 2 + 2); // padding from right and brackets
        int remainingTime = input.Visible.HasFlag(Elements.TimeRemaining) ? input.RemainingTime : -1;
        int remainingDigits = input.RemainingTime < 0 ? 0 : (int)Math.Log10(input.RemainingTime) + 1;
        remainingDigits += 2; // space and 's'
        actualItemCounterSpace -= remainingDigits;


    }
}
