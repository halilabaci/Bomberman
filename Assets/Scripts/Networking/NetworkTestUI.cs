using Unity.Netcode;
using Unity.Netcode.Transports.UTP; // IP ayarý için þart
using TMPro; // TextMeshPro (Input Field) için þart
using UnityEngine;
using UnityEngine.UI;

public class NetworkTestUI : MonoBehaviour
{
    [Header("UI Panelleri")]
    public GameObject networkPanel;      // Host/Client butonlarýnýn olduðu panel
    public GameObject themeSelectionPanel; // Tema (Orman/Çöl) seçme paneli

    [Header("Giriþ Elemanlarý")]
    public TMP_InputField ipInput; // Orta kýsma eklediðin IP kutusu
    public Button hostBtn;
    public Button clientBtn;

    void Start()
    {
        // Buton dinleyicilerini baðlýyoruz
        hostBtn.onClick.AddListener(OnHostButtonClicked);
        clientBtn.onClick.AddListener(OnClientButtonClicked);

        // Baþlangýçta tema paneli kapalý olsun
        if (themeSelectionPanel != null) themeSelectionPanel.SetActive(false);
    }

    public void OnHostButtonClicked()
    {
        // 1. Önce Host olarak baþlatýyoruz
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Host baþlatýldý. Tema seçimi bekleniyor...");
            // 2. Host olunca seçim panelini açýyoruz, giriþ panelini kapatýyoruz
            networkPanel.SetActive(false);
            themeSelectionPanel.SetActive(true);
        }
    }

    public void OnClientButtonClicked()
    {
        // 1. Yazýlan IP'yi alýp transport'a aktarýyoruz
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (!string.IsNullOrEmpty(ipInput.text))
        {
            transport.ConnectionData.Address = ipInput.text;
        }

        // 2. Client olarak baðlanmayý dene
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client olarak IP'ye baðlanýlýyor: " + transport.ConnectionData.Address);
    }

    // Bu fonksiyonu kodun en altýna, LoadSelectedLevel'in hemen üstüne yapýþtýr abi
    public void OnBackButtonClicked()
    {
        // 1. Host olmayý iptal ediyoruz (Baðlantýyý kesiyoruz)
        NetworkManager.Singleton.Shutdown();

        // 2. Panelleri eski haline getiriyoruz
        themeSelectionPanel.SetActive(false);
        networkPanel.SetActive(true);

        Debug.Log("Host iþlemi iptal edildi, ana menüye dönüldü.");
    }

    // TEMA BUTONLARI ÝÇÝN YENÝ FONKSÝYONLAR
    public void LoadSelectedLevel(string levelName)
    {
        Debug.Log("Sahne yükleniyor: " + levelName);
        // Netcode üzerinden herkesi sahneye taþýyan sihirli satýr
        NetworkManager.Singleton.SceneManager.LoadScene(levelName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}