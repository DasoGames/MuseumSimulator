using UnityEngine;
using UnityEngine.UI;

public class FridgeItemSlot : MonoBehaviour
{
    private FoodData myFoodData = null; // 💡 이 슬롯 버튼이 품고 있는 고유 FoodData 에셋
    private Button slotButton;
    private Image iconImage;

    void Awake()
    {
        slotButton = GetComponent<Button>();
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnSlotClicked);
        }

        // 프리팹 내부의 자식 오브젝트 중 "Icon"이라는 이름을 가진 이미지 컴포넌트 자동 캐싱
        Transform iconTransform = transform.Find("Icon");
        if (iconTransform != null)
        {
            iconImage = iconTransform.GetComponent<Image>();
        }
    }

    /// <summary>
    /// 💡 FridgeUI가 인스턴스 생성(Instantiate) 직후 호출하여 데이터를 바인딩해주는 함수
    /// </summary>
    public void SetupSlot(FoodData data)
    {
        myFoodData = data;

        // 💡 프리팹의 디폴트 값을 무시하고, 전달받은 에셋의 고유 아이콘으로 즉시 갈아끼웁니다!
        if (iconImage != null && myFoodData != null && myFoodData.icon != null)
        {
            iconImage.sprite = myFoodData.icon;
            iconImage.enabled = true;
        }
    }

    private void OnSlotClicked()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        
        // 유효성 체크: 플레이어가 없거나, 손에 이미 무언가를 들고 있다면 꺼낼 수 없음
        if (player == null || player.IsHoldingItem || myFoodData == null)
        {
            Debug.LogWarning("냉장고슬롯: 손에 이미 물건을 들고 있거나 데이터가 올바르지 않습니다.");
            return;
        }

        // 1. ⭐ [버그 해결 핵심] 원본 프리팹 값이 아니라, 내 슬롯이 기억하는 정직한 고유 데이터를 플레이어 손에 전달!
        player.HoldNewData(myFoodData);

        // 2. 냉장고 매니저의 데이터 리스트에서 제거 요청 (동시에 UI가 자동으로 새로고침됩니다)
        if (FridgeManager.Instance != null)
        {
            FridgeManager.Instance.RemoveIngredient(myFoodData);
        }
    }
}