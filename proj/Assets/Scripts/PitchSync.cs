using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitchSync : MonoBehaviour
{
    public float defaultPitch; 

	void Update ()
    {
		foreach (AudioSource source in GetComponents<AudioSource>())
        {
            source.pitch = defaultPitch*Time.timeScale;
        }
	}
}
