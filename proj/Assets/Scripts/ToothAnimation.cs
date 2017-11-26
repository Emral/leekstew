using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToothAnimation : MonoBehaviour
{
    public string modelName;
    private float hoverOffset;
    private Transform tooth;
    private Transform hover;

    // Use this for initialization
    void Start ()
    {
        hoverOffset = transform.position.x + transform.position.y + transform.position.z;
	}

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale != 0f)
        {
            hover = transform.Find("Hover");
            tooth = hover.Find(modelName);

            if (tooth != null)
                tooth.Rotate(0, -2, 0);

            if (hover != null)
                hover.transform.localPosition = new Vector3(0f, 0.5f + 0.125f * Mathf.Cos(2.5f * Time.timeSinceLevelLoad + hoverOffset), 0f);
        }
    }
}
