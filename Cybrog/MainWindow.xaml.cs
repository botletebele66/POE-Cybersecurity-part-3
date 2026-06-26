using Cybrog.Engine;
using Cybrog.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Cybrog
{
    /// <summary>
    /// Code-behind for the main Cybrog window. Owns the engine instances and is
    /// responsible only for translating user interaction into engine calls and
    /// engine results into UI updates — all conversational/business logic itself
    /// lives in the Engine layer, keeping this class a thin presentation adapter
    /// (separation of concerns, per strict OOP requirements of the brief).
    /// </summary>
    public partial class MainWindow : Window
    {
        // ── Engine layer (instantiated once, for the lifetime of the window) ──
        private readonly DatabaseManager _db;
        private readonly TaskService _taskService;
        private readonly ActivityLogger _logger;
        private readonly QuizEngine _quizEngine;
        private readonly ConversationEngine _conversation;
        private readonly AudioManager _audio;

        private readonly DispatcherTimer _clockTimer = new() { Interval = TimeSpan.FromSeconds(1) };
        private int? _bestQuizScore;
        private int _bestQuizTotal;
        private bool _showFullActivityLog;

        public TaskService Tasks => _taskService;

        public MainWindow()
        {
            InitializeComponent();

            // Compose the engine graph. DatabaseManager is the only class that
            // talks to SQL Server directly; everything above it only sees the
            // higher-level TaskService API.
            _db = new DatabaseManager();
            _taskService = new TaskService(_db);
            _logger = new ActivityLogger();
            _quizEngine = new QuizEngine();
            _conversation = new ConversationEngine(_quizEngine, _taskService, _logger);
            _audio = new AudioManager();

            _logger.LogChanged += () => Dispatcher.Invoke(RefreshActivityLog);
            _taskService.Tasks.CollectionChanged += (_, _) => RefreshTasksView();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BuildQuickTopics();
            BuildTopicsGrid();
            RefreshTasksView();
            RefreshActivityLog();
            RefreshMemoryPanel();
            UpdateDbStatus();
            StartClock();

            GreetingText.Text = "Hello! I'm Cybrog 🤖";
            AppendBotMessage(ResponseLibrary.GetGreeting());

            _audio.PlayGreeting(); // fire-and-forget; failure is silent by design (see AudioManager)
            _logger.Log("Application started", "Session");

            ChatInput.Focus();
        }

        // ════════════════════════════════════════════════════════════════════
        // NAVIGATION
        // ════════════════════════════════════════════════════════════════════

        private void NavTab_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is not RadioButton rb || ChatView == null) return; // guard against firing during InitializeComponent

            ChatView.Visibility = Visibility.Collapsed;
            TopicsView.Visibility = Visibility.Collapsed;
            TasksView.Visibility = Visibility.Collapsed;
            QuizView.Visibility = Visibility.Collapsed;

            switch (rb.Tag as string)
            {
                case "Chat": ChatView.Visibility = Visibility.Visible; ChatInput.Focus(); break;
                case "Topics": TopicsView.Visibility = Visibility.Visible; break;
                case "Tasks": TasksView.Visibility = Visibility.Visible; RefreshTasksView(); break;
                case "Quiz": QuizView.Visibility = Visibility.Visible; break;
            }
        }

        private void GoToQuiz_Click(object sender, RoutedEventArgs e)
        {
            NavQuiz.IsChecked = true;
        }

        // ════════════════════════════════════════════════════════════════════
        // CLOCK (sidebar header)
        // ════════════════════════════════════════════════════════════════════

        private void StartClock()
        {
            UpdateClock();
            _clockTimer.Tick += (_, _) => UpdateClock();
            _clockTimer.Start();
        }

        private void UpdateClock()
        {
            var now = DateTime.Now;
            ClockText.Text = now.ToString("hh:mm:ss tt", CultureInfo.InvariantCulture);
            DateText.Text = now.ToString("dddd, dd MMMM yyyy", CultureInfo.InvariantCulture);
        }

        // ════════════════════════════════════════════════════════════════════
        // CHAT
        // ════════════════════════════════════════════════════════════════════

        private void ChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                SendCurrentMessage();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) => SendCurrentMessage();

        private void SendCurrentMessage()
        {
            string text = ChatInput.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            AppendUserMessage(text);
            ChatInput.Clear();

            string reply = _conversation.ProcessMessage(text);
            AppendBotMessage(reply);

            RefreshMemoryPanel();
            RefreshTasksView();
            UpdateSidebarBestScore();
        }

        private void AppendUserMessage(string text) => AppendMessage(text, MessageSender.User);
        private void AppendBotMessage(string text) => AppendMessage(text, MessageSender.Bot);

        private void AppendMessage(string text, MessageSender sender)
        {
            var items = (ChatItems.ItemsSource as List<ChatMessage>) ?? new List<ChatMessage>();
            items.Add(new ChatMessage { Sender = sender, Text = text });
            ChatItems.ItemsSource = null;
            ChatItems.ItemsSource = items;

            // FIX: Replace the old ChatScroll reference with ListBox scrolling
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (ChatItems.Items.Count > 0)
                {
                    ChatItems.ScrollIntoView(ChatItems.Items[^1]);
                }
            }), DispatcherPriority.Background);
        }

        private void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            ChatItems.ItemsSource = new List<ChatMessage>();
            AppendBotMessage("Chat cleared. What would you like to learn about next?");
            _logger.Log("Cleared chat", "Session");
        }

        // ── Quick topic pills (under the chat input) ───────────────────────

        private void BuildQuickTopics()
        {
            QuickTopicsPanel.Children.Clear();
            foreach (var topic in SecurityKnowledgeBase.All)
            {
                var btn = new Button
                {
                    Style = (Style)FindResource("PillButton"),
                    Tag = topic.Key,
                    Content = $"{topic.Icon} {topic.DisplayName}"
                };
                btn.Click += QuickTopic_Click;
                QuickTopicsPanel.Children.Add(btn);
            }
        }

        private void QuickTopic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string key) return;
            string topicName = SecurityKnowledgeBase.Get(key)?.DisplayName ?? key;
            AppendUserMessage($"Tell me about {topicName}");
            AppendBotMessage(_conversation.AskAboutTopic(key));
            RefreshMemoryPanel();
        }

        // ════════════════════════════════════════════════════════════════════
        // TOPICS TAB
        // ════════════════════════════════════════════════════════════════════

        private void BuildTopicsGrid()
        {
            TopicsItems.ItemsSource = SecurityKnowledgeBase.All.ToList();
        }

        private void TopicCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string key) return;
            NavChat.IsChecked = true; // jump to chat to show the full lesson
            string topicName = SecurityKnowledgeBase.Get(key)?.DisplayName ?? key;
            AppendUserMessage($"Tell me about {topicName}");
            AppendBotMessage(_conversation.AskAboutTopic(key));
            RefreshMemoryPanel();
        }

        // ════════════════════════════════════════════════════════════════════
        // TASKS TAB
        // ════════════════════════════════════════════════════════════════════

        private void UpdateDbStatus()
        {
            DbStatusText.Text = _taskService.IsDatabaseAvailable
                ? "● Connected to SQL Server"
                : "● Database unavailable — tasks won't be saved";
            DbStatusText.Foreground = _taskService.IsDatabaseAvailable
                ? (Brush)FindResource("BrushSuccess")
                : (Brush)FindResource("BrushDanger");
        }

        private void RefreshTasksView()
        {
            TasksItems.ItemsSource = null;
            TasksItems.ItemsSource = _taskService.Tasks;
            NoTasksText.Visibility = _taskService.Tasks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = NewTaskTitle.Text.Trim();
            string description = NewTaskDescription.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Please enter a task title before adding.", "Cybrog",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DateTime? reminder = NewTaskReminder.SelectedDate;
            var created = _taskService.AddTask(title, description, reminder);

            if (created == null)
            {
                MessageBox.Show($"Couldn't save the task — the database is unavailable.\n\n{_taskService.DatabaseError}",
                    "Cybrog", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _logger.Log($"Added task: {title}", "Tasks");
            NewTaskTitle.Clear();
            NewTaskDescription.Clear();
            NewTaskReminder.SelectedDate = null;
            RefreshActivityLog();
        }

        private void TaskCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox cb || cb.Tag is not int id) return;

            var task = _taskService.FindById(id);
            if (task == null) return;

            if (cb.IsChecked == true && !task.IsCompleted)
            {
                _taskService.CompleteTask(id);
                _logger.Log($"Completed task: {task.Title}", "Tasks");
            }
            else if (cb.IsChecked == false && task.IsCompleted)
            {
                _taskService.ReopenTask(id);
                _logger.Log($"Reopened task: {task.Title}", "Tasks");
            }
            RefreshActivityLog();
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int id) return;

            var task = _taskService.FindById(id);
            if (task == null) return;

            var result = MessageBox.Show($"Delete \"{task.Title}\"?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            string title = task.Title;
            _taskService.DeleteTask(id);
            _logger.Log($"Deleted task: {title}", "Tasks");
            RefreshActivityLog();
        }

        // ════════════════════════════════════════════════════════════════════
        // QUIZ TAB
        // ════════════════════════════════════════════════════════════════════

        /// <summary>Exit the active quiz and return to the intro screen.</summary>
        private void ExitQuiz_Click(object sender, RoutedEventArgs e)
        {
            QuizActiveCard.Visibility = Visibility.Collapsed;
            QuizIntroCard.Visibility = Visibility.Visible;
            QuizFeedbackBox.Visibility = Visibility.Collapsed;
            QuizNextButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>Handle keyboard shortcuts during the quiz: 1‑4 = answer, Enter = next.</summary>
        private void QuizActiveCard_KeyDown(object sender, KeyEventArgs e)
        {
            if (QuizActiveCard.Visibility != Visibility.Visible)
                return;

            int? answerIndex = e.Key switch
            {
                Key.D1 or Key.NumPad1 => 0,
                Key.D2 or Key.NumPad2 => 1,
                Key.D3 or Key.NumPad3 => 2,
                Key.D4 or Key.NumPad4 => 3,
                _ => null
            };

            if (answerIndex.HasValue && answerIndex.Value < QuizOptionsPanel.Children.Count)
            {
                if (QuizOptionsPanel.Children[answerIndex.Value] is Button btn)
                    btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            else if (e.Key == Key.Enter && QuizNextButton.Visibility == Visibility.Visible)
            {
                QuizNextButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            QuizIntroCard.Visibility = Visibility.Collapsed;
            QuizResultsCard.Visibility = Visibility.Collapsed;
            QuizActiveCard.Visibility = Visibility.Visible;

            _quizEngine.Start();
            _logger.Log("Started cybersecurity quiz", "Quiz");
            RenderQuizQuestion(_quizEngine.GetNextQuestion());

            // Important: give the keyboard focus to the quiz card so key events work
            QuizActiveCard.Focus();
        }

        private void RenderQuizQuestion(QuizQuestion? q)
        {
            if (q == null) { ShowQuizResults(); return; }

            QuizProgressText.Text = $"Question {_quizEngine.CurrentQuestionNumber}/{_quizEngine.TotalQuestions}";
            QuizScoreText.Text = $"Score: {_quizEngine.Score}";
            QuizQuestionText.Text = q.Text;
            QuizFeedbackBox.Visibility = Visibility.Collapsed;
            QuizNextButton.Visibility = Visibility.Collapsed;

            QuizOptionsPanel.Children.Clear();
            for (int i = 0; i < q.Options.Count; i++)
            {
                int optionIndex = i;
                var optBtn = new Button
                {
                    Style = (Style)FindResource("SecondaryButton"),
                    Content = $"{(char)('A' + i)})  {q.Options[i]}",
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                optBtn.Click += (_, _) => SubmitQuizAnswer(optionIndex);
                QuizOptionsPanel.Children.Add(optBtn);
            }
        }

        private void SubmitQuizAnswer(int selectedIndex)
        {
            foreach (var child in QuizOptionsPanel.Children.OfType<Button>())
                child.IsEnabled = false;

            bool correct = _quizEngine.SubmitAnswer(selectedIndex);

            QuizFeedbackBox.Visibility = Visibility.Visible;
            QuizFeedbackBox.Background = correct
                ? (Brush)FindResource("BrushSuccessSoft")
                : (Brush)FindResource("BrushWarnSoft");
            QuizFeedbackText.Text = (correct ? "✅ Correct! " : "❌ Not quite. ") + _quizEngine.LastExplanation;
            QuizFeedbackText.Foreground = correct
                ? (Brush)FindResource("BrushSuccess")
                : (Brush)FindResource("BrushWarn");

            QuizScoreText.Text = $"Score: {_quizEngine.Score}";
            QuizNextButton.Visibility = Visibility.Visible;
            QuizNextButton.Content = _quizEngine.IsActive ? "Next Question →" : "See Results →";
        }

        private void QuizNext_Click(object sender, RoutedEventArgs e)
        {
            RenderQuizQuestion(_quizEngine.GetNextQuestion());
        }

        private void ShowQuizResults()
        {
            QuizActiveCard.Visibility = Visibility.Collapsed;
            QuizResultsCard.Visibility = Visibility.Visible;

            var (score, total, message) = _quizEngine.GetFinalResult();
            int pct = total > 0 ? score * 100 / total : 0;

            ResultsEmoji.Text = pct >= 70 ? "🎉" : (pct >= 50 ? "👍" : "📚");
            ResultsScoreText.Text = $"{score}/{total}";
            ResultsMessageText.Text = message;

            if (_bestQuizScore == null || score > _bestQuizScore)
            {
                _bestQuizScore = score;
                _bestQuizTotal = total;
            }

            _logger.Log($"Completed quiz: {score}/{total} ({pct}%)", "Quiz");
            RefreshActivityLog();
            UpdateSidebarBestScore();
            UpdateQuizIntroBestScore();
        }

        private void UpdateQuizIntroBestScore()
        {
            BestScoreText.Text = _bestQuizScore.HasValue
                ? $"Your best score: {_bestQuizScore}/{_bestQuizTotal} ({_bestQuizScore * 100 / Math.Max(1, _bestQuizTotal)}%)"
                : string.Empty;
        }

        private void UpdateSidebarBestScore()
        {
            SidebarBestScoreText.Text = _bestQuizScore.HasValue
                ? $"Best score: {_bestQuizScore}/{_bestQuizTotal} ({_bestQuizScore * 100 / Math.Max(1, _bestQuizTotal)}%)"
                : "No attempts yet";
        }

        // ════════════════════════════════════════════════════════════════════
        // SIDEBAR — MY MEMORY & ACTIVITY LOG
        // ════════════════════════════════════════════════════════════════════

        private void RefreshMemoryPanel()
        {
            var session = _conversation.Session;
            MemNameText.Text = session.HasName ? session.UserName : "Not shared yet";
            MemFavouriteText.Text = string.IsNullOrEmpty(session.FavouriteTopic) ? "—" : session.FavouriteTopic;
            MemLastTopicText.Text = string.IsNullOrEmpty(session.LastTopic) ? "—" : session.LastTopic;
            MemInteractionsText.Text = session.TotalInteractions.ToString();
        }

        private void RefreshActivityLog()
        {
            var sourceEntries = _showFullActivityLog ? _logger.GetAll() : _logger.GetRecent(10);
            var recent = sourceEntries
                .Select(entry => new Models.ActivityLogDisplayItem { Time = entry.Timestamp.ToString("HH:mm"), Action = entry.Action })
                .ToList();

            ActivityLogItems.ItemsSource = recent;
            NoActivityText.Visibility = recent.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            // Only offer the toggle once there's actually more history than the default
            // page shows; otherwise "Show more" would do nothing, which is confusing.
            ToggleLogButton.Visibility = (_logger.TotalCount > 10) ? Visibility.Visible : Visibility.Collapsed;
            ToggleLogButton.Content = _showFullActivityLog ? "Show less" : "Show more";
        }

        private void ToggleActivityLog_Click(object sender, RoutedEventArgs e)
        {
            _showFullActivityLog = !_showFullActivityLog;
            RefreshActivityLog();
        }
    }
}