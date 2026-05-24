using UnityEngine;

public class TteokbokkiDisplay : MonoBehaviour, IInteractable
{
    [Header("진열대 설정")]
    public int maxServings = 10; // 완성작 하나당 채워지는 최대 서빙(컵) 수
    
    [Header("결과물 데이터")]
    // 💡 소분했을 때 플레이어 손에 쥐여줄 '컵에 담긴 떡볶이'의 구조체 데이터입니다.
    public HoldableObject tteokbokkiCupData; // 이름: "떡볶이컵"

    [Header("진열대 상태 (Debug)")]
    [SerializeField] private bool hasFood = false;    // 현재 진열대에 떡볶이가 채워져 있는가?
    [SerializeField] private int remainingServings = 0; // 남은 서빙(컵) 횟수

    [Header("시각적 연출용 (선택사항)")]
    public GameObject foodVisualObject; // 진열대에 떡볶이가 차 있을 때만 켜줄 3D 메쉬 오브젝트

    private void Start()
    {
        UpdateVisual();
    }

    // 💡 인터페이스 구현 (플레이어가 바라보고 E키를 톡 눌렀을 때 실행)
    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // [상황 1] 진열대가 비어있고, 플레이어가 완수된 음식을 들고 왔을 때 -> 진열대에 채우기
        if (!hasFood)
        {
            if (!player.IsHoldingItem)
            {
                Debug.Log("진열대: 현재 비어있습니다. 완성된 떡볶이를 조리해서 가져오세요.");
                return;
            }

            HoldableObject heldData = player.CurrentHeldData.Value;

            // 오직 이름이 "완성된떡볶이"인 데이터만 받음
            if (heldData.objectName == "완성된떡볶이")
            {
                player.ClearHand(); // 플레이어 손 비우기 (냄비/그릇 수거)
                
                hasFood = true;
                remainingServings = maxServings; // 10회로 충전!
                
                UpdateVisual();
                Debug.Log($"진열대: 대형 떡볶이 판에 음식을 부었습니다! 이제 컵으로 판매 가능합니다. (남은 수량: {remainingServings}컵)");
            }
            else if (heldData.objectName == "탄떡볶이")
            {
                Debug.LogWarning("진열대: 상하거나 탄 요리는 판매용 진열대에 올릴 수 없습니다!");
            }
            else
            {
                Debug.LogWarning("진열대: 완성된 떡볶이만 진열할 수 있습니다.");
            }
            return;
        }

        // [상황 2] 진열대에 떡볶이가 채워져 있을 때 -> 빈손으로 누르면 한 컵씩 꺼내기
        if (hasFood)
        {
            if (player.IsHoldingItem)
            {
                Debug.LogWarning("진열대: 떡볶이를 컵에 담으려면 손을 비우고 상호작용하세요!");
                return;
            }

            // 1컵 차감 및 플레이어에게 컵 지급
            remainingServings--;
            player.HoldNewData(tteokbokkiCupData);
            Debug.Log($"진열대: 떡볶이를 한 컵 퍼서 손에 쥐었습니다. (남은 수량: {remainingServings}/{maxServings})");

            // 10번을 다 꺼내 먹었다면(완판) 진열대 초기화
            if (remainingServings <= 0)
            {
                hasFood = false;
                remainingServings = 0;
                Debug.Log("진열대: 떡볶이를 모두 소진했습니다! 새로운 판을 요리해 오세요.");
            }

            UpdateVisual();
        }
    }

    // 진열대 상태에 따라 3D 음식을 켜고 끄는 간단한 비주얼 갱신 함수
    private void UpdateVisual()
    {
        if (foodVisualObject != null)
        {
            // 떡볶이가 채워져 있을 때만 빨간 떡볶이 메쉬를 보이게 만듬
            foodVisualObject.SetActive(hasFood);
        }
    }
}