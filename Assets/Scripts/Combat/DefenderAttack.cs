using System.Collections;
using UnityEngine;


public class DefenderAttack : MonoBehaviour
{
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackInterval = 1.2f;

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
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
