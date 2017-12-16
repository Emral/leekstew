using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[System.Serializable]
public class LevelData
{
    public string key;
    public Scene scene;
    public string name;
    public string creator;
    public bool isHub;
    public bool isIncluded = true;
    public Texture2D thumbnail;
    public AudioClip music;
    [HideInInspector] public int index;
}


public class LevelManager : MonoBehaviour
{
    public static LevelManager instance = null;
    public static LevelData currentLevel;

    private static bool beginningLevel = true;
    public static bool isWarping = false;
    public static bool dontFadeBackIn = false;

    public static Vector3 warpDestination;
    public static int currentRoom = 0;
    public static int checkpointRoom = 0;

    public static int currentCheckpoint = -1;
    public static List<int> checkpointsActive = new List<int>();
    public static List<int> warpsActive = new List<int>();

    public static Dictionary<int, GameObject> roomObjects;
    public static Dictionary<int, string> roomNames;
    public static Dictionary<int, AudioClip> roomMusic;

    public Text levelIntroName;
    public Text levelIntroCreator;
    public RawImage introThumbnail;

    public static GameObject CurrentRoomObject
    {
        get
        {
            if (roomObjects != null)
            {
                if (roomObjects.ContainsKey(currentRoom))
                {
                    return roomObjects[currentRoom];
                }
            }
            return null;
        }
    }

    public static Room CurrentRoomScript
    {
        get
        {
            GameObject _currRoom = CurrentRoomObject;
            if (_currRoom != null)
            {
                return _currRoom.GetComponent<Room>();
            }
            return null;
        }
    }

    // This feature doesn't work yet 
    [HideInInspector] public bool checkToAddScenesToBuildList = false;

    [SerializeField] [ReorderableList] public List<LevelData> levels;
    private static Dictionary<string, LevelData> levelDict;


    #region monobehavior events
    private void Awake()
    {
        if (instance == null)
            instance = this;

        levelDict = new Dictionary<string, LevelData>();
        for (int i = 0; i < levels.Count; i++)
        {
            LevelData level = levels[i];
            level.index = i;
            levelDict.Add(level.key, level);
        }
    }

    public void OnValidate()
    {
        // If the player checked the thing, load all of the levels' scenes into the build scene list
        if (checkToAddScenesToBuildList)
        {
            checkToAddScenesToBuildList = false;

            var original = EditorBuildSettings.scenes;
            var newSettings = new EditorBuildSettingsScene[levels.Count];

            // Add the title screen
            newSettings[0] = original[0];

            int i = 0;
            foreach (LevelData level in levels)
            {
                if (Application.CanStreamedLevelBeLoaded(level.key))
                {
                    var sceneToAdd = new EditorBuildSettingsScene("Assets/Scenes/" + level.key + ".unity", true);
                    newSettings[i++] = sceneToAdd;
                }
            }

            EditorBuildSettings.scenes = newSettings;
        }
    }
    #endregion

    #region initialization
    public static void InitLevel()
        {
        if (instance == null)
            instance = GameManager.instance.GetComponent<LevelManager>();

            instance.StartCoroutine(instance.LevelLoadSequence());
        }
    #endregion

