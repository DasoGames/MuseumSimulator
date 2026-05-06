using UnityEngine;
public class Dirt : InteractableBase
{
    private void Awake()
    {
        isHoldType = true;
        holdRequiredTime = 2.0f;
        requiredItem = "Broom";
    }

    public override void PerformAction()
    {
        Debug.Log("청소 완료!");
        Destroy(gameObject);
    }

    public override string GetInteractText() => "청소하기 (E 꾹 누르기)";
}