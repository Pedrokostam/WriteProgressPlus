using System.Runtime.InteropServices;
using System.Management.Automation.Host;
namespace WriteProgressPlus.Components;

/// <summary>
/// Simple struct to store an instance of <see cref="System.Management.Automation.Host.Size"/> and information about current view style.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct ProgressLayout
{
    public readonly Size Size;
    public readonly bool IsMinimal;
    public int Width => Size.Width;
    public int Height => Size.Height;

    public ProgressLayout(int width, int height, bool isMinimal) : this(new Size(width, height), isMinimal)
    { }
    public ProgressLayout(Size size, bool isMinimal)
    {
        Size = size;
        IsMinimal = isMinimal;
    }
}