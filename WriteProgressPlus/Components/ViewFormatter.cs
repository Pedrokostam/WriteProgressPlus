using System;
using System.Collections.Generic;
using System.Text;

namespace WriteProgressPlus.Components;
public static class ViewFormatter
{
    internal static BarOutput FormatMinimalView(BarInput input)
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
        actualItemCounterSpace -= remainingDigits; // Space left for progressbar status
        // here it means counter, percents, formatted item
        string counterString = input.Counter.GetTextForm(input.Visible, actualItemCounterSpace);
        int formattedItemSpace;
        if (counterString.Length == 0)
        {
            formattedItemSpace = actualItemCounterSpace;
        }
        else
        {
            formattedItemSpace = actualItemCounterSpace - counterString.Length - 3; // 3 for spacing 
        }
        string formattedItem = input.FormattedItem;
        if (input.FormattedItem.Length > formattedItemSpace)
        {
            // substring shorter by one letter to put the ellipsis
            formattedItem = formattedItem.Substring(0, formattedItemSpace - 1) + "…";
        }
        string status = (counterString, formattedItem) switch
        {
            ({ Length: 0 }, { Length: 0 }) => string.Empty,
            ({ Length: > 0 }, { Length: 0 }) => counterString,
            ({ Length: 0 }, { Length: > 0 }) => formattedItem,
            ({ Length: > 0 }, { Length: > 0 }) => $"{counterString} - {formattedItem}",
        };

        return new BarOutput(activity,
                             status,
                             remainingTime,
                             input.Counter.Percent);
    }
}
