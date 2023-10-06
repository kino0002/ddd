using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemStack
{
    public Item Item { get; private set; }
    public int Quantity { get; set; }

    public ItemStack(Item item, int quantity)
    {
        Item = item;
        Quantity = quantity;
    }
}

public class GridSlot
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public Item Item { get; private set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public GridSlot(int x, int y, int width = 1, int height = 1)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool IsOccupied()
    {
        return Item != null;
    }

    public bool CanPlaceItem(Item item)
    {
        return !IsOccupied();
    }

    public void SetItem(Item item)
    {
        Item = item;
    }

    public void RemoveItem()
    {
        Item = null;
    }

    public void SetDimensions(int width, int height)
    {
        this.Width = width;
        this.Height = height;
    }
}

public class GridItemContainer
{
    private const int GridWidth = 5;
    private int gridHeight;
    private GridSlot[,] grid;
    private List<Item> items;
    public List<Item> Items => items;
    public GridItemContainer(EquipmentDefinition equipment)
    {
        int maxStorageSpace = equipment.MaxStorageSpace;
        this.gridHeight = (int)Math.Ceiling((double)maxStorageSpace / GridWidth);
        grid = new GridSlot[GridWidth, gridHeight];
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = new GridSlot(x, y);
            }
        }
        items = new List<Item>();
    }

    public bool AddItem(Item item, int width = 1, int height = 1)
    {
        GridSlot slot = FindSlot(item, width, height);
        if (slot != null)
        {
            slot.SetItem(item);
            slot.SetDimensions(width, height);
            items.Add(item);
            Debug.Log($"Item added to container. Current count: {items.Count}");
            return true;
        }
        Debug.Log("Failed to add item to container.");
        return false;
    }

    // Modified FindSlot method
    public GridSlot FindSlot(Item item, int width, int height)
    {
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (CanPlaceItem(x, y, width, height))
                {
                    return grid[x, y];
                }
            }
        }
        return null;
    }

    // Modified CanPlaceItem method
    public bool CanPlaceItem(int startX, int startY, int width, int height)
    {
        if (startX + width > GridWidth || startY + height > gridHeight)
        {
            return false;
        }

        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (grid[x, y].IsOccupied())
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool RemoveItem(Item item)
    {
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y].Item == item)
                {
                    grid[x, y].RemoveItem();
                    items.Remove(item);
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasAvailableSpace()
    {
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!grid[x, y].IsOccupied())
                {
                    return true;
                }
            }
        }
        return false;
    }

    public List<Item> GetItems()
    {
        return items;
    }
}
