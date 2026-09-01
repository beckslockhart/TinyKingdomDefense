using UnityEngine;


public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private int startingGold = 150;

   
    private int currentGold;

   
    public int CurrentGold => currentGold;

   
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentGold = startingGold;

        Debug.Log($"Starting gold: {currentGold}");
    }

   
    public bool TrySpendGold(int amount)
    {
        if (currentGold < amount)
        {
            Debug.Log("Not enough gold!");
            return false;
        }

        currentGold -= amount;
        Debug.Log($"Defender purchased. Gold remaining: {currentGold}");

        return true;
    }

   
    public void AddGold(int amount)
    {
        currentGold += amount;
        Debug.Log($"Gold earned. Current gold: {currentGold}");
    }
}
