using System.Runtime.InteropServices;

namespace WriteProgressPlus.Components;

/// <summary>
/// Simple width/height struct to avoid having to instantiate <see cref="System.Management.Automation.Host.Size"/> while being equivalent.
/// </summary>
[StructLayout(LayoutKind.Auto)]
internal readonly record struct Size
{
    public readonly int Width;
    public readonly int Height;

    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public static implicit operator Size(System.Management.Automation.Host.Size size)
    {
        return new Size(size.Width, size.Height);
    }
}