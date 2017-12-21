using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_Leek : Collectible
{

    public override bool RespawnConditions()
    {
        return !SaveManager.CurrentLevelSave.leeksCollected.Contains(instanceID);
    }



    public override IEnumerator OnCollectEnd()
    {
        //base.OnCollectEnd();
        UIManager.pickupFadeCounter = 0f;

        if (collectEndSound != null)
            CameraManager.PlaySound(collectEndSound);
        if (collectStartEffect != null)
        {
            GameObject.Instantiate(collectEndEffect, transform.position, Quaternion.identity);
        }

        SaveManager.CurrentLevelSave.leeksCollected.Add(instanceID);
        SaveManager.Autosave();

        yield return null;
	}
}
