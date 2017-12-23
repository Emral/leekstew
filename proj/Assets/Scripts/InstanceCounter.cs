using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstanceCounter : MonoBehaviour
{
    public int offset;
    public string tag = "Green Tooth";
    public string before;
    public string after;
    private Text text;

	// Use this for initialization
	void Start ()
    {
        text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        text.text = before + (GameObject.FindGameObjectsWithTag(tag).Length + offset).ToString() + after;

    }
}
