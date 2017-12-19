using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;


[System.Flags] public enum CameraProperties
{
    Yaw           = 1,
    Pitch         = 2,
    XPan          = 4,
    YPan          = 8,
    Zoom          = 16,
    Target        = 32,
    Position      = 64,
    Region        = 128,
    PlayerControl = 256
}

[System.Serializable]
public class CameraBehavior
{
    public string debugName;

    public float easeTime = 1f;
    public int priority = 0;

    [EnumFlag] public CameraProperties changedProperties;

    public float yaw;
    public float pitch;
    public float panX;
    public float panY;
    public float zoom;
    public Vector3 position;
    public Transform target;
    public Vector3 regionMin;
    public Vector3 regionMax;

    public bool valid = false;

    public CameraBehavior()
    {

    }

    public CameraBehavior(CameraBehavior other)
    {
        FieldInfo[] properties = this.GetType().GetFields();
        foreach (FieldInfo mI in properties)
        {
            mI.SetValue(this, mI.GetValue((object)other));
        }

        this.debugName += " (Copy)";
    }

    public override string ToString()
    {
        string compiled = "(";

        FieldInfo[] properties = this.GetType().GetFields();
        foreach (FieldInfo mI in properties)
        {
            string valueStr = "null";
            object value = mI.GetValue(this);
            if (value != null)
                valueStr = value.ToString();
            compiled = compiled + mI.Name +": "+ valueStr + ", ";
        }
        compiled = compiled + ")";

        return compiled;
    }
}


