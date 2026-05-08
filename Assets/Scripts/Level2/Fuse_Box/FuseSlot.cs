using UnityEngine;

public class FuseSlot : MonoBehaviour, IInteractable
{
    // ── Inspector Fields ──────────────────────────────────────────────────────

    [Header("Slot Identity")]
    [Tooltip("Which fuse belongs here. Must match Fuse.fuseID exactly.")]
    public string requiredFuseID = "fuse_A";

    [Header("Indicator — material swap")]
    [Tooltip("The MeshRenderer on the FuseBox root that holds all materials.")]
    [SerializeField] private MeshRenderer fuseBoxRenderer;

    [Tooltip("Material index on that renderer that controls THIS slot's indicator light.\n" +
             "In your asset: index 1 = first light, 2 = second light, 3 = third light.\n" +
             "Check the Materials list in the FuseBox Inspector to confirm the order.")]
    [SerializeField] private int indicatorMaterialIndex = 1;

    [Tooltip("The Indicator_GreenLight material from the asset.")]
    [SerializeField] private Material matCorrect;

    [Tooltip("The Indicator_RedLight material from the asset.")]
    [SerializeField] private Material matWrong;

    [Tooltip("The Indicator_Off material from the asset.")]
    [SerializeField] private Material matOff;

    [Header("Fuse Visual — where inserted fuse snaps")]
    [Tooltip("Create an empty child Transform at the socket position. " +
             "The inserted fuse mesh will snap here so it looks seated in the box.")]
    [SerializeField] private Transform fuseAnchor;

    [Header("References")]
    [SerializeField] private FuseBoxController fuseBoxController;

    [Header("Interaction Prompts")]
    [SerializeField] private string promptInsert = "Insert Fuse";
    [SerializeField] private string promptRemove = "Remove Fuse";
    [SerializeField] private string promptNoFuse = "Requires Fuse";

    // ── Runtime State ─────────────────────────────────────────────────────────

    [HideInInspector] public bool isOccupied = false;
    [HideInInspector] public bool isCorrect = false;

    private GameObject insertedObject;
    private string insertedFuseID;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        SetIndicator(matOff);
    }

    // ── IInteractable ─────────────────────────────────────────────────────────

    public string GetInteractionPrompt()
    {
        if (isOccupied) return promptRemove;

        // Show insert prompt only if player holds any item in selected slot
        string selected = InventoryManager.Instance?.GetSelectedItem();
        return string.IsNullOrEmpty(selected) ? promptNoFuse : promptInsert;
    }

    public void Interact(InteractionSystem interactor)
    {
        if (isOccupied)
            RemoveFuse(interactor);
        else
            TryInsertFuse();
    }

    // ── Insert ────────────────────────────────────────────────────────────────

    private void TryInsertFuse()
    {
        if (InventoryManager.Instance == null) return;

        // Get the selected item's GameObject from inventory
        GameObject selectedObj = InventoryManager.Instance.GetSelectedItemObject();
        if (selectedObj == null)
        {
            Debug.Log("[FuseSlot] No item selected.");
            return;
        }

        // Must be a Fuse
        Fuse fuse = selectedObj.GetComponent<Fuse>();
        if (fuse == null)
        {
            Debug.Log("[FuseSlot] Selected item is not a Fuse.");
            return;
        }

        // Remove from inventory
        string itemName = InventoryManager.Instance.GetSelectedItem();
        InventoryManager.Instance.RemoveItem(itemName);

        // Snap fuse mesh into the socket
        selectedObj.SetActive(true);
        Transform snapTo = fuseAnchor != null ? fuseAnchor : transform;
        selectedObj.transform.SetParent(snapTo);
        selectedObj.transform.localPosition = Vector3.zero;
        selectedObj.transform.localRotation = Quaternion.identity;

        // Disable its collider so it doesn't block raycasts on the slot
        Collider col = selectedObj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Record state
        insertedObject = selectedObj;
        insertedFuseID = fuse.fuseID;
        isOccupied = true;
        isCorrect = (insertedFuseID == requiredFuseID);

        // Indicator feedback
        SetIndicator(isCorrect ? matCorrect : matWrong);

        Debug.Log($"[FuseSlot] Inserted '{insertedFuseID}' → slot wants '{requiredFuseID}' → {(isCorrect ? "CORRECT ✓" : "WRONG ✗")}");

        fuseBoxController?.OnSlotChanged();
    }

    // ── Remove ────────────────────────────────────────────────────────────────

    private void RemoveFuse(InteractionSystem interactor)
    {
        if (insertedObject == null) return;

        // Unparent first
        insertedObject.transform.SetParent(null);
        insertedObject.SetActive(false);

        // Get the fuse's display name for inventory
        Fuse fuse = insertedObject.GetComponent<Fuse>();
        string displayName = fuse != null ? fuse.fuseID : insertedFuseID;

        // Try to find the display name from the Fuse component
        // We need the actual item name that was used when adding to inventory
        // Re-add to inventory directly
        bool added = false;
        if (fuse != null)
        {
            // Get sprite from fuse component via reflection isn't ideal
            // So we just re-add with null icon — it still works functionally
            added = InventoryManager.Instance.AddItem(
                GetFuseDisplayName(insertedObject),
                GetFuseIcon(insertedObject),
                insertedObject
            );
        }

        if (!added)
        {
            // Inventory full — drop it on the ground instead
            insertedObject.SetActive(true);
            insertedObject.transform.SetParent(null);
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 drop = cam.transform.position + cam.transform.forward * 0.9f;
                drop.y -= 0.15f;
                insertedObject.transform.position = drop;
            }
            Collider col = insertedObject.GetComponent<Collider>();
            if (col != null) col.enabled = true;
            Debug.Log($"[FuseSlot] Inventory full — dropped '{insertedFuseID}' on ground.");
        }
        else
        {
            Debug.Log($"[FuseSlot] Removed '{insertedFuseID}' — returned to inventory.");
        }

        // Reset state
        insertedObject = null;
        insertedFuseID = "";
        isOccupied = false;
        isCorrect = false;

        SetIndicator(matOff);
        fuseBoxController?.OnSlotChanged();
    }

    private string GetFuseDisplayName(GameObject obj)
    {
        Fuse f = obj.GetComponent<Fuse>();
        return f != null ? f.displayName : obj.name.Replace("_", " ");
    }

    private Sprite GetFuseIcon(GameObject obj)
    {
        Fuse f = obj.GetComponent<Fuse>();
        return f != null ? f.itemIcon : null;
    }

    // ── Indicator Helper ──────────────────────────────────────────────────────

    private void SetIndicator(Material mat)
    {
        if (fuseBoxRenderer == null || mat == null) return;

        // Get a copy of the current materials array, swap the one at our index
        Material[] mats = fuseBoxRenderer.materials;

        if (indicatorMaterialIndex < 0 || indicatorMaterialIndex >= mats.Length)
        {
            Debug.LogWarning($"[FuseSlot] indicatorMaterialIndex {indicatorMaterialIndex} " +
                             $"is out of range (FuseBox has {mats.Length} materials).");
            return;
        }

        mats[indicatorMaterialIndex] = mat;
        fuseBoxRenderer.materials = mats;
    }
}