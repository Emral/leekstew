using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnHeadToLook : MonoBehaviour
{
    public Transform target;
    public Transform head;
    public Vector3 offset;
    public float maxAngle = 60;
    public float maxDistance = 2f;

    private Vector3 normalForwardEuler;

	// Use this for initialization
	void Start ()
    {
        normalForwardEuler = head.forward;
    }

    // Update is called once per frame
    void Update ()
    {
        Quaternion turnForward = Quaternion.LookRotation((target.position + offset) - head.position, Vector3.up);
        Quaternion normalForward = Quaternion.LookRotation(normalForwardEuler, Vector3.up);
        Quaternion currentForward = Quaternion.LookRotation(head.forward, Vector3.up);
        Quaternion goalForward;

        goalForward = normalForward;
        if (Vector3.Distance(target.position, transform.position) < maxDistance && Quaternion.Angle(turnForward, normalForward) <= maxAngle)
            goalForward = turnForward;

        head.rotation = Quaternion.Slerp(currentForward,goalForward,0.125f);
	}
}
