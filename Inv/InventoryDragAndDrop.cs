using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


public class InventoryDragAndDrop : MonoBehaviour
{
    private VisualElement root;
    private VisualElement currentSlot;
    private Image draggedItem;
    private bool isDraggingOutside;
    private EquipmentManager equipmentManager;
    private ItemDropping itemDropping;

    public GameObject playerCharacter;
    public Movement playerMovementScript;
    public float throwForce = 4f;

    [SerializeField] private GameObject droppedItemPrefab;

    private void Awake()
    {
        equipmentManager = Object.FindObjectOfType<EquipmentManager>();
        if (equipmentManager == null)
        {
            Debug.LogError("EquipmentManager not found.");
            return;
        }

        root = GetComponent<UIDocument>().rootVisualElement;

        root.RegisterCallback<MouseUpEvent>(OnGlobalMouseUp);
        root.RegisterCallback<MouseMoveEvent>(OnGlobalMouseMove);

        itemDropping = new ItemDropping(playerCharacter, droppedItemPrefab, playerMovementScript, throwForce);
    }

    public void RegisterSlotForDragging(VisualElement slot, EquipmentSlotType? slotType = null, Item item = null)
    {
        if (slotType != null)
            slot.userData = slotType;
        else if (item != null)
            slot.userData = item;

        slot.RegisterCallback<MouseMoveEvent>(OnSlotMouseMove);
    }

    private void OnSlotMouseMove(MouseMoveEvent evt)
    {
        if (Input.GetMouseButton(0) && currentSlot == null)
        {
            currentSlot = evt.currentTarget as VisualElement;
            Image slotIcon = currentSlot.Q<Image>();

            if (slotIcon != null && slotIcon.sprite != null)  // Check if the slot has an item
            {
                Debug.Log("Dragging item: " + slotIcon.sprite.name);

                slotIcon.style.opacity = 0.5f;
                draggedItem = new Image
                {
                    sprite = slotIcon.sprite,
                    style =
                {
                    position = Position.Absolute,
                    width = slotIcon.layout.width,
                    height = slotIcon.layout.height,
                    opacity = 0f
                }
                };
                root.Add(draggedItem);
                UpdateDraggedItemPosition(evt.mousePosition);
                StartCoroutine(FadeIn(draggedItem, 0.6f));
            }
        }
    }


    IEnumerator FadeIn(Image img, float duration)
    {
        float startTime = Time.time;
        while (img.style.opacity.value < 1f)
        {
            float t = (Time.time - startTime) / duration;
            img.style.opacity = Mathf.SmoothStep(0f, 6f, t);
            yield return null;
        }
    }

    private void UpdateDraggedItemPosition(Vector2 mousePosition)
    {
        if (draggedItem != null)
        {
            draggedItem.style.left = mousePosition.x - draggedItem.layout.width / 2;
            draggedItem.style.top = mousePosition.y - draggedItem.layout.height / 2;
        }
    }

