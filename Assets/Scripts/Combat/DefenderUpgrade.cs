using System;
using UnityEngine;

[Serializable]
public class DefenderUpgradeLevel
{
    [Header("Upgrade Information")]
    [SerializeField] private string upgradeName = "Upgrade";
    [SerializeField] private int goldCost = 75;

    [Header("Health Improvement")]
    [SerializeField] private int healthIncrease = 25;

    [Header("Attack Improvements")]
    [SerializeField] private int damageIncrease = 5;
    [SerializeField] private float rangeIncrease = 0.5f;
    [SerializeField] private float attackIntervalReduction = 0.1f;

    [Header("Cannon Improvement")]
    [SerializeField] private float areaDamageRadiusIncrease = 0.25f;

    [Header("Frost Improvements")]
    [SerializeField] private float slowMultiplierReduction = 0.05f;
    [SerializeField] private float slowDurationIncrease = 0.5f;

    public string UpgradeName => upgradeName;
    public int GoldCost => goldCost;
    public int HealthIncrease => healthIncrease;
    public int DamageIncrease => damageIncrease;
    public float RangeIncrease => rangeIncrease;

    public float AttackIntervalReduction =>
        attackIntervalReduction;

    public float AreaDamageRadiusIncrease =>
        areaDamageRadiusIncrease;

    public float SlowMultiplierReduction =>
        slowMultiplierReduction;

    public float SlowDurationIncrease =>
        slowDurationIncrease;
}

public class DefenderUpgrade : MonoBehaviour
{
    [Header("Defender Information")]
    [SerializeField] private string defenderDisplayName =
        "Defender";

    [Header("Upgrade Levels")]
    [SerializeField] private DefenderUpgradeLevel levelOneUpgrade;
    [SerializeField] private DefenderUpgradeLevel levelTwoUpgrade;

    [Header("Visual Levels")]
    [SerializeField] private GameObject baseVisual;
    [SerializeField] private GameObject levelOneVisual;
    [SerializeField] private GameObject levelTwoVisual;

    private DefenderHealth defenderHealth;
    private DefenderAttack defenderAttack;

    private int currentLevel;

    public string DefenderDisplayName =>
        defenderDisplayName;

    public int CurrentLevel => currentLevel;
    public int MaximumLevel => 2;
    public bool IsMaximumLevel => currentLevel >= MaximumLevel;

    public event Action<DefenderUpgrade> Upgraded;

    public DefenderUpgradeLevel NextUpgrade
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
        defenderHealth = GetComponent<DefenderHealth>();
        defenderAttack = GetComponent<DefenderAttack>();

        currentLevel = 0;

        UpdateVisual();
    }

    public bool TryPurchaseUpgrade()
    {
        if (IsMaximumLevel)
        {
            Debug.Log(
                $"{defenderDisplayName} is already at maximum level."
            );

            return false;
        }

        if (NextUpgrade == null)
        {
            Debug.LogError(
                $"{defenderDisplayName} has missing upgrade data."
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
                NextUpgrade.GoldCost
            );

        if (!purchaseSuccessful)
        {
            return false;
        }

        ApplyUpgrade(NextUpgrade);

        currentLevel++;

        UpdateVisual();

        Upgraded?.Invoke(this);

        Debug.Log(
            $"{defenderDisplayName} upgraded to " +
            $"Level {currentLevel}."
        );

        return true;
    }

    private void ApplyUpgrade(
        DefenderUpgradeLevel upgradeLevel)
    {
        if (defenderHealth != null)
        {
            defenderHealth.IncreaseMaximumHealth(
                upgradeLevel.HealthIncrease
            );
        }

        if (defenderAttack != null)
        {
            defenderAttack.ApplyUpgradeStats(
                upgradeLevel.DamageIncrease,
                upgradeLevel.RangeIncrease,
                upgradeLevel.AttackIntervalReduction,
                upgradeLevel.AreaDamageRadiusIncrease,
                upgradeLevel.SlowMultiplierReduction,
                upgradeLevel.SlowDurationIncrease
            );
        }
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