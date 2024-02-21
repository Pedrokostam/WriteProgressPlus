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
                // If the history ids match, or if user requested state keeping, return the existing state
                return pi;
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
    public static bool RemoveProgressInner(int id)
    {
        return RemoveProgressInner(id, true);
    }
    private static bool RemoveProgressInner(int id, bool writeCompleted)
    {
        if (!ProgressDict.TryGetValue(id, out ProgressInner? progressInner))
        {
            return false;
        }
        if (writeCompleted)
        {
            // Set recordtype to completed
            // DOn't know if it does something though, might want to test it
            // TODO: Check if RecordType being completed does anything
            progressInner.AssociatedRecord.RecordType = ProgressRecordType.Completed;
            progressInner.AssociatedRecord.PercentComplete = 100;
            progressInner.WriteProgress(null);
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
