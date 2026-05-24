using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemSlot : MonoBehaviour
{
    [Header("이 슬롯의 상품 정보")]
    public IngredientData itemData; 

    [Header("UI 컴포넌트 연결")]
    public Image itemIconImage;       
    public TextMeshProUGUI itemNameText;  
    public TextMeshProUGUI itemPriceText; 
    public TextMeshProUGUI quantityText;  
    public TextMeshProUGUI totalPriceText;

    private int currentQuantity = 1;  

    void Start()
    {
        InitUI();
    }

    void InitUI()
    {
        if (itemIconImage != null && itemData.icon != null) itemIconImage.sprite = itemData.icon;
        if (itemNameText != null) itemNameText.text = itemData.ingredientName;
        if (itemPriceText != null) itemPriceText.text = $"₩ {itemData.price:N0}"; 
        
        UpdateQuantityAndPriceUI();
    }

    public void IncreaseQuantity()
    {
        if (currentQuantity >= 100) return;
        currentQuantity++;
        UpdateQuantityAndPriceUI();
    }

    public void DecreaseQuantity()
    {
        if (currentQuantity <= 1) return;
        currentQuantity--;
        UpdateQuantityAndPriceUI();
    }

    private void UpdateQuantityAndPriceUI()
    {
        if (quantityText != null) quantityText.text = currentQuantity.ToString();
        if (totalPriceText != null)
        {
            int total = itemData.price * currentQuantity;
            totalPriceText.text = $"총 ₩ {total:N0}";
        }
    }

    public void PurchaseItem()
    {
        int finalPrice = itemData.price * currentQuantity;

        // 1. 냉장고 공간 체크
        if (FridgeManager.Instance == null)
        {
            Debug.LogError("씬에 FridgeManager가 없습니다!");
            return;
        }

        int currentCount = FridgeManager.Instance.CurrentCount;
        if (currentCount + currentQuantity > FridgeManager.Instance.maxCapacity)
        {
            Debug.LogWarning($"냉장고 공간 부족! 남은 공간: {FridgeManager.Instance.maxCapacity - currentCount}개");
            return; 
        }

        // 2. 돈 체크 및 차감
        if (MoneyManager.Instance != null)
        {
            if (!MoneyManager.Instance.ConsumeMoney(finalPrice))
            {
                Debug.LogWarning("잔액이 부족합니다!");
                return; 
            }
        }
        else
        {
            Debug.LogError("씬에 MoneyManager가 없습니다!");
            return;
        }

        // 3. ⭐ [완벽한 데이터 구조화] 프리팹 접근 로직 전면 삭제!
        // 상점 데이터에 심어둔 순수 구조체 데이터를 그대로 가져옵니다.
        HoldableObject dataToSave = itemData.initialHoldableData;
        
        // 3D 물체 생성 없이, 오직 냉장고 리스트(데이터 공간)에만 버튼용으로 추가합니다.
        for (int i = 0; i < currentQuantity; i++)
        {
            FridgeManager.Instance.AddIngredient(dataToSave);
        }

        Debug.Log($"{itemData.ingredientName} {currentQuantity}개 구매 완료! (냉장고 데이터에 추가됨)");
        
        // 4. 수량 초기화 및 UI 변경
        currentQuantity = 1;
        UpdateQuantityAndPriceUI();
    }
}