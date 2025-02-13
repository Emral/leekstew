﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int instanceID;
    public float collectDistance = 0.5f;
    public bool respawn;
    public bool gravitate = true;

    public AudioClip collectStartSound;
    public GameObject collectStartEffect;
    public AudioClip collectEndSound;
    public GameObject collectEndEffect;

    private static Transform playerTrans;
    private float distanceToPlayer;
    //private float hDistanceToPlayer;
    private float collectRange;
    private bool collecting = false;



    public void Start()
    {
        StartCoroutine(DelayedRespawnCheck());
    }
    public IEnumerator DelayedRespawnCheck()
    {
        yield return new WaitForSeconds(0.5f);
        if (!respawn && !RespawnConditions())
            GameObject.Destroy(gameObject);
    }



    public virtual bool RespawnConditions()
    {
        if (GameManager.itemsCollected.ContainsKey(instanceID))
        {
            return (GameManager.itemsCollected[instanceID] == false);
        }
        return false;
    }

    public virtual IEnumerator OnCollectStart()
    {
        if (collectStartSound != null)
            AudioManager.PlaySound(collectStartSound, false, 1f, Random.Range(0.75f, 1.25f));
        if (collectStartEffect != null)
        {

            GameObject.Instantiate(collectStartEffect, transform.position, Quaternion.identity);
            //effect.transform.localScale = new Vector3(3f, 3f, 3f);
        }

        yield return null;
    }
    public virtual IEnumerator OnCollectEnd()
    {
        if (collectEndSound != null)
            AudioManager.PlaySound(collectEndSound, false, 1f, Random.Range(0.75f, 1.25f));
        if (collectEndEffect != null)
        {
            GameObject.Instantiate(collectEndEffect, transform.position, Quaternion.identity);
            //effect.transform.localScale = new Vector3(3f, 3f, 3f);
        }
        yield return null;
    }

    public IEnumerator PullToPlayer()
    {
        yield return StartCoroutine(OnCollectStart());

        // If a gravitating collectible, pull to the player
        if (gravitate)
        {
            Vector3 startPos = transform.position;
            float maxTime = 0.2f;
            float remainingTime = maxTime;
            while (remainingTime > 0)
            {
                float percentMult = remainingTime / maxTime;
                Vector3 newPos = Vector3.Lerp(playerTrans.position, startPos, percentMult);
                newPos.y += 2f*Mathf.Cos(Mathf.PI*(0.5f-percentMult));

                transform.position = newPos;
                remainingTime -= Time.deltaTime;
                yield return null;
            }
        }

        // If a non-respawning collectible, store collection state based on UID
        if (!respawn)
        {
            GameManager.itemsCollected[instanceID] = true;
            GameManager.collectedTypes[instanceID] = this.GetType();
        }

        // Collect

        yield return StartCoroutine(OnCollectEnd());
        GameObject.Destroy(gameObject);
    }

    void Update()
    {
        Player playerScr = GameManager.player;
        if (playerScr != null)
        {
            playerTrans = playerScr.transform;

            //hDistanceToPlayer = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(playerTrans.position.x, 0, playerTrans.position.z));
            distanceToPlayer = Vector3.Distance(transform.position, playerTrans.position);

            collectRange = gravitate ? collectDistance + 1.5f : collectDistance;

            if (distanceToPlayer < collectRange && !collecting)
            {
                collecting = true;
                StartCoroutine(PullToPlayer());
            }
        }
    }
}
