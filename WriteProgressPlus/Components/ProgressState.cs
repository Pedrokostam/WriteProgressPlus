using System.Management.Automation;
using System.Text;
using static System.Globalization.CultureInfo;
using static WriteProgressPlus.Components.PowershellVersionDifferences;

namespace WriteProgressPlus.Components;

internal sealed class ProgressState
{
    private readonly TimeSpan Negative = TimeSpan.FromSeconds(-1);

    private readonly string Placeholder = "placeholder";

    public ProgressState(WriteProgressPlusCommand donor)
    {
        Id = donor.ID;
        ParentId = donor.ParentID < ProgressBaseCommand.Offset ? -1 : donor.ParentID;

        // Let the calculation length about 1/20 of the total length, still subject to minimum, maximum and calculation lengths in  Buffer
        int timeCalculationLength = donor.TotalCount / 20;
        Keeper = new TimeKeeper(timeCalculationLength);

        AssociatedRecord = new ProgressRecord(donor.ID, Placeholder, Placeholder);

        // try to reuse parentRuntime
        ICommandRuntime? parentRuntime = ParentId > 0 ? ProgressBaseCommand.ProgressDict[ParentId].CmdRuntime : null;
        CmdRuntime = parentRuntime ?? donor.CommandRuntime;

        HistoryId = donor.HistoryId;
    }

    /// <summary>
    /// What actually calls WriteProgress.
    /// 
    /// I found no other way to make sure that progress bar are reused,
    /// other than using the same CommandRuntime that was used to create it.
    /// </summary>
    public ICommandRuntime CmdRuntime { get; }

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
            // Negative iteration means we need to calculate it from increment
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
        if (IsThrottlingBuiltIn(CmdRuntime))
        {
            // The updates may be very frequent, but the built-in throttling will handle it.
            return true;
        }
        return Keeper.UpdatedPermitted();
    }

    /// <summary>
    /// Calculate percent done using donor's TotalCount and <see cref="ActualCurrentIteration"/>.
    /// </summary>
    /// <param name="donor"></param>
    /// <returns>Tuple with calculated percentage and a flag whether current iteration is greater than total count.</returns>
    private (int percentage, bool overflow) GetPercentage(WriteProgressPlusCommand donor)
    {
        int percentage;
        bool overflow = false;
        if (donor.TotalCount > 0)
        {
            percentage = ActualCurrentIteration * 100 / donor.TotalCount;
            if (percentage > 100)
            {
                overflow = true;
                percentage = 100;
            }
        }
        else // No TotalCount means we cannot calculate percentage
        {
            percentage = 0;
        }
        return (percentage, overflow);
    }

    internal void UpdateRecord(WriteProgressPlusCommand donor)
    {
        StatusBuilder.Clear();
        StartNewIteration(donor);
        (int percentage, bool overflow) = GetPercentage(donor);
        AppendFormattedItem(donor, percentage);
        AppendCounter(donor);
        AppendPercentage(donor, percentage, overflow);
        int remainingSeconds = GetRemainingSeconds(donor);
        UpdateAssociatedRecord(donor, percentage, remainingSeconds);
    }

    private void UpdateAssociatedRecord(WriteProgressPlusCommand donor, int percentage, int remainingSeconds)
    {
        AssociatedRecord.StatusDescription = StatusBuilder.ToString();
        AssociatedRecord.RecordType = ProgressRecordType.Processing;
        AssociatedRecord.Activity = donor.Activity;
        AssociatedRecord.SecondsRemaining = remainingSeconds;
        AssociatedRecord.ParentActivityId = donor.ParentID >= ProgressBaseCommand.Offset ? donor.ParentID : -1;
        AssociatedRecord.PercentComplete = percentage;
    }

    /// <summary>
    /// Appends percent done: {d2}% or [Incorrect total count] depending on overflow.
    /// <para/>
    /// Skips if donor has at least one of the following:
    /// <list type="bullet">
    ///     <item>
    ///         Negative <see cref="WriteProgressPlusCommand.TotalCount"/>
    ///     </item>
    ///     <item>
    ///         Present <see cref="WriteProgressPlusCommand.NoPercentage"/>
    ///     </item>
    /// </list>
    /// </summary>
    /// <param name="donor"></param>
    /// <param name="percentage"></param>
    /// <param name="overflow"></param>
    private void AppendPercentage(WriteProgressPlusCommand donor, int percentage, bool overflow)
    {
        if (donor.TotalCount <= 0 || donor.NoPercentage)
        {
            return;
        }

        if (overflow)
        {
            StatusBuilder.Append("[Incorrect total count]");
        }
        else
        {
            StatusBuilder.AppendFormat(InvariantCulture, "{0:d2}%", percentage);
        }
    }

    /// <summary>
    /// Appends counter: (current / total) or (current).
    /// </summary>
    /// <param name="donor"></param>
    private void AppendCounter(WriteProgressPlusCommand donor)
    {
        if (donor.NoCounter)
        {
            return;
        }

        StatusBuilder.AppendFormat(InvariantCulture, "{0:d3}", ActualCurrentIteration);
        if (donor.TotalCount > 0) // append the total
        {
            StatusBuilder.Append('/').Append(donor.TotalCount);
        }
        StatusBuilder.Append(' '); // space for possible next parts
    }

    /// <summary>
    /// Appends the formatted (either from properties or from script).
    /// </summary>
    /// <param name="donor"></param>
    /// <param name="percentage">Percent done to be used as one of four script parameters.</param>
    private void AppendFormattedItem(WriteProgressPlusCommand donor, int percentage)
    {
        // object hidden or null - skip
        if (donor.HideObject || donor.InputObject is null)
        {
            return;
        }
        object[] package = [donor.InputObject, ActualCurrentIteration, percentage, donor.TotalCount];
        if (donor.Formatter.FormatItem(package) is string formatted)
        {
            StatusBuilder.Append(formatted);
            bool statusHasOnlyItem = donor.NoPercentage.IsPresent && donor.NoCounter.IsPresent;
            // If there's more than object part in status - append a dash
            if (!statusHasOnlyItem)
            {
                StatusBuilder.Append(" - ");
            }
        }
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
            remainingSeconds = (int)GetRemainingTime(donor.TotalCount).TotalSeconds;
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
