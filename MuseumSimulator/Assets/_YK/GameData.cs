using UnityEngine;
using System.Collections.Generic;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    [Header("트럭 레벨 상태")]
    public int truckLevel = 1;
    public int maxTruckLevel = 5; 

    [Header("⚙️ 레벨별 업그레이드 비용 설정")]
    public List<int> upgradeCosts = new List<int> { 10000, 25000, 50000, 100000 };

    [Header("🚚 트럭 생성 설정")]
    [Tooltip("체크하면 현재 씬을 로비(상점)로 인식하여 운전 및 탑승을 금지합니다. 주행 씬에서는 체크를 해제하세요.")]
    public bool isLobbyScene = false;

    [Tooltip("레벨별 트럭 프리팹들을 순서대로 등록하세요. (Element 0 = 1레벨 트럭)")]
    public List<GameObject> truckPrefabs = new List<GameObject>();

    [Tooltip("현재 씬에서 트럭이 소환될 빈 오브젝트(Transform)를 연결하세요.")]
    public Transform truckSpawnPoint;

    // 현재 씬에 동적으로 생성된 트럭 오브젝트의 원본 추적용
    private GameObject currentSpawnedTruckObject = null;

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

    private void Start()
    {
        // 💡 씬이 처음 시작될 때 현재 레벨에 맞는 트럭을 자동으로 마당/주차장에 스폰합니다.
        RefreshTruckDisplay();
    }

    /// <summary>
    /// ⭐ 현재 레벨의 트럭을 동적으로 소환하고, 씬 모드(로비/주행)에 맞춰 안전 장치를 연동하는 스폰 핵심 메서드
    /// </summary>
    public void RefreshTruckDisplay()
    {
        // 1. 기존에 이미 소환되어 서 있던 트럭이 있다면 깔끔하게 파괴
        if (currentSpawnedTruckObject != null)
        {
            Destroy(currentSpawnedTruckObject);
        }

        // 2. 인덱스 가공 (레벨 1 -> 인덱스 0)
        int targetIndex = truckLevel - 1;

        // 3. 예외 처리를 거친 안전한 프리팹 스폰 진행
        if (targetIndex >= 0 && targetIndex < truckPrefabs.Count)
        {
            if (truckSpawnPoint != null)
            {
                // 트럭 소환
                currentSpawnedTruckObject = Instantiate(truckPrefabs[targetIndex], truckSpawnPoint.position, truckSpawnPoint.rotation);

                // 4. ⭐ 소환된 트럭 내부의 모든 운전/탑승 제어 장치로 씬 세팅값(isLobbyScene)을 강제 주입
                ConfigureTruckComponents();

                Debug.Log($"<color=lime>[GameData] {truckLevel}레벨 트럭 동적 스폰 완료! (로비여부: {isLobbyScene})</color>");
            }
            else
            {
                Debug.LogWarning("GameData: 트럭이 소환될 위치(TruckSpawnPoint)가 인스펙터에 등록되지 않았습니다.");
            }
        }
        else
        {
            Debug.LogError($"GameData: {truckLevel}레벨에 매칭되는 트럭 프리팹이 리스트에 없습니다.");
        }
    }

    /// <summary>
    /// 소환된 트럭의 주행 스크립트(TruckController)와 상호작용(Truck) 스크립트에 씬 상태를 주입합니다.
    /// </summary>
    private void ConfigureTruckComponents()
    {
        if (currentSpawnedTruckObject == null) return;
    }

    /// <summary>
    /// ⭐ 메인 상점 UI의 [트럭 업그레이드 버튼]에 이 함수를 연결하세요!
    /// </summary>
    public void TryUpgradeTruck()
    {
        if (truckLevel >= maxTruckLevel)
        {
            Debug.LogWarning("GameData: 이미 트럭이 최고 레벨입니다!");
            return;
        }

        int costIndex = truckLevel - 1;
        int currentUpgradeCost = 0;

        if (costIndex >= 0 && costIndex < upgradeCosts.Count)
        {
            currentUpgradeCost = upgradeCosts[costIndex];
        }
        else
        {
            Debug.LogError($"GameData: 비용 인덱스 범위 초과!");
            return;
        }

        if (MoneyManager.Instance == null)
        {
            Debug.LogError("GameData: MoneyManager가 씬에 없습니다.");
            return;
        }

        // 재화 검증 진행
        if (MoneyManager.Instance.ConsumeMoney(currentUpgradeCost))
        {
            truckLevel++;              
            Debug.Log($"<color=yellow>GameData: 업그레이드 성공! 새로운 트럭 레벨: {truckLevel}</color>");

            // ⭐ [단순화의 핵심] 다른 컴포넌트를 찾지 않고, 자신의 메서드를 즉시 다시 호출하여 실시간 외형 교체 수행!
            RefreshTruckDisplay();
        }
        else
        {
            Debug.LogWarning("GameData: 잔액 부족으로 인한 업그레이드 실패");
        }
    }

    public int GetTruckLevel()
    {
        return truckLevel;
    }
}