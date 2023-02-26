using System.Globalization;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace WriteProgressPlus;
[Cmdlet(VerbsCommon.Get, "PipelineItemCount")]
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
    [Parameter(ParameterSetName = "Format")]
    public object? FormatProvider { get; set; }


    private IFormatProvider? provider;
    private int Count = 0;
    private bool isAdvanced = false;
    protected override void BeginProcessing()
    {
        if (ParameterSetName != "Format") return;
        provider = FormatProvider switch
        {
            string name => new CultureInfo(name),
            int num => new CultureInfo(num),
            IFormatProvider formatProvider => formatProvider,
            _ => CultureInfo.CurrentCulture,
        };
        try
        {

            if (Format != null && AdvancedFormat().IsMatch(Format))
            {
                _ = string.Format(provider, Format, Count);
                isAdvanced = true;
            }
            else
            {
                _ = Count.ToString(Format, provider);
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
    }
    protected override void EndProcessing()
    {
        if (ParameterSetName == "Format")
        {
            if (isAdvanced)
                WriteObject(string.Format(provider, Format!, Count));
            else
                WriteObject(Count.ToString(Format, provider));
        }
        else
        {
            WriteObject(Count);
        }
    }

#if NET46
    private readonly static Regex AdvancedFormat_46 = new("{\\s*0.*}", RegexOptions.Compiled);
    private static Regex AdvancedFormat() => AdvancedFormat_46;
#else
    [GeneratedRegex("{\\s*0.*}")]
    private static partial Regex AdvancedFormat();
#endif
}
