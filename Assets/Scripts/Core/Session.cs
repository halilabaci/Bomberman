public static class Session
{
    public static int CurrentUserId { get; private set; } = -1;
    public static string CurrentUsername { get; private set; } = "";

    public static bool IsLoggedIn => CurrentUserId > 0;

    public static void SetUser(int id, string username)
    {
        CurrentUserId = id;
        CurrentUsername = username;
    }

    public static void Clear()
    {
        CurrentUserId = -1;
        CurrentUsername = "";
    }
}
