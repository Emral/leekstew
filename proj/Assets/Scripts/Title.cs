using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Title : MonoBehaviour
{
    public GameObject pressAnyObj;
    public GameObject buttonsObj;
    public GameObject newGameObj;
    public GameObject continueObj;
    

    private bool started = false;

	// Use this for initialization
	void Start ()
    {
        UIManager.DoScreenFadeChange(0f, 1f);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (GameManager.inputPress["Any"] && !started)
        {
            started = true;
            AudioManager.PlaySound("game start");

            if (SaveManager.currentSave.gameStarted)
            {
                pressAnyObj.SetActive(false);
                buttonsObj.SetActive(true);
                UIManager.instance.eventSystem.SetSelectedGameObject(continueObj);
            }
            else
            {
                DoStartGame();
            }
        }
	}

    public void DoStartGame()
    {
        StartCoroutine(StartGame());
    }
    public void DoNewGame()
    {
        SaveManager.NewGame();
        DoStartGame();
    }

    public IEnumerator StartGame()
    {
        AudioManager.PlaySound("game start");
        AudioManager.StopMusic();

        UIManager ui = UIManager.instance;
        yield return ui.StartCoroutine(ui.ScreenFadeChange(1f, 1f));
        yield return new WaitForSeconds(1f);

        LevelManager.EnterLevel("Scene_Hub");
    }
}
