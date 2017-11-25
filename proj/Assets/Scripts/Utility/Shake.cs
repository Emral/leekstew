using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour
{
    public float shakeAmount = 0f;
    public float decayRate = 0.05f;
    [HideInInspector] public Vector3 shakeOffset = Vector3.zero;

    // Use this for initialization
    void Start()
    {
		StartCoroutine(ShakeOffsetUpdate());
	}
	
	// Update is called once per frame
	void Update ()
    {
        shakeAmount = Mathf.Max(0f, shakeAmount - decayRate);
    }

    IEnumerator ShakeOffsetUpdate()
    {
        while (true)
        {
            shakeOffset = new Vector3(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount));
            yield return new WaitForSeconds(0.01f);
        }
    }
}
