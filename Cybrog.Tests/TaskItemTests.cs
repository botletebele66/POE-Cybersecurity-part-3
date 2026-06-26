using System;
using System.ComponentModel;
using Cybrog.Models;
using Xunit;

namespace Cybrog.Tests
{
    public class TaskItemTests
    {
        [Fact]
        public void IsOverdue_PendingTaskWithPastReminder_IsTrue()
        {
            var task = new TaskItem
            {
                Title = "Old task",
                IsCompleted = false,
                ReminderDate = DateTime.Today.AddDays(-2)
            };

            Assert.True(task.IsOverdue);
            Assert.Equal("Due", task.StatusLabel);
        }

        [Fact]
        public void IsOverdue_PendingTaskWithFutureReminder_IsFalse()
        {
            var task = new TaskItem
            {
                Title = "Future task",
                IsCompleted = false,
                ReminderDate = DateTime.Today.AddDays(5)
            };

            Assert.False(task.IsOverdue);
            Assert.Equal("Pending", task.StatusLabel);
        }

        [Fact]
        public void IsOverdue_CompletedTaskWithPastReminder_IsFalse()
        {
            // A completed task is never "overdue" even if its old reminder date has passed —
            // overdue only applies to tasks still requiring action.
            var task = new TaskItem
            {
                Title = "Done task",
                IsCompleted = true,
                ReminderDate = DateTime.Today.AddDays(-10)
            };

            Assert.False(task.IsOverdue);
            Assert.Equal("Completed", task.StatusLabel);
        }

        [Fact]
        public void IsOverdue_NoReminderSet_IsFalse()
        {
            var task = new TaskItem { Title = "No reminder", IsCompleted = false, ReminderDate = null };
            Assert.False(task.IsOverdue);
        }

        [Fact]
        public void HasDescription_NonEmptyDescription_IsTrue()
        {
            var task = new TaskItem { Description = "Review account privacy settings" };
            Assert.True(task.HasDescription);
        }

        [Fact]
        public void HasDescription_EmptyOrWhitespaceDescription_IsFalse()
        {
            Assert.False(new TaskItem { Description = "" }.HasDescription);
            Assert.False(new TaskItem { Description = "   " }.HasDescription);
        }

        [Fact]
        public void ReminderLabel_WithDate_FormatsCorrectly()
        {
            var task = new TaskItem { ReminderDate = new DateTime(2026, 5, 30) };
            Assert.Equal("Remind: 30 May 2026", task.ReminderLabel);
        }

        [Fact]
        public void ReminderLabel_NoDate_ShowsNoReminderSet()
        {
            var task = new TaskItem { ReminderDate = null };
            Assert.Equal("No reminder set", task.ReminderLabel);
        }

        [Fact]
        public void IsCompleted_Setter_RaisesPropertyChangedForDependentProperties()
        {
            var task = new TaskItem { Title = "Test", IsCompleted = false };
            var raisedProperties = new System.Collections.Generic.List<string>();
            ((INotifyPropertyChanged)task).PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            task.IsCompleted = true;

            Assert.Contains(nameof(TaskItem.IsCompleted), raisedProperties);
            Assert.Contains(nameof(TaskItem.StatusLabel), raisedProperties);
            Assert.Contains(nameof(TaskItem.StatusColor), raisedProperties);
        }

        [Fact]
        public void IsCompleted_SettingSameValue_DoesNotRaisePropertyChanged()
        {
            var task = new TaskItem { Title = "Test", IsCompleted = false };
            int raiseCount = 0;
            ((INotifyPropertyChanged)task).PropertyChanged += (_, _) => raiseCount++;

            task.IsCompleted = false; // already false — no-op

            Assert.Equal(0, raiseCount);
        }
    }
}
