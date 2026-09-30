using UnityEngine;
using UnityEngine.InputSystem;

public enum DefenderPurchaseType
{
    Archer,
    Cannon,
    Frost
}

public class DefenderPlacementSpot : MonoBehaviour
{
    [Header("Defender Prefabs")]
    [SerializeField] private GameObject archerPrefab;
    [SerializeField] private GameObject cannonPrefab;
    [SerializeField] private GameObject frostPrefab;

    [Header("Defender Costs")]
    [SerializeField] private int archerCost = 50;
    [SerializeField] private int cannonCost = 100;
    [SerializeField] private int frostCost = 75;

    [Header("Placement")]
    [SerializeField] private float defenderHeight = 1f;

    [Header("Feedback Colours")]
    [SerializeField] private Color affordableColour = Color.green;
    [SerializeField] private Color unaffordableColour = Color.red;

    private bool isOccupied;
    private Camera mainCamera;
    private Renderer spotRenderer;
    private Color originalColour;

    public int ArcherCost => archerCost;
    public int CannonCost => cannonCost;
    public int FrostCost => frostCost;

    private void Awake()
    {
        mainCamera = Camera.main;
        spotRenderer = GetComponentInChildren<Renderer>();

        if (spotRenderer != null)
        {
            originalColour = spotRenderer.material.color;
        }
    }

    private void Update()
    {
        if (Mouse.current == null || isOccupied)
        {
            return;
        }

        bool mouseIsOverSpot = IsMouseOverSpot();
        UpdateAppearance(mouseIsOverSpot);

        if (mouseIsOverSpot &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            OpenDefenderSelection();
        }
    }

    private bool IsMouseOverSpot()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return false;
        }

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray =
            mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.transform == transform ||
                   hit.transform.IsChildOf(transform);
        }

        return false;
    }

    private void UpdateAppearance(bool mouseIsOverSpot)
    {
        if (spotRenderer == null)
        {
            return;
        }

        if (!mouseIsOverSpot)
        {
            spotRenderer.material.color = originalColour;
            return;
        }

        bool canAffordAnyDefender =
            CurrencyManager.Instance != null &&
            CurrencyManager.Instance.CurrentGold >=
            Mathf.Min(archerCost, frostCost, cannonCost);

        spotRenderer.material.color =
            canAffordAnyDefender
                ? affordableColour
                : unaffordableColour;
    }

    private void OpenDefenderSelection()
    {
        if (DefenderSelectionUI.Instance == null)
        {
            Debug.LogError(
                "No DefenderSelectionUI exists in the scene."
            );
            return;
        }

        DefenderSelectionUI.Instance.Open(this);
    }

    public bool TryPlaceDefender(
        DefenderPurchaseType purchaseType
    )
    {
        if (isOccupied)
        {
            return false;
        }

        GameObject selectedPrefab = null;
        int selectedCost = 0;

        switch (purchaseType)
        {
            case DefenderPurchaseType.Cannon:
                selectedPrefab = cannonPrefab;
                selectedCost = cannonCost;
                break;

            case DefenderPurchaseType.Frost:
                selectedPrefab = frostPrefab;
                selectedCost = frostCost;
                break;

            default:
                selectedPrefab = archerPrefab;
                selectedCost = archerCost;
                break;
        }

        if (selectedPrefab == null)
        {
            Debug.LogError(
                $"{purchaseType} prefab has not been assigned."
            );
            return false;
        }

        if (CurrencyManager.Instance == null)
        {
            Debug.LogError(
                "No Currency Manager exists in the scene."
            );
            return false;
        }

        bool purchaseSuccessful =
            CurrencyManager.Instance.TrySpendGold(selectedCost);

        if (!purchaseSuccessful)
        {
            return false;
        }

        Vector3 defenderPosition =
            transform.position + Vector3.up * defenderHeight;

        Instantiate(
            selectedPrefab,
            defenderPosition,
            Quaternion.identity
        );

        isOccupied = true;
        gameObject.SetActive(false);

        return true;
    }
}