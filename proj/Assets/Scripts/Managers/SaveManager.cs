using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
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
    public bool goldRadishCollected;

    public LevelSaveData()
    {
        leeksCollected = new List<int>();
        greenTeethCollected = new List<int>();
        checkpointsActivated = new List<int>();
        warpPadsActivated = new List<int>();
        collectGatesOpened = new List<int>();
        goldRadishCollected = false;
    }

    public void ClearDuplicates()
    {
        leeksCollected = leeksCollected.Distinct().ToList();
        greenTeethCollected = greenTeethCollected.Distinct().ToList();
        checkpointsActivated = checkpointsActivated.Distinct().ToList();
        warpPadsActivated = warpPadsActivated.Distinct().ToList();
        collectGatesOpened = collectGatesOpened.Distinct().ToList();
    }

    public override string ToString()
    {
        string _fullStr = "Level: " + level;

        _fullStr += "\n   Leeks: ";
        foreach (int item in leeksCollected)
        {
            _fullStr += item.ToString() + ", ";
        }

        _fullStr += "\n   Checkpoints: ";
        foreach (int item in checkpointsActivated)
        {
            _fullStr += item.ToString() + ", ";
        }

        _fullStr += "\n   Warp Pads: ";
        foreach (int item in warpPadsActivated)
        {
            _fullStr += item.ToString() + ", ";
        }

        _fullStr += "\n   Collection Gates: ";
        foreach (int item in collectGatesOpened)
        {
            _fullStr += item.ToString() + ", ";
        }

        _fullStr += "\n   Gold Radish: " + goldRadishCollected.ToString();

        return _fullStr;
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
            if (levelData.isIncluded)
            {
                LevelSaveData lvl = new LevelSaveData();
                lvl.level = levelData.name;
                allLevelSaves.Add(lvl);
            }
        }
    }

    public void ClearDuplicates()
    {
        foreach (LevelSaveData lvl in allLevelSaves)
        {
            lvl.ClearDuplicates();
        }
    }

    public override string ToString()
    {
        //int tGRad = TotalGoldRadishes;
        //int tLeek = TotalLeeks;
        string _fullStr = "SAVE DATA:\nCurrent hub ID: " + currentHubIndex.ToString()
                       + "\nCurrent hub: " + currentHubIndex.ToString() + " (" + currentHubScene + ")"
                       + "\nCurrent level: " + currentLevelIndex.ToString() + " (" + currentLevelScene + ")"
                       + "\nTeeth: " + teethCollected.ToString() + " collected, " + teethLost.ToString() + " lost, " + teethSpent.ToString() + " spent, " + NetTeeth.ToString() + " net"
                       + "\nTotal gold radishes: " + TotalGoldRadishes.ToString()
                       + "\nTotal leeks: " + TotalLeeks.ToString();

        foreach(LevelSaveData lvl in allLevelSaves)
        {
            _fullStr += "\n\n" + lvl.ToString();
        }

        return _fullStr;
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

    public void OnGUI()
    {
        if (Debug.isDebugBuild && GameManager.instance.debugMode)
        {
            //* 
            string saveStr = currentSave.ToString();
            /*
            foreach (KeyValuePair<string, float> pair in directInputHoldTime)
            {
                inputsStr += pair.Key + ": " + pair.Value.ToString() + "\n";
            }
            */
            Rect box = new Rect(-10f, -10f, 256f+20f, Screen.height+20f);
            Rect boxB = new Rect(0f, 0f, 256f, Screen.height);
            GUI.Box(box, new GUIContent(""), GUI.skin.window);
            GUI.Box(box, new GUIContent(""), GUI.skin.window);
            GUI.Box(box, new GUIContent(""), GUI.skin.window);
            GUI.Label(boxB, saveStr);
            //*/
        }
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
        currentSave.ClearDuplicates();
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
