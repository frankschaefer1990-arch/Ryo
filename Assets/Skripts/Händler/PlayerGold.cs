using UnityEngine;

public class PlayerGold : MonoBehaviour
{
    public static PlayerGold Instance;

    public int currentGold = 100;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            return true;
        }

        return false;
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
    }
}