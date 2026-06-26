namespace Cybrog.Models
{
    /// <summary>Type of quiz question — drives how options render in the GUI.</summary>
    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse
    }

    /// <summary>Difficulty level of a quiz question.</summary>
    public enum QuestionDifficulty
    {
        Beginner,
        Intermediate,
        Advanced
    }

    /// <summary>
    /// Immutable cybersecurity quiz question with answer options, explanation,
    /// and enriched metadata (difficulty, tags, references) for improved
    /// analytics and adaptive learning.
    /// </summary>
    public class QuizQuestion
    {
        // ---- Core content ----
        public string Text { get; }
        public IReadOnlyList<string> Options { get; }
        public int CorrectIndex { get; }
        public string Explanation { get; }
        public string Category { get; }
        public QuestionType Type { get; }

        // ---- Enhanced metadata ----
        public QuestionDifficulty Difficulty { get; }
        public IReadOnlyList<string> Tags { get; }
        public IReadOnlyList<string> References { get; }
        public string? Id { get; }  // optional unique identifier

        /// <summary>
        /// Primary constructor with full metadata.
        /// </summary>
        public QuizQuestion(
            string text,
            IReadOnlyList<string> options,
            int correctIndex,
            string explanation,
            string category,
            QuestionType type = QuestionType.MultipleChoice,
            QuestionDifficulty difficulty = QuestionDifficulty.Beginner,
            IReadOnlyList<string>? tags = null,
            IReadOnlyList<string>? references = null,
            string? id = null)
        {
            Text = text;
            Options = options;
            CorrectIndex = correctIndex;
            Explanation = explanation;
            Category = category;
            Type = type;
            Difficulty = difficulty;
            Tags = tags ?? new List<string>();
            References = references ?? new List<string>();
            Id = id;
        }

        /// <summary>
        /// Legacy constructor for backward compatibility.
        /// New code should use the primary constructor.
        /// </summary>
        public QuizQuestion(
            string text,
            IReadOnlyList<string> options,
            int correctIndex,
            string explanation,
            string category,
            QuestionType type = QuestionType.MultipleChoice)
            : this(text, options, correctIndex, explanation, category, type,
                  QuestionDifficulty.Beginner, null, null, null)
        { }

        /// <summary>
        /// Returns a formatted string for UI display (e.g., in tooltips or lists).
        /// </summary>
        public string GetDisplayInfo() =>
            $"{Category} • {Difficulty} • Tags: {(Tags.Any() ? string.Join(", ", Tags) : "None")}";

        /// <summary>
        /// Returns the correct answer text for review purposes.
        /// </summary>
        public string GetCorrectAnswer() =>
            Options[CorrectIndex];
    }
}