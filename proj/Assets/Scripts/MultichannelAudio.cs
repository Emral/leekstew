using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct SoundArrayDictEntry
{
    public string key;
    public AudioClip[] clips;
}


public class MultichannelAudio : MonoBehaviour
{
    [SerializeField]public SoundArrayDictEntry[] sounds;
    private Dictionary<string, AudioClip[]> soundsDict;

    public int channelCount = 3;
    private List<AudioSource> sourcesAvailable;
    private List<AudioSource> sourcesUsed;
    private int lastUsed;

    public int ChannelsUsed
    {
        get
        {
            return sourcesUsed.Count;
        }
    }
    public int ChannelsAvailable
    {
        get
        {
            return sourcesAvailable.Count;
        }
    }


    // Use this for initialization
    void Start ()
    {
        soundsDict = new Dictionary<string, AudioClip[]>();
        foreach(SoundArrayDictEntry entry in sounds)
        {
            soundsDict.Add(entry.key, entry.clips);
        }

        sourcesAvailable = new List<AudioSource>();
        sourcesUsed = new List<AudioSource>();
        for (int i = 0; i < channelCount; i++)
        {
            sourcesAvailable.Add(gameObject.AddComponent<AudioSource>());
        }
	}


    public void Update()
    {
        // Move used audio sources that have finished playing back into the available list
        for (int i = sourcesUsed.Count-1; i >= 0; i++)
        {
            if (!sourcesUsed[i].isPlaying)
            {
                sourcesAvailable.Add(sourcesUsed[i]);
                sourcesUsed.RemoveAt(i);
            }
        }
    }


    // Play a sound effect
    public AudioSource Play(AudioClip clip, bool loop = false, float volume = 1f, float pitch = 1f, bool overwrite = true)
    {
        AudioSource source = null;
        if (ChannelsAvailable > 0)
        {
            source = sourcesAvailable[0];
            
            // Move it from the available list to the used list
            sourcesAvailable.Remove(source);
            sourcesUsed.Add(source);
        }
        else if (overwrite)
        {
            source = sourcesUsed[0];
            source.Stop();
            
            // Move to the end of the used list
            sourcesUsed.Remove(source);
            sourcesUsed.Add(source);
        }

        if (source != null)
        {
            // Apply the properties
            source.clip = clip;
            source.loop = loop;
            source.pitch = pitch;
            source.volume = volume * OptionsManager.soundVolume;
            source.Play();
        }

        return source;
    }
    public AudioSource Play(string group, bool loop = false, float volume = 1f, float pitch = 1f, bool overwrite = true)
    {
        AudioClip[] groupArray = soundsDict[group];
        if (groupArray != null)
        {
            AudioClip clip = groupArray[Random.Range(0, groupArray.Length)];

            return Play(clip, loop, volume, pitch, overwrite);
        }
        return null;
    }
}
