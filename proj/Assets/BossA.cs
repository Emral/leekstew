using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BossA : Battery
{
    public static bool cutsceneWatched = false;
    public float jumpTimer = 0f;
    public int jumpsUntilThrash = 0;

    private bool bossStarted = false;

    private float rotSpeed = 0f;


    public override void PowerOn()
    {
        if (!bossStarted)
        {
            bossStarted = true;
            StartCoroutine(Battle());
        }
    }

    private IEnumerator Spin(float changeAmt, float changeTime)
    {
        Vector3 oldRot = transform.rotation.eulerAngles;
        Vector3 newRot = oldRot + Vector3.up * changeAmt;

        float elapsedTime = 0;
        while (elapsedTime < changeTime)
        {
            transform.rotation = Quaternion.Euler(new Vector3(oldRot.x, Mathf.SmoothStep(oldRot.y, oldRot.y+changeAmt, elapsedTime/changeTime), oldRot.z));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.Euler(new Vector3(oldRot.x, oldRot.y + changeAmt, oldRot.z));
    }

    private IEnumerator Battle()
    {
        float elapsedTime;

        // Cutscene
        GameManager.cutsceneMode = true;

        UIManager ui = UIManager.instance;
        if (!cutsceneWatched)
        {
            ui.StartCoroutine(ui.ScreenFadeChange(0f, 3f));
            yield return new WaitForSeconds(1f);

            CameraBehavior initialShot = CameraManager.CaptureCurrentShot();
            initialShot.target = transform;
            initialShot.yaw = 180 + transform.rotation.eulerAngles.y;
            initialShot.pitch = 0f;
            initialShot.zoom = -2f;
            CameraManager.DoShiftToNewShot(initialShot, 0.01f);
            yield return new WaitForSeconds(0.02f);

            CameraBehavior zoomedShot = CameraManager.CaptureCurrentShot();
            zoomedShot.zoom = -8.5f;
            CameraManager.DoShiftToNewShot(initialShot, 3f);
            yield return new WaitForSeconds(2f);

            elapsedTime = 0;
            while (elapsedTime < 1)
            {
                AudioManager.musicPitch = Mathf.Lerp(1f, 0f, elapsedTime / 1);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            AudioManager.StopMusic();
            yield return new WaitForSeconds(2f);

            // Dialogue 1
            CameraManager.DoGradualReset(1f);
            DialogSequence dialogSeq = gameObject.GetComponent<DialogSequence>();
            yield return dialogSeq.StartCoroutine(dialogSeq.RunSequence());

            // Music
            yield return new WaitForSeconds(0.25f);
            AudioManager.musicPitch = 1f;
            AudioManager.SetMusic(AudioManager.instance.songs[AudioManager.instance.songs.Count - 2].key);
            yield return new WaitForSeconds(0.25f);

            // Final line
            string[] newLines = { "Rocky really likes double fake-outs." };
            dialogSeq.lines = newLines;
            yield return dialogSeq.StartCoroutine(dialogSeq.RunSequence());

            // Toggle the cutscene flag
            cutsceneWatched = true;
        }
        else
        {
            ui.StartCoroutine(ui.ScreenFadeChange(0f, 1f));
            yield return new WaitForSeconds(0.5f);
        }

        // Begin fight spin anim
        yield return StartCoroutine(Spin(-22f, 0.5f));
        yield return StartCoroutine(Spin(44f+720f, 1f));
        yield return StartCoroutine(Spin(-22f, 0.25f));
        yield return new WaitForSeconds(0.5f);

        GameManager.cutsceneMode = false;

        HealthPoints health = GetComponent<HealthPoints>();
        CameraBehavior bossShot = new CameraBehavior();

        // Phase 1
        while (health.currentHp > 0)
        {

        }
    }
}
