using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraVolume : MonoBehaviour
{
    public CameraBehavior cameraProps;

    private bool cachedPlayerInside;
    private bool active = false;

    // Use this for initialization
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == GameManager.player.gameObject)
        {
            //CameraManager.instance.StopAllCoroutines();
            CameraManager.DoShiftToNewShot(cameraProps);
            //active = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == GameManager.player.gameObject && CameraManager.currentBehavior == cameraProps)
        {
            CameraManager.instance.StopAllCoroutines();
            CameraManager.DoShiftToNewShot(cameraProps);
            //active = false;
        }
    }

    // Update is called once per frame
    void Update ()
    {
        /*
        Vector3 playerPos = GameManager.player.transform.position;
        bool playerInside = (GetComponent<Collider>().bounds.Contains(playerPos));

        if (playerInside)
        {
            CameraManager.DoShiftToNewShot(cameraProps);
        }
        else if (CameraManager.currentBehavior == cameraProps)
        {
            CameraManager.instance.StopAllCoroutines();
            CameraManager.DoGradualReset();
        }

        cachedPlayerInside = playerInside;
        */
	}
}
