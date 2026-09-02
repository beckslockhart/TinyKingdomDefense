using TMPro;
using UnityEngine;


public class GameHUD : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text towerHealthText;
    [SerializeField] private TMP_Text defenderCostText;

    [Header("Settings")]
    [SerializeField] private int defenderCost = 50;

    private TowerHealth towerHealth;

    private void Update()
    {
        UpdateGoldDisplay();
        UpdateTowerHealthDisplay();
        UpdateDefenderCostDisplay();
    }

    
    private void UpdateGoldDisplay()
    {
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
        if (towerHealth == null)
        {
            towerHealth = FindFirstObjectByType<TowerHealth>();
        }

        if (towerHealth == null)
        {
            towerHealthText.text = "Castle Health: 0 / 200";
            return;
        }

        towerHealthText.text =
            $"Castle Health: {towerHealth.CurrentHealth} / " +
            $"{towerHealth.MaximumHealth}";
    }

    
    private void UpdateDefenderCostDisplay()
    {
        defenderCostText.text =
            $"Archer Cost: {defenderCost} Gold";
    }
}
