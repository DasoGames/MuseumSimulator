using UnityEngine;
using UnityEngine.UI; // 💡 UI 제어용
using TMPro;        // 💡 텍스트 제어용

public class DeepFryer : MonoBehaviour, IInteractable
{
    [Header("튀김기 기계 고유 설정")]
    [Tooltip("UpgradeManager에 등록한 고유 식별 이름")]
    public string machineID = "DeepFryer";
    public float baseCookTime = 6f;      // 튀겨지는 기본 시간 (초)
    public float baseBurnTime = 7f;      // 완수 후 방치 시 타버리는(폐기) 시간 (초)

    [Header("치킨 결과물 데이터")]
    public FoodData cookedChickenData;   // 에셋 이름: "완성된치킨"
    public FoodData burntChickenData;    // 에셋 이름: "탄치킨"

    [Header("감자튀김 결과물 데이터")]
    public FoodData cookedPotatoData;    // 에셋 이름: "완성된감자튀김"
    public FoodData burntPotatoData;     // 에셋 이름: "탄감자튀김"

    [Header("⭐ 요리 결과 비주얼 오브젝트 설정")]
    [Tooltip("튀김기 자식으로 넣어둔 '완성된 치킨 3D 모델'을 연결하세요.")]
    public GameObject cookedChickenVisual;
    [Tooltip("튀김기 자식으로 넣어둔 '완성된 감자튀김 3D 모델'을 연결하세요.")]
    public GameObject cookedPotatoVisual;
    [Tooltip("튀김기 자식으로 넣어둔 '새까맣게 탄 튀김 3D 모델'을 연결하세요.")]
    public GameObject burntVisualObject;

    [Header("⭐ 투입 재료 배치 포인트 설정")]
    [Tooltip("생재료 튀김 바스켓이 잠시 소환될 위치(Transform)")]
    public Transform ingredientPoint;

    [Header("⭐ 조리 및 경고 UI 설정")]
    [Tooltip("튀김기 UI 전체를 감싸는 부모 오브젝트 (평소에는 꺼두기용)")]
    public GameObject fryerUIPanel;
    [Tooltip("원형 프로그레스 이미지 (Filled - Radial 360 설정 필수)")]
    public Image circleProgressSlider;
    [Tooltip("진행도 또는 경고를 띄울 TextMeshPro - Text")]
    public TMP_Text statusText;

    [Header("⭐ 연출 오브젝트 세팅 (FX)")]
    [Tooltip("🔥 튀겨지는 동안 지글지글 기름이 튀거나 연기가 모락모락 나는 조리 중 연출용 FX")]
    public GameObject cookingFX;

    [Header("⭐ UI 연출 색상 세팅")]
    public Color normalCookColor = Color.green; 
    public Color alertMaxColor = Color.red;     
    [Tooltip("방치 되었을 때 초당 깜빡거리는 속도")]
    public float blinkSpeed = 8f;

    // 단일 플로우 제어 내부 변수들
    private bool isReadyToCook = false; 
    private bool isCooking = false;     
    private bool isDone = false;        

    private float timer = 0f;
    private float targetCookTime = 0f;  
    
    // 💡 기계가 현재 무슨 재료를 튀기고 있는지 기억할 핵심 필드
    [SerializeField] private string currentIngredientName = "";
    private GameObject spawnedIngredientVisual = null;

