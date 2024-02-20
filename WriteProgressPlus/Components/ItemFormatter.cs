using System.Management.Automation;
using System.Reflection;
using System.Text.RegularExpressions;
using static System.FormattableString;
namespace WriteProgressPlus.Components;
public partial class ItemFormatter
{
    private readonly static string DefaultSeparator = " ";
    private readonly static TimeSpan RegexTimeout = TimeSpan.FromSeconds(2);

    readonly List<object?> Components = [];

    public ScriptBlock? Script { get; set; }

    public string[]? Properties { get; set; }

    public string? PropertiesSeparator { get; set; }

    public void Update(ScriptBlock? script, string[]? props, string? sep)
    {
        if (script is null) Script = null;
        else
        {
            string scs = script.ToString();
            string replaced = AliasDetector.Replace(scs, AliasReplacer);
            if (scs.Length != replaced.Length)
            {
                Script = ScriptBlock.Create(replaced);
            }
            else
            {
                Script = script;
            }

        }
        Properties = props;
        PropertiesSeparator = sep ?? DefaultSeparator;
    }

    public string? FormatItem(params object[]? objects)
    {
        Components.Clear();
        if (objects is null)
        {
            return null;
        }

        if (Script is not null)
        {
            return Script.InvokeReturnAsIs(objects)?.ToString();
        }

        object firstObject = objects[0];

        if (Properties is not null && Properties.Length > 0)
        {
            if (firstObject is PSObject pso)
            {
                GetPropertyOfPsObject(pso);
            }
            else
            {
                GetPropertyOfNormalObject(firstObject);
            }

            if (Components.Count == 0)
            {
                return null;
            }

            return string.Join(PropertiesSeparator, Components);
        }
        else
        {
            return firstObject?.ToString();
        }
    }

    void GetPropertyOfPsObject(PSObject pso)
    {
        PSMemberInfoCollection<PSPropertyInfo> objectProps = pso.Properties;
        foreach (string name in Properties!)
        {
            foreach (var x in objectProps.Match(name))
            {
                if (x is not null)
                {
                    Components.Add(x.Value);
                }
            }
        }
    }

    void GetPropertyOfNormalObject(object obj)
    {
        Type t = obj.GetType();
        var allprops = t.GetProperties();
        foreach (string name in Properties!)
        {
            if (WildcardDetector.IsMatch(name))
            {
                MatchNormalProp_Wildcard(obj, allprops, name);
            }
            else
            {
                MatchNormalProp(obj, allprops, name);
            }
        }
    }

    private void MatchNormalProp(object obj, PropertyInfo[] allprops, string name)
    {
        foreach (var oprop in allprops)
        {
            if (oprop.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                Components.Add(oprop.GetValue(obj));
            }
        }
    }

    private void MatchNormalProp_Wildcard(object obj, PropertyInfo[] allprops, string name)
    {
        MatchEvaluator eval = new(ReplaceWildcard);
        string pattern = WildcardReplacer.Replace(Regex.Escape(name), eval);
        foreach (var oprop in allprops)
        {
            if (Regex.IsMatch(oprop.Name, pattern, RegexOptions.IgnoreCase, RegexTimeout))
            {
                Components.Add(oprop.GetValue(obj));
            }
        }
    }

    private readonly static MatchEvaluator AliasReplacer = new(ReplaceAlias);

    private readonly static Regex WildcardDetector = new(@"[\*\?]", RegexOptions.Compiled, RegexTimeout);

    private readonly static Regex WildcardReplacer = new(@"(\\\*)|(\\\?)", RegexOptions.Compiled | RegexOptions.ExplicitCapture, RegexTimeout);

    private readonly static Regex AliasDetector = new(@"\$(?<alias>[_ctpCTP])", RegexOptions.Compiled, RegexTimeout);

    private static string ReplaceAlias(Match m)
    {
        int num = m.Groups["alias"].Value switch
        {
            "_" => 0,
            "c" or "C" => 1,
            "p" or "P" => 2,
            "t" or "T" => 3,
            _ => throw new NotSupportedException(),
        };
        return Invariant($"$($args[{num}])");
    }

    private static string ReplaceWildcard(Match m)
    {
        if (string.Equals(m.Value, @"\*", StringComparison.Ordinal))
        {
            return ".*";
        }
        return ".";
    }
}