using System.ComponentModel;

namespace Cybrog.Models
{
    /// <summary>Task priority levels for visual sorting and filtering.</summary>
    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Represents a single cybersecurity task stored in the database.
    /// Implements <see cref="INotifyPropertyChanged"/> so the WPF Tasks tab
    /// updates immediately when a task changes, without a manual refresh.
    /// Extended with priority, due date, category, completion timestamp,
    /// and better status reporting.
    /// </summary>
    public class TaskItem : INotifyPropertyChanged, IEquatable<TaskItem>
    {
        private string _title = string.Empty;
        private string _description = string.Empty;
        private bool _isCompleted;
        private DateTime? _completionDate;
        private TaskPriority _priority = TaskPriority.Medium;
        private string _category = string.Empty;
        private DateTime? _dueDate;
        private DateTime? _reminderDate;

        // ---- Core fields ----
        public int Id { get; set; }

        public string Title
        {
            get => _title;
            set
            {
                if (_title == value) return;
                _title = value?.Trim() ?? string.Empty;
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(HasTitle));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description == value) return;
                _description = value?.Trim() ?? string.Empty;
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(HasDescription));
            }
        }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted == value) return;
                _isCompleted = value;
                CompletionDate = value ? DateTime.Now : (DateTime?)null;
                OnPropertyChanged(nameof(IsCompleted));
                OnPropertyChanged(nameof(StatusLabel));
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(IsOverdue));
                OnPropertyChanged(nameof(DaysUntilDue));
            }
        }

        public DateTime? CompletionDate
        {
            get => _completionDate;
            private set
            {
                if (_completionDate == value) return;
                _completionDate = value;
                OnPropertyChanged(nameof(CompletionDate));
            }
        }

        // ---- Extended properties ----
        public TaskPriority Priority
        {
            get => _priority;
            set
            {
                if (_priority == value) return;
                _priority = value;
                OnPropertyChanged(nameof(Priority));
                OnPropertyChanged(nameof(PriorityLabel));
                OnPropertyChanged(nameof(PriorityColor));
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                if (_category == value) return;
                _category = value?.Trim() ?? string.Empty;
                OnPropertyChanged(nameof(Category));
                OnPropertyChanged(nameof(HasCategory));
            }
        }

        /// <summary>
        /// The date by which the task should be completed (deadline).
        /// Distinct from <see cref="ReminderDate"/> (which is a notification time).
        /// </summary>
        public DateTime? DueDate
        {
            get => _dueDate;
            set
            {
                if (_dueDate == value) return;
                _dueDate = value;
                OnPropertyChanged(nameof(DueDate));
                OnPropertyChanged(nameof(DueDateLabel));
                OnPropertyChanged(nameof(IsOverdue));
                OnPropertyChanged(nameof(DaysUntilDue));
                OnPropertyChanged(nameof(StatusLabel));
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        /// <summary>
        /// Optional reminder date – when the user wants to be notified.
        /// Separate from <see cref="DueDate"/>.
        /// </summary>
        public DateTime? ReminderDate
        {
            get => _reminderDate;
            set
            {
                if (_reminderDate == value) return;
                _reminderDate = value;
                OnPropertyChanged(nameof(ReminderDate));
                OnPropertyChanged(nameof(ReminderLabel));
            }
        }

        // ---- Computed display properties ----
        public bool HasTitle => !string.IsNullOrWhiteSpace(Title);
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
        public bool HasCategory => !string.IsNullOrWhiteSpace(Category);

        public string PriorityLabel => Priority.ToString();
        public string PriorityColor => Priority switch
        {
            TaskPriority.Low => "#4CAF50",      // green
            TaskPriority.Medium => "#FFC107",    // amber
            TaskPriority.High => "#FF5722",      // deep orange
            TaskPriority.Critical => "#D32F2F",  // red
            _ => "#9E9E9E"                       // grey
        };

        public string DueDateLabel => DueDate.HasValue
            ? DueDate.Value.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture)
            : "No deadline";

        public string ReminderLabel => ReminderDate.HasValue
            ? ReminderDate.Value.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture)
            : "No reminder";

        /// <summary>True if the task is not completed and has a due date that has passed.</summary>
        public bool IsOverdue => !IsCompleted && DueDate.HasValue && DueDate.Value.Date <= DateTime.Today;

        /// <summary>Days until due date (negative if overdue). Null if no due date.</summary>
        public int? DaysUntilDue => DueDate.HasValue ? (int?)(DueDate.Value.Date - DateTime.Today).Days : null;

        public string StatusLabel => IsCompleted ? "Completed"
            : IsOverdue ? "Overdue"
            : DueDate.HasValue ? "Due"
            : "Pending";

        public string StatusColor => IsCompleted ? "#2E7D32"   // dark green
            : IsOverdue ? "#B45309"   // orange-brown
            : DueDate.HasValue ? "#8A6D00"   // dark amber
            : "#6A6A6A";              // grey

        // ---- Constructors ----
        public TaskItem() { }

        public TaskItem(string title, string description = "", TaskPriority priority = TaskPriority.Medium,
            string category = "", DateTime? dueDate = null, DateTime? reminderDate = null)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? string.Empty;
            Priority = priority;
            Category = category ?? string.Empty;
            DueDate = dueDate;
            ReminderDate = reminderDate;
            CreatedDate = DateTime.Now;
        }

        // ---- Equality ----
        public bool Equals(TaskItem? other) => other != null && Id == other.Id;
        public override bool Equals(object? obj) => Equals(obj as TaskItem);
        public override int GetHashCode() => Id.GetHashCode();

        // ---- Helpers ----
        /// <summary>Returns a short summary for UI lists or notifications.</summary>
        public string GetSummary() =>
            $"{Title} ({(IsCompleted ? "Done" : (IsOverdue ? "Overdue" : "Pending"))})";

        /// <summary>Returns a detailed description for logging or export.</summary>
        public string GetDetails() =>
            $"ID: {Id}, Title: {Title}, Priority: {Priority}, Category: {Category}, " +
            $"Created: {CreatedDate:yyyy-MM-dd}, Due: {DueDateLabel}, Completed: {CompletionDate?.ToString("yyyy-MM-dd") ?? "No"}";

        // ---- INotifyPropertyChanged ----
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}