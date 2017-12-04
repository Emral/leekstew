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
        Move,
        Rotate,
        Wait,
        Despawn
    }
    public CommandType type;
    public float duration;
    public Vector3 direction;

    public MovementCommand(CommandType type, float duration, Vector3 direction)
    {
        this.type = type;
        this.duration = duration;
        this.direction = direction;
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
                newSteps[newSteps.Length - 1 - pos] = new MovementCommand(step.type, step.duration, -step.direction);

                pos += 1;
            }
            steps = newSteps;
        }

        Activate();
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
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        if (type != PathType.Single)
        {
            Activate();
        }

    }

    private void StartNext()
    {
        inRoutine = false;

        if (active)
        {
            inRoutine = true;
            MovementCommand m = steps[currentPos];
            switch (m.type)
            {
                case MovementCommand.CommandType.Move:
                    StartCoroutine(Move(m.duration, m.direction));
                    break;
                case MovementCommand.CommandType.Rotate:
                    StartCoroutine(Rotate(m.duration, m.direction));
                    break;
                case MovementCommand.CommandType.Wait:
                    StartCoroutine(Wait(m.duration));
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

    private IEnumerator Move(float time, Vector3 direction)
    {
        float startTime = 0;
        Vector3 startPosition = transform.position;
        while (startTime < time)
        {
            float t = Time.deltaTime;
            transform.Translate(direction * t, movementSpace);
            startTime += t;
            if (useBattery)
            {
                while (!attachedBattery.GetActive())
                {
                    yield return null;
                }
            }
            yield return null;
        }
        StartNext();
    }
    private IEnumerator Wait(float time)
    {
        float startTime = 0;
        while (startTime < time)
        {
            startTime += Time.deltaTime;
            if (useBattery)
            {
                while (!attachedBattery.GetActive())
                {
                    yield return null;
                }
            }
            yield return null;
        }
        StartNext();
    }
    private IEnumerator Rotate(float time, Vector3 direction)
    {
        float startTime = 0;
        while (startTime < time)
        {
            float t = Time.deltaTime;
            transform.Rotate(direction * t, movementSpace);
            startTime += t;
            if (useBattery)
            {
                while (!attachedBattery.GetActive())
                {
                    yield return null;
                }
            }
            yield return null;
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
            inRoutine = false;
            Reset();

            foreach (Transform child in children)
            {
                child.gameObject.SetActive(true);
            }
        }
    }
}