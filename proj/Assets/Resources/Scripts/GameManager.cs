using UnityEngine;
using System.Collections;
using System.Collections.Generic;       //Allows us to use Lists.
using UnityEditor;



public enum ControllerType { Keyboard, Gamepad };

public class GameManager : MonoBehaviour
{
    public static ControllerType controllerType = ControllerType.Keyboard;
    public static string controllerTypeStr = "Keyboard";

    public static string[] inputVerbs =                    { "Walk X", "Walk Y", "Cam X", "Cam Y", "Cam Focus", "Run", "Jump", "Pause" };
    public static string[] controllerChangeVerbs =         { "Walk X", "Walk Y", "Run", "Jump", "Pause", "Cam Focus" };
    public static Dictionary<string, float> inputVals =    new Dictionary<string, float>();
    public static Dictionary<string, bool> inputPress =    new Dictionary<string, bool>();
    public static Dictionary<string, bool> inputRelease =  new Dictionary<string, bool>();

    public static Dictionary<int, bool> itemsCollected =        new Dictionary<int, bool>();
    public static Dictionary<int, System.Type> collectedTypes = new Dictionary<int, System.Type>();
    public static int teethCollected = 0;
    public static int leeksCollected = 0;

    public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
    public static Player player = null;                     //Static reference to the Player script
    public static CameraManager camera = null;              //Static reference to the CameraManager script

    public static bool isGamePaused = false;

    public static Texture healthSpritesheet;
    public static Sprite heartSprite;
    public static Sprite soulHeartSprite;
    public static Sprite emptyHeartSprite;

    public static float screenFadeAmount = 0f;
    public static float letterboxFadeAmount = 0f;

    public static GameObject letterboxObj;

    public static GameObject canvasObj;
    public static GameObject pauseMenuObj;
    public static GameObject optionsMenuObj;
    public static GameObject exitMenuObj;
    public static GameObject currentMenuObj;

    public static UnityEngine.EventSystems.StandaloneInputModule inputModule;
    public static UnityEngine.EventSystems.EventSystem eventSystem;

    public static Texture talkButtonTex;

    public static float timeRate = 1f;

    public static bool cutsceneMode = false;


    //Awake is always called before any Start functions
    void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        // Initialize certain reference variables
        healthSpritesheet = Resources.Load("Textures/tex_playerHealthSprites") as Texture2D;
        Sprite[] sprites = Resources.LoadAll<Sprite> ("Textures/tex_playerHealthSprites");
        heartSprite = sprites[2];
        soulHeartSprite = sprites[1];
        emptyHeartSprite = sprites[0];

