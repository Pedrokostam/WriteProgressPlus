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
using WriteProgressPlus.Components.Time;

namespace WriteProgressPlus.Tests;

[TestClass]
public class TimeBufferTests
{
    public static TimeSpan[] GetTimeSpanArray(int minLength, params double[] millis)
    {
        var donorLength = millis.Length;
        var repeats = (int)Math.Ceiling((double)minLength / donorLength);
        return Enumerable.Repeat(millis, repeats)
            .SelectMany(x => x)
            .Select(TimeSpan.FromMilliseconds).ToArray();
    }

    [DataTestMethod()]
    [DataRow(0)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(53)]
    public void TestReplacement(int length)
    {
        var buff = new TimeBuffer(length);
        length = buff.BufferLength;
        int i;
        for (i = 0; i < length; i++)
        {
            buff.AddTime(new TimeEntry(DateTime.MaxValue, i));
        }
        var d = DateTime.Now;
        var d2 = d + TimeSpan.FromMinutes(1) * length;
        var e1 = new TimeEntry(d, length + 1);
        var e2 = new TimeEntry(d2, e1.Iteration + length - 1);
        buff.AddTime(e1);
        for (i = e1.Iteration + 1; i < e2.Iteration; i++)
        {
            buff.AddTime(DateTime.MaxValue, i);
        }
        buff.AddTime(e2);
        var average = buff.CalculateMovingAverageTime();
        var span = (d2 - d) / (length - 1);
        Assert.AreEqual(span.Ticks, average.Ticks, 10);

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
        buff.AddTime(start, 0);
        buff.AddTime(end, increment);
        var average = buff.CalculateMovingAverageTime();
        Assert.AreEqual(avg, average);
    }

    [DataTestMethod]
    [DataRow(10, 10)]
    [DataRow(100, 10)]
    [DataRow(10, 100)]
    [DataRow(500, 100)]
    public void TestValueReplacing(int inputLength, int calculationLength)
    {
        var buff = new TimeBuffer(calculationLength);
        calculationLength = buff.BufferLength;
        DateTime date = DateTime.Now;
        var range = Enumerable.Range(1, inputLength).ToArray();
        foreach (var item in range)
        {
            date += TimeSpan.FromTicks(item);
            buff.AddTime(date, item);
        }
        var cutOff = range
            .TakeLast(calculationLength)
            .ToArray();
        bool seqEq = cutOff.SequenceEqual(buff.TimeEntries.Select(x => x.Iteration));
        Assert.IsTrue(seqEq, "Timespans in buffer do not match expected values");
    }
}
