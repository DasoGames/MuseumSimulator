using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    // 💡 유니티 인스펙터에서 각 기계의 업그레이드 스펙을 개별 세팅하는 구조체
    [System.Serializable]
    public struct UpgradeData
    {
        public string machineID;            // 기계 고유 ID (ex: "ChoppingBoard", "CookingPot", "Fryer")
        public int currentLevel;            // 현재 업그레이드 레벨 (초기값 주로 1)
        public float speedModifierPerLevel; // 레벨당 조리 시간 단축 비율 (ex: 0.1이면 레벨당 10%씩 단축)
    }

    [Header("모든 조리기구 업그레이드 데이터 리스트")]
    [SerializeField] private List<UpgradeData> upgradeSetupList = new List<UpgradeData>();

    // 빠른 데이터 조회를 위한 실시간 사전(Dictionary) 데이터 공간
    private Dictionary<string, UpgradeData> upgradeDictionary = new Dictionary<string, UpgradeData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 씬이 변경되어도 파괴되지 않고 유지되길 원한다면 아래 주석을 해제하세요.
            // DontDestroyOnLoad(gameObject);
            InitDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 게임 시작 시 인스펙터의 리스트 데이터를 딕셔너리로 깔끔하게 파싱합니다.
    private void InitDictionary()
    {
        foreach (var data in upgradeSetupList)
        {
            if (!upgradeDictionary.ContainsKey(data.machineID))
            {
                upgradeDictionary.Add(data.machineID, data);
            }
            else
            {
                Debug.LogWarning($"[UpgradeManager] 중복된 Machine ID가 발견되었습니다: {data.machineID}");
            }
        }
    }

    /// <summary>
    /// ⭐ [독립 기구 연동용 핵심 함수] 
    /// 기계가 자신의 ID를 던지면 업그레이드 레벨이 계산된 '시간 단축 비율(Modifier)'을 곧바로 반환합니다.
    /// </summary>
    /// <param name="machineID">기계의 고유 문자열 식별자</param>
    /// <returns>최종 시간 곱셈 비율 (단축이 없을 시 1.0f)</returns>
    public float GetSpeedModifier(string machineID)
    {
        if (upgradeDictionary.TryGetValue(machineID, out UpgradeData data))
        {
            // 공식: 1 - ((현재 레벨 - 1) * 레벨당 단축 비율)
            // 예시: 2레벨이고 10% 단축이면 -> 1 - (1 * 0.1) = 0.9 (시간이 90%로 단축됨)
            float modifier = 1f - ((data.currentLevel - 1) * data.speedModifierPerLevel);
            
            // 아무리 업그레이드를 많이 해도 원래 시간의 최소 20% 밑으로는 내려가지 않도록 안전 방어선 구축
            return Mathf.Max(modifier, 0.2f); 
        }

        // 인스펙터 리스트에 등록되지 않은 기계라면 기본 속도(1.0)를 반환하여 에러를 방지합니다.
        return 1f;
    }

    /// <summary>
    /// 상점 UI나 업그레이드 버튼을 눌렀을 때 특정 기계의 레벨을 1 올려주는 함수입니다.
    /// </summary>
    public void UpgradeMachine(string machineID)
    {
        if (upgradeDictionary.TryGetValue(machineID, out UpgradeData data))
        {
            data.currentLevel++;
            upgradeDictionary[machineID] = data; // 딕셔너리 값 최신화
            Debug.Log($"[Upgrade] '{machineID}' 기계가 {data.currentLevel} 레벨로 업그레이드되었습니다!");
        }
        else
        {
            Debug.LogError($"[UpgradeManager] '{machineID}'는 존재하지 않는 기계 ID라 업그레이드할 수 없습니다.");
        }
    }
}