using UnityEngine;

public class Blender : MonoBehaviour, IInteractable
{
    [Header("블렌더 설정")]
    public string machineID = "Blender";
    public float baseBlendTime = 5f;    // 과일이 자동으로 갈리는 기본 조리 시간 (초)
    public float baseHoldTime = 5f;     // 다 갈린 후 컵에 담기 위해 꾹 눌러야 하는 시간 (초)

    [Header("결과물 데이터")]
    // 💡 과일 종류가 하나이므로, 결과물 주스 구조체 데이터도 딱 하나만 연결하면 됩니다!
    public HoldableObject cookedJuiceData; // 이름: "과일주스"

    [Header("블렌더 내부 3D 배치 위치")]
    public Transform fruitPlaceTransform; // 블렌더 안에 과일이나 주스 비주얼을 띄울 위치

    // 블렌더 상태 제어 변수들
    private bool hasFruit = false;       // 현재 블렌더에 과일이 들어갔는가?
    private bool isBlending = false;     // 현재 자동으로 윙윙 갈리는 중인가?
    private bool isDoneBlending = false; // 갈기가 끝나서 수거 대기 중인가?

    private float timer = 0f;
    private float targetBlendTime = 0f;  // 업그레이드가 반영된 최종 믹싱 시간
    private GameObject spawnedVisual = null; // 내부 시각 연출용 프리팹

    private PlayerInteractor targetPlayer = null;

    // 💡 인터페이스 구현 (E키를 톡 눌렀을 때 실행)
    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // [단계 1] 블렌더가 비어있고, 플레이어가 과일을 들고 있을 때 -> 과일 투입 및 즉시 가동
        if (!hasFruit && !isBlending && !isDoneBlending && player.IsHoldingItem)
        {
            HoldableObject heldData = player.CurrentHeldData.Value;

            // 💡 과일 종류가 하나이므로 오직 "과일" 이름만 정직하게 확인합니다!
            if (heldData.objectName == "과일")
            {
                hasFruit = true;
                isBlending = true; // 투입하자마자 자동으로 조리(믹싱) 시작
                timer = 0f;

                // [업그레이드 연동] 블렌더 갈리는 속도 단축 반영
                float speedModifier = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetSpeedModifier(machineID) : 1f;
                targetBlendTime = baseBlendTime * speedModifier;

                // 손 비우고 블렌더 안에 과일 3D 비주얼 생성
                player.ClearHand();
                if (heldData.prefab != null && fruitPlaceTransform != null)
                {
                    spawnedVisual = Instantiate(heldData.prefab, fruitPlaceTransform);
                    spawnedVisual.transform.localPosition = Vector3.zero;
                    spawnedVisual.transform.localRotation = Quaternion.identity;
                }

                Debug.Log($"블렌더: 과일 투입 완료! 자동으로 갈기 시작합니다. (소요 시간: {targetBlendTime:F1}초)");
            }
            else
            {
                Debug.LogWarning("블렌더: 여기에는 '과일'만 넣을 수 있습니다!");
            }
        }
    }

    void Update()
    {
        // [단계 2] 윙윙 자동으로 갈리는 타이머 상태
        if (isBlending)
        {
            timer += Time.deltaTime;
            
            if (timer >= targetBlendTime)
            {
                isBlending = false;
                isDoneBlending = true; // 갈기 완료! 이제 플레이어가 5초간 꾹 눌러서 가져가야 함
                timer = 0f;            // 꾹 누르기 타이머용으로 리셋
                Debug.Log("블렌더: 과일이 모두 갈렸습니다! 이제 빈손으로 E키를 [5초간 꾹 눌러] 주스를 담으세요.");
            }
        }

        // [단계 3] 갈기 완료 후 플레이어가 빈손으로 E 키를 꾹~ 누르고 있을 때 수거 진행
        if (isDoneBlending)
        {
            if (targetPlayer == null)
            {
                targetPlayer = FindFirstObjectByType<PlayerInteractor>();
                if (targetPlayer == null) return;
            }

            // 플레이어가 E 키를 꾹 누르고 있고 + 손이 완벽히 비어있을 때만 게이지가 올라감
            if (Input.GetKey(KeyCode.E) && !targetPlayer.IsHoldingItem)
            {
                timer += Time.deltaTime;
                Debug.Log($"블렌더: 주스 담는 중... 진행도: {((timer / baseHoldTime) * 100f):F0}%");

                // 5초 동안 완전히 다 꾹 눌렀다면 완성 수거!
                if (timer >= baseHoldTime)
                {
                    CompleteHolding();
                }
            }
            else
            {
                // 중간에 E 키를 떼거나 손에 무언가를 들면 누르기 타임이 초기화됩니다.
                timer = 0f;
            }
        }
    }

    // 5초 꾹 누르기가 끝나 주스를 최종 수거하는 함수
    private void CompleteHolding()
    {
        isDoneBlending = false;
        hasFruit = false;
        timer = 0f;

        // 갈리던 시각 비주얼 삭제
        if (spawnedVisual != null) Destroy(spawnedVisual);

        // 완수된 주스 1컵을 플레이어 빈손에 쏙 쥐여줍니다.
        if (targetPlayer != null)
        {
            targetPlayer.HoldNewData(cookedJuiceData);
            Debug.Log($"블렌더: 컵에 완전히 담았습니다! 플레이어에게 '{cookedJuiceData.objectName}' 지급 완료.");
        }
    }
}