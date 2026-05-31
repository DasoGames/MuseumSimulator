using UnityEngine;
using UnityEngine.UI; 
using TMPro;        

public class ChoppingBoard : MonoBehaviour, IInteractable
{
    [Header("도마 고유 설정")]
    public string machineID = "ChoppingBoard"; 
    public float baseChopTime = 3f; 

    [Header("레시피별 결과물 데이터")]
    public FoodData choppedVegetableData; 
    public FoodData choppedChickenData;   
    public FoodData choppedPotatoData;    

    [Header("도마 위 시각적 배치 위치")]
    public Transform ingredientPlaceTransform; 

    [Header("⭐ 손질 원형 UI 및 이펙트 설정")]
    public GameObject chopUIPanel;
    public Image circleProgressSlider;
    public TMP_Text percentText;
    
    [Tooltip("손질 중(E키를 누르고 있을 때)에만 켜질 칼바람/가루 파티클 FX 오브젝트")]
    public GameObject FX;

    // 내부 제어용 변수들
    private bool hasIngredient = false;      
    private float chopTimer = 0f;            
    private float targetChopTime = 0f;       
    
    private FoodData currentPlacingData;     
    private FoodData resultData;             
    private GameObject spawnedVisual = null;   

    private PlayerInteractor targetPlayer = null;

    void Start()
    {
        if (chopUIPanel != null) chopUIPanel.SetActive(false);
        // 💡 게임 시작 시 이펙트도 확실하게 꺼둡니다.
        if (FX != null) FX.SetActive(false);
    }

    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        if (!hasIngredient && player.IsHoldingItem)
        {
            FoodData heldData = player.CurrentHeldData;
            bool canChop = false;

            if (heldData.foodName == "Vegetables") { resultData = choppedVegetableData; canChop = true; }
            else if (heldData.foodName == "RawChicken") { resultData = choppedChickenData; canChop = true; }
            else if (heldData.foodName == "Potato") { resultData = choppedPotatoData; canChop = true; }
            else return;

            if (canChop)
            {
                currentPlacingData = heldData;
                hasIngredient = true;
                chopTimer = 0f;

                float speedModifier = 1f;
                if (UpgradeManager.Instance != null) speedModifier = UpgradeManager.Instance.GetSpeedModifier(machineID);
                targetChopTime = baseChopTime * speedModifier;

                UpdateUI(0f);
                if (chopUIPanel != null) chopUIPanel.SetActive(true);

                if (currentPlacingData.prefab != null && ingredientPlaceTransform != null)
                {
                    Quaternion finalRotation = ingredientPlaceTransform.rotation * Quaternion.Euler(currentPlacingData.spawnRotation);
                    spawnedVisual = Instantiate(currentPlacingData.prefab, ingredientPlaceTransform.position, finalRotation, ingredientPlaceTransform);
                    spawnedVisual.transform.localPosition = Vector3.zero;
                    spawnedVisual.transform.localScale = currentPlacingData.spawnScale;
                }

                player.ClearHand();
            }
        }
    }

    void Update()
    {
        if (!hasIngredient) return;

        if (targetPlayer == null)
        {
            targetPlayer = FindFirstObjectByType<PlayerInteractor>();
            if (targetPlayer == null) return;
        }

        // 💡 [수정] 플레이어가 빈손으로 E 키를 꾹 누르고 있는 상태 (진짜 칼질 중)
        if (Input.GetKey(KeyCode.E) && !targetPlayer.IsHoldingItem)
        {
            chopTimer += Time.deltaTime;
            UpdateUI(chopTimer / targetChopTime);

            // ⭐ [핵심 추가] 칼질 중일 때만 FX 오브젝트 활성화!
            if (FX != null && !FX.activeSelf) 
            {
                FX.SetActive(true);
            }

            if (chopTimer >= targetChopTime)
            {
                CompleteChopping();
            }
        }
        else
        {
            // ⭐ [핵심 추가] E키를 떼거나, 중간에 손에 아이템을 들면 즉시 이펙트를 꺼서 끊어줍니다.
            if (FX != null && FX.activeSelf) 
            {
                FX.SetActive(false);
            }
        }
    }

    private void UpdateUI(float progressNormalized)
    {
        progressNormalized = Mathf.Clamp01(progressNormalized);

        if (circleProgressSlider != null) circleProgressSlider.fillAmount = progressNormalized;
        if (percentText != null) percentText.text = $"{Mathf.RoundToInt(progressNormalized * 100f)}%";
    }

    void CompleteChopping()
    {
        hasIngredient = false;
        chopTimer = 0f;

        if (chopUIPanel != null) chopUIPanel.SetActive(false);
        
        // ⭐ [핵심 추가] 손질이 완벽히 끝났으므로 연출용 FX도 확실하게 오프!
        if (FX != null) FX.SetActive(false);

        if (spawnedVisual != null)
        {
            Destroy(spawnedVisual);
            spawnedVisual = null;
        }

        if (targetPlayer != null && resultData != null)
        {
            targetPlayer.HoldNewData(resultData);
            Debug.Log("도마: 손질 완료! UI와 칼질 FX를 모두 정리했습니다.");
        }
    }
}