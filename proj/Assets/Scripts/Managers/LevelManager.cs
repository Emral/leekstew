using System.Collections;
using System.Collections.Generic;
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
    public Texture2D thumbnail;
    public AudioClip music;
}


public class LevelManager : MonoBehaviour
{
    public static LevelManager instance = null;

    public static LevelData currentLevel;

    private bool beginningLevel = true;

    public static int currentCheckpoint = -1;
    public static List<int> checkpointsActive = new List<int>();


    [SerializeField] public List<LevelData> levels;
    private Dictionary<string, LevelData> levelDict;

    #region monobehavior events
        private void Awake()
        {
            if (instance == null)
                instance = this;

            levelDict = new Dictionary<string, LevelData>();
            foreach (LevelData level in levels)
            {
                levelDict.Add(level.key, level);
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
    public void LoadScene(string scene, bool resetCheckpoints=true)
        {
            if (resetCheckpoints)
                currentCheckpoint = -1;

            UIManager.instance.StopAllCoroutines();
            GameManager.instance.StopAllCoroutines();

            if (levelDict[scene] != null)
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
        public void EnterLevel(string level)
        {
            beginningLevel = true;
            LoadScene(level, true);
        }
    #endregion

    #region coroutines
    public IEnumerator LevelLoadSequence()
    {
        // Wait until the player reference is valid
        while (GameManager.player == null)
        {
            yield return null;
        }

        // Reset the player's health
        GameManager.player.UpdateReferences();
        GameManager.player.health.currentHp = 2;
        GameManager.player.inputActive = false;

        // Set the camera's target to the player if a preset target doesn't exist
        if (GameManager.camera.target == null)
            GameManager.camera.target = GameManager.player.transform;

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

        // If the current level is null, initialize it from the name of the scene
        if (currentLevel == null)
        {
            currentLevel = levelDict[SceneManager.GetActiveScene().name];
        }


        // Level intro stuff
        // Intro text
        UIManager.UpdateRefs();
        if (beginningLevel)
        {
            if (currentLevel != null && UIManager.levelIntroObj != null)
            {
                Text introName = GameObject.Find("LevelIntroName").GetComponentInChildren<Text>();
                Text introCreator = GameObject.Find("LevelIntroCreator").GetComponentInChildren<Text>();
                RawImage introThumbnail = GameObject.Find("LevelIntroThumbnail").GetComponentInChildren<RawImage>();
                introName.text = currentLevel.name;
                introCreator.text = currentLevel.creator;
                introThumbnail.texture = currentLevel.thumbnail;

                yield return new WaitForSeconds(1f);
                UIManager.DoFadeCanvasGroup(UIManager.levelIntroGroup, 0f, 1f);
            }
            yield return new WaitForSeconds(0.5f);
        }
        else if (UIManager.levelIntroObj != null)
        {
            UIManager.levelIntroObj.SetActive(false);
        }


        // Start the level music
        if (currentLevel != null)
            AudioManager.SetMusic(currentLevel.music);


        // Fade the screen in
        UIManager ui = UIManager.instance;
        ui.StartCoroutine(ui.ScreenFadeChange(0f, 1f));
        yield return new WaitForSeconds(0.5f);

        GameManager.player.inputActive = true;
        beginningLevel = false;
    }
    #endregion
}
