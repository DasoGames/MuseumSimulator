using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class FridgeUI : MonoBehaviour
{
    [Header("냉장고 UI 연결")]
    public GameObject slotPrefab;       
    public Transform gridParent;        
    public TextMeshProUGUI capacityText;

    void Start()
    {
        UpdateFridgeUI();
    }

    public void UpdateFridgeUI()
    {
        if (FridgeManager.Instance == null) return;

        // 기존 슬롯 싹 삭제
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        // ⭐ [에러 해결] 이제 냉장고에서 HoldableObject 구조체를 가져옵니다.
        IReadOnlyList<HoldableObject> ingredients = FridgeManager.Instance.StoredIngredients;
        
        for (int i = 0; i < ingredients.Count; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, gridParent);
            
            // 슬롯 컴포넌트에 구조체 전달 (매개변수 불일치 에러 해결)
            FridgeItemSlot itemSlotScript = newSlot.GetComponent<FridgeItemSlot>();
            if (itemSlotScript != null)
            {
                itemSlotScript.SetupSlot(ingredients[i]);
            }

            // 아이콘 이미지 띄우기 (HoldableObject에 새로 추가한 icon 필드 사용)
            Transform iconTransform = newSlot.transform.Find("Icon");
            if (iconTransform != null)
            {
                Image slotImage = iconTransform.GetComponent<Image>();
                if (slotImage != null && ingredients[i].icon != null)
                {
                    slotImage.sprite = ingredients[i].icon;
                    slotImage.enabled = true;
                }
            }
        }

        if (capacityText != null)
        {
            capacityText.text = $"{FridgeManager.Instance.CurrentCount} / {FridgeManager.Instance.maxCapacity}";
        }
    }
}