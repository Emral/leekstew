﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstanceCounter : MonoBehaviour
{
    public int offset;
    public string tag = "Green Tooth";
    public string before;
    public string after;
    public bool blankOnZero = true;
    private Text text;

	// Use this for initialization
	void Start ()
    {
        text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        int numLeft = GameObject.FindGameObjectsWithTag(tag).Length;

        text.text = "";
        if (numLeft > 0 || !blankOnZero)
            text.text = before + (numLeft + offset).ToString() + after;

    }
}
