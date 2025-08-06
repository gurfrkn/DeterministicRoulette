using UnityEngine;
using TMPro;

public class RouletteUIManager : MonoBehaviour
{
    public static RouletteUIManager Instance;

    public int selectedChipValue = 1;

    public TextMeshProUGUI balanceText;

    public float currentBalance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateBalanceUI();
    }

    public void UpdateBalanceUI()
    {
        balanceText.text = $"Balance: ${currentBalance}";
    }


    public bool HasEnoughBalance(float amount)
    {
        return currentBalance >= amount;
    }

    public void AdjustBalance(float amount)
    {
        currentBalance += amount;
        UpdateBalanceUI();
    }

    public void SelectChip(int value)
    {
        selectedChipValue = value;
        Debug.Log("Chip selected: " + value);
    }
}
