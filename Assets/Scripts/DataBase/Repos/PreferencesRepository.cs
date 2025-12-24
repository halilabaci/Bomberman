public class PreferencesRepository
{
	public PreferencesRow Get(int userId)
	{
		return DbManager.DB.Find<PreferencesRow>(userId);
	}

	public string GetTheme(int userId)
	{
		var row = Get(userId);
		return row?.SelectedTheme ?? "desert";
	}

	public void EnsureDefaults(int userId)
	{
		var row = Get(userId);
		if (row != null) return;

		DbManager.DB.Insert(new PreferencesRow
		{
			UserId = userId,
			SelectedTheme = "desert"
		});
	}

	public void SetTheme(int userId, string theme)
	{
		var row = Get(userId);
		if (row == null)
		{
			row = new PreferencesRow { UserId = userId, SelectedTheme = theme };
			DbManager.DB.Insert(row);
			return;
		}

		row.SelectedTheme = theme;
		DbManager.DB.Update(row);
	}
}