    #region methods
    public static LevelData GetLevelInfo(string sceneName)
    {
        if (levelDict.ContainsKey(sceneName))
            return levelDict[sceneName];
        return null;
    }
    public void LoadScene(string scene, bool resetCheckpoints=true)
    {
        if (resetCheckpoints)
        {
            currentCheckpoint = -1;
            checkpointRoom = 0;
        }

        UIManager.instance.StopAllCoroutines();
        GameManager.instance.StopAllCoroutines();

        if (levelDict.ContainsKey(scene))
        {
            currentLevel = levelDict[scene];
        }

        SceneManager.LoadScene(scene);
        GameManager.instance.Awake();
    }
    public void LoadScene(string scene)
    {
        LoadScene(scene, true);
    }
    public void ReloadScene()
    {
        LoadScene(SceneManager.GetActiveScene().name, false);
    }
    public void RestartLevel()
    {
        LoadScene(SceneManager.GetActiveScene().name, true);
    }
    public static void EnterLevel(string level)
    {
        beginningLevel = true;
        instance.LoadScene(level, true);
    }
    public static void CatalogRooms()
    {
        roomObjects = new Dictionary<int, GameObject>();
        roomNames = new Dictionary<int, string>();
        roomMusic = new Dictionary<int, AudioClip>();

        GameObject[] roomObjectArray = GameObject.FindGameObjectsWithTag("Room");

        foreach (GameObject go in roomObjectArray)
        {
            Room scr = go.GetComponent<Room>();
            print("Catalogued room #" + scr.roomId + ", " + scr.roomName);
            roomObjects[scr.roomId] = go;
            roomNames[scr.roomId] = scr.roomName;
            roomMusic[scr.roomId] = scr.music;
        }
    }
    public static void ShowAndHideRooms()
    {
        foreach (GameObject room in roomObjects.Values)
        {
            Room scr = room.GetComponent<Room>();
            room.SetActive(currentRoom == scr.roomId);
        }
    }
    public static void ChangeRoom(int newRoom)
    {
        currentRoom = newRoom;
        ShowAndHideRooms();
        print("Changed to room " + CurrentRoomScript.roomName);
    }
    #endregion

    #region coroutines
    public IEnumerator LevelLoadSequence()
    {
        // Wait until the player reference is valid
        while (GameManager.player == null || CameraManager.instance == null)
        {
            yield return null;
        }

        // Reset the player's health
        GameManager.player.UpdateReferences();
        GameManager.player.health.currentHp = 2;
        GameManager.player.inputActive = false;

        // Set the camera's target to the player if a preset target doesn't exist
        if (CameraManager.instance.target == null)
            CameraManager.instance.target = GameManager.player.transform;

        // Place the player at the warp destination
        if (isWarping)
        {
            isWarping = false;
            GameManager.player.transform.position = warpDestination;
        }
        else
        {
            currentRoom = checkpointRoom;
        }

        // Checkpoint management
        GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        foreach (GameObject checkpoint in checkpoints)
        {
            Checkpoint scr = checkpoint.GetComponent<Checkpoint>();

            if (checkpointsActive.Contains(scr.checkpointID))
            {
                scr.SetActive();
            }

            if (scr.checkpointID == currentCheckpoint)
            {
                scr.SetCurrent();
                GameManager.player.transform.position = checkpoint.transform.position + Vector3.up * 0.5f;
            }
        }

        // Room management
        CatalogRooms();
        ShowAndHideRooms();

        // If the current level is null, initialize it from the name of the scene
        if (currentLevel == null)
        {
            currentLevel = levelDict[SceneManager.GetActiveScene().name];
        }


        // Level intro stuff
        // Intro text
        if (beginningLevel)
        {
            UIManager.DoFadeCanvasGroup(UIManager.instance.levelIntroGroup, 1f, 0f);
            if (currentLevel != null && UIManager.instance != null)
            {
                levelIntroName.text = currentLevel.name;
                levelIntroCreator.text = currentLevel.creator;
                introThumbnail.texture = currentLevel.thumbnail;

                yield return new WaitForSeconds(1f);
                UIManager.DoFadeCanvasGroup(UIManager.instance.levelIntroGroup, 0f, 1f);
            }
            yield return new WaitForSeconds(0.5f);
        }
        else if (UIManager.instance != null)
        {
            UIManager.instance.levelIntroGroup.gameObject.SetActive(false);
        }


        // Start the level music
        if (currentLevel != null)
            AudioManager.SetMusic(currentLevel.music);


        // Fade the screen in
        if (!dontFadeBackIn)
        {
            UIManager ui = UIManager.instance;
            ui.StartCoroutine(ui.ScreenFadeChange(0f, 1f));
            yield return new WaitForSeconds(0.5f);
        }
        dontFadeBackIn = false;

        GameManager.player.inputActive = true;
        beginningLevel = false;
    }
    #endregion
}
