using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RouletteUIManager : MonoBehaviour
{
    public static RouletteUIManager Instance;

    public int selectedChipValue = 1;

    public TextMeshProUGUI balanceText;

    public float currentBalance;

    public Button spinButtonImg;
    public GameObject chipsUi;
    public GameObject inputUi;

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

    public void SpinButton()
    {
        RouletteManager.Instance.ballController.ballSpeedController(300, .1f);

        RouletteManager.Instance.gameTimer = 3;

        RouletteManager.Instance.gameStart = true;

        spinButtonImg.gameObject.SetActive(false);
        chipsUi.SetActive(false);
        inputUi.SetActive(false);
    }

}
