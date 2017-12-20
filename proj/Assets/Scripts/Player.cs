using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DamageState { Vulnerable, Mercy, Hurt, Dying, Dead };
public enum MoveState { Grounded, Airborn };
public enum GroundedState { Standing, Walking, Sliding };
public enum AirbornState { Jumping, Launched, Falling, WallJumping, WallSliding, SlideJumping, KnockedBack };
public enum CarryState { NotCarrying, Pluck, Pickup, Holding, Throwing };

public enum JumpType { Jump, DoubleJump, WallJump, Bop, Spring};


public class Player : CollidingEntity
{
    // Rates -- these should be changed to whatever the inspector values are
    public float walkSpeed = 0.0625f;
    public float runSpeed = 0.125f;
    public float turnRate = 10f;
    public float jumpStrength = 18f;
    public int jumpLimit = 2;
    public float jumpDecayRate = 0.75f;
    public float jumpLenienceSeconds = 0.3f;

    // Sound effects
    public AudioClip[] jumpSounds;
    public AudioClip[] doublejumpSounds;
    public AudioClip[] hurtSounds;

    // Textures for animation and stuff
    public Texture2D normalTexture;
    public Texture2D hurtTexture;

    // Misc
    public float jumpLenienceTimer = -1f;

    [HideInInspector] public bool inputActive = true;

    // Wall collision
    private Vector3 wallNormal;
    private Vector3 wallPoint;

    // Wall jumping
    private bool continueWallSliding = false;
    //private bool wallJumping = false;
    private int wallSlidingCounter = 0;
    private float wallSlidingTime = 0f;
    public LayerMask wallSlideLayerMask;

    private float lastWalljumpHeight = Mathf.Infinity;
    private Vector3 lastWallNormal = Vector3.zero;

    // Sliding-related
    public float slidingDelayTime = 2f;
    private float slidingTimer = 0f;
    private float slideSpeed = 0f;
    private Vector3 slideForwardVector;

    // Particle effects
    public GameObject hurtParticle;
    public GameObject wallSlideParticle;
    public GameObject wallJumpParticle;

    
    // Control flags and variables
    private bool hurtPause = false;

    private float squashAmount = 0;
    public int jumpsPerformed = 0;
    public List<JumpType> jumpsSinceGround = new List<JumpType>();

    //private Vector3 startPos;

    private bool releasedJump = false;
    private bool groundedCompensation = false;
    private bool touchingMovingSurface = false;

    // References
    public Transform model;
    public Transform modelCenter;
    public Animator animator;
    public SquashAndStretch squash;

    // State values
    private MoveState moveState = MoveState.Grounded;

    private List<DamageState> damageStates = new List<DamageState>();
    private List<GroundedState> groundedStates = new List<GroundedState>();
    private List<AirbornState> airbornStates = new List<AirbornState>();
    private List<CarryState> carryStates = new List<CarryState>();

    // Movement-related
    private Vector3 moveDir;
    private float forwardMomentum;
    private Quaternion targetRotation;
    [HideInInspector] public bool walkingUphill;

    // Animation-related
    private string animState;
    private bool lockedAnimState = false;

    // Yikes I need to organize this stuff better
    private Vector3 cameraForward;
    private bool ehhhImWalkinHere;


    public bool InputHasEffect
    {
        get
        {
            return (inputActive && !GameManager.cutsceneMode);
        }
    }


    public override void Start ()
    {
        // Initialize the state lists
        moveState = MoveState.Airborn;
        damageStates.Add(DamageState.Vulnerable);
        groundedStates.Add(GroundedState.Walking);
        airbornStates.Add(AirbornState.Falling);
        carryStates.Add(CarryState.NotCarrying);

        UpdateReferences();
        health.hp = 3 + SaveManager.currentSave.TotalGoldRadishes;
        health.currentHp = LevelManager.playerCurrentHp;// Mathf.Max(Mathf.FloorToInt(health.hp*2f/3f));
        //startPos = transform.position;
    }
    



