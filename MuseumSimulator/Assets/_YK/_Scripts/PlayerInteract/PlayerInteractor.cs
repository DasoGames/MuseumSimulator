using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("상호작용 설정")]
    public float interactRange = 3f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Hand Settings (카메라 앞 배치)")]
    public Transform handSlot; // 카메라 자식으로 둔 빈 오브젝트 (스폰 위치)

    // 💡 [데이터 대통합] 기존 구조체 대신, 새로 만든 ScriptableObject인 FoodData를 직접 들고 있습니다!
    private FoodData currentHeldData = null;

    // 외부에 현재 들고 있는 순수 데이터 에셋을 안전하게 제공
    public FoodData CurrentHeldData => currentHeldData;
    public bool IsHoldingItem => currentHeldData != null;

    // 👁️ 인스펙터에서 현재 손의 상태를 실시간 모니터링하기 위한 필드
    [Header("현재 손에 든 데이터 (Debug)")]
    [SerializeField] private bool isHolding;
    [SerializeField] private FoodData debugHeldData;

    private GameObject spawnedHandObject = null; // 현재 손 위치에 생성되어 눈에 보이는 3D 실체 오브젝트

    void Update()
    {
        // 💡 [수정 사항 반영] 저번에 누락되었던 E키 레이캐스트 상호작용 로직 복구 완료!
        if (Input.GetKeyDown(KeyCode.Escape) == false && Input.GetKeyDown(KeyCode.E))
        {
            PerformInteraction();
        }
    }

    // 눈앞에 있는 오브젝트가 IInteractable을 가지고 있다면 상호작용 메서드 실행
    void PerformInteraction()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null) 
            {
                interactable.Interact();
            }
        }
    }

    /// <summary>
    /// ⭐ 냉장고 UI나 기계에서 플레이어 손에 FoodData 에셋을 툭 던져줄 때 실행됩니다.
    /// </summary>
    public void HoldNewData(FoodData newData)
    {
        if (newData == null) return;

        if (newData.prefab == null)
        {
            Debug.LogError($"[PlayerInteractor] 던져진 '{newData.foodName}' 데이터에 3D 프리팹이 연결되어 있지 않습니다!");
            return;
        }

        // 1. 기존에 들고 있던 실체가 있다면 깔끔하게 파괴합니다.
        if (IsHoldingItem) ClearHand();

        currentHeldData = newData;

        // 2. 푸드 데이터 에셋이 직접 품고 있는 3D 프리팹을 카메라 앞(handSlot)에 소환합니다!
        spawnedHandObject = Instantiate(newData.prefab, handSlot);

        if (spawnedHandObject != null)
        {
            // 위치와 회전을 handSlot에 딱 맞게 정렬합니다.
            spawnedHandObject.transform.localPosition = Vector3.zero;
            spawnedHandObject.transform.localRotation = Quaternion.identity;

            // 💡 들고 있는 상태에서 물리 연산이나 충돌이 튀지 않도록 리지드바디와 콜라이더를 꺼줍니다.
            Rigidbody rb = spawnedHandObject.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Collider col = spawnedHandObject.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        UpdateInspectorDebug();
        Debug.Log($"플레이어 손: '{newData.foodName}' 에셋을 들고 3D 프리팹 생성을 완료했습니다.");
    }

    /// <summary>
    /// 기계에 재료를 넣거나 손을 완전히 비울 때 사용합니다.
    /// </summary>
    public void ClearHand()
    {
        if (!IsHoldingItem) return;

        // 화면에 보이던 3D 오브젝트 프리팹 파괴
        if (spawnedHandObject != null)
        {
            Destroy(spawnedHandObject);
            spawnedHandObject = null;
        }
        
        currentHeldData = null;
        
        UpdateInspectorDebug();
    }

    // 인스펙터 디버그용 필드 실시간 동기화
    private void UpdateInspectorDebug()
    {
        isHolding = IsHoldingItem;
        debugHeldData = currentHeldData;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * interactRange);
    }
}