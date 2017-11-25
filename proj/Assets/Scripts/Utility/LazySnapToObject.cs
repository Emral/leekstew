using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazySnapToObject : MonoBehaviour
{

    public Transform target;

    public bool moveX = true;
    public bool moveY = true;
    public bool moveZ = true;

    public bool rotation = false;
    public float rate = 0.2f;

    void Update ()
    {
        if (target != null)
        {
            Vector3 targetPos = new Vector3(moveX ? target.position.x : transform.position.x,
                                            moveY ? target.position.y : transform.position.y,
                                            moveZ ? target.position.z : transform.position.z);

            transform.position = Vector3.Lerp(transform.position, targetPos, rate);
            if (rotation)
                transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, rate);
        }
    }
}
