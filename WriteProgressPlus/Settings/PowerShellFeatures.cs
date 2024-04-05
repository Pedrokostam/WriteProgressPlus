using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Net.Http.Headers;
using System.Text;
using WriteProgressPlus.Extensions;

namespace WriteProgressPlus.Settings;
public static class PowerShellFeatures
{
    private static readonly Version MinimalProgressVersion = new(major: 7, minor: 2, build: 0);
    private static readonly Version MinimalThrottlingVersion = new(major: 6, minor: 0, build: 0);

    /// <summary>
    /// Whether the current Powershell host throttles call to Write-Progress. If not initialized returns false.
    /// </summary>
    public static bool HasThrottlingBuiltIn { get; private set; }
    /// <summary>
    /// Whether the current Powershell supports multiple style for progress bar.
    /// </summary>
    public static bool HasPSStyle { get; private set; }
    public static bool Initialized { get; private set; }
    public static void Initialize(Version powershellVersion)
    {
        if (Initialized) { return; }
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
    public static void Initialize(PSCmdlet cmdlet)
    {
        if (Initialized) { return; }
        // If for some reason, the class was not initialized during module import, ther first call to Write-Progress will initialize it
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
}
