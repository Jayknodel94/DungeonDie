using FishNet.Object;
using UnityEngine;

public class PlayerUiController : NetworkBehaviour
{
    public GameObject inventoryGO;
    public PlayerController playerController;
    public CombatController combatController;

    void Update()
    {
        if (Input.GetKeyDown(Controls.OpenInventory))
        {
            inventoryGO.SetActive(!inventoryGO.activeInHierarchy);

            HandleScriptDisabling();
            HandleCursor();
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
}
