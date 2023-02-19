using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections;

namespace WriteProgressPlus.Benchmark;

public class Program
{
    public static void Main()
    {
        var s = BenchmarkRunner.Run<Bench>();
    }
    

}

public class Bench
{
    public List<object> LISTOBJECT = new List<object>();
    public ArrayList ARRAYLIST = new ArrayList();

    //[IterationSetup]
    //public void Clear()
    //{
    //    LISTOBJECT.Clear();
    //    ARRAYLIST.Clear();
    //}

    [Benchmark]
    public void LIST()
    {
        var t = DateTime.Now;
        LISTOBJECT.Add(t);
        LISTOBJECT.Add(t.Nanosecond);
        LISTOBJECT.Add(t.ToString());
    }
    [Benchmark]
    public void ARRAY()
    {
        var t = DateTime.Now;
        ARRAYLIST.Add(t);
        ARRAYLIST.Add(t.Nanosecond);
        ARRAYLIST.Add(t.ToString());
    }
}