using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public int Width { get; private set; } = 1;
    public int Height { get; private set; } = 1;

    public GridSlot(int x, int y)
    {
        X = x;
        Y = y;
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
}

public class GridItemContainer
{
    private const int GridWidth = 5; // Fixed grid width
    private int gridHeight;
    private GridSlot[,] grid;
    private List<Item> items;

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

    public GridSlot FindSlot(Item item)
    {
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y].CanPlaceItem(item))
                {
                    return grid[x, y];
                }
            }
        }
        return null;
    }

    public bool AddItem(Item item)
    {
        if (items.Count >= this.gridHeight * GridWidth)
        {
            Debug.Log("Container is full. Item not added.");
            return false;
        }
        GridSlot slot = FindSlot(item);
        if (slot != null)
        {
            slot.SetItem(item);
            items.Add(item);
            Debug.Log($"Item added to container. Current count: {items.Count}");
            return true;
        }
        Debug.Log("Failed to add item to container.");
        return false;
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

    public List<Item> Items => items;

    public void SetItems(List<ItemStack> newItems)
    {
        items = newItems.Select(itemStack => itemStack.Item).ToList();
    }
}

