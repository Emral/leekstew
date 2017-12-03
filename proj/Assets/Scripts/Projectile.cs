using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProjectileProperties
{
    public Vector3 speed;
    public Vector3 acceleration;
    public Vector3 speedMax;
    public Vector3 speedMin;
    public float magnitudeLimit;

    public float lifetime = 0f;

    public GameObject deathEffect;
}


public class Projectile : MonoBehaviour
{
    [SerializeField] public ProjectileProperties properties;
    private Vector3 speed = Vector3.zero;
    private float timePassed = 0f;


    public void Start()
    {
        speed = properties.speed;
    }

    public virtual void UpdateMovement()
    {
        speed += properties.acceleration;
        if (properties.speedMax != Vector3.zero || properties.speedMin != Vector3.zero)
        {
            speed.x = Mathf.Clamp(speed.x, properties.speedMin.x, properties.speedMax.x);
            speed.y = Mathf.Clamp(speed.y, properties.speedMin.y, properties.speedMax.y);
            speed.z = Mathf.Clamp(speed.z, properties.speedMin.z, properties.speedMax.z);
        }

        if (properties.magnitudeLimit != 0f)
            speed = Vector3.ClampMagnitude(speed, properties.magnitudeLimit);


        transform.Translate(speed);
    }
    public virtual void UpdateLife()
    {
        timePassed += Time.deltaTime;
        if (timePassed >= properties.lifetime && properties.lifetime > 0f)
        {
            if (properties.deathEffect != null)
                GameObject.Instantiate(properties.deathEffect, transform.position, Quaternion.identity);

            GameObject.Destroy(this.gameObject);
        }
    }


    void Update()
    {
        if (!GameManager.isGamePaused)
        {
            UpdateMovement();
            UpdateLife();
        }
    }
}
