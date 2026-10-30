using System.Collections;
using UnityEngine;

public class SlingshotPlanet : Unit, IPlanet
{
    [SerializeField] private float range;
    [SerializeField] private float atkSpeed;
    private float timer;
    void Start()
    {
    }

    private void MoveTowardsTarget()
    {
        Vector3 dir = target.position - transform.position;

        if (currentMoveSpeed < maxSpeed)
        {
            currentMoveSpeed += acceleration;
        }
        transform.position += dir.normalized * currentMoveSpeed * Time.deltaTime;
    }

    private void Attack()
    {
        IDamageable t = target.gameObject.GetComponent<IDamageable>();
        if (t == null) return;
        
        t.TakeDamage(damage);
    }

    void FixedUpdate()
    {
        DetectTarget();
        timer += Time.deltaTime;

        if (Vector2.Distance(target.position, transform.position) > range)
        {
            MoveTowardsTarget();
        } else
        {
            if (timer > atkSpeed)
            {
                Attack();
                timer = 0;
            }
        }
    }
}
