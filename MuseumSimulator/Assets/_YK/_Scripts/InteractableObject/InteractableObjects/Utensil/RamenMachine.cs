using UnityEngine;

public class RamenMachine : MonoBehaviour, IInteractable
{
    [Header("라면 기계 설정")]
    public string machineID = "RamenMachine";
    public float baseCookTime = 6f;     // 조리 시작 후 라면이 끓는 기본 시간 (초)
    public float baseBurnTime = 7f;     // 완수 후 방치 시 불거나 타버리는(폐기) 시간 (초)

    [Header("결과물 데이터")]
    public HoldableObject cookedRamenData; // 이름: "완성된라면"
    public HoldableObject burntRamenData;  // 이름: "탄라면" (또는 불어터진라면)

    [Header("기계 상태 (Debug)")]
    [SerializeField] private bool hasRamenIngredient = false; // 기계에 라면 재료가 투입되었는가?
    [SerializeField] private bool isCooking = false;           // 현재 보글보글 끓는 중인가?
    [SerializeField] private bool isDone = false;              // 조리가 완료되었는가?

    private float timer = 0f;
    private float targetCookTime = 0f;  // 업그레이드가 반영된 최종 조리 시간

    [Header("시각적 연출용 오브젝트 (선택사항)")]
    public GameObject ingredientVisual; // 생라면 상태의 비주얼
    public GameObject cookedVisual;     // 다 끓여진 라면 상태의 비주얼

    private void Start()
    {
        UpdateVisuals();
    }

    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // 💡 [단계 3] 조리가 완전히 끝난 상태 -> 빈손으로 오면 완성작 수거
        if (isDone)
        {
            if (player.IsHoldingItem)
            {
                Debug.LogWarning("라면기계: 완성된 라면을 조심히 들려면 손을 비워야 합니다!");
                return;
            }

            // 방치 타이머 체크 후 알맞은 데이터 지급
            if (timer >= baseBurnTime)
            {
                player.HoldNewData(burntRamenData);
                Debug.Log("라면기계: 너무 오랫동안 방치해서 불어터지고 탄 라면을 수거했습니다.");
            }
            else
            {
                player.HoldNewData(cookedRamenData);
                Debug.Log("라면기계: 보글보글 맛있게 끓여진 완벽한 라면을 수거했습니다!");
            }

            // 수거 즉시 기계 완전 리셋
            ResetMachine();
            return;
        }

        // 💡 [단계 2] 재료가 들어가 있고 아직 조리 전일 때 -> 빈손으로 툭 누르면 조리 시작!
        if (hasRamenIngredient && !isCooking && !isDone)
        {
            if (player.IsHoldingItem)
            {
                Debug.LogWarning("라면기계: 기계를 가동시키려면 손을 비우고 상호작용하세요!");
                return;
            }

            StartCooking();
            return;
        }

        // 💡 [단계 1] 기계가 완전히 비어있을 때 -> 생라면 재료 투입하기
        if (!hasRamenIngredient && !isCooking && !isDone)
        {
            if (!player.IsHoldingItem)
            {
                Debug.Log("라면기계: 끓일 라면 재료를 들고 오세요.");
                return;
            }

            HoldableObject heldData = player.CurrentHeldData.Value;

            // 들고 있는 재료의 이름이 정확히 "라면"일 때만 작동
            if (heldData.objectName == "라면")
            {
                hasRamenIngredient = true;
                player.ClearHand(); // 플레이어 손에 든 재료 소모
                
                UpdateVisuals();
                Debug.Log("라면기계: 용기에 라면 재료를 세팅했습니다! 빈손으로 한 번 더 누르면 조리가 시작됩니다.");
            }
            else
            {
                Debug.LogWarning("라면기계: 이 기계에는 '라면' 재료만 넣을 수 있습니다.");
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

        UpdateVisuals();
        Debug.Log($"라면기계: 온수가 가동되며 라면이 끓기 시작합니다! (소요 시간: {targetCookTime:F1}초)");
    }

    void Update()
    {
        // 자동으로 보글보글 끓는 타이머 작동
        if (isCooking)
        {
            timer += Time.deltaTime;
            if (timer >= targetCookTime)
            {
                isCooking = false;
                isDone = true;
                timer = 0f; // 이 타이머는 이제 완성 후 방치 시간(탄 타이머)으로 전환됩니다.
                UpdateVisuals();
                Debug.Log("라면기계: 띵! 라면 조리가 완료되었습니다! 불어터지기 전에 빈손으로 어서 꺼내가세요.");
            }
        }
        // 완성 후 방치 타이머 작동 (너무 오래 두면 탄 라면이 됨)
        else if (isDone)
        {
            timer += Time.deltaTime;
        }
    }

    // 시각적 연출 제어
    private void UpdateVisuals()
    {
        // 재료 상태 비주얼 조절 (재료가 있고 완성 전일 때만 켜짐)
        if (ingredientVisual != null) 
            ingredientVisual.SetActive(hasRamenIngredient && !isDone);
        
        // 완성 상태 비주얼 조절 (조리가 끝나면 켜짐)
        if (cookedVisual != null) 
            cookedVisual.SetActive(isDone);
    }

    // 기계 완전 초기화
    private void ResetMachine()
    {
        hasRamenIngredient = false;
        isCooking = false;
        isDone = false;
        timer = 0f;
        UpdateVisuals();
        Debug.Log("라면기계: 기계 청소 및 초기화 완료! 다음 라면 조리가 가능합니다.");
    }
}