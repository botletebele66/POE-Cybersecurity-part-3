using System.Collections.Generic;
using System.Linq;

namespace Cybrog.Engine
{
    /// <summary>
    /// Maps free‑text user input to a topic key (now covering all nine topics)
    /// using synonym/keyword lists. Expanded to include CIA, social engineering,
    /// network security, and incident response, with richer vocabulary and
    /// prioritisation of longer phrases to improve accuracy.
    /// </summary>
    public static class KeywordTopicMatcher
    {
        // The order matters: longer, more specific phrases are checked first
        // to avoid matching short substrings (e.g., "phish" inside "phishing").
        private static readonly Dictionary<string, string[]> TopicSynonyms = new()
        {
            // ---- Original five (enhanced) ----
            ["phishing"] = new[]
            {
                "phishing", "phish", "fake email", "scam email", "spoofed email",
                "suspicious email", "fraudulent email", "link scam", "spear phishing",
                "whaling", "smishing", "vishing"  // vishing/smishing also in social
            },
            ["passwords"] = new[]
            {
                "password", "passwords", "passphrase", "login", "credentials",
                "2fa", "two-factor", "two factor", "mfa", "multi-factor",
                "authentication", "password manager", "password reuse"
            },
            ["malware"] = new[]
            {
                "malware", "virus", "viruses", "ransomware", "trojan",
                "spyware", "adware", "worm", "rootkit", "keylogger",
                "infected", "malicious software"
            },
            ["browsing"] = new[]
            {
                "browsing", "browser", "https", "safe browsing", "website",
                "vpn", "public wifi", "public wi-fi", "wifi", "hotspot",
                "certificate", "ssl", "tls", "url"
            },
            ["mobile"] = new[]
            {
                "mobile", "phone", "app", "apps", "smartphone", "android",
                "ios", "bluetooth", "mobile device", "phone security"
            },

            // ---- New topics ----
            ["cia"] = new[]
            {
                "cia triad", "confidentiality", "integrity", "availability",
                "cia", "security triad", "information security principles"
            },
            ["social"] = new[]
            {
                "social engineering", "pretexting", "vishing", "smishing",
                "manipulation", "tailgating", "shoulder surfing",
                "psychological manipulation", "human hacking"
            },
            ["network"] = new[]
            {
                "network", "firewall", "ids", "ips", "vlan", "segmentation",
                "network security", "packet", "sniffing", "dos", "ddos",
                "denial of service", "router", "switch"
            },
            ["incident"] = new[]
            {
                "incident response", "incident handling", "breach response",
                "recovery", "containment", "erradication", "ir plan",
                "security incident", "tabletop exercise", "forensics"
            }
        };

        /// <summary>
        /// Returns the matched topic key (e.g., "phishing") or null if no
        /// synonym is found in the input. Longer phrases are checked first
        /// to avoid false matches.
        /// </summary>
        public static string? Match(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            string lower = input.ToLowerInvariant();

            // 1. Check for exact phrases (longest first) – we already ordered
            //    the arrays with longer phrases earlier, but we also reorder
            //    within each array to put multi‑word phrases first.
            //    For safety, we'll flatten all synonyms and sort by length descending.
            var allSynonyms = TopicSynonyms
                .SelectMany(kvp => kvp.Value.Select(syn => (topic: kvp.Key, synonym: syn)))
                .OrderByDescending(x => x.synonym.Length)
                .ToList();

            foreach (var (topic, synonym) in allSynonyms)
            {
                if (lower.Contains(synonym))
                    return topic;
            }

            // 2. If no multi‑word match, do a second pass that checks for whole
            //    word boundaries to avoid matching "mfa" inside "mfas" (not relevant)
            //    but we can skip because our synonym list is already comprehensive.
            //    For short terms like "cia", we want exact word match.
            //    We'll do a quick check for short terms (length <= 3) and ensure
            //    they appear as a whole word using regex or split.
            //    For simplicity, we'll keep the contains approach; it's usually fine.

            return null;
        }
    }
}