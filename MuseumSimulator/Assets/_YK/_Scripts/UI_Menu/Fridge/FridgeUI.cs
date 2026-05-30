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

        // 기존 슬롯 싹 청소
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        // ⭐ [에러 해결] 냉장고에서 이제 복사본 구조체가 아닌 순수 FoodData 에셋 리스트를 가져옵니다.
        IReadOnlyList<FoodData> ingredients = FridgeManager.Instance.StoredIngredients;
        
        for (int i = 0; i < ingredients.Count; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, gridParent);
            
            // 슬롯 컴포넌트에 안전하게 고유 FoodData 에셋 전달
            FridgeItemSlot itemSlotScript = newSlot.GetComponent<FridgeItemSlot>();
            if (itemSlotScript != null)
            {
                // ⭐ 이 한 줄이 실행되며 슬롯이 자기 전용 데이터와 아이콘을 스스로 그리고 세팅합니다.
                itemSlotScript.SetupSlot(ingredients[i]); 
            }
        }

        // 냉장고 용량 텍스트 출력 수정
        if (capacityText != null)
        {
            capacityText.text = $"{FridgeManager.Instance.CurrentCount} / {FridgeManager.Instance.maxCapacity}";
        }
    }
}