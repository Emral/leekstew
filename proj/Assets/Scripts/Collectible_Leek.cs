using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_Leek : Collectible
{

    // Use this for initialization
    public override bool RespawnConditions()
    {
        return !SaveManager.CurrentLevelSave.leeksCollected.Contains(instanceID);
    }

    // Update is called once per frame
    public override IEnumerator OnCollectEnd()
    {
        //base.OnCollectEnd();
        UIManager.pickupFadeCounter = 0f;

        if (collectEndSound != null)
            GameManager.player.PlaySound(collectEndSound, 1f);
        if (collectStartEffect != null)
        {
            GameObject.Instantiate(collectEndEffect, transform.position, Quaternion.identity);
        }

        SaveManager.CurrentLevelSave.leeksCollected.Add(instanceID);
        SaveManager.Autosave();

        yield return null;
	}
}
