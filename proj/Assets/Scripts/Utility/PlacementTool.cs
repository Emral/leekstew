using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementTool : MonoBehaviour
{

    public GameObject[] prefabs;
    public Transform[] toggledObjs;

    [HideInInspector] public string typeName = "GenericPlacer";


    public virtual void Reset()
    {
        typeName = "GenericGroup";
    }

    public virtual void Update()
    {
    }


    public virtual GameObject RandomPrefab()
    {
        if (prefabs.Length > 0)
            return prefabs[Random.Range(0, prefabs.Length - 1)];
        else
            return null;
    }

    public virtual string PrefabsName()
    {
        return prefabs.Length > 1 ? "Mixed Objects" : prefabs[0].name;
    }

    public virtual int PlacedCount()
    {
        return transform.childCount;
    }

    public virtual void ChangeName()
    {
        gameObject.name = PrefabsName() + (transform.childCount > 1 ? " x" + transform.childCount.ToString() : "") + " (" + typeName + ")";
    }
}
