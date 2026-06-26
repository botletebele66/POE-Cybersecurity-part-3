using System;
using System.Collections.Generic;
using System.Linq;

namespace Cybrog.Models
{
    /// <summary>
    /// In-memory record of what Cybrog has learned about the current user during this
    /// session: their name, the topics they have shown interest in, the last topic
    /// discussed, and a running count of interactions. Drives the "My Memory" panel.
    /// </summary>
    public class UserSession
    {
        public string UserName { get; private set; } = string.Empty;
        public bool HasName => !string.IsNullOrWhiteSpace(UserName);

        public string LastTopic { get; private set; } = string.Empty;
        public string FavouriteTopic => Interests.Count > 0
            ? Interests.GroupBy(i => i).OrderByDescending(g => g.Count()).First().Key
            : string.Empty;

        public List<string> Interests { get; } = new();
        public int TotalInteractions { get; private set; }

        public void SetName(string name) => UserName = name.Trim();

        public void RecordTopic(string topicDisplayName)
        {
            if (string.IsNullOrWhiteSpace(topicDisplayName)) return;
            LastTopic = topicDisplayName;
            Interests.Add(topicDisplayName);
        }

        public void RegisterInteraction() => TotalInteractions++;
    }
}
