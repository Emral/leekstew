using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_Tooth : Collectible
{
    public int teethValue = 1;



    public override IEnumerator OnCollectEnd()
    {
        UIManager.pickupFadeCounter = 0f;
        StartCoroutine(base.OnCollectEnd());
        SaveManager.currentSave.teethCollected += teethValue;
        yield return null;
    }
}
