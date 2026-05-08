using UnityEngine;

/// <summary>
/// Attach to each Fuse_A / Fuse_B / Fuse_C GameObject placed around the scene.
///
/// SETUP PER FUSE:
///   1. Add this script to the fuse GameObject
///   2. Set its Layer to "Interactable"
///   3. Make sure it has a Collider (MeshCollider ticked Convex, or a BoxCollider)
///   4. Set fuseID  →  "fuse_A"  (each fuse gets a unique ID)
///   5. Set displayName  →  "Fuse A"
///   6. Assign itemIcon  →  drag a Sprite so it shows in the hotbar
/// </summary>
public class Fuse : MonoBehaviour, IInteractable
{
    [Header("Fuse Identity")]
    [Tooltip("Must match FuseSlot.requiredFuseID exactly. e.g. fuse_A / fuse_B / fuse_C")]
    public string fuseID = "fuse_A";

    [Header("Display")]
    public string displayName = "Fuse A";
    public Sprite itemIcon;
    [SerializeField] private string pickupPrompt = "Pick Up";

    // ── IInteractable ─────────────────────────────────────────────────────────

    public string GetInteractionPrompt() => $"{pickupPrompt} {displayName}";

    public void Interact(InteractionSystem interactor)
    {
        bool added = InventoryManager.Instance.AddItem(displayName, itemIcon, gameObject);
        if (added)
        {
            gameObject.SetActive(false);
            Debug.Log($"[Fuse] Picked up: {displayName} ({fuseID})");
        }
        else
        {
            Debug.Log("[Fuse] Inventory full — cannot pick up.");
        }
    }
}
