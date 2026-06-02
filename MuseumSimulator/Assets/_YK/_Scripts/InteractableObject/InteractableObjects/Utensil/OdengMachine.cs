using UnityEngine;
using UnityEngine.UI; 
using TMPro;        
using System.Collections.Generic;

public class OdengMachine : MonoBehaviour, IInteractable
{
    [Header("오뎅 기계 고유 설정")]
    [Tooltip("UpgradeManager에 등록한 고유 식별 이름")]
    public string machineID = "OdengMachine";

    public float baseCookTime = 8f;      // 오뎅 조리에 걸리는 기본 시간 (초)
    public float baseBurnTime = 10f;     // 완성 후 불어 터지기(타버리기)까지의 기본 방치 시간 (초)

    [Header("결과물 데이터")]
    public FoodData cookedOdengData;     
    public FoodData burntOdengData;      

    [Header("⭐ 요리 결과 비주얼 오브젝트 설정")]
    [Tooltip("오뎅기계 자식으로 넣어둔 '완성된 오뎅판 3D 모델' 오브젝트를 연결하세요.")]
    public GameObject cookedVisualObject;

    [Tooltip("오뎅기계 자식으로 넣어둔 '불어서 망가진 오뎅판 3D 모델' 오브젝트를 연결하세요.")]
    public GameObject burntVisualObject;

    [Header("⭐ 투입 재료 배치 포인트 설정")]
    [Tooltip("재료가 소환될 위치(Transform)를 연결하세요. (빈 오브젝트)")]
    public Transform ingredientPoint;

    [Header("⭐ 조리 및 경고 UI 설정")]
    public GameObject odengUIPanel;
    public Image circleProgressSlider;
    public TMP_Text statusText;

    [Header("⭐ 연출 오브젝트 세팅 (FX)")]
    [Tooltip("🔥 육수가 보글보글 끓거나 김이 모락모락 나는 조리 중 연출용 FX 오브젝트")]
    public GameObject cookingFX;

    [Header("⭐ UI 연출 색상 세팅")]
    public Color normalCookColor = Color.green; 
    public Color alertMaxColor = Color.red;     
    [Tooltip("방치 되었을 때 초당 깜빡거리는 속도")]
    public float blinkSpeed = 8f;

    // 단일 플로우 제어 변수들
    private bool isReadyToCook = false; 
    private bool isCooking = false;     
    private bool isDone = false;        

    private float timer = 0f;
    private float targetCookTime = 0f;  
    
    private GameObject spawnedIngredientVisual = null;

    void Start()
    {
        UpdateVisuals(false, false);
        if (odengUIPanel != null) odengUIPanel.SetActive(false);
        
        // 💡 게임 시작 시 조리 FX 이펙트는 안전하게 꺼둡니다.
        if (cookingFX != null) cookingFX.SetActive(false);
    }

    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // [상황 1] 수거 프로세스
        if (isDone)
        {
            if (player.IsHoldingItem)
            {
                Debug.LogWarning("오뎅기계: 완성된 요리를 꺼내려면 손을 비워야 합니다!");
                return;
            }

            if (timer >= baseBurnTime)
            {
                if (burntOdengData != null) player.HoldNewData(burntOdengData);
                Debug.Log("오뎅기계: 너무 오래 방치되어 퉁퉁 불어 터진 오뎅을 꺼냈습니다.");
            }
            else
            {
                if (cookedOdengData != null) player.HoldNewData(cookedOdengData);
                Debug.Log("오뎅기계: 맛있게 익은 국물 가득 오뎅 꼬치를 꺼냈습니다!");
            }

            ResetMachine();
            return;
        }

        // [상황 2] 조리 시작!
        if (isReadyToCook && !isCooking)
        {
            if (player.IsHoldingItem)
            {
                Debug.LogWarning("오뎅기계: 조리를 시작하려면 손을 비우고 상호작용하세요!");
                return;
            }

            StartCooking();
            return;
        }

