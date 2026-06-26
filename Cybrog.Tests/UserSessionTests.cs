using Cybrog.Models;
using Xunit;

namespace Cybrog.Tests
{
    public class UserSessionTests
    {
        [Fact]
        public void NewSession_HasNoNameByDefault()
        {
            var session = new UserSession();
            Assert.False(session.HasName);
            Assert.Equal(string.Empty, session.UserName);
        }

        [Fact]
        public void SetName_TrimsWhitespaceAndSetsHasNameTrue()
        {
            var session = new UserSession();
            session.SetName("  Botshelo  ");
            Assert.True(session.HasName);
            Assert.Equal("Botshelo", session.UserName);
        }

        [Fact]
        public void RecordTopic_UpdatesLastTopicAndInterests()
        {
            var session = new UserSession();
            session.RecordTopic("Phishing Attacks");
            Assert.Equal("Phishing Attacks", session.LastTopic);
            Assert.Single(session.Interests);
        }

        [Fact]
        public void RecordTopic_EmptyOrWhitespace_IsIgnored()
        {
            var session = new UserSession();
            session.RecordTopic("");
            session.RecordTopic("   ");
            Assert.Equal(string.Empty, session.LastTopic);
            Assert.Empty(session.Interests);
        }

        [Fact]
        public void FavouriteTopic_NoInterestsYet_ReturnsEmptyString()
        {
            var session = new UserSession();
            Assert.Equal(string.Empty, session.FavouriteTopic);
        }

        [Fact]
        public void FavouriteTopic_ReturnsMostFrequentlyDiscussedTopic()
        {
            var session = new UserSession();
            session.RecordTopic("Phishing Attacks");
            session.RecordTopic("Password Security");
            session.RecordTopic("Phishing Attacks");
            session.RecordTopic("Phishing Attacks");

            Assert.Equal("Phishing Attacks", session.FavouriteTopic);
        }

        [Fact]
        public void LastTopic_ReflectsMostRecentRegardlessOfFrequency()
        {
            var session = new UserSession();
            session.RecordTopic("Phishing Attacks");
            session.RecordTopic("Phishing Attacks");
            session.RecordTopic("Malware Protection"); // discussed last, even though less frequent

            Assert.Equal("Malware Protection", session.LastTopic);
            Assert.Equal("Phishing Attacks", session.FavouriteTopic); // still the most frequent overall
        }

        [Fact]
        public void RegisterInteraction_IncrementsCounter()
        {
            var session = new UserSession();
            session.RegisterInteraction();
            session.RegisterInteraction();
            session.RegisterInteraction();
            Assert.Equal(3, session.TotalInteractions);
        }
    }
}
