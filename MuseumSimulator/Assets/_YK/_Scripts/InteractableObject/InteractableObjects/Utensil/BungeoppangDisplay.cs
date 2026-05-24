using UnityEngine;
using System.Collections.Generic;

public class BungeoppangDisplay : MonoBehaviour, IInteractable
{
    [Header("진열대 설정")]
    public int maxStorage = 30;         // 진열대 최대 수용량 (ex: 10개들이 총 3판분량)
    
    [Header("결과물 데이터")]
    // 진열대에서 빈손으로 꺼낼 때 플레이어 손에 쥐여줄 '낱개 판매용' 구조체 데이터
    public HoldableObject bungeoppangSingleData; // 이름: "완성된붕어빵" 또는 "붕어빵봉투"

    [Header("현재 재고 상태 (Debug)")]
    [SerializeField] private int currentStock = 0; // 현재 진열대에 쌓여있는 낱개 붕어빵 개수

    [Header("시각적 연출용 목록 (선택사항)")]
    // 재고 수량에 비례해 하나씩 켜고 꺼줄 3D 프리팹 무더기 목록
    public List<GameObject> bungeoppangVisuals = new List<GameObject>();

    private void Start()
    {
        UpdateVisuals();
    }

    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // [상황 1] 플레이어가 물건을 들고 있을 때 -> 기계에서 가져온 10개짜리 판을 통째로 붓기
        if (player.IsHoldingItem)
        {
            HoldableObject heldData = player.CurrentHeldData.Value;

            // 기계가 한 번에 내어준 10개 묶음 데이터만 허용
            if (heldData.objectName == "완성된붕어빵판")
            {
                // 10개를 충전해도 최대치(30개)를 넘지 않는지 확인
                if (currentStock + 10 <= maxStorage)
                {
                    currentStock += 10; // 플레이어 기획: 한 번에 10개 일괄 충전!
                    player.ClearHand(); // 들고 있던 빈 판(틀) 수거 처리
                    
                    UpdateVisuals();
                    Debug.Log($"붕어빵진열대: 10개 한 판을 통째로 쏟아부었습니다! (현재 총 재고: {currentStock} / {maxStorage}개)");
                }
                else
                {
                    Debug.LogWarning("붕어빵진열대: 가득 차서 더 이상 10개 판을 부을 공간이 없습니다!");
                }
            }
            else if (heldData.objectName == "탄붕어빵판")
            {
                Debug.LogWarning("붕어빵진열대: 탄 요리는 진열할 수 없습니다. 쓰레기통에 버리세요!");
            }
            else
            {
                Debug.LogWarning("붕어빵진열대: 완성된 붕어빵 판만 여기에 진열할 수 있습니다.");
            }
            return;
        }

        // [상황 2] 플레이어가 빈손일 때 -> 주문한 손님에게 주기 위해 '낱개로 1개씩' 꺼내기
        if (!player.IsHoldingItem)
        {
            if (currentStock > 0)
            {
                currentStock--; // 재고에서 1개 차감
                player.HoldNewData(bungeoppangSingleData); // 플레이어 손에 낱개 데이터 지급
                
                UpdateVisuals();
                Debug.Log($"붕어빵진열대: 판매용 붕어빵을 1개 꺼냈습니다. (남은 총 재고: {currentStock}개)");
            }
            else
            {
                Debug.Log("붕어빵진열대: 재고가 없습니다! 기계에서 붕어빵을 한 판 구워 오세요.");
            }
        }
    }

    // 재고 수량에 맞춰 비주얼 오브젝트들을 활성화/비활성화
    private void UpdateVisuals()
    {
        for (int i = 0; i < bungeoppangVisuals.Count; i++)
        {
            if (bungeoppangVisuals[i] != null)
            {
                bungeoppangVisuals[i].SetActive(i < currentStock);
            }
        }
    }
}