using Cybrog.Engine;
using Xunit;

namespace Cybrog.Tests
{
    public class SentimentAnalyzerTests
    {
        [Theory]
        [InlineData("I think I've been hacked, I'm so scared")]
        [InlineData("I'm worried someone stole my password")]
        public void Detect_RecognisesWorriedSentiment(string input)
        {
            Assert.Equal(Sentiment.Worried, SentimentAnalyzer.Detect(input));
        }

        [Theory]
        [InlineData("this is so annoying and useless")]
        [InlineData("I'm so frustrated with this")]
        public void Detect_RecognisesFrustratedSentiment(string input)
        {
            Assert.Equal(Sentiment.Frustrated, SentimentAnalyzer.Detect(input));
        }

        [Fact]
        public void Detect_NeutralInput_ReturnsNeutral()
        {
            Assert.Equal(Sentiment.Neutral, SentimentAnalyzer.Detect("what is phishing"));
        }

        [Fact]
        public void GetEmpathyOpener_Worried_ReturnsReassuringText()
        {
            string? opener = SentimentAnalyzer.GetEmpathyOpener(Sentiment.Worried);
            Assert.NotNull(opener);
            Assert.NotEmpty(opener!);
        }

        [Fact]
        public void GetEmpathyOpener_Neutral_ReturnsNull()
        {
            Assert.Null(SentimentAnalyzer.GetEmpathyOpener(Sentiment.Neutral));
        }
    }

    public class KeywordTopicMatcherTests
    {
        [Theory]
        [InlineData("tell me about phishing", "phishing")]
        [InlineData("what's a scam email", "phishing")]
        [InlineData("how do I make a strong password", "passwords")]
        [InlineData("what is 2fa", "passwords")]
        [InlineData("I think I have a virus", "malware")]
        [InlineData("what is ransomware", "malware")]
        [InlineData("is public wifi safe", "browsing")]
        [InlineData("what does https mean", "browsing")]
        [InlineData("are app permissions important", "mobile")]
        public void Match_ReturnsExpectedTopicKey(string input, string expectedKey)
        {
            Assert.Equal(expectedKey, KeywordTopicMatcher.Match(input));
        }

        [Fact]
        public void Match_UnrelatedInput_ReturnsNull()
        {
            Assert.Null(KeywordTopicMatcher.Match("what's the weather like today"));
        }
    }

    public class SecurityKnowledgeBaseTests
    {
        [Theory]
        [InlineData("phishing")]
        [InlineData("passwords")]
        [InlineData("malware")]
        [InlineData("browsing")]
        [InlineData("mobile")]
        public void Get_ReturnsTopicForAllFiveRequiredCategories(string key)
        {
            var topic = SecurityKnowledgeBase.Get(key);
            Assert.NotNull(topic);
        }

        [Theory]
        [InlineData("cia")]
        [InlineData("social")]
        [InlineData("network")]
        [InlineData("incident")]
        public void Get_ReturnsTopicForAdvancedCategories(string key)
        {
            var topic = SecurityKnowledgeBase.Get(key);
            Assert.NotNull(topic);
        }

        [Fact]
        public void Get_UnknownKey_ReturnsNull()
        {
            Assert.Null(SecurityKnowledgeBase.Get("nonexistent-topic"));
        }

        [Fact]
        public void Get_IsCaseInsensitive()
        {
            Assert.NotNull(SecurityKnowledgeBase.Get("PHISHING"));
            Assert.NotNull(SecurityKnowledgeBase.Get("Phishing"));
        }

        [Theory]
        [InlineData("phishing")]
        [InlineData("passwords")]
        [InlineData("malware")]
        [InlineData("browsing")]
        [InlineData("mobile")]
        [InlineData("cia")]
        [InlineData("social")]
        [InlineData("network")]
        [InlineData("incident")]
        public void BuildLesson_ProducesNonEmptyStructuredContent(string key)
        {
            var topic = SecurityKnowledgeBase.Get(key)!;
            string lesson = topic.BuildLesson();

            Assert.Contains("Scenario", lesson);
            Assert.Contains("Technique", lesson);
            Assert.Contains("Outcome", lesson);
            Assert.Contains("Mitigation", lesson);
            Assert.Contains("Practice", lesson);
            Assert.Contains("Stallings", lesson);
        }

        [Fact]
        public void All_ReturnsAllTopics()
        {
            int count = 0;
            foreach (var _ in SecurityKnowledgeBase.All) count++;
            Assert.Equal(9, count);  // 5 core topics + 4 advanced topics (CIA Triad, Social Engineering, Network Security, Incident Response)
        }
    }
}