using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("재화 설정")]
    [SerializeField] private int currentMoney = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void AddMoney(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentMoney += amount;
        Debug.Log($"돈 획득: +{amount}원 | 현재 잔액: {currentMoney}원");
        
    }
    public bool ConsumeMoney(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("0 이하의 돈은 소비할 수 없습니다.");
            return false;
        }

        // 잔액이 부족한 경우
        if (currentMoney < amount)
        {
            Debug.LogWarning($"잔액이 부족합니다! 부족한 금액: {amount - currentMoney}원");
            return false; // 구매 실패를 알림
        }

        // 잔액이 충분한 경우 차감
        currentMoney -= amount;
        Debug.Log($"돈 소비: -{amount}원 | 현재 잔액: {currentMoney}원");
        
        // 여기에 UI 업데이트 함수 등을 연결하면 좋습니다.
        return true; // 구매 성공을 알림
    }
    public int getMoney()
    {
        return currentMoney;
    }
}