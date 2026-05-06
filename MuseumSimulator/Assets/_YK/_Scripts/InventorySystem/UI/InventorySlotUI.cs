// InventorySlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI countText;

    private ItemData currentItem;

    public void Setup(InventorySlot slot)
    {
        currentItem = slot.item;
        
        iconImage.sprite = currentItem.itemIcon;
        countText.text = slot.count > 1 ? slot.count.ToString() : ""; // 1개일 땐 숫자 숨김
    }

    // 버튼 클릭 시 배치 모드로 넘어가거나 상세 정보를 띄우는 함수 연결 용도
    public void OnSlotClicked()
    {
        Debug.Log($"{currentItem.itemName} 클릭됨!");
    }
}