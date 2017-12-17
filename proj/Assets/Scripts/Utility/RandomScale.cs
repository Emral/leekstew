using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomScale : MonoBehaviour
{
    public Vector3 minScale;
    public Vector3 maxScale;
    public float minSquash;
    public float maxSquash;

	// Use this for initialization
	void Start ()
    {
        transform.localScale = new Vector3(Random.Range(minScale.x,maxScale.x), Random.Range(minScale.y, maxScale.y), Random.Range(minScale.z, maxScale.z));
        SquashAndStretch squash = GetComponent<SquashAndStretch>();
        if (squash != null)
        {
            squash.effectAmount = Random.Range(minSquash,maxSquash);
        }
	}	
}
