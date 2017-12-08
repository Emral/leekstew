using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SongData
{
    public AudioClip key;
    public string name;
    public string artist;
    public string album;
    public int loopStart;
    public int loopEnd;
}


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance = null;

    public UnityEngine.Audio.AudioMixer mixer;
    [SerializeField] [ReorderableList] public List<SongData> songs;
    private Dictionary<AudioClip, SongData> songDict;

    public static AudioSource source;

    public static AudioClip currentMusic;
    public static SongData currentSong;
    public static bool usingLoopPoints = false;

    public static float musicFadeAmount = 1f;


    // Use this for initialization
    private void Awake ()
    {
        if (instance == null)
            instance = this;

        songDict = new Dictionary<AudioClip, SongData>();
        foreach(SongData song in songs)
        {
            songDict.Add(song.key, song);
        }
    }

    // Update is called once per frame
    void Update()
    {
        mixer.SetFloat("musicVolume", (1 - Mathf.Pow(1 - OptionsManager.musicVolume, 2)) * 70f - 80f);
        mixer.SetFloat("soundVolume", (1 - Mathf.Pow(1 - OptionsManager.soundVolume, 2)) * 80f - 80f);

        currentMusic = null;
        source = transform.GetComponent<AudioSource>();
        if (source.isPlaying)
        {
            currentMusic = source.clip;
            currentSong = songDict[currentMusic];
        }
    }


    public static void StopMusic()
    {
        source.Stop();
    }

    public static void PauseMusic()
    {
        source.Pause();
    }

    public static void ResumeMusic()
    {
        source.UnPause();
    }

    public static void FadeOutMusic(float seconds, bool stop)
    {
        if (stop)
            instance.StartCoroutine(instance.MusicFadeChange(0f, seconds));
        else
            instance.StartCoroutine(instance.MusicFadeToStop(seconds));
    }

    public static void SetMusic(AudioClip music, bool loop=true, bool useLoopPoints=false)
    {
        if (source.clip != music || source.loop != loop || usingLoopPoints != useLoopPoints)
        {
            UIManager.hpFadeCounter = 0f;

            StopMusic();

            source.loop = loop;
            source.clip = music;
            source.Play();
            currentMusic = music;

            if (useLoopPoints)
                instance.StartCoroutine(instance.LoopMusic(music));
        }
    }


    public IEnumerator MusicFadeToStop(float goalTime)
    {
        yield return instance.StartCoroutine(instance.MusicFadeChange(0f, goalTime));
        StopMusic();
    }

    public IEnumerator MusicFadeChange(float goal, float goalTime, float delay = 0f)
    {
        if (delay != 0f)
            yield return new WaitForSeconds(delay);

        float startAmount = musicFadeAmount;
        float currentTime = 0;
        while (currentTime < goalTime)
        {
            musicFadeAmount = Mathf.Lerp(startAmount, goal, currentTime / goalTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
        musicFadeAmount = goal;
    }


    private IEnumerator LoopMusic(AudioClip audio)
    {
        SongData thisSongData = songDict[audio];
        int loopStart = 0;
        int loopEnd = audio.samples;

        if (thisSongData != null)
        {
            loopStart = thisSongData.loopStart > 0 ? thisSongData.loopStart : loopStart;
            loopEnd = thisSongData.loopEnd > 0 ? thisSongData.loopEnd : loopEnd;
        }

        while (source.isPlaying)
        {
            if (source.timeSamples >= loopEnd && loopEnd < audio.samples)
                source.timeSamples = loopStart;
            yield return null;
        }
    }
    //*/
}
