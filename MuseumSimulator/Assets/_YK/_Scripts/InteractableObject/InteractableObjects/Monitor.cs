using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

public class Monitor : MonoBehaviour, IInteractable
{
    [Header("카메라 설정")]
    public Transform cameraTargetPos; // 모니터 앞 카메라 위치 (Empty Object)
    public float transitionSpeed = 5f;

    [Header("커서 설정")]
    public RectTransform monitorCursor; // 모니터 화면 UI 내의 커서 이미지
    public Canvas monitorCanvas;        // 모니터 월드 UI 캔버스

    private bool isUsing = false;
    private FirstPersonController playerController;
    private Camera mainCamera;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void Interact()
    {
        if (isUsing) return;

        playerController = FindFirstObjectByType<FirstPersonController>();
        if (playerController != null)
        {
            StartCoroutine(EnterMonitorMode());
        }
    }

    IEnumerator EnterMonitorMode()
    {
        isUsing = true;
        
        // 1. 플레이어 컨트롤 및 시점 회전 중지
        playerController.enabled = false; 
        // MouseLook의 커서 잠금 해제 (스크립트 구조상 내부 변수 접근이 필요할 수 있음)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false; // "커서는 움직이지만 보이지 않게" 설정

        // 2. 카메라를 모니터 앞으로 이동
        float elapsed = 0;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * transitionSpeed;
            mainCamera.transform.position = Vector3.Lerp(startPos, cameraTargetPos.position, elapsed);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, cameraTargetPos.rotation, elapsed);
            yield return null;
        }

        // 3. 모니터 전용 커서 활성화 (필요 시)
        if(monitorCursor != null) monitorCursor.gameObject.SetActive(true);
    }

    void Update()
    {
        if (isUsing)
        {
            // 모니터 안의 가짜 커서가 진짜 커서 좌표를 따라가게 함
            UpdateMonitorCursor();

            // ESC를 누르면 다시 나가는 로직 (옵션)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitMonitorMode();
            }
        }
    }

    void UpdateMonitorCursor()
    {
        if (monitorCursor == null || monitorCanvas == null) return;

        // 1. 마우스의 현재 스크린 위치 가져오기
        Vector2 mouseScreenPos = Input.mousePosition;
        
        // 2. 캔버스의 RectTransform 참조
        RectTransform canvasRect = monitorCanvas.GetComponent<RectTransform>();

        // 3. 스크린 좌표를 캔버스의 로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, 
            mouseScreenPos, 
            mainCamera, 
            out Vector2 localPos
        );

        // 4. [핵심] 캔버스 영역 밖으로 나가지 않게 좌표 제한(Clamping)
        // 캔버스의 중심이 (0,0)일 때, 좌우 끝은 -width/2 ~ +width/2 입니다.
        float halfWidth = canvasRect.rect.width / 2f;
        float halfHeight = canvasRect.rect.height / 2f;

        // 커서 이미지 자체의 크기도 고려하고 싶다면 커서 너비의 절반만큼 더 빼주면 정교해집니다.
        localPos.x = Mathf.Clamp(localPos.x, -halfWidth, halfWidth);
        localPos.y = Mathf.Clamp(localPos.y, -halfHeight, halfHeight);

        // 5. 제한된 좌표를 커서에 적용
        monitorCursor.localPosition = localPos;
    }

    void ExitMonitorMode()
    {
        isUsing = false;
        playerController.enabled = true;
        if(monitorCursor != null) monitorCursor.gameObject.SetActive(false);
        
        // Cursor를 다시 1인칭 모드로 복구하는 처리는 
        // FirstPersonController가 Update에서 MouseLook.UpdateCursorLock()을 호출하며 자동 복구합니다.
    }
}