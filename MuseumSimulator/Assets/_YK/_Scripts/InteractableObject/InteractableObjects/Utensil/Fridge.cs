using UnityEngine;

public class Fridge : MonoBehaviour, IInteractable
{
    public Canvas Fridgecanvas;
    public void Interact()
    {
        Fridgecanvas.gameObject.SetActive(true);
    }
}
