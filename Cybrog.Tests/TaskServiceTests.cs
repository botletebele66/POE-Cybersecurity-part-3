using System;
using System.Linq;
using Cybrog.Engine;
using Xunit;

namespace Cybrog.Tests
{
    public class TaskServiceTests
    {
        [Fact]
        public void AddTask_AddsToObservableCollectionAndDatabase()
        {
            var fakeDb = new FakeDatabaseManager();
            var service = new TaskService(fakeDb);

            var created = service.AddTask("Enable 2FA", "Turn on two-factor auth", null);

            Assert.NotNull(created);
            Assert.Single(service.Tasks);
            Assert.Equal("Enable 2FA", service.Tasks[0].Title);
        }

        [Fact]
        public void AddTask_WhenDatabaseUnavailable_ReturnsNullAndDoesNotAddToCollection()
        {
            var fakeDb = new FakeDatabaseManager { IsAvailable = false, LastError = "Simulated outage" };
            var service = new TaskService(fakeDb);

            var created = service.AddTask("Enable 2FA", string.Empty, null);

            Assert.Null(created);
            Assert.Empty(service.Tasks);
        }

        [Fact]
        public void CompleteTask_MarksItemCompletedInCollection()
        {
            var fakeDb = new FakeDatabaseManager();
            var service = new TaskService(fakeDb);
            var created = service.AddTask("Update password", string.Empty, null)!;

            bool result = service.CompleteTask(created.Id);

            Assert.True(result);
            Assert.True(service.Tasks[0].IsCompleted);
        }

        [Fact]
        public void CompleteTask_NonExistentId_ReturnsFalse()
        {
            var fakeDb = new FakeDatabaseManager();
            var service = new TaskService(fakeDb);

            bool result = service.CompleteTask(9999);

            Assert.False(result);
        }

        [Fact]
        public void ReopenTask_RevertsCompletedTaskToPending()
        {
            var fakeDb = new FakeDatabaseManager();
            var service = new TaskService(fakeDb);
            var created = service.AddTask("Backup files", string.Empty, null)!;
            service.CompleteTask(created.Id);

            bool result = service.ReopenTask(created.Id);

            Assert.True(result);
            Assert.False(service.Tasks[0].IsCompleted);
        }

        [Fact]
        public void DeleteTask_RemovesFromCollection()
        {
            var fakeDb = new FakeDatabaseManager();
            var service = new TaskService(fakeDb);
            var created = service.AddTask("Scan for malware", string.Empty, null)!;

            bool result = service.DeleteTask(created.Id);

            Assert.True(result);
            Assert.Empty(service.Tasks);
        }

        [Fact]
        public void FindByDisplayNumber_ReturnsCorrectPendingTaskByOneBasedPosition()
        {
            // AddTask inserts each new task at the front of the collection (newest-first
            // display order, matching the GUI's task list), so after adding A, B, C in
            // that order, the collection itself reads [C, B, A] — position 2 is "Task B".
            var fakeDb = new FakeDatabaseManager();
            var service = new TaskService(fakeDb);
            service.AddTask("Task A", string.Empty, null);
            service.AddTask("Task B", string.Empty, null);
            service.AddTask("Task C", string.Empty, null);

            Assert.Equal(new[] { "Task C", "Task B", "Task A" }, service.Tasks.Select(t => t.Title));

            var second = service.FindByDisplayNumber(2);

            Assert.NotNull(second);
            Assert.Equal("Task B", second!.Title);
        }

        [Fact]
        public void FindByDisplayNumber_ExcludesCompletedTasksFromNumbering()
        {
            // After adding A then B, collection order is [B, A] (newest-first).
            // Completing A leaves only B pending, so display position 1 now resolves to B.
            var fakeDb = new FakeDatabaseManager();
            var service = new TaskService(fakeDb);
            var a = service.AddTask("Task A", string.Empty, null)!;
            service.AddTask("Task B", string.Empty, null);
            service.CompleteTask(a.Id);

            var first = service.FindByDisplayNumber(1);

            Assert.NotNull(first);
            Assert.Equal("Task B", first!.Title);
        }

        [Fact]
        public void FindByDisplayNumber_OutOfRange_ReturnsNull()
        {
            var fakeDb = new FakeDatabaseManager();
            var service = new TaskService(fakeDb);
            service.AddTask("Only task", string.Empty, null);

            Assert.Null(service.FindByDisplayNumber(5));
            Assert.Null(service.FindByDisplayNumber(0));
            Assert.Null(service.FindByDisplayNumber(-1));
        }

        [Fact]
        public void PendingTasks_And_CompletedTasks_PartitionCorrectly()
        {
            var fakeDb = new FakeDatabaseManager();
            var service = new TaskService(fakeDb);
            var a = service.AddTask("Task A", string.Empty, null)!;
            service.AddTask("Task B", string.Empty, null);
            service.CompleteTask(a.Id);

            Assert.Single(service.PendingTasks);
            Assert.Single(service.CompletedTasks);
            Assert.Equal("Task B", service.PendingTasks[0].Title);
            Assert.Equal("Task A", service.CompletedTasks[0].Title);
        }

        [Fact]
        public void Refresh_ReloadsFromDatabase()
        {
            var fakeDb = new FakeDatabaseManager();
            fakeDb.AddTask("Pre-existing task", string.Empty, null); // added directly to "DB", bypassing TaskService
            var service = new TaskService(fakeDb);

            // TaskService constructor already calls Refresh() once, so this should already be loaded.
            Assert.Single(service.Tasks);
            Assert.Equal("Pre-existing task", service.Tasks[0].Title);
        }
    }
}
