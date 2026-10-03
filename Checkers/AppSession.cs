namespace Checkers;

public static class AppSession
{
    public static DatabaseManager Database { get; } = new();
    public static AuthManager Auth { get; } = new(Database);
    public static PlayerAccount? CurrentPlayer { get; set; }
    public static GameSettings Settings { get; } = new();
    public static MusicManager Music { get; } = new();
}
