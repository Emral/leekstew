using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkingNPC : NPC
{
    private DialogSequence dialogSeq;

    public override void Update()
    {
        base.Update();
        if (dialogSeq == null && talkCooldown <= 0f)
            dialogSeq = gameObject.GetComponent<DialogSequence>();
    }

    public override IEnumerator OnPlayerInteract()
    {
        if (dialogSeq != null)
        {
            GameManager.cutsceneMode = true;
            yield return dialogSeq.StartCoroutine (dialogSeq.RunSequence());

            talkCooldown = 1f;
            GameManager.cutsceneMode = false;
        }

        while(talkCooldown > 0)
        {
            talkCooldown -= Time.deltaTime;
            yield return null;
        }
    }
}
