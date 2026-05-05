using UnityEngine;
using System.Collections;

public class LockedDrawer : MonoBehaviour, IInteractable
{
    [Header("Key Settings")]
    [SerializeField] private string requiredKeyName = "Key";
    [SerializeField] private bool consumeKey = true;

    [Header("Drawer Animation")]
    [SerializeField] private Transform drawerTransform;     // the part that slides
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 0f, 0.5f);
    [SerializeField] private float openDuration = 0.6f;

    [Header("Prompts")]
    [SerializeField] private string lockedPrompt = "Locked - Requires Key";
    [SerializeField] private string unlockPrompt = "Unlock with Key";
    [SerializeField] private string openedPrompt = "Opened";

    private bool isUnlocked = false;
    private bool isAnimating = false;
    private Vector3 closedLocalPos;

    private void Awake()
    {
        if (drawerTransform == null) drawerTransform = transform;
        closedLocalPos = drawerTransform.localPosition;
    }

    public string GetInteractionPrompt()
    {
        if (isUnlocked) return openedPrompt;

        // Show different prompt depending on whether player has the key
        if (InventoryManager.Instance != null &&
            InventoryManager.Instance.HasItem(requiredKeyName))
            return unlockPrompt;

        return lockedPrompt;
    }

    public void Interact(InteractionSystem interactor)
    {
        if (isUnlocked || isAnimating) return;

        if (InventoryManager.Instance == null) return;

        if (!InventoryManager.Instance.HasItem(requiredKeyName))
        {
            Debug.Log("Drawer is locked. You need the " + requiredKeyName + ".");
            return;
        }

        // Has the key — unlock and open
        if (consumeKey)
            InventoryManager.Instance.RemoveItem(requiredKeyName);

        isUnlocked = true;
        StartCoroutine(OpenDrawer());
        Debug.Log("Drawer unlocked with " + requiredKeyName);
    }

    private IEnumerator OpenDrawer()
    {
        isAnimating = true;
        Vector3 startPos = drawerTransform.localPosition;
        Vector3 targetPos = closedLocalPos + openOffset;

        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);
            drawerTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        drawerTransform.localPosition = targetPos;
        isAnimating = false;

        Collider drawerCollider = GetComponent<Collider>();
        if (drawerCollider != null)
            drawerCollider.enabled = false;
    }
}