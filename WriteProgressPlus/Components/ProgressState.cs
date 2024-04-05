using System.Diagnostics;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using WriteProgressPlus.Components.Layout;
using WriteProgressPlus.Components.Time;
using static System.Globalization.CultureInfo;
using static WriteProgressPlus.Settings.PowerShellFeatures;

namespace WriteProgressPlus.Components;

internal sealed class ProgressState
{
    private readonly TimeSpan Negative = TimeSpan.FromSeconds(-1);

    private readonly string Placeholder = "placeholder";

    public ProgressState(WriteProgressPlusCommand donor)
    {
        Id = donor.Id;
        ParentId = donor.ParentId < ProgressBaseCommand.Offset ? -1 : donor.ParentId;

        // Let the calculation length about 1/20 of the total length, still subject to minimum, maximum and calculation lengths in  Buffer
        int timeCalculationLength = donor.TotalCount / 20;
        Keeper = new TimeKeeper(timeCalculationLength);

        AssociatedRecord = new ProgressRecord(donor.Id, Placeholder, Placeholder);

        // try to reuse parentRuntime
        SessionRuntime? parentSessionRuntime = ParentId > 0 ? ProgressBaseCommand.ProgressDict[ParentId].SessionRuntime : null;
        SessionRuntime = parentSessionRuntime ?? new SessionRuntime(
            donor.SessionState, donor.CommandRuntime);

        HistoryId = donor.HistoryId;
    }

    /// <summary>
    /// What actually calls WriteProgress.
    /// 
    /// I found no other way to make sure that progress bar are reused,
    /// other than using the same CommandRuntime that was used to create it.
    /// </summary>
    private ICommandRuntime CmdRuntime => SessionRuntime.CommandRuntime;

    public SessionRuntime SessionRuntime { get; }

    public int Id { get; }

    public int ParentId { get; }

    internal TimeKeeper Keeper { get; }

    internal ProgressRecord AssociatedRecord { get; }

    public long HistoryId { get; }

    /// <summary>
    /// Actual iteration number, incremented automatically or specified by user.
    /// </summary>
    public int ActualCurrentIteration { get; private set; }

    /// <summary>
    /// Calculate <see cref="ActualCurrentIteration"/> using information from donor.
    /// If donor's CurrentIteration is negative, use its Increment to calculate it.
    /// If it's positive, use donor's value.
    /// <para/>
    /// Also appends new time for ETA-calculation purposes.
    /// </summary>
    /// <param name="donor">Currently called instance of <see cref="WriteProgressPlusCommand"/></param>
    public void StartNewIteration(WriteProgressPlusCommand donor)
    {
        ActualCurrentIteration = donor.CurrentIteration switch
        {
            // Non-negative iteration means it was specified by user
            >= 0 => donor.CurrentIteration,
            // FallbackValue iteration means we need to calculate it from increment
            _ => ActualCurrentIteration + donor.Increment,
        };
        Keeper.AddTime(ActualCurrentIteration);
    }

    /// <summary>
    /// Single StringBuilder to avoid making more objects. Used to create status message.
    /// </summary>
    private StringBuilder StatusBuilder { get; } = new StringBuilder();

    /// <summary>
    /// Calculates remaining time to completion based on total count and average time per iteration.
    /// </summary>
    /// <param name="totalCount">
    ///     How many iterations in total wil happen.
    ///     <para/>If it is less than <see cref="ActualCurrentIteration"/> calculations are disabled.
    /// </param>
    /// <returns>TimeSpan with calculated time, or TimeSpan of negative 1 second if calculation is disabled.</returns>
    public TimeSpan GetRemainingTime(int totalCount)
    {
        int iterationsLeft = totalCount - ActualCurrentIteration;
        return iterationsLeft switch
        {
            < 0 => Negative, // iterations exceeded total count, cannot calculate
            _ => Keeper.GetAverage().Multiply(iterationsLeft),
        };
    }

    public bool ShouldUpdate()
    {
        if (HasThrottlingBuiltIn)
        {
            // The updates may be very frequent, but the built-in throttling will handle it.
            return true;
        }
        return Keeper.UpdatedPermitted();
    }


    private Elements GetElements(WriteProgressPlusCommand donor)
    {
        Elements elements = Elements.All;
        if (donor.NoETA)
        {
            elements &= ~Elements.TimeRemaining;
        }
        if (donor.NoCounter)
        {
            elements &= ~Elements.Counter;
        }
        if (donor.NoPercentage)
        {
            elements &= ~Elements.Percentage;
        }
        if (donor.HideObject)
        {
            elements &= ~(Elements.ItemScript | Elements.ItemProperties);
        }
        return elements;
    }

    internal void UpdateRecord(WriteProgressPlusCommand donor)
    {
        ProgressAreaLayout buffer = ProgressAreaLayout.GetProgressLayout(donor);
        Debug.WriteLine(buffer);
        StatusBuilder.Clear();
        StartNewIteration(donor);
        var visibleElements = GetElements(donor);
        var counter = new Counter(ActualCurrentIteration, donor.TotalCount);
        var formattedItem = GetFormattedItem(donor, counter.Percent);
        int remainingSeconds = GetRemainingSeconds(donor);
        var input = new BarInput(formattedItem, counter, donor.Activity, remainingSeconds, buffer, visibleElements);
        BarOutput output = LayoutFormatter.FormatView(input);
        UpdateAssociatedRecord(output, donor.ParentId);
    }

    private void UpdateAssociatedRecord(BarOutput output, int parentID)
    {
        AssociatedRecord.StatusDescription = output.Status;
        AssociatedRecord.RecordType = ProgressRecordType.Processing;
        AssociatedRecord.Activity = output.Activity;
        AssociatedRecord.SecondsRemaining = output.RemainingTime;
        AssociatedRecord.ParentActivityId = parentID >= ProgressBaseCommand.Offset ? parentID : -1;
        AssociatedRecord.PercentComplete = output.PercentComplete;
        AssociatedRecord.CurrentOperation = output.CurrentOperation;
    }

    private string? GetFormattedItem(WriteProgressPlusCommand donor, int percentage)
    {
        // object hidden or null - skip
        if (donor.HideObject || donor.InputObject is null)
        {
            return null;
        }
        object[] package = [donor.InputObject, ActualCurrentIteration, percentage, donor.TotalCount];
        if (donor.Formatter.FormatItem(package) is string formatted)
        {
            return formatted;
        }
        return null;
    }

    /// <summary>
    /// Calculates remaining time
    /// </summary>
    /// <param name="donor"></param>
    /// <returns>Seconds to finish progress bar.</returns>
    private int GetRemainingSeconds(WriteProgressPlusCommand donor)
    {
        int remainingSeconds = -1;
        if (!donor.NoETA && donor.TotalCount > 0)
        {
            var temp = GetRemainingTime(donor.TotalCount).TotalSeconds;
            remainingSeconds = temp > int.MaxValue ? int.MaxValue : (int)temp;
        }

        return remainingSeconds;
    }

    /// <summary>
    /// Makes ICommandRuntime associated with the ProgressState call its WriteProgress.
    /// Can be skipped by throttling, unless <paramref name="force"/> is <see langword="true"/>
    /// </summary>
    public void WriteProgress(bool force = false)
    {
        if (force || ShouldUpdate())
        {
            CmdRuntime?.WriteProgress(AssociatedRecord);
        }
    }
}
