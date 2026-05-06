using Unity.VisualScripting;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public string CurrentItem;

    public void SetItem(string currentItem)
    {
        CurrentItem = currentItem;
    }
}
