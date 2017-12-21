using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BossA : CollidingEntity
{
    public bool skipCutscene;

    public static bool cutsceneWatched = false;
    public float jumpTimer = 0f;
    public int jumpsUntilThrash = 0;
   
    private bool justHitWall = false;

    public GameObject powerupEffect;
    public GameObject shatterEffect;

    public Transform bossArenaTransform;
    public Transform arenaCeilingTransform;

    public GameObject iciclePrefab;
    public GameObject slowShockwavePrefab;
    public GameObject fastShockwavePrefab;

    public Texture2D newFurbaColorsTex;
    public Texture2D newIceColorsTex;

    private bool poweringUp = false;
    private bool bossStarted = false;
    private bool bossEnded = false;
    private float rotSpeed = 0f;
    public float slideSpeed = 0.085f;

    private MultichannelAudio multiAudio;

    private AudioSource spinSoundSource;

    private bool droppingIcicles;


    private Transform modelTrans;
    private Battery batt;
    private SquashAndStretch squash;
    private Shake shake;

    public override void Start()
    {
        base.Start();
        multiAudio = GetComponent<MultichannelAudio>();
        batt = GetComponent<Battery>();
        modelTrans = transform.Find("model");
        squash = modelTrans.GetComponent<SquashAndStretch>();
        shake = modelTrans.GetComponent<Shake>();
    }

    public override void Update()
    {
        base.Update();

        if (!bossStarted && batt.GetActive())
        {
            bossStarted = true;
            StartCoroutine(Battle());
        }

        if (!bossEnded && health.currentHp <= 0)
        {
            bossEnded = true;
            StopAllCoroutines();
            StartCoroutine(PostBattle());
        }

        if (spinSoundSource != null)
        {
            spinSoundSource.pitch = Time.timeScale;
        }

        modelTrans.Rotate(0f,rotSpeed*Time.deltaTime*60f,0f);

        /*
        if (Vector3.Distance(transform.position, bossArenaTransform.position) > 10)
        {
            transform.position = new Vector3(bossArenaTransform.position.x, transform.position.y, bossArenaTransform.position.z);
            velocity = modelTrans.forward * -1 * slideSpeed;
        }
        */
    }


    public void SpawnIcicle(Vector3 position)
    {
        if (position == Vector3.zero)
            position = arenaCeilingTransform.position + (Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward * Random.Range(0f, 4f));
        position.y = arenaCeilingTransform.position.y;

        GameObject.Instantiate(iciclePrefab, position, Quaternion.identity);
    }
    public void SpawnIcicle()
    {
        SpawnIcicle(Vector3.zero);
    }

    public IEnumerator SequentialIcicles()
    {
        for (int i = 1 + Mathf.FloorToInt((health.hp - health.currentHp)%4 * 0.5f); i >= 0; i--)
        {
            yield return new WaitForSeconds(Random.Range(0f, 0.5f));
            droppingIcicles = false;
            SpawnIcicle();
        }
    }

    public void SpawnIcicles()
    {
        if (!droppingIcicles)
        {
            droppingIcicles = true;
            SpawnIcicle(GameManager.player.transform.position);
            StartCoroutine(SequentialIcicles());
        }
    }


    public override void GiveBounce(CollidingEntity otherScr)
    {
        if (otherScr == GameManager.player && !GameManager.cutsceneMode)
        {
            GameManager.player.velocity = Quaternion.AngleAxis(Random.Range(-90f, 90f), Vector3.up) * (bossArenaTransform.position - GameManager.player.transform.position);
            GameManager.player.PerformGenericJump(JumpType.Jump);

            if (health.vulnerable)
            {
                squash.effectAmount = 0.5f;
                shake.effectAmount = 1f;
                multiAudio.Play("hurt", false, 0.25f);
            }
            health.TakeHit();
        }
    }


    public override void ReceiveBlock(CollideDir side, CollidingEntity otherScr, Transform otherTrans, Vector3 point, Vector3 normal)
    {
        base.ReceiveBlock(side, otherScr, otherTrans, point, normal);
        switch (side)
        {
            case (CollideDir.Up):
                break;
            case (CollideDir.Down):
                break;
            default:
                if (otherTrans.gameObject.layer == 9 || otherTrans.gameObject.layer == 14)
                {
                    multiAudio.Play("hit wall");
                    GameManager.ScreenShake(2f);

                    float tempYVel = velocity.y;

                    if (Random.Range(0, 100) < 50)
                        velocity = Quaternion.LookRotation(GameManager.player.transform.position - transform.position, Vector3.up) * Vector3.Reflect(velocity, normal);
                    else
                        velocity = Quaternion.AngleAxis(Random.Range(-5f,5f), Vector3.up) * Vector3.Reflect(velocity, normal);

                    SpawnIcicles();

                    velocity.y = 0f;
                    velocity = velocity.normalized * slideSpeed * Random.Range(0.5f, 1f);
                    velocity.y = tempYVel;

                    justHitWall = true;
                }
                break;
        }
    }


    private IEnumerator ShockwaveJump(bool maintainVelocity = true, float forwardSpeed = 0f)
    {
        // Backup velocity
        Vector3 tempVel = Vector3.zero;
        if (!maintainVelocity)
        {
            tempVel = velocity;
            velocity = Vector3.zero;
        }

        // Squash
        float elapsedTime = 0f;
        while (elapsedTime < 0.5f)
        {
            squash.effectAmount = Mathf.SmoothStep(0f, 1f, elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Jump
        squash.effectAmount = -0.5f;
        if (!maintainVelocity)
        {
            velocity = tempVel.normalized * forwardSpeed;
        }

        elapsedTime = 0f;
        float totalTime = 0.25f;
        float groundY = transform.position.y;
        while (elapsedTime < totalTime)
        {
            float timePercent = elapsedTime/totalTime;
            float jumpMult = Mathf.Sin(timePercent * Mathf.Deg2Rad * 180f);

            transform.position = new Vector3(transform.position.x, groundY + jumpMult * 2f, transform.position.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);

        // Land
        shake.effectAmount = 0.5f;
        squash.effectAmount = 0.25f;
        GameManager.ScreenShake(0.5f);
        multiAudio.Play("hit floor");

        GameObject prefabToSpawn = slowShockwavePrefab;
        if (health.currentHp <= 2 && Random.value < 0.5f)
            prefabToSpawn = fastShockwavePrefab;

        GameObject.Instantiate(prefabToSpawn, new Vector3(transform.position.x, bossArenaTransform.position.y+0.1f, transform.position.z), Quaternion.identity);
    }


    private IEnumerator ReduceFOV()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 1)
        {
            Camera.main.fieldOfView = Mathf.SmoothStep(60f, 40f, elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Camera.main.fieldOfView = 40f;
    }

    private IEnumerator MusicWindDown()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 2)
        {
            AudioManager.musicPitch = Mathf.Lerp(1f, 0f, elapsedTime / 2);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        AudioManager.StopMusic();
    }

    private IEnumerator Intensifies()
    {
        poweringUp = true;
        float elapsedTime = 0f;
        while (poweringUp)
        {
            float squashCap = Mathf.Lerp(0f, 0.125f, Mathf.Max(elapsedTime) / 3);
            squash.effectAmount = Random.Range(-squashCap, squashCap);
            shake.effectAmount = Mathf.Lerp(0f, 0.125f, Mathf.Max(elapsedTime) / 3);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        GameObject.Instantiate(powerupEffect, transform.position, Quaternion.identity);
        foreach (Transform trans in transform)
        {
            MeshRenderer rend = trans.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                if (rend.gameObject.tag == "FurbaMesh")
                {
                    rend.material.mainTexture = newFurbaColorsTex;
                    /*
                    foreach (Material mat in rend.materials)
                    {
                        mat.mainTexture = newFurbaColorsTex;
                    }
                    */
                }
                if (rend.gameObject.tag == "IcecubeMesh")
                {
                    rend.material.mainTexture = newIceColorsTex;
                    /*
                    foreach (Material mat in rend.materials)
                    {
                        mat.mainTexture = newIceColorsTex;
                    }
                    */
                }
            }
        }
    }

    private IEnumerator Spin(float changeAmt, float changeTime, bool absolute = false)
    {
        Vector3 oldRot = modelTrans.rotation.eulerAngles;
        float newVal;

        float elapsedTime = 0;
        while (elapsedTime < changeTime)
        {
            newVal = oldRot.y + changeAmt;
            if (absolute)
                newVal = changeAmt;

            modelTrans.rotation = Quaternion.Euler(new Vector3(oldRot.x, Mathf.SmoothStep(oldRot.y, newVal, elapsedTime/changeTime), oldRot.z));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        newVal = oldRot.y + changeAmt;
        if (absolute)
            newVal = changeAmt;

        modelTrans.rotation = Quaternion.Euler(new Vector3(oldRot.x, newVal, oldRot.z));
    }

    private IEnumerator Battle()
    {
        //float elapsedTime;

        // Cutscene mode
        GameManager.cutsceneMode = true;
        UIManager ui = UIManager.instance;
        LevelManager.dontFadeBackIn = true;

        // Set up the camera shots
        CameraBehavior initialShot = CameraManager.CaptureCurrentShot();
        initialShot.debugName = "INITIAL SHOT";
        initialShot.target = transform;
        initialShot.yaw = transform.rotation.eulerAngles.y;
        initialShot.pitch = 0f;
        initialShot.panY = 0f;
        initialShot.zoom = -0.89f;

        CameraBehavior zoomedShot = new CameraBehavior(initialShot);
        zoomedShot.debugName = "ZOOMED SHOT";
        zoomedShot.pitch = 20f;
        zoomedShot.zoom = -1.75f;

        CameraBehavior zoomedShotB = new CameraBehavior(zoomedShot);
        FlagsHelper.Set(ref zoomedShotB.changedProperties, CameraProperties.Position);
        zoomedShotB.debugName = "FIGHT SHOT";
        zoomedShotB.target = null;
        zoomedShotB.position = Vector3.Lerp(transform.position, GameManager.player.transform.position, 0.35f);
        zoomedShotB.pitch = 40f;
        zoomedShotB.zoom = -20f;

       
        // Wait for level to begin
        while (LevelManager.beginningLevel)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);

        // Quickly switch to the initial shot
        CameraManager.ApplyBehavior(initialShot);

        // Play the cutscene
        if (!cutsceneWatched && !skipCutscene)
        {
            ui.StartCoroutine(ui.ScreenFadeChange(0f, 7f));
            Camera.main.fieldOfView = 60f;

            CameraManager.DoShiftToNewShot(zoomedShot, 14f, 2);
            yield return new WaitForSeconds(10f);

            //elapsedTime = 0;
            StartCoroutine(MusicWindDown());
            yield return new WaitForSeconds(5f);

            CameraManager.DoShiftToNewShot(zoomedShotB, 1f, 3);
            StartCoroutine(ReduceFOV());
            yield return new WaitForSeconds(2f);


            // Dialogue 1
            //CameraManager.DoGradualReset(1f);
            DialogSequence dialogSeq = gameObject.GetComponent<DialogSequence>();
            yield return dialogSeq.StartCoroutine(dialogSeq.RunSequence());

            // Music
            yield return new WaitForSeconds(0.5f);
            AudioManager.musicPitch = 1f;
            AudioManager.SetMusic(AudioManager.instance.songs.Count - 2);
            yield return new WaitForSeconds(0.75f);

            // Final line
            string[] newLines = { "Rocky really likes fake-outs." };
            dialogSeq.lines = newLines;
            yield return dialogSeq.StartCoroutine(dialogSeq.RunSequence());

            // Toggle the cutscene flag
            cutsceneWatched = true;
        }
        else
        {
            ui.StartCoroutine(ui.ScreenFadeChange(0f, 1f));
            AudioManager.SetMusic(AudioManager.instance.songs.Count - 2);

            CameraManager.DoShiftToNewShot(zoomedShotB, 1f);
            StartCoroutine(ReduceFOV());
            yield return new WaitForSeconds(1f);
        }

        // Begin fight spin anim
        CameraBehavior fightShot = new CameraBehavior();
        fightShot.target = bossArenaTransform;
        fightShot.yaw = 45;

        yield return StartCoroutine(Spin(-22f, 0.5f));

        spinSoundSource = multiAudio.Play("spin", true, 0.5f);
        yield return StartCoroutine(Spin(44f+720f, 1f));
        spinSoundSource.Stop();

        yield return StartCoroutine(Spin(-22f, 0.25f));
        yield return new WaitForSeconds(0.5f);

        GameManager.cutsceneMode = false;

        HealthPoints health = GetComponent<HealthPoints>();

        // Phase 1
        spinSoundSource = multiAudio.Play("spin", true, 0.5f);
        rotSpeed = 15f;
        velocity = (transform.forward + transform.right*0.3f) * -1 * slideSpeed;

        while (health.currentHp > 4)
        {
            yield return null;
        }

        // Phase 2
        // Slow down and zoom in
        zoomedShot.zoom = -2.5f;

        GameManager.cutsceneMode = true;
        droppingIcicles = true;

        AudioManager.FadeOutMusic(2f, true);
        Time.timeScale = 0.25f;
        yield return new WaitForSeconds(0.25f);

        // Restore player HP
        GameManager.player.health.currentHp = GameManager.player.health.hp;

        // Stop spinning
        velocity = Vector3.zero;
        rotSpeed = 0f;
        spinSoundSource.Stop();
        yield return StartCoroutine(Spin(22f, 1f, true));
        //yield return new WaitForSecondsRealtime(1f);

        // Start shaking
        AudioSource quakeSource = AudioManager.PlaySound("quake");
        StartCoroutine(Intensifies());
        yield return StartCoroutine(Spin(0f, 0.5f, true));
        yield return new WaitForSeconds(0.75f);

        // Zoom back out
        quakeSource.Stop();
        poweringUp = false;
        Time.timeScale = 1f;

        AudioManager.SetMusic(AudioManager.instance.songs.Count - 1);
        yield return new WaitForSeconds(2f);

        GameManager.cutsceneMode = false;
        droppingIcicles = false;

        while (true)
        {
            // Shockwave jump
            yield return StartCoroutine(ShockwaveJump());
            yield return new WaitForSeconds(0.5f);

            // Start spinning
            spinSoundSource = multiAudio.Play("spin", true, 0.5f);
            rotSpeed = 15f;
            velocity = (transform.forward + transform.right * 0.3f) * -1 * slideSpeed;
            
            // Hit walls X number of times
            for (int i = Mathf.FloorToInt(Random.Range(2f,4f));  i > 0;  i--)
            {
                while (!justHitWall)
                {
                    yield return null;
                }
                yield return new WaitForSeconds(1f);
            }

            // Do a moving shockwave jump
            yield return StartCoroutine(ShockwaveJump());

            // Wait for more wall hits
            for (int i = Mathf.FloorToInt(Random.Range(2f, 4f)); i > 0; i--)
            {
                while (!justHitWall)
                {
                    yield return null;
                }
                yield return new WaitForSeconds(1f);
            }

            // Do multistomp
            rotSpeed = 0f;
            velocity = Vector3.zero;
            spinSoundSource.Stop();

            for (int i = Mathf.FloorToInt(Random.Range(1f, 3f)); i > 0; i--)
            {
                yield return StartCoroutine(ShockwaveJump());
                yield return new WaitForSeconds(0.25f);
            }

            yield return null;
        }
    }


    IEnumerator PostBattle()
    {
        CameraBehavior zoomedShot = CameraManager.CaptureCurrentShot();
        zoomedShot.target = transform;
        zoomedShot.yaw = transform.rotation.eulerAngles.y;
        zoomedShot.pitch = 10f;
        zoomedShot.panY = 0f;
        zoomedShot.zoom = -4.5f;
        spinSoundSource.Stop();


        // Stop moving
        GameManager.cutsceneMode = true;
        AudioManager.StopMusic();

        rotSpeed = 0f;
        velocity = Vector3.zero;

        CameraManager.DoShiftToNewShot(zoomedShot, 4f);
        yield return new WaitForSeconds(4f);

        AudioSource crackingSource = multiAudio.Play("cracking");
        shake.effectAmount = 0.25f;
        yield return new WaitForSeconds(2f);

        shake.effectAmount = 0.25f;
        yield return new WaitForSeconds(1.5f);
        shake.effectAmount = 0.25f;
        yield return new WaitForSeconds(1f);
        shake.effectAmount = 0.25f;
        yield return new WaitForSeconds(0.75f);
        shake.effectAmount = 0.25f;
        yield return new WaitForSeconds(0.5f);
        shake.effectAmount = 0.25f;
        yield return new WaitForSeconds(0.25f);

        while (crackingSource.isPlaying)
        {
            shake.effectAmount = 0.25f;
            yield return new WaitForSeconds(0.125f);
        }

        yield return new WaitForSeconds(1f);

        multiAudio.Play("shatter");
        GameObject.Instantiate(shatterEffect, transform.position, Quaternion.identity);
        modelTrans.gameObject.SetActive(false);
        yield return new WaitForSeconds(4f);

        GameManager.YouDidIt();
        yield return new WaitForSeconds(2f);

        UIManager.DoScreenFadeChange(1f, 4f);
        AudioManager.SetMusic(AudioManager.instance.songs.Count - 3);
        yield return UIManager.instance.StartCoroutine(UIManager.instance.Credits(false));

        AudioManager.FadeOutMusic(2f, true);
        yield return new WaitForSeconds(4f);

        GameManager.cutsceneMode = false;
        LevelManager.EnterLevel(SaveManager.currentSave.currentHubScene);
    }
}
