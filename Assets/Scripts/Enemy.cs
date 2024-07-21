using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : NetworkBehaviour
{
    private readonly SyncVar<int> health = new(100);

    readonly int maxHealth = 100;

    NavMeshAgent agent;
    Animator animator;
    List<GameObject> players = new();

    public LayerMask groundMask, playerMask;

    // patrolling
    Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;
    public float timeBetweenPatrols = 2f;

    // chase
    float closestPlayerDistance = Mathf.Infinity;
    Transform closestPlayer = null;

    // attacking
    public float timeBetweenAttacks;
    public int attackDamage = 20;
    public int heavyAttackDamage = 30;
    bool alreadyAttacked;

    // states
    public float sightRange, attackRange;
    bool playerInSightRange, playerInAttackRange;

    float timer = 0f;

    public void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // To make sure agent doesn't go closer than the attack range
        agent.stoppingDistance = attackRange;
    }

    private void Update()
    {
        // check for sight and attack range
        playerInSightRange = CheckIfPlayerIsInSight();
        playerInAttackRange = CheckIfPlayerIsInAttackRange();

        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInSightRange && playerInAttackRange) AttackPlayer();

        AnimateMovement();
    }

    private bool CheckIfPlayerIsInSight()
    {
        int maxColliders = 10;
        Collider[] hitColliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, sightRange, hitColliders);

        // Line of sight logic
        foreach (Collider collider in hitColliders)
        {
            if (collider == null) continue;

            if (collider.CompareTag("Player"))
            {
                // Calculate direction towards the player
                Vector3 direction = collider.bounds.center - transform.position;

                Debug.DrawRay(transform.position, direction);

                // Perform raycast to detect obstacles or the player
                bool hasSightline = Physics.Raycast(transform.position, direction, out RaycastHit hit, sightRange);

                if (hit.collider == null) continue;

                // Check if the ray hits the player and there are no obstacles in between
                if (hasSightline && hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool CheckIfPlayerIsInAttackRange()
    {
        if (Physics.CheckSphere(transform.position, attackRange, playerMask))
        {
            return true;
        }
        else return false;
    }

    private void AnimateMovement()
    {
        float speedPercent = agent.velocity.magnitude / agent.speed;
        animator.SetFloat("speed", speedPercent, .1f, Time.deltaTime);
    }

    void Patrolling()
    {
        // START: Makes sure it looks for closest player all the time (probably not the best)
        if (closestPlayer != null)
        {
            walkPoint = closestPlayer.transform.position;
        }

        closestPlayerDistance = Mathf.Infinity;
        closestPlayer = null;
        // END -----------

        SetWalkpointServer();

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        // walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f && walkPointSet)
        {
            timer += Time.deltaTime;

            if (timer >= timeBetweenPatrols)
            {
                walkPointSet = false;
                timer = 0f;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetWalkpointServer()
    {
        if (!walkPointSet)
        {
            SearchWalkPoint();
        }

        if (walkPointSet)
        {
            TellEnemyWhereToGoServer(walkPoint);
        }
    }

    void SearchWalkPoint()
    {
        // calculate random point in range
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        float randomZ = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, groundMask))
            walkPointSet = true;
    }

    void ChasePlayer()
    {
        // if no closest player yet, find him
        if (closestPlayerDistance == Mathf.Infinity)
        {
            // Detect enemies in range of the attack
            Collider[] hitColliders = new Collider[10];
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, sightRange, hitColliders, playerMask);

            for (int i = 0; i < numColliders; i++)
            {
                Collider player = hitColliders[i];

                // determine distance
                float distance = Vector3.Distance(player.transform.position, transform.position);
                if (distance < closestPlayerDistance)
                {
                    closestPlayerDistance = distance;
                    closestPlayer = player.transform;
                }
            }
        }

        TellEnemyWhereToGoServer(closestPlayer.position);
    }

    void AttackPlayer()
    {
        closestPlayerDistance = Mathf.Infinity;

        // if no closest player yet, find him
        if (closestPlayer == null)
        {
            // Detect enemies in range of the attack
            Collider[] hitColliders = new Collider[10];
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, attackRange, hitColliders, playerMask);

            for (int i = 0; i < numColliders; i++)
            {
                Collider player = hitColliders[i];

                // determine distance
                float distance = Vector3.Distance(player.transform.position, transform.position);
                if (distance < closestPlayerDistance)
                {
                    closestPlayerDistance = distance;
                    closestPlayer = player.transform;
                }
            }
        }

        transform.LookAt(closestPlayer);

        // Do the attack!
        if (!alreadyAttacked)
        {
            alreadyAttacked = true;

            // Attack animation and damage
            DecideAttackType();

            // Rest? between attacks
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void DecideAttackType()
    {
        float randomNum = Random.Range(0f, 1f);

        switch (randomNum)
        {
            case > .25f: // light attack
                AnimateMeleeServer("melee");
                closestPlayer.GetComponent<CombatController>().UpdateHealthServer(-attackDamage);
                break;
            default: // heavy attack
                AnimateMeleeServer("meleeHeavy");
                closestPlayer.GetComponent<CombatController>().UpdateHealthServer(-heavyAttackDamage);
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
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

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void TellEnemyWhereToGoServer(Vector3 position)
    {
        agent.SetDestination(position);
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void UpdateHealthServer(int amountToChange)
    {
        UpdateHealth(amountToChange);
    }

    [ObserversRpc]
    void UpdateHealth(int amountToChange)
    {
        health.Value += amountToChange;

        print($"{this.name}'s health: {health.Value}");

        if (health.Value <= 0)
        {
            DespawnEnemy(gameObject);
        }
    }

    // Enemy dies
    [ServerRpc(RequireOwnership = false)]
    public void DespawnEnemy(GameObject enemy)
    {
        ServerManager.Despawn(enemy);
    }

    // Add players who join
    public override void OnSpawnServer(NetworkConnection connection)
    {
        base.OnSpawnServer(connection);

        players = GameObject.FindGameObjectsWithTag("Player").ToList();
    }

    // Get rid of players who left
    public override void OnDespawnServer(NetworkConnection connection)
    {
        base.OnDespawnServer(connection);

        players = GameObject.FindGameObjectsWithTag("Player").ToList();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
