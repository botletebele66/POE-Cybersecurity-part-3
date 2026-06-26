using System;
using Cybrog.Engine;
using Xunit;

namespace Cybrog.Tests
{
    public class ResponseLibraryTests
    {
        [Fact]
        public void GetGreeting_AlwaysReturnsNonEmptyString()
        {
            for (int i = 0; i < 20; i++)
                Assert.False(string.IsNullOrWhiteSpace(ResponseLibrary.GetGreeting()));
        }

        [Fact]
        public void GetFarewell_AlwaysReturnsNonEmptyString()
        {
            for (int i = 0; i < 20; i++)
                Assert.False(string.IsNullOrWhiteSpace(ResponseLibrary.GetFarewell()));
        }

        [Fact]
        public void GetFallback_AlwaysReturnsNonEmptyString()
        {
            for (int i = 0; i < 20; i++)
                Assert.False(string.IsNullOrWhiteSpace(ResponseLibrary.GetFallback()));
        }

        [Fact]
        public void GetTaskAddedConfirmation_WithReminder_IncludesFormattedDate()
        {
            string msg = ResponseLibrary.GetTaskAddedConfirmation("Enable 2FA", new DateTime(2026, 5, 30));
            Assert.Contains("Enable 2FA", msg);
            Assert.Contains("30 May 2026", msg);
        }

        [Fact]
        public void GetTaskAddedConfirmation_WithoutReminder_DoesNotMentionReminder()
        {
            string msg = ResponseLibrary.GetTaskAddedConfirmation("Enable 2FA", null);
            Assert.Contains("Enable 2FA", msg);
            Assert.DoesNotContain("reminder", msg, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetTaskNotFound_IncludesTheRequestedId()
        {
            string msg = ResponseLibrary.GetTaskNotFound(42);
            Assert.Contains("42", msg);
        }

        [Fact]
        public void GetHelpText_MentionsCoreCommands()
        {
            string help = ResponseLibrary.GetHelpText();
            Assert.Contains("quiz", help, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("task", help, StringComparison.OrdinalIgnoreCase);
        }
    }
}
