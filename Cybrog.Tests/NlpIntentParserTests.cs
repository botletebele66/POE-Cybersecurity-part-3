using Cybrog.Engine;
using Xunit;

namespace Cybrog.Tests
{
    public class NlpIntentParserTests
    {
        private readonly NlpIntentParser _parser = new();

        [Theory]
        [InlineData("hi")]
        [InlineData("hello there")]
        [InlineData("Hey!")]
        [InlineData("good morning")]
        [InlineData("hey cybrog, how are you")]
        public void Parse_RecognisesGreetings(string input)
        {
            var (intent, _, _, _) = _parser.Parse(input);
            Assert.Equal(Intent.Greeting, intent);
        }

        [Theory]
        [InlineData("hide and seek")]
        [InlineData("history of phishing")]
        [InlineData("highway")]
        public void Parse_DoesNotFalsePositiveGreetingOnHWords(string input)
        {
            var (intent, _, _, _) = _parser.Parse(input);
            Assert.NotEqual(Intent.Greeting, intent);
        }

        [Theory]
        [InlineData("bye")]
        [InlineData("goodbye!")]
        [InlineData("take care")]
        public void Parse_RecognisesFarewells(string input)
        {
            var (intent, _, _, _) = _parser.Parse(input);
            Assert.Equal(Intent.Farewell, intent);
        }

        [Theory]
        [InlineData("add a task to enable 2FA", "Enable 2FA")]
        [InlineData("can you add a task to review my firewall settings", "Review my firewall settings")]
        [InlineData("create a task to backup my files", "Backup my files")]
        [InlineData("new task to scan for malware", "Scan for malware")]
        public void Parse_AddTask_ExtractsCorrectTitle(string input, string expectedTitle)
        {
            var (intent, _, taskText, _) = _parser.Parse(input);
            Assert.Equal(Intent.AddTask, intent);
            Assert.Equal(expectedTitle, taskText);
        }

        [Fact]
        public void Parse_RemindMeTo_ReturnsSetReminderWithTitle()
        {
            var (intent, _, taskText, _) = _parser.Parse("remind me to update my password");
            Assert.Equal(Intent.SetReminder, intent);
            Assert.Equal("Update my password", taskText);
        }

        [Fact]
        public void Parse_RemindMeInDaysWithDescription_ExtractsTitleAndDays()
        {
            var (intent, _, taskText, days) = _parser.Parse("remind me in 3 days to check privacy settings");
            Assert.Equal(Intent.SetReminder, intent);
            Assert.Equal("Check privacy settings", taskText);
            Assert.Equal(3, days);
        }

        [Fact]
        public void Parse_RemindMeInDaysWithoutDescription_FallsBackToGenericTitle()
        {
            // Regression test: this previously returned the raw unstripped input
            // "remind me in 3 days" as the task title instead of a sensible default.
            var (intent, _, taskText, days) = _parser.Parse("remind me in 3 days");
            Assert.Equal(Intent.SetReminder, intent);
            Assert.Equal("Cybersecurity task", taskText);
            Assert.Equal(3, days);
        }

        [Fact]
        public void Parse_RemindMeInWeeks_MultipliesDaysCorrectly()
        {
            var (_, _, _, days) = _parser.Parse("remind me in 2 weeks to renew my antivirus");
            Assert.Equal(14, days);
        }

        [Theory]
        [InlineData("delete task 2", 2)]
        [InlineData("remove task #3", 3)]
        [InlineData("cancel reminder 7", 7)]
        public void Parse_DeleteTask_ExtractsId(string input, int expectedId)
        {
            var (intent, taskId, _, _) = _parser.Parse(input);
            Assert.Equal(Intent.DeleteTask, intent);
            Assert.Equal(expectedId, taskId);
        }

        [Theory]
        [InlineData("mark task 1 complete", 1)]
        [InlineData("complete task 4", 4)]
        [InlineData("task 5 done", 5)]
        public void Parse_CompleteTask_ExtractsId(string input, int expectedId)
        {
            var (intent, taskId, _, _) = _parser.Parse(input);
            Assert.Equal(Intent.CompleteTask, intent);
            Assert.Equal(expectedId, taskId);
        }

        [Theory]
        [InlineData("show my tasks")]
        [InlineData("what are my pending tasks")]
        [InlineData("list my reminders")]
        public void Parse_RecognisesViewTasks(string input)
        {
            var (intent, _, _, _) = _parser.Parse(input);
            Assert.Equal(Intent.ViewTasks, intent);
        }

        [Theory]
        [InlineData("start the quiz")]
        [InlineData("quiz me")]
        [InlineData("play a mini-game")]
        [InlineData("test my knowledge")]
        public void Parse_RecognisesStartQuiz(string input)
        {
            var (intent, _, _, _) = _parser.Parse(input);
            Assert.Equal(Intent.StartQuiz, intent);
        }

        [Theory]
        [InlineData("show activity log")]
        [InlineData("what have you done for me")]
        public void Parse_RecognisesActivityLog(string input)
        {
            var (intent, _, _, _) = _parser.Parse(input);
            Assert.Equal(Intent.ShowActivityLog, intent);
        }

        [Theory]
        [InlineData("xyzzy random gibberish")]
        [InlineData("tell me about phishing")] // handled by topic matcher upstream, not NLP intent
        public void Parse_UnrecognisedInput_ReturnsUnknown(string input)
        {
            var (intent, _, _, _) = _parser.Parse(input);
            Assert.Equal(Intent.Unknown, intent);
        }

        [Fact]
        public void Parse_EmptyOrWhitespace_ReturnsUnknownWithoutThrowing()
        {
            var (intent1, _, _, _) = _parser.Parse("");
            var (intent2, _, _, _) = _parser.Parse("   ");
            Assert.Equal(Intent.Unknown, intent1);
            Assert.Equal(Intent.Unknown, intent2);
        }
    }
}
