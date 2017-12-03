using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DecayStyle { Subtract, Multiply };

public class TransformEffect : MonoBehaviour
{
    public float effectAmount = 0f;
    public DecayStyle decayStyle = DecayStyle.Multiply;
    public float decayRate = 0.75f;

    public float interval = 0f;

    public bool activeWhenPaused = false;

    [HideInInspector] public Vector3 squashScale = Vector3.one;


    // Use this for initialization
    void Start ()
    {
        StartCoroutine(ReturnValueRoutine());
	}

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.isGamePaused || activeWhenPaused)
        {
            UpdateDecay();
        }
    }

    public virtual void UpdateDecay()
    {
        if (decayStyle == DecayStyle.Multiply)
            effectAmount = effectAmount * decayRate;
        else
            effectAmount = Mathf.Max(0f, effectAmount - decayRate);
    }

    public virtual void UpdateReturnValue()
    {}


    IEnumerator ReturnValueRoutine()
    { 
        while(true)
        {
            // Wait for game to unpause if not active while pausing
            while (GameManager.isGamePaused && !activeWhenPaused)
            {
                yield return null;
            }

            // Update the return value
            if (!GameManager.isGamePaused || activeWhenPaused)
            {
                UpdateReturnValue();
            }

            // Wait for interval seconds
            if (interval > 0f)
                yield return new WaitForSeconds(interval);
            else
                yield return null;
        }
    }
}
