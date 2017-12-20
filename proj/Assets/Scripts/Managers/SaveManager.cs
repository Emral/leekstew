using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

[System.Serializable]
public class LevelSaveData
{
    public string level;
    public List<int> leeksCollected;
    public List<int> greenTeethCollected;
    public List<int> checkpointsActivated;
    public List<int> warpPadsActivated;
    public List<int> collectGatesOpened;
    public bool goldRadishCollected = false;

    public LevelSaveData()
    {
        leeksCollected = new List<int>();
        greenTeethCollected = new List<int>();
        checkpointsActivated = new List<int>();
        warpPadsActivated = new List<int>();
        collectGatesOpened = new List<int>();
    }

    public override string ToString()
    {
        return "Level: " + level + ", Leeks: " + leeksCollected.ToString() + ", Green Teeth: " + greenTeethCollected.ToString() + ", Checkpoints: " + checkpointsActivated.ToString() + ", Warp Pads: " + warpPadsActivated.ToString() + ", Collection Gates: " + collectGatesOpened.ToString() + ", Gold Radish: " + goldRadishCollected.ToString();
    }
}

[System.Serializable]
public class GlobalSaveData
{
    public List<LevelSaveData> allLevelSaves;
    public int currentHubIndex;
    public string currentHubScene;
    public int currentLevelIndex;
    public string currentLevelScene;
    public int teethCollected;
    public int teethSpent;
    public int teethLost;
    public int greenTeethSpent;
    public bool gameStarted;

    public int TotalGoldRadishes
    {
        get
        {
            int _numGRads = 0;
            foreach (LevelSaveData levelData in allLevelSaves)
            {
                if (levelData.goldRadishCollected)
                    _numGRads++;
            }
            return _numGRads;
        }
    }
    public int TotalLeeks
    {
        get
        {
            int _numLeeks = 0;
            foreach (LevelSaveData levelData in allLevelSaves)
            {
                _numLeeks += levelData.leeksCollected.Count;
            }
            return _numLeeks;
        }
    }
    public int NetTeeth
    {
        get
        {
            return (int)Mathf.Max(0f, teethCollected - teethSpent - teethLost);
        }
    }
    public int TotalGreenTeeth
    {
        get
        {
            int _numGT = 0;
            foreach (LevelSaveData levelData in allLevelSaves)
            {
                _numGT += levelData.greenTeethCollected.Count;
            }
            return _numGT;
        }
    }
    public int NetGreenTeeth
    {
        get
        {
            return TotalGreenTeeth - greenTeethSpent;
        }
    }
    public int TotalGatesOpened
    {
        get
        {
            int _numCG = 0;
            foreach (LevelSaveData levelData in allLevelSaves)
            {
                _numCG += levelData.collectGatesOpened.Count;
            }
            return _numCG;
        }
    }

    public LevelSaveData CurrentLevelSave
    {
        get
        {
            if (allLevelSaves.Count > currentLevelIndex)
                return allLevelSaves[currentLevelIndex];
            else
                return null;
        }
    }

    public GlobalSaveData()
    {
        gameStarted = false;
        allLevelSaves = new List<LevelSaveData>();
        foreach (LevelData levelData in LevelManager.instance.levels)
        {
            allLevelSaves.Add(new LevelSaveData());
        }
    }

    public override string ToString()
    {
        return "Level Save Data: "+ allLevelSaves.ToString();
    }
}


public class SaveManager : MonoBehaviour
{
    public static GlobalSaveData currentSave;
    public static int saveSlot = 0;

    public static SaveManager instance;

    public static LevelSaveData CurrentLevelSave
    {
        get
        {
            return currentSave.CurrentLevelSave;
        }
    }

    public static string SavePath
    {
        get
        {
            return Application.persistentDataPath + "/savefile" + saveSlot.ToString() + ".sav";
        }
    }

    /**
     * Saves the save data to the disk
     */

    public void Start()
    {
        instance = this;
    }

    public void Update()
    {
        if (LevelManager.currentLevel != null)
        {
            currentSave.currentLevelIndex = LevelManager.currentLevel.index;
            currentSave.currentLevelScene = LevelManager.currentLevel.key;
            if (LevelManager.currentLevel.isHub)
            {
                currentSave.currentHubIndex = LevelManager.currentLevel.index;
                currentSave.currentHubScene = LevelManager.currentLevel.key;
            }
        }
        /*
        print("CURRENT GLOBAL SAVE DATA: " + currentSave.ToString());
        if (CurrentLevelSave != null)
            print("CURRENT LEVEL SAVE DATA: "  + CurrentLevelSave.ToString());
        */
    }

    public static void InitSave()
    {
        if (currentSave == null)
        {
            if (File.Exists(SavePath))
                LoadProgress();
            else
                NewGame();
        }
    }
    public static void NewGame()
    {
        currentSave = new GlobalSaveData();
    }

    public static void Autosave()
    {
        if (OptionsManager.autosave)
            DoSaveGame();
    }

    public static void DoSaveGame()
    {
        currentSave.gameStarted = true;
        instance.StartCoroutine(instance.AutosaveCoroutine());
    }

    public IEnumerator AutosaveCoroutine()
    {
        yield return UIManager.instance.StartCoroutine(UIManager.instance.ShowSaving());
        SaveProgress();
        yield return UIManager.instance.StartCoroutine(UIManager.instance.ShowSaveFinished());
    }

    public static void SaveProgress()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(SavePath);
        bf.Serialize(file, currentSave);
        file.Close();
    }

    public static GlobalSaveData LoadProgressFromDisk()
    {
        if (File.Exists(SavePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(SavePath, FileMode.Open);
            GlobalSaveData returnedData = (GlobalSaveData)bf.Deserialize(file);
            file.Close();

            // Debug
            /*
            print("FILE LOADED:");

            System.Type type = currentSave.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                print("Name: " + property.Name + ", Value: " + property.GetValue(currentSave, null));
            }
            */

            // Return
            return returnedData;
        }
        return null;
    }

    /**
     * Loads the save data from the disk
     */
    public static void LoadProgress()
    {
        currentSave = LoadProgressFromDisk();
    }
}
