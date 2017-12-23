using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_GreenRadish : Collectible_Radish
{
    public override IEnumerator OnCollectEnd()
    {
        UIManager.hpFadeCounter = 0f;

        if (GameManager.player.health.currentHp < GameManager.player.health.hp)
            GameManager.player.health.currentHp = GameManager.player.health.hp;

        GameManager.player.health.ChangeHP(1, addSoulHearts);

        base.OnCollectEnd();
        yield return null;
    }
}
