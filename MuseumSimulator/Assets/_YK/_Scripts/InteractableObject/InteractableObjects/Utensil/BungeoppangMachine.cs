using UnityEngine;
using UnityEngine.UI; 
using TMPro;        

public class BungeoppangMachine : MonoBehaviour, IInteractable
{
    [Header("붕어빵 기계 설정")]
    [Tooltip("UpgradeManager에 등록한 고유 식별 이름")]
    public string machineID = "BungeoppangMachine";
    public float baseCookTime = 7f;      // 조리 시작 후 구워지는 기본 시간 (초)
    public float baseBurnTime = 8f;      // 완수 후 방치 시 타버리는 시간 (초)

    [Header("결과물 데이터")]
    public FoodData cookedBungeoppangBatch; 
    public FoodData burntBungeoppangBatch;  

    [Header("기계 상태 (Debug)")]
    [SerializeField] private bool hasBatter = false;     
    [SerializeField] private bool hasRedBean = false;    
    [SerializeField] private bool isReadyToCook = false; 
    [SerializeField] private bool isCooking = false;     
    [SerializeField] private bool isDone = false;        

    private float timer = 0f;
    private float targetCookTime = 0f;  

    [Header("⭐ 3D 단계별 연출 오브젝트")]
    public GameObject batterVisuals;
    public GameObject redBeanVisuals;
    public GameObject doneVisuals;
    public GameObject burntVisualObject;

    [Header("⭐ 뚜껑 회전 연출 설정")]
    [Tooltip("회전시킬 붕어빵 기계 뚜껑(기본 닫힌 상태가 X축 0도여야 합니다)")]
    public Transform machineLidTransform;
    [Tooltip("뚜껑이 열고 닫히는 회전 속도")]
    public float lidRotationSpeed = 4f;

    [Header("⭐ 연출 오브젝트 세팅 (FX)")]
    public GameObject cookingFX;

    [Header("⭐ 조리 및 경고 UI 설정")]
    public GameObject bungeoUIPanel;
    public Image circleProgressSlider;
    public TMP_Text statusText;

    [Header("⭐ UI 연출 색상 세팅")]
    public Color normalCookColor = Color.green; 
    public Color alertMaxColor = Color.red;     
    [Tooltip("방치 되었을 때 초당 깜빡거리는 속도")]
    public float blinkSpeed = 8f;

    // 내부 제어용 타겟 회전값
    private float targetXRotation = 0f;

    private void Start()
    {
        UpdateVisuals();
        
        if (bungeoUIPanel != null) bungeoUIPanel.SetActive(false);
        if (cookingFX != null) cookingFX.SetActive(false);

        // 💡 게임 시작 시에는 뚜껑이 열린 상태(0도)를 바라보게 합니다.
        targetXRotation = 0f;
        if (machineLidTransform != null)
        {
            machineLidTransform.localRotation = Quaternion.Euler(targetXRotation, 0f, 0f);
        }
    }

    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // [단계 4] 완성 혹은 탄 상태 -> 수거
        if (isDone)
        {
            if (player.IsHoldingItem)
            {
                Debug.LogWarning("붕어빵기계: 완성된 붕어빵 판을 들려면 손을 비워야 합니다!");
                return;
            }

            if (timer >= baseBurnTime)
            {
                if (burntBungeoppangBatch != null) player.HoldNewData(burntBungeoppangBatch);
                Debug.Log("붕어빵기계: 너무 방치되어 까맣게 타버린 붕어빵 10개 한 판을 통째로 꺼냈습니다!");
            }
            else
            {
                if (cookedBungeoppangBatch != null) player.HoldNewData(cookedBungeoppangBatch);
                Debug.Log("붕어빵기계: 노릇노릇하게 잘 익은 붕어빵 10개 한 판을 통째로 꺼냈습니다!");
            }

            ResetMachine();
            return;
        }

        // [단계 3] 조리 시작
        if (isReadyToCook && !isCooking)
        {
            if (player.IsHoldingItem)
            {
                Debug.LogWarning("붕어빵기계: 구우려면 손을 비우고 상호작용하세요!");
                return;
            }

            StartCooking();
            return;
        }

