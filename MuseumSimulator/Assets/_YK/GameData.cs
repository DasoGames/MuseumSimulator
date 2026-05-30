using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    [Header("트럭 레벨 상태")]
    public int truckLevel = 1; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 💡 씬이 전환되어도 파괴되지 않음
        }
        else
        {
            Destroy(gameObject);
        }
    }
}