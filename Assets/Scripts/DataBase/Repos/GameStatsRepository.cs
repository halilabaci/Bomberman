using System.Collections.Generic;
using System.Linq;

public class GameStatsRepository
{
    // 1 user = 1 stats satýrý olacak þekilde tasarlýyoruz.
    // Bunun için GameStatsRow'da [PrimaryKey] public int UserId {get;set;} olmalý.

    public GameStatsRow Get(int userId)
    {
        // PrimaryKey ise en temiz yol:
        return DbManager.DB.Find<GameStatsRow>(userId);
        // Eðer PK deðilse eski yöntemin gerekir:
        // return DbManager.DB.Table<GameStatsRow>().FirstOrDefault(s => s.UserId == userId);
    }

    public void EnsureDefaults(int userId)
    {
        var row = Get(userId);
        if (row != null) return;

        DbManager.DB.Insert(new GameStatsRow
        {
            UserId = userId,
            Wins = 0,
            Losses = 0,
            TotalGames = 0
        });
    }

    public void RecordMatch(int userId, bool isWin)
    {
        EnsureDefaults(userId);

        var stats = Get(userId);
        stats.TotalGames += 1;
        if (isWin) stats.Wins += 1;
        else stats.Losses += 1;

        DbManager.DB.Update(stats);
    }

    public List<LeaderboardEntry> GetTopPlayers(int limit = 10)
    {
        // wins desc, sonra total desc (istersen losses asc da eklenir)
        var topStats = DbManager.DB.Table<GameStatsRow>()
            .OrderByDescending(s => s.Wins)
            .ThenByDescending(s => s.TotalGames)
            .Take(limit)
            .ToList();

        // Username map
        var userMap = DbManager.DB.Table<UserRow>()
            .ToList()
            .ToDictionary(u => u.Id, u => u.Username);

        var result = new List<LeaderboardEntry>(topStats.Count);
        foreach (var s in topStats)
        {
            userMap.TryGetValue(s.UserId, out var uname);

            result.Add(new LeaderboardEntry
            {
                UserId = s.UserId,
                Username = uname ?? $"User{s.UserId}",
                Wins = s.Wins,
                Losses = s.Losses,
                TotalGames = s.TotalGames
            });
        }

        return result;
    }
}

