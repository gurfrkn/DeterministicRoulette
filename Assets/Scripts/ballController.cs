using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ballController : MonoBehaviour
{
    public int? slotTarget;
    SlotPointController SlotPointController;

    public Rigidbody ballRB;

    bool lockBallOnTarget = false;
    Transform ballTarget;

    private void Awake()
    {
        ballRB = GetComponent<Rigidbody>();
        ballRB.isKinematic = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("slot"))
        {
            SlotPointController = other.gameObject.GetComponent<SlotPointController>();

            if(slotTarget == SlotPointController.id)
            {
                ballRB.velocity = Vector3.zero;
                ballRB.angularVelocity = Vector3.zero;

                lockBallOnTarget = true; ballTarget = other.gameObject.transform;

            }
            else
            {

            }
        }
    }

    private void Update()
    {
        if (lockBallOnTarget)
        {
            transform.position = ballTarget.transform.position;

            return;
        }

        gameObject.transform.Rotate(gameObject.transform.right * 100 * Time.deltaTime);

        gameObject.transform.RotateAround(RouletteManager.Instance.sphereCenter.position, Vector3.up, RouletteManager.Instance.currentBallSpeed * Time.deltaTime);

    }
}
