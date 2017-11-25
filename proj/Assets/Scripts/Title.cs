using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Title : MonoBehaviour {

    private bool started = false;

	// Use this for initialization
	void Start ()
    {
        UIManager.instance.DoScreenFadeChange(0f, 1f);
    }
	
	// Update is called once per frame
	void Update ()
    {
		if (GameManager.inputPress["Any"] && !started)
        {
            started = true;
            StartCoroutine(StartGame());
        }
	}

    public IEnumerator StartGame()
    {
        GetComponent<AudioSource>().Play();
        AudioManager.instance.StopMusic();

        UIManager ui = UIManager.instance;
        yield return ui.StartCoroutine(ui.ScreenFadeChange(1f, 1f));
        yield return new WaitForSeconds(1f);

        LevelManager.instance.EnterLevel("Scene_Test1");
    }
}
