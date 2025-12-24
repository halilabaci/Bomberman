using SQLite;

public class GameStatsRow
{
    [PrimaryKey]
    public int UserId { get; set; } // UserRow.Id

    public int Wins { get; set; }
    public int Losses { get; set; }
    public int TotalGames { get; set; }
}
