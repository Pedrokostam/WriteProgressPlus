using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Text;
using static System.FormattableString;

namespace WriteProgressPlus.Components;
internal static class PowershellVersionDifferences
{
    private static readonly Version ThrottlingVersion = new Version(6, 0, 0);
    private static readonly Version MinimalProgressVersion = new Version(7, 2, 0);
    /// <summary>
    /// Since Powershell 6 ConsoleHost will automatically throttle updates to the progress bar.
    /// <para/>
    /// As per <see href="https://github.com/PowerShell/PowerShell/pull/2822">PR #2822</see>
    /// the host will ignore updates until 200ms have passed since last update
    /// <para/>
    /// The PR was part of the <see href="https://github.com/PowerShell/PowerShell/releases/tag/v6.0.0-alpha.18">v6.0.0-alpha.18</see> release,
    /// so we can test if the host has throttling built-in by checking if the major version is greater than 6
    /// (I assume, that no-one would be using pre-alpha18 Powershell 6 anymore).
    /// <para/>
    /// While it won't really be a problem, if the cmdlet does an additional throttling,
    /// for performance and simplicity reasons it's better to skip it, where applicable.
    /// </summary>
    /// <param name="runtime"></param>
    /// <returns>If host has built-in throttling - <see langword="true"/>. Otherwise - <see langword="false"/></returns>
    public static bool IsThrottlingBuiltIn(SessionRuntime sessionRuntime)
    {
        var versionTable = sessionRuntime.GetVariable<Hashtable>("PSVersionTable")!;
        // Only checking if the version of PowerShell is after throttling was introduced.
        // theoretically throttling is tied to ConsoleHost, and it is possible to implement it on its own (citation needed)
        // but for now I am going to assume that checking the version is all that is needed
        return (Version)versionTable["PSVersion"] >= ThrottlingVersion;
    }

    /// <param name="state">SessionState used to get variable for PSVersionTable - will be wrapped to SessionRuntime with null CommandRuntime.</param>
    /// <inheritdoc cref="IsThrottlingBuiltIn(SessionRuntime)"/>
    public static bool IsThrottlingBuiltIn(SessionState state) => IsThrottlingBuiltIn(new SessionRuntime(state, null!));

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
#endif
        var buffer = cmdlet.CommandRuntime.Host.UI.RawUI.BufferSize;
        var lineWidth = buffer.Width;
        var runtimeVersion = cmdlet.CommandRuntime.Host.Version;
        if (runtimeVersion < MinimalProgressVersion)
        {
            return (buffer, false);
        }
        dynamic psstyle = cmdlet.SessionState.PSVariable.GetValue("PSStyle", defaultValue: null);
        dynamic? progress = psstyle?.Progress;
        bool isMinimalView = progress?.View.ToString() == "Minimal";
        if (isMinimalView)
        {
            int styleMaxWidth = progress?.MaxWidth;
            lineWidth = Math.Min(lineWidth, styleMaxWidth);
        }
#if DEBUG
        dynamicStopwatch.Stop();
        Debug.WriteLine(message: Invariant($"{(double)dynamicStopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond} ms"));
#endif
        return (new Size(lineWidth, buffer.Height), isMinimalView);
    }
}
