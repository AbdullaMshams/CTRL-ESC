using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Slots")]
    [SerializeField] private List<Image> itemIcons;
    [SerializeField] private List<Image> slotBackgrounds;

    [Header("Selection Colors")]
    [SerializeField] private Color normalSlotColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color selectedSlotColor = new Color(0.4f, 0.7f, 1f, 0.9f);

    [Header("Drop Settings")]
    [SerializeField] private float dropDistance = 1.5f;
    [SerializeField] private Camera playerCamera;

    private List<string> items = new List<string>();
    private List<Sprite> icons = new List<Sprite>();
    private List<GameObject> itemObjects = new List<GameObject>();
    private int selectedSlot = 0;

    private void Awake()
    {
        Instance = this;
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        HandleSlotSelection();
        HandleDrop();
    }

    private void HandleSlotSelection()
    {
        for (int i = 0; i < 6; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SelectSlot(i);
        }
    }

    private void HandleDrop()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            DropSelectedItem();
    }

    private void SelectSlot(int index)
    {
        selectedSlot = index;
        UpdateSlotVisuals();
    }

    private void UpdateSlotVisuals()
    {
        for (int i = 0; i < slotBackgrounds.Count; i++)
        {
            if (slotBackgrounds[i] != null)
                slotBackgrounds[i].color = i == selectedSlot
                    ? selectedSlotColor
                    : normalSlotColor;
        }
    }

    public GameObject GetSelectedItemObject()
{
    if (selectedSlot >= itemObjects.Count) return null;
    return itemObjects[selectedSlot];
}

public void RemoveSelectedItemTemporarily()
{
    if (selectedSlot >= items.Count) return;
    // Just hide the icon — don't destroy the reference
    itemIcons[selectedSlot].color = new Color(1, 1, 1, 0);
    itemIcons[selectedSlot].sprite = null;
    items.RemoveAt(selectedSlot);
    icons.RemoveAt(selectedSlot);
    itemObjects.RemoveAt(selectedSlot);
    UpdateSlotVisuals();
}

    public bool AddItem(string itemName, Sprite icon, GameObject itemObject)
    {
        if (items.Count >= itemIcons.Count)
        {
            Debug.Log("Inventory full!");
            return false;
        }

        items.Add(itemName);
        icons.Add(icon);
        itemObjects.Add(itemObject);

        int index = items.Count - 1;
        if (icon != null)
        {
            itemIcons[index].sprite = icon;
            itemIcons[index].color = Color.white;
        }
        else
        {
            itemIcons[index].color = new Color(1, 1, 1, 0.5f);
        }

        return true;
    }

    public bool HasItem(string itemName)
    {
        return items.Contains(itemName);
    }

    public string GetSelectedItem()
    {
        if (selectedSlot < items.Count)
            return items[selectedSlot];
        return null;
    }

    private void DropSelectedItem()
    {
        if (selectedSlot >= items.Count) return;

        string itemName = items[selectedSlot];
        GameObject obj = itemObjects[selectedSlot];

        // Respawn in front of player
        if (obj != null)
        {
            Vector3 dropPosition = playerCamera.transform.position 
                + playerCamera.transform.forward * dropDistance;
            dropPosition.y = 0.5f; // slightly above ground

            obj.transform.position = dropPosition;
            obj.transform.rotation = Quaternion.identity;
            obj.SetActive(true);

            // Re-enable collider if it was disabled
            Collider col = obj.GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }

        RemoveItemAt(selectedSlot);
    }

    public void RemoveItem(string itemName)
    {
        int index = items.IndexOf(itemName);
        if (index == -1) return;
        RemoveItemAt(index);
    }

    private void RemoveItemAt(int index)
    {
        items.RemoveAt(index);
        icons.RemoveAt(index);
        itemObjects.RemoveAt(index);

        // Rebuild visuals
        for (int i = 0; i < itemIcons.Count; i++)
        {
            if (i < items.Count)
            {
                itemIcons[i].sprite = icons[i];
                itemIcons[i].color = icons[i] != null ? Color.white : new Color(1,1,1,0.5f);
            }
            else
            {
                itemIcons[i].sprite = null;
                itemIcons[i].color = new Color(1, 1, 1, 0);
            }
        }

        UpdateSlotVisuals();
    }
}