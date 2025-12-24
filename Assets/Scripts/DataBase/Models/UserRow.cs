using SQLite;

public class UserRow
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public string Username { get; set; }

    public string PasswordHash { get; set; }

    public string CreatedAtIso { get; set; }
}
