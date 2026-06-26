using System;
using System.Linq;
using System.Text.RegularExpressions;
using Cybrog.Models;

namespace Cybrog.Engine
{
    /// <summary>
    /// The central orchestrator: receives one line of raw user input and decides
    /// what Cybrog should do and say. Wires together intent parsing (NLP), sentiment
    /// detection, topic matching, the quiz engine, the task service, and the
    /// activity logger — while keeping each of those concerns in its own class
    /// (single-responsibility, composed rather than inherited). This is the only
    /// class the GUI layer (MainWindow) talks to for conversational logic.
    /// </summary>
    public class ConversationEngine
    {
        private readonly NlpIntentParser _nlp = new();
        private readonly QuizEngine _quiz;
        private readonly TaskService _tasks;
        private readonly ActivityLogger _logger;
        public UserSession Session { get; } = new();

        public QuizEngine Quiz => _quiz;
        public TaskService Tasks => _tasks;
        public ActivityLogger Logger => _logger;

        public ConversationEngine(QuizEngine quiz, TaskService tasks, ActivityLogger logger)
        {
            _quiz = quiz;
            _tasks = tasks;
            _logger = logger;
        }

        /// <summary>
        /// Processes one user message and returns Cybrog's reply text.
        /// This is the single entry point the GUI calls per message.
        /// </summary>
        public string ProcessMessage(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return "I didn't catch anything — try typing a question or pick a Quick Topic below.";

            Session.RegisterInteraction();

            // Quiz answers take priority whenever the shared QuizEngine has a question
            // pending — whether the quiz was started via chat ("quiz me") or via the
            // Mini-Game tab's Start Quiz button, since both share this same QuizEngine
            // instance and CurrentQuestion reflects whichever path started it.
            if (_quiz.IsActive && _quiz.CurrentQuestion != null)
                return HandleQuizAnswer(userInput);

            // Detect a "my name is ..." style introduction before general intent parsing.
            var nameMatch = Regex.Match(userInput, @"(?i)my\s+name\s+is\s+(\w+)|i'?m\s+(\w+)\b|call\s+me\s+(\w+)");
            if (nameMatch.Success)
            {
                string name = nameMatch.Groups[1].Success ? nameMatch.Groups[1].Value
                            : nameMatch.Groups[2].Success ? nameMatch.Groups[2].Value
                            : nameMatch.Groups[3].Value;
                Session.SetName(name);
                _logger.Log($"User introduced themselves as {name}", "Session");
                return $"Nice to meet you, {name}! How can I help with your cybersecurity today?";
            }

            var (intent, taskId, taskText, reminderDays) = _nlp.Parse(userInput);
            string sentimentOpener = SentimentAnalyzer.GetPersonalisedResponse(userInput); // new: richer opener

            return intent switch
            {
                Intent.Greeting => ResponseLibrary.GetGreeting(),
                Intent.Farewell => ResponseLibrary.GetFarewell(),
                Intent.AskHelp => ResponseLibrary.GetHelpText(),
                Intent.AskName => Session.HasName
                    ? $"You told me your name is {Session.UserName}!"
                    : "I don't think you've told me your name yet — try \"my name is ...\".",
                Intent.AddTask => HandleAddTask(taskText!),
                Intent.SetReminder => HandleAddTask(taskText!, reminderDays),
                Intent.ViewTasks => HandleViewTasks(),
                Intent.CompleteTask => HandleCompleteTask(taskId!.Value),
                Intent.DeleteTask => HandleDeleteTask(taskId!.Value),
                Intent.StartQuiz => HandleStartQuiz(),
                Intent.ShowActivityLog => HandleShowLog(),
                _ => sentimentOpener + TryAnswerTopicQuestion(userInput) // use personalised opener
            };
        }

        // ── Topic Q&A (chat-driven education) ───────────────────────────────

        private string TryAnswerTopicQuestion(string userInput)
        {
            string? topicKey = KeywordTopicMatcher.Match(userInput);
            if (topicKey == null)
                return ResponseLibrary.GetFallback(); // fallback already has empathy? We can keep as is.

            return AskAboutTopic(topicKey);
        }

        /// <summary>Returns the full STOMP lesson for a topic key and records it in session memory + the activity log. Used by both chat and the Topics panel buttons.</summary>
        public string AskAboutTopic(string topicKey)
        {
            var topic = SecurityKnowledgeBase.Get(topicKey);
            if (topic == null)
                return ResponseLibrary.GetFallback();

            Session.RecordTopic(topic.DisplayName);
            _logger.Log($"Discussed topic: {topic.DisplayName}", "Education");
            return topic.BuildLesson();
        }

        // ── Task commands ────────────────────────────────────────────────────

