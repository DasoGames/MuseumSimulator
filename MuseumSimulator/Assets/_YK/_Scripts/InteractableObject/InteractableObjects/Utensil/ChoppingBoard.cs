using UnityEngine;

public class ChoppingBoard : MonoBehaviour, IInteractable
{
    [Header("도마 고유 설정")]
    [Tooltip("UpgradeManager에 등록한 고유 식별 이름 (ex: ChoppingBoard)")]
    public string machineID = "ChoppingBoard"; 
    public float baseChopTime = 3f; // 기본 손질에 필요한 시간 (초)

    [Header("레시피별 결과물 데이터")]
    // 💡 기존 HoldableObject 구조체 대신 통합 스크립터블 오브젝트인 FoodData를 참조합니다.
    public FoodData choppedVegetableData; // "야채" -> "손질된야채"
    public FoodData choppedChickenData;   // "생닭" -> "손질된닭"
    public FoodData choppedPotatoData;    // "감자" -> "손질된감자"

    [Header("도마 위 시각적 배치 위치")]
    public Transform ingredientPlaceTransform; // 도마 위에 3D 물체를 올려둘 기준점

    // 내부 제어용 변수들
    private bool hasIngredient = false;      // 현재 도마에 재료가 올라가 있는지 여부
    private float chopTimer = 0f;            // 현재까지 누른 시간
    private float targetChopTime = 0f;       // 목표 손질 시간 (업그레이드 반영)
    
    // 💡 모든 데이터 타입을 FoodData 참조형으로 변경합니다.
    private FoodData currentPlacingData;     // 현재 도마에 올라간 재료 데이터
    private FoodData resultData;             // 손질 완료 후 지급할 결과물 데이터
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
            // 💡 구조체 형식을 완전히 지우고 순수 FoodData 참조로 가져옵니다.
            FoodData heldData = player.CurrentHeldData;
            bool canChop = false;

            // 💡 데이터 규칙에 맞게 'foodName' 필드로 비교를 수행합니다.
            if (heldData.foodName == "Vegetables")
            {
                resultData = choppedVegetableData;
                canChop = true;
            }
            else if (heldData.foodName == "RawChicken")
            {
                resultData = choppedChickenData;
                canChop = true;
            }
            else if (heldData.foodName == "Potato")
            {
                resultData = choppedPotatoData;
                canChop = true;
            }

            if (canChop)
            {
                currentPlacingData = heldData;
                hasIngredient = true;
                chopTimer = 0f;

                // [독립적 업그레이드 연동] 
                float speedModifier = 1f;
                if (UpgradeManager.Instance != null)
                {
                    speedModifier = UpgradeManager.Instance.GetSpeedModifier(machineID);
                }
                targetChopTime = baseChopTime * speedModifier;

                // 💡 손을 비우기 전에 heldData(currentPlacingData)의 프리팹 정보를 받아 도마 위에 3D 비주얼을 생성합니다.
                if (currentPlacingData.prefab != null && ingredientPlaceTransform != null)
                {
                    spawnedVisual = Instantiate(currentPlacingData.prefab, ingredientPlaceTransform);
                    spawnedVisual.transform.localPosition = Vector3.zero;
                    spawnedVisual.transform.localRotation = Quaternion.identity;
                }

                // 플레이어 손 비우기
                player.ClearHand();

                Debug.Log($"도마: '{currentPlacingData.foodName}'을 올렸습니다. 손을 비우고 E키를 꾹 눌러 손질하세요! (목표 시간: {targetChopTime:F1}초)");
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
            // 꾹 누르고 있더라도 손에 무언가 들려있다면 게이지가 가지 않음!
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
        if (targetPlayer != null && resultData != null)
        {
            // 💡 리팩토링된 PlayerInteractor 규칙에 맞게 FoodData 에셋을 손에 쥐여줍니다.
            targetPlayer.HoldNewData(resultData);
            Debug.Log($"도마: 손질 완료! 플레이어에게 '{resultData.foodName}'을 지급했습니다.");
        }
    }
}