using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool checkpointActive = false;
    public GameObject activateEffect;
    public GameObject passiveEffect;
    public int checkpointID;

    private Dialog dialog;

    private Transform modelTrans;
    private Shake shake;
    private float squash = 0f;

    private void Update()
    {
        dialog = GetComponent<Dialog>();
        modelTrans = transform.GetChild(0);
        shake = modelTrans.gameObject.GetComponent<Shake>();

        if (dialog != null)
        {
            dialog.dialogEnabled = IsCurrent();
        }

        Player playerScr = GameManager.player;
        if (playerScr != null)
        {
            Transform playerTrans = playerScr.transform;
            float distanceToPlayer = Vector3.Distance(transform.position, playerTrans.position);

            // Excite if not active
            if (!IsCurrent())
            {
                modelTrans.LookAt(playerScr.transform);
                squash = Mathf.Lerp(0.5f, 0f, Mathf.InverseLerp(1.5f, 4f, distanceToPlayer));
                shake.effectAmount = squash*0.5f;

                Vector3 shakeVector = shake.shakeOffset;
                shakeVector.y = 0f;

                modelTrans.localPosition = shakeVector;


                // Make current on contact
                if (distanceToPlayer < 1.5f)
                    SetCurrent();
            }
        }

        // Handle squash and stretch
        modelTrans.localScale = Vector3.one + new Vector3(squash, -squash, squash);
    }

    public bool IsCurrent()
    {
        return LevelManager.currentCheckpoint == checkpointID;
    }

    public void SetCurrent()
    {
        // Make this the current checkpoint
        if (LevelManager.currentCheckpoint != checkpointID)
        {
            LevelManager.currentCheckpoint = checkpointID;
            LevelManager.checkpointRoom = LevelManager.currentRoom;

            GetComponent<AudioSource>().Play();
            GameManager.ScreenShake(1f);
            if (activateEffect != null)
                GameObject.Instantiate(activateEffect, transform.position, Quaternion.identity);
        }
        SetActive();


        // Start the bouncing animation
        StartCoroutine(BounceAnim());
    }


    public void SetActive()
    {
        // Generate the beacon of light if not active
        if (passiveEffect != null && !checkpointActive)
            GameObject.Instantiate(passiveEffect, transform.position, Quaternion.identity);

        // Make active
        checkpointActive = true;

        // Add self to the list of active checkpoints
        if (!LevelManager.checkpointsActive.Contains(checkpointID))
        {
            LevelManager.checkpointsActive.Add(checkpointID);
        }
    }


    public IEnumerator BounceAnim()
    {
        while(modelTrans == null)
        {
            yield return null;
        }

        // Get the necessary references and stuff
        Transform catModel = modelTrans.Find("mesh_catplanetcat");
        MeshRenderer renderer = catModel.GetComponent<MeshRenderer>();
        Material originalMat = renderer.material;
        Material myMat = new Material(originalMat);

        Vector3 originalPos = modelTrans.localPosition;

        Rotator rotator = modelTrans.gameObject.AddComponent<Rotator>();
        rotator.amount = new Vector3(0, 2f, 0);

        float prevBounceTime = Time.time * 20f;

        // Loop the loop the loop
        while (LevelManager.currentCheckpoint == checkpointID)
        {
            // Catculate the bounce math stuff
            float bounceTime = Time.time * 20f;
            float scaleMult = Mathf.Cos(bounceTime);
            float jumpMult = Mathf.Cos(bounceTime*0.5f - Mathf.Deg2Rad*60);

            if ((bounceTime*Mathf.Rad2Deg)%360 <= (prevBounceTime * Mathf.Rad2Deg)%360)
            {
                modelTrans.GetComponent<AudioSource>().Play();
            }
            prevBounceTime = bounceTime;

            // Commit the bounce math stuff
            squash = scaleMult*0.125f;
            modelTrans.localPosition = Vector3.up * 0.75f * Mathf.Abs(jumpMult);

            // Color effect
            myMat.color = Color.HSVToRGB((Time.time*0.5f)%1f,0.5f,1f);
            renderer.material = myMat;

            yield return null;
        }

        // Reset to non-active state
        renderer.material = originalMat;
        GameObject.Destroy(rotator);
        modelTrans.localPosition = originalPos;
    }
}
