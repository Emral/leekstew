using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

public class ObjectGroup : PlacementTool
{
    public bool updateAutomatically = false;
    public bool isDirty = false;

    public int firstInstanceID;
    public Vector3 rotOffset;
    public Vector3 rotRandom;
    public Vector3 posRandom;
    [HideInInspector] public int idAssigned;
    //private int isDirtyCounter = 0;

    public bool objectsHaveCollision = true;

    public GameObject clearEffect;
    public BatteryCharge[] clearPowerCharges;

    private bool playerStartedClearing = false;
    private int cachedChildCount = -99;
    private bool hasBeenCleared = false;


    public override void Reset()
    {
        typeName = "GenericGroup";
    }



    public virtual void OnValidate()
    {
        if (Application.isEditor && !Application.isPlaying && updateAutomatically)
            isDirty = true;
    }

    public override void Update()
    {
        if (Application.isPlaying)
        {
            if (playerStartedClearing && !hasBeenCleared && cachedChildCount == 0)
            {
                hasBeenCleared = true;
                if (clearEffect != null)
                    GameObject.Instantiate(clearEffect, GameManager.player.transform.position, Quaternion.identity);

                if (clearPowerCharges.Length > 0)
                {
                    foreach (BatteryCharge charge in clearPowerCharges)
                    {
                        charge.target.ReceiveCharge(charge.signal);
                    }
                }
            }

            if (transform.childCount < cachedChildCount)
            {
                playerStartedClearing = true;
            }

            cachedChildCount = transform.childCount;
        }
        else
        {
            if (isDirty && !Application.isPlaying)
                Clear();
        }
    }

    public virtual void LateUpdate()
    {
        if (isDirty && transform.childCount == 0)
        {
            isDirty = false;
            if (!Application.isPlaying)
                Recreate();
        }
    }


    public virtual GameObject PlacePrefab(Vector3 position, Vector3 rotation, string label = "")
    {
        Vector3 newPos = position + new Vector3(Random.Range(-posRandom.x, posRandom.x), Random.Range(-posRandom.y, posRandom.y), Random.Range(-posRandom.z, posRandom.z));
        Quaternion newRot = Quaternion.Euler(rotation + rotOffset + new Vector3(Random.Range(-rotRandom.x, rotRandom.x), Random.Range(-rotRandom.y, rotRandom.y), Random.Range(-rotRandom.z, rotRandom.z)));

        // Spawn a prefab instance if in the editor or an instantiated clone during runtime
        GameObject selected = RandomPrefab();
        GameObject spawned;
        #if UNITY_EDITOR
            spawned = (GameObject)PrefabUtility.InstantiatePrefab(selected);
            spawned.transform.position = newPos;
            spawned.transform.rotation = newRot;
            spawned.transform.parent = transform;
        #else
            spawned = GameObject.Instantiate(selected, newPos, newRot, transform);
        #endif
        spawned.transform.SetAsFirstSibling();

        // Disable the collision if configured to do so
        if (!objectsHaveCollision)
        {
            Collider[] colliders = spawned.GetComponentsInChildren<Collider>(true);
            CollidingEntity[] collidingEntities = spawned.GetComponentsInChildren<CollidingEntity>(true);
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
            foreach (CollidingEntity collidEn in collidingEntities)
            {
                collidEn.enabled = false;
            }
        }

        // Rename the generated instance
        if (!label.Equals(""))
            spawned.name = label + " " + selected.name;

        // Set the prefab's instance ID if it has such a variable (currently only works with Collectibles)
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
            child.gameObject.name = "I should be destroyed";
            DestroyImmediate(child.gameObject);
        }
    }
    
    public virtual void Recreate()
    {
        idAssigned = firstInstanceID - 1;
    }
}
