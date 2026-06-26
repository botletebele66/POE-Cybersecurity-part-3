using System;
using System.Collections.Generic;
using System.Linq;
using Cybrog.Engine;
using Cybrog.Models;

namespace Cybrog.Tests
{
    /// <summary>
    /// In-memory stand-in for <see cref="DatabaseManager"/> used purely for unit testing
    /// <see cref="TaskService"/>'s logic in isolation from a real SQL Server connection.
    /// Mirrors the same identity-generation, CRUD, and "not found" semantics the real
    /// SQL Server-backed implementation has, so tests against this fake exercise the same
    /// contract <see cref="TaskService"/> actually relies on.
    /// </summary>
    public class FakeDatabaseManager : IDatabaseManager
    {
        private readonly List<TaskItem> _store = new();
        private int _nextId = 1;

        public bool IsAvailable { get; set; } = true;
        public string? LastError { get; set; }

        public int AddTask(string title, string? description = null, DateTime? reminderDate = null)
        {
            var item = new TaskItem
            {
                Id = _nextId++,
                Title = title,
                Description = description ?? string.Empty,
                CreatedDate = DateTime.Now,
                ReminderDate = reminderDate,
                IsCompleted = false
            };
            _store.Add(item);
            return item.Id;
        }

        public List<TaskItem> GetTasks(bool includeCompleted = true) =>
            includeCompleted ? _store.ToList() : _store.Where(t => !t.IsCompleted).ToList();

        public bool MarkTaskCompleted(int id)
        {
            var item = _store.FirstOrDefault(t => t.Id == id);
            if (item == null) return false;
            item.IsCompleted = true;
            return true;
        }

        public bool MarkTaskPending(int id)
        {
            var item = _store.FirstOrDefault(t => t.Id == id);
            if (item == null) return false;
            item.IsCompleted = false;
            return true;
        }

        public bool DeleteTask(int id)
        {
            var item = _store.FirstOrDefault(t => t.Id == id);
            if (item == null) return false;
            return _store.Remove(item);
        }

        public bool SetReminder(int id, DateTime reminderDate)
        {
            var item = _store.FirstOrDefault(t => t.Id == id);
            if (item == null) return false;
            item.ReminderDate = reminderDate;
            return true;
        }
    }
}