        // [상황 3] 생재료 투입
        if (!isReadyToCook && !isCooking && !isDone)
        {
            if (!player.IsHoldingItem)
            {
                Debug.Log("오뎅기계: 현재 비어있습니다. 날것의 오뎅 재료를 채워주세요.");
                return;
            }

            FoodData heldData = player.CurrentHeldData;

            if (heldData.foodName == "Raw Odengs")
            {
                isReadyToCook = true;

                if (ingredientPoint != null && heldData.prefab != null)
                {
                    Quaternion finalRotation = ingredientPoint.rotation * Quaternion.Euler(heldData.spawnRotation);
                    spawnedIngredientVisual = Instantiate(heldData.prefab, ingredientPoint.position, finalRotation, this.transform);
                    spawnedIngredientVisual.transform.localScale = heldData.spawnScale;
                }

                player.ClearHand();
                Debug.Log("오뎅기계: 재료 안착 완료! 빈손으로 E키를 눌러 육수를 끓이세요.");
            }
            else
            {
                Debug.LogWarning("오뎅기계: 이 기계에는 지정된 오뎅 생재료만 넣을 수 있습니다.");
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

        if (odengUIPanel != null) odengUIPanel.SetActive(true);
        if (circleProgressSlider != null) circleProgressSlider.color = normalCookColor;

        // ⭐ [FX 연출] 불을 켜고 끓이기 시작했으므로 조리용 연기/거품 FX 활성화!
        if (cookingFX != null) 
        {
            cookingFX.SetActive(true);
        }

        Debug.Log($"오뎅기계: 조리를 시작합니다! 불이 켜져 육수 FX가 가동됩니다.");
    }

    void Update()
    {
        // 1. 🔥 정직하게 오뎅이 익어가는 상태
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

                // 생재료 비주얼 청소
                if (spawnedIngredientVisual != null) Destroy(spawnedIngredientVisual);

                // ⭐ [FX 연출] 조리가 완료되어 불을 껐으므로 끓는 연기/거품 FX를 꺼줍니다.
                if (cookingFX != null) 
                {
                    cookingFX.SetActive(false);
                }

                // 완성 3D 모델 ON
                UpdateVisuals(true, false);
                Debug.Log("오뎅기계: 오뎅 조리 완료! 방치하면 불어 터집니다.");
            }
        }

        // 2. 🚨 요리는 끝났으나 수거하지 않아 퉁퉁 불어 터지는 위험 상태 (ALERT 연출)
        if (isDone)
        {
            timer += Time.deltaTime;
            float burnProgress = timer / baseBurnTime;

            if (timer < baseBurnTime)
            {
                Color currentAlertColor = Color.Lerp(normalCookColor, alertMaxColor, burnProgress);
                float blinkAlpha = Mathf.Lerp(0.2f, 1.0f, Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed)));
                currentAlertColor.a = blinkAlpha;

                if (circleProgressSlider != null)
                {
                    circleProgressSlider.color = currentAlertColor;
                    circleProgressSlider.fillAmount = 1f; 
                }

                if (statusText != null)
                {
                    statusText.text = "<color=red>!!</color>";
                    statusText.alpha = blinkAlpha;
                }
            }
            else
            {
                // 완전히 버닝 타임을 초과하여 퉁퉁 불어 터진 순간
                if (cookedVisualObject != null && cookedVisualObject.activeSelf)
                {
                    UpdateVisuals(false, true);
                    
                    if (circleProgressSlider != null) circleProgressSlider.fillAmount = 0f;
                    if (statusText != null) { statusText.text = "SPOILED"; statusText.alpha = 1f; }
                    Debug.Log("<color=red>오뎅기계: 결국 오뎅이 너무 불어서 상품 가치를 상실했습니다!</color>");
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

    private void UpdateVisuals(bool showCooked, bool showBurnt)
    {
        if (cookedVisualObject != null) cookedVisualObject.SetActive(showCooked);
        if (burntVisualObject != null) burntVisualObject.SetActive(showBurnt);
    }

    private void ResetMachine()
    {
        isReadyToCook = false;
        isCooking = false;
        isDone = false;
        timer = 0f;
        targetCookTime = 0f;

        if (spawnedIngredientVisual != null) Destroy(spawnedIngredientVisual);
        if (odengUIPanel != null) odengUIPanel.SetActive(false);

        // ⭐ [FX 연출] 수거해가거나 강제 초기화가 일어날 때 혹시 켜져 있을 FX를 안전하게 소등
        if (cookingFX != null) 
        {
            cookingFX.SetActive(false);
        }

        UpdateVisuals(false, false);
    }
}