using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UpgradeSelectionManager : MonoBehaviour
{
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private UpgradePanelUI upgradePanelUI;

    [SerializeField] private float maximumSelectionDistance = 200f;

    private void Awake()
    {
        if (gameplayCamera == null)
        {
            gameplayCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        TrySelectUpgradeTarget();
    }

    private void TrySelectUpgradeTarget()
    {
        if (gameplayCamera == null || upgradePanelUI == null)
        {
            return;
        }

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray selectionRay =
            gameplayCamera.ScreenPointToRay(mousePosition);

        bool hitSomething = Physics.Raycast(
            selectionRay,
            out RaycastHit hit,
            maximumSelectionDistance
        );

        if (!hitSomething)
        {
            upgradePanelUI.ClosePanel();
            return;
        }

        DefenderUpgrade defenderUpgrade =
            hit.collider.GetComponentInParent<DefenderUpgrade>();

        if (defenderUpgrade != null)
        {
            upgradePanelUI.ShowDefender(defenderUpgrade);
            return;
        }

        CastleUpgrade castleUpgrade =
            hit.collider.GetComponentInParent<CastleUpgrade>();

        if (castleUpgrade != null)
        {
            upgradePanelUI.ShowCastle(castleUpgrade);
            return;
        }

        upgradePanelUI.ClosePanel();
    }
}