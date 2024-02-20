using System.Management.Automation;
using WriteProgressPlus.Components;
using static System.FormattableString;
namespace WriteProgressPlus;
[Cmdlet(VerbsCommon.Reset, "ProgressPlus")]
[OutputType(typeof(void))]
[Alias("ResPro")]
[CmdletBinding(PositionalBinding = true, DefaultParameterSetName = "NORMAL")]
public sealed class ResetProgressPlusCommand : ProgressBase
{
    [Parameter(Position=0,ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName ="NORMAL")]
    public int[]? ID { get; set; }

    [Parameter(ParameterSetName="ALL")]
    public SwitchParameter All { get; set; }

    protected override void BeginProcessing()
    {
        if (All.IsPresent)
        {
            int count = ProgressDict.Count;
            ClearProgressInners();
            WriteVerbose(Invariant($"Removed all progress bars - {count}"));
        }
    }

    protected override void ProcessRecord()
    {
        if (ID != null)
        {
            foreach (int i in ID)
            {
                if (RemoveProgressInner(i + Offset))
                    WriteVerbose(Invariant($"Removed progress bar - {i}"));
            }
        }
        else if (!All.IsPresent)
        {
            ErrorRecord ec = new(
                new InvalidOperationException("Neither -All nor any ID were specified."),
                "Parameter error",
                ErrorCategory.InvalidArgument,
                this);
            WriteError(ec);
        }
    }
}
