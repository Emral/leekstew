using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_Tooth : Collectible
{
    // Use this for initialization
    void Start ()
    {
    }

    // Update is called once per frame
    public override void OnCollectEnd()
    {
        UIManager.pickupFadeCounter = 0f;
        base.OnCollectEnd();
        GameManager.teethCollected++;
    }
}
