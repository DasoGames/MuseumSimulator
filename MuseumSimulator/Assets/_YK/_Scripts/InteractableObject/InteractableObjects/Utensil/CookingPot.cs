using UnityEngine;
using UnityEngine.UI; // 💡 UI 제어용
using TMPro;
using System.Collections.Generic;        // 💡 텍스트 제어용

public class CookingPot : MonoBehaviour, IInteractable
{
    [Header("냄비 고유 설정")]
    [Tooltip("UpgradeManager에 등록한 고유 식별 이름")]
    public string machineID = "CookingPot";

    public float baseCookTime = 10f;     // 떡볶이 조리에 걸리는 기본 시간 (초)
    public float baseBurnTime = 8f;      // 완성 후 타버리기까지의 기본 방치 시간 (초)

    [Header("결과물 데이터")]
    public FoodData cookedTteokbokkiData; // 인스펙터 에셋 이름: "완성된떡볶이"
    public FoodData burntTteokbokkiData;  // 인스펙터 에셋 이름: "탄떡볶이"

    [Header("⭐ 요리 결과 비주얼 오브젝트 설정")]
    [Tooltip("냄비 자식으로 넣어둔 '완성된 떡볶이 3D 모델' 오브젝트를 연결하세요.")]
    public GameObject cookedVisualObject;

    [Tooltip("냄비 자식으로 넣어둔 '탄 떡볶이 3D 모델' 오브젝트를 연결하세요.")]
    public GameObject burntVisualObject;

    [Header("⭐ 투입 재료 배치 포인트 설정")]
    [Tooltip("재료 3개가 각각 배치될 위치(Transform)를 순서대로 3개 연결하세요.\n(0: 떡 위치, 1: 고추장 위치, 2: 야채 위치)")]
    public List<Transform> ingredientPoints = new List<Transform>();

    public GameObject FX;

    [Header("⭐ 조리 및 경고 UI 설정")]
    [Tooltip("냄비 UI 전체를 감싸는 부모 오브젝트 (평소에는 꺼두기용)")]
    public GameObject potUIPanel;
    [Tooltip("원형 프로그레스 이미지 (Filled - Radial 360 설정 필수)")]
    public Image circleProgressSlider;
    [Tooltip("진행도 또는 경고를 띄울 TextMeshPro - Text")]
    public TMP_Text statusText;

    [Header("⭐ UI 연출 색상 세팅")]
    public Color normalCookColor = Color.white; // 기본 조리 중일 때 게이지 색상
    public Color alertMaxColor = Color.red;     // 완전히 다 타기 직전의 새빨간 경고 색상
    [Tooltip("방치되었을 때 초당 깜빡거리는 속도 (높을수록 방정맞게 깜빡임)")]
    public float blinkSpeed = 8f;

    // 떡볶이에 필요한 3가지 고정 재료 이름 목록 (FoodData의 foodName 기준)
    private readonly List<string> requiredIngredients = new List<string> { "Ricecake", "Gochujang", "SlicedVegetables" };
    
    [Header("현재 냄비에 들어간 재료 목록 (Debug)")]
    [SerializeField] private List<string> currentIngredients = new List<string>();
    private List<GameObject> spawnedIngredientVisuals = new List<GameObject>();

    private bool isReadyToCook = false; 
    private bool isCooking = false;     
    private bool isDone = false;        

    private float timer = 0f;
    private float targetCookTime = 0f;  // 업그레이드가 반영된 최종 조리 목표 시간

    void Start()
    {
        UpdateVisuals(false, false);
        // 💡 시작할 때는 냄비 UI를 숨겨둡니다.
        if (potUIPanel != null) potUIPanel.SetActive(false);
    }

    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // [상황 1] 요리가 완성되었거나 타버린 상태 -> 수거
        if (isDone)
        {
            if (player.IsHoldingItem)
            {
                Debug.LogWarning("냄비: 완성된 요리를 꺼내려면 손을 비워야 합니다!");
                return;
            }

            if (timer >= baseBurnTime)
            {
                if (burntTteokbokkiData != null) player.HoldNewData(burntTteokbokkiData);
                Debug.Log("냄비: 너무 오래 방치되어 새까맣게 탄 떡볶이를 꺼냈습니다.");
                FX.SetActive(false);
            }
            else
            {
                if (cookedTteokbokkiData != null) player.HoldNewData(cookedTteokbokkiData);
                Debug.Log("냄비: 맛있게 조리된 완성된 떡볶이를 꺼냈습니다!");
                FX.SetActive(false);
            }

            ResetPot();
            return;
        }

        // [상황 2] 조리 시작 (빈손 상호작용)
        if (isReadyToCook && !isCooking)
        {
            if (player.IsHoldingItem)
            {
                Debug.LogWarning("냄비: 조리를 시작하려면 손을 비우고 상호작용하세요!");
                return;
            }

            StartCooking();
            return;
        }

