﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public int instanceID = 0;

    private float distToPlayer = 999f;
    public float talkDist = 2.5f;
    public float talkIconHeight = 2f;
    [HideInInspector] public float talkCooldown = 0f;

    public virtual void Update()
    {
        distToPlayer = Vector3.Distance(transform.position, GameManager.player.transform.position);

        if (distToPlayer < talkDist && GameManager.inputPress["Run"] && !GameManager.cutsceneMode && talkCooldown <= 0f)
        {
            StartCoroutine(OnPlayerInteract());
        }
    }

    public virtual void OnGUI()
    {
        if (distToPlayer < talkDist && GameManager.player.InputHasEffect && talkCooldown <= 0f)
        {
            Vector3 worldPos = transform.position + Vector3.up*talkIconHeight;
            Vector3 pos = Camera.main.WorldToScreenPoint(worldPos);

            GUI.DrawTexture(new Rect(pos.x - Screen.width * 0.025f, Screen.height - pos.y - Screen.width * 0.025f, Screen.width*0.05f, Screen.width * 0.05f), UIManager.TalkTextureAnimated);
        }
    }

    public virtual IEnumerator OnPlayerInteract()
    {
        yield return null;
    }
}
