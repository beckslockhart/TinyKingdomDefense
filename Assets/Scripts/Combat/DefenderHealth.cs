using System.Collections;
using UnityEngine;


public class DefenderHealth : MonoBehaviour
{
    [SerializeField] private int maximumHealth = 75;

    private int currentHealth;
    private bool isDestroyed;

    public int CurrentHealth => currentHealth;
    public int MaximumHealth => maximumHealth;

    private void Awake()
    {
        currentHealth = maximumHealth;
    }

    
    public void TakeDamage(int damageAmount)
    {
        if (isDestroyed)
        {
            return;
        }

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            DestroyDefender();
        }
    }

    
    private IEnumerator DamageFlash()
    {
        Renderer defenderRenderer = GetComponent<Renderer>();

        if (defenderRenderer == null)
        {
            yield break;
        }

        Color originalColour = defenderRenderer.material.color;
        defenderRenderer.material.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        if (defenderRenderer != null)
        {
            defenderRenderer.material.color = originalColour;
        }
    }

    
    private void DestroyDefender()
    {
        isDestroyed = true;
        Destroy(gameObject);
    }
}
