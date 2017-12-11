using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CollectibleType { Tooth, GreenTooth, Leek };


public class CollectionGate : NPC
{
    public static int gatesUnlockedThisSession = 0;

    public CollectibleType type = CollectibleType.Tooth;
    public int cost = 10;

    private bool hidden = false;

    private Transform iconTrans;
    private Transform cubeTrans;
    private Transform numberTrans;

    private TextMesh numberText;
    private MeshRenderer iconRenderer;


    public void Start ()
    {
        StartCoroutine(DestroyAtStart());
	}

    IEnumerator DestroyAtStart()
    {
        while(SaveManager.CurrentLevelSave == null)
        {
            yield return null;
        }
        UpdateRefs();

        if (SaveManager.CurrentLevelSave.collectGatesOpened.Contains(instanceID))
            HideTheKids(false);

        yield return null;
    }

    public override void Update()
    {
        base.Update();

        if (hidden)
            talkCooldown = 99f;

        UpdateRefs();
        UpdateDisplay();
    }

    public void UpdateRefs()
    {
        if (iconTrans == null)
            iconTrans = transform.Find("icon");

        if (cubeTrans == null)
            cubeTrans = transform.Find("cube");

        if (numberTrans == null)
            numberTrans = transform.Find("numbertext");

        iconRenderer = iconTrans.GetComponent<MeshRenderer>();
        numberText = numberTrans.GetComponent<TextMesh>();
    }

    public void UpdateDisplay()
    {
        if(numberText != null)
        {
            numberText.text = cost.ToString();
        }

        if (iconRenderer != null)
        {
            switch (type)
            {
                case (CollectibleType.Tooth):
                    iconRenderer.material.mainTextureOffset = Vector2.zero;
                    break;
                case (CollectibleType.GreenTooth):
                    iconRenderer.material.mainTextureOffset = Vector2.zero;
                    break;
                case (CollectibleType.Leek):
                    iconRenderer.material.mainTextureOffset = Vector2.up * 0.5f;
                    break;
            }
        }
        //else
            //print("COLLECTION CUBE CAN'T UPDATE ITS DISPLAY");
    }


    public void HideTheKids(bool val)
    {
        iconTrans.gameObject.SetActive(val);
        cubeTrans.gameObject.SetActive(val);
        numberTrans.gameObject.SetActive(val);
        hidden = !val;
    }

    public void Deny()
    {
        UIManager.pickupFadeCounter = 0f;
        AudioManager.PlayDeniedSound();
        StartCoroutine(DenyDisplay());
    }


    public override IEnumerator OnPlayerInteract()
    {
        switch (type)
        {
            case (CollectibleType.Tooth):
                if (SaveManager.currentSave.NetTeeth >= cost)
                {
                    SaveManager.currentSave.teethSpent += cost;
                    StartCoroutine(OpenSequence());
                }
                else
                   Deny();

                break;


            case (CollectibleType.GreenTooth):
                if (SaveManager.currentSave.NetGreenTeeth >= cost)
                {
                    SaveManager.currentSave.greenTeethSpent += cost;
                    StartCoroutine(OpenSequence());
                }
                else
                    Deny();

                break;


            case (CollectibleType.Leek):
                if (SaveManager.currentSave.TotalLeeks >= cost)
                    StartCoroutine(OpenSequence());
                else
                    Deny();

                break;
        }
        yield return null;
    }


    public IEnumerator DenyDisplay()
    {
        Vector3 newPos = Vector3.zero;
        float totalTime = 0.5f;
        float timeRemaining = 0f;
        while (timeRemaining < totalTime)
        {
            float percent = timeRemaining / totalTime;

            newPos = numberTrans.localPosition;
            newPos.y = Mathf.Lerp(-1.5f, -1.2f, percent);
            numberTrans.localPosition = newPos;
            numberText.fontSize = Mathf.RoundToInt(Mathf.Lerp(600f, 370f, percent));
            numberText.color = Color.Lerp(Color.magenta, Color.black, percent);
            timeRemaining += Time.deltaTime;
            yield return null;
        }
        numberText.fontSize = 370;
        numberText.color = Color.black;

        newPos.y = -1.2f;
        numberTrans.localPosition = newPos;
    }


    public IEnumerator ShakeIncrease()
    {
        float totalTime = 8f/(gatesUnlockedThisSession+1f);
        float timeRemaining = 0f;
        while(timeRemaining < totalTime)
        {
            CameraManager.constantShake = Mathf.Lerp(0f,4f, timeRemaining/totalTime);
            timeRemaining += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator OpenSequence()
    {
        SaveManager.CurrentLevelSave.collectGatesOpened.Add(instanceID);
        GameManager.cutsceneMode = true;
        AudioManager.PauseMusic();

        // Start zooming in
        AudioSource quakeSource = AudioManager.PlaySound(AudioManager.instance.quakeSound, 1f, (gatesUnlockedThisSession+1f));
        CameraBehavior newShot = CameraManager.CaptureCurrentShot();
        newShot.yaw = 180 + transform.rotation.eulerAngles.y;
        newShot.pitch = 0f;
        newShot.zoom = -8.5f;
        //newShot.panY = -1;
        newShot.target = transform;

        StartCoroutine(ShakeIncrease());
        CameraManager.DoShiftToNewShot(newShot, 8f/(gatesUnlockedThisSession+1f));
        yield return new WaitForSeconds(8.5f / (gatesUnlockedThisSession + 1f));

        // Disappear
        quakeSource.Stop();
        CameraManager.constantShake = 0;
        yield return new WaitForSeconds(0.5f / (gatesUnlockedThisSession + 1f));

        HideTheKids(false);
        AudioManager.PlayPopSound();
        yield return new WaitForSeconds(1f / (gatesUnlockedThisSession + 1f));

        CameraManager.DoGradualReset(1f / (gatesUnlockedThisSession + 1f));
        yield return new WaitForSeconds(0.5f / (gatesUnlockedThisSession + 1f));

        // Return to gameplay
        GameManager.cutsceneMode = false;
        AudioManager.ResumeMusic();
        gatesUnlockedThisSession++;
    }
}
