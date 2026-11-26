using Multi_1;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Network unit (comet, asteroid, slingshot planet...). The SERVER runs everything: health, detection,
/// movement, attacks and death. Clients only receive position/rotation (NetworkTransform) and health (NetworkVariable).
///
/// Prefab: NetworkObject + NetworkEntity (Type = this unit's type) + SpawnableUnit + Rigidbody2D + a NON-trigger
/// Collider2D (+ NetworkTransform, optionally NetworkRigidbody2D). Add it to the Network Prefabs list and to
/// NetworkEntityManager.definitions. Created by NetworkEntityManager.ServerSpawnUnit (never Instantiate it yourself).
///
/// Behaviour (GDD 2.9):
///  - always moves toward the enemy black hole;
///  - faces the closest enemy unit within detectionRange;
///  - contact unit (range = 0): damages enemy units it collides with;
///  - range unit (range > 0): stops to shoot the closest enemy in range, atkSpeed shots per second;
///  - on taking damage, knockback proportional to the damage.
/// </summary>
[RequireComponent(typeof(NetworkEntity), typeof(Rigidbody2D))]
public class SpawnableUnit : NetworkBehaviour, IDamageable
{
    [SerializeField] private UnitSO unitSO;
    [SerializeField] private Collider2D unitCollider;

    [Header("Server tuning")]
    [Tooltip("Layers scanned for targets (units + black holes). Tile triggers are ignored anyway.")]
    [SerializeField] private LayerMask targetMask = ~0;
    [SerializeField] private float scanInterval = 0.2f;
    [SerializeField] private float knockbackPerDamage = 1f;

    // Data copied from the ScriptableObject (read on every peer, only used by the server)
    private EntityType unitType;
    private int maxHealth;
    private int damage;
    private float spawnSpeed;
    private float accelerationSpeed;
    private float maxSpeed;
    private float detectionRange;
    private float range;
    private float atkSpeed;

    public EntityType UnitType => unitType;
    /// <summary>Used by the planet spawner (rate at which this unit is produced).</summary>
    public float SpawnSpeed => spawnSpeed;

    private readonly NetworkVariable<int> health = new();

    private NetworkEntity entity;
    private Rigidbody2D rb;
    private bool isDead;

    private BlackHole2 enemyBlackHole;
    private SpawnableUnit nearestEnemyUnit;
    private float nextScan;
    private float nextAttack;
    private Vector2 lastMoveDir = Vector2.right;
    private readonly Collider2D[] hits = new Collider2D[16];

    private bool IsRanged => range > 0f;
    private int MySlot => entity.OwnerSlot.Value;

