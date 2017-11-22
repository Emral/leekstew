using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGroup : PlacementTool
{
    public bool isDirty = false;
    private int isDirtyCounter = 0;


    public override void Reset()
    {
        typeName = "GenericGroup";
    }



    public virtual void OnValidate()
    {
        if (Application.isEditor)
            isDirty = true;
    }

    public override void Update()
    {
        if (Application.isEditor && isDirty)
        {
            Clear();
        }
    }

    public virtual void LateUpdate()
    {
        if (Application.isEditor && isDirty && transform.childCount == 0)
        {
            isDirty = false;
            Recreate();
        }
    }


    public virtual GameObject PlacePrefab(Vector3 position, string label = "")
    {
        GameObject selected = RandomPrefab();
        GameObject spawned = GameObject.Instantiate(selected, position, Quaternion.identity, transform);
        spawned.transform.SetAsFirstSibling();
        if (!label.Equals(""))
            spawned.name = label + " " + selected.name;

        return spawned;
    }

    public virtual void Clear()
    {
        // Destroy all children
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            DestroyImmediate(child.gameObject);
        }
    }
    
    public virtual void Recreate()
    {
	}
}
