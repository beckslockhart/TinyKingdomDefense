using System.Collections;
using UnityEngine;


public class TowerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private int attackDamage = 15;
    [SerializeField] private float attackInterval = 1f;

    
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
                nearestEnemy.TakeDamage(attackDamage);
            }

            yield return new WaitForSeconds(attackInterval);
        }
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
