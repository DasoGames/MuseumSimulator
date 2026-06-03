using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Fridge : MonoBehaviour, IInteractable
{
    [Header("UI 설정 (프리팹일 때는 비워두셔도 됩니다)")]
    public Canvas fridgeCanvas;

    [Header("실시간 수색 설정")]
    [Tooltip("씬에 미리 깔아둔 냉장고 Canvas 오브젝트의 정확한 이름을 적어주세요.")]
    public string targetCanvasName = "FridgeCanvas";

    private bool isUsing = false;
    private FirstPersonController playerController;

    void Start()
    {
        // 💡 [핵심 추가] 프리팹이라서 인스펙터 매핑이 끊겨 있다면, 씬에서 직접 찾아옵니다.
        if (fridgeCanvas == null)
        {
            // 1. 이름으로 찾기 방식
            GameObject canvasObj = GameObject.Find(targetCanvasName);
            if (canvasObj != null)
            {
                fridgeCanvas = canvasObj.GetComponent<Canvas>();
            }

            // 2. 만약 이름으로 못 찾았다면 태그로 찾는 보험용 예외 처리
            if (fridgeCanvas == null)
            {
                GameObject taggedCanvas = GameObject.FindWithTag("FridgeUI");
                if (taggedCanvas != null)
                {
                    fridgeCanvas = taggedCanvas.GetComponent<Canvas>();
                }
            }
        }

        // 💡 냉장고가 스폰될 때는 당연히 UI 화면을 꺼둡니다.
        if (fridgeCanvas != null)
        {
            fridgeCanvas.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError($"Fridge: 씬에서 '{targetCanvasName}' 이름이나 'FridgeUI' 태그를 가진 캔버스를 찾을 수 없습니다!");
        }
    }

    public void Interact()
    {
        if (isUsing) return;

        playerController = FindFirstObjectByType<FirstPersonController>();
        if (playerController != null && fridgeCanvas != null)
        {
            EnterFridgeMode();
        }
    }

    private void EnterFridgeMode()
    {
        isUsing = true;
        
        playerController.enabled = false; 
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        fridgeCanvas.gameObject.SetActive(true);
        
        Debug.Log("냉장고: 그 자리에서 시선을 고정하고 냉장고 UI를 열었습니다.");
    }

    void Update()
    {
        if (isUsing)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitFridgeMode();
            }
        }
    }

    private void ExitFridgeMode()
    {
        if (fridgeCanvas != null)
        {
            fridgeCanvas.gameObject.SetActive(false);
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isUsing = false;
        Debug.Log("냉장고: UI를 닫고 다시 움직일 수 있습니다.");
    }
}