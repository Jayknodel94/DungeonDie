using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    List<Item> itemList;

    public Inventory()
    {
        itemList = new List<Item>();
    }

    public void AddItem(Item item)
    {
        itemList.Add(item);
    }
}
