using System.Globalization;
using System.Management.Automation;
using System.Text;

namespace WriteProgressPlus.Components;
public sealed class ProgressInner
{
    /// <summary>
    /// In milliseconds
    /// </summary>
    private readonly TimeSpan Negative = TimeSpan.FromSeconds(-1);
    private readonly string Placeholder = "placeholder";
    public ProgressInner(int id, int parentId, ICommandRuntime cmdr)
    {
        Id = id;
        ParentId = parentId < 0 ? -1 : parentId;
        Keeper = new TimeKeeper();
        AssociatedRecord = new(id, Placeholder, Placeholder);
        CmdRuntime = cmdr;
    }
    /// <summary>
    /// I found no other way to make sure that progress bar are reused, other than using the same CommandRuntime that was used to create it.
    /// 
    /// </summary>
    ICommandRuntime CmdRuntime { get; }
    public int Id { get; }
    public int ParentId { get; }
    internal TimeKeeper Keeper { get; }
    internal ProgressRecord AssociatedRecord { get; }

    public int CurrentIteration { get; private set; }
    public void NewIteration(int increment, int currentIter)
    {
        if (currentIter > 0)
            CurrentIteration = currentIter;
        else
            CurrentIteration += increment;
        Keeper.AddTime();
    }
    /// <summary>
    /// Single stringbuiled to avoid making more objects.
    /// </summary>
    private StringBuilder StatusBuilder { get; } = new StringBuilder();
    public TimeSpan GetRemainingTime(int totalCount)
    {
        int left = totalCount - CurrentIteration;
        if (left < 0) return Negative;
        return Keeper.GetAverage().Multiply(left);
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
    public bool ShouldDisplay() => Keeper.ShouldDisplay();

    internal void UpdateRecord(WriteProgressPlus donor)
    {
        NewIteration(donor.Increment, donor.CurrentIteration);
        StatusBuilder.Clear();
        int percentage;
        bool overflow = false;
        if (donor.TotalCount > 0)
        {
            percentage = CurrentIteration * 100 / donor.TotalCount;
            if (percentage > 100)
            {
                overflow = true;
                percentage = 100;
            }
        }
        else
        {
            int scale = 10;
            TimeSpan elapsed = DateTime.Now - Keeper.StartTime;
            int modulo = (int)elapsed.TotalSeconds % scale;
            int x = 100 - (scale - modulo) * (int)(100.0 / scale);
            percentage = x >= 100 ? 99 : x < 0 ? 0 : x;
        }
        if (!donor.HideObject && donor.InputObject is not null)
        {
            object[] package = [donor.InputObject, CurrentIteration, percentage, donor.TotalCount];
            if (donor.Formatter.FormatItem(package) is string s)
            {
                StatusBuilder.Append(s);
                if (!donor.NoPercentage.IsPresent || !donor.NoCounter.IsPresent)
                {
                    StatusBuilder.Append(" - ");
                }
            }
        }

        if (!donor.NoCounter)
        {
            StatusBuilder.Append(CurrentIteration.ToString("d3", CultureInfo.CurrentCulture));
            if (donor.TotalCount > 0)
            {
                StatusBuilder.Append('/').Append(donor.TotalCount).Append(' ');
            }
            else
            {
                StatusBuilder.Append(' ');
            }
        }
        if (donor.TotalCount > 0 && (!donor.NoPercentage || overflow))
        {
            if (overflow)
                StatusBuilder.Append("[Incorrect total count]");
            else
            {
                StatusBuilder.Append(percentage.ToString("d2", CultureInfo.CurrentCulture)).Append('%');
            }
        }
        UpdateAssociatedRecord(donor, percentage);
    }

    private void UpdateAssociatedRecord(WriteProgressPlus donor, int percentage)
    {
        int remainingSeconds = -1;
        if (!donor.NoETA && donor.TotalCount > 0)
        {
            remainingSeconds = (int)GetRemainingTime(donor.TotalCount).TotalSeconds;
        }
        AssociatedRecord.StatusDescription = StatusBuilder.ToString();
        AssociatedRecord.RecordType = ProgressRecordType.Processing;
        AssociatedRecord.Activity = donor.Activity;
        AssociatedRecord.SecondsRemaining = remainingSeconds;
        AssociatedRecord.ParentActivityId = donor.ParentID >= 0 ? donor.ParentID : -1;
        AssociatedRecord.PercentComplete = percentage;
    }
}
