using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveState {Standing, Walking, Jumping, Falling};

public class Player : CollidingEntity
{

    public float walkSpeed = 0.0625f;
    public float runSpeed = 0.125f;
    public float turnRate = 10f;
    public float jumpStrength = 18f;
    public int jumpLimit = 2;
    public float jumpDecayRate = 0.75f;

    public AudioClip[] jumpSounds;
    public AudioClip[] doublejumpSounds;
    public AudioClip[] hurtSounds;

    public int jumpLenienceTimer = -1;

    [HideInInspector] public bool inputActive = true;

    private Vector3 wallNormal;
    private Vector3 wallPoint;

    private bool wallSliding = false;
    private bool wallJumping = false;
    private int wallSlidingCounter = 0;
    private float wallSlidingTime = 0f;

    public GameObject hurtParticle;
    public GameObject wallSlideParticle;
    public GameObject wallJumpParticle;

    private bool hurtPause = false;

    private float squashAmount = 0;

    private int jumpsPerformed = 0;

    private Vector3 startPos;

    private bool releasedJump = false;
    private bool groundedCompensation = false;

    private Transform model;
    private MoveState moveState = MoveState.Falling;
    
    private Vector3 moveDir;
    private float forwardMomentum;
    public Vector3 directionalMomentum;

    private string animState;
    private bool lockedAnimState = false;

    private Vector3 cameraForward;
    private Animator animator;
    private bool ehhhImWalkinHere;

    private Vector3 lastTouchedNormal = Vector3.zero;


    // Use this for initialization
    void Start ()
    {
        startPos = transform.position;
        //thisBetterBeWorthIt = transform.GetComponent("ThirdPersonCharacter") as UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter;
    }



    public override void UpdateReferences()
    {
        base.UpdateReferences();

        if (animator == null)
            animator = transform.GetComponentInChildren(typeof(Animator)) as Animator;

        if (model == null)
            model = GameObject.Find("animmodel_demo").transform;
    }



    public CharacterController GetCharacterController()
    {
        UpdateReferences();
        return controller;
    }

    public Vector3 GetDirMomentum()
    {
        return directionalMomentum;
    }

    public bool GetGrounded()
    {
        return groundedCompensation;
    }


    void OnGUI()
    {
        //string debugStr = "Input:\nH:" + Input.GetAxis("Horizontal").ToString() + "\nV:" + Input.GetAxis("Vertical").ToString() + "\n\nCamera:\n" + cameraForward.ToString() + "\n\nMove Direction:\n" + moveDir.ToString();
        //GUI.Label (new Rect(Screen.width*0.25f, Screen.height*0.25f, Screen.width*0.5f, Screen.height*0.5f), debugStr);
    }

    void SetAnimState (string state, float crossfade)
    {
        if (animState != state && !lockedAnimState)
        {
            animator.CrossFade(state, crossfade);
            animState = state;
        }
    }


    void OnCollisionEnter(Collision collision)
    {

    }

    public override void OnControllerColliderHit(ControllerColliderHit hit)
    {
        base.OnControllerColliderHit(hit);

        /*
        // Wall collision
        if (collidingSide)
        {
            // Wall jumps
            if (!collidingDown)
            {
                if (Vector3.Angle(hit.normal, -1f * moveDir) < 15 && directionalMomentum.y <= 0)
                {
                    directionalMomentum.y = Mathf.Max(directionalMomentum.y, -0.25f);
                }
            }
        }
        */
        // Landing on something
        if (collisionSide == CollideDir.Down  &&  hit.gameObject.layer == 9)
        {

            LandOnSolidSurface();
            //transform.position = new Vector3(transform.position.x, contact.point.y, transform.position.z);
            /*
            if (controller != null)
            {
                Vector3 newVel = controller.velocity;
                    newVel.x = 0f;
                newVel.z = 0f;
                //controller.velocity = newVel;
            }
            */
            //forwardMomentum = 0f;
        }
        //print(contact.thisCollider.name + " hit " + contact.otherCollider.name);
        //Debug.DrawRay(contact.point, contact.normal, Color.white);
    }

    public void LandOnSolidSurface()
    {
        if (!groundedCompensation)
        {
            SetAnimState("standing", 0f);

            groundedCompensation = true;
            squashAmount = 1f;
            jumpsPerformed = 0;
            directionalMomentum.y = -0.1f;
            lastTouchedNormal = Vector3.zero;
        }
    }

