using Unity.Netcode;
using UnityEngine;

public class NetworkHandler : MonoBehaviour
{
    // Host (Sen) butonuna basýnca bu çalýþacak
    public void StartHostAndLoadScene()
    {
        // 1. Önce Host (Sunucu) modunu baþlatýyoruz
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Host baþlatýldý, sahne yükleniyor...");

            // 2. TÜM OYUNCULARI beraberinde götürecek olan o özel komut:
            // "Level_Forest" yazan yere hiyerarþideki sahne adýný yaz (Büyük-küçük harf önemli!)
            NetworkManager.Singleton.SceneManager.LoadScene("Level_Desert", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("Host baþlatýlamadý!");
        }
    }

    // Client (Arkadaþýn) butonuna basýnca bu çalýþacak
    public void StartClientOnly()
    {
        // Arkadaþýn sadece baðlanýr, sahne seninle beraber onda da otomatik deðiþir
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client olarak baðlanmaya çalýþýlýyor...");
    }
}