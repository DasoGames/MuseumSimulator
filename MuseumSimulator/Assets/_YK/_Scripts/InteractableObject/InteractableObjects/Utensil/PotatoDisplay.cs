using UnityEngine;
using System.Collections.Generic;

public class PotatoDisplay : MonoBehaviour, IInteractable
{
    [Header("진열대 설정")]
    public int maxStorage = 10;         // 진열대에 최대로 쌓아둘 수 있는 감자튀김 개수
    
    [Header("결과물 데이터")]
    // 💡 기존 HoldableObject 구조체 대신 스크립터블 오브젝트인 FoodData를 사용합니다!
    [Tooltip("진열대에서 빈손으로 꺼낼 때 손님에게 판매할 테이크아웃 감자튀김컵 에셋")]
    public FoodData potatoCupData; // 에셋 이름: "감자튀김컵"

    [Header("현재 재고 상태 (Debug)")]
    [SerializeField] private int currentStock = 0; // 현재 진열된 감자튀김 수량

    [Header("시각적 연출용 목록 (선택사항)")]
    // 감자튀김 재고 수량에 맞춰 하나씩 켜고 꺼줄 3D 프리팹 오브젝트 리스트
    public List<GameObject> potatoVisuals = new List<GameObject>();

    private void Start()
    {
        UpdateVisuals();
    }

    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // 🔥 [상황 1] 플레이어가 물건을 들고 있을 때 -> 진열대에 감자튀김 추가
        if (player.IsHoldingItem)
        {
            // 💡 구조체 형식을 지우고 순수 FoodData 참조로 가져옵니다.
            FoodData heldData = player.CurrentHeldData;

            // 💡 데이터 규칙에 맞게 'foodName' 필드로 필터링을 수행합니다.
            // 튀김기에서 방금 건져낸 "완성된감자튀김" 데이터만 받습니다.
            if (heldData.foodName == "완성된감자튀김")
            {
                if (currentStock < maxStorage)
                {
                    currentStock++;
                    player.ClearHand(); // 플레이어 손에 든 감자튀김 비우기
                    
                    UpdateVisuals();
                    Debug.Log($"감자진열대: 튀겨진 감자튀김을 진열했습니다. (현재 재고: {currentStock} / {maxStorage}개)");
                }
                else
                {
                    Debug.LogWarning("감자진열대: 진열대가 가득 차서 더 이상 채울 수 없습니다!");
                }
            }
            else if (heldData.foodName == "탄감자튀김")
            {
                Debug.LogWarning("감자진열대: 탄 감자튀김은 진열할 수 없습니다. 쓰레기통에 버리세요!");
            }
            else
            {
                Debug.LogWarning("감자진열대: 완성된 감자튀김만 진열할 수 있습니다.");
            }
            return;
        }

        // 🔥 [상황 2] 플레이어가 빈손일 때 -> 손님 지급용 낱개 감자튀김컵 꺼내기
        if (!player.IsHoldingItem)
        {
            if (currentStock > 0)
            {
                if (potatoCupData == null)
                {
                    Debug.LogError("감자진열대: 'potatoCupData' 에셋이 인스펙터에 연결되지 않았습니다!");
                    return;
                }

                currentStock--; // 재고 1개 차감
                
                // 💡 플레이어 빈손에 완제품 감자튀김컵 FoodData 에셋을 안전하게 쥐여줍니다.
                player.HoldNewData(potatoCupData); 
                
                UpdateVisuals();
                Debug.Log($"감자진열대: 판매용 감자튀김컵을 1개 꺼냈습니다. (남은 재고: {currentStock}개)");
            }
            else
            {
                Debug.Log("감자진열대: 재고가 없습니다! 튀김기에서 감자를 더 튀겨오세요.");
            }
        }
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < potatoVisuals.Count; i++)
        {
            if (potatoVisuals[i] != null)
            {
                potatoVisuals[i].SetActive(i < currentStock);
            }
        }
    }
}