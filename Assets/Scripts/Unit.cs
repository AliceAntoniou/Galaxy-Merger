using UnityEngine;

public class Unit : ATeam, IDamageable
{
    [SerializeField] protected float detectionRange;
    protected Transform target;
    [SerializeField] protected float acceleration;
    [SerializeField] protected float maxSpeed;
    [SerializeField] protected int pv;
    [SerializeField] protected int damage;
    protected float currentMoveSpeed;

    void Start()
    {
        if (!blackhole)
        {
            Debug.LogWarning("Missing blackhole");
        }
        target = blackhole.GetEnemyBlackHole().transform;
    }

    protected void DetectTarget()
    {
        LayerMask mask = blackhole.GetEnemyBlackHole().GetMask();

        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, detectionRange, mask);

        if (cols.Length == 0)
        {
            target = blackhole.GetEnemyBlackHole().transform;
            return;
        }
        if (target == null)
        {
            target = blackhole.GetEnemyBlackHole().transform;
        }
        foreach (Collider2D col in cols)
        {
            Vector3 point = col.ClosestPoint(transform.position);
            if (Vector3.Distance(transform.position, point) < Vector3.Distance(transform.position, target.position))
            {
                target = col.transform;
            }
        }
    }

    public void TakeDamage(int dmg)
    {
        pv -= dmg;
        currentMoveSpeed -= dmg;
        if (pv <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void HealHp(int heal)
    {
        pv += heal;
    }
}
