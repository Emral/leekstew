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
        Wait
    }

    public CommandType type;
    public float duration;
    public Vector3 direction;
}



public class PathedMovement : CollidingEntity
{
    public bool active = true;
    public float gap = 1f;
    public bool relative = true;
    public bool closed = false;
    private bool inRoutine = false;
    public MovementCommand[] steps;
    private int currentPos = 0;
    private Transform otherScr;

    void Start()
    {
        if (steps != null && steps.Length > 0)
        {
            StartNext();
        }
    }

    public void Activate()
    {
        active = true;
        if (!inRoutine)
        {
            StartNext();
        }
    }

    public void Deactivate()
    {
        active = false;
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
            }
        }
        currentPos = (currentPos + 1) % steps.Length;
    }

    public override void OnCollisionEnter(Collision c)
    {
        base.OnCollisionEnter(c);
        otherScr = c.transform;
    }
    
    private IEnumerator Move(float time, Vector3 direction)
    {
        float startTime = 0;
        Vector3 startPosition = transform.position;
        while (startTime < time)
        {
            float t = Time.deltaTime;
            transform.position += direction * t;
            startTime += t;
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
            transform.Rotate(direction * t, Space.Self);
            startTime += t;
            yield return null;
        }
        StartNext();
    }
}