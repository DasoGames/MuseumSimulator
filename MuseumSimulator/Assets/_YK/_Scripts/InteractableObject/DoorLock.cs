using UnityEngine;
using System.Collections;

public class DoorLock : MonoBehaviour
{
    [Header("설정")]
    public Transform doorTransform; // 움직일 문 오브젝트
    public float moveSpeed = 2f;    // 움직이는 속도
    public float waitTime = 5f;     // 문이 열려있는 시간
    
    // 특정 태그를 가진 객체(Player)만 문을 열게 하고 싶을 때 사용
    public string playerTag = "Player";

    private Vector3 closedPos;      
    private Vector3 openedPos;      
    private bool isOperating = false;

    void Start()
    {
        if (doorTransform != null)
        {
            closedPos = new Vector3(doorTransform.localPosition.x, doorTransform.localPosition.y, -2.022f);
            openedPos = new Vector3(doorTransform.localPosition.x, doorTransform.localPosition.y, -3.249f);
            doorTransform.localPosition = closedPos;
        }
    }

    // 플레이어가 콜라이더 영역 안에 들어오면 호출됨
    private void OnTriggerEnter(Collider other)
    {
        // 1. 이미 작동 중이 아니고
        // 2. 들어온 물체의 태그가 "Player"일 때 실행
        if (!isOperating && other.CompareTag(playerTag))
        {
            Debug.Log("플레이어 감지: 문을 엽니다.");
            StartCoroutine(DoorRoutine());
        }
    }

    IEnumerator DoorRoutine()
    {
        isOperating = true;

        // 1. 문 열기
        yield return StartCoroutine(MoveDoor(openedPos));

        // 2. 설정한 시간만큼 대기
        yield return new WaitForSeconds(waitTime);

        // 3. 문 닫기
        yield return StartCoroutine(MoveDoor(closedPos));

        isOperating = false;
    }

    IEnumerator MoveDoor(Vector3 targetPos)
    {
        while (Vector3.Distance(doorTransform.localPosition, targetPos) > 0.001f)
        {
            doorTransform.localPosition = Vector3.MoveTowards(
                doorTransform.localPosition, 
                targetPos, 
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
        doorTransform.localPosition = targetPos;
    }
}