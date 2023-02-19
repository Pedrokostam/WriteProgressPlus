using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace WriteProgressPlus;
public class ProgressBase : PSCmdlet
{
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
        if (ProgressDict.Remove(id, out ProgressInner? progressInner))
        {
            progressInner.AssociatedRecord.RecordType = ProgressRecordType.Completed;
            progressInner.AssociatedRecord.PercentComplete = 100;
            progressInner.WriteProgress(this);
            WriteDebug($"Removed state for progress bar {id}");
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
