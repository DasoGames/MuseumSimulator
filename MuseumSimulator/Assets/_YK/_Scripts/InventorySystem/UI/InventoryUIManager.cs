// InventoryUIManager.cs
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform slotContentPanel; // Scroll View의 Content (프리팹이 생성될 부모)
    public GameObject slotPrefab;      // 만들어둔 슬롯 프리팹

    private ItemType currentTab = ItemType.Artifact; // 기본 탭 설정

    private void Start()
    {
        // InventoryManager에 변경사항이 생길 때마다 RefreshUI를 실행하도록 구독
        InventoryManager.Instance.OnInventoryUpdated += RefreshUI;
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryUpdated -= RefreshUI;
    }

    // 탭 버튼(UI Button)의 OnClick 이벤트에 연결할 함수들
    public void OnTabArtifactClicked() => SetTab(ItemType.Artifact);
    public void OnTabDecorationClicked() => SetTab(ItemType.Decoration);
    public void OnTabFacilityClicked() => SetTab(ItemType.Facility);

    private void SetTab(ItemType tabType)
    {
        currentTab = tabType;
        RefreshUI();
    }

    private void RefreshUI()
    {
        // 1. 기존에 생성된 슬롯 UI 전부 삭제 (초기화)
        foreach (Transform child in slotContentPanel)
        {
            Destroy(child.gameObject);
        }

        // 2. 매니저의 리스트를 획득한 순서(0번 인덱스부터)대로 훑기
        var inventory = InventoryManager.Instance.inventoryList;
        for (int i = 0; i < inventory.Count; i++)
        {
            // 3. 현재 탭(currentTab)과 타입이 일치하는 아이템만 필터링
            if (inventory[i].item.itemType == currentTab)
            {
                // 프리팹 생성 후 부모 설정
                GameObject newSlot = Instantiate(slotPrefab, slotContentPanel);
                
                // 데이터 세팅
                newSlot.GetComponent<InventorySlotUI>().Setup(inventory[i]);
            }
        }
    }
}