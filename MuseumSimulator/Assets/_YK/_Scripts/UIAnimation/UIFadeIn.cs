using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class UIFadeIn : MonoBehaviour
{
    [Header("페이드 세팅")]
    public float duration = 0.4f; // 나타나는 시간

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // 💡 오브젝트가 활성화(SetActive(true))될 때마다 자동으로 실행됩니다.
    void OnEnable()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        
        canvasGroup.alpha = 0f; // 0(투명)에서 시작
        fadeCoroutine = StartCoroutine(AnimateFadeIn());
    }

    private IEnumerator AnimateFadeIn()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float percent = elapsedTime / duration;
            
            // 부드러운 가감속 곡선 적용
            canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, percent);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }
}