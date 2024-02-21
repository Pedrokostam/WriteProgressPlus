using System.Management.Automation;
using System.Diagnostics;
using static System.FormattableString;
namespace WriteProgressPlus.Components;
public class ProgressBaseCommand : PSCmdlet
{
    internal const int Offset = 2137;

    internal static readonly Dictionary<int, ProgressState> ProgressDict = [];

    /// <summary>
    /// Looks for the state associated with the given instance (by its ID).
    /// If no matching state is found, creates a new one and returns it.
    /// <para/>
    /// If the state comes from different historyID, it may be removed and created anew (depending on settings of <paramref name="current"/>)
    /// </summary>
    /// <param name="current"></param>
    /// <returns></returns>
    public static ProgressState GetProgressInner(WriteProgressPlusCommand current)
    {
        if (ProgressDict.TryGetValue(current.ID, out ProgressState? existingProgressInner))
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

    private static ProgressState AddNewProgressInner(WriteProgressPlusCommand current)
    {
        ProgressState p = new(current);
        ProgressDict.Add(current.ID, p);
        return p;
    }

    /// <summary>
    /// Removes progress bar state associated with the given id. Does nothing if id is not associated with anything.
    /// <para/>
    /// If state is removed, its bar will be updated once with RecordType set to complete.
    /// </summary>
    /// <param name="id">ID of ProgressState</param>
    /// <returns></returns>
    public static bool RemoveProgressInner(int id) => RemoveProgressInner(id, writeCompleted: true);

    private static bool RemoveProgressInner(int id, bool writeCompleted)
    {
        if (!ProgressDict.TryGetValue(id, out ProgressState? progressInner))
        {
            return false;
        }
        if (writeCompleted)
        {
            // Set recordtype to completed
            // According to documentation, each bar should be written once with RecordType set to Completed
            // This ensures that the bar will be removed
            progressInner.AssociatedRecord.RecordType = ProgressRecordType.Completed;
            progressInner.AssociatedRecord.PercentComplete = 100;
            progressInner.WriteProgress();
            // However, if the state is being removed, because a new bar from different command is requested
            // We should not complete it - the bar should be reused
            // Thus we skip this section altogether if writeCompleted is false
        }
        Debug.WriteLine(Invariant($"Removed state for progress bar {id - Offset} (was {progressInner.ActualCurrentIteration} iteration)"));
        ProgressDict.Remove(id);
        return true;
    }

    /// <summary>
    /// Removes all progress bar states
    /// </summary>
    public void ClearProgressInners()
    {
        var keys = ProgressDict.Keys.ToArray();
        foreach (int id in keys)
        {
            RemoveProgressInner(id);
        }
    }
}
