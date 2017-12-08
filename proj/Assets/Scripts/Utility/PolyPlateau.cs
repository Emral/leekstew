using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PointEditType {None, Single, All};

[ExecuteInEditMode]
public class PolyPlateau : MonoBehaviour
{
    public bool checkToRefresh = false;

    public PointEditType pointEditMode = PointEditType.None;
    public int editPoint = 0;

    [ReorderableList]public Vector2[] points;
    public float height = 1f;

    public bool interior = false;
    public bool hasFloor = true;
    public bool hasCeiling = true;

    public IntRange[] wallsToSkip;

    public Material floorMat;
    public Material wallTopMat;
    public Material wallMat;
    public Material ceilingMat;

    [HideInInspector]public bool isDirty = false;

    private int cachedPointCount = 0;
    private int cachedSkipsCount = 0;

    private MeshFilter floorFilter;
    private MeshFilter ceilingFilter;

    private GameObject[] wallQuads;

    private Mesh groundMesh;

    private Transform floorTrans;
    private Transform ceilingTrans;
    private Transform wallsTrans;


    // Use this for initialization
    private void Awake ()
    {
    }


    public void OnDrawGizmos()
    {
        Vector3 offset = transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(offset - Vector3.up*4f, offset + Vector3.up*(height+4f));

        Gizmos.color = Color.blue;
        if (points.Length > 1)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector2 pA = points[i];
                Vector2 pB = points[i+1];
                Gizmos.DrawLine(offset + new Vector3(pA.x, 0f, pA.y),offset + new Vector3(pB.x, 0f, pB.y));
            }
            Vector2 pC = points[points.Length-1];
            Vector2 pD = points[0];
            Gizmos.DrawLine(offset + new Vector3(pC.x, 0f, pC.y), offset + new Vector3(pD.x, 0f, pD.y));
        }
    }


    public void OnValidate()
    {
        editPoint = editPoint % points.Length;
        if (editPoint < 0)
        {
            editPoint = points.Length - 1;
        }

        if (cachedPointCount != points.Length || cachedSkipsCount != wallsToSkip.Length)
        {
            cachedPointCount = points.Length;
            cachedSkipsCount = wallsToSkip.Length;
            foreach (IntRange range in wallsToSkip)
            {
                range.TotalMinValue = 0;
                range.TotalMaxValue = points.Length;
                if (range.MinValue == range.MaxValue)
                {
                    if (range.MinValue > 0)
                        range.MinValue--;
                    else if (range.MaxValue < range.TotalMaxValue)
                        range.MaxValue++;
                }
            }
        }



        if (checkToRefresh)
        {
            //checkToRefresh = false;
            //isDirty = true;
        }
    }

    public void Update()
    {
        floorTrans = transform.Find("floor");
        ceilingTrans = transform.Find("ceiling");
        wallsTrans = transform.Find("walls");

        floorFilter = floorTrans.GetComponent<MeshFilter>();
        ceilingFilter = ceilingTrans.GetComponent<MeshFilter>();

        if (groundMesh == null)
            groundMesh = new Mesh();


        if (Application.isEditor && isDirty)
        {
            //print("Running Update because a plateau is dirty");
            if (wallsTrans != null)
            {
                GameObject go = wallsTrans.gameObject;
                wallsTrans = null;
                GameObject.DestroyImmediate(go);
            }
        }
    }

    public void LateUpdate()
    {
        if (Application.isEditor && isDirty)
        {
            //print("Running LateUpdate because a plateau is dirty");

            // Make sure the walls have been deleted beyond a shadow of a doubt
            bool deleted = (wallsTrans == null);

            if (!deleted)
            {
                deleted = wallsTrans.gameObject == null;

                if (deleted)
                    wallsTrans = null;
            }

            // If they have been deleted, refresh the mesh
            if (deleted && floorTrans != null && ceilingTrans != null)
            {
                isDirty = false;
                Refresh();
            }
        }
    }

    public void Refresh()
    {
        print("Refreshing...");
        if (points.Length > 2)
        {
            // Misc. calculations and stuff
            float interiorMult = (interior ? -1f : 1f);

            // Reset child transforms and stuff
            floorTrans.localPosition = interior ? Vector3.zero : Vector3.up * height;
            floorTrans.localScale = new Vector3(1f, -1, 1f);
            ceilingTrans.localPosition = !interior ? Vector3.zero : Vector3.up * height;

            // Create a new walls container child
            GameObject newWallsParent = new GameObject();
            newWallsParent.name = "walls";
            newWallsParent.transform.parent = transform;
            wallsTrans = newWallsParent.transform;
            wallsTrans.localPosition = Vector3.zero;

            // Reset the mesh
            print("Rebuilding the model...");
            groundMesh.Clear();

            // Get the walls to exclude
            List<int> skippedWalls = new List<int>();

            foreach (IntRange range in wallsToSkip)
            {
                for (int ii = range.MinValue; ii < range.MaxValue; ii++)
                {
                    if (!skippedWalls.Contains(ii))
                        skippedWalls.Add(ii);
                }
            }

            // Make the vertices and UVs, add the wall quads (also get total x and y size)
            Vector2 lowestPoint = new Vector2(Mathf.Infinity, Mathf.Infinity);
            Vector2 highestPoint = new Vector2(-Mathf.Infinity, -Mathf.Infinity);
            int i = 0;
            Vector3[] verts = new Vector3[points.Length + 1];
            Vector2[] uvs = new Vector2[points.Length + 1];
            foreach (Vector2 point in points)
            {
                highestPoint.x = Mathf.Max(point.x, highestPoint.x);
                highestPoint.y = Mathf.Max(point.y, highestPoint.y);
                lowestPoint.x = Mathf.Min(point.x, lowestPoint.x);
                lowestPoint.y = Mathf.Min(point.y, lowestPoint.y);

                //print("POINT: " + point.ToString());
                verts[i] = new Vector3(point.x, 0f, point.y);
                uvs[i] = points[i];
                i++;
            }

            // Add the center point as the last UV and the first point as the last vert for the sake of the next for loop
            verts[i] = verts[0];
            uvs[i] = Vector2.zero;

            // Make the tris and walls
            int[] tris = new int[3 * points.Length];
            for (i = 0; i < points.Length; i++)
            {
                // Make the walls
                if (!skippedWalls.Contains(i))
                {
                    Vector3 wallMidPoint = 0.5f * (verts[i] + verts[i + 1]);
                    float segmentLength = Vector3.Distance(verts[i + 1], verts[i]);
                    Quaternion wallDirection = Quaternion.LookRotation(Vector3.Cross(verts[i + 1] - verts[i], interiorMult*Vector3.up));

                    GameObject planeA = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    planeA.name = "Top Wall " + i.ToString();
                    planeA.transform.parent = wallsTrans;
                    planeA.transform.localRotation = wallDirection;

                    Renderer renderA = planeA.GetComponent<Renderer>();
                    renderA.sharedMaterial = wallTopMat;

                    // If the height is greater than 1, have separate top and bottom quads for autotexturing stuffs
                    if (height > 1f)
                    {
                        planeA.transform.localPosition = new Vector3(wallMidPoint.x, height - 0.5f, wallMidPoint.z);
                        planeA.transform.localScale = new Vector3(segmentLength, 1f, 1f);

                        GameObject planeB = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        planeB.name = "Wall " + i.ToString();
                        planeB.transform.parent = wallsTrans;
                        planeB.transform.localPosition = wallMidPoint + (Vector3.up * (height - 1f) * 0.5f);
                        planeB.transform.localScale = new Vector3(segmentLength, height - 1f, 1f);
                        planeB.transform.localRotation = wallDirection;

                        Renderer renderB = planeB.GetComponent<Renderer>();
                        renderB.sharedMaterial = wallMat;
                        renderB.material.mainTextureScale = new Vector2(planeB.transform.localScale.x, planeB.transform.localScale.y);
                    }
                    else
                    {
                        planeA.transform.localPosition = new Vector3(wallMidPoint.x, height * 0.5f, wallMidPoint.z);
                        planeA.transform.localScale = new Vector3(segmentLength, height, 1f);
                    }

                    renderA.material.mainTextureScale = new Vector2(planeA.transform.localScale.x, planeA.transform.localScale.z);
                }

                // Make the tris
                tris[3 * i] = verts.Length - 1;
                tris[3 * i + 1] = i;
                tris[3 * i + 2] = i + 1;
            }

            // Correct the final tri and vert points
            verts[verts.Length - 1] = Vector3.zero;
            tris[tris.Length - 1] = 0;

            // Assemble the mesh
            groundMesh.vertices = verts;
            groundMesh.uv = uvs;
            groundMesh.triangles = tris;
            //groundMesh.RecalculateNormals();


            // Assign the mesh
            if (hasFloor)
            {
                floorTrans.GetComponent<MeshFilter>().sharedMesh = groundMesh;
                floorTrans.GetComponent<MeshCollider>().sharedMesh = groundMesh;

                Renderer renderF = floorTrans.GetComponent<Renderer>();
                renderF.sharedMaterial = floorMat;
                renderF.material.mainTextureScale = Vector2.one;
            }
            else
            {
                floorTrans.GetComponent<MeshFilter>().sharedMesh = null;
                floorTrans.GetComponent<MeshCollider>().sharedMesh = null;
            }
            
            if (hasCeiling)
            { 
                ceilingTrans.GetComponent<MeshFilter>().sharedMesh = groundMesh;
                ceilingTrans.GetComponent<MeshCollider>().sharedMesh = groundMesh;

                Renderer renderC = ceilingTrans.GetComponent<Renderer>();
                renderC.sharedMaterial = ceilingMat;
                renderC.material.mainTextureScale = Vector2.one;
            }
            else
            {
                ceilingTrans.GetComponent<MeshFilter>().sharedMesh = null;
                ceilingTrans.GetComponent<MeshCollider>().sharedMesh = null;
            }

            Resources.UnloadUnusedAssets();

            print("Done!");
        }
    }
}
