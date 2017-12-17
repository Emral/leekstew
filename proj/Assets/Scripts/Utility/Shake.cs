using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : TransformEffect
{
    [HideInInspector] public Vector3 shakeOffset = Vector3.zero;


    private void Reset()
    {
        decayStyle = DecayStyle.Subtract;
        decayRate = 0.05f;
    }

    public override void UpdateReturnValue()
    {
        shakeOffset = new Vector3(Random.Range(-effectAmount, effectAmount), Random.Range(-effectAmount, effectAmount), Random.Range(-effectAmount, effectAmount));
        if (applyAutomatically)
            transform.localPosition = shakeOffset;
    }
}
