using UnityEngine;
using System.Collections;
using System.Collections.Generic;       //Allows us to use Lists.
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager instance = null;

    public static bool dynamicCamera = false;
    public static float cameraSpeedX = 4f;
    public static float cameraSpeedY = 4f;
    public static bool cameraInvertedX = false;
    public static bool cameraInvertedY = false;
    public static float cameraShakeStrength = 1f;

    public static float soundVolume = 0.5f;
    public static float musicVolume = 0.5f;
    public static bool showMusicCredits = true;

    public static bool autosave = true;


    public Slider CamXSlider;
    public Slider CamYSlider;
    public Slider CamShakeSlider;

    public Slider SoundSlider;
    public Slider MusicSlider;

    public Toggle DynamicCamToggle;
    public Toggle CamXToggle;
    public Toggle CamYToggle;
    public Toggle AutosaveToggle;


    void Awake ()
    {
        if (instance == null)
            instance = this;
    }

    void Update ()
    {
        cameraSpeedX = CamXSlider.value;
        cameraSpeedY = CamYSlider.value;
        cameraShakeStrength = CamShakeSlider.value;

        dynamicCamera = DynamicCamToggle.isOn;
        cameraInvertedX = CamXToggle.isOn;
        cameraInvertedY = CamYToggle.isOn;
        autosave = AutosaveToggle.isOn;

        soundVolume = SoundSlider.value;
        musicVolume = MusicSlider.value;
    }
}
