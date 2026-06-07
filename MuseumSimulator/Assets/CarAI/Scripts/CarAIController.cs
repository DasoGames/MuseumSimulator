using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CarAIController : MonoBehaviour
{
    [HideInInspector] public Transform frontRight, frontLeft, rearRight, rearLeft;
    [HideInInspector] public WheelCollider frontRightCollider, frontLeftCollider, rearRightCollider, rearLeftCollider;

    [Header("🏎️ 아케이드 주행 및 조향 설정")]
    public float moveSpeed = 15f;       
    public float rotationSpeed = 12f;   

    [Header("Checkpoints And Detections")]
    public Transform nextCheckpoint;
    public List<Transform> checks = new List<Transform> { null };
    public bool CheckPointSearch = true;
    public bool objectDetected = false;
    public bool isCarControlledByAI = true;
    public LayerMask seenLayers = Physics.AllLayers;

    [Header("Car Settings")]
    public int kmh;
    public int speedLimit = 40;
    [Tooltip("💡 이 수치가 곧 레이저 센서의 총 사거리(m)가 됩니다.")]
    public float distanceFromObjects = 6f; 
    public int recklessnessThreshold = 0;
    public bool despawnForFlippingOver = true;
    public bool taxiMode = false;

    public float acceleration = 300f; 
    public float breaking = 4000f;   

    private float currentSpeed = 0f;
    private bool flipOverCheck = false;
    private Stopwatch stopwatch = new Stopwatch();
    private Vector3 lastPos;

    private void Awake()
    {
        UnityEngine.Debug.Log($"<color=#FF00FF><b>[시스템 탄생]</b> {gameObject.name} 오브젝트가 씬에 생성되어 Awake 되었습니다.</color>");
    }

    private void Start()
    {
        currentSpeed = 0f;
        if (seenLayers.value == 0) seenLayers = ~0;

        if (nextCheckpoint == null)
        {
            UnityEngine.Debug.LogError($"<color=red><b>🚨 [초기화 실패]</b> {gameObject.name}이 태어났으나 'nextCheckpoint'가 Null입니다!</color>");
        }
        else
        {
            UnityEngine.Debug.Log($"<color=white><b>🟢 [초기화 성공]</b> {gameObject.name}의 첫 목적지가 {nextCheckpoint.name}으로 확인되었습니다.</color>");
        }
    }

    private float logTimer = 0f;
    private void Update()
    {
        logTimer += Time.deltaTime;
        if (logTimer >= 1.0f) 
        {
            logTimer = 0f;
            string targetName = (nextCheckpoint != null) ? nextCheckpoint.name : "NULL (목적지 없음)";
            UnityEngine.Debug.Log($"<color=#00FFFF><b>📊 [실시간 브리핑]</b> {gameObject.name} -> 현재목적지:{targetName} | 속도:{kmh}km/h | 센서거리:{distanceFromObjects}m</color>");
        }
    }

    private void FixedUpdate()
    {
        CalculateKMH();
        SearchForCheckpoints();

        if (despawnForFlippingOver && !flipOverCheck)
        {
            flipOverCheck = true;
            StartCoroutine(CheckForFlippingOver());
        }
    }

    public void Accelerate(float value) { }
    public void Break(float value) { }
    public void Turn(float angle) { }

    public void SetSpeed(int targetSpeedLimit)
    {
        float targetSpeedInVector = targetSpeedLimit * 0.277f;

        if (currentSpeed > targetSpeedInVector)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeedInVector, Time.fixedDeltaTime * breaking * 0.1f);
        }
        else if (currentSpeed < targetSpeedInVector)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeedInVector, Time.fixedDeltaTime * acceleration * 0.1f);
        }
    }

