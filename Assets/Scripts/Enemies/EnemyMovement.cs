using System.Collections.Generic;
using UnityEngine;


public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float heightAbovePath = 0.75f;

    private List<Vector3> path;
    private int currentWaypointIndex;
    private bool hasPath;

    
    public void Initialise(List<Vector3> newPath)
    {
        path = new List<Vector3>(newPath);
        currentWaypointIndex = 0;
        hasPath = path.Count > 0;

        if (hasPath)
        {
            transform.position = GetRaisedPosition(path[0]);
        }
    }

   
    private void Update()
    {
        if (!hasPath)
        {
            return;
        }

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

    
    private void ReachTower()
    {
        hasPath = false;

        // The enemy will attack the tower here
        Destroy(gameObject);
    }

    
    private Vector3 GetRaisedPosition(Vector3 pathPosition)
    {
        return pathPosition + Vector3.up * heightAbovePath;
    }
}