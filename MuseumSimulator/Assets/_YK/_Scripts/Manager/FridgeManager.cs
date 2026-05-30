using UnityEngine;
using System.Collections.Generic;

public class FridgeManager : MonoBehaviour
{
    public static FridgeManager Instance { get; private set; }

    [Header("냉장고 용량 설정")]
    public int maxCapacity = 30;
    
    // 💡 구조체 리스트가 아닌, 스크립터블 오브젝트(FoodData) 에셋 리스트를 저장합니다!
    [SerializeField] private List<FoodData> storedIngredients = new List<FoodData>();
    
    // 외부(UI 등)에서 리스트를 안전하게 읽기만 가능하도록 보장
    public IReadOnlyList<FoodData> StoredIngredients => storedIngredients;

    // 현재 저장된 총 개수
    public int CurrentCount => storedIngredients.Count;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 상점에서 물건 구매 성공 시 냉장고 리스트에 저장하는 함수
    /// </summary>
    public void AddIngredient(FoodData data)
    {
        if (storedIngredients.Count >= maxCapacity)
        {
            Debug.LogWarning("냉장고가 가득 찼습니다!");
            return;
        }

        storedIngredients.Add(data);
        RefreshUI();
    }

    /// <summary>
    /// 냉장고 UI 슬롯을 클릭해 플레이어가 꺼내갔을 때 리스트에서 지우는 함수
    /// </summary>
    public void RemoveIngredient(FoodData data)
    {
        if (storedIngredients.Contains(data))
        {
            storedIngredients.Remove(data);
            RefreshUI();
        }
    }

    // 데이터가 변할 때마다 화면에 바로 보이도록 UI 새로고침 신호를 보냅니다.
    private void RefreshUI()
    {
        FridgeUI ui = FindFirstObjectByType<FridgeUI>();
        if (ui != null)
        {
            ui.UpdateFridgeUI();
        }
    }
}