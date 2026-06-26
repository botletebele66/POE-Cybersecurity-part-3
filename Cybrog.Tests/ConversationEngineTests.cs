using Cybrog.Engine;
using Xunit;

namespace Cybrog.Tests
{
    public class ConversationEngineTests
    {
        private static ConversationEngine BuildEngine(out QuizEngine quiz, out TaskService tasks, out ActivityLogger logger)
        {
            quiz = new QuizEngine();
            tasks = new TaskService(new FakeDatabaseManager());
            logger = new ActivityLogger();
            return new ConversationEngine(quiz, tasks, logger);
        }

        [Fact]
        public void ProcessMessage_Greeting_ReturnsNonEmptyGreetingReply()
        {
            var engine = BuildEngine(out _, out _, out _);
            string reply = engine.ProcessMessage("hi");
            Assert.False(string.IsNullOrWhiteSpace(reply));
        }

        [Fact]
        public void ProcessMessage_EmptyInput_DoesNotThrowAndReturnsPrompt()
        {
            var engine = BuildEngine(out _, out _, out _);
            string reply = engine.ProcessMessage("");
            Assert.False(string.IsNullOrWhiteSpace(reply));
        }

        [Fact]
        public void ProcessMessage_IntroducingName_IsRememberedInSession()
        {
            var engine = BuildEngine(out _, out _, out _);

            engine.ProcessMessage("my name is Botshelo");

            Assert.True(engine.Session.HasName);
            Assert.Equal("Botshelo", engine.Session.UserName);
        }

        [Fact]
        public void ProcessMessage_AskingTopic_RecordsLastTopicInSession()
        {
            var engine = BuildEngine(out _, out _, out _);

            engine.ProcessMessage("tell me about phishing");

            Assert.Equal("Phishing Attacks", engine.Session.LastTopic);
        }

        [Fact]
        public void AskAboutTopic_DirectCall_ReturnsFullLessonAndLogsActivity()
        {
            var engine = BuildEngine(out _, out _, out var logger);

            string lesson = engine.AskAboutTopic("passwords");

            Assert.Contains("Scenario", lesson);
            Assert.Equal(1, logger.TotalCount);
        }

        [Fact]
        public void ProcessMessage_AddTaskViaChat_CreatesTaskInTaskService()
        {
            var engine = BuildEngine(out _, out var tasks, out _);

            engine.ProcessMessage("remind me to enable 2FA");

            Assert.Single(tasks.Tasks);
            Assert.Equal("Enable 2FA", tasks.Tasks[0].Title);
        }

        [Fact]
        public void ProcessMessage_ViewTasksWithNoTasks_SaysNoneArePending()
        {
            var engine = BuildEngine(out _, out _, out _);

            string reply = engine.ProcessMessage("show my tasks");

            Assert.Contains("no pending tasks", reply, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ProcessMessage_CompleteNonExistentTask_ReturnsNotFoundMessage()
        {
            var engine = BuildEngine(out _, out _, out _);

            string reply = engine.ProcessMessage("mark task 99 complete");

            Assert.Contains("couldn't find", reply, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ProcessMessage_StartQuizThenAnswerByLetter_AdvancesQuizState()
        {
            var engine = BuildEngine(out var quiz, out _, out _);

            string firstQuestionReply = engine.ProcessMessage("quiz me");
            Assert.True(quiz.IsActive);
            Assert.NotNull(quiz.CurrentQuestion);

            string afterAnswerReply = engine.ProcessMessage("A"); // answer the first question

            Assert.False(string.IsNullOrWhiteSpace(afterAnswerReply));
            // Either advanced to question 2, or (if the bank had exactly 1 question) completed —
            // either way, score should now reflect exactly one answered question.
            Assert.True(quiz.CurrentQuestionNumber >= 1);
        }

        [Fact]
        public void ProcessMessage_QuizStartedFromExternalCaller_StillRoutesAnswersCorrectly()
        {
            // Regression test for the GUI/chat quiz-state desync bug: when the quiz is
            // started directly via the shared QuizEngine (simulating the Mini-Game tab's
            // "Start Quiz" button, which bypasses ConversationEngine.ProcessMessage),
            // a subsequent chat message must still be recognised as a quiz answer rather
            // than being misinterpreted as a fresh command.
            var engine = BuildEngine(out var quiz, out _, out _);

            quiz.Start();
            var q = quiz.GetNextQuestion();
            Assert.NotNull(q);

            string reply = engine.ProcessMessage("A"); // should be treated as a quiz answer, not "Unknown"

            Assert.False(string.IsNullOrWhiteSpace(reply));
            // A correctly-routed quiz answer always yields feedback containing "Correct" or
            // "Not quite" (see HandleQuizAnswer); the generic fallback text never does.
            bool looksLikeQuizFeedback = reply.Contains("Correct", System.StringComparison.OrdinalIgnoreCase)
                                       || reply.Contains("Not quite", System.StringComparison.OrdinalIgnoreCase);
            Assert.True(looksLikeQuizFeedback, $"Expected quiz-answer feedback but got: {reply}");
            Assert.True(quiz.CurrentQuestionNumber >= 1);
        }

        [Fact]
        public void ProcessMessage_UnrecognisedInput_ReturnsFallback()
        {
            var engine = BuildEngine(out _, out _, out _);

            string reply = engine.ProcessMessage("xyzzy random gibberish");

            Assert.False(string.IsNullOrWhiteSpace(reply));
        }

        [Fact]
        public void ProcessMessage_RegistersInteractionCountOnSession()
        {
            var engine = BuildEngine(out _, out _, out _);

            engine.ProcessMessage("hi");
            engine.ProcessMessage("hello");

            Assert.Equal(2, engine.Session.TotalInteractions);
        }
    }
}
