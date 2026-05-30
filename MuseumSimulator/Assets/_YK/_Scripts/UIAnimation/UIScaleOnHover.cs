using UnityEngine;
using UnityEngine.EventSystems; // 💡 유니티 UI 마우스 이벤트를 받기 위해 필수!
using System.Collections;

public class UIScaleOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("스케일 세팅")]
    [Tooltip("마우스가 올라갔을 때 확대될 비율입니다. (1.1 이면 원래 크기의 110%)")]
    public float targetScaleMultiplier = 1.15f;
    
    [Tooltip("크기가 변하는 데 걸리는 시간입니다. (낮을수록 빠르게 반응)")]
    public float animationDuration = 0.15f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Coroutine scaleCoroutine;

    void Awake()
    {
        // 💡 게임 시작 시 UI의 원래 크기(Transform 스케일)를 기억해 둡니다.
        originalScale = transform.localScale;
        targetScale = originalScale * targetScaleMultiplier;
    }

    // ⭐ [이벤트 1] 마우스 포인터가 UI 영역 안으로 들어왔을 때 자동으로 호출됨
    public void OnPointerEnter(PointerEventData eventData)
    {
        StopCurrentAnimation();
        // 목표 크기(targetScale)로 부드럽게 변경 시작
        scaleCoroutine = StartCoroutine(AnimateScale(targetScale));
    }

    // ⭐ [이벤트 2] 마우스 포인터가 UI 영역 밖으로 나갔을 때 자동으로 호출됨
    public void OnPointerExit(PointerEventData eventData)
    {
        StopCurrentAnimation();
        // 원래 크기(originalScale)로 부드럽게 복구 시작
        scaleCoroutine = StartCoroutine(AnimateScale(originalScale));
    }

    // UI가 중간에 꺼지거나 파괴될 때 코루틴이 꼬이는 것을 방지
    void OnDisable()
    {
        StopCurrentAnimation();
        transform.localScale = originalScale; // 강제 원상복구
    }

    // 부드러운 크기 변화를 만들어주는 보간(Lerp) 코루틴 연산
    private IEnumerator AnimateScale(Vector3 target)
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // 💡 게임이 일시정지(Time.timeScale = 0)되어도 UI는 작동하게 처리
            float percent = elapsedTime / animationDuration;
            
            // 부드러운 곡선(SmoothStep)을 적용하여 연출을 더 찰지게 만듭니다.
            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);
            
            transform.localScale = Vector3.Lerp(startScale, target, smoothPercent);
            yield return null;
        }

        transform.localScale = target;
    }

    private void StopCurrentAnimation()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }
    }
}