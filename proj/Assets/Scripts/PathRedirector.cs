using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRedirector : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != 12) return;


    }
}
