using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelObject;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private TMP_Text costText;

    [Header("Buttons")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button closeButton;

    private DefenderUpgrade selectedDefender;
    private CastleUpgrade selectedCastle;

    private void Awake()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(
                PurchaseSelectedUpgrade
            );
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }

        ClosePanel();
    }

    public void ShowDefender(DefenderUpgrade defender)
    {
        if (defender == null)
        {
            return;
        }

        selectedDefender = defender;
        selectedCastle = null;

        if (panelObject != null)
        {
            panelObject.SetActive(true);
        }

        RefreshPanel();
    }

    public void ShowCastle(CastleUpgrade castle)
    {
        if (castle == null)
        {
            return;
        }

        selectedCastle = castle;
        selectedDefender = null;

        if (panelObject != null)
        {
            panelObject.SetActive(true);
        }

        RefreshPanel();
    }

    public void ClosePanel()
    {
        selectedDefender = null;
        selectedCastle = null;

        if (panelObject != null)
        {
            panelObject.SetActive(false);
        }
    }

    private void PurchaseSelectedUpgrade()
    {
        if (selectedDefender != null)
        {
            selectedDefender.TryPurchaseUpgrade();
        }
        else if (selectedCastle != null)
        {
            selectedCastle.TryPurchaseUpgrade();
        }

        RefreshPanel();
    }

    private void RefreshPanel()
    {
        if (selectedDefender != null)
        {
            RefreshDefenderPanel();
            return;
        }

        if (selectedCastle != null)
        {
            RefreshCastlePanel();
        }
    }

    private void RefreshDefenderPanel()
    {
        if (titleText != null)
        {
            titleText.text =
                selectedDefender.DefenderDisplayName;
        }

        if (levelText != null)
        {
            levelText.text =
                $"Level {selectedDefender.CurrentLevel}/" +
                $"{selectedDefender.MaximumLevel}";
        }

        if (selectedDefender.IsMaximumLevel)
        {
            ShowMaximumLevel();
            return;
        }

        DefenderUpgradeLevel upgrade =
            selectedDefender.NextUpgrade;

        if (upgrade == null)
        {
            ShowMissingUpgrade();
            return;
        }

        if (detailsText != null)
        {
            detailsText.text =
                $"{upgrade.UpgradeName}\n" +
                $"+{upgrade.HealthIncrease} Health\n" +
                $"+{upgrade.DamageIncrease} Damage\n" +
                $"+{upgrade.RangeIncrease:0.##} Range\n" +
                $"Faster attacks";
        }

        ShowCost(upgrade.GoldCost);
    }

    private void RefreshCastlePanel()
    {
        if (titleText != null)
        {
            titleText.text =
                selectedCastle.CastleDisplayName;
        }

        if (levelText != null)
        {
            levelText.text =
                $"Level {selectedCastle.CurrentLevel}/" +
                $"{selectedCastle.MaximumLevel}";
        }

        if (selectedCastle.IsMaximumLevel)
        {
            ShowMaximumLevel();
            return;
        }

        CastleUpgradeLevel upgrade =
            selectedCastle.NextUpgrade;

        if (upgrade == null)
        {
            ShowMissingUpgrade();
            return;
        }

        if (detailsText != null)
        {
            detailsText.text =
                $"{upgrade.UpgradeName}\n" +
                $"+{upgrade.MaximumHealthIncrease} " +
                $"Maximum Health";
        }

        ShowCost(upgrade.GoldCost);
    }

    private void ShowCost(int cost)
    {
        if (costText != null)
        {
            costText.text = $"Cost: {cost} Gold";
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable =
                CurrencyManager.Instance != null &&
                CurrencyManager.Instance.CanAfford(cost);
        }
    }

    private void ShowMaximumLevel()
    {
        if (detailsText != null)
        {
            detailsText.text = "Maximum level reached";
        }

        if (costText != null)
        {
            costText.text = "Fully upgraded";
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = false;
        }
    }

    private void ShowMissingUpgrade()
    {
        if (detailsText != null)
        {
            detailsText.text = "Upgrade data is missing";
        }

        if (costText != null)
        {
            costText.text = string.Empty;
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = false;
        }
    }
}