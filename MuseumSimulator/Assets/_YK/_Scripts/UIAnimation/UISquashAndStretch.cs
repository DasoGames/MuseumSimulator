using UnityEngine;
using System.Collections;

public class UISquashAndStretch : MonoBehaviour
{
    [Header("찌부 세팅")]
    public float duration = 0.35f;
    
    [Header("시작할 때 찌부될 형태")]
    public Vector3 startingSquashScale = new Vector3(1.5f, 0.2f, 1f); // 가로로 넓고 세로로 엄청 납작한 상태

    private Vector3 originalScale;
    private Coroutine squashCoroutine;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        if (squashCoroutine != null) StopCoroutine(squashCoroutine);
        
        transform.localScale = startingSquashScale; // 완전히 찌부된 상태에서 시작
        squashCoroutine = StartCoroutine(AnimateSquash());
    }

    private IEnumerator AnimateSquash()
    {
        float elapsedTime = 0f;
        Vector3 startScale = startingSquashScale;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float percent = elapsedTime / duration;

            // 💡 팅기는 맛을 주기 위해 탄성 곡선 연산(Elastic 이펙트 수학 공식 유사 적용)
            // 1을 살짝 넘겼다가 자리를 잡는 찰진 느낌을 줍니다.
            float sinBounce = Mathf.Sin(percent * Mathf.PI * 1.5f) * (1f - percent);
            float t = Mathf.SmoothStep(0f, 1f, percent) + sinBounce * 0.2f;

            transform.localScale = Vector3.Lerp(startScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale; // 정확하게 원래 크기로 마감
    }
}