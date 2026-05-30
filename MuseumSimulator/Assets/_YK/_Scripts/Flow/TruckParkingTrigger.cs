using UnityEngine;
using UnityEngine.SceneManagement;

public class TruckParkingTrigger : MonoBehaviour
{
    [Header("판매 씬 이동 설정")]
    [Tooltip("이 자리에 주차하고 E키를 누르면 이동할 판매 씬 이름을 정확히 적으세요. (예: Gangnam_Shop)")]
    public string targetShopSceneName;

    [Header("주차 게이지 설정")]
    [Tooltip("E키를 몇 초 동안 누르고 있어야 주차가 완료될지 지정합니다.")]
    public float requiredHoldTime = 1.0f; 

    private float currentHoldTime = 0f;
    private bool isInsideParkingZone = false;

    void Update()
    {
        // 💡 트럭이 주차 구역 내부에 들어와 있을 때만 키 입력을 검사합니다.
        if (!isInsideParkingZone) return;

        // E키를 꾹 누르고 있는 동안 타이머 증가
        if (Input.GetKey(KeyCode.E))
        {
            currentHoldTime += Time.deltaTime;
            
            // 💡 [팁] 여기에 주차 게이지 UI(Slider 등)가 있다면 값을 실시간으로 채워줄 수 있습니다.
            // ex) parkingSlider.value = currentHoldTime / requiredHoldTime;

            if (currentHoldTime >= requiredHoldTime)
            {
                currentHoldTime = 0f;
                isInsideParkingZone = false; // 중복 로딩 방지
                
                Debug.Log($"<color=orange>🅿️ 주차 성공! 판매 주방 씬으로 다이렉트 이동합니다: {targetShopSceneName}</color>");
                
                // 지정된 1인칭 판매 씬을 즉시 로드
                SceneManager.LoadScene(targetShopSceneName);
            }
        }
        else
        {
            // 키를 중간에 떼면 누르고 있던 시간 초기화 (처음부터 다시 눌러야 함)
            currentHoldTime = 0f;
        }
    }

    // 💡 트럭(Rigidbody와 Collider가 있는 물체)이 주차 공간 트리거에 진입했을 때
    private void OnTriggerEnter(Collider other)
    {
        // 들어온 오브젝트가 트럭인지 확인 (트럭 프리팹 최상위나 콜라이더에 "Player" 태그가 있어야 함)
        if (other.CompareTag("Player") || other.GetComponentInParent<TruckController>() != null)
        {
            isInsideParkingZone = true;
            Debug.Log("🚚 트럭이 주차 자리에 들어왔습니다. [E] 키를 1초간 누르면 장사를 시작합니다.");
        }
    }

    // 💡 트럭이 주차 공간을 벗어났을 때
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<TruckController>() != null)
        {
            isInsideParkingZone = false;
            currentHoldTime = 0f; // 누적 시간 초기화
            Debug.Log("🛑 주차 공간을 벗어났습니다.");
        }
    }
}