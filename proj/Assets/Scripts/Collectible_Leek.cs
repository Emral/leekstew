using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_Leek : Collectible
{
    public Transform glowObj;
    public Transform lightObj;

    public override bool RespawnConditions()
    {
        return !SaveManager.CurrentLevelSave.leeksCollected.Contains(instanceID);
    }



    public override IEnumerator OnCollectEnd()
    {
        //base.OnCollectEnd();
        GameObject.Instantiate(collectEndEffect, transform.position, Quaternion.identity);

        SaveManager.CurrentLevelSave.leeksCollected.Add(instanceID);
        SaveManager.Autosave();

        GameObject child = transform.GetChild(0).gameObject;
        Player player = GameManager.player;

        // Start sequence
        GameManager.cutsceneMode = true;
        glowObj.gameObject.SetActive(false);
        lightObj.gameObject.SetActive(false);
        child.SetActive(false);
        AudioManager.PauseMusic();
        while (!player.GetGrounded())
        {
            yield return null;
        }

        Vector3 cameraGroundPos = Camera.main.transform.position;
        cameraGroundPos.y = player.transform.position.y;

        player.transform.rotation = Quaternion.LookRotation(cameraGroundPos - player.transform.position, Vector3.up);

        CameraBehavior zoomedShot = CameraManager.CaptureCurrentShot();
        zoomedShot.debugName = "ZOOM";
        zoomedShot.target = null;
        zoomedShot.position = player.transform.position;
        zoomedShot.pitch = 5f;
        zoomedShot.panY = 2f;
        zoomedShot.zoom = -4.5f;

        CameraManager.DoShiftToNewShot(zoomedShot, 1f);
        yield return new WaitForSeconds(0.5f);

        if (collectEndSound != null)
            AudioManager.PlaySound(collectEndSound);

        player.PerformGenericJump(JumpType.Jump, 1f);

        yield return player.StartCoroutine(player.Spin(-22f, 0.5f));
        yield return player.StartCoroutine(player.Spin(44f + 720f, 1f));
        yield return player.StartCoroutine(player.Spin(-22f, 0.25f));
        //yield return new WaitForSeconds(1f);

        transform.position = player.transform.position + Vector3.up * 2f;
        child.SetActive(true);
        UIManager.pickupFadeCounter = 0f;
        UIManager.instance.StartCoroutine(UIManager.instance.ShowBossSubtitle(UIManager.instance.leekGetSprite, 0f, 2.5f));
        yield return new WaitForSeconds(2f);

        UIManager.DoScreenFadeChange(1f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        CameraManager.DoGradualReset(0.01f);
        child.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        Camera.main.transform.localPosition = new Vector3(Camera.main.transform.localPosition.x, 0f, Camera.main.transform.localPosition.z);
        UIManager.DoScreenFadeChange(0f, 0.5f);
        yield return new WaitForSeconds(0.25f);


        AudioManager.ResumeMusic();
        GameManager.cutsceneMode = false;

        yield return null;
	}
}
