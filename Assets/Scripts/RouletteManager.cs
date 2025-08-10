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
    public SlotPointController[] AllSlotsEuro;
    public SlotPointController[] AllSlotsAmerican;
    public GameObject destSlot;

    public int selectedNumber;
    public bool numberSelected = false;

    public bool gameStart = false;
    public float gameTimer = 0;

    [Header("Betting System")]
    public List<Bet> playerBets = new List<Bet>();
    public Transform chipParent;
    public GameObject[] chipPrefabs;

    public float moveDuration = 1.5f;
    private Coroutine moveCoroutine;

    public Transform camera;
    public Transform cameraRoulettePos;
    public Transform cameraBetPos;

    public bool betGiven = false;

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

        Debug.Log("[Roulette] Manager initialized.");
    }

    private void Start()
    {
        soundManager.Instance.winBetLoseSource.PlayOneShot(soundManager.Instance.pleaseBetWomanSource);
        Debug.Log("[Roulette] Ready for bets.");
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
                    if (!numberSelected)
                    {
                        soundManager.Instance.winBetLoseSource.Stop();
                        gameStart = false;
                        gameTimer = 3;

                        ballController.ballSpeedController(.3f, .5f);
                        ballController.ballRB.isKinematic = false;

                        Debug.Log("[Roulette] No deterministic number. Releasing ball.");
                    }
                    else
                    {
                        soundManager.Instance.winBetLoseSource.Stop();
                        ballController.startCheckDistance = true; gameStart = false;
                        Debug.Log($"[Roulette] Deterministic target active → {selectedNumber}.");
                    }
                }
            }
        }
    }

    // Sets selected deterministic number based on current roulette type (Euro/American)
    public void SetSelectedNumber(string numberText)
    {
        if (!RouletteUIManager.Instance.isAmericanChoose)
        {
            if (int.TryParse(numberText, out int value) && value >= 0 && value < AllSlotsEuro.Length)
            {
                numberSelected = true;
                selectedNumber = value;

                for (int i = 0; i < AllSlotsEuro.Length; i++)
                {
                    if (selectedNumber == AllSlotsEuro[i].id)
                    {
                        destSlot = AllSlotsEuro[i].gameObject;
                        Debug.Log($"[Roulette] Euro target set → {selectedNumber}");
                    }
                }
            }
            else
            {
                numberSelected = false;
                selectedNumber = -1;
                Debug.Log("[Roulette] Invalid Euro target. Selection cleared.");
            }
        }
        else
        {
            if (int.TryParse(numberText, out int value) && value >= 0 && value < AllSlotsAmerican.Length)
            {
                numberSelected = true;
                selectedNumber = value;

                for (int i = 0; i < AllSlotsAmerican.Length; i++)
                {
                    if (selectedNumber == AllSlotsAmerican[i].id)
                    {
                        destSlot = AllSlotsAmerican[i].gameObject;
                        Debug.Log($"[Roulette] American target set → {selectedNumber}");
                    }
                }
            }
            else
            {
                numberSelected = false;
                selectedNumber = -1;
                Debug.Log("[Roulette] Invalid American target. Selection cleared.");
            }
        }
    }

    // Records a player bet if balance allows
    public void PlaceBet(Bet newBet)
    {
        if (newBet.amount > RouletteUIManager.Instance.currentBalance)
        {
            Debug.LogWarning("[Roulette] Not enough balance to place bet.");
            return;
        }

        soundManager.Instance.winBetLoseSource.PlayOneShot(soundManager.Instance.puttingBetSource);
        playerBets.Add(newBet);

        Debug.Log($"[Roulette] Bet placed → {newBet.type} {newBet.value} | ${newBet.amount}");
    }

    // Smooth camera move to a target transform
    public void MoveCameraTo(Transform target)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveCameraRoutine(target));
    }

    private IEnumerator MoveCameraRoutine(Transform target)
    {
        Vector3 startPos = camera.transform.position;
        Quaternion startRot = camera.transform.rotation;

        Vector3 endPos = target.position;
        Quaternion endRot = target.rotation;

        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime * 3;
            float t = time / moveDuration;

            camera.transform.position = Vector3.Lerp(startPos, endPos, t);
            camera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        camera.transform.position = endPos;
        camera.transform.rotation = endRot;

        moveCoroutine = null;
        Debug.Log("[Roulette] Camera moved.");
    }

    // Evaluates bets after the ball lands
    public void OnBallLanded(int landedNumber)
    {
        Debug.Log($"[Roulette] Ball landed → {landedNumber}");

        string color = GetColor(landedNumber);

        int totalStake = 0;
        int totalWin = 0;

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
                    won = (landedNumber % 2 == 0 && bet.value == "even") ||
                          (landedNumber % 2 == 1 && bet.value == "odd");
                    break;
                case BetType.Range:
                    int n = landedNumber;
                    if (bet.value == "1-18") won = n >= 1 && n <= 18;
                    else if (bet.value == "19-36") won = n >= 19 && n <= 36;
                    else if (bet.value == "1-12") won = n >= 1 && n <= 12;
                    else if (bet.value == "13-24") won = n >= 13 && n <= 24;
                    else if (bet.value == "25-36") won = n >= 25 && n <= 36;
                    break;
            }

            int stake = (int)bet.amount;
            totalStake += stake;

            if (won)
            {
                int multiplier = (int)GetPayoutMultiplier(bet.type);
                int payout = stake * (multiplier + 1);
                totalWin += payout;

                RouletteUIManager.Instance.AdjustBalance(payout);
                Debug.Log($"[Roulette] WIN → {bet.type} {bet.value} | +${payout}");
            }
            else
            {
                Debug.Log($"[Roulette] LOSE → {bet.type} {bet.value} | -${stake}");
            }
        }

        int net = totalWin - totalStake;
        Debug.Log($"[Roulette] ROUND → Stake:${totalStake} Win:${totalWin} Net:{net}");

        RouletteUIManager.Instance.PlayWinLose(Mathf.Abs(net), net >= 0);
        playerBets.Clear();
    }

    private string GetColor(int number)
    {
        if (number == 0) return "green";
        int[] redNumbers = new int[] { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
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
    public string value;
    public int amount;
}
