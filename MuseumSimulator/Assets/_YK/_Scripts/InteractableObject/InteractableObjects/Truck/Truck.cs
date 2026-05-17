using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Truck : MonoBehaviour, IInteractable
{
    [Header("연결할 컴포넌트")]
    public TruckController truckController; // 트럭 컨트롤러 스크립트
    public Camera truckCamera;              // 탑다운 뷰를 비출 트럭 전용 카메라

    void Start()
    {
        // 시작할 때는 트럭 제어와 카메라를 꺼둡니다.
        if (truckController != null) truckController.enabled = false;
        if (truckCamera != null) truckCamera.gameObject.SetActive(false);
    }

    public void Interact()
    {
        // 플레이어 컨트롤러를 찾습니다.
        FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
        
        if (player != null && truckController != null)
        {
            Debug.Log("트럭 탑승! 탑다운 뷰로 전환합니다.");

            // 1. 플레이어 기능 정지 및 시각적으로 숨기기
            player.enabled = false;
            player.gameObject.transform.GetChild(0).gameObject.SetActive(false); // 메인 카메라 숨기기 (또는 플레이어 메쉬)
            
            // 2. 트럭 카메라 활성화 및 컨트롤러 켜기
            if (truckCamera != null) truckCamera.gameObject.SetActive(true);
            
            // 트럭 컨트롤러에게 플레이어 정보를 넘겨주며 활성화
            truckController.EnterTruck(player);
        }
    }
}