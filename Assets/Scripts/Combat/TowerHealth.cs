using UnityEngine;


public class TowerHealth : MonoBehaviour
{
    [SerializeField] private int maximumHealth = 200;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaximumHealth => maximumHealth;

   
    private void Awake()
    {
        currentHealth = maximumHealth;
    }


    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"Tower health: {currentHealth}/{maximumHealth}");

        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            TowerDestroyed();
        }
    }

  
    private System.Collections.IEnumerator DamageFlash()
    {
        Renderer towerRenderer = GetComponentInChildren<Renderer>();

        if (towerRenderer == null)
        {
            yield break;
        }

        Color originalColour = towerRenderer.material.color;
        towerRenderer.material.color = Color.red;

        yield return new WaitForSeconds(0.15f);

        if (towerRenderer != null)
        {
            towerRenderer.material.color = originalColour;
        }
    }

   
    private void TowerDestroyed()
    {
        Debug.Log("GAME OVER — The tower has been destroyed!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndGame();
        }

        gameObject.SetActive(false);
    }
}