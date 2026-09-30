using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProceduralMapGenerator mapGenerator;
    [SerializeField] private EnemyMovement standardGoblinPrefab;
    [SerializeField] private EnemyMovement runnerGoblinPrefab;
    [SerializeField] private EnemyMovement bruteGoblinPrefab;

    [Header("Wave Settings")]
    [SerializeField] private float firstWaveDelay = 2f;
    [SerializeField] private float timeBetweenWaves = 8f;
    [SerializeField] private float baseSpawnInterval = 1.5f;
    [SerializeField] private float minimumSpawnInterval = 0.65f;

    [Header("Difficulty Budget")]
    [SerializeField] private int startingWaveBudget = 6;
    [SerializeField] private int budgetIncreasePerWave = 3;
    [SerializeField] private int standardGoblinCost = 1;
    [SerializeField] private int runnerGoblinCost = 1;
    [SerializeField] private int bruteGoblinCost = 3;

    [Header("Enemy Unlock Waves")]
    [SerializeField] private int runnerUnlockWave = 2;
    [SerializeField] private int bruteUnlockWave = 3;

    [Header("Health Scaling")]
    [SerializeField] private int wavesPerHealthIncrease = 3;
    [SerializeField] private int healthIncreaseAmount = 10;

    [Header("Adaptive Difficulty")]
    [SerializeField] private float adaptiveDifficultyMultiplier = 1f;
    [SerializeField] private float minimumDifficultyMultiplier = 0.8f;
    [SerializeField] private float maximumDifficultyMultiplier = 1.4f;
    [SerializeField] private float difficultyAdjustment = 0.1f;
    [SerializeField] private float fastWaveCompletionTime = 30f;

    private int currentWave;
    private int activeEnemies;
    private int previousPathIndex = -1;

    private float waveStartTime;
    private float timeUntilNextWave;
    private int startingCastleHealth;

    private bool isBetweenWaves;
    private TowerHealth towerHealth;

    public int CurrentWave => currentWave;
    public int EnemiesRemaining => activeEnemies;
    public float TimeUntilNextWave => timeUntilNextWave;
    public bool IsBetweenWaves => isBetweenWaves;
    public float AdaptiveDifficultyMultiplier =>
        adaptiveDifficultyMultiplier;

    private void Start()
    {
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        while (mapGenerator == null ||
               mapGenerator.GeneratedPaths.Count == 0)
        {
            yield return null;
        }

        towerHealth = FindFirstObjectByType<TowerHealth>();

        if (towerHealth != null)
        {
            startingCastleHealth = towerHealth.CurrentHealth;
        }

        timeUntilNextWave = firstWaveDelay;
        isBetweenWaves = true;

        while (timeUntilNextWave > 0f)
        {
            timeUntilNextWave -= Time.deltaTime;
            yield return null;
        }

        while (true)
        {
            currentWave++;
            isBetweenWaves = false;
            waveStartTime = Time.time;

            yield return StartCoroutine(SpawnWave());

            while (activeEnemies > 0)
            {
                yield return null;
            }

            EvaluatePlayerPerformance();

            isBetweenWaves = true;
            timeUntilNextWave = timeBetweenWaves;

            while (timeUntilNextWave > 0f)
            {
                timeUntilNextWave -= Time.deltaTime;
                yield return null;
            }
        }
    }

    private IEnumerator SpawnWave()
    {
        int baseBudget =
            startingWaveBudget +
            ((currentWave - 1) * budgetIncreasePerWave);

        int remainingBudget = Mathf.RoundToInt(
            baseBudget * adaptiveDifficultyMultiplier
        );

        Debug.Log(
            $"Wave {currentWave} started with a budget of " +
            $"{remainingBudget}. Difficulty multiplier: " +
            $"{adaptiveDifficultyMultiplier:F2}"
        );

        while (remainingBudget > 0)
        {
            EnemyMovement selectedPrefab =
                SelectEnemyPrefab(remainingBudget);

            if (selectedPrefab == null)
            {
                break;
            }

            SpawnEnemy(selectedPrefab);

            remainingBudget -= GetEnemyCost(selectedPrefab);

            float adjustedSpawnInterval = Mathf.Max(
                minimumSpawnInterval,
                baseSpawnInterval / adaptiveDifficultyMultiplier
            );

            yield return new WaitForSeconds(adjustedSpawnInterval);
        }
    }

    private EnemyMovement SelectEnemyPrefab(int remainingBudget)
    {
        bool runnerAvailable =
            currentWave >= runnerUnlockWave &&
            runnerGoblinPrefab != null &&
            remainingBudget >= runnerGoblinCost;

        bool bruteAvailable =
            currentWave >= bruteUnlockWave &&
            bruteGoblinPrefab != null &&
            remainingBudget >= bruteGoblinCost;

        int defenderCount =
            FindObjectsByType<DefenderHealth>(
                FindObjectsSortMode.None
            ).Length;

        float bruteChance = 0.2f;
        float runnerChance = 0.35f;

        // More defenders cause a slightly higher chance of Brutes.
        if (defenderCount >= 4)
        {
            bruteChance += 0.15f;
        }

        // Few defenders cause more fast but weaker Runners.
        if (defenderCount <= 2)
        {
            runnerChance += 0.1f;
        }

        float randomValue = Random.value;

        if (bruteAvailable && randomValue < bruteChance)
        {
            return bruteGoblinPrefab;
        }

        if (runnerAvailable &&
            randomValue < bruteChance + runnerChance)
        {
            return runnerGoblinPrefab;
        }

        if (standardGoblinPrefab != null &&
            remainingBudget >= standardGoblinCost)
        {
            return standardGoblinPrefab;
        }

        if (runnerAvailable)
        {
            return runnerGoblinPrefab;
        }

        if (bruteAvailable)
        {
            return bruteGoblinPrefab;
        }

        return null;
    }

    private void SpawnEnemy(EnemyMovement enemyPrefab)
    {
        IReadOnlyList<List<Vector3>> availablePaths =
            mapGenerator.GeneratedPaths;

        if (availablePaths.Count == 0 || enemyPrefab == null)
        {
            return;
        }

        int selectedPathIndex = SelectPathIndex(
            availablePaths.Count
        );

        List<Vector3> selectedPath =
            availablePaths[selectedPathIndex];

        EnemyMovement newEnemy = Instantiate(enemyPrefab);

        newEnemy.name =
            $"{newEnemy.BehaviourType} Goblin - Wave {currentWave}";

        EnemyHealth enemyHealth =
            newEnemy.GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {
            int healthLevel =
                (currentWave - 1) / wavesPerHealthIncrease;

            int additionalHealth =
                healthLevel * healthIncreaseAmount;

            enemyHealth.IncreaseMaximumHealth(additionalHealth);
            enemyHealth.Died += HandleEnemyDeath;

            activeEnemies++;
        }

        newEnemy.Initialise(selectedPath);
    }

    private int SelectPathIndex(int pathCount)
    {
        if (pathCount <= 1)
        {
            previousPathIndex = 0;
            return 0;
        }

        int selectedPathIndex;

        do
        {
            selectedPathIndex = Random.Range(0, pathCount);
        }
        while (selectedPathIndex == previousPathIndex);

        previousPathIndex = selectedPathIndex;
        return selectedPathIndex;
    }

    private int GetEnemyCost(EnemyMovement enemyPrefab)
    {
        if (enemyPrefab == bruteGoblinPrefab)
        {
            return bruteGoblinCost;
        }

        if (enemyPrefab == runnerGoblinPrefab)
        {
            return runnerGoblinCost;
        }

        return standardGoblinCost;
    }

    private void HandleEnemyDeath(EnemyHealth defeatedEnemy)
    {
        defeatedEnemy.Died -= HandleEnemyDeath;
        activeEnemies = Mathf.Max(0, activeEnemies - 1);
    }

    private void EvaluatePlayerPerformance()
    {
        float completedWaveTime = Time.time - waveStartTime;
        float adjustment = 0f;

        if (towerHealth != null && startingCastleHealth > 0)
        {
            float castleHealthPercentage =
                (float)towerHealth.CurrentHealth /
                startingCastleHealth;

            if (castleHealthPercentage >= 0.75f &&
                completedWaveTime <= fastWaveCompletionTime)
            {
                adjustment += difficultyAdjustment;
            }
            else if (castleHealthPercentage <= 0.4f)
            {
                adjustment -= difficultyAdjustment;
            }
        }

        int survivingDefenders =
            FindObjectsByType<DefenderHealth>(
                FindObjectsSortMode.None
            ).Length;

        if (survivingDefenders >= 5)
        {
            adjustment += 0.05f;
        }
        else if (survivingDefenders <= 1)
        {
            adjustment -= 0.05f;
        }

        adaptiveDifficultyMultiplier = Mathf.Clamp(
            adaptiveDifficultyMultiplier + adjustment,
            minimumDifficultyMultiplier,
            maximumDifficultyMultiplier
        );

        Debug.Log(
            $"Wave {currentWave} completed in " +
            $"{completedWaveTime:F1} seconds. " +
            $"Next difficulty multiplier: " +
            $"{adaptiveDifficultyMultiplier:F2}"
        );
    }
}
