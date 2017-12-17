using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BossA : CollidingEntity
{
    public bool skipCutscene;

    public static bool cutsceneWatched = false;
    public float jumpTimer = 0f;
    public int jumpsUntilThrash = 0;

    public Transform bossArenaTransform;
    public Transform arenaCeilingTransform;

    public GameObject iciclePrefab;
    public GameObject slowShockwavePrefab;
    public GameObject fastShockwavePrefab;

    private bool bossStarted = false;
    private float rotSpeed = 0f;
    public float slideSpeed = 0.085f;

    private MultichannelAudio audio;

    private AudioSource spinSoundSource;


    private Transform modelTrans;
    private Battery batt;
    private SquashAndStretch squash;
    private Shake shake;

    public override void Start()
    {
        base.Start();
        audio = GetComponent<MultichannelAudio>();
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

        modelTrans.Rotate(0f,rotSpeed,0f);

        if (Vector3.Distance(transform.position, bossArenaTransform.position) > 10)
        {
            transform.position = new Vector3(bossArenaTransform.position.x, transform.position.y, bossArenaTransform.position.z);
            velocity = modelTrans.forward * -1 * slideSpeed;
        }
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
        for (int i = 1 + Mathf.FloorToInt((health.hp - health.currentHp) * 0.5f); i >= 0; i--)
        {
            yield return new WaitForSeconds(Random.Range(0f, 0.5f));
            SpawnIcicle();
        }
    }

    public void SpawnIcicles()
    {
        SpawnIcicle(GameManager.player.transform.position);
        StartCoroutine(SequentialIcicles());
    }


    public override void ReceiveHarm(CollideDir side, CollidingEntity otherScr, Transform otherTrans, Vector3 point, Vector3 normal)
    {
        base.ReceiveHarm(side, otherScr, otherTrans, point, normal);

        if (otherScr == GameManager.player && collisionSide == CollideDir.Down)
        {
            squash.effectAmount = 2f;
            shake.effectAmount = 5f;
            audio.Play("hurt");

            health.ChangeHP(-1);
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
                    audio.Play("hit wall");
                    GameManager.ScreenShake(2f);
                    if (Random.Range(0, 100) < 50)
                        velocity = Quaternion.LookRotation(GameManager.player.transform.position - transform.position, Vector3.up) * Vector3.Reflect(velocity, normal);
                    else
                        velocity = Quaternion.AngleAxis(Random.Range(-5f,5f), Vector3.up) * Vector3.Reflect(velocity, normal);

                    SpawnIcicles();

                    velocity.y = 0f;
                    velocity = velocity.normalized * slideSpeed * Random.Range(0.5f, 1f);
                }
                break;
        }
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

    private IEnumerator Spin(float changeAmt, float changeTime)
    {
        Vector3 oldRot = modelTrans.rotation.eulerAngles;

        float elapsedTime = 0;
        while (elapsedTime < changeTime)
        {
            modelTrans.rotation = Quaternion.Euler(new Vector3(oldRot.x, Mathf.SmoothStep(oldRot.y, oldRot.y+changeAmt, elapsedTime/changeTime), oldRot.z));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        modelTrans.rotation = Quaternion.Euler(new Vector3(oldRot.x, oldRot.y + changeAmt, oldRot.z));
    }

    private IEnumerator Battle()
    {
        float elapsedTime;

        // Cutscene mode
        GameManager.cutsceneMode = true;
        UIManager ui = UIManager.instance;

        // Set up the camera shots
        CameraBehavior initialShot = CameraManager.CaptureCurrentShot();
        initialShot.target = transform;
        initialShot.yaw = transform.rotation.eulerAngles.y;
        initialShot.pitch = 0f;
        initialShot.panY = 0f;
        initialShot.zoom = -0.89f;

        CameraBehavior zoomedShot = new CameraBehavior(initialShot);
        zoomedShot.pitch = 20f;
        zoomedShot.zoom = -1.75f;

        CameraBehavior zoomedShotB = new CameraBehavior(zoomedShot);
        FlagsHelper.Set(ref zoomedShotB.changedProperties, CameraProperties.Position);
        zoomedShotB.target = null;
        zoomedShotB.position = Vector3.Lerp(transform.position, GameManager.player.transform.position, 0.35f);
        zoomedShotB.pitch = 40f;
        zoomedShotB.zoom = -20f;


        // Quickly switch to the initial shot
        CameraManager.DoShiftToNewShot(initialShot, 0.01f);
        yield return new WaitForSeconds(0.02f);

        // Play the cutscene
        if (!cutsceneWatched && !skipCutscene)
        {
            ui.StartCoroutine(ui.ScreenFadeChange(0f, 7f));
            Camera.main.fieldOfView = 60f;

            CameraManager.DoShiftToNewShot(zoomedShot, 14f);
            yield return new WaitForSeconds(10f);

            elapsedTime = 0;
            StartCoroutine(MusicWindDown());
            yield return new WaitForSeconds(5f);

            CameraManager.DoShiftToNewShot(zoomedShotB, 1f);
            StartCoroutine(ReduceFOV());
            yield return new WaitForSeconds(2f);


            // Dialogue 1
            //CameraManager.DoGradualReset(1f);
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
            AudioManager.SetMusic(AudioManager.instance.songs[AudioManager.instance.songs.Count - 2].key);

            CameraManager.DoShiftToNewShot(zoomedShotB, 1f);
            StartCoroutine(ReduceFOV());
            yield return new WaitForSeconds(1f);
        }

        // Begin fight spin anim
        CameraBehavior fightShot = new CameraBehavior();
        fightShot.target = bossArenaTransform;
        fightShot.yaw = 45;

        yield return StartCoroutine(Spin(-22f, 0.5f));

        spinSoundSource = audio.Play("spin", true);
        yield return StartCoroutine(Spin(44f+720f, 1f));
        spinSoundSource.Stop();

        yield return StartCoroutine(Spin(-22f, 0.25f));
        yield return new WaitForSeconds(0.5f);

        GameManager.cutsceneMode = false;

        HealthPoints health = GetComponent<HealthPoints>();

        // Phase 1
        spinSoundSource = audio.Play("spin", true);
        rotSpeed = 15f;
        velocity = (transform.forward + transform.right*0.3f) * -1 * slideSpeed;

        while (health.currentHp > 0)
        {
            yield return null;
        }
    }
}
