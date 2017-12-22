using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnBatteriesActive : MonoBehaviour {

    public GameObject batteryParent;
    public GameObject target;
    public GameObject goalEffect;

    private Battery[] batteries;
    private bool done = false;

	// Use this for initialization
	void Start () {
        batteries = batteryParent.GetComponentsInChildren<Battery>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!done)
        {
            bool allActive = true;
            foreach (Battery b in batteries)
            {
                if (!b.GetActive())
                {
                    allActive = false;
                    break;
                }
            }
            if (allActive)
            {
                done = true;
                target.SetActive(true);
                if (goalEffect != null)
                    GameObject.Instantiate(goalEffect, GameManager.player.transform.position, Quaternion.identity);
            }
        }
	}
}
