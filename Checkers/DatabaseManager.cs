using System.Data;
using MySql.Data.MySqlClient;

namespace Checkers;

public class DatabaseManager
{
    public string ConnectionString { get; private set; } =
        "Server=localhost;Port=3306;Database=checkers;Uid=root;Pwd=;SslMode=none;AllowUserVariables=True;";

    public void UpdateConnectionString(string connectionString)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            ConnectionString = connectionString.Trim();
        }
    }

    public bool EnsureDatabaseSchema(out string error)
    {
        error = string.Empty;

        const string schemaSql = @"
CREATE TABLE IF NOT EXISTS Players (
    PlayerID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    FullName VARCHAR(100) NOT NULL,
    Password VARCHAR(255) NOT NULL,
    DateCreated DATETIME DEFAULT CURRENT_TIMESTAMP,
    LastLogin DATETIME
);

CREATE TABLE IF NOT EXISTS Stats (
    StatsID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    PlayerID INT NOT NULL,
    EasyWins INT DEFAULT 0,
    NormalWins INT DEFAULT 0,
    HardWins INT DEFAULT 0,
    TotalWins INT DEFAULT 0,
    TotalLosses INT DEFAULT 0,
    BestWinSeconds INT NULL,
    FOREIGN KEY (PlayerID) REFERENCES Players(PlayerID)
);

CREATE TABLE IF NOT EXISTS MatchHistory (
    MatchID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    PlayerID INT NOT NULL,
    OpponentType VARCHAR(20) NOT NULL,
    Difficulty VARCHAR(20),
    Result VARCHAR(10) NOT NULL,
    MovesCount INT,
    DurationSeconds INT NULL,
    DatePlayed DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PlayerID) REFERENCES Players(PlayerID)
);

