using System.Collections.Generic;

public class GameDataService
{
    private readonly UserRepository _users = new UserRepository();
    private readonly GameStatsRepository _stats = new GameStatsRepository();
    private readonly PreferencesRepository _prefs = new PreferencesRepository();

    public UserRow Register(string username, string passwordPlain)
    {
        var user = _users.Register(username, passwordPlain);
        if (user == null) return null;

        // Faz-6 þartý: register sonrasý otomatik satýrlar
        _stats.EnsureDefaults(user.Id);
        _prefs.EnsureDefaults(user.Id);

        return user;
    }

    public UserRow Login(string username, string passwordPlain)
    {
        return _users.Login(username, passwordPlain);
    }

    public void RecordMatch(int userId, bool isWin)
    {
        _stats.RecordMatch(userId, isWin);
    }

    public List<LeaderboardEntry> GetLeaderboardTop(int limit = 10)
    {
        return _stats.GetTopPlayers(limit);
    }

    public string GetTheme(int userId) => _prefs.GetTheme(userId);
    public void SetTheme(int userId, string theme) => _prefs.SetTheme(userId, theme);
}
