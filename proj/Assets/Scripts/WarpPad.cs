using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPad : NPC
{
    public string scene;
    public int room;
    public Vector3 destination;

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

        // Manage dialogue
        if (dialog == null)
            dialog = gameObject.GetComponent<Dialog>();

        if (dialog != null)
        {
            string destName = "";

            if (sameScene)
            {
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
        GameManager.player.inputActive = false;
        LevelManager.warpDestination = destination;


        // Fade out
        if (!sameScene)
        {
            AudioManager.FadeOutMusic(0.5f, true);
        }
        yield return UIManager.instance.StartCoroutine(UIManager.instance.ScreenFadeChange(1f, 0.5f));


        // If warping to a room in the same scene, just move the player, refresh the rooms and fade back in
        if (sameScene)
        {
            GameManager.player.transform.position = destination;
            LevelManager.ChangeRoom(room);
            yield return UIManager.instance.StartCoroutine(UIManager.instance.ScreenFadeChange(0f, 0.5f));
        }
        else
        {
            LevelManager.isWarping = true;
            LevelManager.EnterLevel(scene);
        }
    }
}
