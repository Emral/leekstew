using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


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
    public static float musicPitch = 1f;

    public UnityEngine.Audio.AudioMixer mixer;
    [SerializeField] [ReorderableList] public List<SongData> songs;
    private Dictionary<AudioClip, SongData> songDict;

    public AudioClip[] deniedSounds;
    public AudioClip popSound;
    public AudioClip quakeSound;

    public AudioSource musicSource;
    public MultichannelAudio multiSource;

    public AudioMixerGroup SfxMixGroup;

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
        mixer.SetFloat("musicVolume", (1 - Mathf.Pow(1 - OptionsManager.musicVolume, 2)) * 80f - 80f);
        mixer.SetFloat("soundVolume", (1 - Mathf.Pow(1 - OptionsManager.soundVolume, 2)) * 80f - 80f);

        currentMusic = null;
        instance.musicSource = transform.GetComponent<AudioSource>();
        if (instance.musicSource.isPlaying)
        {
            currentMusic = instance.musicSource.clip;
            currentSong = songDict[currentMusic];
            instance.musicSource.volume = musicFadeAmount;

            if (!GameManager.isGamePaused)
                instance.musicSource.pitch = Time.timeScale * musicPitch;
        }
    }


    public static AudioSource PlaySound(AudioClip clip, bool loop = false, float volume = 1f, float pitch = 1f)
    {
        return instance.multiSource.Play(clip, loop, volume, pitch);
    }
    public static AudioSource PlaySound(string group, bool loop = false, float volume = 1f, float pitch = 1f)
    {
        return instance.multiSource.Play(group, loop, volume, pitch);
    }

    public static void StopMusic()
    {
        instance.musicSource.Stop();
    }

    public static void PauseMusic()
    {
        instance.musicSource.Pause();
    }

    public static void ResumeMusic()
    {
        instance.musicSource.UnPause();
    }

    public static void FadeOutMusic(float seconds, bool stop)
    {
        if (stop)
            instance.StartCoroutine(instance.MusicFadeChange(0f, seconds));
        else
            instance.StartCoroutine(instance.MusicFadeToStop(seconds));
    }

    public static void SetMusic(AudioClip music, bool loop = true, bool useLoopPoints = false)
    {
        if (instance.musicSource.clip != music || instance.musicSource.loop != loop || usingLoopPoints != useLoopPoints || !instance.musicSource.isPlaying)
        {
            UIManager.musicFadeCounter = 0f;

            StopMusic();

            instance.musicSource.loop = loop;
            instance.musicSource.clip = music;
            instance.musicSource.Play();
            currentMusic = music;
            musicFadeAmount = 1f;

            if (useLoopPoints)
                instance.StartCoroutine(instance.LoopMusic(music));
        }
    }

    public static void SetMusic(int track, bool loop = true, bool useLoopPoints = false)
    {
        if (track >= 0 && track < instance.songs.Count)
            SetMusic(instance.songs[track].key, loop, useLoopPoints);
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
            currentTime += Time.unscaledDeltaTime;
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

        while (instance.musicSource.isPlaying)
        {
            if (instance.musicSource.timeSamples >= loopEnd && loopEnd < audio.samples)
                instance.musicSource.timeSamples = loopStart;
            yield return null;
        }
    }
    //*/
}