        // [상황 3] 재료 투입
        if (!isReadyToCook && !isCooking && !isDone)
        {
            if (!player.IsHoldingItem)
            {
                Debug.Log("냄비: 아직 재료가 부족합니다. 떡볶이떡, 양념, 손질된야채를 채워주세요.");
                return;
            }

            FoodData heldData = player.CurrentHeldData;

            if (requiredIngredients.Contains(heldData.foodName) && !currentIngredients.Contains(heldData.foodName))
            {
                int ingredientIndex = requiredIngredients.IndexOf(heldData.foodName);
                currentIngredients.Add(heldData.foodName);
                Debug.Log($"냄비: '{heldData.foodName}' 투입 완료! ({currentIngredients.Count} / {requiredIngredients.Count})");

                if (ingredientIndex < ingredientPoints.Count && ingredientPoints[ingredientIndex] != null)
                {
                    Transform targetPoint = ingredientPoints[ingredientIndex];
                    if (heldData.prefab != null) 
                    {
                        Quaternion finalRotation = targetPoint.rotation * Quaternion.Euler(heldData.spawnRotation);
                        GameObject visual = Instantiate(heldData.prefab, targetPoint.position, finalRotation, this.transform);
                        visual.transform.localScale = heldData.spawnScale;
                        spawnedIngredientVisuals.Add(visual); 
                    }
                }
                
                player.ClearHand();

                if (currentIngredients.Count == requiredIngredients.Count)
                {
                    isReadyToCook = true;
                    Debug.Log("냄비: 모든 재료가 모여 [조리 가능 상태]가 되었습니다! 빈손으로 E키를 눌러 불을 켜세요.");
                }
            }
            else
            {
                Debug.LogWarning("냄비: 이 재료는 떡볶이에 들어가지 않거나 이미 넣은 재료입니다.");
            }
        }
    }

    private void StartCooking()
    {
        isReadyToCook = false;
        isCooking = true;
        timer = 0f;

        float speedModifier = 1f;
        if (UpgradeManager.Instance != null)
        {
            speedModifier = UpgradeManager.Instance.GetSpeedModifier(machineID);
        }
        targetCookTime = baseCookTime * speedModifier;

        // 💡 불을 켜는 순간 UI를 활성화하고 정상 조리 스킨으로 세팅
        if (potUIPanel != null) potUIPanel.SetActive(true);
        if (circleProgressSlider != null) circleProgressSlider.color = normalCookColor;

        Debug.Log($"냄비: 불을 켭니다! 조리 시작");
        
    }

    void Update()
    {
        // 1. 🔥 정직하게 떡볶이가 보글보글 끓으며 만들어지는 상태
        if (isCooking)
        {
            timer += Time.deltaTime;
            float progress = timer / targetCookTime;
            
            // UI 슬라이더 차오르게 하고 퍼센트 갱신
            UpdateProgressUI(progress, $"{Mathf.RoundToInt(Mathf.Clamp01(progress) * 100f)}%");
            FX.SetActive(true);

            if (timer >= targetCookTime)
            {
                isCooking = false;
                isDone = true;
                timer = 0f; 

                ClearSpawnedIngredients();
                UpdateVisuals(true, false);
                Debug.Log("냄비: 떡볶이 조리 완료! 방치하면 타기 시작합니다.");
            }
        }

        // 2. 🚨 요리는 끝났으나 수거하지 않아 실시간으로 불판 위에서 타들어 가는 위험 상태
        if (isDone)
        {
            timer += Time.deltaTime;
            float burnProgress = timer / baseBurnTime;

            if (timer < baseBurnTime)
            {
                // ⭐ [연출 핵심] 점점 시간이 흐를수록 초록색 ➡️ 새빨간색으로 그라데이션 변화(Lerp)
                Color currentAlertColor = Color.Lerp(normalCookColor, alertMaxColor, burnProgress);
                
                // ⭐ [연출 핵심] 삼각함수(Sin)의 절대값을 이용해 알파(투명도) 값을 0.2 ~ 1.0 사이로 빠르게 깜빡이게 만듭니다.
                float blinkAlpha = Mathf.Lerp(0.2f, 1.0f, Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed)));
                currentAlertColor.a = blinkAlpha;

                // 실시간 색상 및 깜빡임 연출 주입
                if (circleProgressSlider != null)
                {
                    circleProgressSlider.color = currentAlertColor;
                    // 타들어 갈 때는 역으로 슬라이더가 닳거나 꽉 찬 상태를 유지하게 처리할 수 있습니다 (여기선 꽉 찬 상태 유지)
                    circleProgressSlider.fillAmount = 1f; 
                }

                if (statusText != null)
                {
                    statusText.text = "<color=red>!!</color>";
                    // 텍스트 투명도도 같이 깜빡거리게 연동
                    statusText.alpha = blinkAlpha;
                }
            }
            else
            {
                // 완전히 버닝 타임을 초과하여 새까맣게 타버린 찰나의 순간
                if (cookedVisualObject != null && cookedVisualObject.activeSelf)
                {
                    UpdateVisuals(false, true);
                    
                    // 다 타버렸으므로 UI는 경고를 멈추고 0% 투명 고정 또는 패널 Off 처리
                    if (circleProgressSlider != null) circleProgressSlider.fillAmount = 0f;
                    if (statusText != null) { statusText.text = "BURNED"; statusText.alpha = 1f; }
                    Debug.Log("<color=red>냄비: 결국 떡볶이가 까맣게 타버렸습니다!</color>");
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
            statusText.alpha = 1f; // 조리 중일 땐 투명도 원복
        }
    }

    private void UpdateVisuals(bool showCooked, bool showBurnt)
    {
        if (cookedVisualObject != null) cookedVisualObject.SetActive(showCooked);
        if (burntVisualObject != null) burntVisualObject.SetActive(showBurnt);
    }

    private void ClearSpawnedIngredients()
    {
        foreach (GameObject visual in spawnedIngredientVisuals)
        {
            if (visual != null) Destroy(visual);
        }
        spawnedIngredientVisuals.Clear();
    }

    private void ResetPot()
    {
        currentIngredients.Clear();
        ClearSpawnedIngredients();
        isReadyToCook = false;
        isCooking = false;
        isDone = false;
        timer = 0f;
        targetCookTime = 0f;

        // 💡 요리를 수거해갔으므로 UI 부모 패널을 투명하게 숨겨줍니다.
        if (potUIPanel != null) potUIPanel.SetActive(false);

        UpdateVisuals(false, false);
    }
}