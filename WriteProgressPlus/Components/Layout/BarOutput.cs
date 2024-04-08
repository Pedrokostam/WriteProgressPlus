namespace WriteProgressPlus.Components.Layout;

internal readonly record struct BarOutput
{
    public readonly string Status = string.Empty;
    public readonly string Activity = string.Empty;
    public readonly int RemainingTime = -1;
    public readonly int PercentComplete = -1;
    public readonly string CurrentOperation = string.Empty;

    /// <summary>
    /// Creates new BarOutput. Ensures proper values of <paramref name="status"/> and <paramref name="activity"/>.
    /// </summary>
    /// <param name="activity">Text used for activity, empty string will be replaced with default value.</param>
    /// <param name="status">Text used for status, empty string will be replaced with a space.</param>
    /// <param name="remainingTime"></param>
    /// <param name="percentComplete"></param>
    /// <param name="currentOperation"></param>
    public BarOutput(string activity, string status, int remainingTime, int percentComplete, string currentOperation)
    {
        Activity = activity.Length > 0 ? activity : "Processing...";
        Status = status.Length > 0 ? status : " ";
        RemainingTime = remainingTime;
        PercentComplete = percentComplete;
        CurrentOperation = currentOperation;
    }
}
