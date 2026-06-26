using Cybrog.Models;

namespace Cybrog.Engine
{
    public class QuizEngine
    {
        private readonly List<QuizQuestion> _bank;
        private readonly Random _rand = new();

        private const int QuestionsPerQuiz = 15;

        private Queue<QuizQuestion> _remaining = new();
        private QuizQuestion? _current;

        public bool IsActive { get; private set; }
        public int TotalQuestions { get; private set; }
        public int CurrentQuestionNumber { get; private set; }
        public int Score { get; private set; }
        public string LastExplanation { get; private set; } = string.Empty;
        public QuizQuestion? CurrentQuestion => _current;

        public QuizEngine() => _bank = BuildBank();

        public void Start()
        {
            var shuffled = _bank.OrderBy(_ => _rand.Next()).Take(QuestionsPerQuiz).ToList();
            _remaining = new Queue<QuizQuestion>(shuffled);
            TotalQuestions = _remaining.Count;
            Score = 0;
            CurrentQuestionNumber = 0;
            IsActive = true;
            _current = null;
            LastExplanation = string.Empty;
        }

        public QuizQuestion? GetNextQuestion()
        {
            if (!IsActive || _remaining.Count == 0)
            {
                IsActive = false;
                _current = null;
                return null;
            }
            _current = _remaining.Dequeue();
            CurrentQuestionNumber++;
            return _current;
        }

        public bool SubmitAnswer(int selectedIndex)
        {
            if (_current == null)
                throw new InvalidOperationException("SubmitAnswer called with no active question. Call GetNextQuestion first.");

            bool correct = selectedIndex == _current.CorrectIndex;
            if (correct) Score++;

            LastExplanation = correct
                ? _current.Explanation
                : $"Correct answer: {_current.Options[_current.CorrectIndex]}. {_current.Explanation}";

            if (_remaining.Count == 0) IsActive = false;
            return correct;
        }

        public (int Score, int Total, string Message) GetFinalResult()
        {
            int pct = TotalQuestions > 0 ? Score * 100 / TotalQuestions : 0;
            string msg = pct switch
            {
                >= 90 => "Outstanding! You're a cybersecurity pro!",
                >= 70 => "Great job! You have strong security knowledge.",
                >= 50 => "Good effort — a little more practice and you'll be unstoppable.",
                _ => "Keep learning to stay safe online! Every expert started somewhere."
            };
            return (Score, TotalQuestions, msg);
        }

        public void Reset()
        {
            IsActive = false;
            _remaining = new Queue<QuizQuestion>();
            TotalQuestions = 0;
            Score = 0;
            CurrentQuestionNumber = 0;
            _current = null;
            LastExplanation = string.Empty;
        }

