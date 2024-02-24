﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
    /// for performance and simplicity reasons it'dynamicStopwatch better to skip it, where applicable.
    /// </summary>
    /// <param name="runtime"></param>
    /// <returns>If host has built-in throttling - <see langword="true"/>. Otherwise - <see langword="false"/></returns>
    public static bool IsThrottlingBuiltIn(ICommandRuntime runtime)
    {
        var runtimeVersion = runtime.Host.Version;
        // The throttling applies only to ConsoleHost, as far as I am aware, so better make sure it matches.
        var runtimeName = runtime.Host.Name;
        return runtimeName is "ConsoleHost" && runtimeVersion >= ThrottlingVersion;
    }

    /// <summary>
    /// Minimal view for progress bar was introduced in PowerShell 7.2.0, along with the PSStyle automatic variable (and its class)
    /// Since this Powershell library does not have the definition of this class, we have to use dynamic objects.
    /// </summary>
    /// <param name="cmdlet"></param>
    /// <returns></returns>
    public static bool IsViewMinimal(PSCmdlet cmdlet)
    {
        var runtimeVersion = cmdlet.CommandRuntime.Host.Version;
        if (runtimeVersion < MinimalProgressVersion)
        {
            return false;
        }
#if DEBUG
        var dynamicStopwatch = Stopwatch.StartNew();
#endif
        dynamic psstyle = cmdlet.SessionState.PSVariable.GetValue("PSStyle", defaultValue: null);
        bool isMinimalView = psstyle?.Progress.View.ToString() == "Minimal";
#if DEBUG
        dynamicStopwatch.Stop();
        Debug.WriteLine(message: Invariant($"{(double)dynamicStopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond} ms"));
#endif
        return isMinimalView;
    }
}
