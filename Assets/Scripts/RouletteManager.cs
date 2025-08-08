using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class RouletteManager : MonoBehaviour
{
    public static RouletteManager Instance { get; private set; }

    [Header("American Euro Roulette Items")]
    public GameObject[] americanRouletteItems;
    public GameObject[] euroRouletteItems;

    [Header("Wheel Settings")]
    public Transform wheel;
    public float currentWheelSpeed = 0f;

    [Header("Ball Settings")]
    public GameObject ballPrefab;
    public GameObject ballObject;

    public ballController ballController;

    public Transform ballSpawnPoint;
    public Transform sphereCenter;

    [Header("Slot Settings")]
    public GameObject[] allSlots;

    [HideInInspector] public int selectedNumber;

    public bool gameStart = false;
    public float gameTimer = 0;

    [Header("Betting System")]
    public List<Bet> playerBets = new List<Bet>();

    public Transform chipParent; // Instantiate edilecek parent (isteğe bağlı)
    public GameObject[] chipPrefabs; // Farklı para değerleri için chip prefabları

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ballObject = Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation);
        ballController = ballObject.GetComponent<ballController>();
    }

    private void Update()
    {
        if (wheel != null)
        {
            wheel.Rotate(Vector3.up * currentWheelSpeed * Time.deltaTime);

            if (gameStart)
            {
                gameTimer -= Time.deltaTime;

                if (gameTimer <= 0)
                {
                    gameStart = false;
                    gameTimer = 3;

                    ballController.ballSpeedController(0, .5f);
                    ballController.ballRB.isKinematic = false;

                    ballController.WaitAndJumpToTarget(allSlots[selectedNumber].transform.position);
                }
            }
        }
    }

    
    public void SetSelectedNumber(string numberText)
    {
        if (int.TryParse(numberText, out int value) && value >= 0 && value < allSlots.Length)
        {
            selectedNumber = value;
            ballController.slotTarget = selectedNumber;
        }
        else
        {
            selectedNumber = -1;
        }
    }

    public void PlaceBet(Bet newBet)
    {
        if (newBet.amount > RouletteUIManager.Instance.currentBalance)
        {
            Debug.Log("Yetersiz bakiye!");
            return;
        }

        Debug.Log(newBet);

        playerBets.Add(newBet);
        RouletteUIManager.Instance.currentBalance -= newBet.amount;
        RouletteUIManager.Instance.UpdateBalanceUI();
    }

    public void OnBallLanded(int landedNumber)
    {
        Debug.Log("Ball landed on: " + landedNumber);

        string color = GetColor(landedNumber);

        foreach (var bet in playerBets)
        {
            bool won = false;

            switch (bet.type)
            {
                case BetType.Number:
                    won = bet.value == landedNumber.ToString();
                    break;
                case BetType.Color:
                    won = bet.value.ToLower() == color.ToLower();
                    break;
                case BetType.EvenOdd:
                    won = (landedNumber % 2 == 0 && bet.value == "even") || (landedNumber % 2 == 1 && bet.value == "odd");
                    break;
                case BetType.Range:
                    int n = landedNumber;
                    if (bet.value == "1-18") won = n >= 1 && n <= 18;
                    else if (bet.value == "19-36") won = n >= 19 && n <= 36;
                    break;
            }

            if (won)
            {
                float payout = bet.amount * GetPayoutMultiplier(bet.type);
                Debug.Log($"Bet WON! {bet.type} - {bet.value} -> payout: {payout}");
                RouletteUIManager.Instance.AdjustBalance(payout);
            }
            else
            {
                Debug.Log($"Bet LOST: {bet.type} - {bet.value}");
            }
        }

        playerBets.Clear();
    }

    private string GetColor(int number)
    {
        if (number == 0) return "green";
        int[] redNumbers = new int[] {
            1,3,5,7,9,12,14,16,18,19,21,23,25,27,30,32,34,36
        };
        return redNumbers.Contains(number) ? "red" : "black";
    }

    private float GetPayoutMultiplier(BetType type)
    {
        switch (type)
        {
            case BetType.Number: return 35f;
            case BetType.Color:
            case BetType.EvenOdd:
            case BetType.Range:
                return 2f;
            default: return 1f;
        }
    }
}

public enum BetType { Number, Color, EvenOdd, Range }

[System.Serializable]
public class Bet
{
    public BetType type;
    public string value; // örnek: "17", "red", "even", "1-18"
    public float amount;
}
