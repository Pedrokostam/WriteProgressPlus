using System.Management.Automation;

namespace WriteProgressPlus.Components;
public class ProgressBase : PSCmdlet
{
    protected const int Offset = 2137;
    internal static readonly Dictionary<int, ProgressInner> ProgressDict = new();
    public static ProgressInner GetProgressInner(int id, int parentid, ICommandRuntime cmdr)
    {
        if (ProgressDict.TryGetValue(id, out ProgressInner? pi))
        {
            return pi;
        }
        else
        {
            ProgressInner p = new(id, parentid, cmdr);
            ProgressDict.Add(id, p);
            return p;
        }
    }
    public bool RemoveProgressInner(int id)
    {
        if (ProgressDict.TryGetValue(id, out ProgressInner? progressInner))
        {
            progressInner.AssociatedRecord.RecordType = ProgressRecordType.Completed;
            progressInner.AssociatedRecord.PercentComplete = 100;
            progressInner.WriteProgress(this);
            WriteDebug($"Removed state for progress bar {id - Offset}");
            ProgressDict.Remove(id);
            return true;
        }
        return false;
    }
    public void ClearProgressInners()
    {
        foreach (int id in ProgressDict.Keys)
        {
            RemoveProgressInner(id);
        }
    }
}
