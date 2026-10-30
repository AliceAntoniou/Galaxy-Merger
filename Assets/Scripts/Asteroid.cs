using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Asteroid : Unit
{
    private void Start()
    {
        if (team == Team.player)
        {
            gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.green;
        } else
        {
            gameObject. GetComponentInChildren<SpriteRenderer>().color = Color.red;
        }
    }

    private void FixedUpdate()
    {
        DetectTarget();

        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        Vector3 dir = target.position - transform.position;
        if (currentMoveSpeed < 0)
        {
            currentMoveSpeed += acceleration * 5;
        }
        if (currentMoveSpeed < maxSpeed)
        {
            currentMoveSpeed += acceleration;
        }
        transform.position += dir.normalized * currentMoveSpeed * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((1 << collision.gameObject.layer) == GetMask())
        {
            return;
        }

        currentMoveSpeed -= maxSpeed;
        IDamageable target = collision.gameObject.GetComponent<IDamageable>();

        if (target == null) return;

        target.TakeDamage(damage);
    }
}
