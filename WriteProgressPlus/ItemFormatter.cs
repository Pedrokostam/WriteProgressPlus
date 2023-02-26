using System.Management.Automation;
using System.Reflection;
using System.Text.RegularExpressions;

namespace WriteProgressPlus;
public partial class ItemFormatter
{
    readonly List<object?> Components = new();
    public ScriptBlock? Script { get; set; }
    public string[]? Properties { get; set; }
    public string? PropertiesSeparator { get; set; }

    public void Update(ScriptBlock? script, string[]? props, string? sep)
    {
        if (script is null) Script = null;
        else
        {
            string scs = script.ToString();
            string replaced = AliasDetector().Replace(scs, AliasReplacer);
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
        PropertiesSeparator = sep ?? " ";
    }
    public string? FormatItem(params object[]? objects)
    {
        Components.Clear();
        if (objects is null) return null;
        if (Script is not null)
        {
            return Script.InvokeReturnAsIs(objects).ToString();
        }
        else if (Properties is not null && Properties.Length > 0)
        {
            if (objects[0] is PSObject pso)
                GetPropertyOfPsObject(pso);
            else
                GetPropertyOfNormalObject(objects[0]);
            if (Components.Count == 0) return null;
            return string.Join(PropertiesSeparator, Components);
        }
        else
        {
            return objects[0].ToString();
        }
    }
    void GetPropertyOfPsObject(PSObject pso)
    {
        PSMemberInfoCollection<PSPropertyInfo> objectProps = pso.Properties;
        foreach (string name in Properties!)
        {
            foreach (var x in objectProps.Match(name))
            {
                if (x != null)
                    Components.Add(x.Value);
            }
        }
    }
    void GetPropertyOfNormalObject(object obj)
    {
        Type t = obj.GetType();
        var allprops = t.GetProperties();
        foreach (string name in Properties!)
        {
            if (WildcardDetector().IsMatch(name))
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
        string pattern = WildcardReplacer().Replace(Regex.Escape(name), eval);
        foreach (var oprop in allprops)
        {
            if (Regex.IsMatch(oprop.Name, pattern, RegexOptions.IgnoreCase))
            {
                Components.Add(oprop.GetValue(obj));
            }
        }
    }

    private string ReplaceWildcard(Match m)
    {
        if (m.Value == @"\*")
            return ".*";
        else
            return ".";
    }
    private static readonly MatchEvaluator AliasReplacer = new(ReplaceAlias);
#if NET46
    private readonly static Regex WildcardDetector_46 = new Regex(@"[\*\?]", RegexOptions.Compiled);
    private readonly static Regex WildcardReplacer_46 = new Regex(@"(\\\*)|(\\\?)", RegexOptions.Compiled);
    private readonly static Regex AliasDetector_46 = new Regex(@"\$(?<alias>[_ctpCTP])", RegexOptions.Compiled);
    private static Regex WildcardDetector() => WildcardDetector_46;
    private static Regex WildcardReplacer() => WildcardReplacer_46;
    private static Regex AliasDetector() => AliasDetector_46;
#else
    [GeneratedRegex(@"[\*\?]")]
    private static partial Regex WildcardDetector();
    [GeneratedRegex(@"(\\\*)|(\\\?)")]
    private static partial Regex WildcardReplacer();

    [GeneratedRegex(@"\$(?<alias>[_ctpCTP])")]
    private static partial Regex AliasDetector();
#endif
    private static string ReplaceAlias(Match m)
    {
        int num = m.Groups["alias"].Value switch
        {
            "_" => 0,
            "c" or "C" => 1,
            "p" or "P" => 2,
            "t" or "T" => 3,
            _ => throw new NotImplementedException(),
        };
        return $"$($args[{num}])";
    }
}