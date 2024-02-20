using System.Management.Automation;
using static System.FormattableString;
namespace WriteProgressPlus.Components;
public class ProgressBase : PSCmdlet
{
    internal const int Offset = 2137;

    internal static readonly Dictionary<int, ProgressInner> ProgressDict = [];

    public static ProgressInner GetProgressInner(WriteProgressPlusCommand current)
    {
        if (ProgressDict.TryGetValue(current.ID, out ProgressInner? pi))
        {
            if (current.KeepState.IsPresent || pi.HistoryId == current.HistoryId)
            {
                // If the history ids match, or if user requested state keeping return the existing state
                return pi;
            }
            else // HistoryId is different and user does not want to keep state
            {
                RemoveProgressInner(current.ID);
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

    public static bool RemoveProgressInner(int id)
    {
        if (ProgressDict.TryGetValue(id, out ProgressInner? progressInner))
        {
            progressInner.AssociatedRecord.RecordType = ProgressRecordType.Completed;
            progressInner.AssociatedRecord.PercentComplete = 100;
            progressInner.WriteProgress(null);
#if DEBUG
            progressInner?.CmdRuntime?.WriteDebug(Invariant($"Removed state for progress bar {id - Offset}"));
#endif
            ProgressDict.Remove(id);
            return true;
        }
        return false;
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
