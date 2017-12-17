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
    public float distanceLimit = 0f;

    public GameObject deathEffect;

    public ProjectileProperties(ProjectileProperties p)
    {
        speed = p.speed;
        acceleration = p.acceleration;
        speedMax = p.speedMax;
        speedMin = p.speedMin;
        magnitudeLimit = p.magnitudeLimit;
        lifetime = p.lifetime;
        distanceLimit = p.distanceLimit;
        deathEffect = p.deathEffect;
    }
}


public class Projectile : MonoBehaviour
{
    [SerializeField] public ProjectileProperties properties;
    [HideInInspector] public bool pooled = false;

    private Vector3 startPoint;
    private Vector3 speed = Vector3.zero;
    private float timePassed = 0f;

    public GameObject deathEffect;


    public void Start()
    {
        startPoint = transform.position;
        speed = properties.speed;
        timePassed = 0;
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
        if ((timePassed >= properties.lifetime && properties.lifetime > 0f) || (Vector3.Distance(transform.position, startPoint) > properties.distanceLimit && properties.distanceLimit > 0f))
        {
            if (!pooled)
            {
                if (deathEffect == null && properties.deathEffect != null)
                    deathEffect = properties.deathEffect;

                if (deathEffect != null)
                {
                    GameObject.Instantiate(properties.deathEffect, transform.position, Quaternion.identity);
                }
                GameObject.Destroy(this.gameObject);
            }
            else
            {
                if (deathEffect != null)
                {
                    deathEffect.SetActive(true);
                    deathEffect.transform.position = transform.position;
                }
                timePassed = 0;
                transform.position = startPoint;
                gameObject.SetActive(false);
            }
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
