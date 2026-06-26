using System.Collections.Generic;
using Cybrog.Models;

namespace Cybrog.Engine
{
    public static class SecurityKnowledgeBase
    {
        public static readonly IReadOnlyDictionary<string, SecurityTopic> Topics = new Dictionary<string, SecurityTopic>
        {
            ["phishing"] = new SecurityTopic(
                key: "phishing",
                displayName: "Phishing Attacks",
                icon: "🎣",
                scenario: "You receive an email that looks like it's from your bank: 'Your account has been suspended — click here to verify your identity within 24 hours.'",
                technique: "The link leads to a near-perfect copy of your bank's login page. The moment you type your username and password, the attacker captures them in real time.",
                outcome: "Within minutes, the attacker logs into your real account, changes the recovery email, and begins transferring funds before you even realise anything is wrong.",
                mitigation: "Never click links in unsolicited messages. Instead, open your browser and type the bank's address yourself, or call the number printed on your card. Check the sender's actual email address, not just the display name — phishing emails often use lookalike domains.",
                practice: "Hover over (don't click) any link to preview the real destination URL before deciding whether to proceed. If the domain doesn't exactly match the organisation's real one, treat it as hostile.",
                sources: new[] { "Stallings, W. (2022). Cybersecurity: Principles and Practice (4th ed.). Pearson", "OWASP (2021). Phishing Prevention." },
                category: "Attack",
                difficulty: TopicDifficulty.Intermediate,
                tags: new[] { "phishing", "email", "social" }
            ),

            ["passwords"] = new SecurityTopic(
                key: "passwords",
                displayName: "Password Security",
                icon: "🔑",
                scenario: "You use the same memorable password — your pet's name plus '123' — across email, banking, and social media, because it's easy to remember.",
                technique: "One of those sites suffers a data breach. Attackers don't even need to guess: they take the leaked password and try it against every other major service automatically, a technique called credential stuffing.",
                outcome: "Because the password was reused, attackers gain access to your email too — and from there, they can reset the passwords on almost every other account you own.",
                mitigation: "Use a unique, long, random password for every account, generated and stored by a reputable password manager. Turn on Two-Factor Authentication (2FA) wherever it's offered, so a leaked password alone isn't enough to break in.",
                practice: "Pick one important account today (start with email) and confirm 2FA is switched on. A password manager removes the need to ever memorise a weak, reused password again.",
                sources: new[] { "Stallings, W. (2022). Cybersecurity: Principles and Practice (4th ed.). Pearson", "NIST SP 800-63B (2020). Digital Identity Guidelines." },
                category: "Defense",
                difficulty: TopicDifficulty.Beginner,
                tags: new[] { "passwords", "authentication", "2FA" }
            ),

            ["malware"] = new SecurityTopic(
                key: "malware",
                displayName: "Malware Protection",
                icon: "🛡️",
                scenario: "You download a 'free' version of paid software from an unofficial site, or plug in a USB stick someone handed you at a conference.",
                technique: "The file silently installs ransomware, which encrypts every document, photo, and spreadsheet on your machine, then displays a payment demand to unlock them.",
                outcome: "Without a backup, your files are gone unless you pay — and even paying offers no real guarantee the attacker will provide a working decryption key.",
                mitigation: "Only install software from official sources (developer websites, official app stores). Keep your operating system and antivirus software fully updated, since most malware exploits already-patched vulnerabilities. Keep regular offline backups so a ransomware infection can never hold your data hostage.",
                practice: "Schedule an automatic weekly backup of your most important folders to an external drive or cloud service that isn't permanently connected to your computer.",
                sources: new[] { "Stallings, W. (2022). Cybersecurity: Principles and Practice (4th ed.). Pearson", "ENISA (2022). Ransomware Protection." },
                category: "Attack",
                difficulty: TopicDifficulty.Intermediate,
                tags: new[] { "malware", "ransomware", "backup" }
            ),

            ["browsing"] = new SecurityTopic(
                key: "browsing",
                displayName: "Safe Browsing",
                icon: "🌐",
                scenario: "You connect to free Wi-Fi at a coffee shop and log into your online banking app to quickly check a balance.",
                technique: "An attacker on the same network runs a packet sniffer, or has set up a fake hotspot with a similar name ('Free_Coffee_WiFi') that routes your traffic through their own machine.",
                outcome: "Unencrypted parts of your session — or, on a malicious hotspot, even encrypted traffic via interception techniques — can expose login tokens or session data to the attacker.",
                mitigation: "Avoid sensitive transactions (banking, shopping with saved cards) on public Wi-Fi. Use a reputable VPN if you must use public networks. Always confirm HTTPS (padlock icon) is present, but remember it confirms encryption only — not that the destination is trustworthy.",
                practice: "Before connecting to any public network, verify the exact network name with staff, and avoid logging into financial accounts until you're back on a trusted connection.",
                sources: new[] { "Stallings, W. (2022). Cybersecurity: Principles and Practice (4th ed.). Pearson", "NIST SP 800-153 (2019). Guidelines for Securing Wireless Networks." },
                category: "Defense",
                difficulty: TopicDifficulty.Beginner,
                tags: new[] { "browsing", "WiFi", "HTTPS" }
            ),

            ["mobile"] = new SecurityTopic(
                key: "mobile",
                displayName: "Mobile Security",
                icon: "📱",
                scenario: "You install a flashlight or wallpaper app from a third‑party app store because it's not available on the official store.",
                technique: "The app requests excessive permissions — contacts, SMS, location, microphone — that have nothing to do with its stated purpose, and quietly harvests your data in the background.",
                outcome: "Your contacts, messages, and location history are exfiltrated and may be sold or used for further targeted phishing (now with personal details that make scams far more convincing).",
                mitigation: "Only install apps from official stores (Google Play, Apple App Store), and review the permissions an app requests before installing — a flashlight app never needs access to your contacts. Keep your phone's OS updated and enable a strong screen lock plus remote‑wipe in case the device is lost or stolen.",
                practice: "Open your phone's app permissions settings today and revoke any permission that doesn't make sense for what the app actually does.",
                sources: new[] { "Stallings, W. (2022). Cybersecurity: Principles and Practice (4th ed.). Pearson", "OWASP Mobile Security Project." },
                category: "Defense",
                difficulty: TopicDifficulty.Beginner,
                tags: new[] { "mobile", "apps", "permissions" }
            ),

            ["cia"] = new SecurityTopic(
                key: "cia",
                displayName: "CIA Triad",
                icon: "🔒",
                scenario: "A hospital’s patient database must always be available to doctors, accurate for treatment, and kept confidential from outsiders. An attacker tries to breach the system to steal or alter patient records.",
                technique: "The adversary uses a SQL injection vulnerability to access the database. They download thousands of records (breaching confidentiality) and also modify medication doses (violating integrity), then launch a ransomware attack that locks the database, rendering it unavailable until a ransom is paid.",
                outcome: "The hospital cannot access patient data for hours, leading to delayed treatments; many records are corrupted, causing misdiagnoses; and the data breach exposes sensitive health information, resulting in regulatory fines and loss of patient trust.",
                mitigation: "Implement encryption (for confidentiality), cryptographic hashing and audit logs (for integrity), and redundant systems with regular backups (for availability). Apply the principle of least privilege and conduct regular vulnerability assessments to prevent initial exploitation.",
                practice: "Review your organisation's backup strategy today: ensure backups are tested, offline, and encrypted. Enable integrity‑checking on critical files and restrict database access to only necessary personnel.",
                sources: new[] { "Stallings, W. (2022). Cybersecurity: Principles and Practice (4th ed.). Pearson", "NIST SP 800-53 (2020) Security and Privacy Controls", "ISO/IEC 27001:2022." },
                category: "Principle",
                difficulty: TopicDifficulty.Intermediate,
                tags: new[] { "CIA", "confidentiality", "integrity", "availability" }
            ),

            ["social"] = new SecurityTopic(
                key: "social",
                displayName: "Social Engineering",
                icon: "🎭",
                scenario: "You receive a phone call from someone claiming to be from your company's IT support. They say they've detected a virus on your machine and need your password to fix it immediately.",
                technique: "The attacker uses a combination of vishing (voice phishing) and pretexting – they already know your name, department, and manager's name from publicly available sources (e.g., LinkedIn). They sound authoritative and urgent, pressuring you to comply.",
                outcome: "You give them your password. They now have legitimate credentials to access the corporate network, potentially stealing sensitive data or installing backdoors. The breach goes undetected for weeks.",
                mitigation: "Never share passwords or sensitive information over the phone, especially from unsolicited calls. Verify the caller's identity by hanging up and calling back the official IT helpdesk number. Implement a culture of 'challenge and verify' – always confirm the legitimacy of requests.",
                practice: "Create a policy that all IT staff must use a unique employee verification code when calling. Practice mock phishing/vishing drills to raise awareness.",
                sources: new[] { "Stallings, W. (2022). Cybersecurity: Principles and Practice (4th ed.). Pearson", "Hadnagy, C. (2018). Social Engineering: The Science of Human Hacking", "NIST SP 800-115 (2010) Technical Guide to Information Security Testing." },
                category: "Attack",
                difficulty: TopicDifficulty.Intermediate,
                tags: new[] { "social engineering", "vishing", "pretexting" }
            ),

            ["network"] = new SecurityTopic(
                key: "network",
                displayName: "Network Security",
                icon: "🌍",
                scenario: "Your organisation’s network is connected to the internet without proper segmentation. An employee accidentally clicks a malicious link in an email, which downloads a worm that spreads laterally across the internal network.",
                technique: "The worm exploits unpatched vulnerabilities in older operating systems and uses default credentials on network devices. It performs reconnaissance, maps the network, and opens a backdoor for the attacker to exfiltrate data.",
                outcome: "Sensitive financial records are stolen. The breach is discovered weeks later during a routine audit. The company suffers reputation damage, legal liability, and loss of intellectual property.",
                mitigation: "Implement a defence‑in‑depth strategy: firewalls, intrusion detection/prevention systems (IDS/IPS), network segmentation (VLANs), and strict access controls. Keep all devices patched and change default credentials. Use encryption for data in transit (TLS/IPsec).",
                practice: "Conduct a network scan to identify all connected devices and their firmware versions. Segment your network so that critical assets are isolated from general employee workstations. Enable logging and monitor for unusual traffic patterns.",
                sources: new[] { "Stallings, W. (2022). Cybersecurity: Principles and Practice (4th ed.). Pearson", "NIST SP 800-41 (2019) Guidelines on Firewalls and Firewall Policy", "CIS Controls (v8)." },
                category: "Defense",
                difficulty: TopicDifficulty.Intermediate,
                tags: new[] { "network", "firewall", "segmentation" }
            ),

            ["incident"] = new SecurityTopic(
                key: "incident",
                displayName: "Incident Response",
                icon: "🚨",
                scenario: "Your IT team receives alerts from your intrusion detection system (IDS) about unusual outbound traffic during off‑hours. A user also reports that their files have been encrypted with a ransom note.",
                technique: "The attacker gained initial access through a phishing email that installed a remote access trojan (RAT). They have been quietly escalating privileges and moving laterally for three days before deploying ransomware.",
                outcome: "Critical business systems are down, and customer data may have been exfiltrated. The company faces operational disruption, regulatory fines (GDPR/CCPA), and loss of customer trust.",
                mitigation: "Have a documented Incident Response Plan (IRP) aligned with NIST SP 800‑61, including clear roles, communication channels, and step‑by‑step procedures for detection, containment, eradication, and recovery. Conduct regular tabletop exercises and maintain an up‑to‑date asset inventory.",
                practice: "Review your current IRP and test it with a simulated ransomware scenario. Ensure backups are restored quickly and that you have a copy of your recovery procedure stored offline.",
                sources: new[] { "Stallings, W. (2022). Cybersecurity: Principles and Practice (4th ed.). Pearson", "NIST SP 800-61 Rev.2 (2012) Computer Security Incident Handling Guide", "SANS Incident Response Framework." },
                category: "Defense",
                difficulty: TopicDifficulty.Advanced,
                tags: new[] { "incident response", "IRP", "forensics" }
            )
        };

        public static SecurityTopic? Get(string key) =>
            Topics.TryGetValue(key.ToLowerInvariant(), out var topic) ? topic : null;

        public static IEnumerable<SecurityTopic> All => Topics.Values;
    }
}