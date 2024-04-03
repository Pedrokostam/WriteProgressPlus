using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Net.Http.Headers;
using System.Text;
using static System.FormattableString;

namespace WriteProgressPlus.Components;
internal static class PowershellVersionDifferences
{
    private static readonly Version MinimalProgressVersion = new Version(7, 2, 0);
    private static readonly Version MinimalThrottlingVersion = new Version(6, 0, 0);

    public static bool HasThrottlingBuiltIn { get; private set; }
    public static bool HasPSStyle { get; private set; }
    public static bool Initialized { get; private set; }
    static public void Initialize(Version powershellVersion)
    {
        // Since Powershell 6 ConsoleHost will automatically throttle updates to the progress bar.
        // As per <see href="https://github.com/PowerShell/PowerShell/pull/2822">PR #2822</see>
        // the host will ignore updates until 200ms have passed since last update.
        // The PR was part of the <see href="https://github.com/PowerShell/PowerShell/releases/tag/v6.0.0-alpha.18">v6.0.0-alpha.18</see> release,
        // so we can test if the host has throttling built-in by checking if the major version is greater than 6.
        // While it won't really be a problem, if the cmdlet does an additional throttling,
        // for performance and simplicity reasons it's better to skip it, where applicable.
        HasThrottlingBuiltIn = powershellVersion >= MinimalThrottlingVersion;
        // Only checking if the version of PowerShell is after throttling was introduced.
        // theoretically throttling is tied to ConsoleHost, and it is possible to implement it on its own (citation needed)
        // but for now I am going to assume that checking the version is all that is needed
        HasPSStyle = powershellVersion >= MinimalThrottlingVersion;
        Initialized = true;
    }


    /// <summary>
    /// Minimal view for progress bar was introduced in PowerShell 7.2.0, along with the PSStyle automatic variable (and its class)
    /// Since this Powershell library does not have the definition of this class, we have to use dynamic objects.
    /// </summary>
    /// <param name="cmdlet"></param>
    /// <returns>Tuple of 2 values: whole line width and whether its minimal view</returns>
    public static (Size, bool isMinimal) GetProgressViewTypeAndWidth(PSCmdlet cmdlet)
    {
#if DEBUG
        var dynamicStopwatch = Stopwatch.StartNew();
        if (!Initialized)
        {
            var table = cmdlet.SessionState.GetVariable<Hashtable>("PSVersionTable")!;
            dynamic versionRaw = table["PSVersion"]!;
            if (versionRaw is Version version)
            {
                Initialize(version);
            }
            else
            {
                Initialize(new Version(versionRaw.Major, versionRaw.Minor));
            }
        }
#endif
        Size buffer = (Size)cmdlet.CommandRuntime.Host.UI.RawUI.BufferSize;
        var lineWidth = buffer.Width;
        bool isMinimalView = false;
        if (HasPSStyle)
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
        Debug.WriteLine(message: Invariant($"{(double)dynamicStopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond} ms"));
#endif
        return (buffer, isMinimalView);
    }
}
