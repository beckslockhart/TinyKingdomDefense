using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProceduralMapGenerator mapGenerator;
    [SerializeField] private EnemyMovement enemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float firstSpawnDelay = 1f;
    [SerializeField] private float spawnInterval = 2.5f;

    private Coroutine spawningCoroutine;

   
    private void Start()
    {
        spawningCoroutine = StartCoroutine(SpawnEnemies());
    }

   
    private IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

   
    private void SpawnEnemy()
    {
        IReadOnlyList<List<Vector3>> availablePaths =
            mapGenerator.GeneratedPaths;

        if (availablePaths.Count == 0 || enemyPrefab == null)
        {
            return;
        }

        int selectedPathIndex = Random.Range(0, availablePaths.Count);
        List<Vector3> selectedPath = availablePaths[selectedPathIndex];

        EnemyMovement newEnemy = Instantiate(enemyPrefab);
        newEnemy.name = "Goblin";
        newEnemy.Initialise(selectedPath);
    }
}
