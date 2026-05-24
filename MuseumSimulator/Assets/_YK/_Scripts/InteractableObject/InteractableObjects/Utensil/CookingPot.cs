using UnityEngine;
using System.Collections.Generic;

public class CookingPot : MonoBehaviour, IInteractable
{
    [Header("냄비 고유 설정")]
    [Tooltip("UpgradeManager에 등록한 고유 식별 이름")]
    public string machineID = "CookingPot";

    // 💡 재료별 시간 다 지우고, 이 냄비 고유의 기본 시간들만 남겼습니다!
    public float baseCookTime = 10f;     // 떡볶이 조리에 걸리는 기본 시간 (초)
    public float baseBurnTime = 8f;      // 완성 후 타버리기까지의 기본 방치 시간 (초)

    [Header("결과물 데이터")]
    public HoldableObject cookedTteokbokkiData; // 이름: "완성된떡볶이"
    public HoldableObject burntTteokbokkiData;  // 이름: "탄떡볶이"

    [Header("냄비 내부 재료 배치 위치 (선택사항)")]
    public Transform foodVisualParent; 

    // 떡볶이에 필요한 3가지 고정 재료 이름 목록
    private readonly List<string> requiredIngredients = new List<string> { "떡볶이떡", "양념", "손질된야채" };
    
    [Header("현재 냄비에 들어간 재료 목록 (Debug)")]
    [SerializeField] private List<string> currentIngredients = new List<string>();

    // 냄비의 상태를 제어하기 위한 변수들
    private bool isReadyToCook = false; 
    private bool isCooking = false;     
    private bool isDone = false;        

    private float timer = 0f;
    private float targetCookTime = 0f;  // 업그레이드가 반영된 최종 조리 목표 시간

    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;

        // [상황 1] 요리가 완성되었거나 타버린 상태 -> 플레이어가 빈손일 때 수거해감
        if (isDone)
        {
            if (player.IsHoldingItem)
            {
                Debug.LogWarning("냄비: 완성된 요리를 꺼내려면 손을 비워야 합니다!");
                return;
            }

            if (timer >= baseBurnTime)
            {
                player.HoldNewData(burntTteokbokkiData);
                Debug.Log("냄비: 너무 오래 방치되어 새까맣게 탄 떡볶이를 꺼냈습니다.");
            }
            else
            {
                player.HoldNewData(cookedTteokbokkiData);
                Debug.Log("냄비: 맛있게 조리된 완성된 떡볶이를 꺼냈습니다!");
            }

            ResetPot();
            return;
        }

        // [상황 2] 3가지 재료가 다 모여서 조리가능상태일 때 -> 빈손으로 누르면 조리 시작!
        if (isReadyToCook && !isCooking)
        {
            if (player.IsHoldingItem)
            {
                Debug.LogWarning("냄비: 조리를 시작하려면 손을 비우고 상호작용하세요!");
                return;
            }

            StartCooking();
            return;
        }

        // [상황 3] 아직 재료를 모으는 중일 때 -> 손에 든 재료를 냄비에 투입
        if (!isReadyToCook && !isCooking && !isDone)
        {
            if (!player.IsHoldingItem)
            {
                Debug.Log("냄비: 아직 재료가 부족합니다. 떡볶이떡, 양념, 손질된야채를 채워주세요.");
                return;
            }

            HoldableObject heldData = player.CurrentHeldData.Value;

            // 레시피에 포함되어 있고, 중복 투입이 아니라면 허용
            if (requiredIngredients.Contains(heldData.objectName) && !currentIngredients.Contains(heldData.objectName))
            {
                currentIngredients.Add(heldData.objectName);
                Debug.Log($"냄비: '{heldData.objectName}' 투입 완료! ({currentIngredients.Count} / {requiredIngredients.Count})");
                
                player.ClearHand();

                // 재료 3가지가 전부 모였다면 조리가능상태로 전환
                if (currentIngredients.Count == requiredIngredients.Count)
                {
                    isReadyToCook = true;
                    Debug.Log("냄비: 모든 재료가 모여 [조리 가능 상태]가 되었습니다! 빈손으로 E키를 눌러 불을 켜세요.");
                }
            }
            else
            {
                Debug.LogWarning("냄비: 이 재료는 떡볶이에 들어가지 않거나 이미 넣은 재료입니다.");
            }
        }
    }

    // 본격적인 조리를 시작하는 내부 함수
    private void StartCooking()
    {
        isReadyToCook = false;
        isCooking = true;
        timer = 0f;

        // 💡 [독립적 업그레이드 연동] 
        // 재료별 시간을 더하는 복잡한 수식 없이, 통째로 baseCookTime에 단축 비율만 곱해줍니다!
        float speedModifier = 1f;
        if (UpgradeManager.Instance != null)
        {
            speedModifier = UpgradeManager.Instance.GetSpeedModifier(machineID);
        }
        targetCookTime = baseCookTime * speedModifier;

        Debug.Log($"냄비: 불을 켭니다! 조리 시작 (업그레이드 반영 최종 시간: {targetCookTime:F1}초)");
    }

    void Update()
    {
        // 불 켜고 조리 중일 때
        if (isCooking)
        {
            timer += Time.deltaTime;
            if (timer >= targetCookTime)
            {
                isCooking = false;
                isDone = true;
                timer = 0f; // 탄 타이머용으로 리셋
                Debug.Log("냄비: 떡볶이 조리 완료! 진열대로 가져갈 수 있습니다.");
            }
        }

        // 조리가 끝나서 방치되고 있을 때 (시간 초과 폐기 프로세스)
        if (isDone)
        {
            timer += Time.deltaTime;
        }
    }

    private void ResetPot()
    {
        currentIngredients.Clear();
        isReadyToCook = false;
        isCooking = false;
        isDone = false;
        timer = 0f;
        targetCookTime = 0f;
    }
}