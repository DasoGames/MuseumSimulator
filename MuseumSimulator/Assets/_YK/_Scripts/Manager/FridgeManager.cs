using UnityEngine;
using System.Collections.Generic;

public class FridgeManager : MonoBehaviour
{
    public static FridgeManager Instance { get; private set; }

    [Header("냉장고 용량 설정")]
    public int maxCapacity = 30;
    
    [SerializeField] private List<FoodData> storedIngredients = new List<FoodData>();
    
    public IReadOnlyList<FoodData> StoredIngredients => storedIngredients;
    public int CurrentCount => storedIngredients.Count;

    private void Awake()
    {
        // ⭐ [핵심] 데이터 영속성 유지 로직
        if (Instance == null)
        {
            Instance = this;
            // 씬이 바뀌어도 냉장고와 그 안의 재료 리스트를 파괴하지 마라!
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            // 이미 다른 씬에서 넘어온 냉장고 매니저가 있다면, 
            // 현재 씬에 새로 생성된 중복 매니저는 파괴한다.
            Destroy(gameObject);
        }
    }

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

    public void RemoveIngredient(FoodData data)
    {
        if (storedIngredients.Contains(data))
        {
            storedIngredients.Remove(data);
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        // 💡 판매 씬으로 넘어갔을 때만 UI가 존재하므로, 찾아서 업데이트해준다.
        FridgeUI ui = FindFirstObjectByType<FridgeUI>();
        if (ui != null)
        {
            ui.UpdateFridgeUI();
        }
    }
}