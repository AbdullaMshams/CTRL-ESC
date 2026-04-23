using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Slots")]
    [SerializeField] private List<Image> itemIcons;
    [SerializeField] private List<Image> slotBackgrounds;

    [Header("Selection Colors")]
    [SerializeField] private Color normalSlotColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color selectedSlotColor = new Color(0.4f, 0.7f, 1f, 0.9f);

    private List<string> items = new List<string>();
    private List<Sprite> icons = new List<Sprite>();
    private int selectedSlot = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        HandleSlotSelection();
        HandleDrop();
    }

    private void HandleSlotSelection()
    {
        // Press 1-6 to select slot
        for (int i = 0; i < 6; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
    }

    private void HandleDrop()
    {
        // Press G to drop selected item
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropSelectedItem();
        }
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
            {
                slotBackgrounds[i].color = i == selectedSlot 
                    ? selectedSlotColor 
                    : normalSlotColor;
            }
        }
    }

    public bool AddItem(string itemName, Sprite icon)
    {
        if (items.Count >= itemIcons.Count)
        {
            Debug.Log("Inventory full!");
            return false;
        }

        items.Add(itemName);
        icons.Add(icon);

        int index = items.Count - 1;
        if (icon != null)
        {
            itemIcons[index].sprite = icon;
            itemIcons[index].color = Color.white;
        }
        else
        {
            // No icon — show white square placeholder
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

    public void RemoveItem(string itemName)
    {
        int index = items.IndexOf(itemName);
        if (index == -1) return;

        items.RemoveAt(index);
        icons.RemoveAt(index);

        // Rebuild all slot visuals
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
    }

    private void DropSelectedItem()
    {
        if (selectedSlot >= items.Count) return;

        string itemName = items[selectedSlot];
        Debug.Log($"Dropped: {itemName}");
        RemoveItem(itemName);
    }
}