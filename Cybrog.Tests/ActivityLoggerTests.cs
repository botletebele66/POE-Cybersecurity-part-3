using Cybrog.Engine;
using Xunit;

namespace Cybrog.Tests
{
    public class ActivityLoggerTests
    {
        [Fact]
        public void Log_IncrementsTotalCount()
        {
            var logger = new ActivityLogger();
            logger.Log("Did something");
            Assert.Equal(1, logger.TotalCount);
        }

        [Fact]
        public void GetRecent_ReturnsNewestFirst()
        {
            var logger = new ActivityLogger();
            logger.Log("First action");
            logger.Log("Second action");
            logger.Log("Third action");

            var recent = logger.GetRecent(10);

            Assert.Equal("Third action", recent[0].Action);
            Assert.Equal("Second action", recent[1].Action);
            Assert.Equal("First action", recent[2].Action);
        }

        [Fact]
        public void GetRecent_RespectsRequestedCount()
        {
            var logger = new ActivityLogger();
            for (int i = 0; i < 20; i++) logger.Log($"Action {i}");

            var recent = logger.GetRecent(5);

            Assert.Equal(5, recent.Count);
        }

        [Fact]
        public void GetRecent_FewerEntriesThanRequested_ReturnsAllAvailable()
        {
            var logger = new ActivityLogger();
            logger.Log("Only action");

            var recent = logger.GetRecent(10);

            Assert.Single(recent);
        }

        [Fact]
        public void GetAll_ReturnsEveryEntryNewestFirst()
        {
            var logger = new ActivityLogger();
            logger.Log("A");
            logger.Log("B");

            var all = logger.GetAll();

            Assert.Equal(2, all.Count);
            Assert.Equal("B", all[0].Action);
        }

        [Fact]
        public void Clear_RemovesAllEntries()
        {
            var logger = new ActivityLogger();
            logger.Log("Something");
            logger.Clear();

            Assert.Equal(0, logger.TotalCount);
            Assert.Empty(logger.GetAll());
        }

        [Fact]
        public void Log_RaisesLogChangedEvent()
        {
            var logger = new ActivityLogger();
            bool eventRaised = false;
            logger.LogChanged += () => eventRaised = true;

            logger.Log("Something happened");

            Assert.True(eventRaised);
        }

        [Fact]
        public void Clear_RaisesLogChangedEvent()
        {
            var logger = new ActivityLogger();
            logger.Log("Something");
            bool eventRaised = false;
            logger.LogChanged += () => eventRaised = true;

            logger.Clear();

            Assert.True(eventRaised);
        }

        [Fact]
        public void Log_DefaultsToGeneralCategoryWhenNotSpecified()
        {
            var logger = new ActivityLogger();
            logger.Log("No category specified");

            var entry = logger.GetRecent(1)[0];

            Assert.Equal("General", entry.Category);
        }

        [Fact]
        public void Log_CapsStoredEntriesAtMaximum()
        {
            // The logger caps stored entries at 1000 to bound memory use over a very long
            // session; the oldest entries should be evicted first (FIFO).
            var logger = new ActivityLogger();
            for (int i = 0; i < 1005; i++)
                logger.Log($"Action {i}");

            Assert.Equal(1000, logger.TotalCount);
            // The very first 5 actions (0-4) should have been evicted; action 5 should be the oldest survivor.
            var all = logger.GetAll();
            Assert.Equal("Action 1004", all[0].Action); // newest
            Assert.Equal("Action 5", all[^1].Action);   // oldest surviving entry
        }
    }
}
