namespace WriteProgressPlus.Tests;

using System.Drawing;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Serialization;
using WriteProgressPlus;
using WriteProgressPlus.Components;

[TestClass]
public class FormatterTests
{
    static PowerShell Pwsh { get; }

    public static ItemFormatter GetFormatter(ScriptBlock? script = null, string[]? properties = null, string? separator = null)
    {
        var formatter = new ItemFormatter();
        formatter.Update(script, properties, separator);
        return formatter;
    }
    public static readonly DateTime TestDate = new(year: 2005,
                                                     month: 4,
                                                     day: 2,
                                                     hour: 21,
                                                     minute: 37,
                                                     second: 5,
                                                     millisecond: 69,
                                                     microsecond: 966);
    static FormatterTests()
    {
        Pwsh = PowerShell.Create();
        Runspace.DefaultRunspace = Pwsh.Runspace;
        Wrapper = ScriptBlock.Create("$args[0]");
    }

    static ScriptBlock Wrapper { get; }
    public static PSObject? GetAsPsObject(object? obj)
    {
        // Code below will wrap a null value into a PSObject, which is not what happens within the pipeline
        // In the pipeline, object with value become wrapped, but null is null
        if (obj is null)
            return null;
        var res = Wrapper.Invoke(obj);
        return res[0];

    }
    public PSObject TestDatePSObject = default!;
    public static readonly string[] Wildcard_PropertyPattern = ["*second"];
    public static readonly object[] Wildcard_PropertyTargets = [5, 69, 966, 0];

