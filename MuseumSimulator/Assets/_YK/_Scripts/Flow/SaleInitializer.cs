using UnityEngine;
using System.Collections.Generic;

public class SaleInitializer : MonoBehaviour
{
    [Header("레벨별 주방 인테리어 프리팹 리스트")]
    [Tooltip("프로젝트 창에 있는 레벨별 주방 프리팹을 순서대로 넣으세요.\n(Element 0 = 1레벨 주방, Element 1 = 2레벨 주방)")]
    public List<GameObject> kitchenPrefabs = new List<GameObject>();

    [Header("⭐ 인테리어 스폰 위치 설정")]
    [Tooltip("주방 인테리어 프리팹이 통째로 소환될 월드 좌표입니다. (씬에 생성한 빈 오브젝트 연결)")]
    public Transform kitchenSpawnPoint;

    [Header("1인칭 플레이어 스폰 위치 (선택사항)")]
    [Tooltip("주방 내부에서 플레이어가 시작할 위치(빈 오브젝트)를 지정하세요. 없으면 기본 위치에 스폰됩니다.")]
    public Transform playerSpawnPoint;

    void Start()
    {
        // 1. 씬에 전역 데이터 보관함(GameData)이 존재하는지 철저히 검사
        if (GameData.Instance == null)
        {
            Debug.LogError("SaleInitializer: GameData 보관함이 씬에 없습니다! 게임을 최초 시작 씬부터 실행해주세요.");
            return;
        }

        // 2. 보관함에 저장되어 있는 현재 트럭 레벨 가져오기
        int currentLevel = GameData.Instance.truckLevel;
        
        // 리스트 인덱스(0부터 시작)에 맞추기 위해 레벨에서 1을 뺍니다. (레벨1 = 인덱스0)
        int targetIndex = currentLevel - 1;

        // 3. 인스펙터 리스트 범위를 벗어나지 않았는지 안전성 검사 후 인테리어 소환
        if (targetIndex >= 0 && targetIndex < kitchenPrefabs.Count)
        {
            if (kitchenPrefabs[targetIndex] != null)
            {
                // 💡 [수정] 인스펙터에서 지정한 위치와 회전값을 가져옵니다. 
                // 만약 빈칸(null)이라면 기존처럼 월드 중앙(0,0,0)에 기본 스폰합니다.
                Vector3 spawnPos = kitchenSpawnPoint != null ? kitchenSpawnPoint.position : Vector3.zero;
                Quaternion spawnRot = kitchenSpawnPoint != null ? kitchenSpawnPoint.rotation : Quaternion.identity;

                // 지정된 위치에 맞춤형 주방 인테리어 소환!
                GameObject spawnedKitchen = Instantiate(kitchenPrefabs[targetIndex], spawnPos, spawnRot);
                
                Debug.Log($"<color=cyan>[판매 씬 가동] 현재 트럭 레벨 {currentLevel}에 맞는 주방 인테리어가 지정된 위치에 생성되었습니다.</color>");
                
                // 4. 주방 내부 시작 지점으로 1인칭 플레이어 위치 정렬
                AlignPlayerPosition();
            }
            else
            {
                Debug.LogError($"SaleInitializer: 리스트의 {targetIndex}번 자리가 비어있습니다. 프리팹을 다시 확인해주세요.");
            }
        }
        else
        {
            Debug.LogError($"SaleInitializer: 현재 트럭 레벨은 {currentLevel}층이지만, 이에 매핑된 주방 프리팹이 인스펙터 리스트에 등록되지 않았습니다! (요구 인덱스: {targetIndex})");
        }
    }

    private void AlignPlayerPosition()
    {
        // 씬에서 1인칭 플레이어 오브젝트를 찾습니다.
        GameObject player = GameObject.FindWithTag("Player");
        
        if (player != null && playerSpawnPoint != null)
        {
            // 플레이어를 활성화하기 전에 주방 안의 정해진 올바른 좌표로 강제 텔레포트 시킵니다.
            player.transform.position = playerSpawnPoint.position;
            player.transform.rotation = playerSpawnPoint.rotation;
            Debug.Log("[SaleInitializer] 1인칭 플레이어를 주방 내부 스폰 포인트로 정렬했습니다.");
        }
    }
}