using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraVolume : MonoBehaviour
{
    public CameraBehavior cameraProps;

    //private bool cachedPlayerInside;
    //private bool active = false;


    private void OnDrawGizmos()
    {
        Color pink = Color.Lerp(Color.red, Color.white, 0.5f);
        Gizmos.color = pink;
        Gizmos.DrawWireCube(transform.position, transform.lossyScale);
        pink.a = 0.5f;
        Gizmos.color = pink;
        Gizmos.DrawCube(transform.position, transform.lossyScale);
    }

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
        if (other.gameObject == GameManager.player.gameObject && CameraManager.CurrentEquals(cameraProps))
        {
            CameraManager.instance.StopAllCoroutines();
            CameraManager.DoGradualReset(cameraProps.easeTime);
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
