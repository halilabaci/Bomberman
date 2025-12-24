using UnityEngine;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject leaderboardPanel;

    public Transform contentParent;   // ScrollView/Viewport/Content
    public GameObject rowPrefab;      // LeaderboardRow prefab (TMP_Text)
    public int limit = 10;

    private GameDataService data;

    private void Awake()
    {
        data = new GameDataService();
    }

    public void OpenLeaderboard()
    {
        mainMenuPanel.SetActive(false);
        leaderboardPanel.SetActive(true);
        Refresh();
    }

    public void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void Refresh()
    {
        // temizle
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        var list = data.GetLeaderboardTop(limit);

        int rank = 1;
        foreach (var e in list)
        {
            var go = Instantiate(rowPrefab, contentParent);
            var txt = go.GetComponent<TMP_Text>();
            txt.text = $"{rank}. {e.Username}  W:{e.Wins}  L:{e.Losses}  T:{e.TotalGames}";
            rank++;
        }
    }
}
