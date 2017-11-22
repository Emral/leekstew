using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public UnityEngine.Audio.AudioMixer mixer;
    public static AudioClip currentMusic;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        mixer.SetFloat("musicVolume", (1-Mathf.Pow(1-OptionsManager.musicVolume, 2)) * 70f - 80f);
        mixer.SetFloat("soundVolume", (1-Mathf.Pow(1-OptionsManager.soundVolume, 2)) * 80f - 80f);
    }

    /*
    public void SetMusic(AudioClip music)
    {
        AudioSource aud = transform.GetComponent("AudioSource") as AudioSource;
        if (aud.clip != music)
        {
            aud.Stop();
            aud.clip = music;
            aud.Play();
            currentMusic = music;
            StartCoroutine(LoopMusic, music);
        }
    }

    private IEnumerator LoopMusic(AudioClip audio, int loopStart, int loopEnd)
    {
        AudioSource aud = transform.GetComponent("AudioSource") as AudioSource;
        while (aud.isPlaying)
        {
            if (aud.timeSamples >= loopEnd)
                aud.timeSamples = loopStart;
            yield return null;
        }
    }
    */
}
