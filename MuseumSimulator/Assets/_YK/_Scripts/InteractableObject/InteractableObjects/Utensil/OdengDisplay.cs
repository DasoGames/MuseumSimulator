using UnityEngine;
using System.Collections.Generic;

public class OdengDisplay : MonoBehaviour, IInteractable
{
    [Header("오뎅 진열대 설정")]
    public int maxStorage = 15; // 진열대에 최대로 쌓아둘 수 있는 오뎅 꼬치 개수
    
    [Header("결과물 데이터")]
    public HoldableObject odengCupData; // 소분해서 손님에게 줄 데이터 (이름: "오뎅컵" 혹은 "접시 오뎅")

    [Header("현재 상태 (Debug)")]
    [SerializeField] private int currentStock = 0; // 현재 진열된 오뎅 개수

    [Header("시각적 연출용 목록 (선택사항)")]
    // 오뎅 개수가 늘어날 때마다 하나씩 켜줄 오뎅 꼬치 3D 모델링들을 리스트로 배치해두면 시각적 표현이 매우 좋습니다.
    public List<GameObject> odengVisualObjects = new List<GameObject>();

    private void Start()
    {
        UpdateVisuals();
    }

    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // [상황 1] 플레이어가 물건을 들고 있을 때 -> 진열대에 오뎅 추가 시도
        if (player.IsHoldingItem)
        {
            HoldableObject heldData = player.CurrentHeldData.Value;

            if (heldData.objectName == "완성된오뎅꼬치")
            {
                if (currentStock < maxStorage)
                {
                    currentStock++;
                    player.ClearHand(); // 플레이어 손 비우기
                    UpdateVisuals();
                    Debug.Log($"오뎅진열대: 완성된 오뎅을 1개 추가했습니다. (현재 재고: {currentStock} / {maxStorage})");
                }
                else
                {
                    Debug.LogWarning("오뎅진열대: 진열대가 가득 차서 더 이상 채울 수 없습니다!");
                }
            }
            else if (heldData.objectName == "탄오뎅꼬치")
            {
                Debug.LogWarning("오뎅진열대: 불어 터지거나 탄 오뎅은 진열할 수 없습니다.");
            }
            return;
        }

        // [상황 2] 플레이어가 빈손일 때 -> 진열대에서 판매용 한 컵 꺼내기
        if (!player.IsHoldingItem)
        {
            if (currentStock > 0)
            {
                currentStock--;
                player.HoldNewData(odengCupData); // 손님 지급용 데이터 들려주기
                UpdateVisuals();
                Debug.Log($"오뎅진열대: 오뎅을 한 컵 소분하여 꺼냈습니다. (남은 재고: {currentStock})");
            }
            else
            {
                Debug.Log("오뎅진열대: 현재 재고가 없습니다. 오뎅 기계에서 조리해 오세요.");
            }
        }
    }

    // 재고 수량에 맞춰 3D 물체를 켜고 끄는 연출용 함수
    private void UpdateVisuals()
    {
        for (int i = 0; i < odengVisualObjects.Count; i++)
        {
            if (odengVisualObjects[i] != null)
            {
                // 현재 재고 번호보다 낮은 인덱스의 시각 오브젝트들만 활성화
                odengVisualObjects[i].SetActive(i < currentStock);
            }
        }
    }
}