using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour, IInteractable
{
    [Header("회전 설정")]
    public float openAngle = 75f;    // 열렸을 때의 Y 각도
    public float closedAngle = 180f; // 닫혔을 때의 Y 각도
    public float rotationSpeed = 150f; // 초당 회전 속도
    public float waitTime = 3f;      // 열려있는 시간

    private bool isOperating = false;

    public void Interact()
    {
        // 이미 작동 중이면 중복 실행 방지
        if (isOperating) return;

        Debug.Log("문과 상호작용: 회전을 시작합니다.");
        StartCoroutine(RotateDoorRoutine());
    }

    IEnumerator RotateDoorRoutine()
    {
        isOperating = true;

        // 1. 문 열기 (75도로 회전)
        yield return StartCoroutine(RotateTo(openAngle));

        // 2. 3초 대기
        yield return new WaitForSeconds(waitTime);

        // 3. 문 닫기 (180도로 회전)
        yield return StartCoroutine(RotateTo(closedAngle));

        isOperating = false;
    }

    IEnumerator RotateTo(float targetYAngle)
    {
        // 목표 회전값을 Quaternion으로 변환
        Quaternion targetRotation = Quaternion.Euler(0, targetYAngle, 0);

        // 현재 회전값과 목표 회전값의 차이가 거의 없을 때까지 반복
        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
            yield return null;
        }

        // 마지막 각도를 정확하게 고정
        transform.localRotation = targetRotation;
    }
}