    IEnumerator HurtAnim()
    {
        // Delay by a frame for reasons
        //yield return null;

        Material tempMat = gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material;
        tempMat.mainTexture = Resources.Load<Texture>("Textures/tex_demo_alt1");

        AudioClip randHurtSound = hurtSounds[(int)Random.Range(0, hurtSounds.Length)];
        PlaySound(randHurtSound, Random.Range(0.8f, 1.2f));

        GameObject.Instantiate(hurtParticle, transform.position+Vector3.up, Quaternion.identity, transform);
        SetAnimState("flipping", 0.25f);
        lockedAnimState = true;
        inputActive = false;

        Transform modelCenter = transform.Find("modelCentered");
        hurtPause = true;

        Vector3 tempMomentum = directionalMomentum + Vector3.zero;

        float timeLeft = 0.35f;
        while (timeLeft > 0)
        {
            modelCenter.localRotation = Quaternion.Euler(-22f, 0f, 0f);

            tempMomentum = new Vector3(directionalMomentum.x != 0 ? directionalMomentum.x : tempMomentum.x,
                                       directionalMomentum.y != 0 ? directionalMomentum.y : tempMomentum.y,
                                       directionalMomentum.z != 0 ? directionalMomentum.z : tempMomentum.z);

            directionalMomentum = new Vector3(0f, 0.01f, 0f);
            jumpsPerformed = 99;
            timeLeft -= Time.deltaTime;
            yield return null;
        }
        directionalMomentum = tempMomentum;

        modelCenter.localRotation = Quaternion.identity;
        hurtPause = false;
        inputActive = true;
        lockedAnimState = false;

        // Blinking
        Material blinkMat = new Material(tempMat);
        Color blinkCol = new Color(1, 1, 1, 1);
        foreach (SkinnedMeshRenderer renderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            renderer.material = blinkMat;
        }

        timeLeft = 0.3f;
        while (health.mercyCountdown > 0f)
        {
            blinkCol.g = 0.5f + 0.3f * Mathf.Round(Mathf.Cos(Time.time * 20f));
            blinkCol.b = blinkCol.g;
            blinkMat.color = blinkCol;

            yield return null;
        }
        tempMat.mainTexture = Resources.Load<Texture>("Textures/tex_demo");

        foreach (SkinnedMeshRenderer renderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            renderer.material = tempMat;
        }
    }

    IEnumerator BounceRoutine()
    {
        /*
        while (hurtPause)
        {
            yield return null;
        }
        //*/

        transform.Translate(Vector3.up*0.1f);
        directionalMomentum.y = jumpStrength*0.015f;
        controller.Move(directionalMomentum);
        groundedCompensation = false;

        releasedJump = true;
        jumpsPerformed = 1;
        yield return null;
    }

    IEnumerator WallJumpVelocity()
    {
        GameObject.Instantiate(wallJumpParticle, wallPoint, Quaternion.identity);
        wallJumping = true;
        SetAnimState("walljumping", 0f);
        lockedAnimState = true;
        transform.rotation = Quaternion.LookRotation(wallNormal, Vector3.up);
        Vector3 reflectedDir = Vector3.Lerp(wallNormal, Vector3.Reflect(moveDir, wallNormal), 0.5f);
        float timeLeft = 0.3f;
        while (timeLeft > 0 && !groundedCompensation)
        {

            directionalMomentum += reflectedDir * 0.25f;
            timeLeft -= Time.deltaTime;
            yield return null;
        }
        lockedAnimState = false;
        wallJumping = false;
    }

