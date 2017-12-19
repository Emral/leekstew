using UnityEngine;
using System.Collections;
using System.Collections.Generic;       //Allows us to use Lists.
using UnityEngine.SceneManagement;


public enum ControllerType { Keyboard, Gamepad };

public class GameManager : MonoBehaviour
{
    public static ControllerType controllerType = ControllerType.Keyboard;
    public static string controllerTypeStr = "Keyboard";

    public static string[] inputVerbs =                    { "Walk X", "Walk Y", "Cam X", "Cam Y", "Cam Focus", "Run", "Jump", "Pause" };
    public static string[] controllerChangeVerbs =         { "Walk X", "Walk Y", "Run", "Jump", "Pause", "Cam Focus" };

    public static List<string> inputsEaten              = new List<string>();
    public static List<string> eatenInputsRegistered    = new List<string>();
    public static Dictionary<string, float> inputVals   = new Dictionary<string, float>();
    public static Dictionary<string, bool> inputPress   = new Dictionary<string, bool>();
    public static Dictionary<string, bool> inputRelease = new Dictionary<string, bool>();

    public static Dictionary<int, bool> itemsCollected =        new Dictionary<int, bool>();
    public static Dictionary<int, System.Type> collectedTypes = new Dictionary<int, System.Type>();
    public static int teethCollected = 0;
    public static int leeksCollected = 0;

    public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.

    public static Player player = null;                     //Static reference to the Player script
    public static bool playerExists = false;
    public static GameObject playerObject = null;

    public static bool cameraExists = false;
    public static GameObject cameraObject = null;

    public static UIManager ui = null;

    public static bool isGamePaused = false;
    public static bool cutsceneMode = false;

    public static float timeSinceInput = 0f;


    public static int youDidIt = -1;

    public static float timeRate = 1f;


    #region manual scene change event stuff
    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("Level Loaded: " + scene.name + ", " + mode);

        player = null;

        playerObject = GameObject.Find("Obj_Player");
        playerExists = (playerObject != null);
        if (playerExists)
        {
            player = playerObject.GetComponent<Player>();
        }

        cameraObject = GameObject.Find("Obj_Camera");
        cameraExists = (cameraObject != null);

        LevelManager.InitLevel();
    }
    #endregion


    #region monobehavior events
    public void Awake()
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

        //Call the InitGame function to initialize the first level 
        InitGame();
        SaveManager.InitSave();
        UIManager.InitUI();
    }
    void Update()
    {
        UpdateRefs();
        ManageInput();
    }
    #endregion

    #region initialization
    void InitGame()
    {
        Physics.gravity = new Vector3(0, -32f, 0);
        foreach (string verb in inputVerbs)
        {
            inputVals[verb] = 0.0f;
            inputPress[verb] = false;
            inputRelease[verb] = false;
            inputPress["Any"] = false;
            inputRelease["Any"] = false;
        }

        UpdateRefs();        
    }
    #endregion

    #region update
    void UpdateRefs()
    {
        // UI manager
        ui = gameObject.GetComponent<UIManager>();
    }
    void ManageInput()
    {
        eatenInputsRegistered.Clear();

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
        float inputAxisAny = 0f;
        inputPress["Any"] = false;
        inputRelease["Any"] = false;
        foreach (string verb in inputVerbs)
        {
            string ctStr = controllerTypeStr + " " + verb;

            inputVals[verb] = Input.GetAxis(ctStr);
            if (inputVals[verb] != 0)
                inputAxisAny = inputVals[verb];
            inputPress[verb] = Input.GetButtonDown(ctStr);
            inputPress["Any"] = inputPress["Any"] || inputPress[verb];
            inputRelease[verb] = Input.GetButtonUp(ctStr);
            inputRelease["Any"] = inputRelease["Any"] || inputRelease[verb];
        }


        // Time since any input was pressed
        timeSinceInput += Time.deltaTime;
        if (inputPress["Any"] || inputRelease["Any"] || inputAxisAny != 0)
        {
            //print("TIME SINCE INPUT RESET");
            timeSinceInput = 0f;
        }


        // Update the input module for the UI accordingly
        if (UIManager.instance.inputModule != null)
        {
            UIManager.instance.inputModule.horizontalAxis = controllerTypeStr + " Walk X";
            UIManager.instance.inputModule.verticalAxis = controllerTypeStr + " Walk Y";
            UIManager.instance.inputModule.cancelButton = controllerTypeStr + " Run";
            UIManager.instance.inputModule.submitButton = controllerTypeStr + " Jump";
        }


        // Eat inputs
        bool anyEaten = inputsEaten.Contains("Any");
        if (anyEaten && inputPress["Any"])
            eatenInputsRegistered.Add("Any");

        foreach (string verb in inputVerbs)
        {
            bool specificEaten = inputsEaten.Contains(verb);

            if (specificEaten || anyEaten)
            {
                inputVals[verb] = 0;
                inputPress[verb] = false;
                inputRelease[verb] = false;
                if (inputPress[verb])
                    eatenInputsRegistered.Add(verb);
            }
        }

        inputsEaten.Clear();
    }
    #endregion

    #region methods
    public void QuitGame()
    {
        if (Application.isEditor)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
    }
        else
            Application.Quit();
    }

    public static void EatInput(string verb = "Any")
    {
        inputsEaten.Add(verb);
    }

    public static bool GetEatenInputPressed(string verb = "Any")
    {
        return eatenInputsRegistered.Contains(verb);
    }

    public static void ScreenShake(float strength)
    {
        CameraManager.instance.StartCoroutine(CameraManager.instance.DelayedShake(strength));
    }
    #endregion

    #region coroutines
    #endregion
}