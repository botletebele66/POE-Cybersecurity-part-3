using System;
using System.Text.RegularExpressions;

namespace Cybrog.Engine
{
    /// <summary>Recognised user intents the conversation engine can act on.</summary>
    public enum Intent
    {
        Unknown,
        Greeting,
        Farewell,
        AskHelp,
        AskName,
        AddTask,
        ViewTasks,
        DeleteTask,
        CompleteTask,
        SetReminder,
        StartQuiz,
        ShowActivityLog
    }

    /// <summary>
    /// Simulates Natural Language Processing using regular-expression pattern
    /// matching over user input (the assignment's permitted approach of using
    /// string.Contains / regex rather than a full ML-based NLP library). Detects the
    /// user's intent even when the request is phrased many different ways, e.g.
    /// "remind me to update my password", "can you add a task to enable 2FA", and
    /// "I need to remember to check my privacy settings" all resolve to AddTask.
    /// Satisfies Task 3 (NLP-style command recognition).
    /// </summary>
    public class NlpIntentParser
    {
        private static readonly Regex DeleteTaskRegex = new(
            @"(?i)(delete|remove|erase|cancel)\s+(task|reminder)\s*#?\s*(\d+)",
            RegexOptions.Compiled);

        private static readonly Regex CompleteTaskRegex = new(
            @"(?i)(mark|complete|finish|tick\s+off)\s+(task|reminder)?\s*#?\s*(\d+)(\s+(complete|done|finished))?|" +
            @"task\s*#?\s*(\d+)\s+(complete|done|finished)",
            RegexOptions.Compiled);

        private static readonly Regex SetReminderRegex = new(
            @"(?i)remind\s+me\s+(in\s+(\d+)\s+(day|days|week|weeks)|to\s+.+)|" +
            @"set\s+a?\s*reminder\s+(for\s+)?(\d+)\s+(day|days|week|weeks)",
            RegexOptions.Compiled);

        private static readonly Regex AddTaskRegex = new(
            @"(?i)(add|create|new|set\s*up)\s+(a\s+)?(task|to\s*-?\s*do)|" +
            @"i\s+need\s+to\s+(remember|do)|can\s+you\s+(add|create)",
            RegexOptions.Compiled);

        private static readonly Regex ViewTasksRegex = new(
            @"(?i)(view|show|list|display|what\s+are|see|check)\s+(my\s+)?(tasks|task\s+list|to\s*-?\s*dos?|reminders?|pending)",
            RegexOptions.Compiled);

        private static readonly Regex StartQuizRegex = new(
            @"(?i)(start|take|play|begin|launch|run)\s+(a\s+|the\s+)?(quiz|test|game|mini-?game)|" +
            @"quiz\s+me|test\s+my\s+knowledge",
            RegexOptions.Compiled);

        private static readonly Regex ActivityLogRegex = new(
            @"(?i)(show|view|display|list|see|check)\s+(the\s+)?(activity\s+log|history|recent\s+actions?|log)|" +
            @"what\s+have\s+you\s+done(\s+for\s+me)?|what\s+happened",
            RegexOptions.Compiled);

        private static readonly Regex GreetingRegex = new(
            @"(?i)^\s*(hi|hello|hey|howdy|good\s+(morning|afternoon|evening)|sup|yo|greetings)\b",
            RegexOptions.Compiled);

        private static readonly Regex FarewellRegex = new(
            @"(?i)^\s*(bye|goodbye|cya|see\s+you|later|farewell|take\s+care|exit|quit)\b",
            RegexOptions.Compiled);

        private static readonly Regex HelpRegex = new(
            @"(?i)\b(help|what\s+can\s+you\s+do|commands|options|features)\b",
            RegexOptions.Compiled);

        private static readonly Regex AskNameRegex = new(
            @"(?i)(what('?s|\s+is)\s+my\s+name|do\s+you\s+know\s+my\s+name|who\s+am\s+i)",
            RegexOptions.Compiled);

