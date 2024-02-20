using System.Management.Automation;
using System.Reflection;
using System.Text.RegularExpressions;
using static System.FormattableString;
namespace WriteProgressPlus.Components;
public partial class ItemFormatter
{
    private readonly static string DefaultSeparator = " ";
    private readonly static TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);

    readonly List<object?> Components = [];

    public ScriptBlock? Script { get; set; }

    public string[] Properties { get; set; } = [];

    public string PropertiesSeparator { get; set; } = DefaultSeparator;

    public void Update(ScriptBlock? script, string[]? props, string? sep)
    {
        if (script is null)
        {
            Script = null;
        }
        else
        {
            string scs = script.ToString().Trim();
            string replaced = AliasDetector.Replace(scs, AliasReplacer);
            if (replaced.Length == 0) // empty script block
            {
                Script = null;
            }
            else if (scs.Length != replaced.Length) // something was replaced, need to create new block
            {
                Script = ScriptBlock.Create(replaced);
            }
            else // everything was the same, reuse already existing block
            {
                Script = script;
            }

        }
        Properties = props ?? [];
        PropertiesSeparator = sep ?? DefaultSeparator;
    }

    /// <summary>
    /// Formats input items according to <see cref="Properties"/> or <see cref="Script"/>.
    /// </summary>
    /// <param name="objects">CurrentItem, CurrentIteration, Percentage, ETA</param>
    /// <returns>Formatted string. Can be null.</returns>
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
        if (Properties.Length > 0)
        {
            return GetFormattedProperties(firstObject);
        }
        else
        {
            return firstObject?.ToString();
        }
    }

    private string? GetFormattedProperties(object firstObject)
    {
        // items from pipeline will actually be PSObjects
        // only specifying it as -InputObject will give the raw object
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

    void GetPropertyOfPsObject(PSObject pso)
    {
        // Uses PSObject's built in Match method for handling wildcards
        PSMemberInfoCollection<PSPropertyInfo> objectProps = pso.Properties;
        foreach (string name in Properties)
        {
            foreach (var matchedProperty in objectProps.Match(name))
            {
                if (matchedProperty is not null)
                {
                    Components.Add(matchedProperty.Value);
                }
            }
        }
    }

    void GetPropertyOfNormalObject(object obj)
    {
        Type t = obj.GetType();
        var allProperties = t.GetProperties();
        foreach (string propertyName in Properties!)
        {
            // Replaced every isntance of * and ? with its regex equivalent
            // If nothing is replaced the method should return the same instance
            string pattern = WildcardDetector.Replace(propertyName, Evaluator);

            // If pattern is the same as propertyName, propertyName contained no wildcards
            if (pattern.Equals(propertyName, StringComparison.Ordinal))
            {
                MatchNormalProp(obj, allProperties, propertyName);
            }
            else // If WildcardDetector replaced something, it means the pattern contains wildcards
            {
                MatchNormapProp_Wild(obj, allProperties, pattern);
            }
        }
    }

    private void MatchNormalProp(object obj, PropertyInfo[] allProperties, string name)
    {
        foreach (var property in allProperties)
        {
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                Components.Add(property.GetValue(obj));
            }
        }
    }

    private void MatchNormapProp_Wild(object obj, PropertyInfo[] allProperties, string pattern)
    {
        foreach (var property in allProperties)
        {
            if (Regex.IsMatch(property.Name, pattern, RegexOptions.IgnoreCase, RegexTimeout))
            {
                Components.Add(property.GetValue(obj));
            }
        }
    }

    private readonly static MatchEvaluator AliasReplacer = new(ReplaceAlias);

    /// <summary>
    /// There are 3 possible wildcard: *, ?, []. The brackets can be left as they are, because they regex-compliant
    /// This detector detects asterisks (*) and question marks (?)
    /// </summary>
    private readonly static Regex WildcardDetector = new(@"[\*\?]", RegexOptions.Compiled, RegexTimeout);

    private readonly static Regex AliasDetector = new(@"\$(?<alias>[_ctpCTP])", RegexOptions.Compiled, RegexTimeout);

    private readonly static MatchEvaluator Evaluator = new(ReplaceWildcard);

    private static string ReplaceAlias(Match match)
    {
        int aliasIndex = match.Groups["alias"].Value switch
        {
            "_" => 0,
            "c" or "C" => 1,
            "p" or "P" => 2,
            "t" or "T" => 3,
            _ => throw new NotSupportedException(),
        };
        return Invariant($"$($args[{aliasIndex}])");
    }

    private static string ReplaceWildcard(Match m)
    {
        string matchedString = m.Value;
        return matchedString switch
        {
            "*" => ".*",
            "?" => ".",
            _ => matchedString,
        };
    }
}