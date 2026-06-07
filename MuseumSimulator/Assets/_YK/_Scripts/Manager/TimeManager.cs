using UnityEngine;
using TMPro; // 💡 TextMeshPro 라이브러리 사용을 위해 필수 포함

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("⏰ 영업 시간 설정 (24시간계 형식)")]
    [Tooltip("영업을 시작할 시간 (시)")]
    public int startHour = 8; 
    [Tooltip("영업을 마감할 시간 (시)")]
    public int endHour = 22; 

    [Header("⏱️ 시간 속도 조절")]
    [Tooltip("현실 시간 1초당 인게임 시간 몇 분이 흐르게 할 것인가?")]
    public float timeSpeed = 2f; 

    [Header("⭐ 시계 UI 설정")]
    [Tooltip("화면에 현재 시간을 표시할 TextMeshPro - Text UI 컴포넌트를 연결하세요.")]
    public TMP_Text clockText; // 💡 이 부분의 타입 누락 오타가 정상 수정되었습니다!

    // 현재 인게임 누적 환산 시간 (분 단위)
    private float currentInGameMinutes = 0f;
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 💡 씬 전환 시 파괴 방지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 시작 시간을 분 단위로 환산하여 타이머 초기화 (ex: 8시 -> 480분)
        currentInGameMinutes = startHour * 60;
        isGameOver = false;
        
        UpdateClockUI();
    }

    private void Update()
    {
        if (isGameOver) return;

        // 현실 시간(Time.deltaTime) * 흐름 속도(Speed) 만큼 인게임 '분' 누적
        currentInGameMinutes += Time.deltaTime * timeSpeed;

        // UI 시계 텍스트 업데이트
        UpdateClockUI();

        // [마감 체크] 현재 시간이 마감 시간에 도달했는지 실시간 체크
        int currentHour = Mathf.FloorToInt(currentInGameMinutes / 60f);
        if (currentHour >= endHour)
        {
            TriggerClosingTime();
        }
    }

    /// <summary>
    /// ⭐ 인게임 시간을 강제로 추가(더하기)해 주는 메서드
    /// </summary>
    /// <param name="minutesToAdd">추가하고 싶은 '분' 단위 시간 (예: 30 입력 시 30분 추가)</param>
    public void AddInGameTime(float minutesToAdd)
    {
        if (minutesToAdd <= 0) return;

        // 1. 시간을 강제로 더해줍니다.
        currentInGameMinutes += minutesToAdd;

        // 2. 만약 영업 마감 상태였는데 마감 시간을 늘려주는 특수 아이템 성격이라면 게임오버를 일시 해제합니다.
        int currentHour = Mathf.FloorToInt(currentInGameMinutes / 60f);
        if (isGameOver && currentHour < endHour)
        {
            isGameOver = false;
            Debug.Log("<color=lime>[TimeManager] 영업 시간이 연장되어 타이머가 재가동됩니다!</color>");
        }

        // 3. 수치가 바뀌었으므로 즉시 UI 시계를 새로고침합니다.
        UpdateClockUI();
        Debug.Log($"[TimeManager] 시간이 강제로 추가되었습니다: +{minutesToAdd}분");
    }

    /// <summary>
    /// 계산된 누적 분을 시/분 포맷 및 오전/오후로 이쁘게 가공해 UI에 매칭하는 메서드
    /// </summary>
    private void UpdateClockUI()
    {
        if (clockText == null) return;

        // 전체 분을 시와 분으로 분리
        int hours = Mathf.FloorToInt(currentInGameMinutes / 60f) % 24;
        int minutes = Mathf.FloorToInt(currentInGameMinutes % 60f);

        // 타이쿤 맛을 살리기 위한 오전/오후(AM/PM) 및 12시간계 가공 연산
        string ampm = (hours >= 12) ? "오후" : "오전";
        int displayHour = hours % 12;
        if (displayHour == 0) displayHour = 12; // 0시는 12시로 보정

        // 결과물 포맷: "오후 02:30" 형태
        clockText.text = $"{ampm} {displayHour:D2}:{minutes:D2}";
    }

    /// <summary>
    /// 영업 마감 시간이 되었을 때 단 한 번 실행될 마감 시스템 함수
    /// </summary>
    private void TriggerClosingTime()
    {
        isGameOver = true;
        
        if (clockText != null)
        {
            clockText.text = "<color=red>영업 마감</color>";
        }

        Debug.Log("<color=red>🚨 [TimeManager] 마감 시간 도달! 오늘의 푸드트럭 영업이 완전히 마감되었습니다.</color>");
    }

    // --- 다른 클래스에서 유용하게 쓸 수 있는 Getter 헬퍼 메서드들 ---

    public int GetCurrentHour()
    {
        return Mathf.FloorToInt(currentInGameMinutes / 60f) % 24;
    }

    public int GetCurrentMinute()
    {
        return Mathf.FloorToInt(currentInGameMinutes % 60f);
    }

    public void ResetNextDay()
    {
        currentInGameMinutes = startHour * 60;
        isGameOver = false;
        UpdateClockUI();
        Debug.Log($"☀️ [TimeManager] 새 아침이 밝았습니다. {startHour}시 정각 영업 재개!");
    }
}