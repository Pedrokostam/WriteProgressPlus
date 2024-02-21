using System.Management.Automation;
using System.Diagnostics;
using static System.FormattableString;
namespace WriteProgressPlus.Components;
public class ProgressBase : PSCmdlet
{
    internal const int Offset = 2137;

    internal static readonly Dictionary<int, ProgressInner> ProgressDict = [];

    public static ProgressInner GetProgressInner(WriteProgressPlusCommand current)
    {
        if (ProgressDict.TryGetValue(current.ID, out ProgressInner? existingProgressInner))
        {
            if (current.KeepState.IsPresent || existingProgressInner.HistoryId == current.HistoryId)
            {
                // If the history ids match, or if user requested state keeping, return the existing state
                return existingProgressInner;
            }
            else // HistoryId is different and user does not want to keep state
            {
                // Do not write the comlete bar - due to pwsh7 throttling the complete update of bar
                // will make the new bar not display
                // From powershell point of view the bar never went away, just changed activity, etc...
                RemoveProgressInner(current.ID, false);
                return AddNewProgressInner(current);
            }
        }
        else
        {
            return AddNewProgressInner(current);
        }
    }

    private static ProgressInner AddNewProgressInner(WriteProgressPlusCommand current)
    {
        ProgressInner p = new(current);
        ProgressDict.Add(current.ID, p);
        return p;
    }
    public static bool RemoveProgressInner(int id) => RemoveProgressInner(id, true);
    private static bool RemoveProgressInner(int id, bool writeCompleted)
    {
        if (!ProgressDict.TryGetValue(id, out ProgressInner? progressInner))
        {
            return false;
        }
        if (writeCompleted)
        {
            // Set recordtype to completed
            // According to documentation, each bar should be written once with RecordType set to Completed
            // Tihs ensures that the bar will be removed
            progressInner.AssociatedRecord.RecordType = ProgressRecordType.Completed;
            progressInner.AssociatedRecord.PercentComplete = 100;
            progressInner.WriteProgress();
            // However, if the state is being removed, because a new bar from different command is requested
            // We should not complete it - the bar should be reused
            // Thus we skip this section altogether if writeCompleted is false
        }
        Debug.WriteLine(Invariant($"Removed state for progress bar {id - Offset}"));
        ProgressDict.Remove(id);
        return true;
    }

    public void ClearProgressInners()
    {
        var keys = ProgressDict.Keys.ToArray();
        foreach (int id in keys)
        {
            RemoveProgressInner(id);
        }
    }
}
