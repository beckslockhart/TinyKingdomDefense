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
    
    [Header("Basic Difficulty Progression")]
    [SerializeField] private int enemiesPerDifficultyIncrease = 8;
    [SerializeField] private int healthIncreasePerLevel = 10;

    private int enemiesSpawned;
    private int currentDifficultyLevel;

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

        currentDifficultyLevel =
            enemiesSpawned / enemiesPerDifficultyIncrease;

        EnemyMovement newEnemy = Instantiate(enemyPrefab);
        newEnemy.name = $"Goblin - Level {currentDifficultyLevel + 1}";
        newEnemy.Initialise(selectedPath);

        EnemyHealth enemyHealth =
            newEnemy.GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {
            int additionalHealth =
                currentDifficultyLevel * healthIncreasePerLevel;

            enemyHealth.IncreaseMaximumHealth(additionalHealth);
        }

        enemiesSpawned++;

        if (enemiesSpawned % enemiesPerDifficultyIncrease == 0)
        {
            Debug.Log(
                $"Goblin difficulty increased! " +
                $"Next level: {currentDifficultyLevel + 2}"
            );
        }
    }
}
