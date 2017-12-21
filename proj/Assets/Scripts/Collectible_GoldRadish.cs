using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_GoldRadish : Collectible_Radish
{
    public override bool RespawnConditions()
    {
        return !SaveManager.CurrentLevelSave.goldRadishCollected;
    }


    public override IEnumerator OnCollectEnd()
    {
        GameManager.player.health.hp++;
        SaveManager.CurrentLevelSave.goldRadishCollected = true;
        SaveManager.Autosave();
        base.OnCollectEnd();
        yield return null;
    }
}
