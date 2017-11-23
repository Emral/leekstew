using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class ArcObjects : ObjectGroup
{
    public float gap = 1f;
    public Vector3 endPoint;// = transform.position + Vector3.right;
    public Vector3 controlPoint;// = transform.position + (Vector3.up + Vector3.right)*0.5f;

    public bool checkToCenterControlPoint = false;

    [HideInInspector] public int cachedCount = 0;



    public override void Reset()
    {
        typeName = "Arc";
    }


    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (transform.childCount > 1)
        {
            for (int i = 0; i < transform.childCount - 1; i++)
            {
                Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i+1).position);
            }
        }
    }

    public static Vector3 GetCurvePoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            oneMinusT * oneMinusT * p0 +
            2f * oneMinusT * t * p1 +
            t * t * p2;
    }


    public override void Recreate()
    {
        base.Recreate();

        typeName = "Arc";

        if (endPoint != null && controlPoint != null && gap > 0)
        {
            if  (checkToCenterControlPoint)
            {
                checkToCenterControlPoint = false;
                controlPoint = 0.5f*(endPoint);
            }

            float length = 0f;
            Vector3 prevPoint = transform.position;
            int resolution = 64;
            for (int i = 1; i < resolution; i++)
            {
                Vector3 currentPoint = GetCurvePoint(transform.position, transform.position+controlPoint, transform.position + endPoint, i / (resolution - 1f));
                length += Vector3.Distance(currentPoint, prevPoint);
                prevPoint = currentPoint;
            }

            int numInstances = Mathf.FloorToInt(length / gap);

            cachedCount = 0;
            for (int j = 0; j < numInstances; j++)
            {
                PlacePrefab(GetCurvePoint(transform.position, transform.position + controlPoint, transform.position + endPoint, j / (numInstances - 1f)), (cachedCount + 1).ToString() + " --");
                cachedCount++;
            }

            ChangeName();
        }
    }
}