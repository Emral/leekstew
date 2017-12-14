using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToothAnimation : MonoBehaviour
{
    private float hoverOffset;
    public Transform hover;
    public Transform toothModel;

    // Use this for initialization
    void Start ()
    {
        hoverOffset = transform.position.x + transform.position.y + transform.position.z;
	}

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.isGamePaused)
        {

            toothModel.Rotate(0, -2, 0);

            
            hover.transform.localPosition = new Vector3(0f, 0.5f + 0.125f * Mathf.Cos(2.5f * Time.timeSinceLevelLoad + hoverOffset), 0f);
        }
    }
}
