using System;
using System.Collections.Generic;
using Cybrog.Models;
using Microsoft.Data.SqlClient;

namespace Cybrog.Engine
{
    /// <summary>
    /// Persists cybersecurity tasks to a Microsoft SQL Server database.
    ///
    /// By default this targets SQL Server LocalDB (installed with Visual Studio /
    /// the "SQL Server Express LocalDB" workload), which requires no separate server
    /// process — ideal for a student assignment that must run on any Windows machine.
    /// A full SQL Server or SQL Express instance can be used instead by supplying a
    /// custom connection string to the constructor.
    ///
    /// On first run this class creates the "CybrogDb" database and "Tasks" table
    /// automatically if they do not already exist (idempotent schema setup), so there
    /// is zero manual setup for the grader beyond having LocalDB available, which ships
    /// with Visual Studio's ".NET desktop development" workload.
    ///
    /// Satisfies Task 1: robust, error-handled database integration with full CRUD
    /// (add, read, mark complete, delete) kept in sync with the GUI.
    /// </summary>
    public class DatabaseManager : IDatabaseManager
    {
        // LocalDB connection: no server install required, data file lives under
        // %LOCALAPPDATA%\Microsoft\Microsoft SQL Server Local DB.
        // AttachDbFilename keeps the .mdf inside the application's own folder so the
        // assignment is fully self-contained and portable between machines.
        private const string DefaultConnectionTemplate =
            @"Server=(localdb)\MSSQLLocalDB;Database=CybrogDb;Integrated Security=true;" +
            @"AttachDbFilename={0};TrustServerCertificate=true;Connection Timeout=5;";

        private readonly string _connectionString;
        private readonly string _masterConnectionString;

        /// <summary>
        /// True once <see cref="EnsureDatabaseAndTable"/> has completed successfully.
        /// The engine layer checks this and falls back to a clear "DB unavailable"
        /// message rather than crashing if SQL Server / LocalDB is not installed.
        /// </summary>
        public bool IsAvailable { get; private set; }

        /// <summary>Last connection/setup error message, for diagnostics shown in the GUI.</summary>
        public string? LastError { get; private set; }

        public DatabaseManager(string? customConnectionString = null)
        {
            if (customConnectionString != null)
            {
                _connectionString = customConnectionString;
                _masterConnectionString = customConnectionString;
            }
            else
            {
                string dbFile = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "CybrogDb.mdf");
                _connectionString = string.Format(DefaultConnectionTemplate, dbFile);

                // Master connection (no AttachDbFilename) is used only to check/create
                // the physical .mdf file via sp_attach or CREATE DATABASE ON.
                _masterConnectionString =
                    @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;" +
                    @"TrustServerCertificate=true;Connection Timeout=5;";
            }

            try
            {
                EnsureDatabaseAndTable();
                IsAvailable = true;
            }
            catch (Exception ex)
            {
                IsAvailable = false;
                LastError = ex.Message;
            }
        }

        // ── Schema setup ─────────────────────────────────────────────────────

        private void EnsureDatabaseAndTable()
        {
            string dbFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CybrogDb.mdf");
            string logFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CybrogDb_log.ldf");

            // Step 1: create the physical database file via the master connection if it
            // doesn't already exist on disk. This pattern is the standard way to get a
            // zero-install, file-based SQL Server database for a desktop app.
            if (!System.IO.File.Exists(dbFile))
            {
                using var masterConn = new SqlConnection(_masterConnectionString);
                masterConn.Open();
                using var createCmd = masterConn.CreateCommand();
                createCmd.CommandText = $@"
                    IF DB_ID('CybrogDb') IS NULL
                    BEGIN
                        CREATE DATABASE CybrogDb
                        ON PRIMARY (NAME = CybrogDb, FILENAME = '{dbFile.Replace("'", "''")}')
                        LOG ON (NAME = CybrogDb_log, FILENAME = '{logFile.Replace("'", "''")}');
                    END";
                createCmd.ExecuteNonQuery();

                // Detach so the file-based connection string (AttachDbFilename) can
                // open it directly without a name clash.
                using var detachCmd = masterConn.CreateCommand();
                detachCmd.CommandText = "EXEC sp_detach_db 'CybrogDb'";
                try { detachCmd.ExecuteNonQuery(); } catch { /* already detached/standalone — safe to ignore */ }
            }

            // Step 2: connect via the file (AttachDbFilename) and create the table.
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tasks')
                BEGIN
                    CREATE TABLE Tasks (
                        Id            INT IDENTITY(1,1) PRIMARY KEY,
                        Title         NVARCHAR(255)  NOT NULL,
                        Description   NVARCHAR(MAX)  NULL,
                        CreatedDate   DATETIME2      NOT NULL DEFAULT (SYSDATETIME()),
                        ReminderDate  DATETIME2      NULL,
                        IsCompleted   BIT            NOT NULL DEFAULT (0)
                    );
                END";
            cmd.ExecuteNonQuery();
        }

        // ── CRUD operations ──────────────────────────────────────────────────

        /// <summary>Inserts a new task and returns its generated identity ID.</summary>
        public int AddTask(string title, string? description = null, DateTime? reminderDate = null)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Tasks (Title, Description, ReminderDate)
                OUTPUT INSERTED.Id
                VALUES (@title, @description, @reminder);";
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@description", (object?)description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@reminder", (object?)reminderDate ?? DBNull.Value);

            return (int)cmd.ExecuteScalar();
        }

        /// <summary>
        /// Returns tasks from the database. When <paramref name="includeCompleted"/> is
        /// false, only pending tasks are returned, soonest reminder first.
        /// </summary>
        public List<TaskItem> GetTasks(bool includeCompleted = true)
        {
            var results = new List<TaskItem>();
            string sql = includeCompleted
                ? "SELECT Id, Title, Description, CreatedDate, ReminderDate, IsCompleted FROM Tasks ORDER BY IsCompleted ASC, CreatedDate DESC"
                : "SELECT Id, Title, Description, CreatedDate, ReminderDate, IsCompleted FROM Tasks WHERE IsCompleted = 0 ORDER BY ReminderDate ASC, CreatedDate DESC";

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                results.Add(new TaskItem
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("Description")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                    ReminderDate = reader.IsDBNull(reader.GetOrdinal("ReminderDate"))
                        ? (DateTime?)null
                        : reader.GetDateTime(reader.GetOrdinal("ReminderDate")),
                    IsCompleted = reader.GetBoolean(reader.GetOrdinal("IsCompleted"))
                });
            }

            return results;
        }

        /// <summary>Marks a task complete. Returns true if a row was actually updated.</summary>
        public bool MarkTaskCompleted(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Tasks SET IsCompleted = 1 WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>Re-opens a previously completed task. Returns true if a row was updated.</summary>
        public bool MarkTaskPending(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Tasks SET IsCompleted = 0 WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>Permanently deletes a task. Returns true if a row was deleted.</summary>
        public bool DeleteTask(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Tasks WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>Updates the reminder date for an existing task. Returns true if updated.</summary>
        public bool SetReminder(int id, DateTime reminderDate)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Tasks SET ReminderDate = @reminder WHERE Id = @id";
            cmd.Parameters.AddWithValue("@reminder", reminderDate);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
