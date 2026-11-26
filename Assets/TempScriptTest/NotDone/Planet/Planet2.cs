using System.Collections;
using Multi_1;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Planet that produces units. The SERVER does all the spawning (units are network objects created by
/// NetworkEntityManager.ServerSpawnUnit); clients just see the units appear.
///
/// Prefab: NetworkObject + NetworkEntity (Type = the PLANET type, e.g. SlingshotPlanetSpawner) + Planet2 (or a subclass).
/// unitType = the UNIT it produces (Comet, Asteroid, SlingshotPlanet...). That unit's prefab must be listed in
/// NetworkEntityManager.definitions and in the Network Prefabs list.
/// </summary>
[RequireComponent(typeof(NetworkEntity))]
public class Planet2 : NetworkBehaviour
{
    [Tooltip("The UNIT type produced (not the planet type).")]
    [SerializeField] private EntityType unitType = EntityType.None;
    [Tooltip("Seconds between two spawns.")]
    [SerializeField] private float spawnRate = 5f;
    [Tooltip("Where units appear. Defaults to this planet's position.")]
    [SerializeField] private Transform spawnPoint;

    private NetworkEntity entity;
    private Coroutine spawnRoutine;

    private void Awake()
    {
        entity = GetComponent<NetworkEntity>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) StartSpawning();
    }

    public override void OnNetworkDespawn()
    {
        StopSpawning();
    }

    public void StartSpawning()
    {
        if (!IsServer || spawnRoutine != null) return;
        spawnRoutine = StartCoroutine(SpawnUnitRoutine());
    }

    public void StopSpawning()
    {
        if (spawnRoutine == null) return;
        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    private void SpawnUnit()
    {
        if (unitType == EntityType.None || NetworkEntityManager.Instance == null) return;

        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        // TODO (optional): stardust cost per unit, cap on the number of units per player.
        NetworkEntityManager.Instance.ServerSpawnUnit(unitType, position, entity.OwnerSlot.Value);
    }

    private IEnumerator SpawnUnitRoutine()
    {
        var wait = new WaitForSeconds(Mathf.Max(0.05f, spawnRate));
        while (true)
        {
            yield return wait;   // wait FIRST: never spawn inside OnNetworkSpawn (avoids spawn loops)
            SpawnUnit();
        }
    }
}
