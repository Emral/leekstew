using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_Tooth : Collectible
{
    public int teethValue = 1;

    // Use this for initialization
    void Start ()
    {
    }

    // Update is called once per frame
    public override IEnumerator OnCollectEnd()
    {
        UIManager.pickupFadeCounter = 0f;
        base.OnCollectEnd();
        SaveManager.currentSave.teethCollected += teethValue;
        yield return null;
    }
}
