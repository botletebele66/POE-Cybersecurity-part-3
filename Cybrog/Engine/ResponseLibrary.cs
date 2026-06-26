using System;
using System.Collections.Generic;

namespace Cybrog.Engine
{
    /// <summary>
    /// Library of canned-but-varied responses for conversational scaffolding
    /// (greetings, farewells, help text, unrecognised-input fallback). Random
    /// selection between several phrasings per category keeps the conversation
    /// from feeling robotic and repetitive across a long session, while all
    /// security-specific content lives in <see cref="SecurityKnowledgeBase"/>.
    /// </summary>
    public static class ResponseLibrary
    {
        private static readonly Random Rand = new();

        // ----- Expanded greetings (now 6 options) -----
        private static readonly string[] Greetings =
        {
            "Hi there! I'm Cybrog, your cybersecurity companion. Ask me about phishing, passwords, malware, safe browsing, or mobile security.",
            "Hello! Ready to talk cybersecurity? Try asking about phishing or check the Topics tab.",
            "Hey! I'm Cybrog. I can teach you about staying safe online, manage your security tasks, or quiz your knowledge — just ask.",
            "Good to see you! I'm here to help you level up your digital safety. What would you like to learn today?",
            "Welcome back! Remember: security is a journey, not a destination. Ask me anything.",
            "Greetings, security enthusiast! I've got tips, tasks, and quizzes — where shall we start?"
        };

        // ----- Expanded farewells (now 5 options) -----
        private static readonly string[] Farewells =
        {
            "Stay safe out there! Come back any time you want to learn more.",
            "Goodbye! Remember: when in doubt, don't click.",
            "See you soon! Your security habits are looking good — keep it up.",
            "Until next time! Keep your software updated and your 2FA on.",
            "Farewell! May your passwords be long and your phishing alerts be few."
        };

        // ----- Expanded fallbacks (now 5 options) -----
        private static readonly string[] Fallbacks =
        {
            "I'm not quite sure I follow — try asking about phishing, passwords, malware, safe browsing, or mobile security, or type 'help' to see what I can do.",
            "Hmm, I didn't catch that. You can ask me to explain a topic, add a task, start the quiz, or show your activity log.",
            "I want to help, but I'm not sure what you mean. Try one of the Quick Topic buttons below, or type 'help'.",
            "Sorry, I didn't understand that. Could you rephrase? I'm best with security questions and task commands.",
            "Not quite sure what you're asking for. Type 'help' for a list of things I can do."
        };

        // ----- New: empty task list -----
        private static readonly string[] TaskListEmpty =
        {
            "You have no tasks right now — let's fix that! Try saying \"add a task\".",
            "Your task list is empty. Add your first security task to get started!",
            "Nothing on your plate yet. Add a task like \"remind me to update my antivirus\"."
        };

        // ----- New: quiz intros -----
        private static readonly string[] QuizIntros =
        {
            "Here's a quiz question to test your cybersecurity smarts:",
            "Pop quiz! See if you know this one:",
            "Time to challenge your knowledge! What's the answer?"
        };

        // ----- New: quiz results (correct / incorrect) -----
        private static readonly string[] QuizCorrect =
        {
            "Exactly right! Well done!",
            "That's correct — you're on fire!",
            "Boom! Nailed it."
        };
        private static readonly string[] QuizIncorrect =
        {
            "Not quite — but you're learning! The right answer is:",
            "Close, but not this time. The correct answer is:",
            "Good try! The actual answer is:"
        };

        // ----- New: activity log empty -----
        private static readonly string[] ActivityLogEmpty =
        {
            "Your activity log is empty so far. Start asking questions or completing tasks to fill it up!",
            "No activity recorded yet. Interact with me to build your log.",
            "Nothing in the log. How about we start with a question or a task?"
        };

        // ----- Existing public methods (unchanged) -----
        public static string GetGreeting() => Greetings[Rand.Next(Greetings.Length)];
        public static string GetFarewell() => Farewells[Rand.Next(Farewells.Length)];
        public static string GetFallback() => Fallbacks[Rand.Next(Fallbacks.Length)];

        public static string GetHelpText() =>
            "Here's what I can do:\n\n" +
            "• Ask about a topic — e.g. \"tell me about phishing\"\n" +
            "• Add a task — e.g. \"remind me to enable 2FA\"\n" +
            "• View tasks — \"show my tasks\"\n" +
            "• Complete a task — \"mark task 2 done\"\n" +
            "• Delete a task — \"delete task 2\"\n" +
            "• Start the quiz — \"quiz me\" or use the Mini-Game tab\n" +
            "• Check your history — \"show activity log\"";

        // ----- Existing confirmation methods (unchanged) -----
        public static string GetTaskAddedConfirmation(string title, DateTime? reminder) =>
            reminder.HasValue
                ? $"Got it — I've added \"{title}\" to your tasks, with a reminder for {reminder.Value.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture)}."
                : $"Got it — I've added \"{title}\" to your tasks.";

        public static string GetTaskCompletedConfirmation(string title) =>
            $"Nice work! \"{title}\" is marked complete.";

        public static string GetTaskDeletedConfirmation(string title) =>
            $"Done — \"{title}\" has been removed from your tasks.";

        public static string GetTaskNotFound(int id) =>
            $"I couldn't find task #{id}. Try \"show my tasks\" to see the current list and their numbers.";

        // ----- New public methods for additional responses -----
        public static string GetTaskListEmpty() => TaskListEmpty[Rand.Next(TaskListEmpty.Length)];

        public static string GetQuizIntro() => QuizIntros[Rand.Next(QuizIntros.Length)];

        public static string GetQuizCorrect() => QuizCorrect[Rand.Next(QuizCorrect.Length)];

        public static string GetQuizIncorrect() => QuizIncorrect[Rand.Next(QuizIncorrect.Length)];

        public static string GetActivityLogEmpty() => ActivityLogEmpty[Rand.Next(ActivityLogEmpty.Length)];
    }
}