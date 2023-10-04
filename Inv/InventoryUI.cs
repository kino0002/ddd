using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    private EquipmentManager equipmentManager;
    private VisualElement root;
    private ScrollView invScrollView;
    private StyleSheet styleSheet;
    private InventoryDragAndDrop dragAndDropManager;

    private const int slotsPerRow = 5;
    private Slot[,] inventoryGrid;

    private void Start()
    {
        InitManagersAndUIElements();
        InitInventoryGrid();
        UpdateStorageDisplay();
    }

    private void OnDestroy()
    {
        if (equipmentManager != null)
        {
            equipmentManager.OnEquipmentChanged -= UpdateEquipmentSlot;
        }
    }

    private void InitManagersAndUIElements()
    {
        equipmentManager = Object.FindObjectOfType<EquipmentManager>();
        if (equipmentManager == null)
        {
            Debug.LogError("EquipmentManager not found.");
            return;
        }

        root = GetComponent<UIDocument>().rootVisualElement;
        invScrollView = root.Q<ScrollView>("Inv");

        styleSheet = Resources.Load<StyleSheet>("inve");
        dragAndDropManager = GetComponent<InventoryDragAndDrop>();

        equipmentManager.OnEquipmentChanged += UpdateEquipmentSlot;
    }

    private void InitInventoryGrid()
    {
        int totalStorageSpace = equipmentManager.GetTotalEquippedStorageSpace();
        int numRows = Mathf.CeilToInt((float)totalStorageSpace / slotsPerRow);

        inventoryGrid = new Slot[numRows, slotsPerRow];

        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < slotsPerRow; j++)
            {
                inventoryGrid[i, j] = new Slot();
            }
        }
    }

    private void UpdateEquipmentSlot(EquipmentDefinition equipment, EquipmentSlotType slotType)
    {
        string slotID = slotType.ToString() + "Icon";
        Image slotIcon = root.Q<Image>(slotID);
        VisualElement parentSlot = slotIcon?.parent;

        if (parentSlot != null)
        {
            if (equipment != null)
            {
                slotIcon.sprite = equipment.Icon;
                dragAndDropManager.RegisterSlotForDragging(parentSlot, slotType);
                parentSlot.AddToClassList("filled");
            }
            else
            {
                slotIcon.sprite = null;
                parentSlot.RemoveFromClassList("filled");
            }
        }
        else
        {
            Debug.LogWarning($"Icon with ID '{slotID}' not found.");
        }
    }

    public void UpdateStorageDisplay()
    {
        if (equipmentManager == null) return;

        invScrollView.Clear();
        var storageEquipments = equipmentManager.GetStorageEquipments();

        foreach (var storageEquipment in storageEquipments)
        {
            var storageContainer = CreateStorageItemContainer();
            if (storageEquipment.equippedItem != null)
            {
                storageContainer.Add(CreateTinyEquipmentView(storageEquipment.equippedItem.Icon));
            }

            var itemsGrid = CreateDynamicItemsGridContainer();
            int maxStorageSpace = storageEquipment.equippedItem.MaxStorageSpace;
            List<Item> items = storageEquipment.storageContainer.Items;

            for (int i = 0; i < maxStorageSpace; i++)
            {
                if (i < items.Count)
                {
                    itemsGrid.Add(CreateItemSlot(items[i].Icon));
                }
                else
                {
                    itemsGrid.Add(CreateItemSlot());
                }
            }
            storageContainer.Add(itemsGrid);
            invScrollView.Add(storageContainer);
        }
    }

    private VisualElement CreateStorageItemContainer()
    {
        var container = new VisualElement();
        container.name = "StorageContainer";
        container.AddToClassList("StorageContainer");

        if (styleSheet != null)
        {
            container.styleSheets.Add(styleSheet);
        }
        else
        {
            Debug.LogError("StyleSheet is still null.");
        }

        return container;
    }

    private VisualElement CreateTinyEquipmentView(Sprite icon)
    {
        var view = new VisualElement();
        view.AddToClassList("TinyEquipmentView");

        if (icon != null)
        {
            var image = new Image { sprite = icon };
            view.Add(image);
        }
        return view;
    }

    private VisualElement CreateDynamicItemsGridContainer()
    {
        var grid = new VisualElement();
        grid.AddToClassList("ItemsGridContainer");

        int rows = inventoryGrid.GetLength(0);
        int cols = inventoryGrid.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var slot = CreateItemSlot();
                grid.Add(slot);
            }
        }
        return grid;
    }

    private VisualElement CreateItemSlot(Sprite icon = null)
    {
        var slot = new VisualElement();
        
        if (icon != null)
        {
            var image = new Image { sprite = icon };
            slot.Add(image);
            slot.AddToClassList("ItemSlotWithItem");
        }
        else
        {
            slot.AddToClassList("ItemSlot");
        }
        return slot;
    }

    private class Slot
    {
        public bool IsOccupied { get; set; }
        public Item ItemInSlot { get; set; }
    }
}