    public static readonly string[] Standard_PropertyPattern = ["hour", "minute"];
    public static readonly object[] Standard_PropertyTargets = [21, 37];

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required for dynamic data")]
    public static string DefaultFormatterTestValuesDisplayName(MethodInfo methodInfo, object[] data)
    {
        var first = data[0];
        if (first is null)
        {
            return "<null value>";
        }
        return $"{first.GetType().Name} - {data[0]}";
    }
    public static IEnumerable<object?[]> DefaultFormatterTestValues =>
    [
        [TestDate],
        [GetAsPsObject(TestDate)],
        ["Jimbo"],
        [21.37],
        [2137],
        [new Point(21,37)],
        [Guid.NewGuid()],
        [null],
    ];
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required for dynamic data")]
    public static string CaseSensitiveTestValuesDisplayName(MethodInfo methodInfo, object[] data)
    {
        var obj = data[0];
        var props = (string[])data[1];
        if (obj is null)
        {
            return $"<null value> - {string.Join(", ", props)}";
        }
        return $"{obj.GetType().Name} - {string.Join(", ", props)} - {obj}";
    }
    public static IEnumerable<object?[]> CaseSensitiveTestValues
    {
        get
        {
            (object?, string[])[] items =
            [
                (TestDate, ["year", "month", "year"]),
                (TestDate, ["*second"]),
                (GetAsPsObject(TestDate), ["year", "month", "year"]),
                (GetAsPsObject(TestDate), ["*second"]),
                ("Jimbo", ["length"]),
                ("Jimbo", ["*gth"]),
                (new Point(21, 37), ["x", "y"]),
                (new Point(21, 37), ["[xy]"]),
                (new int[]{1,2,3 },["length"]),
            ];
            return items.Select(item => new object?[] { item.Item1, item.Item2 });
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required for dynamic data")]
    public static string PropertyOrderTestValuesDisplayName(MethodInfo methodInfo, object[] data)
    {
        var obj = data[0];
        var props = (string[])data[1];
        var targets = (object[])data[2];
        string type = obj?.GetType().Name ?? "<null type>";
        return $"{type} - {string.Join(", ", props)} - {string.Join(", ", targets)}";
    }

    public static IEnumerable<object?[]> PropertyOrderTestValues
    {
        get
        {
            (object?, string[], object[])[] items =
            [
                (TestDate, ["year", "month", "Day", "Hour", "Minute"], [TestDate.Year, TestDate.Month, TestDate.Day, TestDate.Hour, TestDate.Minute]),
                (GetAsPsObject(TestDate), ["year", "month", "Day","Hour", "Minute"], [TestDate.Year, TestDate.Month, TestDate.Day, TestDate.Hour, TestDate.Minute]),
                ("Jimbo", ["length"], ["Jimbo".Length]),
                (new Point(21, 37), ["x", "y"], [21, 37]),
            ];
            return items.Select(item => new object?[] { item.Item1, item.Item2, item.Item3 });
        }
    }

    [TestInitialize]
    public void InitTestDatePSObject()
    {
        using PowerShell ps = PowerShell.Create();
        ps.AddScript("$args[0]").AddArgument(TestDate);
        var res = ps.Invoke();
        TestDatePSObject = res[0];

    }

    [TestMethod("Default Formatter == ToString()")]
    [DynamicData(nameof(DefaultFormatterTestValues), DynamicDataDisplayName = nameof(DefaultFormatterTestValuesDisplayName))]
    public void DefaultFormatter(object? obj)
    {
        var originalString = obj?.ToString();
        var originalItem = obj;
        var psoWrappedItem = GetAsPsObject(obj);
        var formatter = GetFormatter();
        string? originalResult = formatter.FormatItem(originalItem);
        string? psoResult = formatter.FormatItem(psoWrappedItem);
        Assert.AreEqual(originalString,
                        originalResult,
                        StringComparer.Ordinal,
                        message: "Formatted original value is different than ToString.");
        Assert.AreEqual(originalString,
                        psoResult,
                        StringComparer.Ordinal,
                        message: $"Formatted PSObject is different than ToString()");
    }

    [TestMethod("Properties are case insensitive")]
    [DynamicData(nameof(CaseSensitiveTestValues), DynamicDataDisplayName = nameof(CaseSensitiveTestValuesDisplayName))]
    public void CaseSensitive_Impl(object obj, string[] properties)
    {
        var formattedLower = GetFormatter(
                    properties: properties.Select(x => x.ToLowerInvariant()).ToArray()
                    ).FormatItem(obj);
        var formattedUpper = GetFormatter(
            properties: properties.Select(x => x.ToUpperInvariant()).ToArray()
            ).FormatItem(obj);
        Assert.AreEqual(formattedLower, formattedUpper, StringComparer.Ordinal);
    }

    [TestMethod("Properties values match actual values")]
    [DynamicData(nameof(PropertyOrderTestValues), DynamicDataDisplayName = nameof(PropertyOrderTestValuesDisplayName))]
    public void PropertiesMatchValueTest(object obj, string[] properties, object[] targetProperties)
    {
        var formatter = GetFormatter(properties: properties);

        var fseparator = formatter.PropertiesSeparator;
        var target = string.Join(fseparator, targetProperties);

        var originalResult = formatter.FormatItem(obj);
        var originalSplit = originalResult?.Split(fseparator);

        var psoResult = formatter.FormatItem(GetAsPsObject(obj));
        var psoSplit = psoResult?.Split(fseparator);

        Assert.AreEqual(targetProperties.Length, originalSplit?.Length ?? 0, message: "Formatted original value yield fewer elements than properties");
        Assert.AreEqual(targetProperties.Length, psoSplit?.Length ?? 0, message: "Formatted original value yield fewer elements than properties");

        Assert.AreEqual(target, originalResult,
                                    StringComparer.Ordinal,
                        message: $"Formatted original value is different than ToString()");
        Assert.AreEqual(target, psoResult,
                                    StringComparer.Ordinal,
                        message: $"Formatted PSObject is different than ToString()");
        // TODO Find a way to control order
    }

    [TestMethod("Null input always results in null output")]
    public void NullTests()
    {
        ItemFormatter[] formatters =
        [
            GetFormatter(),
            GetFormatter(ScriptBlock.Create("$args[0]")),
            GetFormatter(properties: ["test","*test*"])
        ];
        foreach (var formatter in formatters)
        {
            var result = formatter.FormatItem(null);
            Assert.IsNull(result, message: $"Formatter {formatter.GetFormatSourceType()} did not return null");
        }
    }

    [DataTestMethod()]
    [DataRow(data: null, DisplayName = "<null>")]
    [DataRow(data: " ")] // " "
    [DataRow(data: ", ")] // ", "
    [DataRow(data: ":")] // ":"
    [DataRow("\0", DisplayName = "<Null byte>")] //"\0"
    [DataRow("żźć")] // "żźć"
    [DataRow("獩")] // "獩"
    public void SeparatorsSameTest(string? separator)
    {
        string? separatorForRegex = separator ?? ItemFormatter.DefaultSeparator;
        string? separatorDisplay = separator switch
        {
            null => "<null>",
            _ => string.Join(", ", Encoding.UTF8.GetBytes(separator).Select(x => x.ToString("X2")))
        };

        string[] props = ["year", "month", "Day", "Hour", "Minute", "Second", "Millisecond", "MicroSecond"];
        var formatter = GetFormatter(separator: separator, properties: props);
        var result = formatter.FormatItem(TestDate);
        Assert.IsNotNull(result, "Formatter yielded null time from datetime");

        var occurences = Regex.Matches(result, Regex.Escape(separatorForRegex));
        int expected = props.Length - 1; // off by one
        Assert.AreEqual(expected, occurences.Count, $"Separator {separatorDisplay} did not occur correct number of times");
    }

    [DataTestMethod()]
    [DataRow("$_, $c, $p, $t", "$args[0], $args[1], $args[2], $args[3]")]
    [DataRow("$c, $p, $t, $_", "$args[1], $args[2], $args[3], $args[0]")]
    [DataRow("$_, $c, $p, $t", "$ARGS[0], $ARGS[1], $ARGS[2], $ARGS[3]")]
    [DataRow("$_, $C, $P, $T", "$args[0], $args[1], $args[2], $args[3]")]
    [DataRow("$_, $C, $P, $T", "$ARGS[0], $ARGS[1], $ARGS[2], $ARGS[3]")]
    [DataRow("$p, $t", "$args[2], $args[3]")]
    [DataRow("$_, $_, $_, $_, $_", "$args[0], $args[0], $args[0], $args[0], $args[0]")]
    [DataRow("$t, $t, $t, $t, $t", "$args[3], $args[3], $args[3], $args[3], $args[3]")]
    public void ArgumentAliasTest(string aliased, string originals)
    {
        if (aliased.Split(',').Length != originals.Split(',').Length)
        {
            throw new ArgumentException("Mismatched number of aliases and originals");
        }
        //using PowerShell pwsh = PowerShell.Create();
        //Runspace.DefaultRunspace = pwsh.Runspace; // RunspaceFactory.CreateRunspace(InitialSessionState.Create());

        var formatterOrig = GetFormatter(ScriptBlock.Create(originals));
        var formatterAlias = GetFormatter(ScriptBlock.Create(aliased));
        var resultOrig = formatterOrig.FormatItem("a", "b", "c", "d");
        var resultAlias = formatterAlias.FormatItem("a", "b", "c", "d");
        Assert.AreEqual(resultOrig, resultAlias, message: $"Aliases mismatch ({aliased})");
    }
    [DataTestMethod()]
    [DataRow("$_.Minute", "37")]
    [DataRow("$_.Hour, $_.Minute", "21 37")]
    [DataRow("\"$($_.Hour)$($_.Minute)\"", "2137")]
    [DataRow("$z = $_.AddMinutes(1);$z.Minute", "38")]
    [DataRow("$_.Minute.ToString('d5')", "00037")]
    public void ScriptPropertiesMethodsTest(string? script, string? target)
    {
        ScriptBlock? scriptBlock = script switch
        {
            null => null,
            _ => ScriptBlock.Create(script),
        };
        var formatter = GetFormatter(scriptBlock);
        var result = formatter.FormatItem(TestDate);
        Assert.AreEqual(target, result, StringComparer.Ordinal,message:script);
    }
}