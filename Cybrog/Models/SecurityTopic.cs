using System;
using System.Collections.Generic;
using System.Linq;

namespace Cybrog.Models
{
    /// <summary>
    /// Defines the difficulty level of a security topic, useful for
    /// adaptive learning or filtering.
    /// </summary>
    public enum TopicDifficulty
    {
        Beginner,
        Intermediate,
        Advanced
    }

    /// <summary>
    /// A cybersecurity learning topic presented in the Topics panel and chat.
    /// Content follows the STOMP structure (Scenario, Technique, Outcome, Mitigation,
    /// Practice) introduced in Part 2, with references to Stallings (2022) and other
    /// authoritative texts. Extended with metadata for categorisation, difficulty,
    /// and tags to support richer UI and analytics.
    /// </summary>
    public class SecurityTopic
    {
        // ---- Core STOMP content ----
        public string Key { get; }
        public string DisplayName { get; }
        public string Icon { get; }
        public string Scenario { get; }
        public string Technique { get; }
        public string Outcome { get; }
        public string Mitigation { get; }
        public string Practice { get; }

        // ---- Enhanced metadata ----
        public IReadOnlyList<string> Sources { get; }
        public string Category { get; }        // e.g., "Attack", "Defense", "Principle"
        public TopicDifficulty Difficulty { get; }
        public IReadOnlyList<string> Tags { get; }  // e.g., "phishing", "email", "social"

        /// <summary>
        /// Legacy single reference string – kept for backward compatibility,
        /// but preferably use <see cref="Sources"/>.
        /// </summary>
        [Obsolete("Use Sources property instead")]
        public string Reference => Sources.Count > 0 ? Sources[0] : string.Empty;

        // ---- Primary constructor (enhanced) ----
        public SecurityTopic(
            string key,
            string displayName,
            string icon,
            string scenario,
            string technique,
            string outcome,
            string mitigation,
            string practice,
            IReadOnlyList<string> sources,
            string category,
            TopicDifficulty difficulty,
            IReadOnlyList<string>? tags = null)
        {
            Key = key;
            DisplayName = displayName;
            Icon = icon;
            Scenario = scenario;
            Technique = technique;
            Outcome = outcome;
            Mitigation = mitigation;
            Practice = practice;
            Sources = sources ?? new List<string>();
            Category = category;
            Difficulty = difficulty;
            Tags = tags ?? new List<string>();
        }

        // ---- Legacy constructor for backward compatibility ----
        [Obsolete("Use the constructor with sources, category, difficulty, and tags.")]
        public SecurityTopic(
            string key,
            string displayName,
            string icon,
            string scenario,
            string technique,
            string outcome,
            string mitigation,
            string practice,
            string reference)
            : this(
                key,
                displayName,
                icon,
                scenario,
                technique,
                outcome,
                mitigation,
                practice,
                new[] { reference },
                "General",
                TopicDifficulty.Beginner,
                new[] { key })
        { }

        /// <summary>
        /// Builds the full plain-text STOMP lesson shown in the chat window.
        /// Displays all sources as a bulleted list.
        /// </summary>
        public string BuildLesson()
        {
            var sourceList = Sources.Count == 0
                ? "No specific references."
                : string.Join("\n", Sources.Select(s => $"• {s}"));

            return
                $"{Icon} {DisplayName.ToUpperInvariant()}\n\n" +
                $"Scenario\n{Scenario}\n\n" +
                $"Technique\n{Technique}\n\n" +
                $"Outcome\n{Outcome}\n\n" +
                $"Mitigation\n{Mitigation}\n\n" +
                $"Practice\n{Practice}\n\n" +
                $"References:\n{sourceList}";
        }

        /// <summary>
        /// Returns a short summary for use in list views or tooltips.
        /// </summary>
        public string GetSummary() =>
            $"{Icon} {DisplayName} — {Category} ({Difficulty})";
    }
}