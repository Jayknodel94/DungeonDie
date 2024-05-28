using FishNet.Object;
using UnityEngine;

public class CombatController : NetworkBehaviour
{
    Animator animator;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            // nothing yet
        }
        else
        {
            GetComponent<CombatController>().enabled = false;
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(Controls.Melee))
        {
            AnimateMeleeServer(gameObject, "melee");
        }
        else if (Input.GetMouseButtonDown(Controls.MeleeHeavy))
        {
            AnimateMeleeServer(gameObject, "meleeHeavy");
        }
    }

    [ServerRpc]
    public void AnimateMeleeServer(GameObject player, string meleeType)
    {
        AnimateMelee(player, meleeType);
    }

    [ObserversRpc]
    void AnimateMelee(GameObject player, string meleeType)
    {
        player.GetComponent<Animator>().SetTrigger(meleeType);
    }
}
