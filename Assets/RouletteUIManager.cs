using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RouletteUIManager : MonoBehaviour
{
    public static RouletteUIManager Instance;

    [Header("Chip & Balance Settings")]
    public int selectedChipValue = 1;
    public int currentBalance;
    public TextMeshProUGUI balanceText;

    [Header("UI References")]
    public Button spinButtonImg;
    public GameObject chipsUi;
    public GameObject inputUi;
    public GameObject americanEuroBack;

    [Header("Total Balance Display")]
    public Image totalBalanceBack;
    public TextMeshProUGUI totalBalanceText;
    public Color totalBalanceBackLoseColor;
    public Color totalBalanceBackWinColor;

    [Header("Tour Info")]
    public int tourCounter = 0;
    public TextMeshProUGUI tourTxt;

    [Header("Win/Lose Banner")]
    public RectTransform winLoseRect;
    public TextMeshProUGUI winLoseText;
    public Color winLoseTextLoseColor;
    public Color winLoseTextWinColor;
    private Coroutine _bannerRoutine;
    [Range(0.1f, 2f)] public float slideDuration = 0.6f;
    public float startOffsetY = -400f;
    public float endOffsetY = 0f;

    [Header("American/Euro Mode")]
    public Image americanChooseButtonImage;
    public Image euroChooseButtonImage;
    public bool isAmericanChoose = false;

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
        // Load balance from PlayerPrefs or set default
        if (PlayerPrefs.HasKey("currentBalance"))
            currentBalance = PlayerPrefs.GetInt("currentBalance");
        else
        {
            currentBalance = 10000;
            PlayerPrefs.SetInt("currentBalance", 10000);
        }

        // Load tour count
        tourCounter = PlayerPrefs.GetInt("tourCounter", 0);
        tourTxt.text = "TOUR " + tourCounter;

        UpdateBalanceUI();
        americanOrEuroSelect(isAmericanChoose);
    }

    public void americanOrEuroSelect(bool isAmerican)
    {
        isAmericanChoose = isAmerican;

        // Set button highlight
        americanChooseButtonImage.color = isAmerican ? Color.white : new Color(1, 1, 1, 0);
        euroChooseButtonImage.color = isAmerican ? new Color(1, 1, 1, 0) : Color.white;

        // Toggle roulette items
        foreach (GameObject item in RouletteManager.Instance.americanRouletteItems)
            item.SetActive(isAmerican);

        foreach (GameObject item in RouletteManager.Instance.euroRouletteItems)
            item.SetActive(!isAmerican);
    }

    public void UpdateBalanceUI()
    {
        // Update total gain/loss display
        if (currentBalance >= 10000)
        {
            totalBalanceBack.color = totalBalanceBackWinColor;
            totalBalanceText.text = "TOTAL : " + (currentBalance - 10000);
        }
        else
        {
            totalBalanceBack.color = totalBalanceBackLoseColor;
            totalBalanceText.text = "TOTAL : -" + (10000 - currentBalance);
        }

        // Update balance text
        balanceText.text = $"BALANCE : ${currentBalance}";
        PlayerPrefs.SetInt("currentBalance", currentBalance);
    }

    public bool HasEnoughBalance(int amount) => currentBalance >= amount;

    public void AdjustBalance(int amount)
    {
        currentBalance += amount;
        UpdateBalanceUI();
    }

    public void SelectChip(int value)
    {
        selectedChipValue = value;
        Debug.Log($"Chip selected: {value}");
    }

    public void SpinButton()
    {
        if (!RouletteManager.Instance.betGiven)
            return;

        // Increase tour counter
        tourCounter++;
        PlayerPrefs.SetInt("tourCounter", tourCounter);
        tourTxt.text = "TOUR " + tourCounter;

        // Start wheel spin
        RouletteManager.Instance.ballController.ballSpeedController(300, 0.1f);
        RouletteManager.Instance.gameTimer = 3;
        RouletteManager.Instance.gameStart = true;
        RouletteManager.Instance.MoveCameraTo(RouletteManager.Instance.cameraRoulettePos);

        // Hide betting UI
        spinButtonImg.gameObject.SetActive(false);
        chipsUi.SetActive(false);
        inputUi.SetActive(false);
        americanEuroBack.SetActive(false);

        soundManager.Instance.winBetLoseSource.PlayOneShot(soundManager.Instance.ballWheelSource);
    }

    public void PlayWinLose(float amount, bool isWin)
    {
        if (winLoseRect == null || winLoseText == null) return;

        winLoseText.gameObject.SetActive(true);

        string sign = isWin ? "WIN" : "LOSE";
        winLoseText.text = $"{sign}: ${amount:0}";
        winLoseText.color = isWin ? winLoseTextWinColor : winLoseTextLoseColor;

        soundManager.Instance.winBetLoseSource.PlayOneShot(
            isWin ? soundManager.Instance.winSource : soundManager.Instance.loseSource
        );

        if (_bannerRoutine != null) StopCoroutine(_bannerRoutine);
        _bannerRoutine = StartCoroutine(SlideInFromBottom());
    }

    private IEnumerator SlideInFromBottom()
    {
        // Initial & final positions
        Vector2 endPos = new Vector2(winLoseRect.anchoredPosition.x, endOffsetY);
        Vector2 startPos = new Vector2(endPos.x, endOffsetY + startOffsetY);

        winLoseRect.anchoredPosition = startPos;
        winLoseRect.localScale = Vector3.one * 0.95f;

        // Smooth slide animation
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / slideDuration;
            float ease = Mathf.SmoothStep(0f, 1f, t);
            winLoseRect.anchoredPosition = Vector2.Lerp(startPos, endPos, ease);
            winLoseRect.localScale = Vector3.Lerp(Vector3.one * 0.95f, Vector3.one, ease * 0.7f);
            yield return null;
        }

        winLoseRect.anchoredPosition = endPos;
        winLoseRect.localScale = Vector3.one;

        yield return new WaitForSeconds(2);

        // Restart the scene for next round
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
