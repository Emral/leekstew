using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


/*
[System.Serializable]
public class GamepadButtonImage
{
    public string name;
    public Sprite image;
    public bool flip;
    public bool rotate;
}
*/


public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;

    public Texture2D talkButtonTexture;

    public Texture healthSpritesheet;
    public Sprite heartSprite;
    public Sprite soulHeartSprite;
    public Sprite emptyHeartSprite;

    public Sprite[] collectibleSprites;

    public Font dialogFont;
    public Texture2D dialogTexture;

    public static float screenFadeAmount = 1f;
    public static float letterboxFadeAmount = 0f;

    public CanvasGroup levelIntroGroup;
    public GameObject letterboxObj;

    public static float pickupFadeCounter = 2f;
    public CanvasGroup pickupBarGroup;
    public Text teethCounter;
    public Text leekCounter;
    [HideInInspector]
    public int currentTeethShown;

    public static float hpFadeCounter = 2f;

    public static float musicFadeCounter = 0f;
    public CanvasGroup musicCreditsGroup;
    public RectTransform musicCreditsObj;
    public Text songInfo;

    //public Text saveTextObj;
    //public CanvasGroup saveTextGroup;
    public CanvasGroup hpBarGroup;
    public GameObject canvasObj;
    public GameObject pauseMenuObj;
    public GameObject[] otherMenuObjs;
    public Image screenFadeScr;

    public UnityEngine.EventSystems.StandaloneInputModule inputModule;
    public UnityEngine.EventSystems.EventSystem eventSystem;

    //public GamepadButtonImage[] gamepadButtonImages;
    //private Dictionary<string, Sprite> gamepadButtonImageDict;

    //public Sprite keyboardNineSliceSprite;



    #region monobehavior events
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    private void Start()
    {

    }

    public static void InitUI()
    {
        instance.StartCoroutine(instance.UpdateHPBar());
        instance.StartCoroutine(instance.UpdateToothMonitoring());
    }
    void Update()
    {
        UpdateUI();
        UpdateMenus();
    }
    void OnGUI()
    {
        // Reset HUD fading if the game is paused
        if (GameManager.isGamePaused)
        {
            hpFadeCounter = 0f;
            musicFadeCounter = 0f;
            pickupFadeCounter = 0f;
        }

        if (!GameManager.playerExists)
        {
            hpFadeCounter = 8f;
            pickupFadeCounter = 8f;
            //musicFadeCounter = 8f;
        }

        {
            bool playerHpCheck = true;
            if (GameManager.playerExists)
            {
                playerHpCheck = GameManager.player.health.currentHp >= GameManager.player.health.hp;
            }

            bool shouldFade = hpFadeCounter < 7f || (GameManager.timeSinceInput <= 1f && playerHpCheck);
            hpFadeCounter += (shouldFade ? 1 : -1) * Time.deltaTime;
            hpFadeCounter = Mathf.Max(hpFadeCounter, 0f);
            if (hpFadeCounter >= 7f)
                hpFadeCounter = Mathf.Clamp(hpFadeCounter, 7f, 8f);

            hpBarGroup.alpha = 8f - hpFadeCounter;
        }
        
        teethCounter.text = currentTeethShown.ToString();
        leekCounter.text = SaveManager.currentSave.TotalLeeks.ToString();

        {
            bool shouldFade = (GameManager.timeSinceInput <= 1f || pickupFadeCounter < 7f);
            pickupFadeCounter += (shouldFade ? 1 : -1) * Time.deltaTime;
            pickupFadeCounter = Mathf.Max(pickupFadeCounter, 0f);
            if (pickupFadeCounter >= 7f)
                pickupFadeCounter = Mathf.Clamp(pickupFadeCounter, 7f, 8f);

            pickupBarGroup.alpha = 8f - pickupFadeCounter;
        }

        if (AudioManager.currentSong != null)
        {
            songInfo.text = AudioManager.currentSong.name + (AudioManager.currentSong.artist==null?"":"\n" + AudioManager.currentSong.artist) + (AudioManager.currentSong.album==null?"":"\n" + AudioManager.currentSong.album);
            musicCreditsObj.sizeDelta = new Vector2(songInfo.preferredWidth + 20f, songInfo.preferredHeight + 8f);
        }

        musicFadeCounter += Time.deltaTime;
        musicFadeCounter = Mathf.Clamp(musicFadeCounter, 0f, 8f);

        if (!OptionsManager.showMusicCredits && !GameManager.isGamePaused)
            musicFadeCounter = 8f;

        musicCreditsGroup.alpha = 8f - musicFadeCounter;
        {
            Color tempColor = screenFadeScr.color;
            tempColor.a = GameManager.isGamePaused ? 0.5f : screenFadeAmount;
            screenFadeScr.color = tempColor;
        }
        
        //Image[] barScrs = letterboxObj.GetComponentsInChildren<Image>();
        //foreach (Image bar in barScrs)
        //{
        //    Color tempColor = bar.color;
        //    tempColor.a = letterboxFadeAmount;
        //    bar.color = tempColor;
        //}
    }
    #endregion
    
    void UpdateUI()
    {
        // Letterbox
        letterboxFadeAmount = Mathf.Max(0f, letterboxFadeAmount - 0.05f);
        if (GameManager.cutsceneMode)
        {
            letterboxFadeAmount = Mathf.Min(1f, letterboxFadeAmount + 0.1f);
        }
    }
    void UpdateMenus()
    {
        // Handle pausing
        GameManager.isGamePaused = true;

        GameObject activeObj = null;

        foreach (GameObject g in otherMenuObjs)
        {
            if (g.activeInHierarchy)
            {
                activeObj = g;
            }
        }
        if (pauseMenuObj.activeInHierarchy)
        {
            if (GameManager.inputRelease["Pause"])
            {
                UnpauseGame();
            }
        } else if (activeObj != null)
        {
            if (GameManager.inputRelease["Pause"] || GameManager.inputRelease["Run"])
            {
                activeObj.SetActive(false);
                PauseGame();
            }

        } else
        {
            GameManager.isGamePaused = false;
            if (GameManager.inputRelease["Pause"])
                PauseGame();
        }
    }

    #region methods
    void PauseGame()
    {
        Time.timeScale = 0;
        pauseMenuObj.SetActive(true);
        eventSystem.SetSelectedGameObject(GameObject.Find("UI_PauseMenu_Button1"));
    }
    void UnpauseGame()
    {
        Time.timeScale = GameManager.timeRate;
        pauseMenuObj.SetActive(false);
        musicFadeCounter = 999f;
        pickupFadeCounter = 999f;
    }
    public static void DoScreenFadeChange(float goal, float goalTime, float delay=0f)
    {
        instance.StartCoroutine(instance.ScreenFadeChange(goal, goalTime, delay));
    }
    public static void DoFadeCanvasGroup(CanvasGroup group, float newAlpha, float fadeTime)
    {
        instance.StartCoroutine(instance.FadeCanvasGroup(group, newAlpha, fadeTime));
    }
    #endregion

    #region utility functions
        GameObject AddUISprite(Sprite sprite, Transform parent, Vector3 offset, Vector3 scale)
        {
            GameObject newObj = new GameObject();
            //newObj.name = "Spawned by UIManager";
            Image newImage = newObj.AddComponent<Image>();
            newImage.sprite = sprite;
            newObj.GetComponent<RectTransform>().SetParent(parent);
            newObj.transform.localPosition = offset;
            newObj.transform.localScale = scale;
            newObj.transform.localRotation = Quaternion.identity;
            newObj.SetActive(false);

            return newObj;
        }
    #endregion

    #region coroutines
    public IEnumerator UpdateHPBar()
    {
        while (GameManager.player == null)
        {
            yield return null;
        }

        print("STARTING HP COROUTINE");

        print("HP BAR OBJECT FOUND");

        // References and vars
        RectTransform panel;
        panel = hpBarGroup.GetComponent<RectTransform>();
        HealthPoints playerHP = GameManager.player.health;

        // Health variables
        int currentHP = 2;
        int targetHP = 2;
        int currentMaxHP = 3;
        int targetMaxHP = 3;
        int currentHeartCount = 3;
        int targetHeartCount = 3;
        float currentWidth = 1;
        float targetWidth = 1;

        // Measurements
        int imageSize = 48;
        int gapWidth = 8;


        // Initialize HP list
        List<GameObject> hps = new List<GameObject>();

        // Loop
        while (true)
        {

            hpBarGroup.transform.SetParent(canvasObj.transform);
                
            playerHP = GameManager.player.health;

            bool barExists = hpBarGroup != null;
            bool hpExists = playerHP != null;
            bool panelExists = panel != null;

            //print(" HP CHECKS: " + barExists.ToString() + " " + hpExists.ToString() + " " + panelExists.ToString());
            //print(" HP CHECKS 2: " + currentHeartCount.ToString() + " " + currentHP.ToString() + " " + currentMaxHP.ToString() + " " + currentWidth.ToString());

            if (barExists && hpExists && panelExists)
            {

                // Repopulate the HP objects
                if (hpBarGroup.transform.childCount == 0)
                {
                    hps.Clear();
                    for (int i = 0; i < 50; i++)
                    {
                        hps.Add(AddUISprite(emptyHeartSprite, hpBarGroup.transform, new Vector3(0, -0.5f * imageSize, 0), Vector3.one / 3f));
                    }
                }

                // Determine the target HP and max HP, use those to determine target heart count, and from there derive the target width
                gapWidth = Mathf.Max(-Mathf.CeilToInt(imageSize*0.75f), 8-(targetHeartCount-3));
                targetHP = playerHP.currentHp;
                targetMaxHP = playerHP.hp;

                targetHeartCount = Mathf.Max(targetHP, targetMaxHP);
                targetWidth = targetHeartCount * (imageSize + gapWidth) - gapWidth + 0.5f * imageSize;


                // Width lerp, yo
                currentWidth = Mathf.Lerp(currentWidth, targetWidth, 0.25f);

                // Adjust the panel based on the width
                panel.pivot = new Vector2(panel.pivot.x, 0f);
                panel.offsetMin = new Vector2(panel.pivot.x - currentWidth * 0.5f, panel.pivot.y - (imageSize + 8f));
                panel.offsetMax = new Vector2(panel.pivot.x + currentWidth * 0.5f, panel.pivot.y);


                // Loop through each sprite object and do stuffs
                for (int i = 0; i < Mathf.Min(hps.Count, currentHeartCount + 1); i++)
                {
                    if (hps[i] != null)
                    {

                        // Position the object based on the current panel width
                        hps[i].transform.localPosition = new Vector3(-0.5f * currentWidth + i * (imageSize + gapWidth) + 0.5f * imageSize + 16f, hps[i].transform.localPosition.y, 0f);

                        //Determine whether to use the object
                        if (i >= currentHeartCount)
                        {
                            hps[i].SetActive(false);
                        }

                        else
                        {
                            // If the object is used, determine the sprite
                            hps[i].SetActive(true);
                            Sprite currentSprite = emptyHeartSprite;
                            if (i < targetHP)
                            {
                                currentSprite = heartSprite;
                                if (i >= currentMaxHP)
                                    currentSprite = soulHeartSprite;
                            }

                            Image image = hps[i].GetComponent<Image>();
                            image.sprite = currentSprite;
                        }
                    }
                }

                // Update current properties to change to target properties
                if (targetHeartCount > currentHeartCount)
                {
                    currentHeartCount++;
                }
                else if (targetHeartCount < currentHeartCount)
                {
                    currentHeartCount--;
                }

                if (targetMaxHP > currentMaxHP)
                {
                    currentMaxHP++;
                }
                else if (targetMaxHP < currentMaxHP)
                {
                    currentMaxHP--;
                }

                if (targetHP > currentHP)
                {
                    currentHP++;
                }
                else if (targetHP < currentHP)
                {
                    currentHP--;
                }
            }
            yield return null;
        }

    }

    public IEnumerator UpdateToothMonitoring()
    {
        while (true)
        {
            int toothGap = SaveManager.currentSave.NetTeeth - currentTeethShown;
            if (toothGap != 0)
            {
                int startValue = currentTeethShown;
                float goalTime = 1f;
                float currentTime = 0f;
                while (currentTime < goalTime)
                {
                    int endValue = SaveManager.currentSave.NetTeeth;
                    currentTeethShown = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, currentTime/goalTime));
                    pickupFadeCounter = 0f;
                    currentTime += Time.deltaTime;
                    yield return null;
                }
                currentTeethShown = SaveManager.currentSave.NetTeeth;
            }
            yield return null;
        }
    }

    /*public IEnumerator ShowSaving()
    {
        saveTextObj.text = "Saving...";
        yield return FadeCanvasGroup(saveTextGroup, 1, 0.5f);
    }
    public IEnumerator ShowSaveFinished()
    {
        saveTextObj.text = "Game Saved!";
        yield return new WaitForSeconds(1f);
        yield return FadeCanvasGroup(saveTextGroup, 1, 1f);
    }*/

    public IEnumerator ScreenFadeChange(float goal, float goalTime, float delay = 0f)
    {
        if (delay != 0f)
            yield return new WaitForSeconds(delay);

        float startAmount = screenFadeAmount;
        float currentTime = 0;
        while (currentTime < goalTime)
        {
            screenFadeAmount = Mathf.Lerp(startAmount, goal, currentTime / goalTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
        screenFadeAmount = goal;
    }
    public IEnumerator FadeCanvasGroup(CanvasGroup group, float newAlpha, float fadeTime)
    {
        if (group != null)
        {
            float startAmount = group.alpha;
            float currentTime = 0;
            while (currentTime < fadeTime)
            {
                group.alpha = Mathf.Lerp(startAmount, newAlpha, currentTime / fadeTime);
                currentTime += Time.deltaTime;
                yield return null;
            }
            group.alpha = newAlpha;
        }
    }
    #endregion
}
