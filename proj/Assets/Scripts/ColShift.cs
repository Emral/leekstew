using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColShift : MonoBehaviour {

    private Material[] m;
    private float t;

    private Color[] c;

    public Gradient newColor;

    public float length = 1;

    public bool paused = false;
    public bool shared = false;

    private Battery b;
    public bool useBatteryIfAvailable = true;

	// Use this for initialization
	void Start () {
        if (shared)
        {
            m = gameObject.GetComponent<MeshRenderer>().sharedMaterials;
            c = new Color[m.Length];
            for (int i = 0; i < m.Length; i++)
            {
                c[i] = m[i].color;
            }
        } else
        {
            m = gameObject.GetComponent<MeshRenderer>().materials;
        }
        if (length <= 0)
        {
            length = 0.0001f;
        }

        b = GetComponent<Battery>();

        if (b != null && useBatteryIfAvailable)
        {
            paused = b.GetActive();
        }
    }

    private void UpdateColor()
    {
        t += Time.deltaTime;
        for (int i = 0; i < m.Length; i++)
        {
            m[i].color = newColor.Evaluate((t / length) % 1);
            m[i].color *= 0.75f;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (b != null && useBatteryIfAvailable)
        {
            paused = !b.GetActive();
        }
        if (!paused)
        {
            UpdateColor();
        }
    }

    void OnDisable()
    {
        if (shared)
        {
            for (int i = 0; i < m.Length; i++)
            {
                m[i].color = c[i];
            }
        }
    }
}
