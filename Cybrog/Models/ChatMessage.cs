using System;

namespace Cybrog.Models
{
    /// <summary>Who authored a chat message — drives bubble alignment and colour in XAML.</summary>
    public enum MessageSender
    {
        User,
        Bot,
        System
    }

    /// <summary>
    /// A single chat bubble bound to the chat ItemsControl.
    /// Kept separate from any persistence model — this is purely a UI/display object.
    /// </summary>
    public class ChatMessage
    {
        public MessageSender Sender { get; init; }
        public string Text { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; } = DateTime.Now;

        public bool IsUser => Sender == MessageSender.User;
        public bool IsBot => Sender == MessageSender.Bot;
        public bool IsSystem => Sender == MessageSender.System;

        public string TimeLabel => Timestamp.ToString("HH:mm");
    }
}
