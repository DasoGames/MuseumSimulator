using UnityEngine;

public class EnableMenu : MonoBehaviour
{
    public FirstPersonLook firstPersonLook;
    public GameObject MenuPanel;
    public KeyCode keycode = KeyCode.Escape;

    void Update()
    {
        if (Input.GetKeyDown(keycode))
        {
            bool isActive = MenuPanel.activeSelf;

            if (isActive)
            {
                MenuPanel.SetActive(false);
                if (firstPersonLook != null) firstPersonLook.enabled = true;
                
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                MenuPanel.SetActive(true);
                if (firstPersonLook != null) firstPersonLook.enabled = false;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
    public void DisableMenu()
    {
        MenuPanel.SetActive(false);
        if (firstPersonLook != null) firstPersonLook.enabled = true;
                
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}