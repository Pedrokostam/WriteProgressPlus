using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            if (scs.Contains("$_"))
            {
                scs = scs.Replace("$_", "$args");
                Script = ScriptBlock.Create(scs);
            }
            else
            {
                Script = script;
            }

        }
        Properties = props;
        PropertiesSeparator = sep ?? " ";
    }
    public string? FormatItem(object? item)
    {
        Components.Clear();
        if (item is null) return null;
        if (Script is not null)
        {
            return Script.InvokeReturnAsIs(item).ToString();
        }
        else if (Properties is not null && Properties.Length > 0)
        {
            if (item is PSObject pso)
                GetPropertyOfPsObject(pso);
            else
                GetPropertyOfNormalObject(item);
            if (Components.Count == 0) return null;
            return string.Join(PropertiesSeparator, Components);
        }
        else
        {
            return item.ToString();
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

    [GeneratedRegex(@"[\*\?]")]
    private static partial Regex WildcardDetector();
    [GeneratedRegex(@"(\\\*)|(\\\?)")]
    private static partial Regex WildcardReplacer();
}
