using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_Radish : Collectible
{
    public bool addSoulHearts = false;

    // Use this for initialization
    void Start ()
    {
    }

    // Update is called once per frame
    public override void OnCollectEnd()
    {
        base.OnCollectEnd();
        GameManager.player.health.ChangeHP(1, addSoulHearts);
    }
}
