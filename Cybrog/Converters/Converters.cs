using Cybrog.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Cybrog.Converters
{
    /// <summary>Converts a bool to Visibility.Visible/Collapsed. Pass parameter "Invert" to flip the logic.</summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool b = value is bool bv && bv;
            if (parameter as string == "Invert") b = !b;
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException("One-way converter only.");
    }

    /// <summary>True when count == 0 (used to show "no tasks yet" empty states).</summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            int count = value is int i ? i : 0;
            bool showWhenZero = parameter as string == "ZeroVisible";
            bool visible = showWhenZero ? count == 0 : count > 0;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException("One-way converter only.");
    }

    /// <summary>Aligns a chat bubble left (bot/system) or right (user).</summary>
    public class SenderToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is MessageSender sender && sender == MessageSender.User)
                return HorizontalAlignment.Right;
            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException("One-way converter only.");
    }

    /// <summary>Picks the chat bubble background brush based on who sent the message.</summary>
    public class SenderToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            var app = Application.Current;
            return value switch
            {
                MessageSender.User => TryFindResource(app, "BrushYellowSoft", Colors.LightGoldenrodYellow),
                MessageSender.System => TryFindResource(app, "BrushLightGrey", Colors.LightGray),
                _ => TryFindResource(app, "BrushLightGrey", Colors.LightGray)
            };
        }

        private static Brush TryFindResource(Application app, string key, Color fallbackColor)
        {
            if (app.TryFindResource(key) is Brush brush) return brush;
            return new SolidColorBrush(fallbackColor);
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException("One-way converter only.");
    }

    /// <summary>Converts a hex colour string (e.g. task StatusColor) directly to a SolidColorBrush.</summary>
    public class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            {
                try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)); }
                catch { /* fall through */ }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException("One-way converter only.");
    }

    /// <summary>Returns Strikethrough decoration when true (completed tasks), otherwise none.</summary>
    public class BoolToStrikethroughConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
            => (value is bool b && b) ? TextDecorations.Strikethrough : new TextDecorationCollection();

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException("One-way converter only.");
    }

    /// <summary>Inverts a boolean (used for IsEnabled bindings that need the opposite of IsCompleted, etc.).</summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
            => !(value is bool b && b);

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
            => !(value is bool b && b);
    }

    // ---- NEW CONVERTERS for enhanced TaskItem support ----

    /// <summary>Converts a TaskPriority to a foreground brush (e.g., for priority labels).</summary>
    public class PriorityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            var priority = value as TaskPriority? ?? TaskPriority.Medium;
            return priority switch
            {
                TaskPriority.Low => new SolidColorBrush(Colors.Green),
                TaskPriority.Medium => new SolidColorBrush(Colors.Orange),
                TaskPriority.High => new SolidColorBrush(Colors.OrangeRed),
                TaskPriority.Critical => new SolidColorBrush(Colors.Red),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException("One-way converter only.");
    }

    /// <summary>Converts a TaskPriority to a human-readable string (e.g., "High").</summary>
    public class PriorityToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
            => value is TaskPriority p ? p.ToString() : "Medium";

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
            => Enum.TryParse(value as string, out TaskPriority p) ? p : TaskPriority.Medium;
    }

    /// <summary>Converts a DateTime? to a relative time string (e.g., "in 3 days", "2 days ago").</summary>
    public class RelativeDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not DateTime date) return "No date";

            var now = DateTime.Now;
            var diff = date - now;

            if (diff.TotalSeconds < 60)
                return "Just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes}m from now";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours}h from now";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays}d from now";
            if (diff.TotalDays < 30)
                return $"{(int)(diff.TotalDays / 7)} weeks from now";
            if (diff.TotalDays < 365)
                return $"{(int)(diff.TotalDays / 30)} months from now";

            return date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException("One-way converter only.");
    }

    /// <summary>Converts a TaskItem to a tooltip string showing key details.</summary>
    public class TaskToTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is TaskItem task)
                return $"Priority: {task.Priority}\nDue: {task.DueDateLabel}\nCreated: {task.CreatedDate:dd MMM yyyy}";
            return "Task details";
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException("One-way converter only.");
    }

    /// <summary>Converts a TaskItem's completion status to a checkmark symbol or empty string.</summary>
    public class TaskCompleteToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool completed && completed)
                return parameter as string ?? "✔";
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException("One-way converter only.");
    }
}