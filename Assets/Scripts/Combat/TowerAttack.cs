using System.Collections;
using UnityEngine;


public class TowerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 3.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private AttackProjectile projectilePrefab;

    
    private void Start()
    {
        StartCoroutine(AttackContinuously());
    }

   
    private IEnumerator AttackContinuously()
    {
        while (true)
        {
            EnemyHealth nearestEnemy = FindNearestEnemy();

            if (nearestEnemy != null)
            {
                FireProjectile(nearestEnemy);
            }

            yield return new WaitForSeconds(attackInterval);
        }
    }

    public void SetProjectilePrefab(AttackProjectile newProjectilePrefab)
    {
        projectilePrefab = newProjectilePrefab;
    }

    
    private void FireProjectile(EnemyHealth targetEnemy)
    {
        if (projectilePrefab == null)
        {
            targetEnemy.TakeDamage(attackDamage);
            return;
        }

        Vector3 spawnPosition =
            transform.position + Vector3.up * 3f;

        AttackProjectile newProjectile = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        newProjectile.Initialise(targetEnemy, attackDamage);
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

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
