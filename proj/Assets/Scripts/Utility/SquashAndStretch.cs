using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquashAndStretch : TransformEffect
{
    [HideInInspector] public Vector3 squashScale = Vector3.one;

    public override void UpdateReturnValue()
    {
        squashScale = Vector3.one + new Vector3(effectAmount, -effectAmount, effectAmount);
        if (applyAutomatically)
            transform.localScale = squashScale;
    }
}