CREATE TABLE IF NOT EXISTS MatchSeries (
    SeriesID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    PlayerID INT NOT NULL,
    BestOf INT NOT NULL,
    Wins INT DEFAULT 0,
    Losses INT DEFAULT 0,
    FinalResult VARCHAR(10),
    DatePlayed DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PlayerID) REFERENCES Players(PlayerID)
);";

        try
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            using (var cmd = new MySqlCommand(schemaSql, connection))
            {
                cmd.ExecuteNonQuery();
            }

            // Compatibility path for older MySQL versions that do not support
            // 'ADD COLUMN IF NOT EXISTS'.
            AddColumnIfMissing(connection, "Stats", "BestWinSeconds", "INT NULL");
            AddColumnIfMissing(connection, "MatchHistory", "DurationSeconds", "INT NULL");

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static void AddColumnIfMissing(MySqlConnection connection, string tableName, string columnName, string columnDefinition)
    {
        const string existsSql = @"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = @schema
  AND TABLE_NAME = @table
  AND COLUMN_NAME = @column;";

        using var existsCmd = new MySqlCommand(existsSql, connection);
        existsCmd.Parameters.AddWithValue("@schema", connection.Database);
        existsCmd.Parameters.AddWithValue("@table", tableName);
        existsCmd.Parameters.AddWithValue("@column", columnName);

        var exists = Convert.ToInt32(existsCmd.ExecuteScalar()) > 0;
        if (exists)
        {
            return;
        }

        string alterSql = $"ALTER TABLE `{tableName}` ADD COLUMN `{columnName}` {columnDefinition};";
        using var alterCmd = new MySqlCommand(alterSql, connection);
        alterCmd.ExecuteNonQuery();
    }

    public bool Register(string username, string fullName, string password, out string message)
    {
        message = string.Empty;

        try
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            using var tx = connection.BeginTransaction();

            const string insertPlayer = @"
INSERT INTO Players (Username, FullName, Password)
VALUES (@username, @fullname, @password);
SELECT LAST_INSERT_ID();";

            using var insertPlayerCmd = new MySqlCommand(insertPlayer, connection, tx);
            insertPlayerCmd.Parameters.AddWithValue("@username", username);
            insertPlayerCmd.Parameters.AddWithValue("@fullname", fullName);
            insertPlayerCmd.Parameters.AddWithValue("@password", password);

            int playerId = Convert.ToInt32(insertPlayerCmd.ExecuteScalar());

            const string insertStats = "INSERT INTO Stats (PlayerID) VALUES (@playerId);";
            using var insertStatsCmd = new MySqlCommand(insertStats, connection, tx);
            insertStatsCmd.Parameters.AddWithValue("@playerId", playerId);
            insertStatsCmd.ExecuteNonQuery();

            tx.Commit();
            return true;
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            message = "Username already exists.";
            return false;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return false;
        }
    }

    public PlayerAccount? Login(string username, string password, out string message)
    {
        message = string.Empty;

        try
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            const string selectSql = @"
SELECT PlayerID, Username, FullName, LastLogin
FROM Players
WHERE Username = @username AND Password = @password;";

            using var selectCmd = new MySqlCommand(selectSql, connection);
            selectCmd.Parameters.AddWithValue("@username", username);
            selectCmd.Parameters.AddWithValue("@password", password);

            using var reader = selectCmd.ExecuteReader();
            if (!reader.Read())
            {
                message = "Invalid username or password.";
                return null;
            }

            var account = new PlayerAccount
            {
                PlayerId = reader.GetInt32("PlayerID"),
                Username = reader.GetString("Username"),
                FullName = reader.GetString("FullName"),
                LastLogin = reader.IsDBNull("LastLogin") ? null : reader.GetDateTime("LastLogin")
            };
            reader.Close();

            const string updateLogin = "UPDATE Players SET LastLogin = NOW() WHERE PlayerID = @playerId;";
            using var updateCmd = new MySqlCommand(updateLogin, connection);
            updateCmd.Parameters.AddWithValue("@playerId", account.PlayerId);
            updateCmd.ExecuteNonQuery();

            return account;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return null;
        }
    }

    public bool RecordMatch(int playerId, OpponentType opponentType, DifficultyLevel? difficulty, MatchResult result, int movesCount, int? durationSeconds, out string message)
    {
        message = string.Empty;

        try
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            using var tx = connection.BeginTransaction();

            const string insertMatch = @"
INSERT INTO MatchHistory (PlayerID, OpponentType, Difficulty, Result, MovesCount, DurationSeconds)
VALUES (@playerId, @opponentType, @difficulty, @result, @movesCount, @durationSeconds);";

            using var historyCmd = new MySqlCommand(insertMatch, connection, tx);
            historyCmd.Parameters.AddWithValue("@playerId", playerId);
            historyCmd.Parameters.AddWithValue("@opponentType", opponentType.ToString());
            historyCmd.Parameters.AddWithValue("@difficulty", difficulty?.ToString());
            historyCmd.Parameters.AddWithValue("@result", result.ToString());
            historyCmd.Parameters.AddWithValue("@movesCount", movesCount);
            historyCmd.Parameters.AddWithValue("@durationSeconds", durationSeconds.HasValue ? durationSeconds.Value : DBNull.Value);
            historyCmd.ExecuteNonQuery();

            const string updateStatsWin = @"
UPDATE Stats
SET EasyWins = EasyWins + @easy,
    NormalWins = NormalWins + @normal,
    HardWins = HardWins + @hard,
    TotalWins = TotalWins + 1,
    BestWinSeconds = CASE
        WHEN @trackTime = 0 THEN BestWinSeconds
        WHEN BestWinSeconds IS NULL THEN @durationSeconds
        WHEN @durationSeconds IS NULL THEN BestWinSeconds
        WHEN @durationSeconds < BestWinSeconds THEN @durationSeconds
        ELSE BestWinSeconds
    END
WHERE PlayerID = @playerId;";

            const string updateStatsLoss = @"
UPDATE Stats
SET TotalLosses = TotalLosses + 1
WHERE PlayerID = @playerId;";

            if (result == MatchResult.Win)
            {
                int easy = difficulty == DifficultyLevel.Easy ? 1 : 0;
                int normal = difficulty == DifficultyLevel.Normal ? 1 : 0;
                int hard = difficulty == DifficultyLevel.Hard ? 1 : 0;
                int trackTime = opponentType == OpponentType.Computer && durationSeconds.HasValue ? 1 : 0;

                using var statsCmd = new MySqlCommand(updateStatsWin, connection, tx);
                statsCmd.Parameters.AddWithValue("@easy", easy);
                statsCmd.Parameters.AddWithValue("@normal", normal);
                statsCmd.Parameters.AddWithValue("@hard", hard);
                statsCmd.Parameters.AddWithValue("@trackTime", trackTime);
                statsCmd.Parameters.AddWithValue("@durationSeconds", durationSeconds.HasValue ? durationSeconds.Value : DBNull.Value);
                statsCmd.Parameters.AddWithValue("@playerId", playerId);
                statsCmd.ExecuteNonQuery();
            }
            else if (result == MatchResult.Loss)
            {
                using var statsCmd = new MySqlCommand(updateStatsLoss, connection, tx);
                statsCmd.Parameters.AddWithValue("@playerId", playerId);
                statsCmd.ExecuteNonQuery();
            }

            tx.Commit();
            return true;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return false;
        }
    }

    public bool RecordSeries(int playerId, int bestOf, int wins, int losses, MatchResult finalResult, out string message)
    {
        message = string.Empty;

        try
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            const string insertSeries = @"
INSERT INTO MatchSeries (PlayerID, BestOf, Wins, Losses, FinalResult)
VALUES (@playerId, @bestOf, @wins, @losses, @result);";

            using var cmd = new MySqlCommand(insertSeries, connection);
            cmd.Parameters.AddWithValue("@playerId", playerId);
            cmd.Parameters.AddWithValue("@bestOf", bestOf);
            cmd.Parameters.AddWithValue("@wins", wins);
            cmd.Parameters.AddWithValue("@losses", losses);
            cmd.Parameters.AddWithValue("@result", finalResult.ToString());
            cmd.ExecuteNonQuery();

            return true;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return false;
        }
    }

    public bool RemoveSampleData(out string message)
    {
        message = string.Empty;

        const string cleanupSql = @"
DELETE mh
FROM MatchHistory mh
INNER JOIN Players p ON p.PlayerID = mh.PlayerID
WHERE p.Username = 'player1' AND p.FullName = 'Juan Dela Cruz' AND p.Password = '123456';

DELETE ms
FROM MatchSeries ms
INNER JOIN Players p ON p.PlayerID = ms.PlayerID
WHERE p.Username = 'player1' AND p.FullName = 'Juan Dela Cruz' AND p.Password = '123456';

DELETE s
FROM Stats s
INNER JOIN Players p ON p.PlayerID = s.PlayerID
WHERE p.Username = 'player1' AND p.FullName = 'Juan Dela Cruz' AND p.Password = '123456';

DELETE FROM Players
WHERE Username = 'player1' AND FullName = 'Juan Dela Cruz' AND Password = '123456';";

        try
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var cmd = new MySqlCommand(cleanupSql, connection);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return false;
        }
    }

    public DataTable GetLeaderboard(out string message)
    {
        message = string.Empty;
        DataTable table = new();

        try
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            const string query = @"
SELECT p.Username,
       p.FullName,
       s.EasyWins,
       s.NormalWins,
       s.HardWins,
       s.TotalWins,
       s.TotalLosses,
       ROUND((s.TotalWins / NULLIF((s.TotalWins + s.TotalLosses), 0)) * 100, 2) AS WinRate,
       CASE
           WHEN s.BestWinSeconds IS NULL THEN '-'
           ELSE TIME_FORMAT(SEC_TO_TIME(s.BestWinSeconds), '%H:%i:%s')
       END AS BestWinTime
FROM Players p
JOIN Stats s ON p.PlayerID = s.PlayerID
ORDER BY s.HardWins DESC, s.TotalWins DESC, WinRate DESC, s.BestWinSeconds ASC;";

            using var adapter = new MySqlDataAdapter(query, connection);
            adapter.Fill(table);
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }

        return table;
    }
}
