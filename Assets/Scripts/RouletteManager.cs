using System.Collections;
using UnityEngine;
using System.Linq;

public class RouletteManager : MonoBehaviour
{
    public static RouletteManager Instance { get; private set; }


    [Header("Wheel Settings")]
    public Transform wheel;
    public float currentWheelSpeed = 0f;

    [Header("Ball Settings")]
    public GameObject ballPrefab;
    public GameObject ballObject;

    public ballController ballController;

    public Transform ballSpawnPoint;
    public Transform sphereCenter;

    public float currentBallSpeed = 0f;

    public float ballOrbitRadius = 2f;
    public float orbitSpeed = 300f;

    [Header("Slot Settings")]
    public GameObject[] allSlots; 

    [HideInInspector] public int selectedNumber;

    public bool gameStart = false;
    public float gameTimer = 0;

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

                if(gameTimer <= 0)
                {
                    gameStart = false;
                    gameTimer = 3;

                    ballController.ballRB.isKinematic = false;
                }
            }
        }
    }

    public void Spin()
    {
        currentBallSpeed *= 3;
        gameTimer = 3;

        gameStart = true;
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
            ballController.slotTarget = null;
        }
    }

}
