using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class TileObjects : ObjectGroup
{
    [HideInInspector] public Vector3 cachedScale = Vector3.one;
    public float gap = 1f;





    public override void Reset()
    {
        typeName = "Grid";
    }



    public override void Update()
    {
        transform.localScale = new Vector3(Mathf.Max(1f, Mathf.Round(transform.localScale.x)), Mathf.Max(1f, Mathf.Round(transform.localScale.y)), Mathf.Max(1f, Mathf.Round(transform.localScale.z)));

        if (!Vector3.Equals(transform.localScale, cachedScale))
        {
            cachedScale = transform.localScale;
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
            for (int x = 0; x < transform.localScale.x; x++)
            {
                for (int y = 0; y < transform.localScale.y; y++)
                {
                    for (int z = 0; z < transform.localScale.z; z++)
                    {
                        Vector3 relPos = transform.position + new Vector3(x,y,z)*gap;
                        GameObject placed = PlacePrefab(relPos, relPos.ToString() + " --");
                        placed.transform.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
                    } 
                }
            }

            ChangeName();
        }

    }
}
