using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveRing : MonoBehaviour {

    public float speed = 1f;

    private ParticleSystem part;
    private float cachedRadius;

	// Use this for initialization
	void Start ()
    {
        part = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 pos = transform.position;

        float yDist = Mathf.Abs(playerPos.y - pos.y);

        playerPos.y = 0f;
        pos.y = 0f;

        float xDist = Vector3.Distance(pos, playerPos);

        ParticleSystem.ShapeModule shape = part.shape;
        shape.enabled = true;

        float radiusSize = shape.radius + (Time.deltaTime * speed * 4f);
        shape.radius = radiusSize;

        // If the shockwave crossed the player's path, hurt the player
        if (xDist > cachedRadius && xDist < shape.radius && yDist < 0.5f)
            GameManager.instance.player.ReceiveHarm(CollideDir.Down, null, transform, GameManager.instance.player.transform.position, Vector3.up);


        cachedRadius = radiusSize;


        // Destroy if too big
        if (shape.radius > 15)
            GameObject.Destroy(this.gameObject);

    }
}
