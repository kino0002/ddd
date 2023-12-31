using System;
using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    [System.Serializable]
    public class EquipmentSlot
    {
        public string slotType;
        public EquipmentDefinition equippedItem;
        public ItemContainer storageContainer;
    }

    [SerializeField]
    private List<EquipmentSlot> equipmentSlots;

    public event Action<EquipmentDefinition, string> OnEquipmentChanged;

    // Grid-based inventory variables
    private Item[,] inventoryGrid;

    private void Awake()
    {
        // Initialize equipmentSlots with default values
        equipmentSlots = new List<EquipmentSlot>
        {
            new EquipmentSlot { slotType = "PrimarySlot" },
            new EquipmentSlot { slotType = "HeadSlot" },
            new EquipmentSlot { slotType = "ChestSlot" },
            new EquipmentSlot { slotType = "LegsSlot" },
            new EquipmentSlot { slotType = "BootsSlot" },
            new EquipmentSlot { slotType = "SecondarySlot" },
            new EquipmentSlot { slotType = "NecklaceSlot" },
            new EquipmentSlot { slotType = "BagSlot" },
            new EquipmentSlot { slotType = "BeltSlot" },
            new EquipmentSlot { slotType = "RingSlot" }
        };
    }

    public bool EquipItem(EquipmentDefinition newItem)
    {
        if (newItem == null)
        {
            return false;
        }

        EquipmentSlot slot = GetEquipmentSlot(newItem.slotType);

        if (slot != null)
        {
            if (slot.equippedItem != null)
            {
                UnequipItem(slot.slotType);
            }

            slot.equippedItem = newItem;
            OnEquipmentChanged?.Invoke(newItem, slot.slotType);

            if (newItem is StorageItem storageItem)
            {
                AdjustInventoryGrid(storageItem);
                slot.storageContainer = new ItemContainer(storageItem.MaxStorageSpace);
            }

            // Update the storage display whenever an item is equipped
            FindObjectOfType<InventoryUI>().UpdateStorageDisplay();

            return true;
        }

        return false; // Return false if slot is null.
    }
    private void AdjustInventoryGrid(StorageItem storageItem)
    {
        // The dimensions of the grid will be based on the storage item's max storage space.
        // For simplicity, let's consider it a square grid for now. 
        // This can be further customized based on your game's requirements.
        int sideLength = Mathf.CeilToInt(Mathf.Sqrt(storageItem.MaxStorageSpace));
        inventoryGrid = new Item[sideLength, sideLength];
    }


    public void UnequipItem(string slotType)
    {
        EquipmentSlot slot = GetEquipmentSlot(slotType);
        if (slot != null && slot.equippedItem != null)
        {
            EquipmentDefinition oldItem = slot.equippedItem;
            slot.equippedItem = null;
            OnEquipmentChanged?.Invoke(null, slotType);

            if (oldItem is StorageItem)
            {
                slot.storageContainer = null;
                FindObjectOfType<InventoryUI>().UpdateStorageDisplay();
            }
        }
    }

    public EquipmentSlot GetEquipmentSlot(string slotType)
    {
        EquipmentSlot slot = equipmentSlots.Find(slot => slot.slotType == slotType);

        if (slot != null)
        {
            MessageDisplayManager.Instance.DisplayMessage("Equipment slot found for slotType: " + slotType);
        }
        else
        {
            Debug.LogWarning("Equipment slot not found for slotType: " + slotType);
        }

        return slot;
    }


    public List<EquipmentSlot> GetEquipmentSlots()
    {
        return equipmentSlots;
    }

    public void UnequipItemInstance(EquipmentDefinition equipmentInstance)
    {
        EquipmentSlot slot = equipmentSlots.Find(slot => slot.equippedItem == equipmentInstance);
        if (slot != null)
        {
            slot.equippedItem = null;
            OnEquipmentChanged?.Invoke(slot.equippedItem, slot.slotType);

            if (equipmentInstance is StorageItem)
            {
                slot.storageContainer = null;
            }
        }
        else
        {
            Debug.Log("Slot not found");
        }
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.UpdateStorageDisplay();
        }
    }

    public List<EquipmentSlot> GetStorageEquipments()
    {
        // This will filter out all equipment slots which have a StorageItem equipped and ensure it's not null
        return equipmentSlots.FindAll(slot => slot.equippedItem != null && slot.equippedItem is StorageItem);
    }


    public int GetTotalEquippedStorageSpace()
    {
        int totalSpace = 0;

        foreach (var slot in equipmentSlots)
        {
            if (slot.equippedItem is StorageItem storageItem)
            {
                totalSpace += storageItem.MaxStorageSpace;
            }
        }

        return totalSpace;
    }


}