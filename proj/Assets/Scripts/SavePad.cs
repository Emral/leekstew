using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePad : NPC
{
    public override IEnumerator OnPlayerInteract()
    {
        SaveManager.DoSaveGame();
        talkCooldown = 1f;
        yield return new WaitForSeconds(1f);
        talkCooldown = 0f;
        yield return null;
    }
}
