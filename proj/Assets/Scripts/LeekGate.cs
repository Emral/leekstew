using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeekGate : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    IEnumerator OpenSequence()
    {
        GameManager.cutsceneMode = true;
        //CameraManager.ChangeShot();
        yield return null;
        GameManager.cutsceneMode = false;
    }
}
