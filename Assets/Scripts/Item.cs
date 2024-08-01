using UnityEngine;

public class Item
{
    public enum ItemType
    {
        Sword,
        Shield,
        HealthPotion,
        ManaPotion,
        Coin,
        Medkit,
    }

    public ItemType itemType;
    public int amount;
}
