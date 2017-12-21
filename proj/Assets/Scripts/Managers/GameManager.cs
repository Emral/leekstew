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
    public static string[] immediateChangeVerbs =          { "Walk X", "Walk Y",                   "Cam Focus", "Run", "Jump", "Pause" };
    public static string[] delayedChangeVerbs =            {                     "Cam X", "Cam Y" };

    public static Dictionary<string, float> directInputHoldTime = new Dictionary<string, float>();  // positive values for held, negative for not

    public static string controllerNameStr;

    public static List<string> inputsEaten                = new List<string>();
    public static List<string> eatenInputsRegistered      = new List<string>();
    public static Dictionary<string, float> inputHoldTime = new Dictionary<string, float>();  // same as the direct version
    public static Dictionary<string, float> inputVals     = new Dictionary<string, float>();
    public static Dictionary<string, bool> inputPress     = new Dictionary<string, bool>();
    public static Dictionary<string, bool> inputRelease   = new Dictionary<string, bool>();

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

    public bool debugMode;

    public GameObject youDidItPrefab;
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
    private void LateUpdate()
    {
        // Reset the list of eaten inputs so they only apply for the current frame
        inputsEaten.Clear();
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
    private void OnGUI()
    {
        if (Debug.isDebugBuild)
        {
            /*
            string inputsStr = "";
            foreach(KeyValuePair<string, float> pair in directInputHoldTime)
            {
                inputsStr += pair.Key + ": " + pair.Value.ToString() + "\n";
            }
            GUI.Label(new Rect(0f,0f, 256f, Screen.height), inputsStr);
            */
        }
    }

    void UpdateRefs()
    {
        // UI manager
        ui = gameObject.GetComponent<UIManager>();
    }
    void ManageInput()
    {
        // We'll manage cursor visibility here because it doesn't want to work anywhere else lol
        Cursor.visible = (GameManager.controllerType == ControllerType.Keyboard && (GameManager.isGamePaused || GameManager.player == null));

        // Reset the list of detected eaten inputs
        eatenInputsRegistered.Clear();

        // Get the direct input hold values
        foreach (string verb in inputVerbs)
        {
            string kStr = "Keyboard " + verb;
            string gStr = "Gamepad " + verb;

            if (!directInputHoldTime.ContainsKey(kStr))
                directInputHoldTime[kStr] = 0f;
            if (!directInputHoldTime.ContainsKey(gStr))
                directInputHoldTime[gStr] = 0f;

            if (Input.GetButton(kStr) || Input.GetAxis(kStr) != 0)
                directInputHoldTime[kStr] = Mathf.Clamp(directInputHoldTime[kStr] + Time.deltaTime, 0f, 30f);
            else
                directInputHoldTime[kStr] = Mathf.Clamp(directInputHoldTime[kStr] - Time.deltaTime, -30f, 0f);

            if (Input.GetButton(gStr) || Input.GetAxis(gStr) != 0)
                directInputHoldTime[gStr] = Mathf.Clamp (directInputHoldTime[gStr] + Time.deltaTime, 0f, 30f);
            else
                directInputHoldTime[gStr] = Mathf.Clamp (directInputHoldTime[gStr] - Time.deltaTime, -30f, 0f);
        }


        // Get the controller mode based on immediate and delayed control detection
        ControllerType prevControllerType = controllerType;

        foreach (string verb in immediateChangeVerbs)
        {
            string kStr = "Keyboard " + verb;
            string gStr = "Gamepad " + verb;

            if (directInputHoldTime[kStr] > 0f)
            {
                controllerType = ControllerType.Keyboard;
            }

            if (directInputHoldTime[gStr] > 0f)
            {
                controllerType = ControllerType.Gamepad;
            }
        }
        foreach (string verb in delayedChangeVerbs)
        {
            string kStr = "Keyboard " + verb;
            string gStr = "Gamepad " + verb;

            if (directInputHoldTime[kStr] > 0.125f)
            {
                controllerType = ControllerType.Keyboard;
            }

            if (directInputHoldTime[gStr] > 0.125f)
            {
                controllerType = ControllerType.Gamepad;
            }
        }

        controllerTypeStr = "Keyboard";
        controllerNameStr = "Mouse and Keyboard";
        if (controllerType == ControllerType.Gamepad)
        {
            controllerTypeStr = "Gamepad";
            controllerNameStr = Input.GetJoystickNames()[0];
        }

        if (controllerType != prevControllerType)
            UIManager.inputDeviceFadeCounter = 0f;


        // Get the values
        inputHoldTime["Any"] = -99f;
        inputVals["Any"] = -99f;
        inputPress["Any"] = false;
        inputRelease["Any"] = false;

        foreach (string verb in inputVerbs)
        {
            string ctStr = controllerTypeStr + " " + verb;

            // Button/axis degrees
            inputVals[verb] = Input.GetAxis(ctStr);
            if (inputVals[verb] != 0)
            {
                inputVals["Any"] = inputVals[verb];
            }

            // Hold time
            inputHoldTime[verb] = directInputHoldTime[controllerTypeStr + " " + verb];
            inputHoldTime["Any"] = Mathf.Max(inputHoldTime["Any"], inputHoldTime[verb]);

            // Press and release
            inputPress[verb] = Input.GetButtonDown(ctStr);
            inputPress["Any"] = inputPress["Any"] || inputPress[verb];
            inputRelease[verb] = Input.GetButtonUp(ctStr);
            inputRelease["Any"] = inputRelease["Any"] || inputRelease[verb];
        }

        // Additional handling for gamepad D-pad
        bool menusUseDPad = false;
        if (controllerType == ControllerType.Gamepad)
        {
            if (Input.GetAxis("Gamepad DPad X") != 0f && Input.GetAxis("Gamepad Walk X") == 0f)
            {
                inputVals["Walk X"] = Input.GetAxis("Gamepad DPad X");
                inputPress["Walk X"] = Input.GetButtonDown("Gamepad DPad X");
                inputRelease["Walk X"] = Input.GetButtonUp("Gamepad DPad X");
                menusUseDPad = true;
            }
            if (Input.GetAxis("Gamepad DPad Y") != 0f && Input.GetAxis("Gamepad Walk Y") == 0f)
            {
                inputVals["Walk Y"] = Input.GetAxis("Gamepad DPad Y");
                inputPress["Walk Y"] = Input.GetButtonDown("Gamepad DPad Y");
                inputRelease["Walk Y"] = Input.GetButtonUp("Gamepad DPad Y");
                menusUseDPad = true;
            }
        }


        // Update the input module for the UI accordingly
        if (UIManager.instance.inputModule != null)
        {
            UIManager.instance.inputModule.horizontalAxis = controllerTypeStr + " Walk X";
            UIManager.instance.inputModule.verticalAxis = controllerTypeStr + " Walk Y";
            UIManager.instance.inputModule.cancelButton = controllerTypeStr + " Run";
            UIManager.instance.inputModule.submitButton = controllerTypeStr + " Jump";

            if (menusUseDPad)
            {
                UIManager.instance.inputModule.horizontalAxis = "Gamepad DPad X";
                UIManager.instance.inputModule.verticalAxis = "Gamepad DPad Y";
            }
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

    public static void YouDidIt()
    {
        GameObject.Instantiate(instance.youDidItPrefab, player.transform.position, Quaternion.identity);
    }
    #endregion

    #region coroutines
    #endregion
}