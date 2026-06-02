using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(Rigidbody))]
public class TruckController : MonoBehaviour
{
    [Header("운전 능력치 (레이더 충돌 방식)")]
    public float moveSpeed = 20f;       // 전진/후진 속도 (안 가면 수치를 더 올리세요!)
    public float rotationSpeed = 100f;  // 회전 속도

    [Header("장애물 감지 레이더 설정")]
    public float collisionCheckDistance = 1.8f; // 앞에 벽이 있는지 검사할 레이저 거리 (트럭 크기에 맞게 조절)
    public LayerMask hitLayers;                 // 충돌을 인정할 레이어 (기본값: Everything 추천)

    [Header("3인칭 자유 공전 카메라 설정 (복귀 없음)")]
    public Camera truckCamera;          
    public Transform cameraPivot;       
    public float cameraDistance = 7f;   
    public float mouseSensitivity = 2f; 
    public float minYAngle = -5f;       
    public float maxYAngle = 65f;       

    [Header("하차 위치 설정")]
    public Transform exitPoint;

    private FirstPersonController driverPlayer;
    private Rigidbody truckRigidbody;
    
    // 시점 제어용 내부 변수
    private float currentXRotation = 0f; 
    private float currentYRotation = 0f; 
    private bool isDriving = false;

    void Awake()
    {
        isDriving = true;
        truckRigidbody = GetComponent<Rigidbody>();
        if (truckRigidbody != null)
        {
            // ⭐ [해결 핵심 1] 물리 마찰력 버그를 원천 차단하기 위해 리지드바디 연산을 끕니다.
            truckRigidbody.useGravity = false; 
            truckRigidbody.isKinematic = true; 
        }

        // 레이더 검사 기본 레이어를 Everything(모든 물체 부딪힘)으로 초기 세팅
        if (hitLayers.value == 0)
        {
            hitLayers = ~0; 
        }
    }

    public void EnterTruck(FirstPersonController player)
    {
        driverPlayer = player;
        isDriving = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraPivot != null)
        {
            currentXRotation = transform.eulerAngles.y;
            currentYRotation = 20f; 
            cameraPivot.rotation = Quaternion.Euler(currentYRotation, currentXRotation, 0f);
        }
    }

    void Update()
    {
        if (!isDriving) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            ExitTruck();
            return;
        }

        HandleFreeOrbitCamera();
    }

    void FixedUpdate()
    {
        if (!isDriving) return;
        HandleArcadeMovement();
    }

    private void HandleFreeOrbitCamera()
    {
        if (truckCamera == null || cameraPivot == null) return;

        float rawMouseX = Input.GetAxis("Mouse X");
        float rawMouseY = Input.GetAxis("Mouse Y");

        float mouseX = (Mathf.Abs(rawMouseX) > 0.001f) ? rawMouseX * mouseSensitivity : 0f;
        float mouseY = (Mathf.Abs(rawMouseY) > 0.001f) ? rawMouseY * mouseSensitivity : 0f;

        currentXRotation += mouseX;
        currentYRotation -= mouseY;

        currentYRotation = Mathf.Clamp(currentYRotation, minYAngle, maxYAngle);

        Quaternion rotation = Quaternion.Euler(currentYRotation, currentXRotation, 0f);
        cameraPivot.rotation = rotation;

        Vector3 position = cameraPivot.position - (rotation * Vector3.forward * cameraDistance);
        truckCamera.transform.position = position;
        
        truckCamera.transform.LookAt(cameraPivot.position + Vector3.up * 0.5f);
    }

    // --- 🏎️ 레이더 감지 기반 완벽 가속 주행 시스템 ---
    private void HandleArcadeMovement()
    {
        float moveInput = Input.GetAxis("Vertical");   // W, S
        float turnInput = Input.GetAxis("Horizontal"); // A, D

        // 꼬여있는 트럭 앞방향 축 보정치 (-transform.right가 실제 3D 전방)
        Vector3 truckForwardDirection = -transform.right;

        // 1. 전진(W)이나 후진(S) 키가 입력되고 있을 때만 회전(AD) 작동
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            float directionModifier = moveInput > 0f ? 1f : -1f;
            float turnAmount = turnInput * rotationSpeed * Time.fixedDeltaTime * directionModifier;
            transform.Rotate(Vector3.up, turnAmount);
        }

        // ⭐ [해결 핵심 2] 레이더(Raycast)로 이동할 방향에 벽이 있는지 미리 탐지합니다.
        // 내가 가려는 방향(W면 전방, S면 후방)으로 눈에 보이지 않는 레이저를 쏩니다.
        Vector3 rayDirection = moveInput > 0f ? truckForwardDirection : -truckForwardDirection;
        
        // 트럭 정중앙 바닥에서 살짝 위(0.5f) 지점에서 레이저 발사
        Vector3 rayStartPoint = transform.position + Vector3.up * 0.5f; 

        bool isWallAhead = false;
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            // 정면에 벽이나 건물 콜라이더가 포착되었는지 검사
            isWallAhead = Physics.Raycast(rayStartPoint, rayDirection, collisionCheckDistance, hitLayers);
        }

        // 디버그용: 유니티 씬(Scene) 창에서 레이저가 어떻게 발사되는지 빨간색/녹색 선으로 실시간 시각화
        Debug.DrawRay(rayStartPoint, rayDirection * collisionCheckDistance, isWallAhead ? Color.red : Color.green);

        // 3. 벽이 앞을 막고 있지 않을 때만 강제로 좌표를 이동시킵니다!
        // 이 방식은 바닥 마찰력을 완벽하게 무시하므로 무조건 미끄러지듯 시원하게 달립니다.
        if (!isWallAhead)
        {
            Vector3 moveDistance = truckForwardDirection * moveInput * moveSpeed * Time.fixedDeltaTime;
            transform.position += moveDistance;
        }
        else
        {
            // 벽에 가로막혔을 땐 억지로 가려 하지 않고 속도를 0으로 만들어 줍니다.
            if (truckRigidbody != null) truckRigidbody.linearVelocity = Vector3.zero;
        }
    }

    private void ExitTruck()
    {
        if (driverPlayer == null) return;

        isDriving = false;

        if (truckRigidbody != null)
        {
            truckRigidbody.isKinematic = false;
            truckRigidbody.useGravity = true;
            truckRigidbody.linearVelocity = Vector3.zero;
            truckRigidbody.angularVelocity = Vector3.zero;
        }

        if (truckCamera != null)
        {
            truckCamera.tag = "Untagged";
            truckCamera.gameObject.SetActive(false);
        }

        driverPlayer.gameObject.SetActive(true);
        driverPlayer.enabled = true;

        if (exitPoint != null)
        {
            driverPlayer.gameObject.transform.position = exitPoint.position;
        }
        else
        {
            driverPlayer.gameObject.transform.position = transform.position + (transform.right * 3.5f);
        }

        if (driverPlayer.transform.childCount > 0)
        {
            driverPlayer.transform.GetChild(0).gameObject.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        driverPlayer = null;
        this.enabled = false;
        Debug.Log("트럭 하차 완료.");
    }
}