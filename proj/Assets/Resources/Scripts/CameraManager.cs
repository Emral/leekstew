using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public Transform target;
    private Transform dolly;
    private Transform camera;

    private bool playerChoiceLock = false;

    public bool getBehindPlayer = false;


    // Use this for initialization
    void Start()
    {
        dolly = transform.Find("Dolly");
        camera = dolly.Find("Main Camera");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target != null && !GameManager.isGamePaused)
        {
            transform.position = target.position;  //transform.position = Vector3.Lerp(transform.position, target.position, 0.25f);


            float moveX = 0;
            float moveY = 0;

            if (target == GameManager.player.transform)
            {
                // Player camera control stick
                moveX = GameManager.inputVals["Cam X"] * OptionsManager.cameraSpeedX*0.5f * (OptionsManager.cameraInvertedX ? -1 : 1);
                moveY = GameManager.inputVals["Cam Y"] * OptionsManager.cameraSpeedY*0.33f * (OptionsManager.cameraInvertedY ? 1 : -1);

                Vector2 vertLimits = new Vector2(-22f, 80f);

                float currentVal = dolly.rotation.eulerAngles.x;
                if (currentVal > 180)
                    currentVal -= 360;

                float vertSpan = vertLimits.y - vertLimits.x;
                float vertMid = (vertLimits.x + vertLimits.y) * 0.5f;
                float vertDistAbs = Mathf.Abs(currentVal - vertMid);
                float vertMidDistMult = 1f - Mathf.InverseLerp(0f, vertSpan * 0.5f, vertDistAbs);

                float vertAdd = moveY;
                if ((moveY > 0 && currentVal > vertMid) || (moveY < 0 && currentVal < vertMid))
                    vertAdd *= vertMidDistMult;

                float newX = dolly.rotation.eulerAngles.x + vertAdd;
                if (newX > 180)
                    newX = Mathf.Max(newX, 360f + vertLimits.x);
                else
                    newX = Mathf.Clamp(newX, vertLimits.x, vertLimits.y);



                Quaternion rotation = Quaternion.Euler(newX, dolly.rotation.eulerAngles.y + moveX, 0f);
                dolly.rotation = rotation;

                float angleForLerp = dolly.rotation.eulerAngles.x;
                while (angleForLerp > 180)
                {
                    angleForLerp -= 360;
                }

                float distanceInvLerp = Mathf.InverseLerp(-22f, 80f, angleForLerp);
                float distanceAmount = Mathf.Lerp(-2.5f, -25f, distanceInvLerp);
                float fovAmount = Mathf.Lerp(70f, 60f, distanceInvLerp);
                Vector3 newLocalPos = new Vector3(0, 0, distanceAmount);
                camera.localPosition = newLocalPos;
                Camera.main.fieldOfView = fovAmount;


                Vector3 newPos = new Vector3(0, Mathf.Lerp(2f, 0f, distanceInvLerp), 0);
                dolly.localPosition = newPos;

                // Camera snap
                if (GameManager.inputVals["Cam Focus"] > 0.5)
                {
                    dolly.rotation = Quaternion.Lerp(dolly.rotation, GameManager.player.transform.rotation, 0.03f);
                }


                // Player camera automation
                float rotRate = (GameManager.player.GetGrounded()) ? 0.01f : 0.00625f;
                Vector3 playerRotEuler = GameManager.player.transform.rotation.eulerAngles;
                Vector3 dollyEuler = dolly.rotation.eulerAngles;

                // Avoidance
                /*
                RaycastHit hit;
                LayerMask avoidMask = 1 << 9;

                if (Physics.Linecast(camera.position, GameManager.player.transform.position, out hit, avoidMask) && moveX == 0)
                {
                    Debug.DrawLine(camera.position, GameManager.player.transform.position, Color.red);

                    Vector3 toPlayerDir = GameManager.player.transform.position - camera.position;
                    float toPlayerDirY = toPlayerDir.y;
                    toPlayerDir.y = 0;
                    float toPlayerDist = Vector3.Distance(GameManager.player.transform.position, camera.position);

                    for (int i = 0; i < 40; i++)
                    {
                        Vector3 lWhisker = -1 * (Quaternion.Euler(0, 5 * i, 0) * toPlayerDir);
                        lWhisker.y = -toPlayerDirY;
                        Vector3 rWhisker = -1 * (Quaternion.Euler(0, -5 * i, 0) * toPlayerDir);
                        rWhisker.y = -toPlayerDirY;

                        Vector3 lWhiskerEnd = GameManager.player.transform.position + lWhisker.normalized * toPlayerDist;
                        Vector3 rWhiskerEnd = GameManager.player.transform.position + rWhisker.normalized * toPlayerDist;

                        RaycastHit lAvoidHit;
                        RaycastHit rAvoidHit;
                        bool lHit = Physics.Linecast(GameManager.player.transform.position, lWhiskerEnd, out lAvoidHit, avoidMask);
                        bool rHit = Physics.Linecast(GameManager.player.transform.position, rWhiskerEnd, out rAvoidHit, avoidMask);

                        if (lHit != rHit)
                        {
                            if (lHit)
                            {
                                print("Avoiding to the right");
                                Debug.DrawLine(GameManager.player.transform.position, lWhiskerEnd, Color.red);
                                Debug.DrawLine(GameManager.player.transform.position, rWhiskerEnd, Color.yellow);
                                dolly.rotation = Quaternion.RotateTowards(dolly.rotation, Quaternion.Euler(dolly.rotation.x, dolly.rotation.y-5*i, dolly.rotation.z), 2);
                                break;
                            }
                            else if (rHit)
                            {
                                print("Avoiding to the left");
                                Debug.DrawLine(GameManager.player.transform.position, lWhiskerEnd, Color.yellow);
                                Debug.DrawLine(GameManager.player.transform.position, rWhiskerEnd, Color.red);
                                dolly.rotation = Quaternion.RotateTowards(dolly.rotation, Quaternion.Euler(dolly.rotation.x, dolly.rotation.y+5*i, dolly.rotation.z), 2);
                                break;
                            }

                        }
                        else if (!lHit)
                        {
                            print("Random avoiding");
                            Debug.DrawLine(GameManager.player.transform.position, lWhiskerEnd, Color.yellow);
                            Debug.DrawLine(GameManager.player.transform.position, rWhiskerEnd, Color.yellow);
                            //dolly.rotation = Quaternion.RotateTowards(dolly.rotation, Quaternion.Euler(dolly.rotation.x, dolly.rotation.y-5*i, dolly.rotation.z), 2);
                            break;
                        }
                        else
                        {
                            Debug.DrawLine(GameManager.player.transform.position, lWhiskerEnd, Color.red);
                            Debug.DrawLine(GameManager.player.transform.position, rWhiskerEnd, Color.red);
                        }
                    }
                }
                else
                {
                    Debug.DrawLine(camera.position, GameManager.player.transform.position, Color.yellow);
                }
                */

                // Look down when falling
                if (GameManager.player.groundDistance > 3)
                {
                    playerRotEuler.x += 55;
                }
                if (GameManager.player.groundDistance > 5)
                {
                    playerRotEuler.x += 22;
                }

                playerRotEuler.y = (playerRotEuler.y) % 360;


                // If the player moves the camera at a certain height, keep it there until it gets lower again
                if (dollyEuler.x < 33 && moveX == 0 && moveY == 0)
                    playerChoiceLock = false;

                if (dollyEuler.x > 33 && (moveX != 0 || moveY != 0))
                {
                    playerChoiceLock = true;
                }

                if (playerChoiceLock)
                    playerRotEuler.x = dollyEuler.x;


                // Rotate when moving
                CharacterController playercc = GameManager.player.GetCharacterController();
                if (playercc.velocity.magnitude > 0 && moveX == 0 && moveY == 0 && Quaternion.Angle(Quaternion.Euler(0f, playerRotEuler.y, 0f), dolly.rotation) < 120f)
                {
                    dolly.rotation = Quaternion.Lerp(dolly.rotation, Quaternion.Euler(playerRotEuler.x, playerRotEuler.y, playerRotEuler.z), rotRate);
                }

                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(playerRotEuler.x, playerRotEuler.y, -playerRotEuler.z), 0.005f);
            }
        }
    }
}
