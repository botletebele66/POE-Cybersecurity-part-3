using System;
using System.Collections.Generic;
using System.Linq;

namespace Cybrog.Engine
{
    /// <summary>Single timestamped entry in the activity log.</summary>
    public readonly record struct LogEntry(DateTime Timestamp, string Action, string Category);

    /// <summary>
    /// Records every significant bot action (task added/completed, reminder set, quiz
    /// started/finished, NLP-recognised command) with a timestamp, and exposes paged
    /// retrieval so the GUI can show "last 10" with an optional "show more".
    /// Satisfies Task 4.
    /// </summary>
    public class ActivityLogger
    {
        private readonly List<LogEntry> _entries = new();
        private const int MaxStoredEntries = 1000;

        public int TotalCount => _entries.Count;

        public event Action? LogChanged;

        public void Log(string action, string category = "General")
        {
            _entries.Add(new LogEntry(DateTime.Now, action, category));
            if (_entries.Count > MaxStoredEntries)
                _entries.RemoveAt(0);
            LogChanged?.Invoke();
        }

        /// <summary>Returns the most recent <paramref name="count"/> entries, newest first.</summary>
        public List<LogEntry> GetRecent(int count = 10) =>
            _entries.AsEnumerable().Reverse().Take(count).ToList();

        /// <summary>Returns every stored entry, newest first (used by "show more").</summary>
        public List<LogEntry> GetAll() =>
            _entries.AsEnumerable().Reverse().ToList();

        public void Clear()
        {
            _entries.Clear();
            LogChanged?.Invoke();
        }
    }
}
