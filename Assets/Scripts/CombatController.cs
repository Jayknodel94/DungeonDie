using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class CombatController : NetworkBehaviour
{
    private readonly SyncVar<int> health = new(100);
    public int damage = 30;
    public int damageHeavy = 50;
    public float attackRange = 1f;
    public float timeBetweenAttacks = 1f;

    public LayerMask enemyLayer;

    Animator animator;

    bool alreadyAttacked = false;

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
            AnimateMeleeServer("melee");
            Attack(damage);
        }
        else if (Input.GetMouseButtonDown(Controls.MeleeHeavy))
        {
            AnimateMeleeServer("meleeHeavy");
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
            // Do the attack!
            if (!alreadyAttacked)
            {
                alreadyAttacked = true;

                Collider enemy = hitColliders[i];
                enemy.TryGetComponent(out Enemy enemyScript);
                enemyScript.UpdateHealthServer(-damage);

                // Rest? between attacks
                Invoke(nameof(ResetAttack), timeBetweenAttacks);
            }
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    [ServerRpc]
    public void AnimateMeleeServer(string meleeType)
    {
        // Don't try to trigger animation if already triggered
        if (animator.GetBool("melee") || animator.GetBool("meleeHeavy")) return;

        AnimateMelee(meleeType);
    }

    [ObserversRpc]
    void AnimateMelee(string meleeType)
    {
        animator.SetTrigger(meleeType);
    }

    [ServerRpc]
    public void UpdateHealthServer(int amountToChange)
    {
        health.Value += amountToChange;

        print($"{this.name}'s health: {health.Value}");

        if (health.Value <= 0)
        {
            DespawnPlayer(gameObject);
        }
    }

    // Player dies
    [ServerRpc(RequireOwnership = false)]
    public void DespawnPlayer(GameObject player)
    {
        ServerManager.Despawn(player);
    }
}
