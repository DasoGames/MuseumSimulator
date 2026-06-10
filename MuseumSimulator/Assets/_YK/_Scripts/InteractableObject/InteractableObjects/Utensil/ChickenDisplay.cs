using UnityEngine;
using UnityEngine.UI; // 💡 원형 슬라이더(Image) 제어용 추가
using TMPro;        // 💡 텍스트 제어용 추가
using System.Collections.Generic;

public class ChickenDisplay : MonoBehaviour, IInteractable
{
    [Header("진열대 설정")]
    [Tooltip("진열대 최대 수용량 (ex: 튀김기에서 한 판 건질 때마다 대량 충전되는 공간 용량)")]
    public int maxStorage = 30; 
    
    [Header("결과물 데이터")]
    [Tooltip("진열대에서 빈손으로 꺼낼 때 손님에게 판매할 소분용 테이크아웃 치킨컵 에셋")]
    public FoodData chickenCupData; // 에셋 이름: "치킨컵" 혹은 "닭강정컵"

    [Header("현재 재고 상태 (Debug)")]
    [SerializeField] private int currentStock = 0; // 현재 진열대에 쌓여있는 낱개 치킨 조각(또는 슬롯) 개수

    [Header("⭐ 시각적 연출용 목록 (최대 용량 매핑)")]
    [Tooltip("진열대 자식으로 배치해 둔 낱개 치킨 3D 오브젝트들을 차곡차곡 쌓이는 순서대로 전부 연결하세요!")]
    public List<GameObject> chickenVisuals = new List<GameObject>();

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
        if (player == null)
        {
            Debug.LogError($"<color=red><b>🚨 [상호작용 실패]</b> 씬에서 PlayerInteractor를 찾을 수 없습니다!</color>");
            return;
        }

        // =================================================================
        // 🔍 [실시간 상태 모니터링 디버깅 블랙박스]
        // =================================================================
        string heldItemName = player.IsHoldingItem ? player.CurrentHeldData.foodName : "빈손";
        Debug.Log($"<color=#FF00FF><b>🔍 [치킨 진열대 상호작용 검문]</b>\n" +
                  $"▶ 클릭한 플레이어 손: [{heldItemName}]\n" +
                  $"▶ 진열대 현재 재고: {currentStock} / {maxStorage}</color>");

        // 🔥 [상황 1] 플레이어가 물건을 들고 있을 때 -> 튀김기에서 가져온 완성된 판을 통째로 붓기
        if (player.IsHoldingItem)
        {
            FoodData heldData = player.CurrentHeldData;

            // 💡 [데이터 규칙 일치화] 튀김기 완료 분기 이름인 영어 "CookedChicken" 규칙으로 완벽 동기화!
            if (heldData.foodName == "CookedChicken")
            {
                // 한 판(예: 10개 수량)을 부었을 때 최대치를 넘지 않는지 안전 검사
                if (currentStock + 10 <= maxStorage)
                {
                    currentStock += 10; // 규칙 반영: 한 번에 10개 일괄 충전!
                    player.ClearHand(); // 플레이어가 들고 있던 빈 바스켓/그릇 비우기
                    
                    UpdateVisuals();
                    UpdateUI();
                    Debug.Log($"<color=lime><b>📦 [진열 충전 성공]</b> 치킨 한 판(10개)을 통째로 쏟아부었습니다! (현재 총 재고: {currentStock} / {maxStorage}개)</color>");
                }
                else
                {
                    Debug.LogWarning($"<color=orange><b>⚠️ [진열 거부]</b> 가득 차서 더 이상 치킨 한 판을 부을 공간이 없습니다! (공간 부족)</color>");
                }
            }
            // 💡 [데이터 규칙 일치화] 탄 음식 이름도 영어 "BurntChicken" 규칙으로 완벽 동기화!
            else if (heldData.foodName == "BurntChicken")
            {
                Debug.LogWarning("<color=red><b>❌ [진열 불가]</b> 탄 치킨은 진열할 수 없습니다. 쓰레기통에 폐기하세요!</color>");
            }
            else
            {
                Debug.LogWarning($"<color=orange><b>⚠️ [진열 거부]</b> [{heldData.foodName}]은 여기에 둘 수 없습니다. 완성된 치킨 판만 진열 가능합니다.</color>");
            }
            return;
        }

        // 🔥 [상황 2] 플레이어가 빈손일 때 -> 주문한 손님에게 주기 위해 '낱개로 1개씩' 치킨컵에 소분하기
        if (!player.IsHoldingItem)
        {
            if (currentStock > 0)
            {
                if (chickenCupData == null)
                {
                    Debug.LogError("<color=red><b>🚨 [오류]</b> 치킨진열대: 'chickenCupData' 에셋이 인스펙터에 연결되지 않았습니다!</color>");
                    return;
                }

                currentStock--; // 재고에서 낱개 1개 차감
                
                // 플레이어 빈손에 단품 치킨컵 FoodData를 쥐여줍니다.
                player.HoldNewData(chickenCupData); 
                
                UpdateVisuals();
                UpdateUI();
                Debug.Log($"<color=green><b>🍟 [판매 수거 성공]</b> 판매용 치킨컵을 1개 꺼냈습니다. (남은 총 재고: {currentStock}개)</color>");
            }
            else
            {
                Debug.Log("<color=white>치킨진열대: 현재 재고가 완전히 동났습니다! 튀김기에서 치킨을 더 튀겨오세요.</color>");
            }
        }
    }

    // ⭐ 재고 수량 인덱스에 맞춰 자식 치킨 오브젝트들을 순차적으로 켜고 끄는 연출 함수
    private void UpdateVisuals()
    {
        for (int i = 0; i < chickenVisuals.Count; i++)
        {
            if (chickenVisuals[i] != null)
            {
                // i가 현재 재고(currentStock)보다 작을 때만 true가 되어 활성화됨
                chickenVisuals[i].SetActive(i < currentStock);
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
            circleProgressSlider.color = Color.green; // 기본 UI 바 색상 고정
            circleProgressSlider.fillAmount = progressNormalized;
        }

        // 텍스트에 직관적으로 수량 표시 (예: "10 / 30")
        if (stockText != null)
        {
            stockText.text = $"{currentStock}/{maxStorage}";
        }
    }
}