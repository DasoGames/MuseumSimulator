using UnityEngine;

public class MuseumCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform camTransform;

    [Header("Orbit (Middle Click)")]
    public float xSpeed = 200.0f;
    public float ySpeed = 100.0f;
    public float yMinLimit = 20f;
    public float yMaxLimit = 80f;
    [Range(0.01f, 1.0f)]
    public float smoothSpeed = 0.15f;

    [Header("Movement (Right Click)")]
    public float dragSensitivity = 1.2f; 
    public Vector2 minPos = new Vector2(-100, -100);
    public Vector2 maxPos = new Vector2(100, 100);

    [Header("Zoom")]
    public float zoomSpeed = 20.0f;
    public float minDistance = 5.0f;
    public float maxDistance = 50.0f;

    private float x, y, distance;
    private float targetX, targetY, targetDistance;
    private Vector3 targetPivotPos;

    void Start()
    {
        if (camTransform == null) camTransform = Camera.main.transform;
        
        Vector3 angles = camTransform.eulerAngles;
        targetX = x = angles.y;
        targetY = y = angles.x;
        targetDistance = distance = 25.0f;
        targetPivotPos = transform.position;
    }

    void Update()
    {
        HandleInput();
    }

    void LateUpdate()
    {
        x = Mathf.LerpAngle(x, targetX, smoothSpeed);
        y = Mathf.Lerp(y, targetY, smoothSpeed);
        distance = Mathf.Lerp(distance, targetDistance, smoothSpeed);

        transform.position = Vector3.Lerp(transform.position, targetPivotPos, smoothSpeed * 2f);

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = rotation * new Vector3(0, 0, -distance) + transform.position;

        camTransform.rotation = rotation;
        camTransform.position = position;
    }

    void HandleInput()
    {
        if (Input.GetMouseButton(2))
        {
            targetX += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            targetY -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            targetY = Mathf.Clamp(targetY, yMinLimit, yMaxLimit);
        }
        else if (Input.GetMouseButton(1))
        {
            Vector3 camForward = camTransform.forward; camForward.y = 0; camForward.Normalize();
            Vector3 camRight = camTransform.right; camRight.y = 0; camRight.Normalize();

            float moveFactor = dragSensitivity * (distance * 0.05f);
            Vector3 moveDelta = (camRight * -Input.GetAxis("Mouse X") * moveFactor) + 
                                (camForward * -Input.GetAxis("Mouse Y") * moveFactor);
            
            Vector3 nextPos = targetPivotPos + moveDelta;

            nextPos.x = Mathf.Clamp(nextPos.x, minPos.x, maxPos.x);
            nextPos.z = Mathf.Clamp(nextPos.z, minPos.y, maxPos.y);
            nextPos.y = 0;

            targetPivotPos = nextPos;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetDistance -= scroll * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
    }
}