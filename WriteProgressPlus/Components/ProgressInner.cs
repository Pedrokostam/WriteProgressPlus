using System.Drawing;
using System.Management.Automation;
using System.Text;
using static System.Globalization.CultureInfo;
namespace WriteProgressPlus.Components;
public sealed class ProgressInner
{
    private readonly TimeSpan Negative = TimeSpan.FromSeconds(-1);

    private readonly string Placeholder = "placeholder";

    public ProgressInner(int id, int parentId, ICommandRuntime cmdr)
    {
        Id = id;
        ParentId = parentId < ProgressBase.Offset ? -1 : parentId;
        Keeper = new TimeKeeper();
        AssociatedRecord = new(id, Placeholder, Placeholder);
        // try to reuse parentRuntime
        ICommandRuntime? parentRuntime = ParentId > 0 ? ProgressBase.ProgressDict[ParentId].CmdRuntime : null;
        CmdRuntime = parentRuntime ?? cmdr;
    }

    /// <summary>
    /// What actually calls WriteProgress.
    /// 
    /// I found no other way to make sure that progress bar are reused,
    /// other than using the same CommandRuntime that was used to create it.
    /// </summary>
    ICommandRuntime CmdRuntime { get; }

    public int Id { get; }

    public int ParentId { get; }

    internal TimeKeeper Keeper { get; }

    internal ProgressRecord AssociatedRecord { get; }

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
        if (donor.CurrentIteration > 0)
            ActualCurrentIteration = donor.CurrentIteration;
        else
            ActualCurrentIteration += donor.Increment;
        Keeper.AddTime();
    }

    /// <summary>
    /// Single stringbuilder to avoid making more objects. Used to create status message.
    /// </summary>
    private StringBuilder StatusBuilder { get; } = new StringBuilder();

    public TimeSpan GetRemainingTime(int totalCount)
    {
        int left = totalCount - ActualCurrentIteration;
        if (left < 0) return Negative;
        return Keeper.GetAverage().Multiply(left);
    }

    /// <inheritdoc cref="TimeKeeper.ShouldDisplay"/>
    public bool ShouldDisplay() => Keeper.ShouldDisplay();

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
        AssociatedRecord.StatusDescription = StatusBuilder.ToString();
        AssociatedRecord.RecordType = ProgressRecordType.Processing;
        AssociatedRecord.Activity = donor.Activity;
        AssociatedRecord.SecondsRemaining = remainingSeconds;
        AssociatedRecord.ParentActivityId = donor.ParentID >= ProgressBase.Offset ? donor.ParentID : -1;
        AssociatedRecord.PercentComplete = percentage;
    }

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

    public void WriteProgress(Cmdlet parent)
    {
        if (!ShouldDisplay()) return;

        if (parent.CommandRuntime == CmdRuntime)
        {
            parent.WriteProgress(AssociatedRecord);
        }
        else
        {
            CmdRuntime.WriteProgress(AssociatedRecord);
        }
    }
}
