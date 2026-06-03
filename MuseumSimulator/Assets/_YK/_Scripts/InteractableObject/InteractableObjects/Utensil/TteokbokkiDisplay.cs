using UnityEngine;
using UnityEngine.UI; // 💡 원형 슬라이더(Image) 제어용 추가
using TMPro;        // 💡 텍스트 제어용 추가

public class TteokbokkiDisplay : MonoBehaviour, IInteractable
{
    [Header("진열대 설정")]
    public int maxServings = 10; // 완성작 하나당 채워지는 최대 서빙(컵) 수
    
    [Header("결과물 데이터")]
    [Tooltip("진열대에서 빈손으로 꺼낼 때 손님에게 판매할 소분용 떡볶이컵 에셋")]
    public FoodData tteokbokkiCupData; // 에셋 이름: "떡볶이컵"

    [Header("진열대 상태 (Debug)")]
    [SerializeField] private bool hasFood = false;    // 현재 진열대에 떡볶이가 채워져 있는가?
    [SerializeField] private int remainingServings = 0; // 남은 서빙(컵) 횟수

    [Header("시각적 연출용 (선택사항)")]
    public GameObject foodVisualObject; // 진열대에 떡볶이가 차 있을 때만 켜줄 3D 메쉬 오브젝트

    [Header("⭐ 진열대 UI 설정")]
    [Tooltip("진열대 UI 전체를 감싸는 부모 오브젝트 (비어있을 때는 숨기기용)")]
    public GameObject displayUIPanel;
    
    [Tooltip("Image Type이 Filled(Radial 360)로 설정된 원형 게이지 이미지")]
    public Image circleProgressSlider;
    
    [Tooltip("현재 남은 수량을 표시할 TextMeshPro - Text UI (ex: 10 / 10)")]
    public TMP_Text servingsText;

    private void Start()
    {
        UpdateVisual();
        
        // 💡 첫 시작 시 진열대가 비어있다면 UI 패널을 숨깁니다.
        if (displayUIPanel != null)
        {
            displayUIPanel.SetActive(hasFood);
        }
    }

    // 💡 인터페이스 구현 (플레이어가 바라보고 E키를 톡 눌렀을 때 실행)
    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // 💡 [상황 1] 진열대가 비어있고, 플레이어가 완수된 음식을 들고 왔을 때 -> 진열대에 채우기
        if (!hasFood)
        {
            if (!player.IsHoldingItem)
            {
                Debug.Log("진열대: 현재 비어있습니다. 완성된 떡볶이를 조리해서 가져오세요.");
                return;
            }

            FoodData heldData = player.CurrentHeldData;

            // 오직 이름이 "Completed Tteokbokki"인 데이터만 받음
            if (heldData.foodName == "Completed Tteokbokki")
            {
                // 플레이어 손 비우기 (냄비/그릇 수거)
                player.ClearHand(); 
                
                hasFood = true;
                remainingServings = maxServings; // 10회로 충전!
                
                // ⭐ 음식을 부었으니 UI 패널을 켜고 게이지를 만땅(1.0)으로 갱신합니다.
                if (displayUIPanel != null) displayUIPanel.SetActive(true);
                UpdateUI();

                UpdateVisual();
                Debug.Log($"진열대: 대형 떡볶이 판에 음식을 부었습니다! (남은 수량: {remainingServings}컵)");
            }
            else if (heldData.foodName == "Burnt Tteokbokki")
            {
                Debug.LogWarning("진열대: 상하거나 탄 요리는 판매용 진열대에 올릴 수 없습니다!");
            }
            else
            {
                Debug.LogWarning("진열대: 완성된 떡볶이만 진열할 수 있습니다.");
            }
            return;
        }

        // 💡 [상황 2] 진열대에 떡볶이가 채워져 있을 때 -> 빈손으로 누르면 한 컵씩 꺼내기
        if (hasFood)
        {
            if (player.IsHoldingItem)
            {
                Debug.LogWarning("진열대: 떡볶이를 컵에 담으려면 손을 비워고 상호작용하세요!");
                return;
            }

            if (tteokbokkiCupData == null)
            {
                Debug.LogError("진열대: 'tteokbokkiCupData' 에셋이 인스펙터에 연결되지 않았습니다!");
                return;
            }

            // 1컵 차감 및 플레이어에게 컵 지급
            remainingServings--;
            
            // 플레이어 빈손에 완제품 떡볶이컵 FoodData 에셋을 안전하게 쥐여줍니다.
            player.HoldNewData(tteokbokkiCupData);
            Debug.Log($"진열대: 떡볶이를 한 컵 퍼서 손에 쥐었습니다. (남은 수량: {remainingServings}/{maxServings})");

            // ⭐ 컵을 퍼냈으므로 남은 비율에 맞춰 UI 실시간 갱신
            UpdateUI();

            // 10번을 다 꺼내 먹었다면(완판) 진열대 초기화
            if (remainingServings <= 0)
            {
                hasFood = false;
                remainingServings = 0;
                
                // ⭐ 완판되었으므로 UI 패널을 다시 깔끔하게 숨겨줍니다.
                if (displayUIPanel != null) displayUIPanel.SetActive(false);
                
                Debug.Log("진열대: 떡볶이를 모두 소진했습니다! 새로운 판을 요리해 오세요.");
            }

            UpdateVisual();
        }
    }

    // ⭐ 진열대 UI 수치를 계산하고 동기화해주는 메서드
    private void UpdateUI()
    {
        if (maxServings <= 0) return;

        // 현재 남은 수량 비율 계산 (0.0 ~ 1.0)
        float progressNormalized = (float)remainingServings / maxServings;
        progressNormalized = Mathf.Clamp01(progressNormalized);

        // 원형 이미지 fillAmount 갱신
        if (circleProgressSlider != null)
        {
            circleProgressSlider.fillAmount = progressNormalized;
        }

        // 텍스트에 직관적으로 수량 표시 (예: "7 / 10")
        if (servingsText != null)
        {
            servingsText.text = $"{remainingServings}/{maxServings}";
        }
    }

    // 진열대 상태에 따라 3D 음식을 켜고 끄는 간단한 비주얼 갱신 함수
    private void UpdateVisual()
    {
        if (foodVisualObject != null)
        {
            foodVisualObject.SetActive(hasFood);
        }
    }
}