        //Call the InitGame function to initialize the first level 
        InitLevel();
    }

    //Initializes the game for each level.
    void InitLevel()
    {
        Physics.gravity = new Vector3(0, -32f, 0);
        foreach (string verb in inputVerbs)
        {
            inputVals[verb] = 0.0f;
            inputPress[verb] = false;
            inputRelease[verb] = false;
        }

        UpdateRefs();
        camera.target = player.transform;
    }


    private void Start()
    {
        StartCoroutine(UpdateHPBar());
    }


    // Update the static references
    void UpdateRefs()
    {
        // Player
        GameObject playerObj = GameObject.Find("Obj_Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent("Player") as Player;
        }

        // Camera
        GameObject cameraObj = GameObject.Find("Obj_Camera");
        if (cameraObj != null)
        {
            camera = cameraObj.GetComponent("CameraManager") as CameraManager;
        }

        // Letterbox
        letterboxObj = GameObject.Find("UI_Letterbox");

        // Textures
        talkButtonTex = Resources.Load<Texture>("Textures/tex_leftButtonPrompt");

        // Menu references
        currentMenuObj = null;
        canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            foreach (Transform child in canvasObj.transform)
            {
                if (child.name.Contains("Menu_") && child.gameObject.activeInHierarchy)
                {
                    currentMenuObj = child.gameObject;
                }
            }
        }
        if (pauseMenuObj == null)
        {
            pauseMenuObj = GameObject.Find("Menu_Pause");
            pauseMenuObj.SetActive(false);
        }
        if (exitMenuObj == null)
        {
            exitMenuObj = GameObject.Find("Menu_Exit");
            exitMenuObj.SetActive(false);
        }
        if (optionsMenuObj == null)
        {
            optionsMenuObj = GameObject.Find("Menu_Options");
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


    void ManageInput()
    {
        // Get the controller mode
        foreach (string verb in controllerChangeVerbs)
        {
            string kStr = "Keyboard " + verb;
            string gStr = "Gamepad " + verb;

            if (Input.GetButtonDown(kStr))
            {
                controllerType = ControllerType.Keyboard;
            }

            if (Input.GetButtonDown(gStr) || Input.GetAxis(gStr) != 0)
            {
                controllerType = ControllerType.Gamepad;
            }
        }

        controllerTypeStr = "Keyboard";
        if (controllerType == ControllerType.Gamepad)
            controllerTypeStr = "Gamepad";


        // Get the values
        foreach (string verb in inputVerbs)
        {
            string ctStr = controllerTypeStr + " " + verb;

            inputVals[verb] = Input.GetAxis(ctStr);
            inputPress[verb] = Input.GetButtonDown(ctStr);
            inputRelease[verb] = Input.GetButtonUp(ctStr);
        }
    }


    void ManageMenus()
    {
        letterboxFadeAmount = Mathf.Max(0f, letterboxFadeAmount - 0.05f);
        if (cutsceneMode)
        {
            letterboxFadeAmount = Mathf.Min(1f, letterboxFadeAmount + 0.1f);
        }


        if (inputModule != null)
        {
            inputModule.horizontalAxis = controllerTypeStr + " Walk X";
            inputModule.verticalAxis = controllerTypeStr + " Walk Y";
            inputModule.submitButton = controllerTypeStr + " Jump";
            inputModule.cancelButton = controllerTypeStr + " Run";
        }

        isGamePaused = true;
        if (currentMenuObj != null)
        {
            switch(currentMenuObj.name)
            {
                case "Menu_Pause":
                    if (inputRelease["Pause"])
                        UnpauseGame();
                    break;

                default:
                    if (inputRelease["Pause"] || inputRelease["Run"])
                    {
                        currentMenuObj.SetActive(false);
                        PauseGame();
                    }
                    break;
            }
        }
        else
        {
            isGamePaused = false;
            if (inputRelease["Pause"])
                PauseGame();
        }
    }


    /*
    public IEnumerator HealHeart(GameObject heartObj)
    {
        yield return null;
    }


    public IEnumerator ChangeHearts()
    {
        while(true)
        {
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
                yield return StartCoroutine(HealHeart());
            }
            else if (targetMaxHP < currentMaxHP)
            {
                currentMaxHP--;
            }
            yield return null;
        }
    }
    */

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

    public IEnumerator UpdateHPBar()
    {
        GameObject hpBarObj = GameObject.Find("UI_PlayerHP");

        // References and vars
        RectTransform panel = hpBarObj.GetComponent("RectTransform") as RectTransform;
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
        for (int i = 0; i < 10; i++)
        {
            hps.Add(AddUISprite(emptyHeartSprite, hpBarObj.transform, new Vector3 (0, -0.5f*imageSize, 0), Vector3.one/3f));
        }

        // Loop
        while (true)
        {
            playerHP = GameManager.player.health;
            hpBarObj = GameObject.Find("UI_PlayerHP");
            if (hpBarObj != null && playerHP != null)
            {
                // Determine the target HP and max HP, use those to determine target heart count, and from there derive the target width
                targetHP = playerHP.currentHp;
                targetMaxHP = playerHP.hp;

                targetHeartCount = Mathf.Max(targetHP, targetMaxHP);
                targetWidth = targetHeartCount * (imageSize + gapWidth) - gapWidth + 0.5f*imageSize;


                // Width lerp, yo
                currentWidth = Mathf.Lerp(currentWidth, targetWidth, 0.25f);

                // Adjust the panel based on the width
                panel.offsetMin = new Vector2(panel.pivot.x - currentWidth * 0.5f, panel.pivot.y - (imageSize + 8f));
                panel.offsetMax = new Vector2(panel.pivot.x + currentWidth * 0.5f, panel.pivot.y);


                // Loop through each sprite object and do stuffs
                for (int i = 0; i < currentHeartCount + 1; i++)
                {
                    GameObject hpObj = hps[i];

                    // Position the object based on the current panel width
                    hpObj.transform.localPosition = new Vector3(-0.5f * currentWidth + i * (imageSize + gapWidth) + 0.5f*imageSize + 16f, hpObj.transform.localPosition.y, 0f);

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

    void OnGUI()
    {
        if (GameObject.Find("UI_CollectionCounters") != null)
        {
            UnityEngine.UI.Text teethCounter = GameObject.Find("TeethCounter").GetComponent("Text") as UnityEngine.UI.Text;
            UnityEngine.UI.Text leekCounter = GameObject.Find("LeekCounter").GetComponent("Text") as UnityEngine.UI.Text;
            teethCounter.text = teethCollected.ToString();
            leekCounter.text = leeksCollected.ToString();
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
            tempColor.a = isGamePaused ? 0.5f : screenFadeAmount;
            screenFadeScr.color = tempColor;
        }

        if (letterboxObj != null)
        {
            UnityEngine.UI.Image[] barScrs = letterboxObj.GetComponentsInChildren<UnityEngine.UI.Image>();
            foreach(UnityEngine.UI.Image bar in barScrs)
            {
                Color tempColor = bar.color;
                tempColor.a = letterboxFadeAmount;
                bar.color = tempColor;
            }
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

    public void QuitGame()
    {
        if (Application.isEditor)
            EditorApplication.isPlaying = false;
        else
            Application.Quit();
    }

    void PauseGame()
    {
        Time.timeScale = 0;
        pauseMenuObj.SetActive(true);
        eventSystem.SetSelectedGameObject(GameObject.Find("UI_PauseMenu_Button1"));
    }

    void UnpauseGame()
    {
        Time.timeScale = timeRate;
        pauseMenuObj.SetActive(false);
    }


    void Update()
    {
        UpdateRefs();
        ManageInput();
        ManageMenus();
    }
}