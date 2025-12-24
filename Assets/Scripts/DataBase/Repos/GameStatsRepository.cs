using System.Collections.Generic;
using System.Linq;

public class GameStatsRepository
{
    public GameStatsRow GetStats(int userId)
    {
        return DbManager.DB.Table<GameStatsRow>().FirstOrDefault(s => s.UserId == userId);
    }

    public void RecordMatch(int userId, bool isWin)
    {
        var stats = GetStats(userId);
        if (stats == null)
        {
            stats = new GameStatsRow { UserId = userId, Wins = 0, Losses = 0, TotalGames = 0 };
            DbManager.DB.Insert(stats);
        }

        stats.TotalGames++;
        if (isWin) stats.Wins++;
        else stats.Losses++;

        DbManager.DB.Update(stats);
    }

    public List<(string username, int wins, int losses, int total)> GetLeaderboardTop(int limit)
    {
        var users = DbManager.DB.Table<UserRow>().ToList();
        var stats = DbManager.DB.Table<GameStatsRow>().ToList();

        var joined = (from s in stats
                      join u in users on s.UserId equals u.Id
                      orderby s.Wins descending, s.TotalGames descending
                      select (u.Username, s.Wins, s.Losses, s.TotalGames))
                     .Take(limit)
                     .ToList();

        return joined;
    }
}
