using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMaterial : MonoBehaviour {

    public Material newMat;
    private Material oldMat;
    private MeshRenderer meshRenderer;

    private Battery b;

    private bool wasChanged = false;

	// Use this for initialization
	void Start () {
        b = GetComponent<Battery>();
        meshRenderer = GetComponent<MeshRenderer>();

        oldMat = meshRenderer.material;
	}
	
	// Update is called once per frame
	void Update () {
		if (!wasChanged && b != null)
        {
            if (b.GetActive())
            {
                Change();
            }
        }
	}

    public void Change()
    {
        meshRenderer.material = newMat;
    }

    public void Reset()
    {
        wasChanged = false;
        meshRenderer.material = oldMat;
    }
}
