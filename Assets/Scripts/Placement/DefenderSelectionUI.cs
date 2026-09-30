using UnityEngine;
using UnityEngine.UI;

public class DefenderSelectionUI : MonoBehaviour
{
    public static DefenderSelectionUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Button archerButton;
    [SerializeField] private Button cannonButton;
    [SerializeField] private Button frostButton;

    private DefenderPlacementSpot selectedSpot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
    }

    public void Open(DefenderPlacementSpot placementSpot)
    {
        if (placementSpot == null || selectionPanel == null)
        {
            return;
        }

        selectedSpot = placementSpot;
        selectionPanel.SetActive(true);
        UpdateButtonAvailability();
    }

    public void BuyArcher()
    {
        TryPurchase(DefenderPurchaseType.Archer);
    }

    public void BuyCannon()
    {
        TryPurchase(DefenderPurchaseType.Cannon);
    }

    public void BuyFrost()
    {
        TryPurchase(DefenderPurchaseType.Frost);
    }

    public void Close()
    {
        selectedSpot = null;

        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
    }

    private void TryPurchase(DefenderPurchaseType purchaseType)
    {
        if (selectedSpot == null)
        {
            Close();
            return;
        }

        bool purchaseSuccessful =
            selectedSpot.TryPlaceDefender(purchaseType);

        if (purchaseSuccessful)
        {
            Close();
        }
        else
        {
            UpdateButtonAvailability();
        }
    }

    private void UpdateButtonAvailability()
    {
        if (selectedSpot == null ||
            CurrencyManager.Instance == null)
        {
            return;
        }

        int currentGold = CurrencyManager.Instance.CurrentGold;

        if (archerButton != null)
        {
            archerButton.interactable =
                currentGold >= selectedSpot.ArcherCost;
        }

        if (cannonButton != null)
        {
            cannonButton.interactable =
                currentGold >= selectedSpot.CannonCost;
        }

        if (frostButton != null)
        {
            frostButton.interactable =
                currentGold >= selectedSpot.FrostCost;
        }
    }
}