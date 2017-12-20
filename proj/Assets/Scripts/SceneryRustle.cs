using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneryRustle : MonoBehaviour
{
    private float bumpCooldown = 0f;

	
    void Rustle ()
    {
        foreach (MultichannelAudio multiSource in GetComponentsInChildren<MultichannelAudio>())
        {
            multiSource.Play("rustle");
        }
        foreach (Shake shake in GetComponentsInChildren<Shake>())
        {
            shake.effectAmount = 0.25f;
        }

    }

    // Update is called once per frame
    private void Update()
    {
        bumpCooldown = Mathf.Max(0f, bumpCooldown-Time.deltaTime);
    }



    public void OnTriggerEnter(Collider other)
    {
        Rustle();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (bumpCooldown == 0f)
        {
            bumpCooldown = 0.75f;
            Rustle();
        }
    }
}