    private void OnGlobalMouseUp(MouseUpEvent evt)
    {
        if (currentSlot != null)
        {
            if (currentSlot.userData is EquipmentSlotType)
            {
                EquipmentSlotType slotId = (EquipmentSlotType)currentSlot.userData;
                EquipmentManager.EquipmentSlot equipmentSlot = equipmentManager.GetEquipmentSlot(slotId);

                if (equipmentSlot == null || equipmentSlot.equippedItem == null)
                {
                    Debug.LogError($"equipmentSlot or equippedItem is null for ID: {slotId}");
                    return;
                }

                EquipmentDefinition itemToBeDropped = equipmentSlot.equippedItem;
                Image slotIcon = currentSlot.Q<Image>();

                if (slotIcon != null)
                {
                    slotIcon.style.opacity = 1.0f;
                }

                if (draggedItem != null)
                {
                    Debug.Log("Entered draggedItem != null block");  // New Debug Line 1

                    if (isDraggingOutside)
                    {
                        Debug.Log("Entered isDraggingOutside block");  // New Debug Line 2

                        // Debugging: Print the values before using them
                        Debug.Log($"equipmentSlot.storageContainer: {equipmentSlot.storageContainer}");
                        Debug.Log($"equipmentManager.tempStorage: {equipmentManager.tempStorage}");
                        Debug.Log($"itemToBeDropped: {itemToBeDropped}");

                        Debug.Log("Checking itemToBeDropped and equipmentManager.tempStorage");  // New Debug Line 3

                        if (itemToBeDropped != null && equipmentManager.tempStorage != null)
                        {
                            Debug.Log("Entered itemToBeDropped and equipmentManager.tempStorage block");  // Existing Debug Line

                            Debug.Log($"itemToBeDropped.MaxStorageSpace: {itemToBeDropped.MaxStorageSpace}");  // New Debug Line
                            Debug.Log($"equipmentManager.tempStorage.ContainsKey(itemToBeDropped): {equipmentManager.tempStorage.ContainsKey(itemToBeDropped)}");  // New Debug Line
                            Debug.Log("Keys in equipmentManager.tempStorage:");
                            foreach (var key in equipmentManager.tempStorage.Keys)
                            {
                                Debug.Log(key);
                            }
                            if (itemToBeDropped.MaxStorageSpace > 0 && equipmentManager.tempStorage.ContainsKey(itemToBeDropped))
                            {
                                // Debugging: Print the value before using it
                                Debug.Log($"equipmentManager.tempStorage[itemToBeDropped]: {equipmentManager.tempStorage[itemToBeDropped]}");

                                equipmentSlot.storageContainer.SetItems(new List<Item>(equipmentManager.tempStorage[itemToBeDropped]));
                                equipmentManager.tempStorage.Remove(itemToBeDropped);
                            }
                        }
                        else
                        {
                            Debug.LogError("Either itemToBeDropped or equipmentManager.tempStorage is null");
                            Debug.Log("Did not enter innermost block");  // New Debug Line

                        }
                        // Create the dropped item instance
                        itemDropping.CreateDroppedItemInstance(itemToBeDropped, evt.mousePosition);

                        // Explicitly update UI
                        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
                        if (inventoryUI != null)
                        {
                            inventoryUI.UpdateStorageDisplay();
                        }
                    }


                    root.Remove(draggedItem);
                    draggedItem = null;
                    currentSlot = null;
                }
            }
            else if (currentSlot.userData is Item)
            {
                Item item = (Item)currentSlot.userData;
                // Handle drop logic for regular inventory items here...
            }
        }
    }

    private void RemoveItemFromInventory(EquipmentDefinition itemToBeDropped)
    {
        // Call the method to remove the item from inventory (this might need to be added to the EquipmentManager class)
        equipmentManager.RemoveItem(itemToBeDropped);

        // Update the UI to reflect the changes
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.UpdateStorageDisplay();
        }
    }


    private void OnGlobalMouseMove(MouseMoveEvent evt)
    {
        if (draggedItem != null)
        {
            UpdateDraggedItemPosition(evt.mousePosition);

            VisualElement inventoryPanel = root.Q<VisualElement>("InvContainer");
            Rect panelRect = new Rect(inventoryPanel.layout.position, inventoryPanel.layout.size);
            Vector2 mousePos = evt.mousePosition;
            isDraggingOutside = !panelRect.Contains(mousePos);

            if (isDraggingOutside)
            {
                if (!draggedItem.Children().Any(child => child.name == "DroppingLabel"))
                {
                    var droppingLabel = new Label("Dropping")
                    {
                        name = "DroppingLabel",
                        style =
                        {
                            unityTextAlign = TextAnchor.MiddleCenter,
                            fontSize = 16,
                            color = Color.white,
                            position = Position.Absolute,
                            top = -30,
                            left = draggedItem.layout.width / 2 - 50,
                            width = 100
                        }
                    };
                    draggedItem.Add(droppingLabel);
                }
            }
            else
            {
                draggedItem.Q<Label>("DroppingLabel")?.RemoveFromHierarchy();
            }
        }
    }

    private void SaveStoredItemsBeforeDropping(EquipmentDefinition equipmentToBeDropped)
    {
        if (equipmentToBeDropped != null && equipmentToBeDropped.MaxStorageSpace > 0)
        {
            EquipmentManager.EquipmentSlot slot = equipmentManager.GetEquipmentSlot(equipmentToBeDropped.Slot); // Change from equipmentToBeDropped.slotType

            if (slot != null && slot.storageContainer != null)
            {
                // Create a new list to store the item stacks
                List<ItemStack> itemStacks = new List<ItemStack>();

                // Populate the list with items from the slot's storage container
                foreach (var item in slot.storageContainer.Items)
                {
                    itemStacks.Add(new ItemStack(item, 1)); // Assuming each item in the storage is a single item for now
                }

                // Retrieve the ItemPickup component from the dropped item prefab
                ItemPickup itemPickupComponent = droppedItemPrefab.GetComponent<ItemPickup>();
                if (itemPickupComponent != null)
                {
                    itemPickupComponent.StoredItems = itemStacks;
                }
            }
        }
    }

}