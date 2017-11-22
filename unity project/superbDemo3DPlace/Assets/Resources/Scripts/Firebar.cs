using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firebar : MonoBehaviour
{
    public float gap = 1f;
    public int length = 4;

    private PathedObjects path;
    private Transform rotateTrans;


    void Start ()
    {
        UpdateReferences();

        if (path != null)
        {
            path.gap = gap;
            path.points[1] = path.points[0] + new Vector3(length*gap,0f,0f);
            path.Recreate();
        }
    }

    void UpdateReferences ()
    {
        rotateTrans = transform.GetChild(0);
        path = rotateTrans.GetComponentInChildren<PathedObjects>();
	}
}