    void Start()
    {
        // 첫 시작 시 모든 비주얼들과 UI, FX를 정돈하여 소등합니다.
        UpdateVisuals(false, false, false);
        if (fryerUIPanel != null) fryerUIPanel.SetActive(false);
        if (cookingFX != null) cookingFX.SetActive(false);
    }

    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // 💡 [상황 1] 요리가 완성되었거나 타버린 상태 -> 수거 프로세스
        if (isDone)
        {
            if (player.IsHoldingItem)
            {
                Debug.LogWarning("튀김기: 완성된 요리를 꺼내려면 손을 비워야 합니다!");
                return;
            }

            FoodData resultData = null;

            // 1. 💡 투입되었던 원재료 이름에 맞춰 지급할 결과물(치킨 vs 감자)과 탄 분기를 철저히 가릅니다.
            if (currentIngredientName == "손질된닭")
            {
                resultData = (timer >= baseBurnTime) ? burntChickenData : cookedChickenData;
            }
            else if (currentIngredientName == "손질된감자")
            {
                resultData = (timer >= baseBurnTime) ? burntPotatoData : cookedPotatoData;
            }

            // 플레이어 손에 최종 지급
            if (resultData != null)
            {
                player.HoldNewData(resultData);
                Debug.Log($"튀김기: 갓 튀겨진 [{resultData.foodName}]을(를) 바스켓에서 건져 올렸습니다!");
            }

            ResetFryer();
            return;
        }

        // 💡 [상황 2] 재료가 들어가서 조리 대기 중일 때 -> 빈손으로 누르면 즉시 가동!
        if (isReadyToCook && !isCooking)
        {
            if (player.IsHoldingItem)
            {
                Debug.LogWarning("튀김기: 조리를 시작하려면 손을 비우고 상호작용하세요!");
                return;
            }

            StartCooking();
            return;
        }

