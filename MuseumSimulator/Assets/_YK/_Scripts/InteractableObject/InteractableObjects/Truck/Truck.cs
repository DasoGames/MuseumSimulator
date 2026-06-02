using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Truck : MonoBehaviour, IInteractable
{
    [Header("연결할 컴포넌트")]
    public TruckController truckController; 
    public Camera truckCamera;              

    void Start()
    {
        if (truckCamera != null) truckCamera.gameObject.SetActive(false);
        if (truckController != null) truckController.enabled = false;
    }

    public void Interact()
    {
        FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
        
        if (player != null && truckController != null && truckCamera != null)
        {
            Debug.Log("<color=cyan>▶ 트럭 탑승: 1인칭 마우스 입력 체인 완전 차단</color>");

            // 💡 [해결 핵심 1] 플레이어를 비활성화하기 전에, 마우스 조작 컴포넌트를 강제로 Unfreeze 시켜 먹통 버그 예방
            player.enabled = false;
            
            // 💡 [해결 핵심 2] 플레이어 자식에 들어있는 1인칭 카메라와 오디오 리스너를 완전히 꺼서 카메라 간섭 차단
            if (player.transform.childCount > 0)
            {
                player.transform.GetChild(0).gameObject.SetActive(false);
            }

            // 플레이어 본체를 통째로 숨김 처리하여 백그라운드 연산 방지
            player.gameObject.SetActive(false);
            
            // 3인칭 트럭 카메라 가동 및 권한 이양
            truckCamera.gameObject.SetActive(true);
            truckCamera.tag = "MainCamera"; 
            
            truckController.enabled = true; 
            truckController.EnterTruck(player);
        }
    }
}