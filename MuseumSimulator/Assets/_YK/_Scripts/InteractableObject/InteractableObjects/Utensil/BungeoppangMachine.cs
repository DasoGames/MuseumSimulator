using UnityEngine;

public class BungeoppangMachine : MonoBehaviour, IInteractable
{
    [Header("붕어빵 기계 설정")]
    public string machineID = "BungeoppangMachine";
    public float baseCookTime = 7f;     // 조리 시작 후 구워지는 기본 시간 (초)
    public float baseBurnTime = 8f;      // 완수 후 방치 시 타버리는 시간 (초)

    [Header("결과물 데이터")]
    // 💡 기존 HoldableObject 구조체 대신 스크립터블 오브젝트인 FoodData를 연결합니다!
    // 이 데이터들은 낱개가 아니라 '10개짜리 한 판(묶음)' 데이터 에셋입니다.
    public FoodData cookedBungeoppangBatch; // 인스펙터 에셋 이름: "완성된붕어빵판"
    public FoodData burntBungeoppangBatch;  // 인스펙터 에셋 이름: "탄붕어빵판"

    [Header("기계 상태 (Debug)")]
    [SerializeField] private bool hasBatter = false;     // 판 전체에 반죽이 채워졌는가?
    [SerializeField] private bool hasRedBean = false;    // 판 전체에 팥이 채워졌는가?
    [SerializeField] private bool isReadyToCook = false; // 조리 시작 대기 중인가?
    [SerializeField] private bool isCooking = false;     // 현재 구워지는 중인가?
    [SerializeField] private bool isDone = false;        // 완성되었는가?

    private float timer = 0f;
    private float targetCookTime = 0f;  

    [Header("시각적 연출용 오브젝트 (선택사항)")]
    public GameObject batterVisuals;
    public GameObject redBeanVisuals;
    public GameObject doneVisuals;

    private void Start()
    {
        UpdateVisuals();
    }

    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // 💡 [단계 4] 완성 혹은 탄 상태 -> 빈손일 때 10개 한 판을 '한 번에' 통째로 수거
        if (isDone)
        {
            if (player.IsHoldingItem)
            {
                Debug.LogWarning("붕어빵기계: 완성된 붕어빵 판을 들려면 손을 비워야 합니다!");
                return;
            }

            // 탄 타이머 체크 후 알맞은 FoodData 에셋을 통째로 지급
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

            // 한 판을 꺼내는 순간 기계는 즉시 완전히 리셋됩니다.
            ResetMachine();
            return;
        }

        // 💡 [단계 3] 조리가능상태 -> 빈손으로 누르면 10개 동시 조리 시작
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

        // 💡 [재료 투입 단계] 반죽과 팥을 차례대로 판 전체에 채우기
        if (!isReadyToCook && !isCooking && !isDone)
        {
            if (!player.IsHoldingItem)
            {
                Debug.Log("붕어빵기계: 반죽이나 팥을 들고 오세요.");
                return;
            }

            // 💡 구조체 형식을 지우고 순수 FoodData 참조로 변경합니다.
            FoodData heldData = player.CurrentHeldData;

            // 💡 데이터 규칙에 맞춰 'foodName' 필드로 식별합니다.
            // [순서 1] 반죽 투입 -> 모든 틀에 동시에 깔림 (팥보다 먼저여야 함)
            if (heldData.foodName == "밀가루 반죽" && !hasBatter)
            {
                hasBatter = true;
                player.ClearHand(); // 플레이어 손 비우기
                UpdateVisuals();
                Debug.Log("붕어빵기계: 판 전체에 [밀가루 반죽]을 깔았습니다. 이제 [팥]을 가져오세요.");
                return;
            }

            // [순서 2] 팥 투입 -> 반죽이 있을 때만 가능하며 모든 틀에 동시에 채워짐
            if (heldData.foodName == "팥" && hasBatter && !hasRedBean)
            {
                hasRedBean = true;
                isReadyToCook = true; // 10개 틀 모두 충족되어 조리가능상태 돌입
                player.ClearHand(); // 플레이어 손 비우기
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

        // UpgradeManager 연동 (속도 단축 반영)
        float speedModifier = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetSpeedModifier(machineID) : 1f;
        targetCookTime = baseCookTime * speedModifier;

        UpdateVisuals();
        Debug.Log($"붕어빵기계: 뚜껑을 닫았습니다! 조리 시작 (최종 소요 시간: {targetCookTime:F1}초)");
    }

    void Update()
    {
        if (isCooking)
        {
            timer += Time.deltaTime;
            if (timer >= targetCookTime)
            {
                isCooking = false;
                isDone = true;
                timer = 0f; // 이제부터 탄 타이머로 작동
                UpdateVisuals();
                Debug.Log("붕어빵기계: 띵! 붕어빵 10개가 한 판으로 완성되었습니다! 빈손으로 통째로 꺼내가세요.");
            }
        }
        else if (isDone)
        {
            timer += Time.deltaTime;
        }
    }

    private void UpdateVisuals()
    {
        if (batterVisuals != null) batterVisuals.SetActive(hasBatter && !isDone);
        if (redBeanVisuals != null) redBeanVisuals.SetActive(hasRedBean && !isDone);
        if (doneVisuals != null) doneVisuals.SetActive(isDone);
    }

    private void ResetMachine()
    {
        hasBatter = false;
        hasRedBean = false;
        isReadyToCook = false;
        isCooking = false;
        isDone = false;
        timer = 0f;
        UpdateVisuals();
        Debug.Log("붕어빵기계: 판이 초기화되었습니다. 다음 판 조리 가능!");
    }
}