using System;
using UnityEngine;

[Serializable]
public class CastleUpgradeLevel
{
    [Header("Upgrade Information")]
    [SerializeField] private string upgradeName;
    [SerializeField] private int goldCost;

    [Header("Health Improvement")]
    [SerializeField] private int maximumHealthIncrease;

    public string UpgradeName => upgradeName;
    public int GoldCost => goldCost;
    public int MaximumHealthIncrease => maximumHealthIncrease;

    public CastleUpgradeLevel(
        string name,
        int cost,
        int healthIncrease)
    {
        upgradeName = name;
        goldCost = cost;
        maximumHealthIncrease = healthIncrease;
    }
}

public class CastleUpgrade : MonoBehaviour
{
    private string castleDisplayName = "Central Castle";

    private CastleUpgradeLevel levelOneUpgrade;
    private CastleUpgradeLevel levelTwoUpgrade;

    private GameObject baseVisual;
    private GameObject levelOneVisual;
    private GameObject levelTwoVisual;

    private TowerHealth towerHealth;
    private int currentLevel;
    private bool hasBeenConfigured;

    public string CastleDisplayName => castleDisplayName;
    public int CurrentLevel => currentLevel;
    public int MaximumLevel => 2;
    public bool IsMaximumLevel => currentLevel >= MaximumLevel;

    public event Action<CastleUpgrade> Upgraded;

    public CastleUpgradeLevel NextUpgrade
    {
        get
        {
            if (currentLevel == 0)
            {
                return levelOneUpgrade;
            }

            if (currentLevel == 1)
            {
                return levelTwoUpgrade;
            }

            return null;
        }
    }

    public int NextUpgradeCost
    {
        get
        {
            if (NextUpgrade == null)
            {
                return 0;
            }

            return NextUpgrade.GoldCost;
        }
    }

    private void Awake()
    {
        towerHealth = GetComponent<TowerHealth>();
        currentLevel = 0;
    }

    public void ConfigureRuntimeVisuals(
        GameObject baseVisualObject,
        GameObject levelOneVisualObject,
        GameObject levelTwoVisualObject)
    {
        baseVisual = baseVisualObject;
        levelOneVisual = levelOneVisualObject;
        levelTwoVisual = levelTwoVisualObject;

        levelOneUpgrade = new CastleUpgradeLevel(
            "Fortified Castle",
            150,
            100
        );

        levelTwoUpgrade = new CastleUpgradeLevel(
            "Royal Stronghold",
            250,
            150
        );

        hasBeenConfigured = true;

        UpdateVisual();
    }

    public bool TryPurchaseUpgrade()
    {
        if (!hasBeenConfigured)
        {
            Debug.LogError(
                "The castle upgrade system has not been configured."
            );

            return false;
        }

        if (IsMaximumLevel)
        {
            Debug.Log(
                $"{castleDisplayName} is already at maximum level."
            );

            return false;
        }

        CastleUpgradeLevel upgrade = NextUpgrade;

        if (upgrade == null)
        {
            Debug.LogError(
                $"{castleDisplayName} has missing upgrade data."
            );

            return false;
        }

        if (towerHealth == null)
        {
            Debug.LogError(
                "CastleUpgrade must be on the same object as TowerHealth."
            );

            return false;
        }

        if (CurrencyManager.Instance == null)
        {
            Debug.LogError(
                "No CurrencyManager exists in the scene."
            );

            return false;
        }

        bool purchaseSuccessful =
            CurrencyManager.Instance.TrySpendGold(
                upgrade.GoldCost
            );

        if (!purchaseSuccessful)
        {
            return false;
        }

        towerHealth.IncreaseMaximumHealth(
            upgrade.MaximumHealthIncrease
        );

        currentLevel++;

        UpdateVisual();

        Upgraded?.Invoke(this);

        Debug.Log(
            $"{castleDisplayName} upgraded to Level {currentLevel}."
        );

        return true;
    }

    private void UpdateVisual()
    {
        if (baseVisual != null)
        {
            baseVisual.SetActive(currentLevel == 0);
        }

        if (levelOneVisual != null)
        {
            levelOneVisual.SetActive(currentLevel == 1);
        }

        if (levelTwoVisual != null)
        {
            levelTwoVisual.SetActive(currentLevel == 2);
        }
    }
}