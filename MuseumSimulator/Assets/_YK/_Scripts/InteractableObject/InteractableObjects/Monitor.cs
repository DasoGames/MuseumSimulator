using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

public class Monitor : MonoBehaviour, IInteractable
{
    [Header("카메라 설정")]
    public Transform cameraTargetPos; // 모니터 앞 카메라 위치 (Empty Object)
    public float transitionSpeed = 5f;

    [Header("커서 설정")]
    public Canvas monitorCanvas;

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
        Cursor.visible = true; // "커서는 움직이지만 보이지 않게" 설정

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
        monitorCanvas.gameObject.SetActive(true);

    }

    void Update()
    {
        if (isUsing)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitMonitorMode();
            }
        }
    }


    void ExitMonitorMode()
    {
        isUsing = false;
        playerController.enabled = true;
        monitorCanvas.gameObject.SetActive(false);
    }
}