    // Why do I have Get functions for this class when I just use public variables hidden 
    //   from the inspector with every other class
    //   apologies in advance to hoeloe
    //   oh wait right it's to ensure that the reference is valid, hence the UpdateReferences();
    //   okay yeah this project's codebase needs better reference management overall
    public CharacterController GetCharacterController()
    {
        return controller;
    }

    public Vector3 GetDirMomentum()
    {
        return velocity;
    }

    public bool GetGrounded()
    {
        return (controller.isGrounded && moveState==MoveState.Grounded);
    }



    void SetAnimState (string state, float crossfade)
    {
        if (animState != state && !lockedAnimState)
        {
            animator.CrossFade(state, crossfade);
            animState = state;
        }
    }


    /*
    public override void OnCollisionEnter(Collision collision)
    {

    }
    */

    public override void OnControllerColliderHit(ControllerColliderHit hit)
    {
        base.OnControllerColliderHit(hit);

        // Landing on something
        if (collisionSide == CollideDir.Down && controller.velocity.y < 0)
        {
            if (hit.gameObject.layer == 9 || hit.gameObject.layer == 14)
            {
                LandOnSolidSurface();
            }

            else if (hit.gameObject.layer == 12)
            {
                LandOnMovingSurface(hit.transform.parent);
            }
        }
        //print(contact.thisCollider.name + " hit " + contact.otherCollider.name);
        //Debug.DrawRay(contact.point, contact.normal, Color.white);
    }

    public void LandOnSolidSurface()
    {
        if (!GetGrounded())
        {
            SetAnimState("standing", 0f);
            moveState = MoveState.Grounded;
            squashAmount = 1f;
            jumpsPerformed = 0;
            velocity.y = -0.1f;
            lastWalljumpHeight = Mathf.Infinity;
            lastWallNormal = Vector3.zero;
        }
    }
    public void LandOnMovingSurface(Transform platform)
    {
        if (!GetGrounded())
        {
            transform.SetParent(platform);
            touchingMovingSurface = true;
            LandOnSolidSurface();
        }
    }


    public void PerformDoubleJump()
    {
        // Fancy flip yo
        StartCoroutine("DoubleJumpFlip");

        // Jump the jump the jump jump
        PerformGenericJump(JumpType.DoubleJump, jumpDecayRate * jumpStrength, true, doublejumpSounds);
    }

    public void PerformWallJump()
    {
        // Store info for walljump limits
        lastWallNormal = wallNormal;

        // Perform the jump
        PerformGenericJump(JumpType.WallJump, jumpDecayRate*jumpStrength*0.9f, true);
        
        // Lock the player's input for a bit with a coroutine
        StartCoroutine(WallJumpVelocity());

        // Reenable double-jumping
        if (jumpsSinceGround.Contains(JumpType.DoubleJump))
            jumpsSinceGround.Remove(JumpType.DoubleJump);
    }


    public void PerformGenericJump(JumpType type, float vertStrength = -999f, bool shouldStretch = false, AudioClip[] soundArray = null)
    {
        // Default sound array handling
        if (soundArray == null)
            soundArray = jumpSounds;
        if (vertStrength == -999f)
            vertStrength = jumpStrength;

        // Stretch if necessary
        if (shouldStretch)
            squashAmount = -1.5f;

        // Reset variable jump handling
        releasedJump = false;

        // VERTICAL BOOST
        transform.Translate(Vector3.up * 0.1f);
        velocity.y = jumpStrength * 0.015f;

        moveState = MoveState.Airborn;


        // Bells and whistles
        AudioClip randJumpSound = soundArray[(int)Random.Range(0, soundArray.Length)];
        SetAnimState("jumping", 0.1f);
        PlaySound(randJumpSound, Random.Range(0.8f, 1.2f));

        // State management
        jumpsSinceGround.Add(type);
        airbornStates[0] = AirbornState.Jumping;
    }



