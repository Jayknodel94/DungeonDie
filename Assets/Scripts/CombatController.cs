using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class CombatController : NetworkBehaviour
{
    private readonly SyncVar<int> health = new(100);
    public int damage = 30;
    public int damageHeavy = 50;
    public float attackRange = 1f;

    public LayerMask enemyLayer;

    Animator animator;

    readonly int maxHealth = 100;

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
            Attack(damage);
        }
        else if (Input.GetMouseButtonDown(Controls.MeleeHeavy))
        {
            AnimateMeleeServer(gameObject, "meleeHeavy");
            Attack(damageHeavy);
        }
    }

    public void Attack(int damage)
    {
        // Detect enemies in range of the attack
        Collider[] hitColliders = new Collider[10];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, attackRange, hitColliders, enemyLayer);

        // Damage them
        for (int i = 0; i < numColliders; i++)
        {
            Collider enemy = hitColliders[i];
            enemy.TryGetComponent(out Enemy enemyScript);
            enemyScript.UpdateHealthServer(enemy.gameObject, -damage);
        }
    }

    [ServerRpc]
    public void AnimateMeleeServer(GameObject player, string meleeType)
    {
        // Don't try to trigger animation if already triggered
        if (animator.GetBool("melee") || animator.GetBool("meleeHeavy")) return;

        AnimateMelee(player, meleeType);
    }

    [ObserversRpc]
    void AnimateMelee(GameObject player, string meleeType)
    {
        player.GetComponent<Animator>().SetTrigger(meleeType);
    }

    [ServerRpc]
    public void UpdateHealthServer(CombatController cc, int amountToChange)
    {
        cc.health.Value += amountToChange;
    }
}
