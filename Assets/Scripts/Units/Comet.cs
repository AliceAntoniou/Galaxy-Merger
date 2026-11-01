using UnityEngine;

public class Comet : Unit
{
    private float timer;
    void Start()
    {
        DetectTarget();
    }

    private void FixedUpdate()
    {
        timer += Time.deltaTime;

        if (timer > 2)
        {
            DetectTarget();
            timer = 0;
        }

        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        Vector3 dir = target.position - transform.position;
        if (currentMoveSpeed < 0)
        {
            currentMoveSpeed += acceleration * 9;
        }
        else if (currentMoveSpeed < maxSpeed)
        {
            currentMoveSpeed += acceleration;
        }
        if (currentMoveSpeed > maxSpeed) currentMoveSpeed = maxSpeed;
        transform.position += dir.normalized * currentMoveSpeed * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((1 << collision.gameObject.layer) == GetMask())
        {
            return;
        }

        IDamageable target = collision.gameObject.GetComponent<IDamageable>();

        if (target == null) return;

        target.TakeDamage(damage);
    }
}
