using UnityEngine;

public class ChoppingBoard : MonoBehaviour, IInteractable
{
    [Header("도마 고유 설정")]
    [Tooltip("UpgradeManager에 등록한 고유 식별 이름 (ex: ChoppingBoard)")]
    public string machineID = "ChoppingBoard"; 
    public float baseChopTime = 3f; // 기본 손질에 필요한 시간 (초)

    [Header("레시피별 결과물 데이터")]
    public HoldableObject choppedVegetableData; // "야채" -> "손질된야채"
    public HoldableObject choppedChickenData;   // "생닭" -> "손질된닭"
    public HoldableObject choppedPotatoData;    // "감자" -> "손질된감자"

    [Header("도마 위 시각적 배치 위치")]
    public Transform ingredientPlaceTransform; // 도마 위에 3D 물체를 올려둘 기준점

    // 내부 제어용 변수들
    private bool hasIngredient = false;      // 현재 도마에 재료가 올라가 있는지 여부
    private float chopTimer = 0f;            // 현재까지 누른 시간
    private float targetChopTime = 0f;       // 목표 손질 시간 (업그레이드 반영)
    
    private HoldableObject currentPlacingData; // 현재 도마에 올라간 재료 데이터
    private HoldableObject resultData;         // 손질 완료 후 지급할 결과물 데이터
    private GameObject spawnedVisual = null;   // 도마 위에 잠시 띄울 3D 모델링 원본

    private PlayerInteractor targetPlayer = null;

    // 💡 인터페이스 구현 (플레이어가 바라보고 E키를 톡 눌렀을 때 실행)
    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // [단계 1] 도마가 비어있고, 플레이어가 손질 가능한 재료를 들고 있을 때 -> 도마에 올리기
        if (!hasIngredient && player.IsHoldingItem)
        {
            HoldableObject heldData = player.CurrentHeldData.Value;
            bool canChop = false;

            if (heldData.objectName == "야채")
            {
                resultData = choppedVegetableData;
                canChop = true;
            }
            else if (heldData.objectName == "생닭")
            {
                resultData = choppedChickenData;
                canChop = true;
            }
            else if (heldData.objectName == "감자")
            {
                resultData = choppedPotatoData;
                canChop = true;
            }

            if (canChop)
            {
                currentPlacingData = heldData;
                hasIngredient = true;
                chopTimer = 0f;

                // 💡 [독립적 업그레이드 연동] 
                // 상속 없이 UpgradeManager를 직접 찔러서 내 machineID에 맞는 단축 비율을 가져옵니다.
                float speedModifier = 1f;
                if (UpgradeManager.Instance != null)
                {
                    speedModifier = UpgradeManager.Instance.GetSpeedModifier(machineID);
                }
                targetChopTime = baseChopTime * speedModifier;

                // 플레이어 손에서 데이터를 지우고, 도마 위에 시각적으로 3D 모델 생성
                player.ClearHand();
                if (currentPlacingData.prefab != null && ingredientPlaceTransform != null)
                {
                    spawnedVisual = Instantiate(currentPlacingData.prefab, ingredientPlaceTransform);
                    spawnedVisual.transform.localPosition = Vector3.zero;
                    spawnedVisual.transform.localRotation = Quaternion.identity;
                }

                Debug.Log($"도마: '{currentPlacingData.objectName}'을 올렸습니다. 손을 비우고 E키를 꾹 눌러 손질하세요! (목표 시간: {targetChopTime:F1}초)");
            }
        }
    }

    void Update()
    {
        // 도마에 재료가 올라가 있을 때만 꾹 누르기 로직 가동
        if (!hasIngredient) return;

        if (targetPlayer == null)
        {
            targetPlayer = FindFirstObjectByType<PlayerInteractor>();
            if (targetPlayer == null) return;
        }

        // [단계 2] 플레이어가 E 키를 꾹 누르고 있을 때
        if (Input.GetKey(KeyCode.E))
        {
            // 💡 [질문자님 조건 반영] 꾹 누르고 있더라도 손에 무언가 들려있다면 게이지가 가지 않음!
            if (targetPlayer.IsHoldingItem)
            {
                return; 
            }

            // 빈손일 때만 정직하게 게이지 증가
            chopTimer += Time.deltaTime;
            Debug.Log($"도마 손질 중... 진행도: {((chopTimer / targetChopTime) * 100f):F0}%");

            // 손질 완료 조건 충족
            if (chopTimer >= targetChopTime)
            {
                CompleteChopping();
            }
        }
    }

    void CompleteChopping()
    {
        hasIngredient = false;
        chopTimer = 0f;

        // 도마 위 원재료 모델 삭제
        if (spawnedVisual != null)
        {
            Destroy(spawnedVisual);
            spawnedVisual = null;
        }

        // 손질이 끝났으므로 결과물을 플레이어 손에 즉시 지급
        if (targetPlayer != null)
        {
            targetPlayer.HoldNewData(resultData);
            Debug.Log($"도마: 손질 완료! 플레이어에게 '{resultData.objectName}'을 지급했습니다.");
        }
    }
}