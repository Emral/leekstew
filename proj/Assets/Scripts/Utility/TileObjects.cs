using System.Collections;
using System.Collections.Generic;
//using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class TileObjects : ObjectGroup
{
    public float gap = 1f;
    public Vector3 size = Vector3.one;

    public bool singleCollider = false;


    public void OnDrawGizmosSelected()
    {
        Vector3 offset = transform.position;
        Gizmos.color = Color.blue;
        if (gap > 0f)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        Gizmos.DrawWireCube(offset + Vector3.up*0.5f + new Vector3(x,y,z)*gap, Vector3.one*gap);
                    }
                }
            }

        }
    }


    public override void Reset()
    {
        typeName = "Grid";
    }


    public override void Update()
    {
        transform.localScale = new Vector3(Mathf.Max(0f, Mathf.Round(transform.localScale.x)), Mathf.Max(0f, Mathf.Round(transform.localScale.y)), Mathf.Max(0f, Mathf.Round(transform.localScale.z)));

        if (!Vector3.Equals(transform.localScale, Vector3.one))
        {
            size = new Vector3 (Mathf.Max(1f, size.x + transform.localScale.x-1f),
                                Mathf.Max(1f, size.y + transform.localScale.y - 1f),
                                Mathf.Max(1f, size.z + transform.localScale.z - 1f));
            transform.localScale = Vector3.one;
            isDirty = true;
        }
        base.Update();
    }



    public override void Recreate ()
    {
        base.Recreate();

        typeName = "Grid";

        if (prefabs.Length > 0 && gap > 0)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        Vector3 relPos = transform.position + new Vector3(x,y,z)*gap;
                        GameObject placed = PlacePrefab(relPos, relPos.ToString() + " --");
                        placed.transform.localScale = Vector3.one;
                    } 
                }
            }

            // Consolidated collision
            if (singleCollider)
            {
                GameObject newBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newBox.name = "Collision";
                newBox.layer = prefabs[0].layer;

                newBox.transform.parent = transform;
                newBox.transform.localPosition = (size - new Vector3(1f, 0f, 1f)) * 0.5f;
                newBox.transform.localScale = size;

                MeshRenderer rend = newBox.GetComponent<MeshRenderer>();
                rend.enabled = false;

                CollidingEntity oldCde = prefabs[0].GetComponent<CollidingEntity>();
                if (oldCde != null)
                {
                    newBox.AddComponent<CollidingEntity>(oldCde);
                }
            }

            ChangeName();
        }

    }
}
