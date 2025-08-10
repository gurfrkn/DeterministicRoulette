using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ballController : MonoBehaviour
{
    public int slotTarget;
    SlotPointController slotPointController;

    public Rigidbody ballRB;

    bool lockBallOnTarget = false;
    Transform ballTarget;

    public float currentBallSpeed;
    private Coroutine speedLerpCoroutine;

    public bool startCheckDistance = false;

    private Vector3 smoothVelocity;

    float ballDistanceToSlot;
    public float minDistanceToSlotToJump = 3f;

    private void Awake()
    {
        ballRB = GetComponent<Rigidbody>();
        ballRB.isKinematic = true;

        ballSpeedController(150f, 0.1f);
        Debug.Log("Ball initialized with starting speed.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("slot")) return;

        slotPointController = other.GetComponent<SlotPointController>();

        if (startCheckDistance)
        {
            if (slotPointController.id == RouletteManager.Instance.destSlot.GetComponent<SlotPointController>().id)
            {
                RouletteManager.Instance.OnBallLanded(slotPointController.id);
                Debug.Log($"Ball landed on target slot: {slotPointController.id}");
            }
        }
        else
        {
            ballSpeedController(0f, 0.1f);
            RouletteManager.Instance.OnBallLanded(slotPointController.id);

            lockBallOnTarget = true;
            ballTarget = slotPointController.transform;

            Debug.Log($"Ball locked to slot: {slotPointController.id}");
        }
    }

    private void Update()
    {
        if (lockBallOnTarget)
        {
            if (startCheckDistance)
            {
                if (Vector3.Distance(transform.position, ballTarget.position) > 0.2f)
                {
                    // Small upward bounce effect
                    ballRB.AddForce(Vector3.up * 100, ForceMode.Acceleration);

                    // Smooth movement towards the target slot
                    Vector3 newPos = Vector3.SmoothDamp(
                        ballRB.position,
                        ballTarget.position,
                        ref smoothVelocity,
                        0.06f
                    );
                    ballRB.MovePosition(newPos);
                }
                else
                {
                    transform.position = ballTarget.position;
                    Debug.Log("Ball reached target position.");
                }
            }
            else
            {
                transform.position = ballTarget.position;
            }
            return;
        }

        if (startCheckDistance)
            ballDistanceCheck();

        // Natural rotation around the wheel
        transform.Rotate(transform.right * 100f * Time.deltaTime);
        transform.RotateAround(RouletteManager.Instance.sphereCenter.position, Vector3.up, currentBallSpeed * Time.deltaTime);
    }

    public void ballSpeedController(float targetSpeed, float duration)
    {
        if (speedLerpCoroutine != null)
            StopCoroutine(speedLerpCoroutine);

        speedLerpCoroutine = StartCoroutine(LerpBallSpeed(targetSpeed, duration));
    }

    private IEnumerator LerpBallSpeed(float targetSpeed, float duration)
    {
        float startSpeed = currentBallSpeed;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            currentBallSpeed = Mathf.Lerp(startSpeed, targetSpeed, time / duration);
            yield return null;
        }

        currentBallSpeed = targetSpeed;
        speedLerpCoroutine = null;
        Debug.Log($"Ball speed changed to {targetSpeed}.");
    }

    public void ballDistanceCheck()
    {
        ballSpeedController(0.5f, 1f);

        ballDistanceToSlot = Vector3.Distance(transform.position, RouletteManager.Instance.destSlot.transform.position);

        if (ballDistanceToSlot <= minDistanceToSlotToJump)
        {
            ballTarget = RouletteManager.Instance.destSlot.transform;
            lockBallOnTarget = true;
            Debug.Log("Target slot within jump range. Locking ball.");
        }
    }
}
