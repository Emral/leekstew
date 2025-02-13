﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    [HideInInspector] public Collider myCollider;
    [HideInInspector] public HealthPoints health;
    [HideInInspector] public AudioSource sound;

    [HideInInspector] public CollideDir collisionSides;
    [HideInInspector] public CollideDir collisionSide;

    [HideInInspector] public Vector3 groundNormal = Vector3.up;
    [HideInInspector] public float groundDistance;
    [HideInInspector] public Vector3 groundPoint;

    [HideInInspector] public bool bounceFlagsUsed;
    [HideInInspector] public bool powerFlagsUsed;

    public static Dictionary<CollideDir, CollideDir> oppositeSide;

    public Vector3 velocity;
    public float gravityRate = 0.0f;

    public bool bounceRestoresDoubleJump = false;
    public float bounceStrength = 18;
    public bool canPower = true;
    public float powerCooldown = 0f;
    public Battery[] powerTargets;
    private float powerCooldownTimer;

    [HideInInspector] public bool shouldShiftDown;

    public virtual void Update()
    {
        if (!GameManager.isGamePaused)
        {
            UpdateGroundInfo();
            UpdateAI();
            if (velocity.magnitude > 0)
                CommitMovement();
        }
    }
    public virtual void Start()
    {
        UpdateReferences();
    }

    public virtual void LateUpdate()
    {
        ResetCollisionFlags();

        if (powerCooldownTimer > 0)
        {
            powerCooldownTimer = Mathf.Max(powerCooldownTimer - Time.deltaTime, 0);
        }
    }

    public void UpdateGroundInfo()
    {
        if (gravityRate != 0)
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
    public virtual void UpdateAI()
    {}

    public virtual void CommitMovement()
    {
        // Commit movement
        if (controller != null)
        {
            controller.Move(velocity * Time.deltaTime * 60f);
        }
        else
        {
            transform.Translate(velocity * Time.deltaTime * 60f);
        }

        if (shouldShiftDown)
        {
            ShiftToGround();
        }
    }

    public virtual void UpdateReferences()
    {
        if (oppositeSide == null)
        {
            oppositeSide = new Dictionary<CollideDir, CollideDir>();
            oppositeSide[CollideDir.None] = CollideDir.None;
            oppositeSide[CollideDir.Back] = CollideDir.Front;
            oppositeSide[CollideDir.Front] = CollideDir.Back;
            oppositeSide[CollideDir.Right] = CollideDir.Left;
            oppositeSide[CollideDir.Left] = CollideDir.Right;
            oppositeSide[CollideDir.Up] = CollideDir.Down;
            oppositeSide[CollideDir.Down] = CollideDir.Up;
        }

        if (controller == null)
            controller = GetComponent<CharacterController>();
        if (myCollider == null)
            myCollider = GetComponent<Collider>();
        if (health == null)
            health = GetComponent<HealthPoints>();
        if (sound == null)
            sound = GetComponent<AudioSource>();
    }


    public virtual void ShiftToGround()
    {
        UpdateGroundInfo();

        if (groundDistance < 1f && groundDistance > 0.2f)
        {
            Vector3 shiftVector = Vector3.up * -1 * (groundDistance + (1-groundNormal.y));

            //if (controller != null)
            //    controller.Move(shiftVector);
            //else
            transform.Translate(shiftVector);
        }
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

    public virtual void ProcessCollision(Transform otherTrans, Vector3 point, Vector3 normal, CollidingEntity otherScr)
    {
        // Debug
        /*
        if (otherScr != null)
            print("COLLISION: " + otherTrans.gameObject.name + " (" + otherScr.collisionSide.ToString() + ") --> " + gameObject.name + " (" + collisionSide.ToString() + ")");
        else
            print("COLLISION: " + otherTrans.gameObject.name + " --> " + gameObject.name + " (" + collisionSide.ToString() + ")");
        */
        //print(otherTrans.gameObject.name + " touched " + gameObject.name + " from " + gameObject.name + "'s " + collisionSide.ToString());

        if (otherScr != null)
        {
            // Kill
            if (FlagsHelper.IsSet(otherScr.killFlags, collisionSide))
            {
                ReceiveKill(collisionSide, otherScr, otherTrans, point, normal);
            }

            // Harm
            if (FlagsHelper.IsSet(otherScr.harmFlags, collisionSide) && FlagsHelper.IsSet(vulnerableFlags, oppositeSide[collisionSide]))
            {
                ReceiveHarm(oppositeSide[collisionSide], otherScr, otherTrans, point, normal);
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

                // HACKY WORKAROUND
                if (otherScr != null)
                    otherScr.GiveBounce(this);
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
        CollidingEntity otherScr = collision.transform.GetComponent<CollidingEntity>();

        // Go through each hit
        foreach (ContactPoint hit in collision.contacts)
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
            ProcessCollision (hit.otherCollider.transform, hit.point, hit.normal, otherScr);
            if (otherScr != null)
            {
                otherScr.ProcessCollision(transform, hit.point, hit.normal, this);
            }
        }
    }

    public virtual void OnTriggerEnter(Collider collision)
    {
        // Reset the current collison side
        collisionSide = CollideDir.None;
        CollidingEntity otherScr = collision.transform.GetComponent<CollidingEntity>();
        
        // Now process the collision for this hit
        ProcessCollision(collision.transform, Vector3.zero, Vector3.zero, otherScr);
        if (otherScr != null)
        {
            otherScr.ProcessCollision(transform, Vector3.zero, Vector3.zero, this);
        }
    }

    public virtual void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Reset the current collison side

        if (controller != null)
        {
            collisionSide = CollideDir.None;
            CollidingEntity other = hit.transform.gameObject.GetComponent<CollidingEntity>();
            
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

            // Store horizontal collison. We store this first, because storing it later causes the freefall glitch.
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


            // Now process the collision for this hit
            ProcessCollision (hit.transform, hit.point, hit.normal, other);
            if (other != null)
            {
                other.ProcessCollision(transform, hit.point, hit.normal, this);
            }
        }
    }


    public virtual void ReceiveHarm(CollideDir side, CollidingEntity otherScr, Transform otherTrans, Vector3 point, Vector3 normal)
    {
        //print(gameObject.name + " was harmed by " + otherTrans.gameObject.name + " from " + gameObject.name + "'s " + side.ToString());

        if (health != null)
        {
            health.TakeHit();
        }
    }
    public virtual void ReceiveKill(CollideDir side, CollidingEntity otherScr, Transform otherTrans, Vector3 point, Vector3 normal)
    {
        //print(gameObject.name + " was killed by " + otherTrans.gameObject.name + " from " + gameObject.name + "'s " + side.ToString());

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
        //print(gameObject.name + " was pushed by " + otherTrans.gameObject.name + " from " + gameObject.name + "'s " + side.ToString());
    }
    public virtual void ReceiveBounce(CollideDir side, CollidingEntity otherScr, Transform otherTrans, Vector3 point, Vector3 normal, bool refreshDouble, float strength)
    {
        //print(gameObject.name + " bounced off " + otherTrans.gameObject.name + " from " + gameObject.name + "'s " + side.ToString());
    }

    public virtual void GiveBounce(CollidingEntity otherScr)
    {
    }


    public virtual void ResetCollisionFlags()
    {
        collisionSides = CollideDir.None;
    }
}