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

    private Text numberText;
    private Image iconRenderer;


    public void Start ()
    {
        StartCoroutine(DestroyAtStart());
	}

    IEnumerator DestroyAtStart()
    {
        yield return new WaitForSeconds(0.5f);
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
        if (cubeTrans == null)
            cubeTrans = transform.Find("cube");

        if (iconTrans == null)
            iconTrans = transform.Find("displaycanvas/icon");

        if (numberTrans == null)
            numberTrans = transform.Find("displaycanvas/numbertext");

        iconRenderer = iconTrans.GetComponent<Image>();
        numberText = numberTrans.GetComponent<Text>();
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
                    iconRenderer.sprite = UIManager.instance.collectibleSprites[0];
                    break;
                case (CollectibleType.GreenTooth):
                    iconRenderer.sprite = UIManager.instance.collectibleSprites[0];
                    break;
                case (CollectibleType.Leek):
                    iconRenderer.sprite = UIManager.instance.collectibleSprites[1];
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
        AudioManager.PlaySound("denied");
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

            numberText.fontSize = Mathf.RoundToInt(Mathf.Lerp(125f, 100f, percent));
            numberText.color = Color.Lerp(Color.magenta, Color.white, percent);
            timeRemaining += Time.deltaTime;
            yield return null;
        }
        numberText.fontSize = 100;
        numberText.color = Color.white;
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
        SaveManager.Autosave();
        GameManager.cutsceneMode = true;
        AudioManager.PauseMusic();

        // Start zooming in
        AudioSource quakeSource = AudioManager.PlaySound("quake", false, 1f, (gatesUnlockedThisSession+1f));
        CameraBehavior newShot = CameraManager.CaptureCurrentShot();
        //CameraBehavior origShot = CameraManager.CaptureCurrentShot();
        newShot.yaw = 180 + transform.rotation.eulerAngles.y;
        newShot.pitch = 0f;
        newShot.zoom = -8.5f - 7f*(transform.lossyScale.x-1f);
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
        AudioManager.PlaySound("pop");
        yield return new WaitForSeconds(1f / (gatesUnlockedThisSession + 1f));

        UIManager.DoScreenFadeChange(1f, 0.5f);
        //CameraManager.defaultBehavior.zoom = origShot.zoom;
        //CameraManager.defaultBehavior.pitch = origShot.pitch;
        //CameraManager.defaultBehavior.yaw = origShot.yaw;
        //CameraManager.defaultBehavior.position = origShot.position;
        yield return new WaitForSeconds(0.5f);
        CameraManager.DoGradualReset(0.01f);
        yield return new WaitForSeconds(0.1f);
        UIManager.DoScreenFadeChange(0f, 0.5f);
        yield return new WaitForSeconds(0.25f);

        // Return to gameplay
        GameManager.cutsceneMode = false;
        AudioManager.ResumeMusic();
        gatesUnlockedThisSession++;
    }
}