        // 💡 [상황 3] 현재 완전히 비어있는 상태 -> 손에 든 생재료 투입 처리
        if (!isReadyToCook && !isCooking && !isDone)
        {
            if (!player.IsHoldingItem)
            {
                Debug.Log("튀김기: 현재 비어있습니다. 손질된닭 또는 손질된감자를 가져와 투입하세요.");
                return;
            }

            FoodData heldData = player.CurrentHeldData;

            // 도마에서 썰어온 두 가지 손질 재료 이름 검사
            if (heldData.foodName == "손질된닭" || heldData.foodName == "손질된감자")
            {
                isReadyToCook = true;
                currentIngredientName = heldData.foodName; // 💡 기계에 어떤 재료가 담겼는지 저장!

                // ⭐ [비주얼 연동] 지정된 포인트 위치에 개별 스케일/회전값을 반영하여 소환합니다.
                if (ingredientPoint != null && heldData.prefab != null)
                {
                    Quaternion finalRotation = ingredientPoint.rotation * Quaternion.Euler(heldData.spawnRotation);
                    spawnedIngredientVisual = Instantiate(heldData.prefab, ingredientPoint.position, finalRotation, this.transform);
                    spawnedIngredientVisual.transform.localScale = heldData.spawnScale;
                }

                // 플레이어 손 비우기
                player.ClearHand();
                Debug.Log($"튀김기: [{currentIngredientName}] 투입 완료! 빈손으로 E키를 눌러 기름통에 담그세요.");
            }
            else
            {
                Debug.LogWarning("튀김기: 이 기계에는 도마에서 예쁘게 썰어온 '손질된닭'이나 '손질된감자'만 투입할 수 있습니다.");
            }
        }
    }

    private void StartCooking()
    {
        isReadyToCook = false;
        isCooking = true;
        timer = 0f;

        float speedModifier = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetSpeedModifier(machineID) : 1f;
        targetCookTime = baseCookTime * speedModifier;

        // UI 패널 온 및 튀김용 지글지글 FX 파티클 전격 활성화
        if (fryerUIPanel != null) fryerUIPanel.SetActive(true);
        if (circleProgressSlider != null) circleProgressSlider.color = normalCookColor;
        
        if (cookingFX != null) 
        {
            cookingFX.SetActive(true);
        }

        Debug.Log($"튀김기: 기름 속에 바스켓을 내렸습니다! 조리 시작 (목표 시간: {targetCookTime:F1}초)");
    }

    void Update()
    {
        // 1. 🔥 정직하게 기름 온도 유지하며 지글지글 바삭하게 익어가는 연산
        if (isCooking)
        {
            timer += Time.deltaTime;
            float progress = timer / targetCookTime;
            
            UpdateProgressUI(progress, $"{Mathf.RoundToInt(Mathf.Clamp01(progress) * 100f)}%");

            if (timer >= targetCookTime)
            {
                isCooking = false;
                isDone = true;
                timer = 0f; 

                // 조리가 완료되었으니 생재료 비주얼 정리 및 끓는 기름 이펙트 OFF
                if (spawnedIngredientVisual != null) Destroy(spawnedIngredientVisual);
                if (cookingFX != null) cookingFX.SetActive(false);

                // 💡 [비주얼 핵심] 튀겨진 재료 이름에 맞춰 치킨 3D 메쉬 혹은 감자튀김 3D 메쉬를 선별하여 켭니다!
                bool isChicken = (currentIngredientName == "손질된닭");
                UpdateVisuals(isChicken, !isChicken, false);

                Debug.Log($"튀김기: {currentIngredientName} 조리 완벽 완료! 빨리 안 건지면 시꺼멓게 탑니다.");
            }
        }

        // 2. 🚨 요리는 끝났으나 수거하지 않아 기름 속에서 기름을 먹고 타버리는 상태 (ALERT 연출)
        if (isDone)
        {
            timer += Time.deltaTime;
            float burnProgress = timer / baseBurnTime;

            if (timer < baseBurnTime)
            {
                // 실시간 초록색 ➡️ 새빨간색 그라데이션 변환
                Color currentAlertColor = Color.Lerp(normalCookColor, alertMaxColor, burnProgress);
                
                // 알파값을 깜빡이게 만듬 (사이렌 경고 효과)
                float blinkAlpha = Mathf.Lerp(0.2f, 1.0f, Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed)));
                currentAlertColor.a = blinkAlpha;

                if (circleProgressSlider != null)
                {
                    circleProgressSlider.color = currentAlertColor;
                    circleProgressSlider.fillAmount = 1f; 
                }

                if (statusText != null)
                {
                    statusText.text = "<color=red>WARN</color>";
                    statusText.alpha = blinkAlpha;
                }
            }
            else
            {
                // 완전히 버닝 타임을 초과하여 새까맣게 숯이 되어버린 순간
                if ((cookedChickenVisual != null && cookedChickenVisual.activeSelf) || (cookedPotatoVisual != null && cookedPotatoVisual.activeSelf))
                {
                    // 일반 음식 메쉬 싹 다 끄고 탄 메쉬 모델 가동
                    UpdateVisuals(false, false, true);
                    
                    if (circleProgressSlider != null) circleProgressSlider.fillAmount = 0f;
                    if (statusText != null) { statusText.text = "BURNED"; statusText.alpha = 1f; }
                    Debug.Log("<color=red>튀김기: 기름연기가 피어오르며 타이쿤 요리가 새까맣게 타버렸습니다!</color>");
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

    // 💡 재료 분기 및 타버린 임계점에 완벽 동기화된 3D 메쉬 연출 컨트롤러
    private void UpdateVisuals(bool showChicken, bool showPotato, bool showBurnt)
    {
        if (cookedChickenVisual != null) cookedChickenVisual.SetActive(showChicken);
        if (cookedPotatoVisual != null) cookedPotatoVisual.SetActive(showPotato);
        if (burntVisualObject != null) burntVisualObject.SetActive(showBurnt);
    }

    private void ResetFryer()
    {
        isReadyToCook = false;
        isCooking = false;
        isDone = false;
        timer = 0f;
        targetCookTime = 0f;
        currentIngredientName = "";

        if (spawnedIngredientVisual != null) Destroy(spawnedIngredientVisual);
        if (fryerUIPanel != null) fryerUIPanel.SetActive(false);
        if (cookingFX != null) cookingFX.SetActive(false);

        UpdateVisuals(false, false, false);
        Debug.Log("튀김기: 바스켓 청소 및 기름 정제 완료! 새로운 재료 투입 가능.");
    }
}