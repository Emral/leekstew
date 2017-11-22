using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class PathedObjects : ObjectGroup
{
    public float gap = 1f;
    public bool relative = true;
    [HideInInspector] public int cachedCount = 0;

    [ReorderableList] public Vector3[] points;



    public override void Reset()
    {
        typeName = "Path";
    }


    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (points.Length > 1)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector3 offset = relative ? transform.position : Vector3.zero;
                Gizmos.DrawLine(offset+points[i], offset + points[i + 1]);
            }
        }
    }


    public override void Recreate()
    {
        base.Recreate();
        Vector3 offset = relative ? transform.position : Vector3.zero;

        typeName = "Path";

        if (points.Length > 1 && prefabs.Length > 0 && gap > 0)
        {
            cachedCount = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector3 startPt = points[i];
                Vector3 endPt = points[i+1];
                float dist = Vector3.Distance(startPt, endPt);

                float numInstances = Mathf.Floor(dist / gap);
                for (int j = 0; j < numInstances; j++)
                {
                    PlacePrefab(Vector3.Lerp(startPt, endPt, j/numInstances) + offset, (cachedCount + 1).ToString() + " --");
                    cachedCount++;
                }
            }
            PlacePrefab(points[points.Length - 1] + offset, (cachedCount+1).ToString() + " --");
            cachedCount++;

            ChangeName();
        }
    }
}