using UnityEngine;

public class TrashCan : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
        if (player == null) return;
        if (!player.IsHoldingItem)
        {
            return;
        }
        FoodData wastedData = player.CurrentHeldData;
        if (wastedData != null)
        {
            string wastedItemName = wastedData.foodName;
            player.ClearHand();
        }
    }
}