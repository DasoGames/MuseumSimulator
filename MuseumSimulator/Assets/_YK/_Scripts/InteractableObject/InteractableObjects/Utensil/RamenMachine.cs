using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 💡 UI 슬라이더 이미지 제어용 추가
using TMPro;        // 💡 TextMeshPro 제어용 추가

public class RamenMachine : MonoBehaviour, IInteractable
{
    [Header("라면 기계 설정")]
    [Tooltip("UpgradeManager에 등록한 고유 식별 이름")]
    public string machineID = "RamenMachine";
    public float baseCookTime = 6f;     // 조리 시작 후 라면이 끓는 기본 시간 (초)
    public float baseBurnTime = 7f;     // 완수 후 방치 시 불거나 타버리는(폐기) 시간 (초)

    [Header("결과물 데이터")]
    public FoodData cookedRamenData; // 인스펙터 에셋 이름: "완성된라면"
    public FoodData burntRamenData;  // 인스펙터 에셋 이름: "탄라면" (불어터진라면)

    [Header("기계 상태 (Debug)")]
    [SerializeField] private bool hasRamenIngredient = false; // 기계에 라면 재료가 투입되었는가?
    [SerializeField] private bool isCooking = false;           // 현재 보글보글 끓는 중인가?
    [SerializeField] private bool isDone = false;              // 조리가 완료되었는가?

    private float timer = 0f;
    private float targetCookTime = 0f;  // 업그레이드가 반영된 최종 조리 시간

    [Header("⭐ 시각적 연출용 오브젝트 (3D 메쉬)")]
    [Tooltip("용기에 담긴 생라면/스프 상태의 3D 비주얼")]
    public GameObject ingredientVisual; 
    [Tooltip("맛있게 끓여진 라면 상태의 3D 비주얼")]
    public GameObject cookedVisual;
    [Tooltip("새까맣게 타거나 불어터진 라면 상태의 3D 비주얼 (없다면 cookedVisual과 같이 세팅 가능)")]
    public GameObject burntVisualObject;

    [Header("⭐ 조리 및 경고 UI 설정")]
    [Tooltip("라면기계 UI 전체를 감싸는 부모 캔버스/패널 (재료가 투입되면 켜집니다!)")]
    public GameObject ramenUIPanel;
    [Tooltip("원형 프로그레스 이미지 (Filled - Radial 360 설정 필수)")]
    public Image circleProgressSlider;
    [Tooltip("진행도(%) 또는 경고(WARN/BURNED)를 띄울 TextMeshPro")]
    public TMP_Text statusText;

    [Header("⭐ 연출 오브젝트 세팅 (FX)")]
    [Tooltip("💨 조리 시작 시 보글보글 끓거나 뜨거운 김이 모락모락 나는 연출용 파티클 FX")]
    public GameObject cookingFX;

    [Header("⭐ UI 연출 색상 및 속도 세팅")]
    public Color normalCookColor = Color.green; 
    public Color alertMaxColor = Color.red;     
    [Tooltip("완성 후 방치 되었을 때 UI가 깜빡거리는 초당 속도")]
    public float blinkSpeed = 8f;

