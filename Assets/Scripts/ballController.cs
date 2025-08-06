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

    private void Awake()
    {
        ballRB = GetComponent<Rigidbody>();
        ballRB.isKinematic = true;

        ballSpeedController(150f, 0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("slot"))
        {
            slotPointController = other.GetComponent<SlotPointController>();

            ballSpeedController(0f, 0.1f);

            RouletteManager.Instance.OnBallLanded(slotTarget);
        }
    }

    private void Update()
    {
        if (lockBallOnTarget)
        {
            transform.position = ballTarget.position;
            return;
        }

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
            float t = time / duration;
            currentBallSpeed = Mathf.Lerp(startSpeed, targetSpeed, t);
            yield return null;
        }

        currentBallSpeed = targetSpeed;
        speedLerpCoroutine = null;
    }

    public bool IsTargetFeasible(Vector3 targetPosition, float maxDistance = 1.5f, float maxAngle = 45f)
    {
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance > maxDistance) return false;

        Vector3 toTarget = (targetPosition - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toTarget);

        return angle <= maxAngle;
    }

    public void JumpToTarget(Vector3 targetPosition, float upwardForce = 3f, float forwardForce = 2f)
    {
        ballRB.velocity = Vector3.zero;
        ballRB.angularVelocity = Vector3.zero;

        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector3 force = direction * forwardForce + Vector3.up * upwardForce;

        ballRB.AddForce(force, ForceMode.VelocityChange);
    }

    private Coroutine jumpWaitCoroutine;

    public void WaitAndJumpToTarget(Vector3 targetPosition, float upwardForce = 3f, float forwardForce = 2f, float checkInterval = 0.05f)
    {
        if (jumpWaitCoroutine != null)
            StopCoroutine(jumpWaitCoroutine);

        jumpWaitCoroutine = StartCoroutine(WaitUntilFeasibleAndJump(targetPosition, upwardForce, forwardForce, checkInterval));
    }

    private IEnumerator WaitUntilFeasibleAndJump(Vector3 targetPosition, float upwardForce, float forwardForce, float checkInterval)
    {
        while (true)
        {
            if (IsTargetFeasible(targetPosition))
            {
                JumpToTarget(targetPosition, upwardForce, forwardForce);
                jumpWaitCoroutine = null;
                yield break;
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

}
