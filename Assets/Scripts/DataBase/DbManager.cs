using System.IO;
using UnityEngine;
using SQLite;

public static class DbManager
{
    private static SQLiteConnection _db;

    public static SQLiteConnection DB
    {
        get
        {
            if (_db == null) Init();
            return _db;
        }
    }

    public static void Init()
    {
        if (_db != null) return; // ✅ tekrar init olmasın

        var dbPath = Path.Combine(Application.persistentDataPath, "game.db");
        _db = new SQLiteConnection(dbPath);

        _db.CreateTable<UserRow>();
        _db.CreateTable<GameStatsRow>();
        _db.CreateTable<PreferencesRow>(); // ✅ doğru tablo

        Debug.Log($"[DB] Ready: {dbPath}");
    }

    public static void Close()
    {
        _db?.Close();
        _db = null;
    }
}