    private void Start()
    {
        // 첫 시작 시 모든 비주얼들과 UI, FX를 깔끔하게 소등(초기화)합니다.
        UpdateVisuals(false, false, false);
        if (ramenUIPanel != null) ramenUIPanel.SetActive(false);
        if (cookingFX != null) cookingFX.SetActive(false);
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
        Debug.Log($"<color=#00FF00><b>🔍 [라면 기계 상호작용 검문]</b>\n" +
                  $"▶ 클릭한 플레이어 손: [{heldItemName}]\n" +
                  $"▶ 기계 상태: 재료투입={hasRamenIngredient} | 조리중={isCooking} | 완료={isDone}\n" +
                  $"▶ 현재 타이머: {timer:F1}초</color>");

        // 💡 [단계 3] 조리가 완전히 끝난 상태 -> 빈손으로 오면 완성작 수거
        if (isDone)
        {
            Debug.Log("<color=cyan><b>[플로우 진입]</b> 단계 3: 조리 완료 상태 수거 프로세스 가동</color>");
            if (player.IsHoldingItem)
            {
                Debug.LogWarning($"<color=orange><b>⚠️ [수거 거부]</b> 플레이어가 [{heldItemName}]을 들고 있습니다. 빈손이어야 수거 가능합니다.</color>");
                return;
            }

            FoodData resultData = null;

            // 방치 타이머 체크 후 알맞은 FoodData 에셋을 결정
            if (timer >= baseBurnTime)
            {
                resultData = burntRamenData;
                Debug.Log("<color=red>라면기계: 너무 방치해서 쫄아붙은 라면이 수거됩니다.</color>");
            }
            else
            {
                resultData = cookedRamenData;
                Debug.Log("<color=lime>라면기계: 기가 막히게 익은 라면이 수거됩니다!</color>");
            }

            // 플레이어 손에 최종 지급
            if (resultData != null)
            {
                player.HoldNewData(resultData);
                Debug.Log($"<color=green><b>🏁 [수거 성공]</b> [{resultData.foodName}] 데이터를 플레이어 손에 할당했습니다.</color>");
            }

            ResetMachine();
            return;
        }

        // 💡 [단계 2] 재료가 들어가 있고 아직 조리 전일 때 -> 빈손으로 툭 누르면 조리 시작!
        if (hasRamenIngredient && !isCooking && !isDone)
        {
            Debug.Log("<color=cyan><b>[플로우 진입]</b> 단계 2: 재료 세팅 완료 상태 -> 버튼 입력 조리 가동 프로세스</color>");
            if (player.IsHoldingItem)
            {
                Debug.LogWarning($"<color=orange><b>⚠️ [가동 거부]</b> 조리를 시작하려면 손을 비우고 상호작용하세요! 현재 손: [{heldItemName}]</color>");
                return;
            }

            StartCooking();
            return;
        }

        // 💡 [단계 1] 기계가 완전히 비어있을 때 -> 생라면 재료 투입하기
        if (!hasRamenIngredient && !isCooking && !isDone)
        {
            Debug.Log("<color=cyan><b>[플로우 진입]</b> 단계 1: 완전히 비어있는 대기 상태 -> 재료 검문 프로세스</color>");
            if (!player.IsHoldingItem)
            {
                Debug.Log("<color=white>라면기계: 현재 비어있습니다. 조리할 '라면' 재료를 들고 오세요.</color>");
                return;
            }

            FoodData heldData = player.CurrentHeldData;

            // 데이터 규칙에 맞게 'foodName' 필드로 정확하게 확인합니다.
            if (heldData.foodName == "RawRamen")
            {
                hasRamenIngredient = true;
                
                // 플레이어 손 깔끔하게 클리어
                player.ClearHand(); 
                
                // 💡 [핵심 요청사항 반영]: 라면이 들어간 즉시 대기 UI 패널 작동 및 생라면 비주얼 온!
                if (ramenUIPanel != null) ramenUIPanel.SetActive(true);
                UpdateProgressUI(0f, "READY"); // 시작 전이므로 0% 및 대기 문구 출력
                if (circleProgressSlider != null) circleProgressSlider.color = normalCookColor;

                UpdateVisuals(true, false, false); // 생라면 메쉬만 활성화
                
                Debug.Log("<color=lime><b>📦 [재료 투입 성공]</b> 라면 기계 용기에 재료가 안착되어 UI가 활성화되었습니다. 빈손으로 한번 더 누르면 물이 나옵니다.</color>");
            }
            else
            {
                Debug.LogWarning($"<color=red><b>❌ [투입 거절]</b> [{heldData.foodName}]은(는) 라면 기계에 넣을 수 없습니다. 오직 '라면'만 허용됩니다.</color>");
            }
        }
    }

    // 완전히 자동으로 끓이기 시작하는 함수
    private void StartCooking()
    {
        isCooking = true;
        timer = 0f;

        // [업그레이드 연동] UpgradeManager를 통해 조리 시간 단축 비율 계산
        float speedModifier = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetSpeedModifier(machineID) : 1f;
        targetCookTime = baseCookTime * speedModifier;

        // 💡 [핵심 요청사항 반영]: 조리 시작 시 보글보글/연기 끓는 FX 오작동 없이 즉시 활성화!
        if (cookingFX != null) 
        {
            cookingFX.SetActive(true);
        }

        Debug.Log($"<color=red><b>🔥 [라면 조리 가동]</b> 온수 주입 완료! 보글보글 끓기 시작합니다. (목표 시간: {targetCookTime:F1}초)</color>");
    }

