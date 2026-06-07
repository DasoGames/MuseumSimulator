using UnityEngine;
using UnityEngine.UI; // 💡 원형 슬라이더(Image) 제어용 추가
using TMPro;        // 💡 텍스트 제어용 추가
using System.Collections.Generic;

public class PotatoDisplay : MonoBehaviour, IInteractable
{
    [Header("진열대 설정")]
    [Tooltip("진열대 최대 수용량 (ex: 10개들이 총 3판 분량인 30개로 고정)")]
    public int maxStorage = 30; 
    
    [Header("결과물 데이터")]
    [Tooltip("진열대에서 빈손으로 꺼낼 때 손님에게 판매할 테이크아웃 감자튀김컵 에셋")]
    public FoodData potatoCupData; // 에셋 이름: "감자튀김컵"

    [Header("현재 재고 상태 (Debug)")]
    [SerializeField] private int currentStock = 0; // 현재 진열대에 쌓여있는 낱개 감자튀김 수량

    [Header("⭐ 시각적 연출용 목록 (최대 용량 매핑)")]
    [Tooltip("진열대 자식으로 배치해 둔 낱개 감자튀김 3D 오브젝트들을 차곡차곡 쌓이는 순서대로 전부 연결하세요!")]
    public List<GameObject> potatoVisuals = new List<GameObject>();

    [Header("⭐ 진열대 UI 설정")]
    [Tooltip("진열대 UI 전체를 감싸는 부모 오브젝트 (재고가 0개일 때는 숨기기용)")]
    public GameObject displayUIPanel;
    
    [Tooltip("Image Type이 Filled(Radial 360)로 설정된 원형 게이지 이미지")]
    public Image circleProgressSlider;
    
    [Tooltip("현재 남은 재고 수량을 표시할 TextMeshPro - Text UI (ex: 10 / 30)")]
    public TMP_Text stockText;

    private void Start()
    {
        // 게임 시작 시 현재 재고 수량에 맞춰 3D 모델들과 UI 세팅 동기화
        UpdateVisuals();
        UpdateUI();
    }

    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // 🔥 [상황 1] 플레이어가 물건을 들고 있을 때 -> 튀김기에서 가져온 완성된 판을 통째로 붓기
        if (player.IsHoldingItem)
        {
            FoodData heldData = player.CurrentHeldData;

            // 데이터 룰에 맞춰 'foodName' 필드로 필터링합니다.
            if (heldData.foodName == "완성된감자튀김")
            {
                // 한 판(10개 수량)을 부었을 때 최대치를 넘지 않는지 안전 검사
                if (currentStock + 10 <= maxStorage)
                {
                    currentStock += 10; // 붕어빵/치킨 규칙 반영: 한 번에 10개 일괄 충전!
                    player.ClearHand(); // 플레이어가 들고 있던 빈 바스켓/그릇 비우기
                    
                    UpdateVisuals();
                    UpdateUI();
                    Debug.Log($"감자진열대: 튀김 한 판을 통째로 쏟아부었습니다! (현재 총 재고: {currentStock} / {maxStorage}개)");
                }
                else
                {
                    Debug.LogWarning("감자진열대: 가득 차서 더 이상 감자튀김 한 판을 부을 공간이 없습니다!");
                }
            }
            else if (heldData.foodName == "탄감자튀김")
            {
                Debug.LogWarning("감자진열대: 탄 감자튀김은 진열할 수 없습니다. 쓰레기통에 버리세요!");
            }
            else
            {
                Debug.LogWarning("감자진열대: 완성된 감자튀김 판만 여기에 진열할 수 있습니다.");
            }
            return;
        }

        // 🔥 [상황 2] 플레이어가 빈손일 때 -> 주문한 손님에게 주기 위해 '낱개로 1개씩' 꺼내기
        if (!player.IsHoldingItem)
        {
            if (currentStock > 0)
            {
                if (potatoCupData == null)
                {
                    Debug.LogError("감자진열대: 'potatoCupData' 에셋이 인스펙터에 연결되지 않았습니다!");
                    return;
                }

                currentStock--; // 재고에서 낱개 1개 차감
                
                // 플레이어 빈손에 완제품 감자튀김컵 FoodData 에셋을 안전하게 쥐여줍니다.
                player.HoldNewData(potatoCupData); 
                
                UpdateVisuals();
                UpdateUI();
                Debug.Log($"감자진열대: 판매용 감자튀김컵을 1개 꺼냈습니다. (남은 총 재고: {currentStock}개)");
            }
            else
            {
                Debug.Log("감자진열대: 재고가 없습니다! 튀김기에서 감자를 더 튀겨오세요.");
            }
        }
    }

    // ⭐ 재고 수량 인덱스에 맞춰 자식 감자튀김 오브젝트들을 순차적으로 켜고 끄는 연출 함수
    private void UpdateVisuals()
    {
        for (int i = 0; i < potatoVisuals.Count; i++)
        {
            if (potatoVisuals[i] != null)
            {
                // i가 현재 재고(currentStock)보다 작을 때만 true가 되어 활성화됨
                // ex) 튀김 한 판을 부으면 0번부터 9번 감자튀김 메쉬가 한 번에 탁 켜집니다.
                // 1개씩 팔리면 끝에 쌓여있던 메쉬부터 순서대로 꺼집니다.
                potatoVisuals[i].SetActive(i < currentStock);
            }
        }
    }

    // ⭐ 진열대 UI 수치와 패널 온/오프 상태를 동기화해주는 메서드
    private void UpdateUI()
    {
        // 재고가 완전히 0개라면 UI 패널 자체를 비활성화해서 숨김
        if (displayUIPanel != null)
        {
            displayUIPanel.SetActive(currentStock > 0);
        }

        if (maxStorage <= 0) return;

        // 현재 남은 재고 비율 계산 (0.0 ~ 1.0)
        float progressNormalized = (float)currentStock / maxStorage;
        progressNormalized = Mathf.Clamp01(progressNormalized);

        // 원형 이미지 fillAmount 실시간 동기화
        if (circleProgressSlider != null)
        {
            circleProgressSlider.fillAmount = progressNormalized;
        }

        // 텍스트에 직관적으로 수량 표시 (예: "10 / 30")
        if (stockText != null)
        {
            stockText.text = $"{currentStock}/{maxStorage}";
        }
    }
}