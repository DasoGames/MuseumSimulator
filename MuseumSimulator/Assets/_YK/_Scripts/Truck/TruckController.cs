using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class TruckController : MonoBehaviour
{
    [Header("운전 능력치")]
    public float moveSpeed = 10f;     // 전진/후진 속도
    public float rotationSpeed = 90f; // 회전 속도

    [Header("하차 위치")]
    public Transform exitPoint;       // 트럭에서 내릴 때 플레이어가 스폰될 위치 (Empty Object)

    private FirstPersonController driverPlayer;
    private Camera truckCamera;

    void Start()
    {
        // 자식이나 본인에게 붙은 카메라를 자동으로 참조 (없으면 인스펙터 할당 가능)
        truckCamera = GetComponentInChildren<Camera>();
    }

    // 트럭에 탑승할 때 Truck 스크립트가 호출해 줄 메소드
    public void EnterTruck(FirstPersonController player)
    {
        driverPlayer = player;
        this.enabled = true; // Update 로직 활성화
    }

    void Update()
    {
        // 1. 트럭 이동 조작 (W, S / 위, 아래 화살표)
        float moveInput = Input.GetAxis("Vertical");
        Vector3 moveDistance = -transform.right * moveInput * moveSpeed * Time.deltaTime;
        transform.position += moveDistance;

        // 2. 트럭 회전 조작 (A, D / 좌, 우 화살표)
        // 차가 움직일 때만 회전하게 하려면 moveInput 조건을 추가해도 좋습니다.
        float rotateInput = Input.GetAxis("Horizontal");
        float rotationAmount = rotateInput * rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, rotationAmount);

        // 3. 트럭에서 내리기 (여기서는 Return(엔터) 또는 E 키로 설정)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
        {
            ExitTruck();
        }
    }

    void ExitTruck()
    {
        if (driverPlayer == null) return;

        Debug.Log("트럭에서 내립니다.");

        // 1. 플레이어 위치를 하차 지점으로 이동시키고 다시 켜기
        if (exitPoint != null)
        {
            driverPlayer.gameObject.transform.position = exitPoint.position;
        }
        else
        {
            // 하차 지점이 지정 안 되어있으면 트럭 살짝 옆에 배치
            driverPlayer.gameObject.transform.position = transform.position + (transform.right * 2f);
        }

        driverPlayer.gameObject.transform.GetChild(0).gameObject.SetActive(true); // 플레이어 카메라 다시 켜기
        driverPlayer.enabled = true; // 플레이어 스크립트 재활성화

        // 2. 트럭 카메라 및 본인 컴포넌트 끄기
        if (truckCamera != null) truckCamera.gameObject.SetActive(false);
        
        driverPlayer = null;
        this.enabled = false; // 더 이상 Update가 돌지 않도록 끔
    }
}