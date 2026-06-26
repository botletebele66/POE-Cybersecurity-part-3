namespace Cybrog.Engine
{
    /// <summary>
    /// Emotional tone of the user's message. Used to adapt Cybrog's tone and
    /// offer appropriate reassurance or encouragement.
    /// </summary>
    public enum Sentiment
    {
        Worried,
        Frustrated,
        Annoyed,      // more intense than frustrated
        Curious,
        Confused,
        Positive,
        Neutral
    }

    /// <summary>
    /// Detects the emotional tone of user input via keyword matching, and
    /// supplies empathetic openers so Cybrog's tone adapts to the user's state.
    /// Extended from Part 2 with more keywords, additional sentiments, and
    /// random variation to keep responses fresh.
    /// </summary>
    public static class SentimentAnalyzer
    {
        private static readonly Random Rand = new();

        // ---- Keyword lists for each sentiment ----
        private static readonly Dictionary<Sentiment, string[]> Keywords = new()
        {
            [Sentiment.Worried] = new[]
            {
                "scared", "worried", "afraid", "anxious", "panic", "nervous",
                "terrified", "frightened", "uneasy", "concerned", "alarmed",
                "help me", "hacked", "stolen", "compromised", "victim",
                "breach", "exposed", "leaked", "ransom", "lost", "can't access"
            },

            [Sentiment.Frustrated] = new[]
            {
                "frustrated", "annoying", "stupid", "useless", "sick of",
                "fed up", "tired of", "why won't", "doesn't work", "not working",
                "pointless", "waste of time", "confusing", "difficult", "hard"
            },

            [Sentiment.Annoyed] = new[]
            {
                "angry", "pissed", "mad", "furious", "irritated", "hate",
                "ridiculous", "absurd", "outrageous", "unbelievable", "appalling"
            },

            [Sentiment.Curious] = new[]
            {
                "curious", "wondering", "interested", "tell me more", "why",
                "how does", "what about", "explain", "clarify", "elaborate",
                "curiosity", "intrigued", "fascinated"
            },

            [Sentiment.Confused] = new[]
            {
                "confused", "don't understand", "not clear", "unclear",
                "i don't get", "what do you mean", "lost", "overwhelmed",
                "complex", "perplexed", "baffled", "mixed up"
            },

            [Sentiment.Positive] = new[]
            {
                "thanks", "thank you", "great", "awesome", "cool", "nice",
                "love this", "helpful", "amazing", "fantastic", "brilliant",
                "good", "excellent", "super", "useful", "appreciate"
            }
        };

        /// <summary>
        /// Detects the most likely sentiment from the given input text.
        /// Returns <see cref="Sentiment.Neutral"/> if no keywords match.
        /// </summary>
        public static Sentiment Detect(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Sentiment.Neutral;

            string lower = input.ToLowerInvariant();
            foreach (var (sentiment, words) in Keywords)
            {
                if (words.Any(w => lower.Contains(w)))
                    return sentiment;
            }
            return Sentiment.Neutral;
        }

        // ---- Empathy openers (multiple variants per sentiment) ----
        private static readonly Dictionary<Sentiment, string[]> EmpathyOpeners = new()
        {
            [Sentiment.Worried] = new[]
            {
                "Take a breath — you're in the right place, and we can sort this out together. ",
                "I understand that's worrying — let's tackle this step by step. ",
                "It's okay to be concerned — I'm here to help you through it. ",
                "I hear your concern. Let's work through this calmly. ",
                "Don't panic — many people face this, and I'll guide you. "
            },

            [Sentiment.Frustrated] = new[]
            {
                "I hear you, this stuff can be genuinely annoying. Let's make it simple. ",
                "I get your frustration — let's cut through the noise. ",
                "That does sound irritating. Let me break it down clearly. ",
                "I know it's frustrating — but together we'll get it sorted. "
            },

            [Sentiment.Annoyed] = new[]
            {
                "I can see you're really annoyed — and rightly so. Let's fix this. ",
                "That level of annoyance is totally understandable — I'm on it. ",
                "I feel your anger — let's channel that into solving the problem. ",
                "You're absolutely right to be upset — let's get to the bottom of it. "
            },

            [Sentiment.Curious] = new[]
            {
                "Great question! ",
                "I love that curiosity! ",
                "That's a brilliant thing to wonder about. ",
                "Excellent inquiry — let's dive in. ",
                "You're asking the right questions. "
            },

            [Sentiment.Confused] = new[]
            {
                "Let me clear that up for you. ",
                "I get why that might be confusing — let me simplify it. ",
                "No worries — it's a bit complex, but I'll make it easy. ",
                "I'll rephrase that so it's crystal clear. ",
                "Confusion is normal here — I'll walk you through it. "
            },

            [Sentiment.Positive] = new[]
            {
                "Glad that helped! ",
                "Thank you! I'm happy to assist. ",
                "That's wonderful to hear! ",
                "You're very welcome — I'm here whenever you need. ",
                "I appreciate the kind words! "
            }
        };

        /// <summary>
        /// Returns a random empathy‑friendly opener for the given sentiment,
        /// or null if the sentiment is <see cref="Sentiment.Neutral"/>.
        /// </summary>
        public static string? GetEmpathyOpener(Sentiment sentiment)
        {
            if (sentiment == Sentiment.Neutral)
                return null;

            if (EmpathyOpeners.TryGetValue(sentiment, out var openers))
                return openers[Rand.Next(openers.Length)];

            return null;
        }

        /// <summary>
        /// Combines a sentiment‑aware empathy opener with a default helpful
        /// phrase that encourages the user to continue or ask a specific thing.
        /// Falls back to a generic friendly opening for neutral input.
        /// </summary>
        public static string GetPersonalisedResponse(string input)
        {
            var sentiment = Detect(input);
            var opener = GetEmpathyOpener(sentiment);

            // Neutral fallback – we still want to be friendly
            if (opener == null)
            {
                var neutralOpeners = new[]
                {
                    "Got it. ",
                    "Okay, ",
                    "Sure, ",
                    "Alright, "
                };
                opener = neutralOpeners[Rand.Next(neutralOpeners.Length)];
            }

            // Add a gentle follow‑up based on sentiment
            string followUp = sentiment switch
            {
                Sentiment.Worried => "Let's take it slowly — what would you like to focus on first?",
                Sentiment.Frustrated => "Let's strip it down to the essentials — what's the core issue?",
                Sentiment.Annoyed => "I'm going to give you straight, no‑nonsense answers — just ask.",
                Sentiment.Curious => "What aspect would you like to explore in more depth?",
                Sentiment.Confused => "I'll explain it again in simpler terms — just tell me where to start.",
                Sentiment.Positive => "Let me know if you have any other questions — I'm all ears.",
                _ => "How can I help you further?"
            };

            return opener + followUp;
        }
    }
}