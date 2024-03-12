using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WriteProgressPlus.Components;
internal class ProgressViewFormatter
{
    const string CutOffString = "~";
    private static (string activity, int innerProgressLength) GetMinimalParts(int linewidth, string activity)
    {
        // brackets, required spaces
        const int minimalViewReservedSpace = 5;
        int maxActivityLength = linewidth / 2; // half of width, rounded down
        int activityLength = Math.Min(maxActivityLength, activity.Length); //whatever is shorter
        int innerBarLength = linewidth - activityLength - minimalViewReservedSpace;
        string activityString;
        if (activityLength == activity.Length)
        {
            activityString = activity;
        }
        else
        {
            activityString = activity.Substring(0, activityLength - CutOffString.Length) + CutOffString;
        }
        return (activityString,
            innerBarLength);
    }
    private static string? Crop(string str, int length, bool partialCutOff)
    {
        if (str.Length <= length)
        {
            return str;
        }
        else if (partialCutOff)
        {
            return str.Substring(0, length - CutOffString.Length) + CutOffString;
        }
        return null;
    }
    private static string GetMinimalInnerProgress(
        string? item,
        string? couter,
        string? percentage,
        int remainingTime,
        int innerBarLength)
    {
        const int timePadding = 1;
        int timeLength;
        if (remainingTime > 0)
        {
            timeLength = ((int)Math.Log10(remainingTime) + 1) + timePadding;
        }
        else
        {
            timeLength = 0;
        }
        int statusLength = innerBarLength - timeLength;
        StringBuilder builder = new(statusLength + 1);
        Append(builder, couter, statusLength, true);
        Append(builder, item, statusLength, true);
        Append(builder, percentage, statusLength, false);
        var finalStatus = builder.ToString(0, statusLength);
        
    }

    private static void Append(StringBuilder builder, string? couter, int maxLength, bool partialCutoff)
    {
        int availableLength = maxLength - builder.Length;
        if (couter is not null)
        {
            if (couter.Length <= availableLength)
            {
                builder.Append(couter);
            }
            else if (Crop(couter, availableLength, partialCutoff) is string s)
            {

                builder.Append(s);
            }
            else
            {
                return;
            }
            builder.Append(' ');
        }
    }

    static (string status, string currentOperation) GetMinimalView(
        string? item,
        string? couter,
        string? percentage,
        int remainingTime,
        string activity,
        int lineWidth)
    {
        (string actualActivity, int innerProgressLength) = GetMinimalParts(lineWidth, activity);
        var status = GetMinimalInnerProgress(item, couter, percentage, remainingTime, innerProgressLength);
    }
}
