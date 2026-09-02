using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float heightAbovePath = 0.75f;

    [Header("Attack")]
    [SerializeField] private int attackDamage = 5;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float defenderAttackRange = 2.6f;

    private List<Vector3> path;
    private int currentWaypointIndex;
    private bool hasPath;
    private bool isAttacking;
    private bool hasReachedTower;

    private TowerHealth targetTower;

    public void Initialise(List<Vector3> newPath)
    {
        path = new List<Vector3>(newPath);
        currentWaypointIndex = 0;
        hasPath = path.Count > 0;

        targetTower = FindFirstObjectByType<TowerHealth>();

        if (hasPath)
        {
            transform.position = GetRaisedPosition(path[0]);
        }
    }

    private void Update()
    {
        if (!hasPath || isAttacking || hasReachedTower)
        {
            return;
        }

        DefenderHealth nearbyDefender = FindNearbyDefender();

        if (nearbyDefender != null)
        {
            StartCoroutine(AttackDefender(nearbyDefender));
            return;
        }

        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        Vector3 targetPosition =
            GetRaisedPosition(path[currentWaypointIndex]);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            movementSpeed * Time.deltaTime
        );

        Vector3 movementDirection = targetPosition - transform.position;

        if (movementDirection.sqrMagnitude > 0.001f)
        {
            transform.forward = movementDirection.normalized;
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            MoveToNextWaypoint();
        }
    }

    private void MoveToNextWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= path.Count)
        {
            ReachTower();
        }
    }

    private DefenderHealth FindNearbyDefender()
    {
        DefenderHealth[] defenders = FindObjectsByType<DefenderHealth>(
            FindObjectsSortMode.None
        );

        DefenderHealth nearestDefender = null;
        float nearestDistance = defenderAttackRange;

        foreach (DefenderHealth defender in defenders)
        {
            float distance = Vector3.Distance(
                transform.position,
                defender.transform.position
            );

            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearestDefender = defender;
            }
        }

        return nearestDefender;
    }

    private IEnumerator AttackDefender(DefenderHealth defender)
    {
        isAttacking = true;

        while (defender != null && defender.CurrentHealth > 0)
        {
            float distance = Vector3.Distance(
                transform.position,
                defender.transform.position
            );

            if (distance > defenderAttackRange + 0.5f)
            {
                break;
            }

            transform.LookAt(defender.transform);
            defender.TakeDamage(attackDamage);

            yield return new WaitForSeconds(attackInterval);
        }

        isAttacking = false;
    }

    private void ReachTower()
    {
        hasPath = false;
        hasReachedTower = true;
        StartCoroutine(AttackTower());
    }

    private IEnumerator AttackTower()
    {
        isAttacking = true;

        while (targetTower != null && targetTower.CurrentHealth > 0)
        {
            transform.LookAt(targetTower.transform);
            targetTower.TakeDamage(attackDamage);

            yield return new WaitForSeconds(attackInterval);
        }

        isAttacking = false;
    }

    private Vector3 GetRaisedPosition(Vector3 pathPosition)
    {
        return pathPosition + Vector3.up * heightAbovePath;
    }
}