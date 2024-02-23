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

    // Saves given values inside this instance, so that they can be used when formatting a future item.
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
    public string? FormatItem(params object?[]? objects)
    {
        Components.Clear();
        if (objects is null)
        {
            return null;
        }

        // Script has priority
        if (Script is not null)
        {
            return Script.InvokeReturnAsIs(objects)?.ToString();
        }

        object? firstObject = objects[0];
        if (Properties.Length > 0)
        {
            return GetFormattedProperties(firstObject);
        }
        else
        {
            return firstObject?.ToString();
        }
    }

    /// <summary>
    /// Return a string made of values of selected <see cref="Properties"/>, concatenated with <see cref="PropertiesSeparator"/>.
    /// If no property was selected, returns null.
    /// </summary>
    /// <param name="objectToFormat"></param>
    /// <returns>String of property values or null</returns>
    private string? GetFormattedProperties(object? objectToFormat)
    {
        if (objectToFormat is null)
        {
            return null;
        }
        // items from pipeline will actually be PSObjects
        // only specifying it as -InputObject will give the raw object
        if (objectToFormat is PSObject pso)
        {
            GetPropertyOfPsObject(pso);
        }
        else
        {
            GetPropertyOfNormalObject(objectToFormat);
        }

        if (Components.Count == 0)
        {
            return null;
        }

        return string.Join(PropertiesSeparator, Components);
    }

    /// <summary>
    /// Given a PSObject, selects properties matching given pattern (possible with wildcards).
    /// Fills <see cref="Components"/> with matching properties' values.
    /// </summary>
    /// <param name="pso"></param>
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

    /// <summary>
    /// Given a non-PSObject, goes through each of its properties
    /// and selects those matching given pattern (possible with wildcards).
    /// Fills <see cref="Components"/> with matching properties' values.
    /// </summary>
    /// <param name="obj"></param>
    void GetPropertyOfNormalObject(object obj)
    {
        if (obj is PSObject)
        {
            throw new InvalidOperationException($"Method {nameof(GetPropertyOfNormalObject)} cannot be used on a PSObject");
        }
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

    /// <summary>
    /// Given a non-wildcard pattern compares each property's name to the pattern (case insensitive).
    /// If the property matches, its value is added to <see cref="Components"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="allProperties"></param>
    /// <param name="name"></param>
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

    /// <summary>
    /// Uses the given pattern as a case-insensitive regex pattern and applies it to each property.
    /// If the property matches, its value is added to <see cref="Components"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="allProperties"></param>
    /// <param name="regexPattern">Valid regex pattern</param>
    private void MatchNormapProp_Wild(object obj, PropertyInfo[] allProperties, string regexPattern)
    {
        foreach (var property in allProperties)
        {
            if (Regex.IsMatch(property.Name, regexPattern, RegexOptions.IgnoreCase, RegexTimeout))
            {
                Components.Add(property.GetValue(obj));
            }
        }
    }


    /// <summary>
    /// There are 3 possible wildcard: *, ?, []. The brackets can be left as they are, because they regex-compliant
    /// This detector detects asterisks (*) and question marks (?)
    /// </summary>
    private readonly static Regex WildcardDetector = new(@"[\*\?]", RegexOptions.Compiled, RegexTimeout);

    private readonly static Regex AliasDetector = new(@"\$(?<alias>[_ctpCTP])", RegexOptions.Compiled, RegexTimeout);

    private readonly static MatchEvaluator Evaluator = new(ReplaceWildcard);

    private readonly static MatchEvaluator AliasReplacer = new(ReplaceAlias);

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