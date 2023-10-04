using System;
using UnityEngine;

public interface IEquippable
{
    void Equip(EquipmentManager equipmentManager);
}

public enum EquipmentSlotType
{
    PrimarySlot,
    HeadSlot,
    ChestSlot,
    LegsSlot,
    BootsSlot,
    SecondarySlot,
    NecklaceSlot,
    BagSlot,
    BeltSlot,
    RingSlot
}

public enum WeaponType
{
    Sword,
    Bow,
    Axe,
    Wand
}

[CreateAssetMenu(fileName = "New Item", menuName = "Data/Item")]
public class Item : ScriptableObject
{
    public string itemId;
    public string itemName;
    public string Description;
    public int Price;
    public Sprite Icon;
    public Dimensions SlotDimension;
    public int stackSize;
    public int maxStack;
    public int Durability;
    public bool isHorizontal = true;
}

[Serializable]
public struct Dimensions
{
    public int Height;
    public int Width;
}

[CreateAssetMenu(fileName = "New Equipment", menuName = "Data/Equipment")]
public class EquipmentDefinition : Item, IEquippable
{
    public EquipmentSlotType Slot;
    public int Armor;
    public int MaxStorageSpace;// 0 for non-storage items, positive value for bags

    public void Equip(EquipmentManager equipmentManager)
    {
        equipmentManager.EquipItem(this);
    }
}

[CreateAssetMenu(fileName = "New Weapon", menuName = "Data/Weapon")]
public class WeaponDefinition : EquipmentDefinition
{
    public WeaponType Type;
    public int Damage;
}

[CreateAssetMenu(fileName = "New Food", menuName = "Data/Food")]
public class FoodDefinition : Item
{
    public int HealthRestore;
    public int EnergyRestore;
}

[CreateAssetMenu(fileName = "New Health", menuName = "Data/Health")]
public class HealthDefinition : Item
{
    public int HealthRestore;
}
