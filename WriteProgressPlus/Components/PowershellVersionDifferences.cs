using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Text;

namespace WriteProgressPlus.Components;
internal static class PowershellVersionDifferences
{
    /// <summary>
    /// Since Powershell 6 ConsoleHost will automatically throttle updates to the progress bar.
    /// <para/>
    /// As per <see href="https://github.com/PowerShell/PowerShell/pull/2822">PR #2822</see>
    /// the host will ignore updates until 200ms have passed since last update
    /// <para/>
    /// The PR was part of the <see href="https://github.com/PowerShell/PowerShell/releases/tag/v6.0.0-alpha.18">v6.0.0-alpha.18</see> release,
    /// so we can test if the host has throttling built-in by checking if the major version is greater than 6
    /// The earlies available version on Nuget is 6.0.4.
    /// <para/>
    /// While it won't really be a problem, if the cmdlet does an additional throttling,
    /// for performance and simplicity reasons it's better to skip it, where applicable.
    /// </summary>
    /// <param name="state">SessionState used to get variable for PSVersionTable.</param>
    /// <returns>If host has built-in throttling - <see langword="true"/>. Otherwise - <see langword="false"/></returns>
    public static bool IsThrottlingBuiltIn(SessionState state)
    {
#if POWERSHELL_BUILTIN_THROTTLING
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    /// Minimal view for progress bar was introduced in PowerShell 7.2.0, along with the PSStyle automatic variable (and its class)
    /// </summary>
    /// <param name="cmdlet"></param>
    /// <returns>Tuple of 2 values: whole line width and whether its minimal view</returns>
    public static (Size, bool isMinimal) GetProgressViewTypeAndWidth(PSCmdlet cmdlet)
    {
        var buffer = cmdlet.CommandRuntime.Host.UI.RawUI.BufferSize;
        var lineWidth = buffer.Width;
        bool isMinimalView = false;
#if POWERSHELL_HAS_MINIMAL_STYLE
        PSStyle? psstyle = cmdlet.SessionState.PSVariable.GetValue("PSStyle", defaultValue: null) as PSStyle;
        isMinimalView = psstyle?.Progress?.View == ProgressView.Minimal;
        if (isMinimalView)
        {
            // psstyle is guaranteed not null here, since its view is minimal.
            lineWidth = Math.Min(lineWidth, psstyle!.Progress.MaxWidth);
        }
#endif
        var bufferSize = new Size(lineWidth, buffer.Height);
        return (bufferSize, isMinimalView);
    }
}
