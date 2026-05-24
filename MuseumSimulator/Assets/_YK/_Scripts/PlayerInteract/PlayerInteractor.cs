using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 3f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Hand Settings (카메라 앞 배치)")]
    public Transform handSlot; // 카메라 자식으로 둔 빈 오브젝트 (스폰 위치)

    // 실제 손에 들린 3D 오브젝트 실체와 순수 구조체 데이터
    private HoldableItemInstance currentHeldInstance = null;
    private HoldableObject? currentHeldData = null;

    // 외부에 현재 들고 있는 순수 구조체 데이터를 안전하게 제공 (Nullable)
    public HoldableObject? CurrentHeldData => currentHeldData;
    public bool IsHoldingItem => currentHeldData.HasValue;

    // 👁️ 인스펙터에서 구조체 내부(이름, 상태, 연결된 프리팹)를 통째로 모니터링하기 위한 필드
    [Header("현재 손에 든 구조체 데이터 (Debug)")]
    [SerializeField] private bool isHolding;
    [SerializeField] private HoldableObject debugHeldData;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            PerformInteraction();
        }
    }

    void PerformInteraction()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null) interactable.Interact();
        }
    }

    // ⭐ [에러 해결용 핵심 함수] 냉장고 UI나 기계에서 구조체 데이터를 툭 던져줄 때 실행됩니다.
    public void HoldNewData(HoldableObject newData)
    {
        if (newData.prefab == null)
        {
            Debug.LogError($"[PlayerInteractor] 던져진 '{newData.objectName}' 데이터에 3D 프리팹이 연결되어 있지 않습니다!");
            return;
        }

        // 1. 기존에 들고 있던 실체가 있다면 흔적도 없이 파괴합니다.
        if (IsHoldingItem) ClearHand();

        currentHeldData = newData;

        // 2. 구조체가 직접 품고 있는 프리팹을 카메라 앞에 소환합니다!
        GameObject spawnedObj = Instantiate(newData.prefab, handSlot);
        currentHeldInstance = spawnedObj.GetComponent<HoldableItemInstance>();

        if (currentHeldInstance != null)
        {
            // 실체화된 오브젝트에게 현재 들고 있는 최신 데이터를 주입해 동기화합니다.
            currentHeldInstance.Setup(newData);
            currentHeldInstance.OnPickedUp();

            // 위치와 회전을 handSlot에 딱 맞게 정렬합니다.
            spawnedObj.transform.localPosition = Vector3.zero;
            spawnedObj.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogError($"{newData.prefab.name} 프리팹에 'HoldableItemInstance' 컴포넌트가 없습니다!");
            Destroy(spawnedObj);
            currentHeldData = null;
        }

        UpdateInspectorDebug();
    }

    // 손에 든 실체와 데이터를 모두 비울 때 사용합니다.
    public void ClearHand()
    {
        if (!IsHoldingItem) return;

        if (currentHeldInstance != null)
        {
            Destroy(currentHeldInstance.gameObject);
        }
        
        currentHeldInstance = null;
        currentHeldData = null;
        
        UpdateInspectorDebug();
    }

    // 인스펙터 디버그용 필드 실시간 동기화
    private void UpdateInspectorDebug()
    {
        isHolding = IsHoldingItem;
        debugHeldData = IsHoldingItem ? currentHeldData.Value : default;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * interactRange);
    }
}