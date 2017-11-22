using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPoints : MonoBehaviour
{
    public int hp = 1;
    public int currentHp;
    public bool vulnerable = true;
    public float mercySeconds = 1f;

    public Transform deathEffect;
    public bool respawn = true;
    public bool destroyOnDeah = true;
    public float respawnDelay = 2f;

    private Vector3 startPosition;

    private float mercyCountdown = 0f;
    private float respawnCountdown;



    // Use this for initialization
    void Start ()
    {
        //currentHp = hp;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if  (mercySeconds > 0f  &&  mercyCountdown > 0f)
        {
            mercyCountdown = Mathf.Max(mercyCountdown-Time.deltaTime, 0f);
            vulnerable = (mercyCountdown == 0f);
        }
    }

    public void Kill()
    {
        if (deathEffect != null)
        {
            GameObject.Instantiate(deathEffect, transform.position, transform.rotation);
        }

        if (respawn)
        {
            currentHp = -1;
            vulnerable = false;
            respawnCountdown = respawnDelay;
            transform.Translate(0f,-999f,0f);
        }
        else if (destroyOnDeah)
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    public void Respawn()
    {
        transform.position = startPosition;
        currentHp = hp;
        vulnerable = true;
    }


    public void ChangeHP (int amount, bool bypassMax)
    {
        if (bypassMax || currentHp+amount <= hp)
            currentHp += amount;
        else
            currentHp = Mathf.Max(currentHp, hp);

        // Check for death
        if (currentHp <= 0)
        {
            Kill();
        }
        else
        {
            vulnerable = false;
            mercyCountdown = mercySeconds;
        }
    }
    public void ChangeHP(int amount)
    {
        ChangeHP(amount, false);
    }


    public void TakeHit()
    {
        if (vulnerable)
        {
            ChangeHP(-1);
        }
    }
}