private void SearchForCheckpoints()
{
    if (!CheckPointSearch || !isCarControlledByAI) return;

    if (nextCheckpoint == null)
    {
        UnityEngine.Debug.LogWarning($"<color=#FFFF00><b>⚠️ [주행 엔진 중단]</b> {gameObject.name}의 'nextCheckpoint'가 Null입니다.</color>");
        return;
    }

    // 1. 🧭 [대각선 고착화 해결] 로컬 좌표 기반 조향 및 선 자석 흡착 시스템
    Vector3 localTargetPos = transform.InverseTransformPoint(nextCheckpoint.position);
    localTargetPos.y = 0; 

    float distanceToPoint = localTargetPos.magnitude;

    // 기본 오차 각도 (Atan2 기반 라디안 -> 디그리 변환)
    float angleToTarget = Mathf.Atan2(localTargetPos.x, localTargetPos.z) * Mathf.Rad2Deg;

    // =================================================================
    // ✨ [핵심 치우침 보정식] 차선 이탈 가중치 주입 (Cross Track Correction)
    // 타겟이 멀리 있더라도 가로축(localTargetPos.x)으로 차가 밀려나 있으면 
    // 선 정중앙으로 복귀하도록 핸들을 더 정교하고 강하게 꺾어줍니다.
    // =================================================================
    if (distanceToPoint > 1.5f)
    {
        // 선에서 좌우로 이탈한 수치에 비례해 회전력을 보정합니다.
        float driftCorrection = localTargetPos.x * 2.5f; // 2.5f는 흡착 강도 (치우침이 심하면 3.0~4.0으로 상향)
        float finalSteerAmount = angleToTarget + driftCorrection;

        // 프레임당 회전 한계 상한선 제어
        float steerAmount = Mathf.Clamp(finalSteerAmount, -rotationSpeed * 15f, rotationSpeed * 15f);
        
        // 정직하게 지면 Y축 회전 주입
        transform.Rotate(0f, steerAmount * Time.fixedDeltaTime, 0f);
    }

    // [근접 조기 노드 자동 전환]
    if (distanceToPoint < 1.0f)
    {
        UnityEngine.Debug.Log($"<color=cyan><b>[거리 충돌]</b> {gameObject.name}이 {nextCheckpoint.name} 근접 반경 내로 진입하여 다음 노드로 강제 전환합니다.</color>");
        SwitchToNextTarget();
    }

    // 2. 🚨 아케이드 최적화 전방 레이더 검사 (maxDistance = distanceFromObjects 고정형)
    float maxDistance = distanceFromObjects; 
    RaycastHit carHit = new RaycastHit();
    int objectInFront = 0;

    for (int i = 0; i < checks.Count; i++)
    {
        if (checks[i] == null) continue;

        bool isObjectInFront = Physics.Raycast(checks[i].position, checks[i].forward, out carHit, maxDistance, seenLayers, QueryTriggerInteraction.Collide);

        if (isObjectInFront)
        {
            if (carHit.transform != transform && !carHit.transform.IsChildOf(transform))
            {
                objectInFront++;
                UnityEngine.Debug.LogWarning($"<color=orange><b>[레이더 감지]</b> {gameObject.name} 전방 장애물 확정! 대상: {carHit.transform.name} (거리: {carHit.distance:F2}m)</color>");
            }
        }
    }

    // 3. 🚦 제동 및 가속 제어
    if (objectInFront > 0)
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, Time.fixedDeltaTime * breaking * 0.5f);
        objectDetected = true;
    }
    else
    {
        objectDetected = false;
        int speed = speedLimit + recklessnessThreshold;
        if (speedLimit == 0) speed = 0;
        SetSpeed(speed);
    }

    // 4. 🚛 [최종 이동] 정돈된 내 로컬 정면 방향으로 좌표 강제 전진
    transform.Translate(Vector3.forward * currentSpeed * Time.fixedDeltaTime, Space.Self);
}
    private void SwitchToNextTarget()
    {
        if (nextCheckpoint == null) return;
        CheckpointScript script = nextCheckpoint.GetComponent<CheckpointScript>();
        
        if (script != null)
        {
            if (script.nextCheckpoints.Count > 0)
            {
                int index = Random.Range(0, script.nextCheckpoints.Count);
                Transform oldPoint = nextCheckpoint;
                nextCheckpoint = script.nextCheckpoints[index];
                
                UnityEngine.Debug.Log($"<color=green><b>🏁 [목적지 갱신 완료]</b> {gameObject.name}: {oldPoint.name} ➡️ {nextCheckpoint.name}</color>");
            }
            else
            {
                UnityEngine.Debug.LogError($"<color=red><b>❌ [도로 끊김 대참사]</b> {nextCheckpoint.name}의 다음 연결점 리스트가 완전히 비어있습니다!</color>");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (nextCheckpoint != null && other.gameObject == nextCheckpoint.gameObject)
        {
            UnityEngine.Debug.Log($"<color=yellow><b>[물리 트리거 충돌]</b> {gameObject.name}이 {other.gameObject.name} 영역 통과 완료.</color>");
            SwitchToNextTarget();
        }
    }

    private void OnDrawGizmos()
    {
        if (checks == null) return;

        // 주행 연산 루프와 완벽하게 1:1로 일치화된 사거리 기즘 드로잉
        float maxDistance = distanceFromObjects;

        for (int i = 0; i < checks.Count; i++)
        {
            if (checks[i] == null) continue;

            RaycastHit hit;
            bool isHit = Physics.Raycast(checks[i].position, checks[i].forward, out hit, maxDistance, seenLayers, QueryTriggerInteraction.Collide);
            
            if (isHit && hit.transform != transform && !hit.transform.IsChildOf(transform))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(checks[i].position, hit.point);
                Gizmos.DrawWireSphere(hit.point, 0.4f);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(checks[i].position, checks[i].position + (checks[i].forward * maxDistance));
            }
        }
    }

    private void CalculateKMH()
    {
        if (stopwatch.IsRunning)
        {
            stopwatch.Stop();
            float distance = (transform.position - lastPos).magnitude;
            float time = stopwatch.Elapsed.Milliseconds / (float)1000;
            if (time <= 0) time = 0.01f;
            kmh = (int)(3600 * distance / time / 1000);
            lastPos = transform.position;
            stopwatch.Reset();
            stopwatch.Start();
        }
        else
        {
            lastPos = transform.position;
            stopwatch.Reset();
            stopwatch.Start();
        }
    }

    IEnumerator CheckForFlippingOver()
    {
        bool deleteCar = isCarFlipedOver();
        if (deleteCar)
        {
            for (int i = 0; i < 10; i++)
            {
                if (!isCarFlipedOver()) deleteCar = false;
                yield return new WaitForSeconds(1);
            }
            if (deleteCar)
            {
                Destroy(gameObject);
            }
        }
        yield return new WaitForSeconds(10);
        flipOverCheck = false;
    }

    private bool isCarFlipedOver()
    {
        if (transform.rotation.eulerAngles.z > 30f || transform.rotation.eulerAngles.z < -30f)
            return true;
        return false;
    }
}