namespace WriteProgressPlus.Components;

public class PowershellCapabilities
{
    private static readonly Lazy<PowershellCapabilities> instance = new Lazy<PowershellCapabilities>(() => new PowershellCapabilities());
    public static PowershellCapabilities Instance => instance.Value;
    public bool CanThrottleProgressUpdates { get; private set; }
    public bool HasMinimalStyle { get; private set; }
}
