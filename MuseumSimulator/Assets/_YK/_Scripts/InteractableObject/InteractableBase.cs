using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractableObject
{
    [Header("Interaction Settings")]
    public string requiredItem = "Hand";
    public bool isHoldType = false;
    public float holdRequiredTime = 1.0f;
    
    protected float currentHoldTime = 0f;

    public virtual bool CanInteract(string currentItem) => currentItem == requiredItem;

    public virtual void OnInteractStart()
    {
        if (!isHoldType) PerformAction();
    }

    public virtual void OnInteracting(float deltaTime)
    {
        if (!isHoldType) return;

        currentHoldTime += deltaTime;
        if (currentHoldTime >= holdRequiredTime)
        {
            PerformAction();
            currentHoldTime = 0f; // 실행 후 초기화
        }
    }

    public virtual void OnInteractCancel() => currentHoldTime = 0f;

    public abstract void PerformAction(); // 실제 동작은 자식에서 구현
    public abstract string GetInteractText();
}