    // Update is called once per frame
    public override void UpdateAI()
    {
        // Only update when the game isn't paused
        if (!GameManager.isGamePaused)
        {
            health.invincible = GameManager.cutsceneMode;

            // Get walk/steer input vector
            float h = InputHasEffect ? GameManager.inputVals["Walk X"] : 0;
            float v = InputHasEffect ? GameManager.inputVals["Walk Y"] : 0;

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

            // Reset rotation stuffs
            modelCenter.localRotation = Quaternion.identity;
            targetRotation = moveDir.magnitude > 0 ? Quaternion.LookRotation(moveDir, Vector3.up) : transform.rotation;
            float rotRateMult = 1f;

            // Should shift down flag
            shouldShiftDown = false;

            // STATUS EX MACHINA
            switch (moveState)
            {
                // ----- GROUNDED -----
                case (MoveState.Grounded):

                    // Reset jump stuffs and things and stuffs
                    jumpsSinceGround.Clear();
                    releasedJump = true;


                    // Moving platform stuff
                    if (touchingMovingSurface)
                    {
                        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                    }


                    // Slope and sliding checks
                    float forwardSlopeAngle = Vector3.Angle(moveDir, groundNormal)-90;
                    float backSlopeAngle = Vector3.Angle(-moveDir, groundNormal)-90;
                    float momentumSlopeAngle = Vector3.Angle(velocity, groundNormal)-90;
                    float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);

                    walkingUphill = forwardSlopeAngle > backSlopeAngle;
                    bool slopeTooSteep = (slopeAngle > 45);
                    shouldShiftDown = !walkingUphill;

                    if (momentumSlopeAngle < 0)
                    {
                        //velocity.y = (1f-groundNormal.y) * -50f;
                        // * Mathf.Lerp(0f, 1f-groundNormal.y, Mathf.InverseLerp(0f, slopeAngle, momentumSlopeAngle))*20f;
                    }

                    // Handling for moving over a ledge
                    if (!controller.isGrounded && groundDistance > 1f && Mathf.Abs(controller.velocity.y) > 1f)
                    {
                        moveState = MoveState.Airborn;
                        airbornStates[0] = AirbornState.Falling;
                        jumpLenienceTimer = jumpLenienceSeconds;
                        //print("LEAVING THE GROUND YO");
                    }

                    // Jumping
                    if (GameManager.inputPress["Jump"] && InputHasEffect)
                        PerformGenericJump(JumpType.Jump);

                    // Substate behavior
                    switch (groundedStates[0])
                    {
                        // STANDING, WALKING, RUNNING, ETC.
                        case (GroundedState.Walking):

                            // Standing > walking > running animation
                            if (moveDir.magnitude > 0.5f)
                            {
                                animator.speed = 1.5f;
                                SetAnimState("running", 0.25f);
                            }
                            else if (moveDir.magnitude > 0f)
                            {
                                animator.speed = 1 + moveDir.magnitude * 2;
                                SetAnimState("walking", 0.25f);
                            }
                            else
                            {
                                SetAnimState("standing", 0.25f);
                            }

                            // Reset model center offsets
                            modelCenter.localRotation = Quaternion.identity;
                            modelCenter.localPosition = Vector3.up * 0.75f;

                            // Reset horizontal momentum to steer vector * run speed
                            velocity = Vector3.up*velocity.y + moveDir * runSpeed; //Vector3.forward * forwardMomentum;
                             
                            if (slopeTooSteep)
                            {
                                /*
                                if (walkingUphill)
                                    slidingTimer -= Time.deltaTime;
                                else
                                    slidingTimer = 0f;

                                float tempYVal = velocity.y;

                                velocity = velocity * (slidingDelayTime - slidingTimer);
                                velocity.y = tempYVal;
                                if (slidingTimer <= 0)
                                    */
                                {
                                    velocity = Vector3.zero;
                                    groundedStates[0] = GroundedState.Sliding;
                                    transform.rotation = Quaternion.LookRotation(groundNormal, Vector3.up);
                                    slideSpeed = 0f;
                                }

                                //if (velocity.magnitude += 0.2f * new Vector3(groundNormal.x, 0f, groundNormal.z);
                                //groundedStates[0] = GroundedState.Sliding;
                            }
                            else
                            {
                                slidingTimer = slidingDelayTime;
                            }
                            break;

                        // SLIDING
                        case (GroundedState.Sliding):
                            SetAnimState("flipping", 0.25f);
                            modelCenter.localRotation = Quaternion.Euler(-45, 0, 0);
                            modelCenter.localPosition = Vector3.up * 0.5f;

                            //print("SLIDE SPEED: "+slideSpeed.ToString());

                            if (slopeAngle > 11)
                            {
                                slideSpeed = Mathf.Min(slideSpeed + 0.05f, 0.5f);
                                slideForwardVector = groundNormal;
                            }
                            else
                            {
                                slideSpeed -= 0.01f;

                                if (Mathf.Abs(slideSpeed) <= 0.02f || moveDir.magnitude > 0.1f)
                                {
                                    groundedStates[0] = GroundedState.Walking;
                                }
                            }
                            velocity.x = slideForwardVector.x * slideSpeed;
                            velocity.z = slideForwardVector.z * slideSpeed;

                            Vector3 hDirMom = velocity*1f;
                            hDirMom.y = 0f;
                            targetRotation = Quaternion.LookRotation(hDirMom, Vector3.up);

                            break;
                    }

                    break;


                // ----- AERIAL ASSAULT -----
                case (MoveState.Airborn):

                    // Slower rotation in the air because PHYSICS YO
                    if (moveDir.magnitude > 0)
                        rotRateMult = 0.5f;


                    // Different momentum in the air because PHYSIIIIIICS YOOOOOOOO
                    Vector3 airSteering = moveDir * walkSpeed * 0.135f;
                    Vector2 newHorzVel = Vector2.ClampMagnitude(new Vector2(velocity.x + airSteering.x, velocity.z + airSteering.z), runSpeed);
                    velocity = new Vector3(newHorzVel.x, velocity.y, newHorzVel.y);

                    // LET THERE BE G̮̰͕̻͇̼Ŕ͜͏̤͚̝̼͇͚A̸̶̻̼̼̫͇͟V̨͏̣̘̞̝̜Į̝̗͖̙̭̪ͅT̲̱̥̥̗͡Y̭̘͍͔̗͖͎
                    velocity.y = Mathf.Max(velocity.y - gravityRate*Time.deltaTime*60f, -2f);

                    // Undo moving platform parenting
                    if (touchingMovingSurface)
                    {
                        transform.SetParent(null);
                    }

                    // Jump leniency handling
                    float lastLenienceTimer = jumpLenienceTimer;
                    jumpLenienceTimer = Mathf.Max(-1f, jumpLenienceTimer - Time.deltaTime);
                    if (jumpLenienceTimer <= 0f && lastLenienceTimer > 0f)
                    {
                        jumpsSinceGround.Add(JumpType.Jump);
                        jumpsPerformed = Mathf.Max(jumpsPerformed, 1);
                        //print("Jump leniency over.");
                    }

                    // Variable jumping - dampen jump upon button release
                    if (GameManager.inputRelease["Jump"] && controller.velocity.y > 0 && !releasedJump && InputHasEffect)
                    {
                        releasedJump = true;
                        velocity.y *= 0.5f;
                    }

                    // Change the animation state to jumping or falling
                    string newState = controller.velocity.y > 0 ? "jumping" : "falling";
                    //SetAnimState(newState, 0.5f);


                    // Reset some wall sliding stuff if not wall sliding
                    if (airbornStates[0] != AirbornState.WallSliding)
                    {
                        wallSlidingTime = 0f;
                        wallSlidingCounter = 1;
                    }


                    // Can double jump flag
                    bool canDoubleJump = !jumpsSinceGround.Contains(JumpType.DoubleJump);

                    // Substate behavior
                    switch (airbornStates[0])
                    {
                        case (AirbornState.Falling):
                            break;
                        case (AirbornState.Jumping):
                            break;
                        case (AirbornState.KnockedBack):
                            break;
                        case (AirbornState.Launched):
                            break;
                        case (AirbornState.WallSliding):
                            // Special rotation
                            modelCenter.localRotation = Quaternion.Lerp(modelCenter.localRotation, Quaternion.Euler(0f, 100f, 0f), 0.5f);

                            // Time spent wallsliding
                            wallSlidingTime += Time.deltaTime;

                            // Restrict momentum based on time spent sliding
                            //velocity.x = 0;
                            //velocity.z = 0;
                            velocity.y = Mathf.Max(Mathf.Lerp(0f, -0.3f, Mathf.InverseLerp(0f, 1f, wallSlidingTime)), velocity.y);

                            // Sliding particles
                            wallPoint.y = transform.position.y;
                            wallSlidingCounter = (wallSlidingCounter + 1) % 6;
                            if (wallSlidingCounter == 0)
                                GameObject.Instantiate(wallSlideParticle, wallPoint, Quaternion.identity);

                            // Walljumping
                            canDoubleJump = false;
                            if (GameManager.inputPress["Jump"] && InputHasEffect)
                                PerformWallJump();

                            // Stop sliding if contact with the wall is lost
                            if (continueWallSliding)
                                continueWallSliding = false;
                            else
                                airbornStates[0] = AirbornState.Falling;

                            // Stop sliding if wall not detected
                            /*
                            RaycastHit hit;
                            if (!Physics.Linecast(wallPoint+wallNormal, wallPoint-wallNormal, out hit, wallSlideLayerMask))
                            {
                                airbornStates[0] = AirbornState.Falling;
                            }
                            */
                            break;
                    }


                    // Double jump yo
                    if (GameManager.inputPress["Jump"] && canDoubleJump && InputHasEffect)
                    {
                        if (jumpLenienceTimer > 0f)
                            PerformGenericJump(JumpType.Jump);
                        else
                            PerformDoubleJump();
                    }
                    break;
            }


            // Squash & stretch management
            squashAmount *= 0.75f;
            float squashMult = squashAmount * 0.25f;
            model.localScale = new Vector3(1 + squashMult, 1 - squashMult, 1 + squashMult);

            // Rotate self toward target rotation
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnRate*rotRateMult);


