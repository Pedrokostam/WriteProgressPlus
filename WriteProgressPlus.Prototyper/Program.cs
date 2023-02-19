using System.Management.Automation;

namespace WriteProgressPlus.Prototyper;

internal class Program
{
    static void Main(string[] args)
    {
        var format = new ItemFormatter();
        Dictionary<string, object> bounds = new();
        ScriptBlock? sc = ScriptBlock.Create("");
        string[]? pp = new string[] { "Month","Year" };
        format.Update(null,pp,null);
        var t = format.FormatItem(DateTime.Now);
        Console.WriteLine("Hello, World!");
    }
}
