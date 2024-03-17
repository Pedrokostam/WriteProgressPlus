using JetBrains.Annotations;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using WriteProgressPlus.Components;

namespace WriteProgressPlus.Tests;

[TestClass]
public class TimeBufferTests
{
    //    / unit test part
    //Mock<Program> p = new Mock<Program>();
    //    p.Setup(x => x.GetLastName()).Returns("qqq");

    //public static TimeEntry[] GetTimeEntryArray(int Length, DateTime start, DateTime end)
    //{

    //    var donorLength = millis.Length;
    //    var repeats = (int)Math.Ceiling((double)minLength / donorLength);
    //    return Enumerable.Repeat(millis, repeats)
    //        .SelectMany(x => x)
    //        .Select(TimeSpan.FromMilliseconds).ToArray();
    //}

    public static TimeSpan[] GetTimeSpanArray(int minLength, params double[] millis)
    {
        var donorLength = millis.Length;
        var repeats = (int)Math.Ceiling((double)minLength / donorLength);
        return Enumerable.Repeat(millis, repeats)
            .SelectMany(x => x)
            .Select(TimeSpan.FromMilliseconds).ToArray();
    }
    const double Negative = -1 * 1000.0;
    //public static IEnumerable<object?[]> TestRunningAverageTestValues => [
    //    [GetTimeSpanArray(0,100), Negative, 0],
    //    [GetTimeSpanArray(10, 110,100, 90,80,70),90,0],
    //    [GetTimeSpanArray(10,110,100,90,80,70),90,50],
    //    [GetTimeSpanArray(10,110,100,90,80,70),90,150],
    //    [GetTimeSpanArray(100, 110,100, 90,80,70),90,50],
    //    [GetTimeSpanArray(100,110,100,90,80,70),90,150],
    //    [GetTimeSpanArray(100, 110,100, 90,80,70), 90, 0],

    //    [GetTimeSpanArray(10,21,51,1214,8748,651), 2137, 0],
    //    [GetTimeSpanArray(10,21,51,1214,8748,651), 2137, 50],
    //    [GetTimeSpanArray(10, 21, 51, 1214, 8748, 651), 2137, 150],
    //    [GetTimeSpanArray(100,21,51,1214,8748,651), 2137, 50],
    //    [GetTimeSpanArray(100,21,51,1214,8748,651), 2137, 150],
    //    [GetTimeSpanArray(100, 21, 51, 1214, 8748, 651), 2137, 0],
    //    [GetTimeSpanArray(10, 10,10000), 5005, 0],
    //    [GetTimeSpanArray(100, 10,10000), 5005, 0],
    //    ];
    //public static string TestRunningAverageTestValuesDisplayName(MethodInfo methodInfo, object[] data)
    //{
    //    int arrLen = ((TimeSpan[])data[0]).Length;
    //    var target = data[1];
    //    var calcLen = data[2];
    //    return $"{arrLen} values with target {target} and calc length of {calcLen}";
    //}
    //[TestMethod("Running average calculated correctly")]
    //[DynamicData(nameof(TestRunningAverageTestValues), DynamicDataDisplayName = nameof(TestRunningAverageTestValuesDisplayName))]
    //public void TestRunningAverage(TimeSpan[] ts, double targetMillis, int calculationLength)
    //{
    //    var buff = new TimeBuffer(calculationLength);
    //    DateTime origDate = DateTime.Now;
    //    DateTime date = origDate;
    //    int index = 1;
    //    buff.AddTime(new TimeEntry(date, index++));
    //    foreach (var item in ts)
    //    {
    //        date += item;
    //        buff.AddTime(new TimeEntry(date, index++));
    //    }
    //    var aver = buff.CalculateMovingAverageTime();
    //    Assert.AreEqual(TimeSpan.FromMilliseconds(targetMillis), buff.CalculateMovingAverageTime());
    //}
    [DataTestMethod()]
    [DataRow(0)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(53)]
    public void TestReplacement(int length)
    {
        var buff = new TimeBuffer(length);
        length = buff.MaxLength;
        int i;
        for (i = 0; i < length; i++)
        {
            buff.AddTime(new TimeEntry(DateTime.MaxValue, i));
        }
        var d = DateTime.Now;
        var d2 = d + TimeSpan.FromMinutes(1)*length;
        var e1 = new TimeEntry(d, length + 1);
        var e2 = new TimeEntry(d2, e1.Iteration + length-1);
        buff.AddTime(e1);
        for (i = e1.Iteration+1; i < e2.Iteration; i++)
        {
            buff.AddTime(new TimeEntry(DateTime.MaxValue, i));
        }
        buff.AddTime(e2);
        var aver = buff.CalculateMovingAverageTime();
        var span = (d2 - d) / (length-1);
        Assert.AreEqual(span.Ticks, buff.CalculateMovingAverageTime().Ticks,10);

    }
    [DataTestMethod()]
    [DataRow(2137, 1)]
    [DataRow(10000, 10)]
    [DataRow(10000, 100)]
    [DataRow(10000, 1000)]
    [DataRow(10000, 10000)]
    public void TestRunningAverage(double millis, int increment)
    {
        var time = TimeSpan.FromMilliseconds(millis);
        var start = DateTime.Now;
        var end = start + time;
        var avg = time / increment;
        var buff = new TimeBuffer(0);
        buff.AddTime(new TimeEntry(start, 0));
        buff.AddTime(new TimeEntry(end, increment));
        var aver = buff.CalculateMovingAverageTime();
        Assert.AreEqual(avg, buff.CalculateMovingAverageTime());
    }
    [DataTestMethod]
    [DataRow(10, 10)]
    [DataRow(100, 10)]
    [DataRow(10, 100)]
    [DataRow(500, 100)]
    public void TestValueReplacing(int inputLength, int calculationLength)
    {
        var buff = new TimeBuffer(calculationLength);
        calculationLength = buff.MaxLength;
        DateTime date = DateTime.Now;
        var range = Enumerable.Range(1, inputLength).ToArray();
        foreach (var item in range)
        {
            date += TimeSpan.FromTicks(item);
            buff.AddTime(new TimeEntry(date, item));
        }
        var cutOff = range
            .TakeLast(calculationLength)
            .ToArray();
        bool seqEq = cutOff.SequenceEqual(buff.TimeEntries.Select(x => x.Iteration));
        Assert.IsTrue(seqEq, "Timespans in buffer do not match expected values");
    }
}