public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [ReadOnly, TextArea(5, 5)] public string defaultBehaviorName;
    [ReadOnly, TextArea(5, 5)] public string currentBehaviorName;
    [ReadOnly, TextArea(5, 5)] public string roomBehaviorName;
    [ReadOnly, TextArea(5, 5)] public string appliedBehaviorName;
    private static CameraBehavior defaultBehavior = null;
    private static CameraBehavior currentBehavior = null;
    private static CameraBehavior roomBehavior = null;
    private static CameraBehavior appliedBehavior = null;
    public static List<CameraBehavior> behaviorHistory;

    public static float targetZoom = 1f;
    public static float currentZoom = 1f;
    public static float zoomSpeed = 0.05f;

    public Transform target;
    public static Transform dollyTrans;
    public static Transform cameraTrans;
    public static Shake shake;
    public static Skybox skybox;

    public static float constantShake = 0f;

    private List<float> leftWhiskerDistances;
    private List<float> rightWhiskerDistances;

    private float walkInputTime = 0f;
    private float cameraInputTime = 0f;

    private bool shifting = false;

    private bool playerChoiceLock = false;

    public bool getBehindPlayer = false;



    public static int CurrentPriority
    {
        get
        {
            if (currentBehavior != null)
                return currentBehavior.priority;

            return 0;
        }
    }

    public static bool CurrentEquals(CameraBehavior behavior)
    {
        return (currentBehavior == behavior);
    }

    public void UpdateRefs()
    {
        instance = this;
        dollyTrans = transform.Find("Dolly");
        cameraTrans = transform.Find("Dolly/Main Camera");
        shake = dollyTrans.gameObject.GetComponent<Shake>();
        skybox = Camera.main.gameObject.GetComponent<Skybox>();

        roomBehavior = null;
        Room currentRoomScr = LevelManager.CurrentRoomScript;
        if (currentRoomScr != null)
        {
            if (currentRoomScr.newCamera.debugName == null)
                currentRoomScr.newCamera.debugName = "[ROOM " + currentRoomScr.roomName + " (" + currentRoomScr.roomId.ToString() + ") BEHAVIOR]";

            if (currentRoomScr.changeCamera)
            {
                roomBehavior = currentRoomScr.newCamera;
            }
        }

        if (defaultBehavior != null)
        {
            defaultBehavior.zoom = cameraTrans.localPosition.z;
            defaultBehavior.pitch = dollyTrans.localRotation.eulerAngles.x;
            defaultBehavior.yaw = dollyTrans.localRotation.eulerAngles.y;
            defaultBehavior.debugName = "[DEFAULT BEHAVIOR]";
            defaultBehavior.valid = true;
        }

        if (behaviorHistory != null)
        {
            if (behaviorHistory.Count > 10)
                behaviorHistory.RemoveAt(10);
        }

        appliedBehavior = currentBehavior != null ? currentBehavior : (roomBehavior != null ? roomBehavior : null);

        defaultBehaviorName = defaultBehavior == null ? "" : defaultBehavior.ToString();
        currentBehaviorName = currentBehavior == null ? "" : currentBehavior.ToString();
        roomBehaviorName = roomBehavior == null ? "" : roomBehavior.ToString();
        appliedBehaviorName = appliedBehavior == null ? "" : appliedBehavior.ToString();
    }

    private void Awake()
    {
        UpdateRefs();
    }

    void Start()
    {
        leftWhiskerDistances = new List<float>();
        rightWhiskerDistances = new List<float>();
        behaviorHistory = new List<CameraBehavior>();
        UpdateRefs();
        defaultBehavior = CaptureCurrentShot();
        defaultBehavior.changedProperties = (CameraProperties)~0;
        defaultBehavior.regionMin = Vector3.one * -9999f;
        defaultBehavior.regionMax = Vector3.one * 9999f;
    }

    public static CameraBehavior CaptureCurrentShot()
    {
        CameraBehavior newBehavior = new CameraBehavior();
        newBehavior.changedProperties = (CameraProperties)~0;

        //print("INITIAL PROPS: " + System.Convert.ToString((int)newBehavior.changedProperties, 2));

        FlagsHelper.Unset(ref newBehavior.changedProperties, CameraProperties.Region);

        newBehavior.debugName = "[CAPTURED SHOT]";
        newBehavior.valid = true;
        newBehavior.panX = cameraTrans.localPosition.x;
        newBehavior.panY = cameraTrans.localPosition.y;
        newBehavior.zoom = cameraTrans.localPosition.z;
        newBehavior.pitch = dollyTrans.localRotation.eulerAngles.x;
        newBehavior.yaw = dollyTrans.localRotation.eulerAngles.y;
        newBehavior.position = instance.transform.position;
        newBehavior.target = instance.target;

       // print("CHANGED PROPS: " + System.Convert.ToString((int)newBehavior.changedProperties,2));
        return newBehavior;
    }


    // Update is called once per frame
    void LateUpdate()
    {
        UpdateRefs();

        if (!GameManager.isGamePaused)
        {
            if (constantShake >= 0f)
                shake.effectAmount = constantShake;

            dollyTrans.localPosition = (shake.shakeOffset * OptionsManager.cameraShakeStrength);


            if (target != null)
            {
                float moveX = 0;
                float moveY = 0;

                if (!shifting)
                {
                    // Apply camera behavior

                    //Or don't do it yet, because it breaks the camera when respawning. Uncomment this and the if statement below to see what I mean. ~Enjl

                    //ApplyBehavior(appliedBehavior);

                    if (target != null)
                    {
                        transform.position = target.position;  //transform.position = Vector3.Lerp(transform.position, target.position, 0.25f);
                        if (GameManager.player != null && target == GameManager.player.transform)  // && appliedBehavior == null)
                        {
                            // Player camera control stick
                            moveX = GameManager.inputVals["Cam X"] * OptionsManager.cameraSpeedX * 0.5f * (OptionsManager.cameraInvertedX ? -1 : 1) * (GameManager.cutsceneMode ? 0 : 1);
                            moveY = GameManager.inputVals["Cam Y"] * OptionsManager.cameraSpeedY * 0.33f * (OptionsManager.cameraInvertedY ? 1 : -1) * (GameManager.cutsceneMode ? 0 : 1);


                            // Vertical management
                            Vector2 vertLimits = new Vector2(-22f, 80f);

                            float currentVal = dollyTrans.rotation.eulerAngles.x;
                            if (currentVal > 180)
                                currentVal -= 360;

                            float vertSpan = vertLimits.y - vertLimits.x;
                            float vertMid = (vertLimits.x + vertLimits.y) * 0.5f;
                            float vertDistAbs = Mathf.Abs(currentVal - vertMid);
                            float vertMidDistMult = 1f - Mathf.InverseLerp(0f, vertSpan * 0.5f, vertDistAbs);

                            float vertAdd = moveY;
                            if ((moveY > 0 && currentVal > vertMid) || (moveY < 0 && currentVal < vertMid))
                                vertAdd *= vertMidDistMult;

                            float newX = dollyTrans.rotation.eulerAngles.x + vertAdd;
                            if (newX > 180)
                                newX = Mathf.Max(newX, 360f + vertLimits.x);
                            else
                                newX = Mathf.Clamp(newX, vertLimits.x, vertLimits.y);


                            // Vert and horizontal
                            Quaternion rotation = Quaternion.Euler(newX, dollyTrans.rotation.eulerAngles.y + moveX, 0f);
                            dollyTrans.rotation = rotation;

                            float angleForLerp = dollyTrans.rotation.eulerAngles.x;
                            while (angleForLerp > 180)
                            {
                                angleForLerp -= 360;
                            }


                            // Determine zoom
                            float distanceInvLerp = Mathf.InverseLerp(-22f, 80f, angleForLerp);
                            float distanceAmount = Mathf.Lerp(-2.5f, -25f, distanceInvLerp);
                            float fovAmount = Mathf.Lerp(70f, 60f, distanceInvLerp);
                            Vector3 newLocalPos = new Vector3(cameraTrans.localPosition.x, cameraTrans.localPosition.y, distanceAmount);
                            cameraTrans.localPosition = newLocalPos;
                            Camera.main.fieldOfView = fovAmount;


                            // Whiskers to avoid walls
                            leftWhiskerDistances.Clear();
                            rightWhiskerDistances.Clear();
                            float rightMinDist = 999f;
                            float leftMinDist = 999f;


                            Vector3 startDirection = cameraTrans.position - target.position;
                            startDirection.y = 0f;//*= -1f;

                            RaycastHit hit;
                            LayerMask avoidMask = 1 << 9;


                            // Main raycast for whiskers (inaccurate comment but)
                            float minDist = 999f;
                            if (Physics.Linecast(GameManager.player.transform.position, cameraTrans.position, out hit, avoidMask))
                            {
                                bool noNeedToJump = false;
                                for (int i = -5; i < 5; i++)
                                {
                                    noNeedToJump = !Physics.Linecast(GameManager.player.transform.position + Vector3.up * i, cameraTrans.position, out hit, avoidMask);
                                    if (noNeedToJump)
                                        break;
                                }
                                if (!noNeedToJump)
                                    minDist = Mathf.Max(hit.distance, 4f);
                            }



                            // Side raycasts for whiskers
                            /*
                            for (int i = 0; i <= 10; i++)
                            {

                                // Right side
                                Ray ray = new Ray(target.position, Quaternion.AngleAxis(45 + (i / 10f) * 90f, Vector3.up) * startDirection);
                                Vector3 endPoint = ray.GetPoint(distanceAmount);

                                if (Physics.Linecast(target.position, endPoint, out hit, avoidMask))
                                {
                                    rightMinDist = Mathf.Min(rightMinDist, hit.distance);
                                    rightWhiskerDistances.Add(hit.distance);
                                    endPoint = ray.GetPoint(hit.distance);
                                    Debug.DrawLine(target.position, endPoint, Color.red);
                                }
                                else
                                    Debug.DrawLine(target.position, endPoint, Color.yellow);

                                // Left side
                                ray = new Ray(target.position, Quaternion.AngleAxis(-45 - (i / 10f) * 90f, Vector3.up) * startDirection);
                                endPoint = ray.GetPoint(distanceAmount);

                                if (Physics.Linecast(target.position, endPoint, out hit, avoidMask))
                                {
                                    leftMinDist = Mathf.Min(leftMinDist, hit.distance);
                                    leftWhiskerDistances.Add(hit.distance);
                                    endPoint = ray.GetPoint(hit.distance);
                                    Debug.DrawLine(target.position, endPoint, Color.red);
                                }
                                else
                                    Debug.DrawLine(target.position, endPoint, Color.yellow);
                            }
                            */

                            // Move the camera forward and backward based on the whiskers and stuff
                            newLocalPos.z = Mathf.Max(-minDist + 1, newLocalPos.z);
                            cameraTrans.localPosition = newLocalPos;



                            // Raise and lower the dolly to look over the player's shoulder when the camera is close
                            Vector3 newPos = new Vector3(0, Mathf.Lerp(2f, 0f, distanceInvLerp * distanceInvLerp * distanceInvLerp), 0);
                            dollyTrans.localPosition = newPos + (shake.shakeOffset * OptionsManager.cameraShakeStrength);


                            // Player camera automation control vars
                            float rotRate = (GameManager.player.GetGrounded()) ? 0.01f : 0.00625f;
                            Vector3 playerRotEuler = GameManager.player.transform.rotation.eulerAngles;
                            Vector3 dollyTransEuler = dollyTrans.rotation.eulerAngles;

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
                                            dollyTrans.rotation = Quaternion.RotateTowards(dollyTrans.rotation, Quaternion.Euler(dollyTrans.rotation.x, dollyTrans.rotation.y-5*i, dollyTrans.rotation.z), 2);
                                            break;
                                        }
                                        else if (rHit)
                                        {
                                            print("Avoiding to the left");
                                            Debug.DrawLine(GameManager.player.transform.position, lWhiskerEnd, Color.yellow);
                                            Debug.DrawLine(GameManager.player.transform.position, rWhiskerEnd, Color.red);
                                            dollyTrans.rotation = Quaternion.RotateTowards(dollyTrans.rotation, Quaternion.Euler(dollyTrans.rotation.x, dollyTrans.rotation.y+5*i, dollyTrans.rotation.z), 2);
                                            break;
                                        }

                                    }
                                    else if (!lHit)
                                    {
                                        print("Random avoiding");
                                        Debug.DrawLine(GameManager.player.transform.position, lWhiskerEnd, Color.yellow);
                                        Debug.DrawLine(GameManager.player.transform.position, rWhiskerEnd, Color.yellow);
                                        //dollyTrans.rotation = Quaternion.RotateTowards(dollyTrans.rotation, Quaternion.Euler(dollyTrans.rotation.x, dollyTrans.rotation.y-5*i, dollyTrans.rotation.z), 2);
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

                            // wait until X seconds after the player has moved
                            CharacterController playercc = GameManager.player.GetCharacterController();
                            walkInputTime = playercc.velocity.magnitude > 0f ? Mathf.Clamp(walkInputTime + Time.deltaTime, 0f, 2f) : Mathf.Clamp(walkInputTime - Time.deltaTime, -2f, 0f);
                            cameraInputTime = moveX != 0f || moveY != 0f ? Mathf.Clamp(cameraInputTime + Time.deltaTime, 0f, 2f) : Mathf.Clamp(cameraInputTime - Time.deltaTime, -2f, 0f);

                            // Look down when falling
                            if (GameManager.player.groundDistance > 5)
                            {
                                playerRotEuler.x += 55;
                            }
                            if (GameManager.player.groundDistance > 10)
                            {
                                playerRotEuler.x += 55;
                            }

                            // Look over the edge when standing at cliffs
                            if (GameManager.player.groundDistance < 0.1f)
                            {
                                Vector3 playerFront = GameManager.player.transform.position + GameManager.player.transform.forward * 1.5f;
                                Vector3 playerFrontGround = playerFront - Vector3.up * 8f;
                                bool didHit = Physics.Linecast(playerFront, playerFrontGround, out hit, avoidMask);

                                Vector3 floorPoint = playerFrontGround;
                                if (didHit)
                                    floorPoint = hit.point;

                                if (Physics.Linecast(cameraTrans.position, floorPoint, out hit, avoidMask) && floorPoint.y < GameManager.player.groundPoint.y - 0.25f)
                                    playerRotEuler.x += 55;

                                // Look up and down slopes
                                float groundAngle = Vector3.Angle(GameManager.player.groundNormal, Vector3.up);
                                if (groundAngle > 11 && GameManager.player.velocity.magnitude > 0)
                                {
                                    if (GameManager.player.walkingUphill)
                                    {
                                        playerRotEuler.x -= 22;
                                    }
                                    else
                                    {
                                        playerRotEuler.x += 22;
                                    }
                                }
                            }

                            playerRotEuler.y = (playerRotEuler.y) % 360;


                            // If the player moves the camera at a certain height, keep it there until it gets lower again
                            if (dollyTransEuler.x < 33 && moveX == 0 && moveY == 0)
                                playerChoiceLock = false;

                            if (dollyTransEuler.x > 33 && (moveX != 0 || moveY != 0))
                            {
                                playerChoiceLock = true;
                            }

                            if (playerChoiceLock)
                                playerRotEuler.x = dollyTransEuler.x;


                            // Rotate when moving
                            float playerAngleFromCamera = Quaternion.Angle(Quaternion.Euler(0f, playerRotEuler.y, 0f), dollyTrans.rotation);
                            if (OptionsManager.dynamicCamera && cameraInputTime == -2f && walkInputTime == 2f && playerAngleFromCamera < 120f)
                            {
                                dollyTrans.rotation = Quaternion.Lerp(dollyTrans.rotation, Quaternion.Euler(playerRotEuler.x, playerRotEuler.y, playerRotEuler.z), rotRate);
                            }


                            // Camera snap if holding the right trigger
                            if (GameManager.inputVals["Cam Focus"] > 0.5 && (playerAngleFromCamera < 160f || playercc.velocity.magnitude == 0f))
                            {
                                dollyTrans.rotation = Quaternion.Lerp(dollyTrans.rotation, GameManager.player.transform.rotation, 0.03f);
                            }


                            //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(playerRotEuler.x, playerRotEuler.y, -playerRotEuler.z), 0.005f);
                        }
                    }
                }
            }
        }
    }


    public static void ApplyBehavior(CameraBehavior behavior)
    {
        if (behavior != null)
        {
            //print("APPLYING CAMERA BEHAVIOR "+behavior.debugName);

            Vector3 oldPos = instance.transform.position;
            Quaternion oldAngle = dollyTrans.rotation;
            Vector3 oldOffset = cameraTrans.localPosition;

            CameraProperties chProps = behavior.changedProperties;

            bool targetChanged = FlagsHelper.IsSet(chProps, CameraProperties.Target);
            bool positionChanged = FlagsHelper.IsSet(chProps, CameraProperties.Position);
            bool pitchChanged = FlagsHelper.IsSet(chProps, CameraProperties.Pitch);
            bool yawChanged = FlagsHelper.IsSet(chProps, CameraProperties.Yaw);
            bool xPanChanged = FlagsHelper.IsSet(chProps, CameraProperties.XPan);
            bool yPanChanged = FlagsHelper.IsSet(chProps, CameraProperties.YPan);
            bool zoomChanged = FlagsHelper.IsSet(chProps, CameraProperties.Zoom);
            bool regionChanged = FlagsHelper.IsSet(chProps, CameraProperties.Region);


            // Position
            Vector3 newPos = oldPos + Vector3.zero;

            if (targetChanged && behavior.target != null)
            {
                newPos = behavior.target.position;
            }
            else if (positionChanged)
            {
                newPos = behavior.position;
            }

            if (regionChanged)
            {
                newPos = new Vector3(Mathf.Clamp(newPos.x, behavior.regionMin.x, behavior.regionMax.x),
                                     Mathf.Clamp(newPos.y, behavior.regionMin.y, behavior.regionMax.y),
                                     Mathf.Clamp(newPos.z, behavior.regionMin.z, behavior.regionMax.z));
            }


            // Orbit angle
            Quaternion newAngle = oldAngle;

            if (yawChanged || pitchChanged)
            {
                Vector3 newAngleEuler = oldAngle.eulerAngles;
                if (yawChanged)
                    newAngleEuler.y = behavior.yaw;
                if (pitchChanged)
                    newAngleEuler.x = behavior.pitch;

                newAngle = Quaternion.Euler(newAngleEuler);
            }

            // Panning and zoom
            Vector3 newOffset = oldOffset + Vector3.zero;
            if (xPanChanged)
            {
                newOffset.x = behavior.panX;
            }
            if (yPanChanged)
            {
                newOffset.y = behavior.panY;
            }
            if (zoomChanged)
            {
                newOffset.z = behavior.zoom;
            }

            // Assign
            instance.transform.position = newPos;
            dollyTrans.rotation = newAngle;
            cameraTrans.localPosition = newOffset;
        }
    }



    public static void DoShiftToNewShot(CameraBehavior newBehavior, float goalTime = 0f, int priority = 0)
    {
        instance.StartCoroutine(instance.ShiftToNewShot(newBehavior, goalTime, priority));
    }
    public static void DoGradualReset(float goalTime = 1f)
    {
        instance.StartCoroutine(instance.GraduallyResetShot(goalTime));
    }

    public IEnumerator DelayedShake(float strength)
    {
        while(shake == null)
        {
            yield return null;
        }

        shake.effectAmount = strength;
    }

    public IEnumerator ShiftToNewShot(CameraBehavior newBehavior, float goalTime = 0f, int priority = 0)
    {
        if (priority == 0)
            priority = newBehavior.priority;

        if (priority >= CurrentPriority && currentBehavior != newBehavior && !shifting)
        {

            Vector3 oldPos = transform.position;
            Quaternion oldAngle = dollyTrans.rotation;
            Vector3 oldOffset = cameraTrans.localPosition;

            if (goalTime == 0)
                goalTime = newBehavior.easeTime;

            bool targetChanged = FlagsHelper.IsSet(newBehavior.changedProperties, CameraProperties.Target);
            bool positionChanged = FlagsHelper.IsSet(newBehavior.changedProperties, CameraProperties.Position);
            bool pitchChanged = FlagsHelper.IsSet(newBehavior.changedProperties, CameraProperties.Pitch);
            bool yawChanged = FlagsHelper.IsSet(newBehavior.changedProperties, CameraProperties.Yaw);
            bool xPanChanged = FlagsHelper.IsSet(newBehavior.changedProperties, CameraProperties.XPan);
            bool yPanChanged = FlagsHelper.IsSet(newBehavior.changedProperties, CameraProperties.YPan);
            bool zoomChanged = FlagsHelper.IsSet(newBehavior.changedProperties, CameraProperties.Zoom);
            bool regionChanged = FlagsHelper.IsSet(newBehavior.changedProperties, CameraProperties.Region);

            /*
            if (currentBehavior != null)
                print("SHIFTING TO " + newBehavior.ToString()+" FROM "+currentBehavior.ToString());
            else
                print("SHIFTING TO "+newBehavior.ToString());


            int numChanged = 0;
            bool[] allFlags = { targetChanged, positionChanged, pitchChanged, yawChanged, xPanChanged, yPanChanged, zoomChanged, regionChanged };
            foreach (bool flag in allFlags)
            {
                if (flag)
                    numChanged++;
            }
            print("CHANGED FLAGS: " + numChanged.ToString());
            //*/

            target = null;
            currentBehavior = newBehavior;
            shifting = true;

            float timeLeft = goalTime;
            while (timeLeft > 0f)
            {
                // Position
                Vector3 newPos = oldPos + Vector3.zero;

                if (targetChanged && newBehavior.target != null)
                {
                    newPos = newBehavior.target.position;
                }
                else if (positionChanged)
                {
                    newPos = newBehavior.position;
                }

                if (regionChanged)
                {
                    //print("regionChanged");
                    newPos = new Vector3(Mathf.Clamp(newPos.x, newBehavior.regionMin.x, newBehavior.regionMax.x),
                                         Mathf.Clamp(newPos.y, newBehavior.regionMin.y, newBehavior.regionMax.y),
                                         Mathf.Clamp(newPos.z, newBehavior.regionMin.z, newBehavior.regionMax.z));
                }


                // Orbit angle
                Quaternion newAngle = oldAngle;

                if (yawChanged || pitchChanged)
                {
                    Vector3 newAngleEuler = oldAngle.eulerAngles;
                    if (yawChanged)
                        newAngleEuler.y = newBehavior.yaw;
                    if (pitchChanged)
                        newAngleEuler.x = newBehavior.pitch;

                    newAngle = Quaternion.Euler(newAngleEuler);
                }

                // Panning and zoom
                Vector3 newOffset = oldOffset + Vector3.zero;
                if (xPanChanged)
                {
                    newOffset.x = newBehavior.panX;
                }
                if (yPanChanged)
                {
                    newOffset.y = newBehavior.panY;
                }
                if (zoomChanged)
                {
                    newOffset.z = newBehavior.zoom;
                }

                /*
                if (Vector3.Equals(oldPos, newPos))
                    print("POSITIONS ARE THE SAME");
                if (Vector3.Equals(oldOffset, newOffset))
                    print("OFFSETS ARE THE SAME");
                if (Quaternion.Equals(oldAngle, newAngle))
                    print("ROTATIONS ARE THE SAME");
                */


                // Assign
                float percent = 1f - (timeLeft / goalTime);

                transform.position = new Vector3(Mathf.SmoothStep(oldPos.x, newPos.x, percent), Mathf.SmoothStep(oldPos.y, newPos.y, percent), Mathf.SmoothStep(oldPos.z, newPos.z, percent));
                dollyTrans.rotation = Quaternion.Slerp(oldAngle, newAngle, percent);
                cameraTrans.localPosition = new Vector3(Mathf.SmoothStep(oldOffset.x, newOffset.x, percent), Mathf.SmoothStep(oldOffset.y, newOffset.y, percent), Mathf.SmoothStep(oldOffset.z, newOffset.z, percent));

                //print("NEW POSITION: "+newPos.ToString());


                timeLeft -= Time.deltaTime;

                yield return null;
            }

            if (targetChanged)
            {
                target = newBehavior.target;
            }
            behaviorHistory.Insert(0, newBehavior);
            shifting = false;
        }
        else
            yield return null;
    }

    public IEnumerator GraduallyResetShot(float goalTime = 1f)
    {
        yield return StartCoroutine(ShiftToNewShot(roomBehavior != null ? roomBehavior : defaultBehavior, goalTime, 9999));
        currentBehavior = null;
    }
}
