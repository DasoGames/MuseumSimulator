using UnityEngine;
using UnityEngine.UI; // 💡 원형 슬라이더(Image) 제어용 추가
using TMPro;        // 💡 텍스트 제어용 추가
using System.Collections.Generic;

public class OdengDisplay : MonoBehaviour, IInteractable
{
    [Header("오뎅 진열대 설정")]
    [Tooltip("진열대에 최대로 쌓아둘 수 있는 오뎅 꼬치 개수 (10개로 고정)")]
    public int maxStorage = 10; 
    
    [Header("결과물 데이터")]
    [Tooltip("진열대에서 빈손으로 꺼낼 때 손님에게 판매할 소분용 오뎅컵 에셋")]
    public FoodData odengCupData; // 에셋 이름: "오뎅컵" 혹은 "접시오뎅"

    [Header("현재 상태 (Debug)")]
    [SerializeField] private int currentStock = 0; // 현재 진열된 오뎅 개수

    [Header("⭐ 시각적 연출용 목록 (10개 매핑)")]
    [Tooltip("진열대 자식으로 넣어둔 10개의 오뎅 꼬치 오브젝트를 순서대로 여기에 전부 연결하세요! (0번부터 9번까지 총 10개)")]
    public List<GameObject> odengVisualObjects = new List<GameObject>();

    [Header("⭐ 진열대 UI 설정")]
    [Tooltip("진열대 UI 전체를 감싸는 부모 오브젝트 (재고가 0개일 때는 숨기기용)")]
    public GameObject displayUIPanel;
    
    [Tooltip("Image Type이 Filled(Radial 360)로 설정된 원형 게이지 이미지")]
    public Image circleProgressSlider;
    
    [Tooltip("현재 남은 재고 수량을 표시할 TextMeshPro - Text UI (ex: 10 / 10)")]
    public TMP_Text stockText;

    private void Start()
    {
        // 게임 시작 시 현재 재고 상태에 맞춰 3D 비주얼과 UI 세팅
        UpdateVisuals();
        UpdateUI();
    }

    // 💡 인터페이스 구현 (플레이어가 바라보고 E키를 톡 눌렀을 때 실행)
    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // 🔥 [상황 1] 플레이어가 물건을 들고 있을 때 -> 진열대에 오뎅 추가 시도
        if (player.IsHoldingItem)
        {
            FoodData heldData = player.CurrentHeldData;

            // 오직 이름이 "완성된오뎅꼬치"인 데이터만 받음
            if (heldData.foodName == "Odeng")
            {
                if (currentStock < maxStorage)
                {
                    // 재고 증가 (이에 따라 UpdateVisuals에서 인덱스 번호에 맞는 오브젝트가 활성화됨)
                    currentStock = currentStock + 10;
                    player.ClearHand(); // 플레이어 손 비우기
                    
                    UpdateVisuals();
                    UpdateUI();
                    Debug.Log($"오뎅진열대: 완성된 오뎅을 1개 추가했습니다. (현재 재고: {currentStock} / {maxStorage})");
                }
                else
                {
                    Debug.LogWarning("오뎅진열대: 진열대가 가득 차서 더 이상 채울 수 없습니다!");
                }
            }
            else if (heldData.foodName == "BurntOdeng")
            {
                Debug.LogWarning("오뎅진열대: 불어 터지거나 탄 오뎅은 진열할 수 없습니다.");
            }
            return;
        }

        // 🔥 [상황 2] 플레이어가 빈손일 때 -> 진열대에서 판매용 한 컵 꺼내기
        if (!player.IsHoldingItem)
        {
            if (currentStock > 0)
            {
                if (odengCupData == null)
                {
                    Debug.LogError("오뎅진열대: 'odengCupData' 에셋이 인스펙터에 연결되지 않았습니다!");
                    return;
                }

                // 재고 감소 (이에 따라 UpdateVisuals에서 맨 끝에 있던 오브젝트부터 하나씩 꺼짐)
                currentStock--;
                
                // 플레이어 빈손에 완제품 오뎅컵 FoodData 에셋을 쥐여줍니다.
                player.HoldNewData(odengCupData); 
                
                UpdateVisuals();
                UpdateUI();
                Debug.Log($"오뎅진열대: 오뎅을 한 컵 소분하여 꺼냈습니다. (남은 재고: {currentStock} / {maxStorage})");
            }
            else
            {
                Debug.Log("오뎅진열대: 현재 재고가 없습니다. 오뎅 기계에서 조리해 오세요.");
            }
        }
    }

    // ⭐ 재고 수량에 맞춰 자식 오뎅 오브젝트들을 실시간 켜고 끄는 순차 연출 함수
    private void UpdateVisuals()
    {
        for (int i = 0; i < odengVisualObjects.Count; i++)
        {
            if (odengVisualObjects[i] != null)
            {
                // i가 현재 재고(currentStock)보다 작을 때만 true가 됨
                // ex) 재고가 3개면: 0,1,2번 오브젝트는 SetActive(true), 3번부터는 false로 꺼짐!
                odengVisualObjects[i].SetActive(i < currentStock);
            }
        }
    }

    // ⭐ 진열대 UI 수치를 동기화해주는 메서드
    private void UpdateUI()
    {
        // 재고가 하나도 없다면 UI 패널 자체를 숨겨서 주방을 깔끔하게 유지
        if (displayUIPanel != null)
        {
            displayUIPanel.SetActive(currentStock > 0);
        }

        if (maxStorage <= 0) return;

        // 현재 남은 재고 비율 계산 (0.0 ~ 1.0)
        float progressNormalized = (float)currentStock / maxStorage;
        progressNormalized = Mathf.Clamp01(progressNormalized);

        // 원형 이미지 fillAmount 갱신
        if (circleProgressSlider != null)
        {
            circleProgressSlider.fillAmount = progressNormalized;
        }

        // 텍스트에 직관적으로 수량 표시 (예: "3 / 10")
        if (stockText != null)
        {
            stockText.text = $"{currentStock}/{maxStorage}";
        }
    }
}