using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PrewarmType { False, Single, Full };

public class Generator : PlacementTool
{
    public bool active = true;

    public int simultaneousLimit = 1;
    public int totalLimit = 0;
    public float spawnCooldown = 2f;
    public float spawnDelay = 0.5f;
    public bool spawnInWaves = false;

    public PrewarmType prewarm = PrewarmType.Full;
    public float spawnRadius = 0f;

    public GameObject spawnEffect;

    private int totalSpawned = 0;
    private List<Transform> spawnedObjs = new List<Transform>();

    private bool exhausted = false;
    private bool full = false;
    private bool waveActive = false;
    private int waveSpawnCount = 0;





    #region monobehavior events
    public override void Reset()
    {
        typeName = "Generator";
    }

    void Start()
    {
        StartCoroutine(GeneratorInit());
    }

    public override void Update()
    {
        // Cleanup list
        for (int i = spawnedObjs.Count-1; i >= 0; i--)
        {
            if (spawnedObjs[i] == null)
                spawnedObjs.RemoveAt(i);
        }

        // Manage spawn monitoring flags
        bool lifeLimited = (totalLimit > 0);
        bool simulLimited = (simultaneousLimit > 0);
        bool isLimited = (lifeLimited || simulLimited);
        bool bothLimits = (lifeLimited && simulLimited);

        exhausted = (lifeLimited && totalSpawned >= totalLimit);
        full = (simulLimited && spawnedObjs.Count >= simultaneousLimit);


        // Change name
        typeName = "Generator"
            + (isLimited ? ": " : "")
            + (simulLimited ? spawnedObjs.Count.ToString() + "/" + simultaneousLimit.ToString() + " spawned" : "")
            + (bothLimits ? ", " : "")
            + (lifeLimited ? totalSpawned.ToString() + "/" + totalLimit.ToString() + " total" : "");
        ChangeName();
    }
    #endregion

    #region generator events
    public void OnExhausted()
    {
    }
    public void OnLastWaveCleared()
    {
    }
    #endregion

    #region methods
    public override int PlacedCount()
    {
        return spawnedObjs.Count;
    }

    public void StartGenerator()
    {
        StartCoroutine(GeneratorInit());
    }

    public void EndGenerator()
    {
        StopAllCoroutines();
    }
    #endregion

    #region coroutines
    public IEnumerator GeneratorInit()
    {
        if (prewarm != PrewarmType.False)
        {
            yield return StartCoroutine(Spawn());

            if (prewarm == PrewarmType.Full)
            {
                while (!full && !exhausted)
                {
                    yield return StartCoroutine(Spawn());
                }
            }
        }

        StartCoroutine(SpawnLoop());
    }

    public IEnumerator Spawn ()
    {
        // Increment the total counter
        totalSpawned++;

        // Calculate spawn position
        Vector3 randomPos = Random.insideUnitSphere*spawnRadius;
        randomPos.y = 0f;


        // Spawn the effect
        if (spawnEffect != null)
        {
            GameObject.Instantiate(spawnEffect, transform.position + randomPos, Quaternion.identity);
            yield return new WaitForSeconds(spawnDelay);
        }

        // Add the new prefab intance
        spawnedObjs.Add(GameObject.Instantiate(RandomPrefab(), transform.position + randomPos, Quaternion.identity).transform);
        yield return new WaitForSeconds(spawnCooldown);

        // If spawning in waves
        //if ()
    }


    public IEnumerator SpawnLoop ()
    {
		while (active)
        {
            // generate a new one
            if (prefabs.Length > 0  && !exhausted  &&  !full  &&  !waveActive)
            {
                yield return StartCoroutine(Spawn());
            }
            yield return null;
        }
	}
    #endregion
}
