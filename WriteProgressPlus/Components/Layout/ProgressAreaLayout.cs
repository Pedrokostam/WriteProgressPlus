using System.Runtime.InteropServices;
using System.Management.Automation.Host;
using System.Management.Automation;
using System.Diagnostics;
using WriteProgressPlus.Settings;
using static System.FormattableString;
using System.Collections;

namespace WriteProgressPlus.Components.Layout;

/// <summary>
/// Simple struct to store an instance of <see cref="System.Management.Automation.Host.Size"/> and information about current view style.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct ProgressAreaLayout
{
    public readonly Size Size;
    public readonly bool IsMinimal;
    public int Width => Size.Width;
    public int Height => Size.Height;

    public ProgressAreaLayout(int width, int height, bool isMinimal) : this(new Size(width, height), isMinimal)
    { }
    public ProgressAreaLayout(Size size, bool isMinimal)
    {
        Size = size;
        IsMinimal = isMinimal;
    }
    public static ProgressAreaLayout GetProgressLayout(PSCmdlet cmdlet)
    {
        // Minimal view for progress bar was introduced in PowerShell 7.2.0, along with the PSStyle automatic variable (and its class)
        // Since this Powershell library does not have the definition of this class, we have to use dynamic objects.
#if DEBUG
        var dynamicStopwatch = Stopwatch.StartNew();
#endif
        PowerShellFeatures.Initialize(cmdlet);
        Size buffer = cmdlet.CommandRuntime.Host.UI.RawUI.BufferSize;
        var lineWidth = buffer.Width;
        bool isMinimalView = false;
        if (PowerShellFeatures.HasPSStyle)
        {
            dynamic psstyle = cmdlet.SessionState.PSVariable.GetValue("PSStyle", defaultValue: null);
            dynamic? progress = psstyle?.Progress;
            isMinimalView = progress?.View.ToString() == "Minimal";
            if (isMinimalView)
            {
                int styleMaxWidth = progress?.MaxWidth;
                lineWidth = Math.Min(lineWidth, styleMaxWidth);
            }
            buffer = new Size(lineWidth, buffer.Height);
        }
#if DEBUG
        dynamicStopwatch.Stop();
        Debug.WriteLine(message: Invariant($"GetProgressLayout: {(double)dynamicStopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond} ms"));
#endif
        return new ProgressAreaLayout(buffer, isMinimalView);
    }
}