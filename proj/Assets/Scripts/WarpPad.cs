using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPad : NPC
{
    public string scene;

    public int room;
    public Transform destinationTransform;
    public Vector3 destinationOffset;
    public bool useDefaultPosition;

    public bool locked;
    public bool unlockDestinationWarpPad = false;

    public bool dontFadeBackIn = false;

    private Dialog dialog;
    private bool sameScene = false;


    public override void Update()
    {
        base.Update();

        // Detect whether the warp destination is in the same scene or not
        if (LevelManager.currentLevel != null)
        {
            sameScene = (LevelManager.currentLevel.key == scene || scene == "");
        }

        // Disable talking if locked
        if (locked)
        {
            talkCooldown = 2f;
            if (SaveManager.CurrentLevelSave.warpPadsActivated.Contains(instanceID))
                locked = false;
        }

        // Manage dialogue
        if (dialog == null)
            dialog = gameObject.GetComponent<Dialog>();

        if (dialog != null && dialog.text == "")
        {
            string destName = "";

            if (sameScene)
            {
                if (LevelManager.roomNames.Count > room)
                    destName = LevelManager.roomNames[room];
            }
            else
            {
                LevelData info = LevelManager.GetLevelInfo(scene);
                if (info != null)
                    destName = info.name;
            }

            dialog.text = destName;
        }
    }


    public override IEnumerator OnPlayerInteract()
    {
        // Deny if locked
        if (locked)
        {
            AudioManager.PlaySound("denied");
        }
        else
        {
            GameManager.player.inputActive = false;
            if (destinationTransform != null)
                LevelManager.warpDestination = destinationTransform.position + Vector3.up * 0.1f;

            LevelManager.warpDestination += destinationOffset;


            // Fade out
            if (!sameScene || LevelManager.roomMusic.ContainsKey(room) && LevelManager.roomMusic[room] != AudioManager.currentMusic)
            {
                AudioManager.FadeOutMusic(0.5f, true);
            }
            yield return UIManager.instance.StartCoroutine(UIManager.instance.ScreenFadeChange(1f, 0.5f));


            // If warping to a room in the same scene, just move the player, refresh the rooms and fade back in
            if (sameScene)
            {
                if (destinationTransform != null && unlockDestinationWarpPad)
                {
                    WarpPad destPadScr = destinationTransform.GetComponent<WarpPad>();
                    SaveManager.CurrentLevelSave.warpPadsActivated.Add(destPadScr.instanceID);
                }

                GameManager.player.transform.position = LevelManager.warpDestination;
                LevelManager.ChangeRoom(room);
                GameManager.player.inputActive = true;
                if (LevelManager.roomMusic.ContainsKey(room))
                    AudioManager.SetMusic(LevelManager.roomMusic[room]);

                if (!dontFadeBackIn)
                    yield return UIManager.instance.StartCoroutine(UIManager.instance.ScreenFadeChange(0f, 0.5f));
            }
            else
            {
                LevelManager.isWarping = !useDefaultPosition;
                LevelManager.dontFadeBackIn = dontFadeBackIn;
                LevelManager.EnterLevel(scene);
            }
        }
    }
}
