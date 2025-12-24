using UnityEngine;

public class DbBoot : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log("[DbBoot] Awake called");
        DbManager.Init();
    }
}