    void Update()
    {
        // 1. 🍜 자동으로 보글보글 끓는 타이머 조리 연산
        if (isCooking)
        {
            timer += Time.deltaTime;
            float progress = timer / targetCookTime;
            
            // UI 원형 슬라이더 및 텍스트 1:1 리얼타임 동기화
            UpdateProgressUI(progress, $"{Mathf.RoundToInt(Mathf.Clamp01(progress) * 100f)}%");

            if (timer >= targetCookTime)
            {
                isCooking = false;
                isDone = true;
                timer = 0f; // 이 타이머는 이제 완성 후 방치 시간(탄 타이머)으로 전환됩니다.

                // 조리가 완료되었으므로 물 끓는 이펙트(FX)는 임시 소등
                if (cookingFX != null) cookingFX.SetActive(false);

                // 3D 비주얼 스위칭: 생라면 메쉬 끄고 다 익은 라면 메쉬 온!
                UpdateVisuals(false, true, false);

                Debug.Log("<color=yellow><b>🔔 [조리 완료 알림]</b> 라면 조리가 끝났습니다! 면이 불기 전에 어서 꺼내가세요.</color>");
            }
        }
        // 2. 🚨 완성 후 방치 타이머 작동 (너무 오래 두면 불어터지고 시꺼멓게 탄 라면이 됨)
        else if (isDone)
        {
            timer += Time.deltaTime;
            float burnProgress = timer / baseBurnTime;

            if (timer < baseBurnTime)
            {
                // 실시간 초록색 ➡️ 새빨간색 경고 그라데이션 변환
                Color currentAlertColor = Color.Lerp(normalCookColor, alertMaxColor, burnProgress);
                
                // 알파값(투명도)을 Sine 파형으로 꼬아 사이렌 경고 효과 부여
                float blinkAlpha = Mathf.Lerp(0.2f, 1.0f, Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed)));
                currentAlertColor.a = blinkAlpha;

                if (circleProgressSlider != null)
                {
                    circleProgressSlider.color = currentAlertColor;
                    circleProgressSlider.fillAmount = 1f; // 경고창일 땐 게이지 가득 채우기
                }

                if (statusText != null)
                {
                    statusText.text = "<color=red>WARN</color>";
                    statusText.alpha = blinkAlpha;
                }
            }
            else
            {
                // 완전히 버닝 타임을 초과하여 불어터지고 숯이 되어버린 순간
                if ((cookedVisual != null && cookedVisual.activeSelf) || (ingredientVisual != null && ingredientVisual.activeSelf))
                {
                    // 정상 라면 메쉬 내리고 새까만 탄 라면 메쉬 가동
                    UpdateVisuals(false, false, true);
                    
                    if (circleProgressSlider != null) circleProgressSlider.fillAmount = 0f;
                    if (statusText != null) { statusText.text = "BURNED"; statusText.alpha = 1f; }
                    Debug.LogWarning("<color=black><b>💀 [탄화 완료]</b> 라면 국물이 전부 졸아붙어 폐기물 수준으로 타버렸습니다.</color>");
                }
            }
        }
    }

    private void UpdateProgressUI(float progressNormalized, string textMessage)
    {
        progressNormalized = Mathf.Clamp01(progressNormalized);
        if (circleProgressSlider != null) circleProgressSlider.fillAmount = progressNormalized;
        if (statusText != null)
        {
            statusText.text = textMessage;
            statusText.alpha = 1f;
        }
    }

    // 💡 라면 진행 프로세스에 완벽 대응하는 삼원 분기 비주얼 활성/비활성화 컨트롤러
    private void UpdateVisuals(bool showIngredient, bool showCooked, bool showBurnt)
    {
        if (ingredientVisual != null) ingredientVisual.SetActive(showIngredient);
        if (cookedVisual != null) cookedVisual.SetActive(showCooked);
        if (burntVisualObject != null) burntVisualObject.SetActive(showBurnt);
    }

    // 기계 완전 초기화 및 마감 리셋
    private void ResetMachine()
    {
        hasRamenIngredient = false;
        isCooking = false;
        isDone = false;
        timer = 0f;
        targetCookTime = 0f;

        if (ramenUIPanel != null) ramenUIPanel.SetActive(false);
        if (cookingFX != null) cookingFX.SetActive(false);

        UpdateVisuals(false, false, false);
        Debug.Log("<color=white>라면기계: 바닥 찌꺼기 세척 및 시스템 초기화 완료! 다음 손님 재료 대기중.</color>");
    }
}