using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    private readonly SyncVar<int> health = new(100);

    int maxHealth = 100;

    [ServerRpc]
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
