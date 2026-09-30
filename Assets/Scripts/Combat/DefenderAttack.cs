using System.Collections;
using UnityEngine;

public enum DefenderAttackType
{
    Archer,
    Cannon,
    Frost
}

public class DefenderAttack : MonoBehaviour
{
    [Header("Defender Type")]
    [SerializeField] private DefenderAttackType defenderType =
        DefenderAttackType.Archer;

    [Header("Attack")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackInterval = 1.2f;
    [SerializeField] private AttackProjectile projectilePrefab;
    [SerializeField] private float projectileSpawnHeight = 1.2f;

    [Header("Cannon Settings")]
    [SerializeField] private float areaDamageRadius = 2.5f;

    [Header("Frost Settings")]
    [Range(0.1f, 1f)]
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float slowDuration = 2.5f;

    public DefenderAttackType DefenderType => defenderType;

    private void Start()
    {
        StartCoroutine(AttackContinuously());
    }

    private IEnumerator AttackContinuously()
    {
        while (true)
        {
            EnemyHealth targetEnemy = FindTarget();

            if (targetEnemy != null)
            {
                FireProjectile(targetEnemy);
            }

            yield return new WaitForSeconds(attackInterval);
        }
    }

    private void FireProjectile(EnemyHealth targetEnemy)
    {
        if (projectilePrefab == null)
        {
            ApplyInstantAttack(targetEnemy);
            return;
        }

        Vector3 spawnPosition =
            transform.position + Vector3.up * projectileSpawnHeight;

        AttackProjectile newProjectile = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        ProjectileEffectType effectType =
            GetProjectileEffectType();

        newProjectile.Initialise(
            targetEnemy,
            attackDamage,
            effectType,
            areaDamageRadius,
            slowMultiplier,
            slowDuration
        );
    }

    private ProjectileEffectType GetProjectileEffectType()
    {
        switch (defenderType)
        {
            case DefenderAttackType.Cannon:
                return ProjectileEffectType.AreaDamage;

            case DefenderAttackType.Frost:
                return ProjectileEffectType.Slow;

            default:
                return ProjectileEffectType.Normal;
        }
    }

    private void ApplyInstantAttack(EnemyHealth targetEnemy)
    {
        if (targetEnemy == null)
        {
            return;
        }

        switch (defenderType)
        {
            case DefenderAttackType.Cannon:
                ApplyInstantAreaDamage(targetEnemy.transform.position);
                break;

            case DefenderAttackType.Frost:
                targetEnemy.TakeDamage(attackDamage);

                EnemyMovement movement =
                    targetEnemy.GetComponent<EnemyMovement>();

                if (movement != null)
                {
                    movement.ApplySlow(
                        slowMultiplier,
                        slowDuration
                    );
                }

                break;

            default:
                targetEnemy.TakeDamage(attackDamage);
                break;
        }
    }

    private void ApplyInstantAreaDamage(Vector3 impactPosition)
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

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

            if (distance <= areaDamageRadius)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    private EnemyHealth FindTarget()
    {
        if (defenderType == DefenderAttackType.Cannon)
        {
            return FindBestCannonTarget();
        }

        return FindNearestEnemy();
    }

    private EnemyHealth FindNearestEnemy()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        EnemyHealth nearestEnemy = null;
        float nearestDistance = attackRange;

        foreach (EnemyHealth enemy in enemies)
        {
            float distance = Vector3.Distance(
                transform.position,
                enemy.transform.position
            );

            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    private EnemyHealth FindBestCannonTarget()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        EnemyHealth bestTarget = null;
        int largestNearbyGroup = -1;
        float closestDistance = attackRange;

        foreach (EnemyHealth possibleTarget in enemies)
        {
            float distanceFromDefender = Vector3.Distance(
                transform.position,
                possibleTarget.transform.position
            );

            if (distanceFromDefender > attackRange)
            {
                continue;
            }

            int nearbyEnemyCount = 0;

            foreach (EnemyHealth otherEnemy in enemies)
            {
                float distanceFromTarget = Vector3.Distance(
                    possibleTarget.transform.position,
                    otherEnemy.transform.position
                );

                if (distanceFromTarget <= areaDamageRadius)
                {
                    nearbyEnemyCount++;
                }
            }

            if (nearbyEnemyCount > largestNearbyGroup ||
                (nearbyEnemyCount == largestNearbyGroup &&
                 distanceFromDefender < closestDistance))
            {
                largestNearbyGroup = nearbyEnemyCount;
                closestDistance = distanceFromDefender;
                bestTarget = possibleTarget;
            }
        }

        return bestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        switch (defenderType)
        {
            case DefenderAttackType.Cannon:
                Gizmos.color = Color.red;
                break;

            case DefenderAttackType.Frost:
                Gizmos.color = Color.cyan;
                break;

            default:
                Gizmos.color = Color.blue;
                break;
        }

        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}