using UnityEngine;

public enum ProjectileEffectType
{
    Normal,
    AreaDamage,
    Slow
}

public class AttackProjectile : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 12f;

    private EnemyHealth targetEnemy;
    private int damage;
    private ProjectileEffectType effectType;

    private float areaRadius;
    private float slowMultiplier;
    private float slowDuration;

    // This keeps existing Archer and Castle projectiles working.
    public void Initialise(EnemyHealth target, int damageAmount)
    {
        Initialise(
            target,
            damageAmount,
            ProjectileEffectType.Normal,
            0f,
            1f,
            0f
        );
    }

    public void Initialise(
        EnemyHealth target,
        int damageAmount,
        ProjectileEffectType newEffectType,
        float newAreaRadius,
        float newSlowMultiplier,
        float newSlowDuration
    )
    {
        targetEnemy = target;
        damage = damageAmount;
        effectType = newEffectType;

        areaRadius = newAreaRadius;
        slowMultiplier = newSlowMultiplier;
        slowDuration = newSlowDuration;
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
            ApplyProjectileEffect();
            Destroy(gameObject);
        }
    }

    private void ApplyProjectileEffect()
    {
        if (targetEnemy == null)
        {
            return;
        }

        switch (effectType)
        {
            case ProjectileEffectType.AreaDamage:
                ApplyAreaDamage();
                break;

            case ProjectileEffectType.Slow:
                ApplySlowEffect();
                break;

            default:
                targetEnemy.TakeDamage(damage);
                break;
        }
    }

    private void ApplyAreaDamage()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        Vector3 impactPosition = targetEnemy.transform.position;

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null)
            {
                continue;
            }

            float distance = Vector3.Distance(
                impactPosition,
                enemy.transform.position
            );

            if (distance <= areaRadius)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    private void ApplySlowEffect()
    {
        targetEnemy.TakeDamage(damage);

        EnemyMovement enemyMovement =
            targetEnemy.GetComponent<EnemyMovement>();

        if (enemyMovement != null)
        {
            enemyMovement.ApplySlow(
                slowMultiplier,
                slowDuration
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (effectType != ProjectileEffectType.AreaDamage)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, areaRadius);
    }
}