using System.Collections;
using System.Collections.Generic;
//using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class PathedObjects : ObjectGroup
{
    public float gap = 1f;
    public bool relative = true;
    public bool closed = false;
    public bool orientToPath = false;

    public bool useCollisionQuads = false;
    public float collisionQuadWidth = 0f;
    public bool collisionQuadsDoubleSided = false;
    public Vector3 collisionQuadsUp = Vector3.up;

    [HideInInspector] public int cachedCount = 0;

    [ReorderableList] public Vector3[] points;

    private Vector3 startPt;
    private Vector3 endPt;
    //private Vector3 posOffsetPt;


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

    public override GameObject PlacePrefab(Vector3 position, string label = "")
    {
        GameObject spawned = base.PlacePrefab(position, label);
        if (orientToPath)
        {
            Vector3 lookDir = endPt-startPt;
            GameObject lookAtParent = new GameObject();
            lookAtParent.transform.position = spawned.transform.position;
            lookAtParent.transform.parent = transform;

            lookAtParent.name = "Spawned by "+gameObject.name;
            spawned.transform.parent = lookAtParent.transform;
            lookAtParent.transform.rotation = Quaternion.LookRotation(lookDir);
            spawned.transform.parent = transform;
            spawned.transform.SetAsFirstSibling();
            GameObject.DestroyImmediate(lookAtParent);
        }

        return spawned;
    }




    public override void Recreate()
    {
        base.Recreate();
        Vector3 offset = relative ? transform.position : Vector3.zero;
        //posOffsetPt = offset;

        typeName = "Path";

        if (points.Length > 1 && prefabs.Length > 0 && gap > 0)
        {
            bool validClosed = (closed && points.Length > 2);


            // If adding collision quads, make a parent for them
            GameObject collideParent;
            Transform collideTrans = transform;
            if (collisionQuadWidth > 0  &&  useCollisionQuads)
            {
                // Create a new walls container child
                collideParent = new GameObject();
                collideParent.name = "Collision";
                collideParent.transform.parent = transform;
                collideTrans = collideParent.transform;
                collideTrans.localPosition = Vector3.zero;
            }


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
                startPt = tempPoints[i];
                endPt = tempPoints[i + 1];

                // Place the prefab instances
                float dist = Vector3.Distance(startPt, endPt);
                float numInstances = Mathf.Floor(dist / gap);
                for (int j = 0; j < numInstances; j++)
                {
                    PlacePrefab(Vector3.Lerp(startPt, endPt, j / numInstances) + offset, (cachedCount + 1).ToString() + " --");
                    cachedCount++;
                }

                // Place the collision objects
                if (useCollisionQuads && collisionQuadWidth > 0f)
                {
                    Vector3 segmentDir = endPt - startPt;
                    Vector3 midPt = 0.5f * (startPt + endPt);
                    GameObject quadA = GameObject.CreatePrimitive(PrimitiveType.Quad);

                    quadA.layer = 9;
                    quadA.name = "top side " + i.ToString();
                    Vector3 localScale = new Vector3(collisionQuadWidth, Mathf.Abs(segmentDir.magnitude), 1f);
                    quadA.transform.localScale = localScale;

                    quadA.transform.parent = collideTrans;
                    quadA.transform.localPosition = midPt;

                    Vector3 leftDir = Vector3.Cross(segmentDir, -collisionQuadsUp);
                    quadA.transform.localRotation = Quaternion.LookRotation(Vector3.Cross(leftDir, segmentDir), -segmentDir);
                    quadA.GetComponent<Renderer>().enabled = false;

                    if (collisionQuadsDoubleSided)
                    {
                        GameObject quadB = GameObject.Instantiate(quadA, quadA.transform.position, quadA.transform.localRotation, collideTrans);
                        quadB.name = "bottom side " + i.ToString();
                        localScale.z = -1f;
                        quadB.transform.localScale = localScale;
                    }
                }
            }

            // If the path isn't closed, place one last prefab at the end point
            if (!closed || points.Length <= 2)
            {
                endPt += (endPt - startPt);
                PlacePrefab(points[points.Length - 1] + offset, (cachedCount + 1).ToString() + " --");
                cachedCount++;
            }

            // Finally, change the name to indicate the number of instances that were placed
            ChangeName();
        }
    }
}