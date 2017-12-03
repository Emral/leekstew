using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_Leek : Collectible
{

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	public override void OnCollectEnd()
    {
        //base.OnCollectEnd();

        UIManager.pickupFadeCounter = 0f;

        if (collectEndSound != null)
            GameManager.player.PlaySound(collectEndSound, 1f);
        if (collectStartEffect != null)
        {
            GameObject effect = GameObject.Instantiate(collectEndEffect, transform.position, Quaternion.identity);
        }

        GameManager.leeksCollected++;
	}
}
