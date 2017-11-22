using UnityEngine;

[System.Flags] public enum CollideDir
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 4,
    Right = 8,
    Front = 16,
    Back = 32

    //Y = Up | Down,
    //X = Left | Right,
    //Z = Front | Back,

    //XY = Up | Down | Left | Right,
    //XZ = Front | Back | Left | Right,
    //YZ = Up | Down | Left | Right
}

public class CollidingEntity : MonoBehaviour
{
    [EnumFlag] public CollideDir vulnerableFlags;
    [EnumFlag] public CollideDir harmFlags;
    [EnumFlag] public CollideDir killFlags;
    [EnumFlag] public CollideDir blockFlags;
    [EnumFlag] public CollideDir pushFlags;
    [EnumFlag] public CollideDir bounceFlags;

    [HideInInspector] public CharacterController controller;
    [HideInInspector] public Collider collider;
    [HideInInspector] public HealthPoints health;
    [HideInInspector] public AudioSource sound;

    [HideInInspector] public CollideDir collisionSides;
    [HideInInspector] public CollideDir collisionSide;

    [HideInInspector] public Vector3 groundNormal = Vector3.up;
    [HideInInspector] public float groundDistance;
    [HideInInspector] public Vector3 groundPoint;


    public virtual void Update()
    {
        UpdateReferences();

        if (!GameManager.isGamePaused)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -Vector3.up, out hit, 100.0f, 1 << 9))
            {
                groundDistance = hit.distance;
                groundPoint = hit.point;
                groundNormal = hit.normal;
            }
            else
            {
                groundDistance = 100f;
                groundPoint = transform.position + (Vector3.up * -100f);
                groundNormal = Vector3.up;
            }
        }
    }

    public virtual void LateUpdate()
    {
        ResetCollisionFlags();
    }

    public virtual void UpdateReferences()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
        if (collider == null)
            collider = GetComponent<Collider>();
        if (health == null)
            health = GetComponent<HealthPoints>();
        if (sound == null)
            sound = GetComponent<AudioSource>();
    }


    public virtual void PlaySound(AudioClip clip, float pitch)
    {
        if (sound != null)
        {
            sound.clip = clip;
            sound.pitch = pitch;
            sound.PlayOneShot(clip);
        }
    }
    public virtual void PlaySound(AudioClip clip)
    {
        PlaySound(clip, 1f);
    }

    public virtual void ProcessCollision(Transform otherTrans)
    {
        // Debug
        print(otherTrans.gameObject.name + " touched " + gameObject.name + "from the " + collisionSide.ToString());

        // Process collision effects
        CollidingEntity otherScr = otherTrans.GetComponent("CollidingEntity") as CollidingEntity;

        if (otherScr != null)
        {
            // Kill
            if (FlagsHelper.IsSet(otherScr.killFlags, collisionSide))
            {
                ReceiveKill(collisionSide, otherScr);
            }

            // Harm
            else if (FlagsHelper.IsSet(otherScr.harmFlags, collisionSide) && FlagsHelper.IsSet(vulnerableFlags, collisionSide))
            {
                ReceiveHarm(collisionSide, otherScr);
            }

            // Push
            else if (FlagsHelper.IsSet(otherScr.pushFlags, collisionSide))
            {
                ReceivePush(collisionSide, otherScr);
            }

            // Block
            else if (FlagsHelper.IsSet(otherScr.blockFlags, collisionSide))
            {
                ReceiveBlock(collisionSide, otherScr);
            }

            // Bounce
            else if (FlagsHelper.IsSet(otherScr.bounceFlags, collisionSide))
            {
                ReceiveBounce(collisionSide, otherScr);
            }
        }
    }

    public virtual void OnCollisionEnter(Collision collision)
    {
        // Reset the current collison side
        collisionSide = CollideDir.None;

        // Go through each hit
        foreach(ContactPoint hit in collision.contacts)
        {
            // Store top collision
            if (Vector3.Angle(hit.normal, -transform.up) < 45)
            {
                FlagsHelper.Set(ref collisionSides, CollideDir.Up);
                collisionSide = CollideDir.Up;
            }

            // Store bottom collision
            else if (Vector3.Angle(hit.normal, transform.up) < 45)
            {
                FlagsHelper.Set(ref collisionSides, CollideDir.Down);
                collisionSide = CollideDir.Down;
                groundNormal = hit.normal;
            }

            // Store horizontal collison
            else
            {
                Vector3 fwd = transform.forward;
                float angleDiff = Vector3.Angle(hit.normal * -1f, fwd);
                if (angleDiff <= 45)
                {
                    // front
                    FlagsHelper.Set(ref collisionSides, CollideDir.Front);
                    collisionSide = CollideDir.Front;
                }
                else if (angleDiff <= 135)
                {
                    // left and right
                    if (Vector3.Angle(hit.normal * -1f, transform.right) <= 45)
                    {
                        FlagsHelper.Set(ref collisionSides, CollideDir.Right);
                        collisionSide = CollideDir.Right;
                    }
                    else
                    {
                        FlagsHelper.Set(ref collisionSides, CollideDir.Left);
                        collisionSide = CollideDir.Left;
                    }
                }
                else
                {
                    // back
                    FlagsHelper.Set(ref collisionSides, CollideDir.Back);
                    collisionSide = CollideDir.Back;
                }
            }

            // Now process the collision for this hit
            ProcessCollision (hit.otherCollider.transform);
        }
    }

    public virtual void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Reset the current collison side
        collisionSide = CollideDir.None;

        if (controller != null)
        {
            // Store top collision
            if (FlagsHelper.IsSet(controller.collisionFlags, CollisionFlags.Above))
            {
                FlagsHelper.Set(ref collisionSides, CollideDir.Up);
                collisionSide = CollideDir.Up;
            }

            // Store bottom collision
            if (FlagsHelper.IsSet(controller.collisionFlags, CollisionFlags.Below))
            {
                FlagsHelper.Set(ref collisionSides, CollideDir.Down);
                collisionSide = CollideDir.Down;
                groundNormal = hit.normal;
            }

            // Store horizontal collison
            if (FlagsHelper.IsSet(controller.collisionFlags, CollisionFlags.Sides))
            {
                Vector3 fwd = transform.forward;
                float angleDiff = Vector3.Angle(hit.normal * -1f, fwd);
                if (angleDiff <= 45)
                {
                    // front
                    FlagsHelper.Set(ref collisionSides, CollideDir.Front);
                    collisionSide = CollideDir.Front;
                }
                else if (angleDiff <= 135)
                {
                    // left and right
                    if (Vector3.Angle(hit.normal * -1f, transform.right) <= 45)
                    {
                        FlagsHelper.Set(ref collisionSides, CollideDir.Right);
                        collisionSide = CollideDir.Right;
                    }
                    else
                    {
                        FlagsHelper.Set(ref collisionSides, CollideDir.Left);
                        collisionSide = CollideDir.Left;
                    }
                }
                else
                {
                    // back
                    FlagsHelper.Set(ref collisionSides, CollideDir.Back);
                    collisionSide = CollideDir.Back;
                }
            }

            // Now process the collision for this hit
            ProcessCollision (hit.transform);
        }
    }


    public virtual void ReceiveHarm(CollideDir side, CollidingEntity otherScr)
    {
        print(gameObject.name + " was harmed by " + otherScr.gameObject.name);

        if (health != null)
        {
            health.TakeHit();
        }
    }
    public virtual void ReceiveKill(CollideDir side, CollidingEntity otherScr)
    {
        print(gameObject.name + " was killed by " + otherScr.gameObject.name);

        if (health != null)
        {
            health.Kill();
        }
    }
    public virtual void ReceiveBlock(CollideDir side, CollidingEntity otherScr)
    {
        print(gameObject.name + " was blocked by " + otherScr.gameObject.name);
    }
    public virtual void ReceivePush(CollideDir side, CollidingEntity otherScr)
    {
        print(gameObject.name + " was pushed by " + otherScr.gameObject.name);
    }
    public virtual void ReceiveBounce(CollideDir side, CollidingEntity otherScr)
    {
        print(gameObject.name + " bounced off " + otherScr.gameObject.name + " to their " + side.ToString());
    }


    public virtual void ResetCollisionFlags()
    {
        collisionSides = CollideDir.None;
    }
}