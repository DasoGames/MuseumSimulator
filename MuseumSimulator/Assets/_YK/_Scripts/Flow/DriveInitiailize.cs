using UnityEngine;
using System.Collections.Generic;

public class DriveInitializer : MonoBehaviour
{
    [Header("레벨별 주행용 트럭 프리팹 리스트")]
    [Tooltip("프로젝트 창의 트럭 프리팹들을 레벨 순서대로 넣으세요. (Element 0 = 1레벨 트럭)")]
    public List<GameObject> truckPrefabs = new List<GameObject>();
    
    [Header("트럭이 스폰될 주차장 위치")]
    [Tooltip("씬에 배치한 빈 오브젝트(SpawnPoint)를 연결하세요.")]
    public Transform truckSpawnPoint;

    void Start()
    {
        // 1. 데이터 보관함(GameData)이 있는지 체크
        if (GameData.Instance == null)
        {
            Debug.LogError("DriveInitializer: GameData가 씬에 없습니다! 최초 시작 씬부터 게임을 실행해 주세요.");
            return;
        }

        // 2. 현재 트럭 레벨을 인덱스 번호(0부터 시작)로 변환
        int currentLevel = GameData.Instance.truckLevel;
        int targetIndex = currentLevel - 1;

        // 3. 리스트 범위를 벗어나지 않는지 안전 검사 후 해당 트럭만 소환
        if (targetIndex >= 0 && targetIndex < truckPrefabs.Count)
        {
            if (truckSpawnPoint != null)
            {
                // 지정된 위치와 회전값으로 트럭 프리팹 동적 생성
                GameObject spawnedTruck = Instantiate(truckPrefabs[targetIndex], truckSpawnPoint.position, truckSpawnPoint.rotation);
                Debug.Log($"<color=lime>[운전 씬] {currentLevel}레벨 외형 트럭 생성 완료!</color>");
            }
            else
            {
                Debug.LogError("DriveInitializer: 트럭이 소환될 주차장 위치(TruckSpawnPoint)가 지정되지 않았습니다!");
            }
        }
        else
        {
            Debug.LogError($"DriveInitializer: {currentLevel}레벨에 해당하는 트럭 프리팹이 인스펙터 리스트에 없습니다!");
        }
    }
}