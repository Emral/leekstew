using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_GoldRadish : Collectible_Radish
{
    // Use this for initialization
    void Start ()
    {
    }

    // Update is called once per frame
    public override IEnumerator OnCollectEnd()
    {
        GameManager.instance.player.health.hp++;
        base.OnCollectEnd();
        yield return null;
    }
}
