using UnityEngine;
using UnityEngine.UI;

public class FridgeItemSlot : MonoBehaviour
{
    // ⭐ [수정] 이제 냉장고 슬롯은 HoldableObject 데이터를 보관합니다.
    private HoldableObject myIngredientData; 
    private Button slotButton;

    void Awake()
    {
        slotButton = GetComponent<Button>();
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnSlotClicked);
        }
    }

    // ⭐ [수정] UI에서 데이터를 안전하게 넘겨받을 수 있도록 매개변수 타입 일치화!
    public void SetupSlot(HoldableObject data)
    {
        myIngredientData = data;
    }

    private void OnSlotClicked()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null || player.IsHoldingItem)
        {
            Debug.LogWarning("플레이어가 없거나 이미 손에 무언가를 들고 있습니다.");
            return;
        }

        // 플레이어 손에 구조체 데이터를 던져서 3D 물체 스폰 시키기
        player.HoldNewData(myIngredientData);

        // 냉장고 매니저 데이터에서 삭제 요청
        if (FridgeManager.Instance != null)
        {
            FridgeManager.Instance.RemoveIngredient(myIngredientData);
        }
    }
}