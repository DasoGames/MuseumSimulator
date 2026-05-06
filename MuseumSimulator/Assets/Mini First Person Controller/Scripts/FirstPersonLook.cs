using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField] Transform character;
    public float sensitivity = 2;
    public float smoothing = 1.5f;

    Vector2 velocity;
    Vector2 frameVelocity;

    // 외부에서 제어할 수 있는 변수 추가
    public bool isPaused = false;

    void Reset()
    {
        character = GetComponentInParent<FirstPersonMovement>().transform;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        // 시작할 때 현재 오브젝트의 회전값을 velocity에 동기화 (값이 튀는 것 방지)
        velocity.y = -transform.localEulerAngles.x;
        velocity.x = character.localEulerAngles.y;
    }

    void Update()
    {
        // 일시정지 상태라면 아래 로직을 아예 실행하지 않음
        if (isPaused) return;

        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);

        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }

    // 메뉴를 끄고 켤 때 호출할 함수
    public void SetPause(bool pause)
    {
        isPaused = pause;
        if (isPaused)
        {
            // 멈출 때 남은 속도를 즉시 제거
            frameVelocity = Vector2.zero;
        }
    }
}