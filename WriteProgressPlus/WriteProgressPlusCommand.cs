using System.Management.Automation;
using WriteProgressPlus.Components;

namespace WriteProgressPlus;
[Cmdlet(VerbsCommunications.Write, "ProgressPlus")]
[Alias("WriPro")]
[CmdletBinding(PositionalBinding = false)]
public sealed class WriteProgressPlusCommand : ProgressBase
{
    [Parameter()]
    [ValidateRange(0, int.MaxValue)]
    public int ID { get; set; } = 1;

    [Parameter()]
    public int ParentID { get; set; } = -1;

    [Parameter()]
    [ValidateNotNull]
    public string Activity { get; set; } = "Processing...";

    [Parameter()]
    [ValidateRange(1, int.MaxValue)]
    public int TotalCount { get; set; } = -1;

    [Parameter()]
    public int Increment { get; set; } = 1;

    [Parameter()]
    public int CurrentIteration { get; set; } = -1;

    [Parameter(ValueFromPipeline = true)]
    public object? InputObject { get; set; }

    [Parameter()]
    public SwitchParameter NoETA { get; set; }

    [Parameter()]
    public ScriptBlock? DisplayScript { get; set; }

    [Parameter()]
    public string[]? DisplayProperties { get; set; }

    [Parameter()]
    [ValidateNotNullOrEmpty]
    public string DisplayPropertiesSeparator { get; set; } = ", ";

    [Parameter()]
    public SwitchParameter HideObject { get; set; }

    [Parameter()]
    public SwitchParameter NoCounter { get; set; }

    [Parameter()]
    public SwitchParameter NoPercentage { get; set; }

    [Parameter()]
    public SwitchParameter PassThru { get; set; } // PassThru is only in pipeline, so no need to store its state


    internal readonly ItemFormatter Formatter = new();
    private bool MiddleOfPipe { get; set; }
    private bool PipelineMode { get; set; }
    private bool EmitItem { get; set; }
    private ProgressInner? BarWorker { get; set; }
    protected override void BeginProcessing()
    {
        ID += Offset;
        ParentID += Offset;
        int pipePosition = MyInvocation.PipelinePosition;
        int pipeLength = MyInvocation.PipelineLength;
        MiddleOfPipe = pipeLength > pipePosition;
        PipelineMode = MyInvocation.ExpectingInput;
        EmitItem = PassThru || MiddleOfPipe;
#if DEBUG
        WriteDebug(PipelineMode ? "Pipeline mode" : "Iterative mode");
#endif
        Formatter.Update(DisplayScript, DisplayProperties, DisplayPropertiesSeparator);
        try
        {
            BarWorker = GetProgressInner(ID, ParentID, CommandRuntime);
        }
        catch (ArgumentException e)
        {
            ThrowTerminatingError(new ErrorRecord(e, e.Source, ErrorCategory.InvalidArgument, this));
        }
        catch (InvalidOperationException e)
        {
            ThrowTerminatingError(new ErrorRecord(e, e.Source, ErrorCategory.InvalidOperation, this));
        }
    }

    protected override void ProcessRecord()
    {
        BarWorker!.UpdateRecord(this);
        BarWorker!.WriteProgress(this);
        if (EmitItem)
        {
            WriteObject(InputObject);
        }
    }
    protected override void EndProcessing()
    {
        if (PipelineMode)
        {
            RemoveProgressInner(ID);
        }
    }
    protected override void StopProcessing() => EndProcessing();
    //  ACTIV[STATUS]
    //      SUBACTIV[SUBSTATUS]

    //  ACTIV STATUS[oo]         
    //    SUBACTIV SUBSTATUS[oooooooooooooooooooooooooooooooooooooooooooo]

}