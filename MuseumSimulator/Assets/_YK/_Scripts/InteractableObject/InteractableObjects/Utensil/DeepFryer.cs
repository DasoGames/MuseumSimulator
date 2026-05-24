using UnityEngine;
using System.Collections.Generic;

public class DeepFryer : MonoBehaviour, IInteractable
{
    [Header("튀김기 기계 설정")]
    public string machineID = "DeepFryer";
    public int maxSlots = 3;            // 튀김 바스켓(슬롯) 개수 (ex: 3구 짜리 튀김기)
    public float baseCookTime = 6f;     // 튀겨지는 기본 시간 (초)
    public float baseBurnTime = 7f;     // 완수 후 방치 시 타버리는(폐기) 시간 (초)

    [Header("치킨 결과물 데이터")]
    public HoldableObject cookedChickenData; // 이름: "완성된치킨" (또는 후라이드치킨)
    public HoldableObject burntChickenData;  // 이름: "탄치킨"

    [Header("감자튀김 결과물 데이터")]
    public HoldableObject cookedPotatoData;  // 이름: "완성된감자튀김" (또는 감자튀김)
    public HoldableObject burntPotatoData;   // 이름: "탄감자튀김"

    // 💡 튀김기 바스켓 하나의 상태를 관리하기 위한 내부 구조체
    [System.Serializable]
    public struct FryerSlot
    {
        public bool isOccupied;         // 이 바스켓에 무언가 들어있는가?
        public string inputIngredientName; // 처음에 투입된 재료의 이름 ("손질된닭" 또는 "손질된감자")
        
        public bool isCooking;          // 현재 기름에 튀겨지는 중인가?
        public bool isDone;             // 완료되었는가?
        
        public float timer;             // 바스켓 개별 타이머
        public float targetCookTime;    // 업그레이드가 반영된 최종 조리 시간
        public GameObject visualRef;    // 바스켓 안에 잠시 소환될 3D 프리팹 인스턴스 (선택사항)
    }

    [Header("현재 바스켓 상태들 (Debug)")]
    [SerializeField] private List<FryerSlot> slots = new List<FryerSlot>();

    [Header("바스켓별 3D 배치 위치")]
    public List<Transform> slotTransforms = new List<Transform>(); // 바스켓들의 위치 (maxSlots 개수만큼 필요)

    private void Awake()
    {
        // 슬롯 리스트 초기화
        slots.Clear();
        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new FryerSlot { isOccupied = false });
        }
    }

    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // 💡 [상황 1] 플레이어가 빈손일 때 -> 수거 우선 체크 (다 튀겨진 음식을 하나씩 꺼냄)
        if (!player.IsHoldingItem)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].isOccupied && slots[i].isDone)
                {
                    TakeFoodFromBasket(i, player);
                    return;
                }
            }
            Debug.Log("튀김기: 현재 건져올릴 수 있는 완성된 튀김 요리가 없습니다.");
            return;
        }

        // 💡 [상황 2] 플레이어가 손에 재료를 들고 있을 때 -> 튀김기에 투입
        if (player.IsHoldingItem)
        {
            HoldableObject heldData = player.CurrentHeldData.Value;

            // 투입 가능한 손질된 재료들인지 검사
            if (heldData.objectName == "손질된닭" || heldData.objectName == "손질된감자")
            {
                // 비어있는 바스켓이 있는지 확인
                int emptyBasketIndex = slots.FindIndex(x => !x.isOccupied);

                if (emptyBasketIndex != -1)
                {
                    PutFoodInBasket(emptyBasketIndex, heldData, player);
                }
                else
                {
                    Debug.LogWarning("튀김기: 모든 튀김 바스켓이 사용 중입니다! 다 익은 요리를 먼저 건지세요.");
                }
            }
            else
            {
                Debug.LogWarning("튀김기: 이 기계에는 도마에서 손질된닭 또는 손질된감자만 넣을 수 있습니다.");
            }
        }
    }

    // 바스켓에 재료를 넣고 즉시 튀기기 시작하는 함수
    private void PutFoodInBasket(int index, HoldableObject heldData, PlayerInteractor player)
    {
        FryerSlot slot = slots[index];
        slot.isOccupied = true;
        slot.isCooking = true;
        slot.isDone = false;
        slot.timer = 0f;
        slot.inputIngredientName = heldData.objectName; // 닭인지 감자인지 기계가 기억하게 함

        // [업그레이드 연동] UpgradeManager를 찔러 속도 단축 비율 계산
        float speedModifier = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetSpeedModifier(machineID) : 1f;
        slot.targetCookTime = baseCookTime * speedModifier;

        // 플레이어 손 비우기
        player.ClearHand();

        // (선택사항) 기름에 들어간 3D 비주얼 프리팹 소환
        if (heldData.prefab != null && slotTransforms.Count > index && slotTransforms[index] != null)
        {
            slot.visualRef = Instantiate(heldData.prefab, slotTransforms[index]);
            slot.visualRef.transform.localPosition = Vector3.zero;
            slot.visualRef.transform.localRotation = Quaternion.identity;
        }

        slots[index] = slot;
        Debug.Log($"튀김기: {index + 1}번 바스켓에 [{slot.inputIngredientName}]을 넣었습니다. 튀기기 시작! (시간: {slot.targetCookTime:F1}초)");
    }

    // 튀김이 완료된 요리를 건져내어 플레이어에게 지급하는 함수
    private void TakeFoodFromBasket(int index, PlayerInteractor player)
    {
        FryerSlot slot = slots[index];
        HoldableObject resultData;

        // 💡 투입되었던 재료 이름("손질된닭" vs "손질된감자")에 따라 결과물 분기 처리
        if (slot.inputIngredientName == "손질된닭")
        {
            // 탄 타이머 시간 체크
            resultData = (slot.timer >= baseBurnTime) ? burntChickenData : cookedChickenData;
        }
        else // "손질된감자"인 경우
        {
            resultData = (slot.timer >= baseBurnTime) ? burntPotatoData : cookedPotatoData;
        }

        // 플레이어 손에 결과물 데이터 지급
        player.HoldNewData(resultData);
        Debug.Log($"튀김기: {index + 1}번 바스켓에서 요리를 건졌습니다! 결과물: [{resultData.objectName}]");

        // 비주얼 오브젝트 파괴 및 해당 바스켓 완전 초기화
        if (slot.visualRef != null) Destroy(slot.visualRef);
        
        slot.isOccupied = false;
        slot.inputIngredientName = "";
        slot.isCooking = false;
        slot.isDone = false;
        slot.timer = 0f;
        slot.visualRef = null;

        slots[index] = slot;
    }

    void Update()
    {
        // 모든 튀김 바스켓을 실시간으로 돌며 각각 독립적인 타이머 작동
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].isOccupied) continue;

            FryerSlot slot = slots[i];

            // A. 기름에 지글지글 튀겨지는 중인 상태
            if (slot.isCooking)
            {
                slot.timer += Time.deltaTime;
                if (slot.timer >= slot.targetCookTime)
                {
                    slot.isCooking = false;
                    slot.isDone = true;
                    slot.timer = 0f; // 이 타이머는 이제 완성 후 방치 시간(탄 타이머)으로 사용됩니다.
                    Debug.Log($"튀김기: {i + 1}번 바스켓의 튀김 요리가 완료되었습니다! 타이머가 지나면 타버립니다.");
                }
            }
            // B. 요리가 완성된 상태로 튀김기 안에 방치된 상태
            else if (slot.isDone)
            {
                slot.timer += Time.deltaTime;
            }

            slots[i] = slot;
        }
    }
}