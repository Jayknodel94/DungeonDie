using FishNet.Object;
using UnityEngine;

public class PlayerUiController : NetworkBehaviour
{
    public GameObject inventoryGO;
    Inventory inventory;

    private void Awake()
    {
        inventory = new Inventory();
    }

    void Update()
    {
        if (Input.GetKeyDown(Controls.OpenInventory))
        {
            inventoryGO.SetActive(!inventoryGO.activeInHierarchy);
        }
    }
}
