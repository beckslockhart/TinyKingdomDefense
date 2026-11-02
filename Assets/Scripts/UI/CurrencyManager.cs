using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance
    {
        get;
        private set;
    }

    [SerializeField] private int startingGold = 150;

    private int currentGold;

    public int CurrentGold => currentGold;

    public event Action<int> GoldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentGold = startingGold;

        Debug.Log($"Starting gold: {currentGold}");
    }

    public bool CanAfford(int amount)
    {
        if (amount < 0)
        {
            return false;
        }

        return currentGold >= amount;
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning(
                "The gold cost must be greater than zero."
            );

            return false;
        }

        if (!CanAfford(amount))
        {
            Debug.Log("Not enough gold!");
            return false;
        }

        currentGold -= amount;

        GoldChanged?.Invoke(currentGold);

        Debug.Log(
            $"Gold spent: {amount}. " +
            $"Gold remaining: {currentGold}"
        );

        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentGold += amount;

        GoldChanged?.Invoke(currentGold);

        Debug.Log(
            $"Gold earned: {amount}. " +
            $"Current gold: {currentGold}"
        );
    }
}