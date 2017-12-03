using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGroup : PlacementTool
{
    public bool isDirty = false;
    public int firstInstanceID;
    public Vector3 rotOffset;
    public Vector3 rotRandom;
    public Vector3 posRandom;
    [HideInInspector] public int idAssigned;
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


    public virtual GameObject PlacePrefab(Vector3 position, Vector3 rotation, string label = "")
    {
        Vector3 newPos = position + new Vector3(Random.Range(-posRandom.x, posRandom.x), Random.Range(-posRandom.y, posRandom.y), Random.Range(-posRandom.z, posRandom.z));
        Quaternion newRot = Quaternion.Euler(rotation + rotOffset + new Vector3(Random.Range(-rotRandom.x, rotRandom.x), Random.Range(-rotRandom.y, rotRandom.y), Random.Range(-rotRandom.z, rotRandom.z)));

        GameObject selected = RandomPrefab();
        GameObject spawned = GameObject.Instantiate(selected, newPos, newRot, transform);
        spawned.transform.SetAsFirstSibling();

        if (!label.Equals(""))
            spawned.name = label + " " + selected.name;

        Collectible collectScr = spawned.GetComponent<Collectible>();
        if (collectScr != null)
        {
            collectScr.instanceID = idAssigned++;
        }

        return spawned;
    }
    public virtual GameObject PlacePrefab(Vector3 position, string label = "")
    {
        return PlacePrefab(position, Vector3.zero, label);
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
        idAssigned = firstInstanceID - 1;
    }
}