        private string HandleAddTask(string title, int? reminderDays = null)
        {
            if (!_tasks.IsDatabaseAvailable)
                return $"I'd love to save that, but the database isn't available right now ({_tasks.DatabaseError}). Please check your SQL Server LocalDB installation.";

            DateTime? reminder = reminderDays.HasValue ? DateTime.Now.AddDays(reminderDays.Value) : null;
            var item = _tasks.AddTask(title, string.Empty, reminder);
            if (item == null)
                return "Something went wrong saving that task — please try again.";

            _logger.Log($"Added task: {title}", "Tasks");
            return ResponseLibrary.GetTaskAddedConfirmation(title, reminder);
        }

        private string HandleViewTasks()
        {
            var pending = _tasks.PendingTasks;
            if (pending.Count == 0)
                return ResponseLibrary.GetTaskListEmpty(); // use new dedicated response

            var lines = pending.Select((t, i) => $"{i + 1}. {t.Title}{(t.ReminderDate.HasValue ? $" (reminder: {t.ReminderDate.Value.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture)})" : string.Empty)}");
            return "Your pending tasks:\n" + string.Join("\n", lines);
        }

        private string HandleCompleteTask(int displayNumber)
        {
            var item = _tasks.FindByDisplayNumber(displayNumber) ?? _tasks.FindById(displayNumber);
            if (item == null)
                return ResponseLibrary.GetTaskNotFound(displayNumber);

            _tasks.CompleteTask(item.Id);
            _logger.Log($"Completed task: {item.Title}", "Tasks");
            return ResponseLibrary.GetTaskCompletedConfirmation(item.Title);
        }

        private string HandleDeleteTask(int displayNumber)
        {
            var item = _tasks.FindByDisplayNumber(displayNumber) ?? _tasks.FindById(displayNumber);
            if (item == null)
                return ResponseLibrary.GetTaskNotFound(displayNumber);

            string title = item.Title;
            _tasks.DeleteTask(item.Id);
            _logger.Log($"Deleted task: {title}", "Tasks");
            return ResponseLibrary.GetTaskDeletedConfirmation(title);
        }

        // ── Quiz flow ─────────────────────────────────────────────────────────

        private string HandleStartQuiz()
        {
            _quiz.Start();
            _logger.Log("Started cybersecurity quiz", "Quiz");
            var q = _quiz.GetNextQuestion();
            if (q == null) return "The quiz couldn't start — please try again.";

            // use quiz intro from library
            return ResponseLibrary.GetQuizIntro() + "\n\n" +
                   FormatQuestion(q, _quiz.CurrentQuestionNumber, _quiz.TotalQuestions);
        }

        private string HandleQuizAnswer(string userInput)
        {
            var q = _quiz.CurrentQuestion;
            if (q == null)
            {
                return ResponseLibrary.GetFallback();
            }

            int? selected = ParseAnswerIndex(userInput, q.Options.Count);
            if (selected == null)
                return $"Please answer with a letter (A-{(char)('A' + q.Options.Count - 1)}) or the option number.";

            bool correct = _quiz.SubmitAnswer(selected.Value);
            string feedback = correct
                ? ResponseLibrary.GetQuizCorrect()
                : ResponseLibrary.GetQuizIncorrect() + " " + _quiz.LastExplanation;

            var next = _quiz.GetNextQuestion();
            if (next == null)
            {
                var (score, total, msg) = _quiz.GetFinalResult();
                _logger.Log($"Completed quiz: {score}/{total}", "Quiz");
                return $"{feedback}\n\nQuiz complete! You scored {score}/{total}. {msg}";
            }

            return $"{feedback}\n\n{FormatQuestion(next, _quiz.CurrentQuestionNumber, _quiz.TotalQuestions)}";
        }

        private static string FormatQuestion(QuizQuestion q, int number, int total)
        {
            var lines = q.Options.Select((opt, i) => $"{(char)('A' + i)}) {opt}");
            return $"Question {number}/{total} [{q.Category}]\n{q.Text}\n\n{string.Join("\n", lines)}";
        }

        private static int? ParseAnswerIndex(string input, int optionCount)
        {
            input = input.Trim();
            if (input.Length >= 1)
            {
                char c = char.ToUpperInvariant(input[0]);
                if (c >= 'A' && c < 'A' + optionCount) return c - 'A';
            }
            if (int.TryParse(input, out int n) && n >= 1 && n <= optionCount) return n - 1;
            return null;
        }

        // ── Activity log ─────────────────────────────────────────────────────

        private string HandleShowLog()
        {
            var recent = _logger.GetRecent(10);
            if (recent.Count == 0)
                return ResponseLibrary.GetActivityLogEmpty(); // use dedicated empty message

            var lines = recent.Select(e => $"{e.Timestamp:HH:mm} — {e.Action}");
            return "Recent activity:\n" + string.Join("\n", lines);
        }
    }
}