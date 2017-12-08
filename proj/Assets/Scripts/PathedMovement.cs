using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[System.Serializable]
public class MovementCommand
{
    public enum CommandType
    {
        Motion,
        Despawn
    }
    public CommandType type;
    [Tooltip("Also determines after how long the object should respawn if 'Despawn' is selected. 0 is never.")]
    public float duration = 1;
    public bool freezePosition = false;
    public bool freezeRotation = false;

    [Tooltip("Whether or not the speed vectors should be interpreted as target position/rotation.")]
    public bool interpretSpeedAsGoal;
    [Tooltip("Moves the GameObject.")]
    public Vector3 speed;
    [Tooltip("Rotates the GameObject.")]
    public Vector3 angularSpeed;

    public MovementCommand(CommandType type, bool interpretSpeedAsGoal, float duration, Vector3 speed, Vector3 angularSpeed, bool freezePosition, bool freezeRotation)
    {
        this.type = type;
        this.duration = duration;
        this.speed = speed;
        this.angularSpeed = angularSpeed;
        this.interpretSpeedAsGoal = interpretSpeedAsGoal;
        this.freezePosition = freezePosition;
        this.freezeRotation = freezeRotation;
    }
}



public class PathedMovement : MonoBehaviour
{
    public enum PathType
    {
        Looping,
        PingPong,
        Single
    }

    [HideInInspector]
    public bool active = false;
    
    public PathType type;
    public Space movementSpace;
    
    private bool inRoutine = false;
    public MovementCommand[] steps;
    private int currentPos = 0;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Battery attachedBattery;

    private bool useBattery;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        attachedBattery = GetComponent<Battery>();
        useBattery = attachedBattery != null;

        if (type == PathType.PingPong)
        {
            MovementCommand[] newSteps = new MovementCommand[steps.Length * 2];
            int pos = 0;
            foreach (MovementCommand step in steps)
            {
                newSteps[pos] = step;
                newSteps[newSteps.Length - 1 - pos] = new MovementCommand(step.type, step.interpretSpeedAsGoal, step.duration, -step.speed, -step.angularSpeed, step.freezePosition, step.freezeRotation);

                pos += 1;
            }
            steps = newSteps;
        }

        Activate();
    }

    private void Update()
    {
        if (!active && useBattery && !inRoutine)
        {
            if (attachedBattery.GetActive())
            {
                Activate();
            }
        }
    }

    public void Activate()
    {
        active = true;
        if (!inRoutine && steps != null && steps.Length > 0)
        {
            StartNext();
        }
    }

    public void Pause()
    {
        active = false;
    }

    public void Reset()
    {
        active = false;
        inRoutine = false;
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        if (type != PathType.Single)
        {
            Activate();
        } else
        {
            if (useBattery && currentPos == 0)
            {
                attachedBattery.PowerOff();
            }
        }

    }

    private void StartNext()
    {
        if (inRoutine && active)
        {
            int lastPos = currentPos - 1;
            if (currentPos == 0)
            {
                lastPos = steps.Length - 1;
            }
            MovementCommand m = steps[lastPos];
            OnEventEnd(lastPos, m.type, m.duration, m.interpretSpeedAsGoal, m.speed, m.angularSpeed, m.freezePosition, m.freezeRotation);
        }
        inRoutine = false;

        if (active)
        {
            MovementCommand m = steps[currentPos];
            inRoutine = true;
            OnEventStart(currentPos,m.type, m.duration, m.interpretSpeedAsGoal, m.speed, m.angularSpeed, m.freezePosition, m.freezeRotation);
            switch (m.type)
            {
                case MovementCommand.CommandType.Motion:
                    StartCoroutine(Motion(m.duration, m.speed, m.angularSpeed, m.interpretSpeedAsGoal, m.freezePosition, m.freezeRotation));
                    break;
                case MovementCommand.CommandType.Despawn:
                    StartCoroutine(Despawn(m.duration));
                    break;
            }
            currentPos = (currentPos + 1) % steps.Length;

            if (currentPos == 0 && type == PathType.Single)
            {
                active = false;
            }
        }
    }

    private IEnumerator Motion(float time, Vector3 speed, Vector3 angularSpeed, bool isGoal, bool freezeP, bool freezeR)
    {

        float startTime = 0;
        Vector3 modifiedSpeed = speed;
        Vector3 modifiedAngular = angularSpeed;

        if (isGoal)
        {
            if (movementSpace == Space.Self)
            {
                modifiedSpeed = (speed - transform.localPosition) / time;
                modifiedAngular = (angularSpeed - transform.localEulerAngles) / time;
            }
            else
            {
                modifiedSpeed = (speed - transform.position) / time;
                modifiedAngular = (angularSpeed - transform.eulerAngles) / time;
            }
        }

        while (startTime < time)
        {
            float t = Time.deltaTime;
            if (!freezeP)
            {
                transform.Translate(modifiedSpeed * t, movementSpace);
            }
            if (!freezeR)
            {
                transform.Rotate(modifiedAngular * t, movementSpace);
            }
            startTime += t;
            yield return StartCoroutine(Wait());
        }

        if (isGoal)
        {
            if (!freezeP)
            {
                if (movementSpace == Space.Self)
                {
                    transform.localPosition = speed;
                } else
                {
                    transform.position = speed;
                }
            }
            if (!freezeR)
            {
                if (movementSpace == Space.Self)
                {
                    transform.localEulerAngles = speed;
                }
                else
                {
                    transform.eulerAngles = speed;
                }
            }
        }

        StartNext();
    }

    private IEnumerator Despawn(float time)
    {
        Transform[] children = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.gameObject.layer == 8 && child.GetComponent<Player>() != null)
            {
                child.parent = null;
            }else if (child != transform && child.gameObject.layer != 8)
            {
                child.gameObject.SetActive(false);
            }
        }
        if (time > 0)
        {
            yield return new WaitForSeconds(time);
            Reset();

            foreach (Transform child in children)
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    private IEnumerator Wait()
    {
        if (useBattery)
        {
            while (!attachedBattery.GetActive())
            {
                yield return null;
            }
        }
        yield return null;
    }

    public virtual void OnEventStart(int index, MovementCommand.CommandType type, float duration, bool isGoal, Vector3 speed, Vector3 angularSpeed, bool freezeP, bool freezeR)
    {

    }

    public virtual void OnEventEnd(int index, MovementCommand.CommandType type, float duration, bool isGoal, Vector3 speed, Vector3 angularSpeed, bool freezeP, bool freezeR)
    {

    }



    /*private IEnumerator Rotate(float time, Vector3 speed, bool isGoal)
    {
        float startTime = 0;
        Vector3 modifiedSpeed = speed;

        if (isGoal)
        {
            modifiedSpeed /= time;
        }

        while (startTime < time)
        {
            float t = Time.deltaTime;
            transform.Rotate(modifiedSpeed * t, movementSpace);
            startTime += t;
            yield return StartCoroutine(Wait());
        }

        if (isGoal)
        {
            transform.eulerAngles = speed;
        }

        StartNext();
    }

    private IEnumerator Wait(float time)
    {
        float startTime = 0;
        while (startTime < time)
        {
            startTime += Time.deltaTime;
            yield return StartCoroutine(Wait());
        }
        StartNext();
    }*/
}