using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class PathedObjects : ObjectGroup
{
    public float gap = 1f;
    public bool relative = true;
    public bool closed = false;
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
            bool validClosed = (closed && points.Length > 2);

            // Set up a copy array for the sake of closed path stuff
            Vector3[] tempPoints;
            if (validClosed)
            {
                tempPoints = new Vector3[points.Length + 1];
                tempPoints[points.Length] = points[0] + Vector3.zero;
            }
            else
            {
                tempPoints = new Vector3[points.Length];
            }


            // Copy all the points to the copy array
            for (int i = 0; i < points.Length; i++)
            {
                tempPoints[i] = points[i] + Vector3.zero;
            }


            // Now loop through the copy array
            cachedCount = 0;
            for (int i = 0; i < tempPoints.Length - 1; i++)
            {
                Vector3 startPt = tempPoints[i];
                Vector3 endPt = tempPoints[i+1];
                float dist = Vector3.Distance(startPt, endPt);

                float numInstances = Mathf.Floor(dist / gap);
                for (int j = 0; j < numInstances; j++)
                {
                    PlacePrefab(Vector3.Lerp(startPt, endPt, j/numInstances) + offset, (cachedCount + 1).ToString() + " --");
                    cachedCount++;
                }
            }

            // If the path isn't closed, place one last prefab at the end point
            if (!closed || points.Length <= 2)
            {
                PlacePrefab(points[points.Length - 1] + offset, (cachedCount + 1).ToString() + " --");
                cachedCount++;
            }

            // Finally, change the name to indicate the number of instances that were placed
            ChangeName();
        }
    }
}