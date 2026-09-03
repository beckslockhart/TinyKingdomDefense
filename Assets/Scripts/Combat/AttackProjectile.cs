using UnityEngine;


public class AttackProjectile : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 12f;

    private EnemyHealth targetEnemy;
    private int damage;

    
    public void Initialise(EnemyHealth target, int damageAmount)
    {
        targetEnemy = target;
        damage = damageAmount;
    }

    
    private void Update()
    {
        if (targetEnemy == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition =
            targetEnemy.transform.position + Vector3.up * 0.5f;

        Vector3 direction = targetPosition - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.forward = direction.normalized;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            movementSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.15f)
        {
            targetEnemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
