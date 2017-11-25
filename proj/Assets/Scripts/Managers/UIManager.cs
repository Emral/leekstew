using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public static GameObject letterboxObj;

    public static GameObject hpBarObj;

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
            if (GameObject.Find("UI_CollectionCounters") != null)
            {
                UnityEngine.UI.Text teethCounter = GameObject.Find("TeethCounter").GetComponent("Text") as UnityEngine.UI.Text;
                UnityEngine.UI.Text leekCounter = GameObject.Find("LeekCounter").GetComponent("Text") as UnityEngine.UI.Text;
                teethCounter.text = GameManager.teethCollected.ToString();
                leekCounter.text = GameManager.leeksCollected.ToString();
            }

            if (GameObject.Find("UI_MusicCredits") != null)
            {
                UnityEngine.UI.Text songName = GameObject.Find("SongName").GetComponent("Text") as UnityEngine.UI.Text;
                UnityEngine.UI.Text songArtist = GameObject.Find("SongArtist").GetComponent("Text") as UnityEngine.UI.Text;
                UnityEngine.UI.Text songAlbum = GameObject.Find("SongAlbum").GetComponent("Text") as UnityEngine.UI.Text;
            }

            GameObject screenFadeObj = GameObject.Find("UI_ScreenFade");
            if (screenFadeObj != null)
            {
                UnityEngine.UI.Image screenFadeScr = screenFadeObj.GetComponent("Image") as UnityEngine.UI.Image;
                Color tempColor = screenFadeScr.color;
                tempColor.a = GameManager.isGamePaused ? 0.5f : screenFadeAmount;
                screenFadeScr.color = tempColor;
            }

            if (letterboxObj != null)
            {
                UnityEngine.UI.Image[] barScrs = letterboxObj.GetComponentsInChildren<UnityEngine.UI.Image>();
                foreach (UnityEngine.UI.Image bar in barScrs)
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

            // Letterbox
            letterboxObj = GameObject.Find("UI_Letterbox");

            // Level intro stuff
            levelIntroObj = GameObject.Find("UI_LevelIntro");

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
        }
        public void DoScreenFadeChange(float goal, float goalTime)
        {
            StartCoroutine(ScreenFadeChange(goal, goalTime));
        }
    #endregion

    #region utility functions
        GameObject AddUISprite(Sprite sprite, Transform parent, Vector3 offset, Vector3 scale)
        {
            GameObject newObj = new GameObject();
            UnityEngine.UI.Image newImage = newObj.AddComponent<UnityEngine.UI.Image>();
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
                        for (int i = 0; i < 20; i++)
                        {
                            hps.Add(AddUISprite(emptyHeartSprite, hpBarObj.transform, new Vector3(0, -0.5f * imageSize, 0), Vector3.one / 3f));
                        }
                    }

                    // Determine the target HP and max HP, use those to determine target heart count, and from there derive the target width
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

                                UnityEngine.UI.Image image = hpObj.GetComponent("Image") as UnityEngine.UI.Image;
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
    #endregion
}
