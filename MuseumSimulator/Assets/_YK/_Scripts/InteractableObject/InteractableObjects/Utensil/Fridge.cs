using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Fridge : MonoBehaviour, IInteractable
{
    [Header("UI 설정")]
    public Canvas fridgeCanvas;

    private bool isUsing = false;
    private FirstPersonController playerController;

    public void Interact()
    {
        // 이미 냉장고 UI를 열어둔 상태라면 중복 실행 방지
        if (isUsing) return;

        playerController = FindFirstObjectByType<FirstPersonController>();
        if (playerController != null)
        {
            EnterFridgeMode();
        }
    }

    private void EnterFridgeMode()
    {
        isUsing = true;
        
        // 1. 플레이어 이동 및 마우스 시점 회전만 그 자리에서 정지
        playerController.enabled = false; 
        
        // 2. 냉장고 식재료를 클릭할 수 있도록 마우스 커서 활성화
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. 냉장고 UI 화면 표시
        fridgeCanvas.gameObject.SetActive(true);
        
        Debug.Log("냉장고: 그 자리에서 시선을 고정하고 냉장고 UI를 열었습니다.");
    }

    void Update()
    {
        if (isUsing)
        {
            // 냉장고를 보다가 ESC 키를 누르면 원래 플레이 상태로 복귀
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitFridgeMode();
            }
        }
    }

    private void ExitFridgeMode()
    {
        // 1. 냉장고 UI 끄기
        fridgeCanvas.gameObject.SetActive(false);

        // 2. 플레이어 조작 및 시점 회전 다시 허용
        playerController.enabled = true;

        // 3. 다시 인게임 조작을 위해 마우스 커서 숨기고 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isUsing = false;
        Debug.Log("냉장고: UI를 닫고 다시 움직일 수 있습니다.");
    }
}