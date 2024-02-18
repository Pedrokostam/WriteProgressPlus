using System.Globalization;
using System.Management.Automation;
using System.Text.RegularExpressions;
using WriteProgressPlus.Components;

namespace WriteProgressPlus;
[Cmdlet(VerbsCommon.Get, "PipelineItemCount")]
[Alias("Count")]
[CmdletBinding(PositionalBinding = false, DefaultParameterSetName = "Default")]
[OutputType(typeof(int), ParameterSetName = new[] { "Default" })]
[OutputType(typeof(string), ParameterSetName = new[] { "Format" })]
public partial class GetPipelineItemCount : PSCmdlet
{
    [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public object? InputItem { get; set; }
    [Parameter(ParameterSetName = "Format")]
    [ValidateNotNullOrEmpty]
    public string? Format { get; set; }
    [Parameter(ParameterSetName = "FormatProvider")]
    [ArgumentFormatProviderTransformation()]
    public IFormatProvider? FormatProvider { get; set; } = null;

    [Parameter]
    public SwitchParameter PassThru { get; set; }

    [Parameter]
    public SwitchParameter ToHost { get; set; }

    private int Count = 0;
    private bool isAdvanced = false;
    protected override void BeginProcessing()
    {
        if (ParameterSetName == "Default")
            return;
        try
        {
            // Try to format count using format and formatprovider
            if (Format != null && AdvancedFormat().IsMatch(Format))
            {
                //if format contains brackets we need to pass an argument
                _ = string.Format(FormatProvider, Format, Count);
                isAdvanced = true;
            }
            else
            {
                _ = Count.ToString(Format, FormatProvider);
                isAdvanced = false;
            }
        }
        catch (FormatException)
        {
            throw;
        }
    }

    protected override void ProcessRecord()
    {
        Count++;
        if (PassThru.IsPresent)
        {
            WriteObject(InputItem);
        }
    }
    protected override void EndProcessing()
    {
        if (ParameterSetName == "Format")
        {
            string message;
            if (isAdvanced)
                message = string.Format(FormatProvider, Format!, Count);
            else
                message = Count.ToString(Format, FormatProvider);
            if (ToHost.IsPresent)
            {
                WriteInformation(message, new string[] { });
            }
            else
            {
                WriteObject(Count.ToString(Format, FormatProvider));
            }
        }
        else
        {
            WriteObject(Count);
        }
    }
    private static Regex AdvancedFormat() => new("{\\s*0.*}", RegexOptions.Compiled);
}