    // ------------------------------------------------------------------ setup
    private void Awake()
    {
        entity = GetComponent<NetworkEntity>();
        rb = GetComponent<Rigidbody2D>();
        if (unitCollider == null) unitCollider = GetComponent<Collider2D>();

        rb.gravityScale = 0f;     // top-down
        rb.linearDamping = 0f;

        unitType = unitSO.unitType;
        maxHealth = unitSO.health;
        damage = unitSO.damage;
        spawnSpeed = unitSO.spawnSpeed;
        accelerationSpeed = unitSO.accelerationSpeed;
        maxSpeed = unitSO.maxSpeed;
        detectionRange = unitSO.detectionRange;
        range = unitSO.range;
        atkSpeed = unitSO.atkSpeed;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            health.Value = maxHealth;
            if (entity.Type != unitType)
                Debug.LogWarning($"[Unit] NetworkEntity.Type ({entity.Type}) differs from UnitSO.unitType ({unitType}) on {name}");
        }
        else
        {
            // Clients never simulate physics: they just follow the synced transform.
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    // ------------------------------------------------------------------ server loop
    private void FixedUpdate()
    {
        if (!IsServer || isDead) return;

        if (Time.time >= nextScan)
        {
            nextScan = Time.time + scanInterval;
            ScanForEnemyUnit();
            ResolveEnemyBlackHole();               // re-evaluated regularly: matters with more than one enemy
        }
        if (enemyBlackHole == null) ResolveEnemyBlackHole(); // current target gone -> pick another one now

        Vector2 pos = rb.position;

        // --- attack (range units) ---
        IDamageable attackTarget = IsRanged ? GetAttackTarget(pos) : null;
        if (attackTarget != null && Time.time >= nextAttack)
        {
            nextAttack = Time.time + 1f / Mathf.Max(0.01f, atkSpeed);
            attackTarget.TakeDamage(damage);       // TODO: projectile instead of an instant hit
            Debug.Log($"Attack target HP: {attackTarget.GetHp()}");
            if (isDead) return;
        }

        // --- movement: toward the enemy black hole, range units stop while they have a target ---
        Vector2 moveDir = Vector2.zero;
        if (enemyBlackHole != null)
        {
            Vector2 toBlackHole = (Vector2)enemyBlackHole.transform.position - pos;
            if (toBlackHole.sqrMagnitude > 0.0001f) moveDir = toBlackHole.normalized;
        }

        if (attackTarget != null)
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, accelerationSpeed);
        }
        else
        {
            // acceleration is applied once per physics step
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity + moveDir * accelerationSpeed, maxSpeed);
            if (moveDir != Vector2.zero) lastMoveDir = moveDir;
        }

        // --- facing: closest enemy unit in detection range, else the direction we travel ---
        Vector2 faceDir = nearestEnemyUnit != null
            ? nearestEnemyUnit.rb.position - pos
            : (moveDir != Vector2.zero ? moveDir : lastMoveDir);

        rb.angularVelocity = 0f;
        rb.rotation = Mathf.Atan2(faceDir.y, faceDir.x) * Mathf.Rad2Deg; // assumes the sprite looks to the right (+X)
    }

    // ------------------------------------------------------------------ targeting
    private void ScanForEnemyUnit()
    {
        nearestEnemyUnit = null;
        float best = float.MaxValue;

        var filter = new ContactFilter2D { useLayerMask = true, layerMask = targetMask, useTriggers = false };
        int count = Physics2D.OverlapCircle(rb.position, detectionRange, filter, hits);

        for (int i = 0; i < count; i++)
        {
            var unit = hits[i].GetComponentInParent<SpawnableUnit>();
            if (unit == null || unit == this || unit.isDead || unit.MySlot == MySlot) continue;

            float d = (unit.rb.position - rb.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                nearestEnemyUnit = unit;
            }
        }
    }

    /// <summary>
    /// Targets the closest black hole that is not ours. With several enemies we keep the current one
    /// unless another is clearly closer (20%), so a unit between two black holes does not keep switching.
    /// </summary>
    private void ResolveEnemyBlackHole()
    {
        BlackHole2 closest = null;
        float best = float.MaxValue;

        foreach (var b in BlackHole2.All)
        {
            if (b.Slot.Value == MySlot) continue;
            float d = ((Vector2)b.transform.position - rb.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                closest = b;
            }
        }

        if (enemyBlackHole != null && enemyBlackHole.Slot.Value != MySlot && closest != enemyBlackHole)
        {
            float current = ((Vector2)enemyBlackHole.transform.position - rb.position).sqrMagnitude;
            if (best > current * 0.64f) return; // 0.8^2: switch only if the new one is >= 20% closer
        }

        enemyBlackHole = closest;
    }

    /// <summary>Closest enemy unit within attack range, otherwise the enemy black hole if it is in range.</summary>
    private IDamageable GetAttackTarget(Vector2 pos)
    {
        if (nearestEnemyUnit != null && DistanceTo(nearestEnemyUnit.unitCollider, pos) <= range)
            return nearestEnemyUnit;

        if (enemyBlackHole != null && DistanceTo(enemyBlackHole.GetComponent<Collider2D>(), pos) <= range)
            return enemyBlackHole;

        return null;
    }

    private static float DistanceTo(Collider2D col, Vector2 from)
    {
        if (col == null) return float.MaxValue;
        return Vector2.Distance(from, col.ClosestPoint(from)); // distance to the edge, not the center
    }

    // ------------------------------------------------------------------ contact damage
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer || isDead || IsRanged) return;

        // Hits on a black hole are resolved by BlackHole2.OnCollisionEnter2D (avoid applying damage twice).
        if (collision.gameObject.GetComponentInParent<BlackHole2>() != null) return;

        var other = collision.gameObject.GetComponentInParent<SpawnableUnit>();
        if (other == null || other.MySlot == MySlot) return;

        other.TakeDamage(damage);
    }

    // ------------------------------------------------------------------ IDamageable (server only)
    public void TakeDamage(int dmg)
    {
        if (!IsServer || isDead || dmg <= 0) return;

        health.Value = Mathf.Max(0, health.Value - dmg);

        // GDD: knockback equal to the damage (pushed back against our travel direction)
        rb.AddForce(-lastMoveDir * (dmg * knockbackPerDamage), ForceMode2D.Impulse);

        if (health.Value <= 0) Die();
    }

    public void HealHp(int heal)
    {
        if (!IsServer || isDead) return;
        health.Value = Mathf.Min(maxHealth, health.Value + heal);
    }

    public int GetHp()
    {
        return health.Value;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // TODO: reward, visual effect (do it in OnNetworkDespawn so every machine plays it)
        if (NetworkObject.IsSpawned) NetworkObject.Despawn(true);
    }

    private void OnDrawGizmosSelected()
    {
        if (unitSO == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, unitSO.detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, unitSO.range);
    }
}
