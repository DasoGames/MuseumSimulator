using UnityEngine;
using System.Collections.Generic;

public class ChickenDisplay : MonoBehaviour, IInteractable
{
    [Header("진열대 설정")]
    public int maxStorage = 10;         // 진열대에 최대로 쌓아둘 수 있는 치킨 개수
    
    [Header("결과물 데이터")]
    // 💡 기존 HoldableObject 구조체 대신 스크립터블 오브젝트인 FoodData를 사용합니다!
    [Tooltip("진열대에서 빈손으로 꺼낼 때 손님에게 판매할 테이크아웃 치킨컵 에셋")]
    public FoodData chickenCupData; // 에셋 이름: "치킨컵" 또는 "닭강정컵"

    [Header("현재 재고 상태 (Debug)")]
    [SerializeField] private int currentStock = 0; // 현재 진열된 치킨 수량

    [Header("시각적 연출용 목록 (선택사항)")]
    // 치킨 재고 수량(0~10개)에 맞춰 하나씩 켜고 꺼줄 3D 프리팹 오브젝트 리스트
    public List<GameObject> chickenVisuals = new List<GameObject>();

    private void Start()
    {
        UpdateVisuals();
    }

    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // 🔥 [상황 1] 플레이어가 물건을 들고 있을 때 -> 진열대에 치킨 추가
        if (player.IsHoldingItem)
        {
            // 💡 구조체 형식을 지우고 순수 FoodData 참조로 변경합니다.
            FoodData heldData = player.CurrentHeldData;

            // 💡 데이터 규칙에 맞춰 'foodName' 필드로 필터링합니다.
            // 튀김기에서 방금 건져낸 "완성된치킨" 데이터만 받습니다.
            if (heldData.foodName == "완성된치킨")
            {
                if (currentStock < maxStorage)
                {
                    currentStock++;
                    player.ClearHand(); // 플레이어 손에 든 치킨 비우기
                    
                    UpdateVisuals();
                    Debug.Log($"치킨진열대: 튀겨진 치킨을 진열했습니다. (현재 재고: {currentStock} / {maxStorage}개)");
                }
                else
                {
                    Debug.LogWarning("치킨진열대: 진열대가 가득 차서 더 이상 채울 수 없습니다!");
                }
            }
            else if (heldData.foodName == "탄치킨")
            {
                Debug.LogWarning("치킨진열대: 탄 치킨은 진열할 수 없습니다. 쓰레기통에 버리세요!");
            }
            else
            {
                Debug.LogWarning("치킨진열대: 완성된 치킨만 진열할 수 있습니다.");
            }
            return;
        }

        // 🔥 [상황 2] 플레이어가 빈손일 때 -> 손님 지급용 낱개 치킨컵 꺼내기
        if (!player.IsHoldingItem)
        {
            if (currentStock > 0)
            {
                if (chickenCupData == null)
                {
                    Debug.LogError("치킨진열대: 'chickenCupData' 에셋이 인스펙터에 연결되지 않았습니다!");
                    return;
                }

                currentStock--; // 재고 1개 차감
                
                // 💡 플레이어 빈손에 테이크아웃 컵 FoodData 에셋을 쥐여줍니다.
                player.HoldNewData(chickenCupData); 
                
                UpdateVisuals();
                Debug.Log($"치킨진열대: 판매용 치킨컵을 1개 꺼냈습니다. (남은 재고: {currentStock}개)");
            }
            else
            {
                Debug.Log("치킨진열대: 재고가 없습니다! 튀김기에서 치킨을 더 튀겨오세요.");
            }
        }
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < chickenVisuals.Count; i++)
        {
            if (chickenVisuals[i] != null)
            {
                chickenVisuals[i].SetActive(i < currentStock);
            }
        }
    }
}