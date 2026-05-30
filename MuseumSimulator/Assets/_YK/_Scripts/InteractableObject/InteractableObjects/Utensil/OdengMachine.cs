using UnityEngine;
using System.Collections.Generic;

public class OdengMachine : MonoBehaviour, IInteractable
{
    [Header("오뎅 기계 설정")]
    public string machineID = "OdengMachine";
    public int maxSlots = 4;            // 이 기계에 동시에 꽂을 수 있는 최대 오뎅 수 (ex: 4구 짜리 기계)
    public float baseCookTime = 8f;     // 오뎅 하나 조리에 걸리는 기본 시간
    public float baseBurnTime = 10f;    // 완수 후 방치 시 타버리는(폐기) 시간

    [Header("결과물 데이터")]
    // 💡 기존 HoldableObject 구조체 대신 통합 스크립터블 오브젝트인 FoodData를 참조합니다.
    public FoodData cookedOdengData; // 에셋 이름: "완성된오뎅꼬치"
    public FoodData burntOdengData;  // 에셋 이름: "탄오뎅꼬치"

    // 💡 개별 오뎅 슬롯의 상태를 관리하기 위한 내부 구조체
    [System.Serializable]
    public struct OdengSlot
    {
        public bool isOccupied;      // 이 구멍에 오뎅이 꽂혀있는가?
        public bool isCooking;       // 조리 중인가?
        public bool isDone;          // 완성되었는가?
        public float timer;          // 슬롯 개별 타이머
        public float targetCookTime; // 업그레이드가 반영된 이 슬롯의 최종 조리 시간
        public GameObject visualRef; // 슬롯에 꽂힌 오뎅의 3D 프리팹 인스턴스
    }

    [Header("현재 슬롯 상태들 (Debug)")]
    [SerializeField] private List<OdengSlot> slots = new List<OdengSlot>();

    [Header("슬롯별 3D 배치 위치")]
    public List<Transform> slotTransforms = new List<Transform>(); // 오뎅이 꽂힐 위치들 (maxSlots 개수만큼 필요)

    private void Awake()
    {
        // 설정된 최대 슬롯 수만큼 슬롯 리스트 초기화
        slots.Clear();
        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new OdengSlot { isOccupied = false });
        }
    }

    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // 💡 [상황 1] 플레이어가 빈손일 때 -> 수거 우선 체크 (완성되거나 타버린 오뎅이 있다면 하나씩 꺼냄)
        if (!player.IsHoldingItem)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].isOccupied && slots[i].isDone)
                {
                    // 꺼내기 프로세스 실행
                    TakeOdengFromSlot(i, player);
                    return;
                }
            }
            Debug.Log("오뎅기계: 현재 수거 가능한 완성된 오뎅이 없습니다.");
            return;
        }

        // 💡 [상황 2] 플레이어가 손에 재료를 들고 있을 때 -> 오뎅기계 빈 슬롯에 투입
        if (player.IsHoldingItem)
        {
            // 💡 구조체 형식을 지우고 순수 FoodData 참조로 가져옵니다.
            FoodData heldData = player.CurrentHeldData;

            // 💡 데이터 규칙에 맞게 'foodName' 필드로 필터링을 수행합니다.
            if (heldData.foodName == "오뎅이 꽂혀있는 꼬치")
            {
                // 빈 구멍 찾기
                int emptySlotIndex = slots.FindIndex(x => !x.isOccupied);

                if (emptySlotIndex != -1)
                {
                    PutOdengInSlot(emptySlotIndex, heldData, player);
                }
                else
                {
                    Debug.LogWarning("오뎅기계: 모든 슬롯이 가득 찼습니다! 완성된 오뎅을 먼저 빼세요.");
                }
            }
            else
            {
                Debug.LogWarning("오뎅기계: 이 기계에는 오뎅 꼬치만 넣을 수 있습니다.");
            }
        }
    }

    // 빈 슬롯에 오뎅을 넣고 즉시 조리를 시작하는 함수
    private void PutOdengInSlot(int index, FoodData heldData, PlayerInteractor player)
    {
        OdengSlot slot = slots[index];
        slot.isOccupied = true;
        slot.isCooking = true;
        slot.isDone = false;
        slot.timer = 0f;

        // [업그레이드 연동] 기계의 속도 단축 비율 계산
        float speedModifier = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetSpeedModifier(machineID) : 1f;
        slot.targetCookTime = baseCookTime * speedModifier;

        // 💡 손을 비우기 전에 프리팹 정보를 사용하여 슬롯 위치에 3D 비주얼을 복사 소환합니다.
        if (heldData.prefab != null && slotTransforms.Count > index && slotTransforms[index] != null)
        {
            slot.visualRef = Instantiate(heldData.prefab, slotTransforms[index]);
            slot.visualRef.transform.localPosition = Vector3.zero;
            slot.visualRef.transform.localRotation = Quaternion.identity;
        }

        // 플레이어 손 비우기
        player.ClearHand();

        slots[index] = slot; // 구조체 값 갱신
        Debug.Log($"오뎅기계: {index + 1}번 슬롯에 오뎅을 넣었습니다. 즉시 조리 시작! (소요 시간: {slot.targetCookTime:F1}초)");
    }

    // 완성된 오뎅을 슬롯에서 꺼내 플레이어에게 쥐여주는 함수
    private void TakeOdengFromSlot(int index, PlayerInteractor player)
    {
        OdengSlot slot = slots[index];
        FoodData resultData = null; // 💡 FoodData 타입 변수로 선언

        // 타버리는(불어 터지는) 시간 기준을 넘었는지 확인하여 분기 처리
        if (slot.timer >= baseBurnTime)
        {
            resultData = burntOdengData;
            Debug.Log($"오뎅기계: {index + 1}번 슬롯에서 너무 불어 터져서 '탄 오뎅 꼬치'를 꺼냈습니다.");
        }
        else
        {
            resultData = cookedOdengData;
            Debug.Log($"오뎅기계: {index + 1}번 슬롯에서 맛있게 익은 '완성된 오뎅 꼬치'를 꺼냈습니다!");
        }

        // 💡 슬롯 데이터 초기화 및 비주얼 오브젝트 파괴 전에 안전하게 들려주기
        if (resultData != null)
        {
            player.HoldNewData(resultData);
        }

        // 비주얼 오브젝트 파괴 및 슬롯 초기화
        if (slot.visualRef != null) Destroy(slot.visualRef);
        
        slot.isOccupied = false;
        slot.isCooking = false;
        slot.isDone = false;
        slot.timer = 0f;
        slot.visualRef = null;

        slots[index] = slot; // 구조체 값 갱신
    }

    void Update()
    {
        // 💡 모든 슬롯을 실시간으로 돌며 각각 독립적인 타이머를 증가시킵니다!
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].isOccupied) continue;

            OdengSlot slot = slots[i];

            // A. 아직 조리 중인 경우
            if (slot.isCooking)
            {
                slot.timer += Time.deltaTime;
                if (slot.timer >= slot.targetCookTime)
                {
                    slot.isCooking = false;
                    slot.isDone = true;
                    slot.timer = 0f; // 이 타이머는 이제 완성 후 방치 시간(탄 타이머)으로 사용됩니다.
                    Debug.Log($"오뎅기계: {i + 1}번 슬롯의 오뎅 조리가 완료되었습니다!");
                }
            }
            // B. 조리 완료 후 꺼내지 않고 방치되고 있는 경우
            else if (slot.isDone)
            {
                slot.timer += Time.deltaTime;
            }

            slots[i] = slot; // 변경된 타이머 값을 리스트에 재저장
        }
    }
}