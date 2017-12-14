using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public int roomId = 0;
    public bool changeCamera;
    public CameraBehavior newCamera;
    public AudioClip music;
    public string roomName;
    public Material skybox;


	// Use this for initialization
	void Start ()
    {
    }

    // Update is called once per frame
    public void EnableRoom ()
    {
        CameraManager.skybox.material = skybox;
        AudioManager.SetMusic(music);
    }
}
