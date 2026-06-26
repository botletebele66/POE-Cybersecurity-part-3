using System;
using System.Collections.Generic;
using System.Linq;
using Cybrog.Models;

namespace Cybrog.Engine
{
    /// <summary>
    /// Business-logic layer between the GUI/ConversationEngine and <see cref="DatabaseManager"/>.
    /// Keeps an in-memory cache (<see cref="Tasks"/>) synchronised with the database so the
    /// WPF Tasks tab can bind directly to an ObservableCollection without re-querying SQL on
    /// every keystroke, while every mutating operation still round-trips to the database
    /// immediately so state never drifts between the GUI and persistence layer.
    /// </summary>
    public class TaskService
    {
        private readonly IDatabaseManager _db;

        public System.Collections.ObjectModel.ObservableCollection<TaskItem> Tasks { get; } = new();

        public bool IsDatabaseAvailable => _db.IsAvailable;
        public string? DatabaseError => _db.LastError;

        public TaskService(IDatabaseManager db)
        {
            _db = db;
            Refresh();
        }

        /// <summary>Reloads the in-memory cache from the database (all tasks, completed or not).</summary>
        public void Refresh()
        {
            Tasks.Clear();
            if (!_db.IsAvailable) return;
            foreach (var t in _db.GetTasks(includeCompleted: true))
                Tasks.Add(t);
        }

        /// <summary>
        /// Adds a new task with an optional reminder date and refreshes the cache.
        /// Returns the created <see cref="TaskItem"/>, or null if the database is unavailable.
        /// </summary>
        public TaskItem? AddTask(string title, string description, DateTime? reminderDate)
        {
            if (!_db.IsAvailable) return null;
            int id = _db.AddTask(title, description, reminderDate);
            var item = new TaskItem
            {
                Id = id,
                Title = title,
                Description = description,
                CreatedDate = DateTime.Now,
                ReminderDate = reminderDate,
                IsCompleted = false
            };
            Tasks.Insert(0, item);
            return item;
        }

        /// <summary>Marks a task complete both in the database and the bound collection.</summary>
        public bool CompleteTask(int id)
        {
            if (!_db.IsAvailable) return false;
            bool ok = _db.MarkTaskCompleted(id);
            if (ok)
            {
                var item = Tasks.FirstOrDefault(t => t.Id == id);
                if (item != null) item.IsCompleted = true;
            }
            return ok;
        }

        /// <summary>Re-opens a completed task.</summary>
        public bool ReopenTask(int id)
        {
            if (!_db.IsAvailable) return false;
            bool ok = _db.MarkTaskPending(id);
            if (ok)
            {
                var item = Tasks.FirstOrDefault(t => t.Id == id);
                if (item != null) item.IsCompleted = false;
            }
            return ok;
        }

        /// <summary>Deletes a task from the database and the bound collection.</summary>
        public bool DeleteTask(int id)
        {
            if (!_db.IsAvailable) return false;
            bool ok = _db.DeleteTask(id);
            if (ok)
            {
                var item = Tasks.FirstOrDefault(t => t.Id == id);
                if (item != null) Tasks.Remove(item);
            }
            return ok;
        }

        /// <summary>Finds a task by its 1-based display position in the currently pending list (used by chat/NLP commands like "delete task 2").</summary>
        public TaskItem? FindByDisplayNumber(int number)
        {
            var pending = Tasks.Where(t => !t.IsCompleted).ToList();
            return number >= 1 && number <= pending.Count ? pending[number - 1] : null;
        }

        /// <summary>Finds a task by its real database ID.</summary>
        public TaskItem? FindById(int id) => Tasks.FirstOrDefault(t => t.Id == id);

        public List<TaskItem> PendingTasks => Tasks.Where(t => !t.IsCompleted).ToList();
        public List<TaskItem> CompletedTasks => Tasks.Where(t => t.IsCompleted).ToList();
    }
}
