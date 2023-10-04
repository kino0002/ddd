using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    public float pickupDelay = 0.5f;
    private float timeSinceThrown;
    public int playerLayer = 8;
    private InventoryUI inventoryUIInstance;

    public List<ItemStack> StoredItems { get; set; }

    public void SetTimeSinceThrown(float time)
    {
        timeSinceThrown = time;
    }

    private void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && item != null)
        {
            spriteRenderer.sprite = item.Icon;
            UpdateColliderShape();
        }
        playerLayer = LayerMask.NameToLayer("Player");
        inventoryUIInstance = FindObjectOfType<InventoryUI>();
    }

    private void UpdateColliderShape()
    {
        PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();
        if (polygonCollider != null)
        {
            Sprite sprite = GetComponent<SpriteRenderer>().sprite;
            if (sprite != null)
            {
                polygonCollider.pathCount = sprite.GetPhysicsShapeCount();
                List<Vector2> path = new List<Vector2>();

                for (int i = 0; i < polygonCollider.pathCount; i++)
                {
                    path.Clear();
                    sprite.GetPhysicsShape(i, path);
                    polygonCollider.SetPath(i, path);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Time.time > timeSinceThrown + pickupDelay)
        {
            EquipmentManager equipmentManager = other.GetComponent<EquipmentManager>();
            if (equipmentManager == null) return;

            if (item is EquipmentDefinition equipment)
            {
                HandleEquipmentPickup(equipmentManager, equipment);
            }
            else
            {
                HandleItemPickup(equipmentManager);
            }
            equipmentManager.LoadItemsAfterPickingUp(item as EquipmentDefinition);
        }
    }


    private void HandleEquipmentPickup(EquipmentManager equipmentManager, EquipmentDefinition equipment)
    {
        if (equipment == null)
        {
            Debug.LogError("Equipment is null. Cannot equip.");
            return;
        }

        EquipmentManager.EquipmentSlot slot = equipmentManager.GetEquipmentSlot(equipment.Slot);

        if (slot == null)
        {
            Debug.LogError($"No slot found for type {equipment.Slot}. Cannot equip.");
            return;
        }

        if (slot.equippedItem == null)
        {
            if (equipment is IEquippable equippableItem)
            {
                equippableItem.Equip(equipmentManager);

                if (StoredItems != null && StoredItems.Count > 0 && slot.storageContainer != null)
                {
                    foreach (var itemStack in StoredItems)
                    {
                        slot.storageContainer.AddItem(itemStack.Item); // Fix here
                    }
                }
                Destroy(gameObject);
                inventoryUIInstance?.UpdateStorageDisplay();
            }
        }
        else
        {
            string occupyingItemName = slot.equippedItem.itemName;
        }
    }

    private void HandleItemPickup(EquipmentManager equipmentManager)
    {
        bool itemAdded = false;

        // Loop through all storage slots and attempt to add the item
        foreach (var addSlot in equipmentManager.GetEquipmentSlots())
        {
            if (addSlot.equippedItem != null && addSlot.equippedItem.MaxStorageSpace > 0 && addSlot.storageContainer != null)
            {
                if (addSlot.storageContainer.AddItem(item)) // Assuming AddItem returns true if item was added
                {
                    itemAdded = true;
                    Destroy(gameObject);
                    inventoryUIInstance?.UpdateStorageDisplay();
                    break;  // Break out of the loop if the item was added
                }
                else
                {
                    Debug.Log($"Failed to add item to {addSlot.equippedItem.itemName}");
                }
            }
        }

        if (!itemAdded)
        {
            Debug.Log("No storage container found to pick up the item.");
        }
    }


}
