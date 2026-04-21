using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Slots")]
    [SerializeField] private List<Image> itemIcons;

    private List<string> items = new List<string>();

    private void Awake()
    {
        Instance = this;
    }

    public bool AddItem(string itemName, Sprite icon)
    {
        if (items.Count >= itemIcons.Count)
        {
            Debug.Log("Inventory full!");
            return false;
        }

        items.Add(itemName);
        int index = items.Count - 1;
        itemIcons[index].sprite = icon;
        itemIcons[index].color = Color.white;
        return true;
    }

    public bool HasItem(string itemName)
    {
        return items.Contains(itemName);
    }

    public void RemoveItem(string itemName)
    {
        int index = items.IndexOf(itemName);
        if (index == -1) return;

        items.RemoveAt(index);
        itemIcons[index].sprite = null;
        itemIcons[index].color = new Color(1, 1, 1, 0);

        // Shift remaining icons left
        for (int i = index; i < items.Count; i++)
        {
            itemIcons[i].sprite = itemIcons[i + 1].sprite;
            itemIcons[i].color = itemIcons[i + 1].color;
        }

        // Clear last slot
        itemIcons[items.Count].sprite = null;
        itemIcons[items.Count].color = new Color(1, 1, 1, 0);
    }
}