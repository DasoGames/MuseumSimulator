using UnityEngine;
using UnityEngine.SceneManagement;

public class TravelZoneTrigger : MonoBehaviour
{
    [Header("UI 설정")]
    [Tooltip("트럭이 진입했을 때 화면에 띄울 '지도 캔버스(또는 패널)' 오브젝트를 연결하세요.")]
    public GameObject mapUIPanel;

    // 💡 내부에서 일시정지시킬 트럭의 컨트롤러와 리지드바디를 기억해두는 캐싱 변수
    private TruckController activeTruckController;
    private Rigidbody activeTruckRigidbody;

    private void Start()
    {
        // 게임 시작 시 지도 UI는 당연히 꺼둡니다.
        if (mapUIPanel != null)
        {
            mapUIPanel.SetActive(false);
        }
    }

    // ⭐ [플로우 1] 트럭이 이 구역에 엔터(진입)하면 조작을 끄고 맵 UI를 띄워줍니다.
    private void OnTriggerEnter(Collider other)
    {
        // 들어온 물체에서 트럭 컨트롤러 컴포넌트를 추출합니다.
        TruckController truck = other.GetComponentInParent<TruckController>();
        if (truck == null) truck = other.GetComponent<TruckController>();

        // 플레이어 본체거나 트럭인 경우 처리
        if (other.CompareTag("Player") || truck != null)
        {
            if (mapUIPanel != null)
            {
                Debug.Log("🚚 지역 이동 구역 진입! 트럭 조작을 잠그고 지도 UI를 켭니다.");
                
                // 1. 맵 UI 활성화 및 커서 풀기
                mapUIPanel.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // 2. ⭐ [핵심 추가] 트럭 컨트롤러 조작 완전히 끄기
                if (truck != null)
                {
                    activeTruckController = truck; // 나중에 다시 켜주기 위해 기록
                    activeTruckController.enabled = false; // 주행/조향 스크립트 Off

                    // 3. ⭐ [안전장치] 미끄러짐 방지를 위해 트럭 물리 속도를 제로(0)로 강제 고정
                    activeTruckRigidbody = truck.GetComponent<Rigidbody>();
                    if (activeTruckRigidbody != null)
                    {
                        activeTruckRigidbody.linearVelocity = Vector3.zero;
                        activeTruckRigidbody.angularVelocity = Vector3.zero;
                    }
                }
            }
            else
            {
                Debug.LogError("TravelZoneTrigger: 인스펙터에 mapUIPanel이 연결되지 않았습니다!");
            }
        }
    }

    // ⭐ [플로우 2] 지도 UI의 각 지역 '이동하기' 버튼들이 호출할 public 메서드
    public void LoadSelectedRegionScene(string regionName)
    {
        if (!string.IsNullOrEmpty(regionName))
        {
            string sceneToLoad = regionName;
            Debug.Log($"🎯 지도 UI에서 [{regionName}] 선택됨. 씬 로딩 시작: {sceneToLoad}");
            
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("LoadSelectedRegionScene: 넘겨받은 지역 이름(regionName)이 비어있습니다!");
        }
    }

    // 💡 지도를 열었다가 이동 안 하고 그냥 닫을 때 (취소 버튼에 연결)
    public void CloseMapUI()
    {
        if (mapUIPanel != null)
        {
            mapUIPanel.SetActive(false);
            
            // 다시 운전에 집중할 수 있게 마우스 커서를 가둡니다.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // ⭐ [핵심 추가] 취소하고 닫았으므로, 잠가놨던 트럭 제어권을 다시 복구시킵니다!
            if (activeTruckController != null)
            {
                activeTruckController.enabled = true;
                activeTruckController = null; // 참조 해제
            }
        }
    }
}