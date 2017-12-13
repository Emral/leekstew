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
    public bool useRelativeDirection = false;

    public PrewarmType prewarm = PrewarmType.Full;
    public float spawnRadius = 0f;

    public GameObject spawnEffect;


    // Projectile stuff
    public bool spawnProjectiles;
    [SerializeField] public ProjectileProperties[] projectileProperties;


    // Control vars
    private int totalSpawned = 0;
    private int currentSpawned = 0;

    private bool exhausted = false;
    private bool full = false;
    private bool waveActive = false;
    private int waveSpawnCount = 0;

    public int poolSize = 20;

    private GameObject[] projectilePool;
    private GameObject[] spawnEffectPool;
    private GameObject[] deathEffectPool;

    #region monobehavior events
    public override void Reset()
    {
        typeName = "Generator";
    }

    void Start()
    {
        if (simultaneousLimit >= 0 && simultaneousLimit < poolSize)
        {
            poolSize = simultaneousLimit;
        }
        projectilePool = new GameObject[poolSize];
        spawnEffectPool = new GameObject[poolSize];
        deathEffectPool = new GameObject[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            if (spawnProjectiles && projectileProperties.Length > 0 && prefabs.Length > 0)
            {
                projectilePool[i] = Instantiate(prefabs[0]);
                projectilePool[i].SetActive(false);
                projectilePool[i].transform.parent = transform;
                GameObject deathEffect = projectileProperties[Random.Range(0, projectileProperties.Length - 1)].deathEffect;
                if (deathEffect != null)
                {
                    deathEffectPool[i] = Instantiate(deathEffect);
                    deathEffectPool[i].SetActive(false);
                    deathEffectPool[i].transform.parent = transform;
                }
            }
            spawnEffectPool[i] = Instantiate(spawnEffect);
            spawnEffectPool[i].SetActive(false);
            spawnEffectPool[i].transform.parent = transform;
        }

        StartCoroutine(GeneratorInit());
    }

    public override void Update()
    {
        // Cleanup list
        currentSpawned = 0;
        for (int i = projectilePool.Length-1; i >= 0; i--)
        {
            if (projectilePool[i].activeSelf)
            {
                currentSpawned += 1;
            }
        }

        // Manage spawn monitoring flags
        bool lifeLimited = (totalLimit > 0);
        bool simulLimited = (simultaneousLimit > 0);
        bool isLimited = (lifeLimited || simulLimited);
        bool bothLimits = (lifeLimited && simulLimited);

        exhausted = (lifeLimited && totalSpawned >= totalLimit);
        full = (simulLimited && currentSpawned >= simultaneousLimit);


        // Change name
        typeName = "Generator"
            + (isLimited ? ": " : "")
            + (simulLimited ? currentSpawned.ToString() + "/" + simultaneousLimit.ToString() + " spawned" : "")
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
        return currentSpawned;
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

    private GameObject returnFirstInactive(GameObject[] pool, out int position)
    {
        GameObject result = null;
        position = 0;


        for (int i = 0; i < pool.Length; i++)
        {
            if (!pool[i].activeSelf)
            {
                result = pool[i];
                position = i;
                break;
            }
        }

        return result;
    }

    private GameObject returnFirstInactive(GameObject[] pool)
    {
        int a;
        return returnFirstInactive(pool, out a);
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
            GameObject spawnedInstance = returnFirstInactive(spawnEffectPool);
            if (spawnedInstance != null)
            {
                spawnedInstance.SetActive(true);
                spawnedInstance.transform.position = transform.position + randomPos;
                spawnedInstance.transform.rotation = Quaternion.identity;
            }
            yield return new WaitForSeconds(spawnDelay);
        }

        // Add the new prefab intance
        if (spawnProjectiles && projectileProperties.Length > 0)
        {
            int pos = 0;
            GameObject spawnedInstance = returnFirstInactive(projectilePool, out pos);
            if (spawnedInstance != null)
            {
                spawnedInstance.transform.position = transform.position + randomPos;
                spawnedInstance.transform.rotation = Quaternion.identity;
                Projectile projectileScr = spawnedInstance.GetComponent<Projectile>();
                if (projectileScr == null)
                {
                    projectileScr = spawnedInstance.AddComponent<Projectile>();
                }
                projectileScr.deathEffect = deathEffectPool[pos];
                projectileScr.properties = new ProjectileProperties(projectileProperties[Random.Range(0, projectileProperties.Length - 1)]);
                if (useRelativeDirection)
                {
                    Debug.Log(projectileScr.properties.speed);
                    projectileScr.properties.speed = transform.InverseTransformDirection(projectileScr.properties.speed);
                }
                spawnedInstance.SetActive(true);
                projectileScr.Start();
            }
        }
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
