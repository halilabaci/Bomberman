using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkHandler : MonoBehaviour
{
    [Header("UI Ana Panelleri")]
    public GameObject modeSelectionUI; // Single mý Multi mi seçtiðimiz yer
    public GameObject singlePlayerUI;  // Play butonunun olduðu grup
    public GameObject multiplayerUI;   // Host/Client/Theme Selection grubu

    [Header("Multiplayer Detay")]
    public GameObject loginPanel;      // Host/Client butonlarýnýn olduðu yer
    public GameObject themeSelection;  // Sadece Host'un göreceði tema seçimi

    void Start()
    {
        // Sahne açýldýðýnda her þeyi kapat, sadece ilk seçimi aç
        ShowModeSelection();
    }

    public void ShowModeSelection()
    {
        modeSelectionUI.SetActive(true);
        singlePlayerUI.SetActive(false);
        multiplayerUI.SetActive(false);
    }

    // "TEK OYUNCU" butonuna basýnca çalýþacak
    public void SelectSinglePlayer()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            modeSelectionUI.SetActive(false);
            singlePlayerUI.SetActive(true);
            multiplayerUI.SetActive(false); // Multiplayer kýsmýný tamamen kapat ki önümüz açýlsýn!
            Debug.Log("Single Player Modu Aktif.");
        }
    }

    // "ÇOK OYUNCULU" butonuna basýnca çalýþacak
    public void SelectMultiplayer()
    {
        modeSelectionUI.SetActive(false);
        multiplayerUI.SetActive(true);
        loginPanel.SetActive(true);      // Önce Host/Client butonlarý gelsin
        themeSelection.SetActive(false);  // Tema seçimi henüz gizli kalsýn
    }

    // MULTIPLAYER'DA "HOST OL" butonuna basýnca çalýþacak
    public void StartHostAndShowThemes()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            loginPanel.SetActive(false);    // Giriþ ekranýný kapat
            themeSelection.SetActive(true); // Tema seçme ekranýný aç
            Debug.Log("Host Olundu, Tema Seçiliyor...");
        }
    }

    // MULTIPLAYER'DA "CLIENT OL" butonuna basýnca çalýþacak
    public void StartClientOnly()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client olarak baðlanýlýyor...");
    }

    // TEMALARDAN BÝRÝ SEÇÝLÝNCE (Örn: Desert)
    public void LoadDesert()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Level_Desert", LoadSceneMode.Single);
    }
}