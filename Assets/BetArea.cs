using UnityEngine;
using System.Linq;

public class BetArea : MonoBehaviour
{
    public BetType betType;
    public string betValue; // örnek: "17", "red", "even", "1-18"

    private Renderer rend;
    private MaterialPropertyBlock propBlock;
    private Color originalColor;
    public Color hoverColor = new Color(1f, 1f, 0.6f, 1f); // Hafif sarımsı vurgulu renk

    private int chipCount = 0; // Eklenen chip sayısını takip eder

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();

        if (rend != null)
        {
            rend.GetPropertyBlock(propBlock);
            originalColor = propBlock.GetColor("_BaseColor"); // URP'de "_BaseColor" kullanılır
        }
    }

    private void OnMouseDown()
    {
        if (!RouletteManager.Instance || RouletteUIManager.Instance.selectedChipValue <= 0)
            return;

        float chipAmount = RouletteUIManager.Instance.selectedChipValue;

        // Bakiye kontrolü
        if (!RouletteUIManager.Instance.HasEnoughBalance(chipAmount))
            return;

        Bet newBet = new Bet
        {
            type = betType,
            value = betValue,
            amount = chipAmount
        };

        RouletteManager.Instance.PlaceBet(newBet);
        RouletteUIManager.Instance.AdjustBalance(-chipAmount);

        InstantiateChip(chipAmount);

        Debug.Log($"Bet placed: {betType} - {betValue} - {chipAmount}");
    }

    private void InstantiateChip(float amount)
    {
        GameObject chipPrefab = RouletteManager.Instance.chipPrefabs.FirstOrDefault(chip => chip.name.Contains(amount.ToString()));

        if (chipPrefab != null)
        {
            Vector3 offset = Vector3.up * 0.01f * (chipCount + 1);
            Instantiate(chipPrefab, transform.position + offset, Quaternion.Euler(90f, 0f, 0f), RouletteManager.Instance.chipParent);
            chipCount++;
        }
    }

    private void OnMouseEnter()
    {
        if (rend != null)
        {
            rend.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", hoverColor);
            rend.SetPropertyBlock(propBlock);
        }
    }

    private void OnMouseExit()
    {
        if (rend != null)
        {
            rend.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", originalColor);
            rend.SetPropertyBlock(propBlock);
        }
    }
}
