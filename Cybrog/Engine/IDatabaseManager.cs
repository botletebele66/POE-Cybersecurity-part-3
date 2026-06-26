using System;
using System.Collections.Generic;
using Cybrog.Models;

namespace Cybrog.Engine
{
    /// <summary>
    /// Abstraction over task persistence. Extracted as an interface (rather than using
    /// the concrete <see cref="DatabaseManager"/> directly) so that <see cref="TaskService"/>
    /// can be unit-tested with an in-memory fake instead of requiring a real SQL Server
    /// LocalDB connection in the test environment — and so a different storage backend
    /// could be substituted in future without touching any business logic above this layer.
    /// This is the dependency-inversion piece of the OOP design: TaskService depends on
    /// this abstraction, not on the concrete SQL Server implementation.
    /// </summary>
    public interface IDatabaseManager
    {
        bool IsAvailable { get; }
        string? LastError { get; }

        int AddTask(string title, string? description = null, DateTime? reminderDate = null);
        List<TaskItem> GetTasks(bool includeCompleted = true);
        bool MarkTaskCompleted(int id);
        bool MarkTaskPending(int id);
        bool DeleteTask(int id);
        bool SetReminder(int id, DateTime reminderDate);
    }
}
