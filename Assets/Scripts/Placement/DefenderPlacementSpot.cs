using UnityEngine;
using UnityEngine.InputSystem;


public class DefenderPlacementSpot : MonoBehaviour
{
    [SerializeField] private GameObject defenderPrefab;
    [SerializeField] private int defenderCost = 50;
    [SerializeField] private float defenderHeight = 1f;

    private bool isOccupied;
    private Camera mainCamera;

    
    private void Awake()
    {
        mainCamera = Camera.main;
    }

    
    private void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckForClick();
        }
    }

    
    private void CheckForClick()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform)
            {
                TryPlaceDefender();
            }
        }
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