        // ---- Question bank: 22 questions ----
        private static List<QuizQuestion> BuildBank() => new()
        {
            // ---- Phishing (3) ----
            new QuizQuestion(
                "What should you do if you receive an email asking for your password?",
                new[] { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                2,
                "Reporting phishing emails helps your provider block similar attacks and protects other potential victims. Never share credentials via email.",
                "Phishing",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Beginner,
                new[] { "phishing", "email" },
                new[] { "Stallings (2022)" },
                "q1"
            ),

            new QuizQuestion(
                "You get an unexpected email from 'your bank' with a login link. What's safest?",
                new[] { "Click the link to check your account", "Reply asking if it's real", "Call the bank using the number on your card", "Forward it to friends for advice" },
                2,
                "Always verify through a number you already trust — never one supplied by the suspicious message itself.",
                "Phishing",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Beginner,
                new[] { "phishing", "email" },
                null,
                "q2"
            ),

            new QuizQuestion(
                "What is 'spear phishing'?",
                new[] { "A phishing attack targeting a specific individual or organisation", "A type of malware", "A secure email protocol", "A password cracking tool" },
                0,
                "Spear phishing is personalised and often uses social media to craft convincing, targeted lures that are far harder to spot than generic campaigns.",
                "Phishing",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Intermediate,
                new[] { "phishing", "social engineering" },
                new[] { "Stallings (2022)" },
                "q3"
            ),

            // ---- Passwords (3) ----
            new QuizQuestion(
                "Which of these makes the strongest password?",
                new[] { "12345678", "Password123", "T#9kR!mZ@vQ2xLpW", "yourname1990" },
                2,
                "Long, random strings mixing case, numbers and symbols resist brute-force and dictionary attacks far better than predictable patterns (Stallings, 2022).",
                "Passwords",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Beginner,
                new[] { "passwords" },
                new[] { "Stallings (2022)" },
                "q4"
            ),

            new QuizQuestion(
                "What's the safest way to store your passwords?",
                new[] { "A text file on your desktop", "A sticky note under the keyboard", "A reputable password manager", "The same password everywhere, memorised" },
                2,
                "Password managers generate and store unique strong credentials for every account, encrypted behind one master password (NIST SP 800-63B).",
                "Passwords",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Beginner,
                new[] { "passwords", "password manager" },
                new[] { "NIST SP 800-63B" },
                "q5"
            ),

            new QuizQuestion(
                "TRUE or FALSE: Reusing the same strong password across multiple sites is safe, as long as the password itself is long and complex.",
                new[] { "True", "False" },
                1,
                "False. If any one site is breached, attackers will try that same password everywhere else (credential stuffing) — strength alone doesn't prevent reuse risk.",
                "Passwords",
                QuestionType.TrueFalse,
                QuestionDifficulty.Intermediate,
                new[] { "passwords", "reuse" },
                null,
                "q6"
            ),

            // ---- Malware (2) ----
            new QuizQuestion(
                "Which malware type encrypts your files and demands payment to unlock them?",
                new[] { "Spyware", "Trojan", "Ransomware", "Adware" },
                2,
                "Ransomware denies access to your own data until a ransom is paid. Regular offline backups are the best defence (ENISA, 2022).",
                "Malware",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Beginner,
                new[] { "malware", "ransomware" },
                new[] { "ENISA (2022)" },
                "q7"
            ),

            new QuizQuestion(
                "You find a USB stick in a public car park. What should you do?",
                new[] { "Plug it in to identify the owner", "Hand it to IT/security without plugging it in", "Format it first, then use it", "Take it home and scan it yourself" },
                1,
                "Attackers deliberately plant infected USB drives. Only trained security staff should handle an unknown device.",
                "Malware",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Intermediate,
                new[] { "malware", "USB" },
                null,
                "q8"
            ),

            // ---- Safe Browsing (3) ----
            new QuizQuestion(
                "What does HTTPS in a website address actually guarantee?",
                new[] { "The site is malware-free", "The connection is encrypted in transit", "The site is government-verified", "No data is collected" },
                1,
                "HTTPS only encrypts the connection — it says nothing about whether the destination site itself is trustworthy.",
                "Safe Browsing",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Beginner,
                new[] { "browsing", "HTTPS" },
                null,
                "q9"
            ),

            new QuizQuestion(
                "TRUE or FALSE: A padlock icon (HTTPS) in your browser means a website is completely safe to use.",
                new[] { "True", "False" },
                1,
                "False. HTTPS only encrypts data in transit; scam and phishing sites can use HTTPS too. The padlock is not a trust seal.",
                "Safe Browsing",
                QuestionType.TrueFalse,
                QuestionDifficulty.Beginner,
                new[] { "browsing", "HTTPS" },
                null,
                "q10"
            ),

            new QuizQuestion(
                "TRUE or FALSE: Using public Wi-Fi for online banking is fine as long as the site has a padlock icon.",
                new[] { "True", "False" },
                1,
                "False. The padlock protects data in transit, but the Wi-Fi network itself could be a malicious 'evil twin' hotspot intercepting other parts of your traffic.",
                "Safe Browsing",
                QuestionType.TrueFalse,
                QuestionDifficulty.Intermediate,
                new[] { "browsing", "WiFi" },
                null,
                "q11"
            ),

            // ---- Social Engineering (2) ----
            new QuizQuestion(
                "What is social engineering, in a cybersecurity context?",
                new[] { "Hacking social media accounts", "Manipulating people psychologically to reveal information", "Writing malicious code", "Scanning a network for open ports" },
                1,
                "Social engineering targets human trust rather than technical flaws, which is why it underlies the majority of breaches (Stallings, 2022; Hadnagy, 2018).",
                "Social Engineering",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Beginner,
                new[] { "social engineering" },
                new[] { "Stallings (2022)", "Hadnagy (2018)" },
                "q12"
            ),

            new QuizQuestion(
                "A caller claims to be from your bank's fraud team and asks for your one-time PIN to 'verify' you. What do you do?",
                new[] { "Give the PIN — they sound official", "Hang up and call the number on your card", "Give half the PIN as a compromise", "Ask them to email instead" },
                1,
                "Legitimate banks never ask you to read out a one-time PIN over the phone. Hang up and call a number you already trust.",
                "Social Engineering",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Beginner,
                new[] { "social engineering", "vishing" },
                null,
                "q13"
            ),

            // ---- 2FA / Updates (2) ----
            new QuizQuestion(
                "What does 2FA stand for?",
                new[] { "Two-Factor Authentication", "Second Factor Access", "Dual Form Approval", "Two-File Authorization" },
                0,
                "Two-Factor Authentication requires a second proof of identity beyond your password, blocking the vast majority of automated account-takeover attempts.",
                "2FA",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Beginner,
                new[] { "2FA", "authentication" },
                null,
                "q14"
            ),

            new QuizQuestion(
                "Why should you install software updates promptly?",
                new[] { "Only for new features", "To patch security vulnerabilities before attackers exploit them", "Only for performance", "Updates rarely matter for security" },
                1,
                "Vendors patch known vulnerabilities continually; delaying updates leaves a known, exploitable gap open.",
                "Updates",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Beginner,
                new[] { "updates", "patching" },
                null,
                "q15"
            ),

            // ---- True/False extra ----
            new QuizQuestion(
                "TRUE or FALSE: Enabling 2FA on your email is especially important because email is used to reset most other account passwords.",
                new[] { "True", "False" },
                0,
                "True. Email is effectively the master key to your other accounts — protecting it first has an outsized security benefit.",
                "2FA",
                QuestionType.TrueFalse,
                QuestionDifficulty.Beginner,
                new[] { "2FA", "email" },
                null,
                "q16"
            ),

            // ---- CIA Triad ----
            new QuizQuestion(
                "Which of the following is NOT one of the three core components of the CIA triad?",
                new[] { "Confidentiality", "Integrity", "Availability", "Authentication" },
                3,
                "Authentication is a separate security mechanism, not part of the CIA triad (Stallings, 2022; ISO/IEC 27001).",
                "CIA Triad",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Intermediate,
                new[] { "CIA" },
                new[] { "Stallings (2022)", "ISO/IEC 27001" },
                "q17"
            ),

            // ---- Network Security ----
            new QuizQuestion(
                "What is the primary function of a firewall in network security?",
                new[] { "Scan for viruses", "Block unauthorised network traffic", "Encrypt data", "Manage user passwords" },
                1,
                "Firewalls enforce access control policies between networks, blocking or allowing traffic based on rules (NIST SP 800-41).",
                "Network Security",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Beginner,
                new[] { "network", "firewall" },
                new[] { "NIST SP 800-41" },
                "q18"
            ),

            // ---- Incident Response (2) ----
            new QuizQuestion(
                "According to standard incident response frameworks, what is the first step you should take after preparing?",
                new[] { "Eradication", "Recovery", "Detection and Analysis", "Containment" },
                2,
                "After preparation, the NIST IR framework (SP 800-61) identifies Detection & Analysis as the next step – you must first discover and confirm an incident.",
                "Incident Response",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Intermediate,
                new[] { "incident response" },
                new[] { "NIST SP 800-61" },
                "q19"
            ),

            new QuizQuestion(
                "TRUE or FALSE: Regularly testing your incident response plan through tabletop exercises is a recommended best practice.",
                new[] { "True", "False" },
                0,
                "True. Tabletop exercises simulate real incidents, exposing gaps in processes and communication before a real breach occurs (SANS).",
                "Incident Response",
                QuestionType.TrueFalse,
                QuestionDifficulty.Intermediate,
                new[] { "incident response", "tabletop" },
                new[] { "SANS" },
                "q20"
            ),

            // ---- Mobile Security ----
            new QuizQuestion(
                "A smartphone flashlight app asks for access to your contacts and location. This is an example of:",
                new[] { "Overprivileged permissions", "Normal app behaviour", "Required for malware scanning", "Part of OS security" },
                0,
                "Apps should only request permissions necessary for their core function. A flashlight only needs camera (flash) – contacts/location are excessive (OWASP Mobile).",
                "Mobile Security",
                QuestionType.MultipleChoice,
                QuestionDifficulty.Beginner,
                new[] { "mobile", "permissions" },
                new[] { "OWASP Mobile" },
                "q21"
            )
        };
    }
}