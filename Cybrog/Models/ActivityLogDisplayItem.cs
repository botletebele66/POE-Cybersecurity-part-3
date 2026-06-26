namespace Cybrog.Models
{
    /// <summary>
    /// Display-only wrapper around an <see cref="Engine.LogEntry"/> for the Activity
    /// Log sidebar binding. Kept separate from the engine's LogEntry so the engine
    /// layer has no WPF/display-formatting concerns mixed into it (separation of
    /// concerns between business logic and presentation).
    /// </summary>
    public class ActivityLogDisplayItem
    {
        public string Time { get; init; } = string.Empty;
        public string Action { get; init; } = string.Empty;
    }
}
