using UnityEngine;
using TMPro;

public class DestinationIndicator : MonoBehaviour
{
    [Header("대상 설정")]
    public Transform playerObject;      // 프리팹이라 인스펙터에서 비워둬도 됩니다!
    public Transform destinationTarget; 
    public RectTransform indicatorUI;   
    public TMP_Text distanceText;       

    [Header("둥둥 애니메이션 설정")]
    public float hoverSpeed = 3f;       
    public float hoverAmount = 15f;     

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null || !mainCamera.gameObject.activeInHierarchy)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        // 💡 ⭐ [추가된 예외 처리 연산]
        // 만약 트럭이 소환되어서 씬에 존재하는데, playerObject가 아직 비어있다면 자동으로 찾아오기!
        if (playerObject == null)
        {
            // 방법 A: 씬에 소환된 TruckController 컴포넌트를 가진 오브젝트를 찾기
            TruckController spawnedTruck = FindFirstObjectByType<TruckController>();
            if (spawnedTruck != null)
            {
                playerObject = spawnedTruck.transform;
            }
        }

        if (destinationTarget == null || indicatorUI == null) return;

        // 1. 📏 [거리 계산] 이제 소환된 트럭을 찾았으므로 정상 작동합니다.
        if (playerObject != null && distanceText != null)
        {
            float distance = Vector3.Distance(playerObject.position, destinationTarget.position);
            distanceText.text = $"{Mathf.RoundToInt(distance)}m"; 
        }

        // 2. 🎥 [3D -> 2D 변환]
        Vector3 screenPos = mainCamera.WorldToScreenPoint(destinationTarget.position);

        if (screenPos.z < 0)
        {
            indicatorUI.gameObject.SetActive(false);
            return;
        }

        indicatorUI.gameObject.SetActive(true);

        // 3. 🎈 [둥둥 애니메이션]
        float hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverAmount;

        // 4. 최종 좌표 주입
        indicatorUI.position = new Vector3(screenPos.x, screenPos.y + hoverOffset, 0f);
    }
}