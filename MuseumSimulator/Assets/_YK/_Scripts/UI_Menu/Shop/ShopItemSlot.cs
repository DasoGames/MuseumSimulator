using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemSlot : MonoBehaviour
{
    [Header("이 슬롯의 상품 정보")]
    public FoodData itemData; 

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
        if (itemData == null) return;

        if (itemIconImage != null && itemData.icon != null) itemIconImage.sprite = itemData.icon;
        if (itemNameText != null) itemNameText.text = itemData.foodName;
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
        if (totalPriceText != null && itemData != null)
        {
            int total = itemData.price * currentQuantity;
            totalPriceText.text = $"총 ₩ {total:N0}";
        }
    }

    // ⭐ 디버깅 로그가 대폭 추가된 구매 메서드
    public void PurchaseItem()
    {
        // 0. 버튼 클릭 및 데이터 에셋 존재 여부 확인
        Debug.Log($"<color=cyan>[상점] 구매 버튼 클릭됨!</color> 현재 슬롯의 게임 오브젝트 이름: {gameObject.name}");

        if (itemData == null)
        {
            Debug.LogError("[상점 에러] 이 슬롯에 'FoodData' 에셋이 연결되어 있지 않습니다! 인스펙터를 확인하세요.");
            return;
        }

        int finalPrice = itemData.price * currentQuantity;
        Debug.Log($"[상점 디버그] 구매 시도 상품: {itemData.foodName} | 수량: {currentQuantity}개 | 총 가격: {finalPrice}원");

        // 1. 냉장고 공간 체크
        if (FridgeManager.Instance == null)
        {
            Debug.LogError("[상점 에러] 씬에 FridgeManager 인스턴스(Instance)가 존재하지 않습니다!");
            return;
        }

        int currentCount = FridgeManager.Instance.CurrentCount;
        Debug.Log($"[상점 디버그] 냉장고 상태 체크 -> 현재 재고 수량: {currentCount} / 최대 용량: {FridgeManager.Instance.maxCapacity}");

        if (currentCount + currentQuantity > FridgeManager.Instance.maxCapacity)
        {
            Debug.LogWarning($"[상점 거절] 냉장고 공간이 부족합니다! (현재: {currentCount}개 + 구매시도: {currentQuantity}개 > 최대: {FridgeManager.Instance.maxCapacity}개)");
            return; 
        }

        // 2. 돈 체크 및 차감
        if (MoneyManager.Instance != null)
        {
            Debug.Log("[상점 디버그] MoneyManager 발견. 잔액 차감을 시도합니다.");
            if (!MoneyManager.Instance.ConsumeMoney(finalPrice))
            {
                Debug.LogWarning($"[상점 거절] 소지 금액이 부족하여 구매할 수 없습니다! 필요 금액: {finalPrice}원");
                return; 
            }
        }
        else
        {
            Debug.LogError("[상점 에러] 씬에 MoneyManager 인스턴스(Instance)가 존재하지 않습니다! 돈 체크를 패스할 수 없어 구매를 중단합니다.");
            return;
        }

        // 3. 냉장고 데이터 공간에 FoodData 추가
        Debug.Log($"[상점 성공] 조건 통과! 냉장고에 {itemData.foodName} 에셋 {currentQuantity}개를 저장을 시작합니다.");
        for (int i = 0; i < currentQuantity; i++)
        {
            FridgeManager.Instance.AddIngredient(itemData);
        }

        Debug.Log($"<color=green>[상점 완료]</color> {itemData.foodName} {currentQuantity}개 구매 처리가 정상 종료되었습니다! 냉장고 최종 개수: {FridgeManager.Instance.CurrentCount}개");
        
        // 4. 수량 초기화 및 UI 변경
        currentQuantity = 1;
        UpdateQuantityAndPriceUI();
    }
}