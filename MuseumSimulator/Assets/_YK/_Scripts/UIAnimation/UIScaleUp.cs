using UnityEngine;
using System.Collections;

public class UIScaleUp : MonoBehaviour
{
    [Header("스케일 업 세팅")]
    public float duration = 0.25f; // 커지는 데 걸리는 시간

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        
        transform.localScale = Vector3.zero; // 크기 0에서 시작
        scaleCoroutine = StartCoroutine(AnimateScaleUp());
    }

    private IEnumerator AnimateScaleUp()
    {
        float elapsedTime = 0f;
        Vector3 startScale = Vector3.zero;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float percent = elapsedTime / duration;

            // 살짝 커졌다가 슥 줄어드는 오버슈트(Overshoot) 효과 가미
            // 0에서 1로 갈 때 부드럽게 넘어가도록 SmoothStep 처리
            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

            transform.localScale = Vector3.Lerp(startScale, originalScale, smoothPercent);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}