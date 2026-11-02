using System;
using System.Collections;
using UnityEngine;

public class TowerHealth : MonoBehaviour
{
    [SerializeField] private int maximumHealth = 200;

    private int currentHealth;
    private bool isDestroyed;

    public int CurrentHealth => currentHealth;
    public int MaximumHealth => maximumHealth;
    public bool IsDestroyed => isDestroyed;

    public event Action TowerDamaged;
    public event Action<int, int> HealthChanged;

    private void Awake()
    {
        currentHealth = maximumHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDestroyed || damageAmount <= 0)
        {
            return;
        }

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(
            $"Tower health: {currentHealth}/{maximumHealth}"
        );

        TowerDamaged?.Invoke();
        HealthChanged?.Invoke(
            currentHealth,
            maximumHealth
        );

        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            TowerDestroyed();
        }
    }

    public void IncreaseMaximumHealth(int additionalHealth)
    {
        if (isDestroyed || additionalHealth <= 0)
        {
            return;
        }

        maximumHealth += additionalHealth;
        currentHealth += additionalHealth;

        currentHealth = Mathf.Min(
            currentHealth,
            maximumHealth
        );

        HealthChanged?.Invoke(
            currentHealth,
            maximumHealth
        );

        Debug.Log(
            $"Tower upgraded: {currentHealth}/{maximumHealth}"
        );
    }

    public void RestoreToFullHealth()
    {
        if (isDestroyed)
        {
            return;
        }

        currentHealth = maximumHealth;

        HealthChanged?.Invoke(
            currentHealth,
            maximumHealth
        );
    }

    private IEnumerator DamageFlash()
    {
        Renderer towerRenderer =
            GetComponentInChildren<Renderer>();

        if (towerRenderer == null)
        {
            yield break;
        }

        Color originalColour =
            towerRenderer.material.color;

        towerRenderer.material.color = Color.red;

        yield return new WaitForSeconds(0.15f);

        if (towerRenderer != null && !isDestroyed)
        {
            towerRenderer.material.color =
                originalColour;
        }
    }

    private void TowerDestroyed()
    {
        if (isDestroyed)
        {
            return;
        }

        isDestroyed = true;

        Debug.Log(
            "GAME OVER — The tower has been destroyed!"
        );

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndGame();
        }

        gameObject.SetActive(false);
    }
}