        /// <summary>
        /// Parses raw user input into a recognised <see cref="Intent"/> plus any
        /// extracted values (task ID, free-text task title, reminder day count).
        /// </summary>
        public (Intent Intent, int? TaskId, string? TaskText, int? ReminderDays) Parse(string input)
        {
            input = input.Trim();

            var del = DeleteTaskRegex.Match(input);
            if (del.Success)
                return (Intent.DeleteTask, int.Parse(del.Groups[3].Value), null, null);

            var comp = CompleteTaskRegex.Match(input);
            if (comp.Success)
            {
                string idGroup = comp.Groups[3].Success ? comp.Groups[3].Value : comp.Groups[6].Value;
                return (Intent.CompleteTask, int.Parse(idGroup), null, null);
            }

            var rem = SetReminderRegex.Match(input);
            if (rem.Success)
            {
                int days = 7;
                if (rem.Groups[2].Success)
                {
                    days = int.Parse(rem.Groups[2].Value);
                    if (rem.Groups[3].Value.StartsWith("week", StringComparison.OrdinalIgnoreCase)) days *= 7;
                }
                else if (rem.Groups[5].Success)
                {
                    days = int.Parse(rem.Groups[5].Value);
                    if (rem.Groups[6].Value.StartsWith("week", StringComparison.OrdinalIgnoreCase)) days *= 7;
                }
                return (Intent.SetReminder, null, ExtractTaskTitle(input), days);
            }

            if (AddTaskRegex.IsMatch(input))
                return (Intent.AddTask, null, ExtractTaskTitle(input), null);

            if (ViewTasksRegex.IsMatch(input))
                return (Intent.ViewTasks, null, null, null);

            if (StartQuizRegex.IsMatch(input))
                return (Intent.StartQuiz, null, null, null);

            if (ActivityLogRegex.IsMatch(input))
                return (Intent.ShowActivityLog, null, null, null);

            if (GreetingRegex.IsMatch(input))
                return (Intent.Greeting, null, null, null);

            if (FarewellRegex.IsMatch(input))
                return (Intent.Farewell, null, null, null);

            if (AskNameRegex.IsMatch(input))
                return (Intent.AskName, null, null, null);

            if (HelpRegex.IsMatch(input))
                return (Intent.AskHelp, null, null, null);

            return (Intent.Unknown, null, null, null);
        }

        /// <summary>Strips leading directive phrases to isolate the task description, e.g. "remind me to update my password" → "update my password". Falls back to a generic title if no description was actually provided (e.g. "remind me in 3 days" with nothing after it).</summary>
        private static string ExtractTaskTitle(string input)
        {
            // Branch order matters: more specific "...task to" phrasings are tried before
            // the bare "can you add/create" fallback, so "can you add a task to review X"
            // strips the whole directive rather than stopping early at "can you add".
            string stripPattern =
                @"(?i)^.*?(add\s+(a\s+)?task\s+to|create\s+(a\s+)?task\s+to|new\s+task\s+to|remind\s+me\s+to|" +
                @"remind\s+me\s+in\s+\d+\s+(day|days|week|weeks)\s+to|i\s+need\s+to\s+(remember|do)\s+to|" +
                @"i\s+need\s+to\s+(remember|do)|can\s+you\s+(add|create)\s+(a\s+)?task\s+to|can\s+you\s+(add|create))\s*";

            string clean = Regex.Replace(input, stripPattern, "").Trim();
            clean = clean.TrimEnd('.', '!', '?');

            // If nothing was actually stripped (no recognised directive phrase matched,
            // e.g. "remind me in 3 days" with no trailing description), or the result is
            // empty, there is no real task description to use — fall back to a generic title.
            if (string.IsNullOrWhiteSpace(clean) || string.Equals(clean, input.Trim(), StringComparison.OrdinalIgnoreCase))
                return "Cybersecurity task";

            return char.ToUpperInvariant(clean[0]) + clean[1..];
        }
    }
}
