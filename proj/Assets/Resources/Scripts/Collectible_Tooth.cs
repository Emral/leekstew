using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_Tooth : Collectible
{
    // Use this for initialization
    void Start ()
    {
        //collectSound = Resources.Load("");

    }

    // Update is called once per frame
    public override void OnCollectEnd()
    {
        base.OnCollectEnd();
        GameManager.teethCollected++;
    }
}
