using UnityEngine;
using UnityEngine.UI;


public class WorldHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;

    private EnemyHealth enemyHealth;
    private DefenderHealth defenderHealth;
    private Camera mainCamera;

    
    private void Awake()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();
        defenderHealth = GetComponentInParent<DefenderHealth>();
        mainCamera = Camera.main;
    }

    
    private void Start()
    {
        UpdateHealthBar();
    }

    
    private void Update()
    {
        UpdateHealthBar();
    }

    
    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }
    }

    
    private void UpdateHealthBar()
    {
        if (enemyHealth != null)
        {
            healthSlider.maxValue = enemyHealth.MaximumHealth;
            healthSlider.value = enemyHealth.CurrentHealth;
        }
        else if (defenderHealth != null)
        {
            healthSlider.maxValue = defenderHealth.MaximumHealth;
            healthSlider.value = defenderHealth.CurrentHealth;
        }
    }
}
