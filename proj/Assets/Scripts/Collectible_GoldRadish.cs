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
        SaveManager.CurrentLevelSave.goldRadishCollected = true;
        GameManager.player.health.hp = 3 + SaveManager.currentSave.TotalGoldRadishes;
        SaveManager.Autosave();
        base.OnCollectEnd();
        yield return null;
    }
}
