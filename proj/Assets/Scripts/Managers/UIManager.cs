using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;

    public Texture2D talkButtonTexture;

    public Texture healthSpritesheet;
    public Sprite heartSprite;
    public Sprite soulHeartSprite;
    public Sprite emptyHeartSprite;

    public Font dialogFont;
    public Texture2D dialogTexture;

    public static float screenFadeAmount = 1f;
    public static float letterboxFadeAmount = 0f;


    public static GameObject levelIntroObj;
    public static CanvasGroup levelIntroGroup;
    public static GameObject letterboxObj;

    public static float pickupFadeCounter = 2f;
    public static GameObject pickupBarObj;
    public static CanvasGroup pickupBarGroup;

    public static float hpFadeCounter = 2f;
    public static GameObject hpBarObj;
    public static CanvasGroup hpBarGroup;

    public static float musicFadeCounter = 0f;
    public static GameObject musicCreditsObj;
    public static CanvasGroup musicCreditsGroup;

    public static GameObject canvasObj;
    public static GameObject pauseMenuObj;
    public static GameObject optionsMenuObj;
    public static GameObject exitMenuObj;
    public static GameObject currentMenuObj;

    public static UnityEngine.EventSystems.StandaloneInputModule inputModule;
    public static UnityEngine.EventSystems.EventSystem eventSystem;



    #region monobehavior events
    private void Awake()
    {
    if (instance == null)
        instance = this;
    }
    public static void InitUI()
    {
        instance.StartCoroutine(instance.UpdateHPBar());
    }
    void Update()
    {
        UpdateRefs();
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

        if (hpBarObj != null)
        {
            bool shouldFade = hpFadeCounter < 7f || (GameManager.timeSinceInput <= 1f && GameManager.player.health.currentHp >= GameManager.player.health.hp);
            hpFadeCounter += (shouldFade ? 1 : -1)* Time.deltaTime;
            hpFadeCounter = Mathf.Max(hpFadeCounter, 0f);
            if (hpFadeCounter >= 7f)
                hpFadeCounter = Mathf.Clamp(hpFadeCounter, 7f, 8f);

            hpBarGroup.alpha = 8f - hpFadeCounter;
        }

        if (pickupBarObj != null)
        {
            Text teethCounter = GameObject.Find("TeethCounter").GetComponent("Text") as Text;
            Text leekCounter = GameObject.Find("LeekCounter").GetComponent("Text") as Text;
            teethCounter.text = GameManager.teethCollected.ToString();
            leekCounter.text = GameManager.leeksCollected.ToString();

            bool shouldFade = (GameManager.timeSinceInput <= 1f || pickupFadeCounter < 7f);
            pickupFadeCounter += (shouldFade ? 1 : -1) * Time.deltaTime;
            pickupFadeCounter = Mathf.Max(pickupFadeCounter, 0f);
            if (pickupFadeCounter >= 7f)
                pickupFadeCounter = Mathf.Clamp(pickupFadeCounter, 7f, 8f);

            pickupBarGroup.alpha = 8f - pickupFadeCounter;
        }

        if (musicCreditsObj != null)
        {
            RectTransform musicCreditsBox = musicCreditsObj.GetComponent<RectTransform>();
            Text songInfo = GameObject.Find("SongInfo").GetComponent("Text") as Text;

            if (AudioManager.currentSong != null)
            {
                songInfo.text = AudioManager.currentSong.name + (AudioManager.currentSong.artist==null?"":"\n" + AudioManager.currentSong.artist) + (AudioManager.currentSong.album==null?"":"\n" + AudioManager.currentSong.album);
                musicCreditsBox.sizeDelta = new Vector2(songInfo.preferredWidth + 20f, songInfo.preferredHeight + 8f);
            }

            musicFadeCounter += Time.deltaTime;
            musicFadeCounter = Mathf.Clamp(musicFadeCounter, 0f, 8f);

            if (!OptionsManager.showMusicCredits && !GameManager.isGamePaused)
                musicFadeCounter = 8f;

            musicCreditsGroup.alpha = 8f - musicFadeCounter;
        }

        GameObject screenFadeObj = GameObject.Find("UI_ScreenFade");
        if (screenFadeObj != null)
        {
            Image screenFadeScr = screenFadeObj.GetComponent("Image") as Image;
            Color tempColor = screenFadeScr.color;
            tempColor.a = GameManager.isGamePaused ? 0.5f : screenFadeAmount;
            screenFadeScr.color = tempColor;
        }

        if (letterboxObj != null)
        {
            Image[] barScrs = letterboxObj.GetComponentsInChildren<Image>();
            foreach (Image bar in barScrs)
            {
                Color tempColor = bar.color;
                tempColor.a = letterboxFadeAmount;
                bar.color = tempColor;
            }
        }
    }
    #endregion

    #region update
    public static void UpdateRefs()
    {
        // HP Bar
        hpBarObj = GameObject.Find("UI_PlayerHP");
        if (hpBarObj != null)
            hpBarGroup = hpBarObj.GetComponent<CanvasGroup>();

        // Pickup counters
        pickupBarObj = GameObject.Find("UI_CollectionCounters");
        if (pickupBarObj != null)
            pickupBarGroup = pickupBarObj.GetComponent<CanvasGroup>();

        // Music credits
        musicCreditsObj = GameObject.Find("UI_MusicCredits");
        if (musicCreditsObj != null)
            musicCreditsGroup = musicCreditsObj.GetComponent<CanvasGroup>();

        // Letterbox
        letterboxObj = GameObject.Find("UI_Letterbox");

        // Level intro stuff
        levelIntroObj = GameObject.Find("UI_LevelIntro");
        if (levelIntroObj != null)
            levelIntroGroup = levelIntroObj.GetComponent<CanvasGroup>();

        // Menu references
        currentMenuObj = null;
        canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            foreach (GameObject menu in GameObject.FindGameObjectsWithTag("Menu"))
            {
                if (menu.activeInHierarchy)
                    currentMenuObj = menu;
            }
        }

        if (pauseMenuObj == null)
        {
            pauseMenuObj = GameObject.Find("Menu_Pause");
            if (pauseMenuObj != null)
                pauseMenuObj.SetActive(false);
        }

        if (exitMenuObj == null)
        {
            exitMenuObj = GameObject.Find("Menu_Exit");
            if (exitMenuObj != null)
                exitMenuObj.SetActive(false);
        }

        if (optionsMenuObj == null)
        {
            optionsMenuObj = GameObject.Find("Menu_Options");
            if (optionsMenuObj != null)
                optionsMenuObj.SetActive(false);
        }


        // UI event stuff
        GameObject eventSystemObj = GameObject.Find("EventSystem");
        if (eventSystemObj != null)
        {
            inputModule = eventSystemObj.GetComponent("StandaloneInputModule") as UnityEngine.EventSystems.StandaloneInputModule;
            eventSystem = eventSystemObj.GetComponent("EventSystem") as UnityEngine.EventSystems.EventSystem;
        }
    }
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
        if (currentMenuObj != null)
        {
            //print("CURRENT MENU: " + currentMenuObj.name);
            switch (currentMenuObj.name)
            {
                case "Menu_Pause":
                    if (GameManager.inputRelease["Pause"])
                        UnpauseGame();
                    break;

                default:
                    if (GameManager.inputRelease["Pause"] || GameManager.inputRelease["Run"])
                    { 
                        currentMenuObj.SetActive(false);
                        PauseGame();
                    }
                    break;
            }
        }
        else
        {
            GameManager.isGamePaused = false;
            if (GameManager.inputRelease["Pause"])
                PauseGame();
        }
    }
    #endregion

    #region methods
    void PauseGame()
    {
        if (pauseMenuObj != null)
        {
            Time.timeScale = 0;
            pauseMenuObj.SetActive(true);
            eventSystem.SetSelectedGameObject(GameObject.Find("UI_PauseMenu_Button1"));
        }
    }
    void UnpauseGame()
    {
        Time.timeScale = GameManager.timeRate;
        pauseMenuObj.SetActive(false);
        musicFadeCounter = 999f;
        pickupFadeCounter = 999f;
    }
    public static void DoScreenFadeChange(float goal, float goalTime)
    {
        instance.StartCoroutine(instance.ScreenFadeChange(goal, goalTime));
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
        print("STARTING HP COROUTINE");

        while (hpBarObj == null)
        {
            yield return null;
        }

        print("HP BAR OBJECT FOUND");

        // References and vars
        RectTransform panel;
        panel = hpBarObj.GetComponent<RectTransform>();
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
            hpBarObj.transform.parent = canvasObj.transform;
                
            playerHP = GameManager.player.health;
            hpBarObj = GameObject.Find("UI_PlayerHP");
            panel = hpBarObj.GetComponent<RectTransform>();

            bool barExists = hpBarObj != null;
            bool hpExists = playerHP != null;
            bool panelExists = panel != null;

            //print(" HP CHECKS: " + barExists.ToString() + " " + hpExists.ToString() + " " + panelExists.ToString());
            //print(" HP CHECKS 2: " + currentHeartCount.ToString() + " " + currentHP.ToString() + " " + currentMaxHP.ToString() + " " + currentWidth.ToString());

            if (barExists && hpExists && panelExists)
            {

                // Repopulate the HP objects
                if (hpBarObj.transform.GetChildCount() == 0)
                {
                    hps.Clear();
                    for (int i = 0; i < 50; i++)
                    {
                        hps.Add(AddUISprite(emptyHeartSprite, hpBarObj.transform, new Vector3(0, -0.5f * imageSize, 0), Vector3.one / 3f));
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

                    GameObject hpObj = hps[i];

                    if (hpObj != null)
                    {

                        // Position the object based on the current panel width
                        hpObj.transform.localPosition = new Vector3(-0.5f * currentWidth + i * (imageSize + gapWidth) + 0.5f * imageSize + 16f, hpObj.transform.localPosition.y, 0f);

                        //Determine whether to use the object
                        if (i >= currentHeartCount)
                        {
                            hpObj.SetActive(false);
                        }

                        else
                        {
                            // If the object is used, determine the sprite
                            hpObj.SetActive(true);
                            Sprite currentSprite = emptyHeartSprite;
                            if (i < targetHP)
                            {
                                currentSprite = heartSprite;
                                if (i >= currentMaxHP)
                                    currentSprite = soulHeartSprite;
                            }

                            Image image = hpObj.GetComponent("Image") as Image;
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
    public IEnumerator ScreenFadeChange(float goal, float goalTime)
    {
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
    #endregion
}
