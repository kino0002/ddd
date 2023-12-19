using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryGrid : MonoBehaviour
{
    public int slotsPerRow = 5; // Example size
    private int gridHeight; // This will be set by the bag's MaxStorageSpace

    private Item[,] grid; // 2D array to represent the grid

    private EquipmentManager equipmentManager;

    private void Awake()
    {
        equipmentManager = Object.FindObjectOfType<EquipmentManager>();
        if (equipmentManager == null)
        {
            Debug.LogError("EquipmentManager not found.");
            return;
        }

        // Update gridHeight based on total equipped storage space
        gridHeight = Mathf.CeilToInt((float)equipmentManager.GetTotalEquippedStorageSpace() / slotsPerRow); // Assuming each row has 5 slots
        grid = new Item[slotsPerRow, gridHeight];
    }



    public bool CanPlaceItem(Item item, int x, int y, bool isHorizontal)
    {
        int width = isHorizontal ? item.SlotDimension.Width : item.SlotDimension.Height;
        int height = isHorizontal ? item.SlotDimension.Height : item.SlotDimension.Width;

        if (x + width > slotsPerRow || y + height > gridHeight)
        {
            return false; // Out of grid bounds
        }

        for (int i = x; i < x + width; i++)
        {
            for (int j = y; j < y + height; j++)
            {
                if (grid[i, j] != null)
                {
                    return false; // Overlapping another item
                }
            }
        }

        return true;
    }

    public void PlaceItem(Item item, int x, int y, bool isHorizontal)
    {
        if (!CanPlaceItem(item, x, y, isHorizontal)) return;

        int width = isHorizontal ? item.SlotDimension.Width : item.SlotDimension.Height;
        int height = isHorizontal ? item.SlotDimension.Height : item.SlotDimension.Width;

        for (int i = x; i < x + width; i++)
        {
            for (int j = y; j < y + height; j++)
            {
                grid[i, j] = item;
            }
        }
    }

    public void RemoveItem(Item item, int x, int y, bool isHorizontal)
    {
        int width = isHorizontal ? item.SlotDimension.Width : item.SlotDimension.Height;
        int height = isHorizontal ? item.SlotDimension.Height : item.SlotDimension.Width;

        for (int i = x; i < x + width; i++)
        {
            for (int j = y; j < y + height; j++)
            {
                grid[i, j] = null;
            }
        }
    }

    public bool RotateItem(Item item, int x, int y, bool currentOrientation)
    {
        // Remove item in its current orientation
        RemoveItem(item, x, y, currentOrientation);

        // Check if it can be placed in the other orientation
        if (CanPlaceItem(item, x, y, !currentOrientation))
        {
            PlaceItem(item, x, y, !currentOrientation);
            return true; // Rotation successful
        }
        else
        {
            // Place it back in its original orientation if rotation isn't possible
            PlaceItem(item, x, y, currentOrientation);
            return false; // Rotation unsuccessful
        }
    }
}