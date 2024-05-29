using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : NetworkBehaviour
{
    private readonly SyncVar<int> health = new(100);

    int maxHealth = 100;

    NavMeshAgent agent;
    List<Transform> players = new();

    public LayerMask groundMask, playerMask;

    // patrolling
    Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    // states
    public float sightRange, attackRange;
    bool playerInSightRange, playerInAttackRange;

    public void Awake()
    {
        List<GameObject> playersGO = GameObject.FindGameObjectsWithTag("Player").ToList();
        foreach (var player in playersGO)
        {
            players.Add(player.transform);
        }

        agent = GetComponent<NavMeshAgent>();

        // CHANGE TO GATHER PLAYERS WHEN THEY CONNECT
    }

    private void Update()
    {
        // check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerMask);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerMask);

        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (!playerInSightRange && !playerInAttackRange) ChasePlayer(players[0]); // TEMP!
        if (!playerInSightRange && !playerInAttackRange) AttackPlayer(players[0]); // TEMP!
    }

    void Patrolling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        // walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        // calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, groundMask))
            walkPointSet = true;
    }

    void ChasePlayer(Transform playerInRange)
    {
        agent.SetDestination(playerInRange.position);
    }

    void AttackPlayer(Transform playerInRange)
    {
        // make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(playerInRange);

        if (!alreadyAttacked)
        {
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void UpdateHealthServer(GameObject enemy, int amountToChange)
    {
        UpdateHealth(enemy, amountToChange);
    }

    [ObserversRpc]
    void UpdateHealth(GameObject enemy, int amountToChange)
    {
        enemy.GetComponent<Enemy>().health.Value += amountToChange;

        if (enemy.GetComponent<Enemy>().health.Value <= 0)
        {
            DespawnEnemy(enemy);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnEnemy(GameObject enemy)
    {
        ServerManager.Despawn(enemy);
    }
}
