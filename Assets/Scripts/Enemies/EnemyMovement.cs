using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyBehaviourType
{
    Standard,
    Runner,
    Brute
}

public class EnemyMovement : MonoBehaviour
{
    [Header("Enemy Type")]
    [SerializeField] private EnemyBehaviourType behaviourType =
        EnemyBehaviourType.Standard;

    [Header("Movement")]
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float heightAbovePath = 0.75f;

    [Header("Attack")]
    [SerializeField] private int attackDamage = 5;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float defenderAttackRange = 2.6f;

    [Header("Runner Sprint")]
    [SerializeField] private float sprintSpeedMultiplier = 1.8f;
    [SerializeField] private float sprintDuration = 1.2f;
    [SerializeField] private float sprintCooldown = 3.5f;

    [Header("Brute Enrage")]
    [SerializeField] private float enrageHealthPercentage = 0.5f;
    [SerializeField] private float enragedSpeedMultiplier = 1.35f;
    [SerializeField] private float enragedAttackSpeedMultiplier = 1.5f;

    private List<Vector3> path;
    private int currentWaypointIndex;
    private bool hasPath;
    private bool isAttacking;
    private bool hasReachedTower;

    private bool isSprinting;
    private float sprintCooldownTimer;

    private float slowMultiplier = 1f;
    private Coroutine slowCoroutine;

    private TowerHealth targetTower;
    private EnemyHealth enemyHealth;

    public EnemyBehaviourType BehaviourType => behaviourType;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        sprintCooldownTimer = sprintCooldown;
    }

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

        UpdateRunnerSprint();

        DefenderHealth nearbyDefender = FindNearbyDefender();

        if (nearbyDefender != null)
        {
            StartCoroutine(AttackDefender(nearbyDefender));
            return;
        }

        MoveAlongPath();
    }

    private void UpdateRunnerSprint()
    {
        if (behaviourType != EnemyBehaviourType.Runner || isSprinting)
        {
            return;
        }

        sprintCooldownTimer -= Time.deltaTime;

        if (sprintCooldownTimer <= 0f)
        {
            StartCoroutine(Sprint());
        }
    }

    private IEnumerator Sprint()
    {
        isSprinting = true;

        yield return new WaitForSeconds(sprintDuration);

        isSprinting = false;
        sprintCooldownTimer = sprintCooldown;
    }

    private void MoveAlongPath()
    {
        Vector3 targetPosition =
            GetRaisedPosition(path[currentWaypointIndex]);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            GetCurrentMovementSpeed() * Time.deltaTime
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

    private float GetCurrentMovementSpeed()
    {
        float currentSpeed = movementSpeed;

        if (behaviourType == EnemyBehaviourType.Runner && isSprinting)
        {
            currentSpeed *= sprintSpeedMultiplier;
        }

        if (behaviourType == EnemyBehaviourType.Brute && IsEnraged())
        {
            currentSpeed *= enragedSpeedMultiplier;
        }

        return currentSpeed * slowMultiplier;
    }

    private float GetCurrentAttackInterval()
    {
        if (behaviourType == EnemyBehaviourType.Brute && IsEnraged())
        {
            return attackInterval / enragedAttackSpeedMultiplier;
        }

        return attackInterval;
    }

    private bool IsEnraged()
    {
        if (enemyHealth == null || enemyHealth.MaximumHealth <= 0)
        {
            return false;
        }

        float healthPercentage =
            (float)enemyHealth.CurrentHealth / enemyHealth.MaximumHealth;

        return healthPercentage <= enrageHealthPercentage;
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

            yield return new WaitForSeconds(GetCurrentAttackInterval());
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

            yield return new WaitForSeconds(GetCurrentAttackInterval());
        }

        isAttacking = false;
    }

    public void ApplySlow(float movementMultiplier, float duration)
    {
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }

        slowCoroutine = StartCoroutine(
            ApplySlowCoroutine(movementMultiplier, duration)
        );
    }

    private IEnumerator ApplySlowCoroutine(
        float movementMultiplier,
        float duration
    )
    {
        slowMultiplier = Mathf.Clamp(movementMultiplier, 0.1f, 1f);

        yield return new WaitForSeconds(duration);

        slowMultiplier = 1f;
        slowCoroutine = null;
    }

    private Vector3 GetRaisedPosition(Vector3 pathPosition)
    {
        return pathPosition + Vector3.up * heightAbovePath;
    }
}