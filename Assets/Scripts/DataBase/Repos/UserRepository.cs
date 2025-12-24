using System;
using System.Linq;

public class UserRepository
{
    public bool UsernameExists(string username)
    {
        return DbManager.DB.Table<UserRow>().Any(u => u.Username == username);
    }

    public UserRow GetByUsername(string username)
    {
        return DbManager.DB.Table<UserRow>().FirstOrDefault(u => u.Username == username);
    }

    public UserRow Register(string username, string passwordPlain)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(passwordPlain))
            return null;

        if (UsernameExists(username)) return null;

        var user = new UserRow
        {
            Username = username.Trim(),
            PasswordHash = PasswordHasher.Sha256(passwordPlain),
            CreatedAtIso = DateTime.UtcNow.ToString("o")
        };

        DbManager.DB.Insert(user);

        // Stats init
        DbManager.DB.Insert(new GameStatsRow
        {
            UserId = user.Id,
            Wins = 0,
            Losses = 0,
            TotalGames = 0
        });

        return user;
    }

    public UserRow Login(string username, string passwordPlain)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(passwordPlain))
            return null;

        var user = GetByUsername(username.Trim());
        if (user == null) return null;

        var hash = PasswordHasher.Sha256(passwordPlain);
        return user.PasswordHash == hash ? user : null;
    }
}
