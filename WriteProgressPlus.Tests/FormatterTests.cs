namespace WriteProgressPlus.Tests;

using System.Drawing;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using WriteProgressPlus;
using WriteProgressPlus.Components;

[TestClass]
public class FormatterTests
{
    public ItemFormatter GetFormatter(ScriptBlock? script = null, string[]? properties = null, string? separator = null)
    {
        var formatter = new ItemFormatter();
        formatter.Update(script, properties, separator);
        return formatter;
    }
    public static readonly DateTime TestDate = new DateTime(year: 2005,
                                                     month: 4,
                                                     day: 2,
                                                     hour: 21,
                                                     minute: 37,
                                                     second: 5,
                                                     millisecond: 69,
                                                     microsecond: 966);
    public static PSObject GetAsPsObject(object? obj)
    {
        using PowerShell ps = PowerShell.Create();
        ps.AddScript("$args[0]").AddArgument(obj);
        var res = ps.Invoke();
        return res[0];

    }
    public PSObject TestDatePSObject = default!;
    public static readonly string[] Wildcard_PropertyPattern = ["*second"];
    public static readonly object[] Wildcard_PropertyTargets = [5, 69, 966, 0];

    public static readonly string[] Standard_PropertyPattern = ["hour", "minute"];
    public static readonly object[] Standard_PropertyTargets = [21, 37];

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
                (null, ["Keksam"]),
                (null, ["Keksam*"]),
            ];
            return items.Select(item => new object?[] { item.Item1, item.Item2 });
        }
    }
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
            (object?, string[], object[], string?, string?)[] items =
            [
                (TestDate, ["year", "month", "Day", "Hour", "Minute"], [TestDate.Year, TestDate.Month, TestDate.Day, TestDate.Hour, TestDate.Minute], null,null),
                (TestDate, ["*second"], [TestDate.Second, TestDate.Millisecond, TestDate.Microsecond, TestDate.Nanosecond], null,null),
                (GetAsPsObject(TestDate), ["year", "month", "Day","Hour", "Minute"], [TestDate.Year, TestDate.Month, TestDate.Day, TestDate.Hour, TestDate.Minute], null,null),
                (GetAsPsObject(TestDate), ["*second"], [TestDate.Second, TestDate.Millisecond, TestDate.Microsecond, TestDate.Nanosecond], null,null),
                ("Jimbo", ["length"], ["Jimbo".Length], null,null),
                ("Jimbo", ["*gth"], ["Jimbo".Length], null,null),
                (new Point(21, 37), ["x", "y"], [21, 37], null,null),
                (new Point(21, 37), ["[xy]"], [21, 37], null,null),
                (new Point(21, 37), ["?"], [21, 37], null,null),
                (null, [], [],"null",null),

                (TestDate, ["year", "month", "Day", "Hour", "Minute"], [TestDate.Year, TestDate.Month, TestDate.Day, TestDate.Hour, TestDate.Minute], null,"||"),
                (TestDate, ["*second"], [TestDate.Second, TestDate.Millisecond, TestDate.Microsecond, TestDate.Nanosecond], null,"||"),
                (GetAsPsObject(TestDate), ["year", "month", "Day","Hour", "Minute"], [TestDate.Year, TestDate.Month, TestDate.Day, TestDate.Hour, TestDate.Minute], null,"||"),
                (GetAsPsObject(TestDate), ["*second"], [TestDate.Second, TestDate.Millisecond, TestDate.Microsecond, TestDate.Nanosecond], null,"||"),
                ("Jimbo", ["length"], ["Jimbo".Length], null,"||"),
                ("Jimbo", ["*gth"], ["Jimbo".Length], null,"||"),
                (new Point(21, 37), ["x", "y"], [21, 37], null,"||"),
                (new Point(21, 37), ["[xy]"], [21, 37], null,"||"),
                (new Point(21, 37), ["?"], [21, 37], null,"||"),
                (null, [], [],"null","||"),
            ];
            return items.Select(item => new object?[] { item.Item1, item.Item2, item.Item3, item.Item4, item.Item5 });
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
        Assert.AreEqual(originalResult,
                        originalString,
                        StringComparer.Ordinal,
                        message: "Formatted original value is different than ToString.");
        Assert.AreEqual(psoResult,
                        originalString,
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

    [TestMethod]
    [DynamicData(nameof(PropertyOrderTestValues), DynamicDataDisplayName = nameof(PropertyOrderTestValuesDisplayName))]
    public void Value(object obj, string[] properties, object[] targetProperties, string? overrideValue, string? separator)
    {
        var formatter = GetFormatter(properties: properties, separator: separator);

        var fseparator = formatter.PropertiesSeparator;
        var target = overrideValue ?? string.Join(fseparator, targetProperties);

        var originalResult = formatter.FormatItem(obj);
        var originalSplit = originalResult?.Split(fseparator);

        var psoResult = formatter.FormatItem(GetAsPsObject(obj));
        var psoSplit = psoResult?.Split(fseparator);

        bool hasWildcard = targetProperties.Any(x =>
        {
            if (x is not string s)
            {
                return false;
            }
            else
            {
                return s.Contains('*') || s.Contains('[') || s.Contains("?");
            }
        });

        if (!hasWildcard)
        {
            Assert.AreEqual(originalSplit?.Length ?? 0, targetProperties.Length, message: "Formatted original value yield fewer elements than properties");
            Assert.AreEqual(psoSplit?.Length ?? 0, targetProperties.Length, message: "Formatted original value yield fewer elements than properties");
        }
        string[] stringProperties = targetProperties.Select(x=> x?.ToString() ?? "").ToArray();
        bool originalSequenceEquals = stringProperties.SequenceEqual(originalSplit ?? []);
        bool psoSequenceEquals = stringProperties.SequenceEqual(psoSplit ?? []);

        Assert.IsTrue(originalSequenceEquals, message: "Formatted original value yielded different values than target values (order not checked)");
        Assert.IsTrue(psoSequenceEquals, message: "Formatted PSObject yielded different values than target values (order not checked)");

        Assert.AreEqual(originalResult,
                        target,
                        StringComparer.Ordinal,
                        message: $"Formatted original value is different than ToString()");
        Assert.AreEqual(psoResult,
                        target,
                        StringComparer.Ordinal,
                        message: $"Formatted PSObject is different than ToString()");
        // TODO Find a way to control order
    }


}