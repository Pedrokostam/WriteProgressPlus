using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace WriteProgressPlus.Components.Layout;
/// <summary>
/// Contains data for:
/// <list type="bullet">
/// <item>iteration</item>
/// <item>total count</item>
/// <item>percentage</item>
/// </list>
/// Can create counter string, calculate percentage (with proper values for undetermined cases).
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct Counter
{
    public const int BarDisabled = -1;
    public const string Separator = "/";

    public readonly int Iteration;
    public readonly int Total;

    public Counter()
    {
        Iteration = -1;
        Total = -1;
    }

    public Counter(int iteration, int total)
    {
        Iteration = iteration;
        Total = total;
    }

    /// <summary>
    /// Checks whether Total is positive
    /// </summary>
    public bool IsTotalProvided => Total > 0;
    /// <summary>
    /// Checks whether Current iteration is positive or zero
    /// </summary>
    public bool IsIterationProvided => Iteration >= 0;

    /// <summary>
    /// Return the calculated percentage, or -1, if either total or current are not provided.
    /// </summary>
    public int Percent
    {
        get
        {
            if (IsTotalProvided && IsIterationProvided && Iteration <= Total)
            {
                return Iteration * 100 / Total;
            }
            return BarDisabled;
        }
    }

    public bool KnownTotal => Total > 0;

    /// <summary>
    /// Create the text form of counter with percents: {current}/{total} ({percent})
    /// <para/>
    /// If total is non-positive or <paramref name="elements"/> does not have it, its part will be skipped. 
    /// <para/>
    /// If current iteration is non-positive or <paramref name="elements"/> does not have it, its part will be skipped. 
    /// <para/>
    /// Skipping one part leave just the other, with no additional characters.
    /// <para/>
    /// If percent is non-positive or <paramref name="elements"/> does not have it, its part will be skipped. 
    /// <para/>
    /// Skipping all parts in an empty string.
    /// If only percent is present, it will be displayed without parentheses
    /// <para/>
    /// The result will fit in <paramref name="maxLength"/>, removing elements as necessary. The elements will be removed in order:
    /// <list type="number">
    ///     <item>Percent</item>
    ///     <item>Total</item>
    ///     <item>Current</item>
    /// </list>
    /// </summary>
    /// <param name="elements">Which elements user wants to be visible in status.</param>
    /// <param name="maxLength">Space in which the result must fit.</param>
    /// <returns>Concatenation of current, total and percent, one or two of the elements, or an empty string </returns>
    /// <returns></returns>
    public string GetTextForm(Elements elements, int maxLength)
    {
        // [counter_part] ([percent_part])
        string counterPart = GetCounterString(elements, maxLength);
        // if we have a counter part, percents will be in parentheses, after a space
        // otherwise, there will be only percents or nothing
        int percentLengthReserved = counterPart == "" ? counterPart.Length + 3 : 0;
        string percentPart = GetPercentString(elements, maxLength - percentLengthReserved);

        string result = (counterPart, percentPart) switch
        {
            ({ Length: 0 }, { Length: 0 }) => string.Empty,
            ({ Length: > 0 }, { Length: 0 }) => counterPart,
            ({ Length: 0 }, { Length: > 0 }) => percentPart,
            ({ Length: > 0 }, { Length: > 0 }) => $"{counterPart} ({percentPart})",

        };

        return result;

    }
    /// <summary>
    /// Create the text form of counter: {current}/{total}
    /// <para/>
    /// If total is non-positive or <paramref name="elements"/> does not have it, its part will be skipped. 
    /// <para/>
    /// If current iteration is non-positive or <paramref name="elements"/> does not have it, its part will be skipped. 
    /// <para/>
    /// Skipping one part leave just the other, with no additional characters.
    /// <para/>
    /// Skipping both results in an empty string.
    /// <para/>
    /// The result will fit in <paramref name="maxLength"/>, removing elements as necessary. The elements will be removed in order:
    /// <list type="number">
    ///     <item>Total</item>
    ///     <item>Current</item>
    /// </list>
    /// </summary>
    /// <param name="elements">Which elements user wants to be visible in status.</param>
    /// <param name="maxLength">Space in which the result must fit.</param>
    /// <returns>Concatenation of current and total with separator, one of the elements, or an empty string </returns>
    public string GetCounterString(Elements elements, int maxLength)
    {
        bool iterationPresent = elements.HasFlag(Elements.Iteration) && IsIterationProvided;
        bool totalPresent = elements.HasFlag(Elements.TotalCount) && IsTotalProvided;
        if (!iterationPresent && !totalPresent || maxLength == 0)
        {
            // Neither part of counter is requested (or we don;t have any space)
            return string.Empty;
        }
        string total = string.Empty;
        string iteration = string.Empty;
        if (totalPresent)
        {
            total = Total.ToString(CultureInfo.InvariantCulture);
        }
        if (iterationPresent)
        {
            iteration = Iteration.ToString(CultureInfo.InvariantCulture);
        }

        if (!totalPresent && iterationPresent)
        {
            return iteration;
        }
        if (totalPresent && !iterationPresent)
        {
            return total;
        }

        int partLength = Math.Max(total.Length, iteration.Length);
        // possible length includes iteration part padded to at least the width of total count.
        int possibleLength = partLength * 2 + Separator.Length;

        if (possibleLength > maxLength)
        {
            // If 3 parts are longer than maxLength, try to remove total count
            // iteration count is more important
            if (iteration.Length <= maxLength)
            {
                // return iteration
                return iteration;
            }
            else
            {
                // we cannot even fit current iteration - return null
                return string.Empty;
            }
        }
        // All three parts can fit

        // PadLeft, since we want the iteration part to have at least as many character as the total part
        // this way, the total width will not change with iteration (at least while below totalcount)
        // PadLeft returns unchanged instance if nothing is done.
        iteration = iteration.PadLeft(total.Length, '0');

        return $"{iteration}/{total}";
    }

    /// <summary>
    /// Formats the percentage to string with 2 digit format.
    /// <para/>
    /// If percentage is negative, or it's not requested in <paramref name="elements"/> it will be an empty string.
    /// If the result would be longer than <paramref name="maxLength"/>, an empty string will be returned.
    /// </summary>
    /// <param name="elements">Which elements user wants to be visible in status.</param>
    /// <param name="maxLength">Space in which the result must fit.</param>
    /// <returns></returns>
    public string GetPercentString(Elements elements, int maxLength) => GetPercentStringImpl(elements, maxLength, @"00\%");

    /// <summary>
    /// Formats the percentage to string with 1 digit format.
    /// <para/>
    /// If percentage is negative, or it's not requested in <paramref name="elements"/> it will be an empty string.
    /// If the result would be longer than <paramref name="maxLength"/>, an empty string will be returned.
    /// </summary>
    /// <param name="elements">Which elements user wants to be visible in status.</param>
    /// <param name="maxLength">Space in which the result must fit.</param>
    /// <returns></returns>
    public string GetShortPercentString(Elements elements, int maxLength) => GetPercentStringImpl(elements, maxLength, @"0\%");

    /// <summary>
    /// Formats the percentage to string with given format.
    /// <para/>
    /// If percentage is negative, or it's not requested in <paramref name="elements"/> it will be an empty string.
    /// If the result would be longer than <paramref name="maxLength"/>, an empty string will be returned.
    /// </summary>
    /// <param name="elements">Which elements user wants to be visible in status.</param>
    /// <param name="maxLength">Space in which the result must fit.</param>
    /// <returns></returns>
    private string GetPercentStringImpl(Elements elements, int maxLength, string format)
    {
        if (Percent < 0 || !elements.HasFlag(Elements.Percentage))
        {
            return string.Empty;
        }
        string p = Percent.ToString(format, CultureInfo.InvariantCulture);
        if (p.Length > maxLength)
        {
            // can't really trim it
            return string.Empty;
        }
        return p;
    }
}