            /*
            if (moveState == MoveState.Grounded)
            {
                controller.SimpleMove((velocity) * 50f * Time.deltaTime);
                if (shouldShiftDown)
                    ShiftToGround();
            }
            else
                controller.Move(velocity * Time.deltaTime);
            */


            // Restart at the last checkpoint position if fallen off the level
            if (transform.position.y < -15 && !health.dead)
            {
                CameraManager.instance.target = null;
                health.Kill();
                StartCoroutine(DeathSequence());
            }
        }
    }


    #region collision events
    public override void ReceiveBounce(CollideDir side, CollidingEntity otherScr, Transform otherTrans, Vector3 point, Vector3 normal, bool refreshDouble, float strength)
    {
        base.ReceiveBounce(side, otherScr, otherTrans, point, normal, refreshDouble, strength);

        if (velocity.y < 0)
            StartCoroutine(BounceRoutine(refreshDouble, strength));
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
        if (Vector3.Angle(normal, lastWallNormal) <= 90.5 && point.y > lastWalljumpHeight) return;

        if (side == CollideDir.Front)
        {
            wallNormal = normal;
            wallPoint = point;

            /*
            continueWallSliding = true;


            if (!GetGrounded() && groundDistance > 0.2f && velocity.y < 0f)
            {
                airbornStates[0] = AirbornState.WallSliding;
                lastWalljumpHeight = point.y;
            }
            */
        }
    }
    #endregion

    #region coroutines
    IEnumerator HurtAnim()
    {
        // Delay by a frame for reasons
        //yield return null;
        UIManager.hpFadeCounter = 0f;

        Material tempMat = gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material;
        tempMat.mainTexture = hurtTexture;

        AudioClip randHurtSound = hurtSounds[(int)Random.Range(0, hurtSounds.Length)];
        PlaySound(randHurtSound, Random.Range(0.8f, 1.2f));

        GameObject.Instantiate(hurtParticle, transform.position + Vector3.up, Quaternion.identity, transform);
        SetAnimState("flipping", 0.25f);
        lockedAnimState = true;
        inputActive = false;

        Transform modelCenter = transform.Find("modelCentered");
        hurtPause = true;

       
        // Black background if the player dies
        if (health.currentHp <= 0)
        {
            Camera.main.cullingMask = 1 << 8;
            Camera.main.clearFlags = CameraClearFlags.Color;
            Camera.main.backgroundColor = Color.black;
        }


        // Momentary freeze to show the player is hurt
        Vector3 tempMomentum = velocity;

        int tempJumpsPerformed = jumpsPerformed;

        float totalTime = 0.35f;
        float timeLeft = totalTime;
        while (timeLeft > 0)
        {
            modelCenter.localRotation = Quaternion.Euler(-22f, 0f, 0f);

            tempMomentum = new Vector3(velocity.x != 0 ? velocity.x : tempMomentum.x,
                                        velocity.y != 0 ? velocity.y : tempMomentum.y,
                                        velocity.z != 0 ? velocity.z : tempMomentum.z);

            velocity = Vector3.zero;
            jumpsPerformed = 99;
            timeLeft -= Time.deltaTime;
            yield return null;
        }
        velocity = tempMomentum;


        // If health remains, give control back and make the player blink
        if (health.currentHp > 0)
        {
            modelCenter.localRotation = Quaternion.identity;
            hurtPause = false;
            inputActive = true;
            lockedAnimState = false;
            jumpsPerformed = tempJumpsPerformed;


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
            tempMat.mainTexture = normalTexture;

            foreach (SkinnedMeshRenderer renderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                renderer.material = tempMat;
            }
        }

        // If the player is dead, run the death coroutine
        else
        {
            StartCoroutine(DeathSequence());
        }
    }
    IEnumerator BounceRoutine(bool refreshDouble, float strength)
    {
        /*
        while (hurtPause)
        {
            yield return null;
        }
        //*/

        transform.Translate(Vector3.up * 0.1f);
        velocity.y = strength * 0.015f;
        controller.Move(velocity);

        moveState = MoveState.Airborn;

        if (refreshDouble)
        {
            releasedJump = true;
            jumpsPerformed = 1;
        }
        yield return null;
    }
    IEnumerator WallJumpVelocity()
    {
        GameObject.Instantiate(wallJumpParticle, wallPoint, Quaternion.identity);
        airbornStates[0] = AirbornState.WallJumping; 
        //wallJumping = true;
        SetAnimState("walljumping", 0f);
        lockedAnimState = true;
        transform.rotation = Quaternion.LookRotation(wallNormal, Vector3.up);
        Vector3 reflectedDir = Vector3.Lerp(wallNormal, Vector3.Reflect(moveDir, wallNormal), 0.5f);
        float timeLeft = 0.3f;
        while (timeLeft > 0 && !GetGrounded())
        {
            targetRotation = Quaternion.LookRotation(velocity, Vector3.up);
            velocity += reflectedDir * 0.25f;
            timeLeft -= Time.deltaTime;
            yield return null;
        }
        lockedAnimState = false;
        airbornStates[0] = AirbornState.Falling;
        //wallJumping = false;
    }
    IEnumerator DoubleJumpFlip()
    {
        SetAnimState("flipping", 0.5f);
        lockedAnimState = true;
        Transform modelCenter = transform.Find("modelCentered");
        float totalTime = 0.5f;
        float timeLeft = totalTime;

        float lastTimeLeft = timeLeft;

        while (timeLeft > 0 && !GetGrounded())
        {
            float timeMult = timeLeft / totalTime;
            float timeMultEased = Mathf.Sqrt(1 - timeMult * timeMult);
            modelCenter.localRotation = Quaternion.Euler(360 * timeMultEased, 0, 0);
            timeLeft -= Time.deltaTime;

            if (timeLeft < 0.5f * totalTime && lastTimeLeft >= 0.5f * totalTime)
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
    IEnumerator DeathSequence()
    {
        AudioManager.StopMusic();
        inputActive = false;
        health.vulnerable = false;
        SetAnimState("flipping", 0.5f);
        lockedAnimState = true;

        SaveManager.currentSave.teethLost += (int)Mathf.Min(10, SaveManager.currentSave.NetTeeth);

        yield return UIManager.instance.StartCoroutine(UIManager.instance.ScreenFadeChange(1f, 0.5f));
        yield return new WaitForSeconds(0.5f);
        LevelManager.playerCurrentHp = health.hp;
        LevelManager.instance.ReloadScene();

        yield return null;
    }
    #endregion
}
