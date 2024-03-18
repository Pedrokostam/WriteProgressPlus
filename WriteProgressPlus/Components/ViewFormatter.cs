using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace WriteProgressPlus.Components;
public static class ViewFormatter
{
    /// <summary>
    /// String used to denote the string has been truncated.
    /// </summary>
    private static readonly string CutOff = "~";
    private static string FitString(string s, int maxLenth)
    {
        if (maxLenth <= 1)
        {
            // zero length would result in negative substring
            // one length would result in just the CutOff
            return string.Empty;
        }
        if (s.Length <= maxLenth)
        {
            return s;
        }
        return s.Substring(0, maxLenth - CutOff.Length) + CutOff;
    }
    internal static BarOutput FormatMinimalView(BarInput input)
    {
        int maxActivityPartWidth = input.LineWidth / 2; // half, inclusive
        int maxActivityTextWidth = maxActivityPartWidth - 1; // padding from right
        int actualActivityWidth = Math.Min(maxActivityTextWidth, input.Activity.Length);
        string activity = FitString(input.Activity, actualActivityWidth);

        int actualItemCounterSpace = input.LineWidth - (activity.Length + 2 + 2); // padding from right and brackets
        int remainingTime = input.Visible.HasFlag(Elements.TimeRemaining) ? input.RemainingTime : -1;
        int remainingDigits = input.RemainingTime switch
        {
            < 0 => 0,
            0 => 1, // '0',
            _ => (int)Math.Log10(input.RemainingTime) + 1
        };
        remainingDigits += 3; // space and 's'
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
            formattedItem = FitString(formattedItem, formattedItemSpace);
        }
        string status = (counterString, formattedItem) switch
        {
            ({ Length: 0 }, { Length: 0 }) => string.Empty,
            ({ Length: > 0 }, { Length: 0 }) => counterString,
            ({ Length: 0 }, { Length: > 0 }) => formattedItem,
            ({ Length: > 0 }, { Length: > 0 }) => $"{counterString} - {formattedItem}",
        };

        return new BarOutput(activity: activity, status: status, remainingTime: remainingTime, percentComplete: input.Counter.Percent, currentOperation: string.Empty);
    }
    internal static BarOutput FormatClassicFullView(BarInput input)
    {
        string activity = FitString(input.Activity, input.LineWidth - 1);

        //bool isCurrentOperatioVisible = input.LineHeight > 

        string currentOperation = FitString(input.FormattedItem, input.LineWidth - 4);
        string status = input.Counter.GetTextForm(input.Visible, input.LineWidth - 4);
        int remainingTime = input.Visible.HasFlag(Elements.TimeRemaining) ? input.RemainingTime : -1;

        return new BarOutput(activity: activity,
                             status: status,
                             remainingTime: remainingTime,
                             percentComplete: input.Counter.Percent,
                             currentOperation: currentOperation);
    }
    internal static BarOutput FormatClassicMidView(BarInput input)
    {
        string activity = FitString(input.Activity, input.LineWidth - 1);

        // if bar is not shown, numerical percentage and time remaining are placed on status line
        // percent has no padding, just number immediately followed by space
        // time remaining has default tostring form, followed by space
        string percent = input.VisiblePercent >= 0 ? input.Counter.Percent.ToString(@"0\%", CultureInfo.InvariantCulture) : "";
        string time = input.VisibleRemainingTime >= 0 ? TimeSpan.FromSeconds(input.RemainingTime).ToString() : "";
        int reservedSpace = (percent, time) switch
        {
            ({ Length: 0 }, { Length: 0 }) => 0,
            ({ Length: > 0 }, { Length: > 0 }) => percent.Length + time.Length + 2,
            ({ Length: > 0 }, { Length: 0 }) => percent.Length + 1,
            ({ Length: 0 }, { Length: > 0 }) => time.Length + 1,
        };
        string status = input.Counter.GetCounterString(input.Visible, input.LineWidth - 4 - reservedSpace);
        string currentOperation = FitString(input.FormattedItem, input.LineWidth - 4);
        int remainingTime = input.Visible.HasFlag(Elements.TimeRemaining) ? input.RemainingTime : -1;

        return new BarOutput(activity: activity,
                             status: status,
                             remainingTime: remainingTime,
                             percentComplete: input.Counter.Percent,
                             currentOperation: currentOperation);
    }
    internal static BarOutput FormatClassicMiniView(BarInput input)
    {
        string activity = FitString(input.Activity, input.LineWidth - 1);

        // In minimal classic view, only activity and status are shown
        // percents and time remaining are not present
        int reservedSpace = 4; // padding

        string status = input.Counter.GetShortPercentString(input.Visible, input.LineWidth - reservedSpace);
        string time = input.VisibleRemainingTime >= 0 ? TimeSpan.FromSeconds(input.RemainingTime).ToString() : "";

        if (status.Length > 1 && status[status.Length - 1] != ' ')
        {
            status += " ";
        }
        status += FitString(time, input.LineWidth - reservedSpace - status.Length);

        if (status.Length > 1 && status[status.Length - 1] != ' ')
        {
            status += " ";
        }
        status += input.Counter.GetCounterString(input.Visible, input.LineWidth - reservedSpace - status.Length);

        if (status.Length > 1 && status[status.Length - 1] != ' ')
        {
            status += " ";
        }
        status += FitString(input.FormattedItem, input.LineWidth - reservedSpace - status.Length);

        int remainingTime = input.Visible.HasFlag(Elements.TimeRemaining) ? input.RemainingTime : -1;

        return new BarOutput(activity: activity,
                             status: status,
                             remainingTime: remainingTime,
                             percentComplete: input.Counter.Percent,
                             currentOperation: string.Empty);
    }

    /// <summary>
    /// Creates a BarOutput for the current style of progress bar.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="isViewMinimal"></param>
    /// <returns></returns>
    internal static BarOutput FormatView(BarInput input, bool isViewMinimal)
    {
        if (isViewMinimal)
        {
            Debug.WriteLine("MINIMAL");
            return FormatMinimalView(input);
        }
        if (input.LineHeight > 17)
        {
            Debug.WriteLine("FULL CLASSIC");
            return FormatClassicFullView(input);
        }
        if (input.LineHeight > 5)
        {
            Debug.WriteLine("MID CLASSIC");
            return FormatClassicMidView(input);
        }
        Debug.WriteLine("MINI CLASSIC");
        return FormatClassicMiniView(input);
    }
}
