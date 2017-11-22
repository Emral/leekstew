using UnityEngine;
using System.Collections;
using System.Collections.Generic;       //Allows us to use Lists.

public class OptionsManager : MonoBehaviour
{
    public static float cameraSpeedX = 4f;
    public static float cameraSpeedY = 4f;
    public static bool cameraInvertedX = false;
    public static bool cameraInvertedY = false;
    public static float cameraShakeStrength = 1f;

    public static float soundVolume = 0.5f;
    public static float musicVolume = 0.5f;
    public static bool showMusicCredits = true;


    void Start ()
    {
		
	}

    void ConnectSlider(string objName, ref float optionVar)
    {
        GameObject sliderObj = GameObject.Find(objName);
        if (sliderObj != null)
        {
            UnityEngine.UI.Slider sliderScr = sliderObj.GetComponent("Slider") as UnityEngine.UI.Slider;
            optionVar = sliderScr.value;
        }
    }
    void ConnectToggle(string objName, ref bool optionVar)
    {
        GameObject toggleObj = GameObject.Find(objName);
        if (toggleObj != null)
        {
            UnityEngine.UI.Toggle toggleScr = toggleObj.GetComponent("Toggle") as UnityEngine.UI.Toggle;
            optionVar = toggleScr.isOn;
        }
    }

    void Update ()
    {
        ConnectSlider("UI_CamXSlider", ref cameraSpeedX);
        ConnectSlider("UI_CamYSlider", ref cameraSpeedY);
        ConnectSlider("UI_CamShakeSlider", ref cameraShakeStrength);

        ConnectToggle("UI_CamXToggle", ref cameraInvertedX);
        ConnectToggle("UI_CamYToggle", ref cameraInvertedY);

        ConnectSlider("UI_SoundVolumeSlider", ref soundVolume);
        ConnectSlider("UI_MusicVolumeSlider", ref musicVolume);
    }
}
