using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

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
        equipmentManager = Object.FindObjectOfType<EquipmentManager>();
        if (equipmentManager == null)
        {
            Debug.LogError("EquipmentManager not found.");
            return;
        }

        UpdateStorageDisplay(); // Call this to ensure grid is created on start

        root = GetComponent<UIDocument>().rootVisualElement;
        equipmentManager.OnEquipmentChanged += UpdateEquipmentSlot;

        invScrollView = root.Q<ScrollView>("Inv");
        Debug.Log("InventoryUI Start called.");
        styleSheet = Resources.Load<StyleSheet>("inve");

        dragAndDropManager = GetComponent<InventoryDragAndDrop>();

        // Calculate total storage space from all equipped storage items
        int totalStorageSpace = equipmentManager.GetTotalEquippedStorageSpace();

        // Determine the number of rows based on the total storage space, assuming each row has 5 slots
        int numRows = Mathf.CeilToInt((float)equipmentManager.GetTotalEquippedStorageSpace() / slotsPerRow);
        inventoryGrid = new Slot[numRows, slotsPerRow];
        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < slotsPerRow; j++)
            {
                inventoryGrid[i, j] = new Slot();
            }
        }
    }


    private void OnDestroy()
    {
        if (equipmentManager != null)
        {
            equipmentManager.OnEquipmentChanged -= UpdateEquipmentSlot;
        }
    }

    private void UpdateEquipmentSlot(EquipmentDefinition equipment, string slotType)
    {
        string slotID = slotType + "Icon";
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
        Debug.Log("UpdateStorageDisplay called.");

        // Clear current display
        invScrollView.Clear();

        // Get storage equipment and items
        var storageEquipments = equipmentManager.GetStorageEquipments();


        foreach (var storageEquipment in storageEquipments)
        {
            if (storageEquipment.equippedItem != null) // Ensure equippedItem is not null
            {
                var storageContainer = CreateStorageItemContainer();
                storageContainer.Add(CreateTinyEquipmentView(storageEquipment.equippedItem.Icon));

                var itemsGrid = CreateDynamicItemsGridContainer();

                if (storageEquipment.storageContainer != null)
                {
                    var items = storageEquipment.storageContainer.GetItems();
                    int currentItemCount = items.Count;

                    for (int i = 0; i < ((StorageItem)storageEquipment.equippedItem).MaxStorageSpace; i++)
                    {
                        if (i < currentItemCount)
                        {
                            itemsGrid.Add(CreateItemSlot(items[i].Item.Icon));
                        }
                        else
                        {
                            itemsGrid.Add(CreateItemSlot());
                        }
                    }
                }
                storageContainer.Add(itemsGrid);
                invScrollView.Add(storageContainer);
            }
        }
    }

    private VisualElement CreateDynamicItemsGridContainer()
    {
        var grid = new VisualElement();
        grid.AddToClassList("ItemsGridContainer");

        // Fetching the dimensions of the inventory grid directly from the InventoryUI's inventoryGrid
        int rows = inventoryGrid.GetLength(0);
        int cols = inventoryGrid.GetLength(1);

        // Create slots based on these dimensions
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

    private bool CanPlaceItem(Item item, int row, int col)
    {
        // Temporary return value until you implement the function
        return false;
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

    private VisualElement CreateItemsGridContainer()
    {
        var grid = new VisualElement();
        grid.AddToClassList("ItemsGridContainer");
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