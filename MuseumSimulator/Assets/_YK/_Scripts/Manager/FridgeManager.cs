using UnityEngine;
using System.Collections.Generic;

public class FridgeManager : MonoBehaviour
{
    public static FridgeManager Instance { get; private set; }

    [Header("냉장고 설정")]
    public int maxCapacity = 100;
    
    // ⭐ [수정] 이제 냉장고 리스트에는 HoldableObject 구조체가 저장됩니다.
    private List<HoldableObject> storedIngredients = new List<HoldableObject>();

    public int CurrentCount => storedIngredients.Count;
    
    // 외부에선 읽기만 가능하도록 ReadOnly 리스트로 전달합니다. (타입 수정)
    public IReadOnlyList<HoldableObject> StoredIngredients => storedIngredients;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ⭐ [수정] ShopItemSlot에서 낱개로 개수만큼 호출하거나, 리스트를 직접 다루기 편하도록 함수를 정돈했습니다.
    public void AddIngredient(HoldableObject data)
    {
        if (CurrentCount >= maxCapacity)
        {
            Debug.LogWarning("냉장고가 이미 가득 찼습니다!");
            return;
        }

        storedIngredients.Add(data);
        RefreshUI();
    }

    // ⭐ [추가] 냉장고 UI 슬롯을 클릭해서 손에 들 때, 냉장고 데이터에서 안전하게 빼주기 위한 함수입니다.
    public void RemoveIngredient(HoldableObject data)
    {
        // 구조체 값 비교를 통해 일치하는 데이터 1개를 리스트에서 지웁니다.
        // 이름과 현재 상태가 같은 첫 번째 데이터를 찾아서 삭제합니다.
        int indexToRemove = storedIngredients.FindIndex(x => x.objectName == data.objectName && x.objectState == data.objectState);
        
        if (indexToRemove != -1)
        {
            storedIngredients.RemoveAt(indexToRemove);
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        FridgeUI fridgeUI = FindFirstObjectByType<FridgeUI>();
        if (fridgeUI != null)
        {
            fridgeUI.UpdateFridgeUI();
        }
        else
        {
            Debug.LogWarning("씬에 FridgeUI를 찾을 수 없습니다!");
        }
    }
}