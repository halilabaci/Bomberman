using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public static class LevelLoader
{
    /// <summary>
    /// Netcode session varsa (Host/Client baþladýysa) SADECE SERVER sahne yükler.
    /// Server deðilsek yerel SceneManager ile yüklemeyiz (desync olur).
    /// </summary>
    public static bool LoadLevel(string sceneName)
    {
        var nm = NetworkManager.Singleton;

        // Netcode yoksa normal load
        if (nm == null || !nm.IsListening)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            return true;
        }

        // Netcode varsa sahne yüklemeyi server yapmalý
        if (!nm.IsServer)
        {
            Debug.LogWarning($"[LevelLoader] Client side scene load blocked. Server must load: {sceneName}");
            return false;
        }

        nm.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        return true;
    }
}
