using UnityEngine;
using UnityEngine.UI; // 원형 슬라이더(Image) 제어용
using TMPro;        // 텍스트 제어용

public class Blender : MonoBehaviour, IInteractable
{
    [Header("블렌더 고유 설정")]
    [Tooltip("UpgradeManager에 등록한 고유 식별 이름")]
    public string machineID = "Blender";
    [Tooltip("주스를 완벽히 갈아내기 위해 꾹 눌러야 하는 기본 시간 (초)")]
    public float baseBlendTime = 5f; 

    [Header("결과물 데이터")]
    [Tooltip("갈기가 완료되었을 때 지급할 과일주스 에셋")]
    public FoodData cookedJuiceData; 

    [Header("블렌더 내부 3D 배치 위치")]
    public Transform fruitPlaceTransform; // 블렌더 안에 과일 비주얼을 띄울 위치

    [Header("⭐ 블렌더 UI 설정")]
    [Tooltip("블렌더 UI 전체를 감싸는 부모 오브젝트 (비어있을 때는 숨기기용)")]
    public GameObject blenderUIPanel;
    
    [Tooltip("Image Type이 Filled(Radial 360)로 설정된 원형 게이지 이미지")]
    public Image circleProgressSlider;
    
    [Tooltip("현재 진행도 퍼센트를 표시할 TextMeshPro - Text UI")]
    public TMP_Text statusText;

    [Header("⭐ 연출 오브젝트 세팅 (FX)")]
    [Tooltip("🌪️ 플레이어가 E키를 꾹 누르고 갈 때만 활성화될 믹싱 파티클 FX 오브젝트")]
    public GameObject blendingFX;

    [Header("⭐ UI 색상 설정")]
    public Color blendColor = Color.green; // 갈리는 중일 때 게이지 색상

    // 내부 상태 제어 변수들 (도마와 정밀 매칭)
    private bool hasFruit = false;       // 현재 블렌더에 재료가 올라가 있는가?
    private float blendTimer = 0f;       // 현재까지 누른 누적 시간
    private float targetBlendTime = 0f;  // 업그레이드가 반영된 최종 목표 시간
    
    private GameObject spawnedVisual = null; // 내부 시각 연출용 생과일 프리팹 원본
    private PlayerInteractor targetPlayer = null;

    void Start()
    {
        // 💡 [도마 규칙] 첫 시작 시 당연히 비어있으므로 UI 패널과 갈기 FX를 숨깁니다.
        if (blenderUIPanel != null) blenderUIPanel.SetActive(false);
        if (blendingFX != null) blendingFX.SetActive(false);
    }

    // 💡 인터페이스 구현 (E키를 톡 눌렀을 때 실행)
    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // [단계 1] 블렌더가 비어있고, 플레이어가 갈 수 있는 과일을 들고 있을 때 -> 블렌더에 넣기
        if (!hasFruit && player.IsHoldingItem)
        {
            FoodData heldData = player.CurrentHeldData;

            // 기획하신 과일 이름 필터링 (여기서는 "Kiwi" 기준으로 매칭)
            if (heldData.foodName == "Kiwi")
            {
                hasFruit = true;
                blendTimer = 0f;

                // [독립적 업그레이드 연동]
                float speedModifier = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetSpeedModifier(machineID) : 1f;
                targetBlendTime = baseBlendTime * speedModifier;

                // ⭐ 재료가 들어가는 즉시 슬라이더를 0%로 초기화하고 UI 패널 가동
                UpdateUI(0f);
                if (blenderUIPanel != null) 
                {
                    blenderUIPanel.SetActive(true);
                }

                // 프리팹 정보를 사용해 블렌더 내부에 과일 3D 비주얼 생성 및 고유 크기/회전 연동
                if (heldData.prefab != null && fruitPlaceTransform != null)
                {
                    Quaternion finalRotation = fruitPlaceTransform.rotation * Quaternion.Euler(heldData.spawnRotation);
                    spawnedVisual = Instantiate(heldData.prefab, fruitPlaceTransform.position, finalRotation, fruitPlaceTransform);
                    spawnedVisual.transform.localScale = heldData.spawnScale;
                }

                // 플레이어 손 비우기
                player.ClearHand();
                Debug.Log($"블렌더: '{heldData.foodName}' 투입 완료. 손을 비우고 E키를 꾹 눌러 주스를 가세요!");
            }
            else
            {
                Debug.LogWarning("블렌더: 이 기계에는 갈 수 있는 과일(Kiwi)만 투입할 수 있습니다.");
            }
        }
    }

    void Update()
    {
        // 블렌더에 과일이 없으면 꾹 누르기 타이머 로직을 완전히 건너뜁니다.
        if (!hasFruit) return;

        if (targetPlayer == null)
        {
            targetPlayer = FindFirstObjectByType<PlayerInteractor>();
            if (targetPlayer == null) return;
        }

        // [단계 2] 플레이어가 빈손으로 E 키를 진짜 꾹 누르고 있을 때 (갈기 가동)
        if (Input.GetKey(KeyCode.E) && !targetPlayer.IsHoldingItem)
        {
            // 정직하게 누른 시간 누적
            blendTimer += Time.deltaTime;
            
            // UI 슬라이더와 텍스트 실시간 갱신 (0.0 ~ 1.0 비율 전달)
            UpdateUI(blendTimer / targetBlendTime);

            // ⭐ [FX 연출] 칼날이 돌고 주스가 갈리는 중이므로 갈기 FX 오브젝트 전격 활성화!
            if (blendingFX != null && !blendingFX.activeSelf)
            {
                blendingFX.SetActive(true);
            }

            // 갈기 완료 조건 충족
            if (blendTimer >= targetBlendTime)
            {
                CompleteBlending();
            }
        }
        else
        {
            // ⭐ [FX 연출] E키를 떼거나 중간에 다른 행동을 하면 즉시 이펙트를 꺼서 믹서기 정지 연출!
            if (blendingFX != null && blendingFX.activeSelf)
            {
                blendingFX.SetActive(false);
            }
        }
    }

    // UI를 실시간 동기화해 주는 내부 헬퍼 메서드
    private void UpdateUI(float progressNormalized)
    {
        progressNormalized = Mathf.Clamp01(progressNormalized);

        if (circleProgressSlider != null)
        {
            circleProgressSlider.color = blendColor;
            circleProgressSlider.fillAmount = progressNormalized;
        }

        if (statusText != null)
        {
            statusText.text = $"{Mathf.RoundToInt(progressNormalized * 100f)}%";
        }
    }

    // 완전히 다 갈려서 최종 주스를 지급하고 기계를 정리하는 함수
    void CompleteBlending()
    {
        hasFruit = false;
        blendTimer = 0f;

        // ⭐ 주스가 다 완성되었으므로 UI 패널과 회전 FX를 통째로 정리하여 숨깁니다.
        if (blenderUIPanel != null) blenderUIPanel.SetActive(false);
        if (blendingFX != null) blendingFX.SetActive(false);

        // 블렌더 안 원재료 과일 3D 모델 삭제
        if (spawnedVisual != null)
        {
            Destroy(spawnedVisual);
            spawnedVisual = null;
        }

        // 결과물 주스를 즉시 플레이어 손에 넘겨줌
        if (targetPlayer != null && cookedJuiceData != null)
        {
            targetPlayer.HoldNewData(cookedJuiceData);
            Debug.Log($"블렌더: 믹싱 완료! 플레이어에게 '{cookedJuiceData.foodName}' 주스를 지급했습니다.");
        }
    }
}