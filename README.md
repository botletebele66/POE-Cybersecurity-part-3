# Cybrog — Cybersecurity Awareness Chatbot (Part 3 / POE)

A WPF desktop chatbot that educates users on cybersecurity, manages a task list with
reminders backed by Microsoft SQL Server, runs a 15-question quiz mini-game, recognises
natural-language commands, and keeps a running activity log — built on top of the
existing Part 1/Part 2 conversational engine (dynamic responses, keyword recognition,
sentiment detection).

![Palette](https://img.shields.io/badge/palette-white%20%2F%20yellow%20%2F%20light--grey-F6C90E)
![.NET](https://img.shields.io/badge/.NET-10.0--windows-512BD4)
![DB](https://img.shields.io/badge/database-SQL%20Server%20LocalDB-CC2927)

---

## 1. Solution layout

```
Cybrog.sln
├── Cybrog/                          ← the WPF application (this is what you run)
│   ├── App.xaml / App.xaml.cs       ← global resource dictionary (design tokens) + entry point
│   ├── MainWindow.xaml / .cs        ← single-window GUI: Chat / Topics / Tasks / Quiz tabs + My Memory sidebar
│   ├── Converters/
│   │   └── Converters.cs            ← 7 IValueConverter implementations for the bindings below
│   ├── Engine/                      ← all business logic (no WPF types in here, except DatabaseManager's
│   │                                   use of System.Data types, which is intentional)
│   │   ├── IDatabaseManager.cs      ← persistence abstraction (enables unit testing without real SQL Server)
│   │   ├── DatabaseManager.cs       ← SQL Server LocalDB implementation, full CRUD, auto-schema creation
│   │   ├── TaskService.cs           ← business layer over IDatabaseManager, owns the bindable task collection
│   │   ├── QuizEngine.cs            ← 15-question bank, shuffled delivery, scoring
│   │   ├── NlpIntentParser.cs       ← regex-based intent recognition (Task 3)
│   │   ├── KeywordTopicMatcher.cs   ← synonym-based topic detection for free-text chat
│   │   ├── SentimentAnalyzer.cs     ← Part 1/2 carry-over: worried/frustrated/curious/positive detection
│   │   ├── SecurityKnowledgeBase.cs ← the 5 required topics, STOMP-structured, Stallings (2022) cited
│   │   ├── ResponseLibrary.cs       ← varied canned responses (greetings, farewells, fallback, confirmations)
│   │   ├── ActivityLogger.cs        ← timestamped action log with paging (Task 4)
│   │   ├── ConversationEngine.cs    ← orchestrator: the only class MainWindow talks to for chat logic
│   │   └── AudioManager.cs          ← plays the startup greeting WAV
│   ├── Models/                      ← plain data classes (TaskItem, QuizQuestion, SecurityTopic, ChatMessage,
│   │                                   UserSession, ActivityLogDisplayItem)
│   └── Resources/
│       └── Greeting AI .wav         ← startup greeting audio
│
├── Cybrog.Tests/                    ← xUnit test project (10 test classes, 90+ test cases)
│   ├── FakeDatabaseManager.cs       ← in-memory IDatabaseManager for testing TaskService without real SQL Server
│   └── *Tests.cs                    ← one file per engine class under test
│
└── README.md                        ← this file
```

## 2. How to build and run

**Prerequisites** (Windows only — WPF cannot run on macOS/Linux):
- Visual Studio 2022 (17.8+) or the .NET 10 SDK, with the **".NET desktop development"** workload installed
  (this installs SQL Server Express LocalDB, which `DatabaseManager` uses automatically)

**Steps:**
1. Open `Cybrog.sln` in Visual Studio, **or** from a terminal:
   ```
   dotnet restore
   dotnet build
   dotnet run --project Cybrog/Cybrog.csproj
   ```
2. On first launch, `DatabaseManager` automatically creates `CybrogDb.mdf` next to the
   executable and the `Tasks` table inside it — there is no manual database setup step.
3. If LocalDB isn't installed, the app still runs; the Tasks tab will show a clear
   "Database unavailable" message instead of silently failing (see §5, error handling).

**To run the test suite:**
```
dotnet test
```

## 3. Database notes (why SQL Server, not MySQL)

The original Part 3 rubric names MySQL as the example database. This build uses
**Microsoft SQL Server** (via `Microsoft.Data.SqlClient`, targeting SQL Server Express
LocalDB by default) per explicit instruction, which is a deliberate, documented
deviation from the rubric's stated example — not an oversight. `DatabaseManager`
auto-creates the database file (`CybrogDb.mdf`) and the `Tasks` table on first run, so
there's no manual SQL setup required by whoever runs this. If your environment has a
named SQL Server / SQL Express instance instead of LocalDB, pass a custom connection
string to `new DatabaseManager("your connection string")` in `MainWindow`'s constructor.

## 4. Rubric coverage map

| Requirement | Where it lives |
|---|---|
| GUI-based (not console) | Entire app is WPF; `OutputType=WinExe` |
| Continues Part 1/2 (dynamic responses, keywords, sentiment) | `ResponseLibrary`, `KeywordTopicMatcher`, `SentimentAnalyzer`, wired through `ConversationEngine` |
| **Task 1** — DB-backed tasks with title, description, optional reminder | `TaskItem` model; Tasks tab form (title + description + date picker); `DatabaseManager` CRUD |
| View / delete / complete tasks, reflected in DB | Tasks tab list (checkbox + delete button), all routed through `TaskService` → `DatabaseManager` |
| **Task 2** — 10+ quiz questions, MCQ + True/False, one at a time, immediate feedback, final score message | `QuizEngine`: 15 questions (11 MCQ + 4 T/F); Quiz tab and "quiz me" chat command both single-question-at-a-time |
| **Task 3** — NLP via string/regex matching, flexible phrasing | `NlpIntentParser`: 11 regex-based intents, handles "remind me to X", "can you add a task to X", "task 3 done", etc. (see test suite for the full phrasing matrix) |
| **Task 4** — Activity log, "Show activity log" / "What have you done for me?" commands, last 5–10 entries, optional "show more" | `ActivityLogger` (capped at 1000, paged); sidebar card with a working **Show more / Show less** toggle |
| White / yellow / light-grey only, no dark UI | `App.xaml` resource dictionary — every colour token is white, yellow, or light-grey; verify via the `Brush*` keys, none below `#E5E3DA` in darkness |
| .NET 10, Windows | `<TargetFramework>net10.0-windows</TargetFramework>` in both `.csproj` files |
| Strict OOP | `IDatabaseManager` abstraction (dependency inversion), encapsulated mutable state (`private set`), single-responsibility classes composed by `ConversationEngine` rather than one God-class |
| Cyborg/Cybrog name + greeting WAV | Window title, sidebar branding, `AudioManager.PlayGreeting()` called from `MainWindow_Loaded` |
| Stallings (2022) citations | Every `SecurityTopic.Reference` in `SecurityKnowledgeBase.cs` |

## 5. Notable design decisions & known trade-offs

- **Quiz state shared between Chat and Mini-Game tabs.** Both the chat's "quiz me"
  command and the Quiz tab's "Start Quiz" button drive the *same* `QuizEngine` instance.
  `ConversationEngine.ProcessMessage` checks `_quiz.IsActive && _quiz.CurrentQuestion
  != null` (rather than its own separate flag) specifically so that switching tabs
  mid-quiz never desyncs the two surfaces — see `ConversationEngineTests
  .ProcessMessage_QuizStartedFromExternalCaller_StillRoutesAnswersCorrectly` for the
  regression test guarding this.
- **`ExtractTaskTitle` falls back to "Cybersecurity task"** when a reminder phrase has
  no actual description (e.g. "remind me in 3 days" with nothing after it), rather than
  using the raw unstripped sentence as the task title.
- **All user-facing dates are formatted with `CultureInfo.InvariantCulture`** ("dd MMM
  yyyy" → e.g. "30 May 2026"), so the app's English-only UI stays consistent regardless
  of the host machine's regional settings.
- **Database unavailability is handled gracefully, not fatally.** If LocalDB isn't
  installed, `DatabaseManager.IsAvailable` is `false`, the Tasks tab shows a status
  banner explaining why, and `AddTask` returns `null` with a clear chat/MessageBox
  explanation rather than throwing or silently losing data.
- **`IDatabaseManager` interface exists purely for testability and dependency
  inversion** — `TaskService` is unit-tested via `FakeDatabaseManager`, an in-memory
  stand-in, since a CI/grading environment can't be assumed to have SQL Server LocalDB
  installed.

## 6. Test coverage

`Cybrog.Tests` contains 10 test classes covering every Engine and Models class:

| Test class | What it verifies |
|---|---|
| `NlpIntentParserTests` | All 11 intents, multiple phrasings each, the "remind me in 3 days" edge case, "can you add a task to X" edge case |
| `QuizEngineTests` | Question count ≥10, MCQ+T/F mix present, scoring, exhaustion behaviour, shuffle, invalid-state exception |
| `TaskServiceTests` | Add/complete/reopen/delete, display-number lookup, pending/completed partitioning, DB-unavailable handling |
| `ConversationEngineTests` | Full chat flow integration: greetings, name memory, task commands via chat, quiz state-machine cross-tab regression |
| `ActivityLoggerTests` | Ordering, paging, the 1000-entry cap with FIFO eviction, event notifications |
| `TaskItemTests` | `IsOverdue`/`StatusLabel` computed logic, `PropertyChanged` notifications |
| `UserSessionTests` | Name memory, favourite-vs-last-topic distinction, interaction counting |
| `ResponseLibraryTests` | Canned response variety and content |
| `EngineSupportTests` | `SentimentAnalyzer`, `KeywordTopicMatcher`, `SecurityKnowledgeBase` (all 5 topics present, Stallings cited) |

Run with `dotnet test` from the solution root (requires Windows + the .NET 10 SDK, since
the test project also targets `net10.0-windows` to reference the WPF main project).

## 7. Author

Botshelo Letebele Rosebank Internatinal College, 2nd-year Software Development, Part 3 POE submission.
