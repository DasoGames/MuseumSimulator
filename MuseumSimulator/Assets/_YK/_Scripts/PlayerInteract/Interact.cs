using UnityEngine;

public class Interact : MonoBehaviour
{
    [SerializeField] private float interactDistance = 3.0f;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private PlayerState playerState;

    private IInteractableObject currentTarget;

    void Update()
    {
        IInteractableObject newTarget = GetInteractableTarget();

        if (newTarget != currentTarget)
        {
            currentTarget?.OnInteractCancel();
            currentTarget = newTarget;
        }

        if (currentTarget == null) return;

        if (!currentTarget.CanInteract(playerState.CurrentItem)) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentTarget.OnInteractStart();
        }
        
        if (Input.GetKey(KeyCode.E))
        {
            currentTarget.OnInteracting(Time.deltaTime);
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            currentTarget.OnInteractCancel();
        }
    }

    private IInteractableObject GetInteractableTarget()
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            return hit.collider.GetComponent<IInteractableObject>();
        }
        return null;
    }
}