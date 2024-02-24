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
    public static TimeSpan[] GetTimeSpanArray(int minLength, params long[] tickses)
    {
        var donorLength = tickses.Length;
        var repeats = (int)Math.Ceiling((double)minLength / donorLength);
        return Enumerable.Repeat(tickses, repeats)
            .SelectMany(x => x)
            .Select(TimeSpan.FromTicks).ToArray();
    }
    public static IEnumerable<object?[]> TestRunningAverageTestValues => [
        [GetTimeSpanArray(10,100,90,80,70,110),90,0],
        [GetTimeSpanArray(10,100,90,80,70,110),90,50],
        [GetTimeSpanArray(10,100,90,80,70,110),90,150],
        [GetTimeSpanArray(100,100,90,80,70,110),90,50],
        [GetTimeSpanArray(100,100,90,80,70,110),90,150],
        [GetTimeSpanArray(100,100,90,80,70,110),90,0],

        [GetTimeSpanArray(10,21,51,1214,8748,651), 2137, 0],
        [GetTimeSpanArray(10,21,51,1214,8748,651), 2137, 50],
        [GetTimeSpanArray(10, 21, 51, 1214, 8748, 651), 2137, 150],
        [GetTimeSpanArray(100,21,51,1214,8748,651), 2137, 50],
        [GetTimeSpanArray(100,21,51,1214,8748,651), 2137, 150],
        [GetTimeSpanArray(100, 21, 51, 1214, 8748, 651),2137,0],
        ];
    public static string TestRunningAverageTestValuesDisplayName(MethodInfo methodInfo, object[] data)
    {
        int arrLen = ((TimeSpan[])data[0]).Length;
        var target = data[1];
        var calcLen = data[2];
        return $"{arrLen} values with target {target} and calc length of {calcLen}";
    }
    [TestMethod("Running average calculated correctly")]
    [DynamicData(nameof(TestRunningAverageTestValues),DynamicDataDisplayName =nameof(TestRunningAverageTestValuesDisplayName))]
    public void TestRunningAverage(TimeSpan[] ts, long targetTicks, int calculationLength)
    {
        var buff = new TimeBuffer(calculationLength );
        DateTime date = DateTime.Now;
        buff.AddTime(date);
        foreach (var item in ts)
        {
            date += item;
            buff.AddTime(date);
        }
        Assert.AreEqual(TimeSpan.FromTicks(targetTicks), buff.CalculateMovingAverageTime());
    }
}
