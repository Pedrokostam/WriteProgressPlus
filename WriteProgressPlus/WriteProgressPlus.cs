using System.Management.Automation;

namespace WriteProgressPlus;
[Cmdlet(VerbsCommunications.Write, "ProgressPlus")]
[Alias("WriPro")]
[CmdletBinding(PositionalBinding = false)]
public sealed class WriteProgressPlus : ProgressBase
{
    [Parameter(
        HelpMessage = "Unique ID of progress bar. " +
        "While IDs is shared with ordinary Write-Progress, " +
        "this module offset all IDs, sho there should not be any conflict."
        )]
    [ValidateRange(0, int.MaxValue)]
    public int ID { get; set; } = 1;

    [Parameter(
        HelpMessage = "ID of parent progress bar. Used to create sub-bars. To make parent independent, set to a negative value."
        )]
    public int ParentID { get; set; } = -1;

    [Parameter(
        HelpMessage = "Activity description. Will be showed before progress bar."
        )]
    [ValidateNotNull]
    public string Activity { get; set; } = "Processing...";

    [Parameter(
        HelpMessage = "Total count of expected iterations. " +
        "If positive, will enable showing percent done (and accurate progress length) and time remaining."
        )]
    [ValidateRange(1, int.MaxValue)]
    public int TotalCount { get; set; } = -1;

    [Parameter(
        HelpMessage = "How much to increase the CurrentIteration if it was not specified. " +
        "If CurrentIteration is specified, Increment is ignored. Set to zero to freeze the progress bar."
        )]
    public int Increment { get; set; } = 1;

    [Parameter(
        HelpMessage = "Overrides the calculated iteration. Works similar to its analogue in WriteProgress"
        )]
    public int CurrentIteration { get; set; } = -1;

    [Parameter(ValueFromPipeline = true,
        HelpMessage = "Current object. If specified, can be used for formatting status."
        )]
    public object? InputObject { get; set; }

    [Parameter(
        HelpMessage = "If true, disables calculation of remaining time."
        )]
    public SwitchParameter NoETA { get; set; }

    [Parameter(
        HelpMessage = "Scriptblock used for formatting status string. " +
        "The script receives 4 parameters: InputObject, CurrentIteration, PercentDone, TotalCount. " +
        "Use $Args[0] to $Args[3] to access them. Alternatively, you can use their aliases: $_, $c, $p, $t, respectively. " +
        "Will override DisplayProperties."
        )]
    public ScriptBlock? DisplayScript { get; set; }

    [Parameter(
        HelpMessage = "List of property names of the input object to format into status. " +
        "You can use wildcard, for example if the InputObject is a DateTime, specifying *seconds will give both Seconds and Milliseconds. " +
        "Overriden by DisplayScript."
        )]
    public string[]? DisplayProperties { get; set; }

    [Parameter(
        HelpMessage = "If DisplayProperties are specified, this string will be used to join them."
        )]
    [ValidateNotNull]
    public string DisplayPropertiesSeparator { get; set; } = ", ";

    [Parameter(
        HelpMessage = "If true, hides object (and its formatting) from status"
        )]
    public SwitchParameter HideObject { get; set; }
    [Parameter(
        HelpMessage = "If true, hides counter from status"
        )]
    public SwitchParameter NoCounter { get; set; }
    [Parameter(
         HelpMessage = "If true, hides percentage from status"
        )]
    public SwitchParameter NoPercentage { get; set; }

    [Parameter(
        HelpMessage = "If true and in pipeline, will emit the InputObject further. " +
        "If this command is in the middle of pipeline, this parameter is forced to true.")]
    public SwitchParameter PassThru { get; set; } // PassThru is only in pipeline, so no need to store its state


    internal readonly ItemFormatter Formatter = new();
    private bool MiddleOfPipe { get; set; }
    private bool PipelineMode { get; set; }
    private bool EmitItem { get; set; }
    private ProgressInner? BarWorker { get; set; }
    protected override void BeginProcessing()
    {
        ID += Offset;
        int pipePosition = MyInvocation.PipelinePosition;
        int pipeLength = MyInvocation.PipelineLength;
        MiddleOfPipe = pipeLength > pipePosition;
        PipelineMode = MyInvocation.ExpectingInput;
        EmitItem = PassThru || MiddleOfPipe;
        WriteDebug(PipelineMode ? "Pipeline mode" : "Iterative mode");
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