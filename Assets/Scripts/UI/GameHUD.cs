using System.Collections;
using TMPro;
using UnityEngine;

public class GameHUD : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text towerHealthText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text enemiesRemainingText;
    [SerializeField] private TMP_Text nextWaveText;

    [Header("Wave Popup")]
    [SerializeField] private float wavePopupDuration = 2f;

    private TowerHealth towerHealth;
    private EnemySpawner enemySpawner;

    private int lastDisplayedWave;
    private Coroutine wavePopupCoroutine;

    private void Start()
    {
        if (waveText != null)
        {
            waveText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateGoldDisplay();
        UpdateTowerHealthDisplay();
        UpdateWaveDisplay();
    }

    private void UpdateGoldDisplay()
    {
        if (goldText == null)
        {
            return;
        }

        if (CurrencyManager.Instance == null)
        {
            goldText.text = "Gold: 0";
            return;
        }

        goldText.text =
            $"Gold: {CurrencyManager.Instance.CurrentGold}";
    }

    private void UpdateTowerHealthDisplay()
    {
        if (towerHealthText == null)
        {
            return;
        }

        if (towerHealth == null)
        {
            towerHealth = FindFirstObjectByType<TowerHealth>();
        }

        if (towerHealth == null)
        {
            towerHealthText.text = "Castle Health: 0 / 0";
            return;
        }

        towerHealthText.text =
            $"Castle Health: {towerHealth.CurrentHealth} / " +
            $"{towerHealth.MaximumHealth}";
    }

    private void UpdateWaveDisplay()
    {
        if (enemySpawner == null)
        {
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
        }

        if (enemySpawner == null)
        {
            return;
        }

        int currentWave = enemySpawner.CurrentWave;

        if (currentWave > 0 &&
            currentWave != lastDisplayedWave)
        {
            lastDisplayedWave = currentWave;

            if (wavePopupCoroutine != null)
            {
                StopCoroutine(wavePopupCoroutine);
            }

            wavePopupCoroutine =
                StartCoroutine(ShowWavePopup(currentWave));
        }

        if (enemiesRemainingText != null)
        {
            enemiesRemainingText.text =
                $"Enemies Remaining: " +
                $"{enemySpawner.EnemiesRemaining}";
        }

        if (nextWaveText != null)
        {
            nextWaveText.gameObject.SetActive(
                enemySpawner.IsBetweenWaves
            );

            if (enemySpawner.IsBetweenWaves)
            {
                int countdown = Mathf.CeilToInt(
                    enemySpawner.TimeUntilNextWave
                );

                int upcomingWave =
                    Mathf.Max(1, currentWave + 1);

                nextWaveText.text =
                    $"Wave {upcomingWave} Begins In: " +
                    $"{countdown}";
            }
        }
    }

    private IEnumerator ShowWavePopup(int waveNumber)
    {
        if (waveText == null)
        {
            yield break;
        }

        waveText.text = $"WAVE {waveNumber}";
        waveText.gameObject.SetActive(true);

        yield return new WaitForSeconds(
            wavePopupDuration
        );

        waveText.gameObject.SetActive(false);
        wavePopupCoroutine = null;
    }
}