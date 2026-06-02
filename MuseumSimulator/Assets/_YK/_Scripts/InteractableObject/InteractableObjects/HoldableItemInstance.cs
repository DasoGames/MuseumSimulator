using UnityEngine;

public class HoldableItemInstance : MonoBehaviour
{
    // ⭐ [핵심] 이 3D 프리팹이 품고 있는 실제 구조체 데이터입니다.
    // 인스펙터에서 이 프리팹이 어떤 아이템(이름, 상태, 자신 프리팹)인지 직접 설정해둘 수 있습니다.
    [Header("이 물체가 소유한 구조체 데이터")]
    public HoldableObject myData;

    /// <summary>
    /// 플레이어 손에 소환될 때, 최신 구조체 데이터를 주입하여 동기화하는 함수입니다.
    /// </summary>
    public void Setup(HoldableObject data)
    {
        myData = data;
    }

    /// <summary>
    /// 플레이어가 상호작용하여 손에 들었을 때(카메라 앞으로 올 때) 호출되는 함수입니다.
    /// 손에 쥐고 다니는 동안 물리 엔진이나 충돌체가 맵에 걸려 튕기는 현상을 막아줍니다.
    /// </summary>
    public void OnPickedUp()
    {
        // 1. 물리 연산 컴포넌트(Rigidbody)가 있다면 무중력/강체 상태(isKinematic)로 바꿉니다.
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false; // 확실하게 중력도 꺼줍니다.
        }

        // 2. 다른 물체나 맵에 부딪혀 플레이어가 덜덜 떨리는 걸 막기 위해 충돌체(Collider)를 꺼줍니다.
        // 자식 오브젝트들에 콜라이더가 분산되어 있는 경우를 대비해 모든 콜라이더를 찾아 끕니다.
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }

    /// <summary>
    /// 플레이어가 아이템을 바닥에 내려놓거나 도마, 기계 위에 올려놓을 때 호출되는 함수입니다.
    /// 꺼두었던 물리 엔진과 충돌 처리를 다시 원래대로 켜줍니다.
    /// </summary>
    public void OnDropped()
    {
        // 1. 물리 엔진을 다시 정상 작동시킵니다.
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // 2. 꺼두었던 모든 충돌체를 다시 활성화합니다.
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
    }
}