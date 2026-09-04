using UnityEngine;
using UnityEngine.InputSystem;


public class DefenderPlacementSpot : MonoBehaviour
{
    [Header("Placement")]
    [SerializeField] private GameObject defenderPrefab;
    [SerializeField] private int defenderCost = 50;
    [SerializeField] private float defenderHeight = 1f;

    [Header("Feedback Colours")]
    [SerializeField] private Color affordableColour = Color.green;
    [SerializeField] private Color unaffordableColour = Color.red;
    
    [Header("Path Tile Visuals")]
    [SerializeField] private GameObject straightPathTile;
    [SerializeField] private GameObject cornerPathTile;
    [SerializeField] private GameObject threeWayPathTile;
    [SerializeField] private GameObject fourWayPathTile;
    [SerializeField] private GameObject endPathTile;
    [SerializeField] private float pathTileYOffset = 0.03f;
    [SerializeField] private float pathTileScale = 1f;

    
    private bool isOccupied;
    private Camera mainCamera;
    private Renderer spotRenderer;
    private Color originalColour;

    
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
            TryPlaceDefender();
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

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

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

        bool canAfford =
            CurrencyManager.Instance != null &&
            CurrencyManager.Instance.CurrentGold >= defenderCost;

        spotRenderer.material.color =
            canAfford ? affordableColour : unaffordableColour;
    }

    
    private void TryPlaceDefender()
    {
        if (isOccupied || defenderPrefab == null)
        {
            return;
        }

        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("No Currency Manager exists in the scene.");
            return;
        }

        bool purchaseSuccessful =
            CurrencyManager.Instance.TrySpendGold(defenderCost);

        if (!purchaseSuccessful)
        {
            return;
        }

        Vector3 defenderPosition =
            transform.position + Vector3.up * defenderHeight;

        Instantiate(
            defenderPrefab,
            defenderPosition,
            Quaternion.identity
        );

        isOccupied = true;
        gameObject.SetActive(false);
    }
}
