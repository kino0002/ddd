using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EquipmentManager : MonoBehaviour
{
    [System.Serializable]
    public class EquipmentSlot
    {
        public EquipmentSlotType slotType;
        public EquipmentDefinition equippedItem;
        public GridItemContainer storageContainer;
    }

    [SerializeField]
    private List<EquipmentSlot> equipmentSlots;

    public event Action<EquipmentDefinition, EquipmentSlotType> OnEquipmentChanged;
    public Dictionary<EquipmentDefinition, List<Item>> tempStorage = new Dictionary<EquipmentDefinition, List<Item>>();

    private void Awake()
    {
        // Initialize equipmentSlots with default values
        equipmentSlots = new List<EquipmentSlot>
    {
        new EquipmentSlot { slotType = EquipmentSlotType.PrimarySlot },
        new EquipmentSlot { slotType = EquipmentSlotType.HeadSlot },
        new EquipmentSlot { slotType = EquipmentSlotType.ChestSlot },
        new EquipmentSlot { slotType = EquipmentSlotType.LegsSlot },
        new EquipmentSlot { slotType = EquipmentSlotType.BootsSlot },
        new EquipmentSlot { slotType = EquipmentSlotType.SecondarySlot },
        new EquipmentSlot { slotType = EquipmentSlotType.NecklaceSlot },
        new EquipmentSlot { slotType = EquipmentSlotType.BagSlot },
        new EquipmentSlot { slotType = EquipmentSlotType.BeltSlot },
        new EquipmentSlot { slotType = EquipmentSlotType.RingSlot }
    };
    }


    public bool EquipItem(EquipmentDefinition newItem)
    {
        if (newItem == null)
        {
            return false;
        }

        EquipmentSlot slot = GetEquipmentSlot(newItem.Slot);

        if (slot != null)
        {
            if (slot.equippedItem != null)
            {
                //Debug.Log($"Equipment slot {newItem.Slot} is already in use by {slot.equippedItem.itemName}.");
                return false;
            }

            slot.equippedItem = newItem;
            OnEquipmentChanged?.Invoke(newItem, slot.slotType);

            if (newItem.MaxStorageSpace > 0)
            {
                slot.storageContainer = new GridItemContainer(newItem); // Updated this line
            }

            FindObjectOfType<InventoryUI>().UpdateStorageDisplay();

            return true;
        }

        return false;
    }


    public void UnequipItem(EquipmentSlotType slotType)
    {
        EquipmentSlot slot = GetEquipmentSlot(slotType);
        if (slot != null && slot.equippedItem != null)
        {
            EquipmentDefinition oldItem = slot.equippedItem;
            slot.equippedItem = null;
            OnEquipmentChanged?.Invoke(null, slotType);

            if (oldItem.MaxStorageSpace > 0)
            {
                slot.storageContainer = null;
                FindObjectOfType<InventoryUI>().UpdateStorageDisplay();
            }
        }
    }

    public EquipmentSlot GetEquipmentSlot(EquipmentSlotType slotType)  // Change from string to EquipmentSlotType
    {
        EquipmentSlot slot = equipmentSlots.Find(slot => slot.slotType == slotType);  // Remove ToString()

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

    public bool UnequipItemInstance(EquipmentDefinition equipmentInstance)
    {
        Debug.Log("Trying to unequip: " + equipmentInstance.itemName);

        if (equipmentInstance == null)
        {
            Debug.LogError("equipmentInstance is null in UnequipItemInstance.");
            return false;
        }

        if (equipmentSlots == null || !equipmentSlots.Any())
        {
            Debug.LogError("equipmentSlots is either null or empty.");
            return false;
        }

        EquipmentSlot slot = equipmentSlots.Find(slot => slot.equippedItem == equipmentInstance);

        if (slot == null)
        {
            Debug.LogError($"No slot found containing {equipmentInstance.itemName}.");
            return false;
        }

        Debug.Log("Unequipping item: " + equipmentInstance.itemName);
        slot.equippedItem = null;
        OnEquipmentChanged?.Invoke(slot.equippedItem, slot.slotType);

        if (equipmentInstance.MaxStorageSpace > 0)
        {
            slot.storageContainer = null;
        }

        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.UpdateStorageDisplay();
        }

        Debug.Log("Equipment Slots: " + string.Join(", ", equipmentSlots.Select(s => s.equippedItem?.itemName ?? "null")));
        return true;
    }



    public List<EquipmentSlot> GetStorageEquipments()
    {
        return equipmentSlots.FindAll(slot => slot.equippedItem != null && slot.equippedItem.MaxStorageSpace > 0);
    }

    public int GetTotalEquippedStorageSpace()
    {
        int totalSpace = 0;

        foreach (var slot in equipmentSlots)
        {
            if (slot.equippedItem != null && slot.equippedItem.MaxStorageSpace > 0)
            {
                totalSpace += slot.equippedItem.MaxStorageSpace;
            }
        }

        return totalSpace;
    }

    public void RemoveItem(EquipmentDefinition itemToRemove)
    {
        if (itemToRemove == null)
        {
            Debug.LogWarning("Tried to remove a null item.");
            return;
        }

        // Find the slot containing the item to be removed
        EquipmentSlot slot = equipmentSlots.Find(s => s.equippedItem == itemToRemove);

        if (slot != null)
        {
            // Log the initial states
            Debug.Log($"Before removal: Slot type: {slot.slotType}, Equipped item: {slot.equippedItem?.itemName}, Storage container: {slot.storageContainer}");

            // Remove the item from the slot
            slot.equippedItem = null;

            // Save items to temporary storage before removing the bag
            if (itemToRemove.MaxStorageSpace > 0 && slot.storageContainer != null)
            {
                tempStorage[itemToRemove] = new List<Item>(slot.storageContainer.Items);
                slot.storageContainer.Items.Clear(); // Clear the items from the bag's storage
            }

            // Log the final states
            Debug.Log($"After removal: Slot type: {slot.slotType}, Equipped item: {slot.equippedItem}, Storage container: {slot.storageContainer}");

            // Update the inventory UI
            InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
            if (inventoryUI != null)
            {
                inventoryUI.UpdateStorageDisplay();
            }
        }
        else
        {
            Debug.LogWarning($"Item {itemToRemove.itemName} not found in any slot.");
        }
    }

    public void SaveItemsBeforeDropping(EquipmentDefinition itemToBeDropped)
    {
        EquipmentSlot slot = equipmentSlots.Find(s => s.equippedItem == itemToBeDropped);
        if (slot != null && slot.storageContainer != null)
        {
            tempStorage[itemToBeDropped] = new List<Item>(slot.storageContainer.Items);
        }
    }

    public void LoadItemsAfterPickingUp(EquipmentDefinition pickedUpItem)
    {
        if (tempStorage.ContainsKey(pickedUpItem))
        {
            EquipmentSlot slot = equipmentSlots.Find(s => s.equippedItem == pickedUpItem);
            if (slot != null && slot.storageContainer != null)
            {
                slot.storageContainer.SetItems(tempStorage[pickedUpItem]);
                tempStorage.Remove(pickedUpItem);
            }
        }
    }



}
