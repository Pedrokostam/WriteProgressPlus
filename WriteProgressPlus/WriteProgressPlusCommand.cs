using System.Management.Automation;
using System.Diagnostics;
using WriteProgressPlus.Components;
using static WriteProgressPlus.Components.PowershellVersionDifferences;

namespace WriteProgressPlus;

[Cmdlet(VerbsCommunications.Write, "ProgressPlus")]
[Alias("WriPro")]
[CmdletBinding(PositionalBinding = false)]
public sealed class WriteProgressPlusCommand : ProgressBaseCommand, IDynamicParameters
{
    [Parameter]
    [ValidateRange(0, int.MaxValue)]
    public int ID { get; set; } = 1;

    [Parameter]
    public int ParentID { get; set; } = -1;

    [Parameter]
    [ValidateNotNull]
    public string Activity { get; set; } = "Processing...";

    [Parameter]
    [Alias("Count")]
    [CollectionToLengthTransformation]
    public int TotalCount { get; set; } = -1;

    [Parameter]
    [ValidateRange(0, int.MaxValue)]
    public int Increment { get; set; } = 1;

    [Parameter]
    [Alias("Iteration")]
    public int CurrentIteration { get; set; } = -1;

    [Parameter(ValueFromPipeline = true)]
    public object? InputObject { get; set; }

    [Parameter]
    [Alias("Script")]
    public ScriptBlock? DisplayScript { get; set; }

    [Parameter]
    [SupportsWildcards]
    [Alias("Properties")]
    public string[]? DisplayProperties { get; set; }

    [Parameter]
    [ValidateNotNull]
    [Alias("Separator")]
    public string DisplayPropertiesSeparator { get; set; } = ", ";

    [Parameter]
    public SwitchParameter HideObject { get; set; }

    [Parameter]
    public SwitchParameter NoCounter { get; set; }

    [Parameter]
    public SwitchParameter NoPercentage { get; set; }

    [Parameter]
    public SwitchParameter NoETA { get; set; }

    [Parameter]
    public SwitchParameter PassThru { get; set; }

    [Parameter]
    [Alias("Persist")]

    public SwitchParameter KeepState { get; set; }

    internal readonly ItemFormatter Formatter = new();

    private bool MiddleOfPipe { get; set; }

    private bool PipelineMode { get; set; }

    private bool EmitItem { get; set; }

    internal long HistoryId => MyInvocation.HistoryId;

    private ProgressState BarWorker { get; set; } = default!;

    private NoThrottleDynamicParameter? NoThrottleDynamicParam { get; set; }

    internal bool DisableThrottling => NoThrottleDynamicParam?.NoThrottle.IsPresent ?? false;

    protected override void BeginProcessing()
    {
        if (ID == ParentID)
        {
            var errorRecord = new ErrorRecord(
                new IdConflictException(),
                "ParentIdSameAsId",
                ErrorCategory.InvalidArgument,
                this
                );
            ThrowTerminatingError(errorRecord);
        }
        ID += Offset;
        ParentID += Offset;

        int pipePosition = MyInvocation.PipelinePosition;
        int pipeLength = MyInvocation.PipelineLength;

        MiddleOfPipe = pipeLength > pipePosition;
        PipelineMode = MyInvocation.ExpectingInput;
        EmitItem = PassThru || MiddleOfPipe;

        Debug.WriteLine(PipelineMode ? "Pipeline mode" : "Iterative mode");

        // Update formatter with new format sources
        Formatter.Update(DisplayScript, DisplayProperties, DisplayPropertiesSeparator);
        try
        {
            BarWorker = GetProgressState(this);
        }
        catch (ArgumentException argumentException)
        {
            var errorRecord = new ErrorRecord(argumentException,
                                              errorId: argumentException.Source,
                                              errorCategory: ErrorCategory.InvalidArgument,
                                              targetObject: this);
            ThrowTerminatingError(errorRecord);
        }
        catch (InvalidOperationException invalidException)
        {
            var errorRecord = new ErrorRecord(invalidException,
                                              errorId: invalidException.Source,
                                              errorCategory: ErrorCategory.InvalidOperation,
                                              targetObject: this);
            ThrowTerminatingError(errorRecord);
        }
    }

    protected override void ProcessRecord()
    {
        BarWorker.UpdateRecord(this);
        BarWorker.WriteProgress(force: DisableThrottling);
        if (EmitItem)
        {
            WriteObject(InputObject);
        }
    }

    protected override void EndProcessing()
    {
        // If user requested to keep state, we won't be removing anything
        // Also applies to pipeline mode
        if (KeepState.IsPresent)
        {
            return;
        }
        // In pipeline mode, we want to complete the bar and remove it at the end
        if (PipelineMode)
        {
            RemoveProgressState(ID);
        }
    }

    protected override void StopProcessing() => EndProcessing();

    public object? GetDynamicParameters()
    {
        if (HasThrottlingBuiltIn)
        {
            return null;
        }
        NoThrottleDynamicParam = new NoThrottleDynamicParameter();
        return NoThrottleDynamicParam;
    }
}