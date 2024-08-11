using FishNet;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUiController : NetworkBehaviour
{
    public List<Image> inventorySlotImages;
    public GameObject inventoryGO;
    public PlayerController playerController;
    public CombatController combatController;

    public Item itemToDrop = null;

    int maxInventorySpace = 12;
    List<Item> itemsInInventory = new();

    void Update()
    {
        if (Input.GetKeyDown(Controls.OpenInventory))
        {
            inventoryGO.SetActive(!inventoryGO.activeInHierarchy);

            HandleScriptDisabling();
            HandleCursor();
            ArrangeInventory();
        }
    }

    private void ArrangeInventory()
    {
        // Loop through max amount of times
        for (int i = 0; i < maxInventorySpace; i++)
        {
            // Only do this for items we actually have
            if (i < itemsInInventory.Count)
            {
                // enable slot
                inventorySlotImages[i].enabled = true;

                // assign image to sprite
                inventorySlotImages[i].sprite = itemsInInventory[i].icon;

                // turn on x button for slot by getting grandparent of inv slot image :/
                var buttons = inventorySlotImages[i].transform.parent.parent.GetComponentsInChildren<Button>();
                foreach (var button in buttons)
                {
                    button.interactable = true;
                }
            }
            else
            {
                // disable slot
                inventorySlotImages[i].enabled = false;

                // unassign image to sprite
                inventorySlotImages[i].sprite = null;

                // turn off x button for slot by getting grandparent of inv slot image :/
                var buttons = inventorySlotImages[i].transform.parent.parent.GetComponentsInChildren<Button>();
                foreach (var button in buttons)
                {
                    if (button.gameObject.name == "RemoveButton")
                        button.interactable = false;
                }
            }
        }
    }

    private void HandleScriptDisabling()
    {
        playerController.canLook = !playerController.canLook;
        combatController.enabled = !combatController.enabled;
    }

    void HandleCursor()
    {
        if (Cursor.visible)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public int GetRoomInInventory()
    {
        return maxInventorySpace - itemsInInventory.Count;
    }

    public void AddItemToInventory(Item item)
    {
        itemsInInventory.Add(item);
    }

    public void DropItem()
    {
        print(this);

        //InstanceFinder.ServerManager.Spawn(gameObject.GetComponent<Item>().prefabOfSelf);
    }
}
