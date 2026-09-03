using System.Collections;
using UnityEngine;


public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maximumHealth = 50;
    [SerializeField] private int goldReward = 25;

    private int currentHealth;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaximumHealth => maximumHealth;

   
    private void Awake()
    {
        currentHealth = maximumHealth;
    }

    
    public void IncreaseMaximumHealth(int additionalHealth)
    {
        maximumHealth += additionalHealth;
        currentHealth += additionalHealth;
    }
    
    
    public void TakeDamage(int damageAmount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

   
    private IEnumerator DamageFlash()
    {
        Renderer enemyRenderer = GetComponent<Renderer>();

        if (enemyRenderer == null)
        {
            yield break;
        }

        Color originalColour = enemyRenderer.material.color;
        enemyRenderer.material.color = Color.white;

        yield return new WaitForSeconds(0.1f);

        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = originalColour;
        }
    }

   
    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddGold(goldReward);
        }

        Destroy(gameObject);
    }
}
