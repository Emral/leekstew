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

    [EnumFlag] public CollideDir powerOnFlags;
    [EnumFlag] public CollideDir powerOffFlags;
    [EnumFlag] public CollideDir toggleFlags;

    [HideInInspector] public CharacterController controller;
    [HideInInspector] public Collider collider;
    [HideInInspector] public HealthPoints health;
    [HideInInspector] public AudioSource sound;

    [HideInInspector] public CollideDir collisionSides;
    [HideInInspector] public CollideDir collisionSide;

    [HideInInspector] public Vector3 groundNormal = Vector3.up;
    [HideInInspector] public float groundDistance;
    [HideInInspector] public Vector3 groundPoint;

    [HideInInspector] public bool bounceFlagsUsed;
    [HideInInspector] public bool powerFlagsUsed;

    public bool bounceRestoresDoubleJump = false;
    public float bounceStrength = 18;
    public bool canPower = true;
    public float powerCooldown = 0f;
    public Battery[] powerTargets;
    private float powerCooldownTimer;

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

        if (powerCooldownTimer > 0)
        {
            powerCooldownTimer = Mathf.Max(powerCooldownTimer - Time.deltaTime, 0);
        }
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

    public virtual void ProcessCollision(Transform otherTrans, Vector3 point, Vector3 normal)
    {
        // Debug
        //print(otherTrans.gameObject.name + " touched " + gameObject.name + "from the " + collisionSide.ToString());

        // Process collision effects
        CollidingEntity otherScr = otherTrans.GetComponent<CollidingEntity>();

        if (otherScr != null)
        {
            // Kill
            if (FlagsHelper.IsSet(otherScr.killFlags, collisionSide))
            {
                ReceiveKill(collisionSide, otherScr, otherTrans, point, normal);
            }

            // Harm
            if (FlagsHelper.IsSet(otherScr.harmFlags, collisionSide) && FlagsHelper.IsSet(vulnerableFlags, collisionSide))
            {
                ReceiveHarm(collisionSide, otherScr, otherTrans, point, normal);
            }

            // Push
            if (FlagsHelper.IsSet(otherScr.pushFlags, collisionSide))
            {
                ReceivePush(collisionSide, otherScr, otherTrans, point, normal);
            }

            // Block
            if (FlagsHelper.IsSet(otherScr.blockFlags, collisionSide))
            {
                ReceiveBlock(collisionSide, otherScr, otherTrans, point, normal);
            }

            // Bounce
            if (FlagsHelper.IsSet(otherScr.bounceFlags, collisionSide))
            {
                ReceiveBounce(collisionSide, otherScr, otherTrans, point, normal, otherScr.bounceRestoresDoubleJump, otherScr.bounceStrength);
            }

            // Power
            if (canPower)
            {
                if (powerCooldownTimer == 0)
                {
                    foreach (Battery target in powerTargets)
                    {
                        if (FlagsHelper.IsSet(powerOnFlags, collisionSide))
                        {
                            target.PowerOn();
                            powerCooldownTimer = powerCooldown;
                        }
                        if (FlagsHelper.IsSet(powerOffFlags, collisionSide))
                        {
                            target.PowerOff();
                            powerCooldownTimer = powerCooldown;
                        }
                        if (FlagsHelper.IsSet(toggleFlags, collisionSide))
                        {
                            target.TogglePower();
                            powerCooldownTimer = powerCooldown;
                        }
                    }
                }
            }
        }
        else
        {
            ReceiveBlock(collisionSide, null, otherTrans, point, normal);
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
            ProcessCollision (hit.otherCollider.transform, hit.point, hit.normal);
        }
    }

    public virtual void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Reset the current collison side
        collisionSide = CollideDir.None;
        CollidingEntity other = hit.transform.gameObject.GetComponent<CollidingEntity>();

        if (controller != null)
        {
            // Store top collision
            if (FlagsHelper.IsSet(controller.collisionFlags, CollisionFlags.Above))
            {
                FlagsHelper.Set(ref collisionSides, CollideDir.Up);
                collisionSide = CollideDir.Up;
                if (other != null)
                {
                    other.collisionSide = CollideDir.Down;
                }
            }

            // Store bottom collision
            if (FlagsHelper.IsSet(controller.collisionFlags, CollisionFlags.Below))
            {
                FlagsHelper.Set(ref collisionSides, CollideDir.Down);
                collisionSide = CollideDir.Down;
                groundNormal = hit.normal;
                if (other != null)
                {
                    other.collisionSide = CollideDir.Up;
                }
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
                    if (other != null)
                    {
                        other.collisionSide = CollideDir.Back;
                    }
                }
                else if (angleDiff <= 135)
                {
                    // left and right
                    if (Vector3.Angle(hit.normal * -1f, transform.right) <= 45)
                    {
                        FlagsHelper.Set(ref collisionSides, CollideDir.Right);
                        collisionSide = CollideDir.Right;
                        if (other != null)
                        {
                            other.collisionSide = CollideDir.Left;
                        }
                    }
                    else
                    {
                        FlagsHelper.Set(ref collisionSides, CollideDir.Left);
                        collisionSide = CollideDir.Left;
                        if (other != null)
                        {
                            other.collisionSide = CollideDir.Right;
                        }
                    }
                }
                else
                {
                    // back
                    FlagsHelper.Set(ref collisionSides, CollideDir.Back);
                    collisionSide = CollideDir.Back;
                    if (other != null)
                    {
                        other.collisionSide = CollideDir.Front;
                    }
                }
            }

            // Now process the collision for this hit
            ProcessCollision (hit.transform, hit.point, hit.normal);
            if (other != null)
            {
                other.ProcessCollision(transform, hit.point, hit.normal);
            }
        }
    }


    public virtual void ReceiveHarm(CollideDir side, CollidingEntity otherScr, Transform otherTrans, Vector3 point, Vector3 normal)
    {
        print(gameObject.name + " was harmed by " + otherTrans.gameObject.name);

        if (health != null)
        {
            health.TakeHit();
        }
    }
    public virtual void ReceiveKill(CollideDir side, CollidingEntity otherScr, Transform otherTrans, Vector3 point, Vector3 normal)
    {
        //print(gameObject.name + " was killed by " + otherTrans.gameObject.name);

        if (health != null)
        {
            health.Kill();
        }
    }
    public virtual void ReceiveBlock(CollideDir side, CollidingEntity otherScr, Transform otherTrans, Vector3 point, Vector3 normal)
    {
        //print(gameObject.name + " was blocked by " + otherTrans.gameObject.name);
    }
    public virtual void ReceivePush(CollideDir side, CollidingEntity otherScr, Transform otherTrans, Vector3 point, Vector3 normal)
    {
        //print(gameObject.name + " was pushed by " + otherTrans.gameObject.name);
    }
    public virtual void ReceiveBounce(CollideDir side, CollidingEntity otherScr, Transform otherTrans, Vector3 point, Vector3 normal, bool refreshDouble, float strength)
    {
        //print(gameObject.name + " bounced off " + otherTrans.gameObject.name + " to their " + side.ToString());
    }


    public virtual void ResetCollisionFlags()
    {
        collisionSides = CollideDir.None;
    }
}