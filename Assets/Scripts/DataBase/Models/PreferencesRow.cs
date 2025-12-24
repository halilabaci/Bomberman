using SQLite;

public class PreferencesRow
{
    [PrimaryKey]
    public int UserId { get; set; }

    public string SelectedTheme { get; set; } = "desert";
}
