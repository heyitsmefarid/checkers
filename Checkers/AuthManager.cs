namespace Checkers;

public class AuthManager
{
    private readonly DatabaseManager _database;

    public AuthManager(DatabaseManager database)
    {
        _database = database;
    }

    public bool Register(string username, string fullName, string password, out string message)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(password))
        {
            message = "All fields are required.";
            return false;
        }

        return _database.Register(username.Trim(), fullName.Trim(), password, out message);
    }

    public PlayerAccount? Login(string username, string password, out string message)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            message = "Username and password are required.";
            return null;
        }

        return _database.Login(username.Trim(), password, out message);
    }
}
