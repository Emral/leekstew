using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_Radish : Collectible
{
    public bool addSoulHearts = false;



    public override IEnumerator OnCollectEnd()
    {
        base.OnCollectEnd();
        GameManager.player.health.ChangeHP(1, addSoulHearts);
        UIManager.hpFadeCounter = 0f;
        yield return null;
    }
}