    IEnumerator DoubleJumpFlip()
    {
        SetAnimState("flipping", 0.5f);
        lockedAnimState = true;
        Transform modelCenter = transform.Find("modelCentered");
        float totalTime = 0.5f;
        float timeLeft = totalTime;

        float lastTimeLeft = timeLeft;

        while(timeLeft > 0 && !groundedCompensation)
        {
            float timeMult = timeLeft / totalTime;
            float timeMultEased = Mathf.Sqrt(1 - timeMult*timeMult);
            modelCenter.localRotation = Quaternion.Euler(360 * timeMultEased, 0, 0);
            timeLeft -= Time.deltaTime;

            if (timeLeft < 0.5f*totalTime && lastTimeLeft >= 0.5f*totalTime)
            {
                lockedAnimState = false;
                SetAnimState("falling", 1f);
                lockedAnimState = true;
            }

            lastTimeLeft = timeLeft;
            yield return null;
        }
        lockedAnimState = false;
        modelCenter.localRotation = Quaternion.Euler(0, 0, 0);
    }


    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (!GameManager.isGamePaused)
        {
            // Update animation stuff based on physics
            if (!controller.isGrounded && groundDistance > 0.5f && groundedCompensation) //&& Mathf.Abs(controller.velocity.y) > 1f)
            {
                print("Started falling");
                groundedCompensation = false;
                jumpLenienceTimer = 20;
            }

            squashAmount *= 0.75f;
            float squashMult = squashAmount * 0.25f;
            model.localScale = new Vector3(1 + squashMult, 1 - squashMult, 1 + squashMult);


            if (!groundedCompensation)
            {
                jumpLenienceTimer = Mathf.Max(-1, jumpLenienceTimer - 1);
                if (jumpLenienceTimer == 0)
                {
                    jumpsPerformed = Mathf.Max(jumpsPerformed, 1);
                    print("Jump leniency over.");
                }

                string newState = controller.velocity.y > 0 ? "jumping" : "falling";
                SetAnimState(newState, 0.5f);
            }


            // Determine walking direction
            float h = inputActive && !GameManager.cutsceneMode ? GameManager.inputVals["Walk X"] : 0;
            float v = inputActive && !GameManager.cutsceneMode ? GameManager.inputVals["Walk Y"] : 0;

            if (Camera.main != null)
            {
                Transform currentCamTrans = Camera.main.transform;
                cameraForward = Vector3.Scale(currentCamTrans.forward, new Vector3(1, 0, 1)).normalized;
                moveDir = v * cameraForward + h * currentCamTrans.right;
            }
            else
            {
                moveDir = v * Vector3.forward + h * Vector3.right;
            }


            // Determine walking speed
            float moveSpeed = runSpeed;


            // Perform horizontal movement
            Quaternion targetRotation = moveDir.magnitude > 0 ? Quaternion.LookRotation(moveDir, Vector3.up) : transform.rotation;
            targetRotation = wallJumping ? Quaternion.LookRotation(directionalMomentum, Vector3.up) : targetRotation;


            if (controller.isGrounded)
            {
                //forwardMomentum = moveDir.magnitude * moveSpeed;
                directionalMomentum = new Vector3(0f, directionalMomentum.y, 0f) + moveDir * moveSpeed; //Vector3.forward * forwardMomentum;

                if (moveDir.magnitude > 0)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnRate);

                    ehhhImWalkinHere = true;
                    if (moveDir.magnitude > 0.5)
                    {
                        animator.speed = 1.5f;
                        SetAnimState("running", 0.25f);
                    }
                    else
                    {
                        animator.speed = 1 + moveDir.magnitude * 2;
                        SetAnimState("walking", 0.25f);
                    }
                }
            }
            else
            {
                if (moveDir.magnitude > 0)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 0.5f * turnRate);
                //controller.AddForce(moveDir * walkSpeed * 500);

                Vector3 airSteering = moveDir * walkSpeed * 0.125f;
                Vector2 newHorzVel = Vector2.ClampMagnitude(new Vector2(directionalMomentum.x + airSteering.x, directionalMomentum.z + airSteering.z), moveSpeed);
                //controller.velocity = new Vector3(newHorzVel.x,controller.velocity.y, newHorzVel.y);

                directionalMomentum = new Vector3(newHorzVel.x, directionalMomentum.y, newHorzVel.y);
            }


            // Vertical movement
            if (!groundedCompensation)
                directionalMomentum.y = Mathf.Max(directionalMomentum.y - 0.01f, -2f);


            //Debug.DrawRay(transform.position, wallNormal * 200f, Color.red);
            if (wallSliding)
                print("CURRENTLY WALL SLIDING");

            // Perform jump
            if (GameManager.inputPress["Jump"] && (wallSliding || jumpsPerformed < jumpLimit) && !GameManager.cutsceneMode)
            {
                squashAmount = jumpsPerformed == 0 || wallSliding ? -1.5f : 0f;
                releasedJump = false;

                transform.Translate(Vector3.up * 0.1f);
                directionalMomentum.y = jumpStrength * Mathf.Pow(jumpDecayRate, jumpsPerformed) * 0.015f;
                groundedCompensation = false;

                AudioClip randJumpSound = jumpSounds[(int)Random.Range(0, jumpSounds.Length)];
                if (wallSliding)
                {
                    print("WALL JUMP");
                    lastTouchedNormal = wallNormal;
                    directionalMomentum.y *= 0.9f;
                    StartCoroutine(WallJumpVelocity());
                    jumpsPerformed = 1;
                }
                else
                {
                    if (jumpsPerformed > 0)
                    {
                        StartCoroutine("DoubleJumpFlip");
                        randJumpSound = doublejumpSounds[(int)Random.Range(0, doublejumpSounds.Length)];
                    }
                    jumpsPerformed++;
                }


                /*
                if (moveDir.magnitude > 0 && jumpsPerformed > 0)
                {
                    directionalMomentum.x = 0f;
                    directionalMomentum.z = 0f;
                }
                */


                //Vector3 newVel = controller.velocity;
                //newVel.y = jumpStrength * Mathf.Pow(jumpDecayRate, jumpsPerformed);
                //controller.velocity = newVel;

                SetAnimState("jumping", 0f);
                PlaySound(randJumpSound, Random.Range(0.8f,1.2f));
                wallSliding = false;
            }

            if (GameManager.inputRelease["Jump"] && !groundedCompensation && controller.velocity.y > 0 && !releasedJump && !GameManager.cutsceneMode)
            {
                releasedJump = true;
                directionalMomentum.y *= 0.5f;
                //Vector3 newVel = controller.velocity;
                //newVel.y = newVel.y * 0.5f;
                //controller.velocity = newVel;
            }


            // Slope movement
            Vector3 slopedMomentum = Vector3.Scale(directionalMomentum, groundNormal + new Vector3(1f, 0f, 1f));


            // Wall sliding stuff (I think this needs to go before committing movement because the collision is processed during movement)
            Transform modelCenter = transform.Find("modelCentered");
            modelCenter.localRotation = Quaternion.Lerp(modelCenter.localRotation, Quaternion.Euler(0f, 100f, 0f), 0.5f);
            if (!wallSliding)
            {
                modelCenter.localRotation = Quaternion.identity;
                wallSlidingTime = 0f;
            }
            else
            {
                print("WALL NORMAL:" + wallNormal.ToString());
            }
            wallSliding = false;


            // Commit movement
            if (groundedCompensation)
            {
                controller.SimpleMove(directionalMomentum * 50f);
                //controller.Move(slopedMomentum);
            }
            else
                controller.Move(directionalMomentum);

            // Animation when the player is on the ground and not walking
            if (moveDir.magnitude <= 0 && groundedCompensation)
            {
                if (ehhhImWalkinHere)
                {
                    ehhhImWalkinHere = false;
                    SetAnimState("standing", 0.25f);
                }
                animator.speed = 1;
            }

            // Restart at the last checkpoint position if fallen off the level
            if (transform.position.y < -20)
                transform.position = startPos;
        }
    }



    public override void ReceiveBounce(CollideDir side, CollidingEntity otherScr, Transform otherTrans, Vector3 point, Vector3 normal)
    {
        base.ReceiveBounce(side, otherScr, otherTrans, point, normal);

        if (directionalMomentum.y < 0)
            StartCoroutine(BounceRoutine());
    }

    public override void ReceiveHarm(CollideDir side, CollidingEntity otherScr, Transform otherTrans, Vector3 point, Vector3 normal)
    {
        if (health != null && !hurtPause)
        {
            if (health.vulnerable && health.mercyCountdown <= 0)
            {
                base.ReceiveHarm(side, otherScr, otherTrans, point, normal);
                StartCoroutine(HurtAnim());
            }
        }
    }


    public override void ReceiveBlock(CollideDir side, CollidingEntity otherScr, Transform otherTrans, Vector3 point, Vector3 normal)
    {
        base.ReceiveBlock(side, otherScr, otherTrans, point, normal);

        if (lastTouchedNormal != Vector3.zero) {
            if (Vector3.Angle(normal, lastTouchedNormal) <= 90.5f) return;
        }

        if (side == CollideDir.Front)
        {
            wallNormal = normal;
            wallPoint = point;

            if (!groundedCompensation && groundDistance > 0.2f && directionalMomentum.y < 0f)
            {
                wallSliding = true;

                wallSlidingCounter = (wallSlidingCounter + 1) % 6;
                if (wallSlidingCounter == 0)
                    GameObject.Instantiate(wallSlideParticle, point, Quaternion.identity);

                wallSlidingTime += Time.deltaTime;
                //print("sliding down wall");
                directionalMomentum.y = Mathf.Max(Mathf.Lerp(0f, -0.3f, Mathf.InverseLerp(0f, 1f, wallSlidingTime)), directionalMomentum.y);
                directionalMomentum.x = 0;
                directionalMomentum.z = 0;
            }
        }
    }
}