        // [재료 투입 단계]
        if (!isReadyToCook && !isCooking && !isDone)
        {
            if (!player.IsHoldingItem)
            {
                Debug.Log("붕어빵기계: 반죽이나 팥을 들고 오세요.");
                return;
            }

            FoodData heldData = player.CurrentHeldData;

            if (heldData.foodName == "flour" && !hasBatter)
            {
                hasBatter = true;
                player.ClearHand(); 
                UpdateVisuals();
                Debug.Log("붕어빵기계: 판 전체에 [밀가루 반죽]을 깔았습니다. 이제 [팥]을 가져오세요.");
                return;
            }

            if (heldData.foodName == "Red Bean" && hasBatter && !hasRedBean)
            {
                hasRedBean = true;
                isReadyToCook = true; 
                player.ClearHand(); 
                UpdateVisuals();
                Debug.Log("붕어빵기계: 모든 반죽 위에 [팥]을 채웠습니다! 빈손으로 E키를 눌러 뚜껑을 닫고 구우세요.");
                return;
            }

            Debug.LogWarning("붕어빵기계: 순서가 잘못되었거나 이 기계에 맞지 않는 재료입니다. (반죽 먼저 -> 그다음 팥)");
        }
    }

    private void StartCooking()
    {
        isReadyToCook = false;
        isCooking = true;
        timer = 0f;

        float speedModifier = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetSpeedModifier(machineID) : 1f;
        targetCookTime = baseCookTime * speedModifier;

        if (bungeoUIPanel != null) bungeoUIPanel.SetActive(true);
        if (circleProgressSlider != null) circleProgressSlider.color = normalCookColor;
        if (cookingFX != null) cookingFX.SetActive(true);

        // ⭐ [회전 연출] 조리 시작 시 뚜껑이 90도로 닫히도록 타겟 설정
        targetXRotation = 90f;

        UpdateVisuals();
        Debug.Log($"붕어빵기계: 뚜껑을 닫았습니다! 조리 시작 (최종 소요 시간: {targetCookTime:F1}초)");
    }

    void Update()
    {
        // ⭐ [회전 연출 핵심] 매 프레임 타겟 각도를 향해 뚜껑 오브젝트를 스르륵 회전시킵니다.
        if (machineLidTransform != null)
        {
            Quaternion targetRot = Quaternion.Euler(targetXRotation, 0f, 0f);
            machineLidTransform.localRotation = Quaternion.Lerp(machineLidTransform.localRotation, targetRot, Time.deltaTime * lidRotationSpeed);
        }

        // 1. 🔥 노릇노릇 붕어빵이 맛있게 구워지는 연산
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

                if (cookingFX != null) cookingFX.SetActive(false);

                // ⭐ [회전 연출] 조리가 완료되어 띵 소리가 날 때 뚜껑이 다시 0도로 스르륵 열리게 타겟 세팅
                targetXRotation = 0f;

                UpdateVisuals();
                Debug.Log("붕어빵기계: 띵! 완성되었습니다! 빈손으로 통째로 꺼내가세요.");
            }
        }
        // 2. 🚨 완성이 다 끝났는데 수거하지 않아서 철판 위에서 타들어가는 연산 (ALERT)
        else if (isDone)
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
                if (doneVisuals != null && doneVisuals.activeSelf)
                {
                    UpdateVisuals(); 
                    
                    if (circleProgressSlider != null) circleProgressSlider.fillAmount = 0f;
                    if (statusText != null) { statusText.text = "BURNED"; statusText.alpha = 1f; }
                    Debug.Log("<color=red>붕어빵기계: 뚜껑 사이로 연기가 나며 붕어빵 한 판이 숯둥이가 되었습니다!</color>");
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

    private void UpdateVisuals()
    {
        if (timer < baseBurnTime || isCooking)
        {
            if (batterVisuals != null) batterVisuals.SetActive(hasBatter && !hasRedBean && !isDone);
            if (redBeanVisuals != null) redBeanVisuals.SetActive(hasRedBean && !isDone);
            if (doneVisuals != null) doneVisuals.SetActive(isDone);
            if (burntVisualObject != null) burntVisualObject.SetActive(false);
        }
        else
        {
            if (batterVisuals != null) batterVisuals.SetActive(false);
            if (redBeanVisuals != null) redBeanVisuals.SetActive(false);
            if (doneVisuals != null) doneVisuals.SetActive(false);
            if (burntVisualObject != null) burntVisualObject.SetActive(true);
        }
    }

    private void ResetMachine()
    {
        hasBatter = false;
        hasRedBean = false;
        isReadyToCook = false;
        isCooking = false;
        isDone = false;
        timer = 0f;

        // 💡 초기화(수거) 시에도 안전하게 뚜껑 타겟을 기본 열린 상태(0도)로 복귀시킵니다.
        targetXRotation = 0f;

        if (bungeoUIPanel != null) bungeoUIPanel.SetActive(false);
        if (cookingFX != null) cookingFX.SetActive(false);

        UpdateVisuals();
        Debug.Log("붕어빵기계: 판이 초기화되었습니다. 다음 판 조리 가능!");
    }
}