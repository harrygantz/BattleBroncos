using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Prime31;

public class MovementController : MonoBehaviour
{
    // movement config
    public float gravity = -25f;
    public float walkSpeed;
    public float chargeAcceleration;
    public float decceleration;
    public float airAcceleration;
    public float maxSpeed;
    public float airSpeed;
    public float groundDamping = 20f; // how fast do we change direction? higher means faster
    public float inAirDamping = 5f;
    public float jumpHeight = 3f;
    public float wallJumpAngle;
    public float baseWallJumpSpeed;
    public float bouncinessFactor = 1f;
    public bool isStickingToLance;

    public string JumpButton = "Jump_P1";
    public string DashButton = "Dash_P1";
    public string HorizontalControl = "Horizontal_P1";
    public string VerticalControl = "Vertical_P1";
    public string ChargeAxis;
    public string DebugButton;

    public Vector3 _velocity;

    public bool isBeingKnockedBack;
    public bool isBouncingOff;
    public bool isDashing;
    public bool isCoolingDownDash;
    public bool shouldDeccellerate;
    public bool shouldJump;
    public bool shouldWallJump;
    public bool shouldCharge;
    public bool shouldMoveRight;
    public bool shouldMoveLeft;
    public bool shouldFallThroughOneWay;
    public bool shouldFastFall;
    public bool shouldDash;
    public bool shouldSlideDownWall;
    public bool shouldResetXVelocity;
    public bool shouldApplyGravity;
    public bool shouldResetYVelocity;
    public bool shouldStickToWall;
    public bool hasStartedRunning;
    public bool canDoubleJump;
    public bool preventMovement;
    public bool preventFastFall;
    public bool preventLeftRight;
    public bool joystickInNeutral;
    public bool collidingWithCeiling;
    public bool collidingWithWall;
    public bool collidingWithSticky;
    public bool ifDidLerp;

    private int jumps = 0;

    public float rotateAngle;
    public float normalizedHorizontalSpeed = 0;
    public float currentSpeed;
    public float holdXVelocityForWallJump;
    public float everyFiveFrames;
    private float lanceAngle;

    //Dash Variables//
    public DashState dashState;
    public GameObject dashHitBox;
    public float dashTimer;
    public float dashVelocityX = 50.0f;
    public float dashVelocityY = 40.0f;
    public int activeDashFrames = 10;
    public int dashCooldownFrames = 60;
    private Vector3 _savedVelocity;
    private Vector3 _dash;
    private Coroutine dashingRoutine;
    //End Dash Variables//

    private CharacterController2D _controller;
    private Animator _animator;
    private RaycastHit2D _lastControllerColliderHit;
    private Player _player;
    private Transform playerTransform;
    private LanceRotation _lanceRotation;
    private CameraManager _cameraManager;

    private Vector3 velocityLastFrame;
    private Vector3 velocityFiveFramesAgo;
    private Vector3 reflectedVelocity;

    // Dash enumerator    
    public enum DashState
    {
        Ready,
        Dashing,
        Cooldown
    }
    // END Dash enumerator

    private List<float> list;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController2D>();
        _player = GetComponent<Player>();
        _lanceRotation = transform.Find("Lance").GetComponent<LanceRotation>();
        _cameraManager = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraManager>();

        shouldJump = false;
        shouldApplyGravity = true;
        list = new List<float>();
        everyFiveFrames = 0;
        // listen to some events for illustration purposes
        _controller.onControllerCollidedEvent += onControllerCollider;
        _controller.onTriggerStayEvent += onTriggerStayEvent;
        _controller.onTriggerEnterEvent += onTriggerEnterEvent;
        _controller.onTriggerExitEvent += onTriggerExitEvent;
    }

    #region Event Listeners

    void onControllerCollider(RaycastHit2D hit)
    {
        // bail out on plain old ground hits cause they arent very interesting
        if (isBeingKnockedBack || _controller.collisionState.above)
        {
            reflectedVelocity = Bounce(new Vector2(_velocity.x, _velocity.y), hit.normal);
            isBouncingOff = true;
        }

        if (hit.normal.y == 1f)
            return;

        if (hit.transform.tag == "Wall" && !_controller.collisionState.above) //Using this we can't detect when we leave the wall but we want to apply checks when our player actually touches the wall
        {
            currentSpeed = 0;
            if (((_controller.collisionState.right && transform.localScale.x == 1) || (_controller.collisionState.left && transform.localScale.x == -1)) && !_controller.isGrounded) //player is facing wall
            {
                Turnaround(true);
            }
        }


        // logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
        //Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
    }

    void onTriggerEnterEvent(Collider2D col)
    {
        if (col.transform.tag == "Wall" && !_controller.collisionState.above)
        {
            if (isStickingToLance)
                GameMaster.KillPlayer(GetComponent<Player>());

            collidingWithWall = _controller.collisionState.right || _controller.collisionState.left;
            collidingWithCeiling = _controller.collisionState.above;
            if (Mathf.Abs(velocityLastFrame.x) > Mathf.Abs(holdXVelocityForWallJump))
            {
                holdXVelocityForWallJump = Mathf.Sqrt((velocityLastFrame.x * velocityLastFrame.x) + (velocityLastFrame.y * velocityLastFrame.y));
            }
            else if (Mathf.Abs(velocityFiveFramesAgo.x) > Mathf.Abs(holdXVelocityForWallJump))
            {
                holdXVelocityForWallJump = Mathf.Sqrt((velocityFiveFramesAgo.x * velocityFiveFramesAgo.x) + (velocityFiveFramesAgo.y * velocityFiveFramesAgo.y));
            }

            if (!_controller.isGrounded)
            {
                //_player.preventTurnaround = true;
            }
        }
    }

    void onTriggerExitEvent(Collider2D col)
    {
        if (col.transform.tag == "Wall")
        {
            collidingWithWall = (_controller.collisionState.right || _controller.collisionState.left);
            collidingWithCeiling = _controller.collisionState.above;
        }
        //_player.preventTurnaround = false;
    }

    void onTriggerStayEvent(Collider2D col)
    {
        if (col.transform.tag == "Wall")
        {
            //collidingWithWall = collidingWithWall; //maintain value set by above
            //_player.preventTurnaround = true;
        }
        if (col.transform.tag == "Ceiling")
        {
            //collidingWithCeiling = collidingWithCeiling;
        }
    }
    #endregion

    void Update()
    {
        collidingWithSticky = (collidingWithWall || collidingWithCeiling);
        //Reset Vars for isGrounded
        if (_controller.isGrounded)
        {
            canDoubleJump = true;
            shouldFastFall = false;
            _animator.SetBool("canDoubleJump", true);
            _animator.SetBool("playerJumping", false);
            _animator.SetBool("isGrounded", true);
            _animator.SetInteger("jumpCount", jumps = 0);
        }
        else
        {
            _animator.SetBool("isGrounded", false);
        }

        //Save Velocity for wall jump
        if (collidingWithSticky && (Mathf.Abs(velocityLastFrame.x) > Mathf.Abs(holdXVelocityForWallJump)))
        {
            holdXVelocityForWallJump = Mathf.Abs(velocityLastFrame.x);
        }

        #region Input
        if (collidingWithCeiling)
        {
            float joystickAngle = GetJoystickAngle() * Mathf.Rad2Deg;
            if (joystickAngle > 0 && joystickAngle <= 90)
                lanceAngle = -25;
            else if (joystickAngle > 90 && joystickAngle <= 180)
                lanceAngle = -165;
            else if (joystickAngle >= -90)
                lanceAngle = Mathf.Clamp(joystickAngle, -90, -25);
            else if (joystickAngle >= -180 && joystickAngle <= -90)
                lanceAngle = Mathf.Clamp(joystickAngle, -165, -90);
            _lanceRotation.RotateLance(lanceAngle);
        }
        else
        {
            float joystickAngle = GetJoystickAngle() * Mathf.Rad2Deg;
            if (transform.localScale.x == 1)
            {
                if (joystickAngle >= 0 && joystickAngle <= 90)
                    lanceAngle = Mathf.Clamp(joystickAngle, 0, 65);
                else if (joystickAngle < 0 && joystickAngle >= -90)
                    lanceAngle = Mathf.Clamp(joystickAngle, -65, 0);
            }
            else
            {
                if (joystickAngle > 90 && joystickAngle <= 180)
                    lanceAngle = Mathf.Clamp(joystickAngle, 115, 180);
                else if (joystickAngle >= -180 && joystickAngle <= -90)
                    lanceAngle = Mathf.Clamp(joystickAngle, -180, -115);
            }
            _lanceRotation.RotateLance(lanceAngle);
        }
        if (!_player.preventInput)
        {
            if (!_controller.isGrounded && collidingWithSticky && isHoldingCharge())
                shouldStickToWall = true;
            else
                shouldStickToWall = false;

            //Left Stick
            if (Input.GetButtonDown(DashButton) && !isCoolingDownDash)
            {
                shouldDash = true;
            }
            else if (isHoldingRight() && !preventLeftRight && !shouldStickToWall) //right
            {
                isBeingKnockedBack = false; //If the player gets out of hitstun while in air they should continue moving until they input
                shouldMoveRight = true;
                if (_controller.isGrounded)
                    _animator.SetBool("playerWalking", true);
            }
            else if (isHoldingLeft() && !preventLeftRight && !shouldStickToWall) //left
            {
                isBeingKnockedBack = false; //If the player gets out of hitstun while in air they should continue moving until they input
                shouldMoveLeft = true;
                if (_controller.isGrounded)
                    _animator.SetBool("playerWalking", true);
            }
            else
            {
                isBeingKnockedBack = false;
                if (_controller.isGrounded)
                    _animator.SetBool("playerWalking", false);
            }

            if (holdingTowardsWall() && !_controller.isGrounded)
            {
                shouldSlideDownWall = true;
                _animator.SetBool("playerJumping", false);
                _animator.SetBool("playerWalking", false);
            }
            else
            {
                shouldSlideDownWall = false;
            }

            if (isHoldingDown()) //down
            {
                if (_controller.isGrounded)
                {
                    shouldDeccellerate = true;
                }
                else if (!preventFastFall && _velocity.y > -1 && _velocity.y < -1)
                {
                    shouldFastFall = true;
                }
            }

            //Charge
            if (isHoldingCharge())
            {
                shouldCharge = true;
            }
            else
            {
                shouldCharge = false;
                holdXVelocityForWallJump = 0f;
            }

            //Jump
            if (!_controller.isGrounded && collidingWithSticky && isPressingDownJump()) //wall jump
            {
                shouldWallJump = true;
                canDoubleJump = true;
                _animator.SetBool("canDoubleJump", true);
                _animator.SetBool("playerJumping", true);
            }
            else if (isPressingDownJump() && canDoubleJump) //regular jump
            {
                shouldJump = true;
                _animator.SetBool("canDoubleJump", true);
                if (!_controller.isGrounded)
                {
                    canDoubleJump = false;
                    _animator.SetBool("canDoubleJump", false);
                }
                _animator.SetBool("playerJumping", true);
            }
            if (isPressingDownJump())
            {
                _animator.SetInteger("jumpCount", jumps += 1);
            }

            //Charge
            if (isPressingDownDebug())
            {
                //Debug shit in here, this code doesn't matter
                Debug.Log("******************************************");
                string nums = "";
                foreach (float f in list)
                {
                    nums = nums + (f + ", ");
                }

                float i = 0;
                string time = "";
                while (i < list.Count)
                {
                    time = time + (i + ", ");
                    i += 1;
                }
                Debug.Log("[" + nums);
                Debug.Log("[" + time);

                list = new List<float>();
            }

            if (_controller.isGrounded && (Input.GetAxis(VerticalControl) < -0.5) && Input.GetButtonDown(JumpButton))
                shouldFallThroughOneWay = true;
        }
        #endregion

    }

    void FixedUpdate()
    {
        float gravityToUse = gravity;
        float speedToUse;
        float accelerationToUse;

        normalizedHorizontalSpeed = 0;
        var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; //how fast do we change direction?

        //Move Left or Right
        if (_controller.isGrounded)
        {
            speedToUse = walkSpeed;
            accelerationToUse = chargeAcceleration;
        }
        else
        {
            speedToUse = airSpeed;
            accelerationToUse = airAcceleration;
        }

        // Dash ability 
        bool didWallJump = false;
        if (shouldDash)
        {
            switch (dashState)
            {
                case DashState.Ready:
                    {
                        // gravity = 0;
                        _savedVelocity = _velocity;
                        _dash = new Vector3(Input.GetAxis(HorizontalControl) * dashVelocityX, -Input.GetAxis(VerticalControl) * dashVelocityY);
                        if (LanceFacingWall())
                            Turnaround();
                        _velocity = _dash;
                        dashState = DashState.Dashing;
                    }
                    break;
                case DashState.Dashing:
                    if (!isDashing)
                        dashingRoutine = StartCoroutine(Dashing(activeDashFrames));
                    break;
                case DashState.Cooldown:
                    if (!isCoolingDownDash)
                        StartCoroutine(CooldownDash(dashCooldownFrames));
                    break;
            } // END dash ability
        }
        else
        {
            if (shouldMoveRight)
            {
                normalizedHorizontalSpeed = 1;
                if (transform.localScale.x < 0f && _controller.isGrounded)
                {
                    Turnaround();
                }
                else if (shouldCharge)
                {
                    currentSpeed = _velocity.x += accelerationToUse * Time.deltaTime;
                }
                currentSpeed = Mathf.Clamp(currentSpeed, walkSpeed, maxSpeed);
                shouldMoveRight = false;
            }
            else if (shouldMoveLeft)
            {
                normalizedHorizontalSpeed = -1;
                if (transform.localScale.x > 0 && _controller.isGrounded)
                {
                    Turnaround();
                }
                else if (shouldCharge)
                {
                    currentSpeed = _velocity.x -= accelerationToUse * Time.deltaTime;
                }
                currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, -walkSpeed);
                shouldMoveLeft = false;
            }
            else
            {
                if (_controller.isGrounded)
                {
                    if (currentSpeed > decceleration * Time.deltaTime)
                        currentSpeed -= decceleration * Time.deltaTime;
                    else if (currentSpeed < -decceleration * Time.deltaTime)
                        currentSpeed += decceleration * Time.deltaTime;
                }
                else
                {
                    //currentSpeed = shouldCharge ? (_velocity.x += transform.localScale.x * chargeAcceleration * Time.deltaTime) : -walkSpeed;
                }
            }

            //Falling
            if (shouldFallThroughOneWay)
            {
                _velocity.y *= 1f;
                _controller.ignoreOneWayPlatformsThisFrame = true;
                shouldFallThroughOneWay = false;
            }

            //Jumping
            if (shouldJump)
            {
                _velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
                if (isBeingKnockedBack)
                {
                    isBeingKnockedBack = false;
                    _velocity.x = 0;
                }
                StartCoroutine(freezeFastFall(10));
                shouldJump = false;
            }

            if (shouldSlideDownWall && _velocity.y < 6f)
            {
                //_velocity.y = -1;
            }
            if (shouldFastFall)
            {
                //gravityToUse = 3f * gravity;
                _velocity.y = 6f * -jumpHeight;
                shouldFastFall = false;
            }
        }

        if (shouldWallJump)
        {
            if (JoystickInNeutral())
            {
                if (transform.localScale.x > 0)
                    lanceAngle = wallJumpAngle;
                else
                    lanceAngle = 180 - wallJumpAngle;
            }
            float horzVelocity = Mathf.Cos(lanceAngle) * baseWallJumpSpeed;
            float vertVelocity = Mathf.Sin(lanceAngle) * baseWallJumpSpeed;

            if (shouldCharge && ((!LanceFacingWall() && collidingWithWall) || collidingWithCeiling))
            {
                lanceAngle *= Mathf.Deg2Rad;
                if (LanceFacingWall() && collidingWithCeiling)
                    Turnaround();
                float wallJumpSpeed;
                if ((holdXVelocityForWallJump > baseWallJumpSpeed && holdXVelocityForWallJump > 0) || (holdXVelocityForWallJump < -baseWallJumpSpeed && holdXVelocityForWallJump < 0))
                {
                    wallJumpSpeed = holdXVelocityForWallJump;
                }
                else
                {
                    wallJumpSpeed = baseWallJumpSpeed;
                }
                horzVelocity = Mathf.Cos(lanceAngle) * wallJumpSpeed * 1.2f;
                vertVelocity = Mathf.Sin(lanceAngle) * wallJumpSpeed * 1.2f;
            }
            else
            {
                if (transform.localScale.x > 0)
                    lanceAngle = wallJumpAngle;
                else
                    lanceAngle = 180 - wallJumpAngle;
                lanceAngle *= Mathf.Deg2Rad;
                horzVelocity = Mathf.Cos(wallJumpAngle * Mathf.Deg2Rad) * baseWallJumpSpeed * transform.localScale.x;
                vertVelocity = Mathf.Sin(wallJumpAngle * Mathf.Deg2Rad) * baseWallJumpSpeed * 1.3f;
            }
            if (horzVelocity == -1)
                horzVelocity = 0;
            if (vertVelocity == -1)
                vertVelocity = 0;
            horzVelocity = horzVelocity < 0 ? Mathf.Ceil(horzVelocity) : Mathf.Floor(horzVelocity);
            vertVelocity = vertVelocity < 0 ? Mathf.Ceil(vertVelocity) : Mathf.Floor(vertVelocity);
            currentSpeed = horzVelocity;
            _velocity.y = vertVelocity;

            StartCoroutine(setHitbox(20, transform.Find("Hitboxes").Find("WallJump").gameObject));
            _player.SetColor(Color.yellow, 20);

            // _player.StopTurnaround(10);
            StartCoroutine(freezeGravity(10));
            StartCoroutine(freezeFastFall(10));
            holdXVelocityForWallJump = 0;
            didWallJump = true;
            shouldApplyGravity = false;
            shouldWallJump = false;
        }
        else if (shouldStickToWall)
        {
            _velocity.x = 0;
            _velocity.y = 0;
            if (isDashing)
            {
                StopCoroutine(dashingRoutine);
                isDashing = false;
                dashState = DashState.Cooldown;
            }
            StartCoroutine(freezeGravity(1));
        }


        //Knockback
        if (isBouncingOff)
        {
            _velocity = reflectedVelocity * bouncinessFactor;
            isBouncingOff = false;
        }
        if (isBeingKnockedBack) //smooth only if not being knocked back
        {
            if (_controller.isGrounded)
                normalizedHorizontalSpeed = 0;
            else
                normalizedHorizontalSpeed = _velocity.x < 0 ? -1 : 1;
        }
        else
        {
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
            if (didWallJump)
            {
                _velocity.x = currentSpeed;
            }
            else if (!isDashing)
            {
                _velocity.x = Mathf.Lerp(_velocity.x, currentSpeed, Time.deltaTime * smoothedMovementFactor);
            }
        }


        if (shouldApplyGravity && !isDashing)
            _velocity.y += gravityToUse * Time.deltaTime;
        float num;
        if (_velocity.x > 0)
            num = (((_velocity.x + (5 / 2)) / 5) * 5);
        else
            num = (((_velocity.x - (5.0f / 2.0f)) / 5.0f) * 5.0f);

        if (everyFiveFrames % 5 == 0)
        {
            velocityFiveFramesAgo = _velocity;
        }

        if (collidingWithCeiling && !didWallJump && !isBeingKnockedBack)
            _velocity.x = 0;
        _controller.move(_velocity * Time.deltaTime);
        _velocity = _controller.velocity;
        velocityLastFrame = _velocity;
        //shouldApplyGravity = true;
        didWallJump = false;

    }

    public void resetVelocity()
    {
        _velocity = new Vector3(0, 0, 0);
    }

    public void knockBack(Vector3 knockBackAmt)
    {
        int normalX = knockBackAmt.x < 0 ? -1 : 1;
        int normalY = knockBackAmt.y < 0 ? -1 : 1;
        knockBackAmt = new Vector3(
            Mathf.Sqrt(Mathf.Abs(knockBackAmt.x) * 10) * normalX,
            Mathf.Sqrt(Mathf.Abs(knockBackAmt.y) * 10) * normalY,
            0
        );
        //list.Add(knockBackAmt.y);
        isBeingKnockedBack = true;
        _velocity = knockBackAmt;
        _player.SetColor(Color.red, _player.hitStunFrames);
        StartCoroutine(Impact(3));
        _cameraManager.ShakeCamera(3);
    }

    public Vector2 Bounce(Vector2 velocity, Vector2 normal)
    {
        return (-2 * normal * Vector2.Dot(velocity, normal) + velocity);
    }

    IEnumerator Impact(int frames)
    {
        Time.timeScale = 0;
        for (int i = 0; i < frames; i++)
            yield return new WaitForEndOfFrame();
        Time.timeScale = 1;
    }

    IEnumerator freezeLeftRight(int time)
    {
        preventLeftRight = true;
        for (int i = 0; i < time; i++)
            yield return new WaitForEndOfFrame();
        preventLeftRight = false;
    }

    IEnumerator freezeGravity(int frames)
    {
        shouldApplyGravity = false;
        for (int i = 0; i < frames; i++)
            yield return new WaitForEndOfFrame();
        shouldApplyGravity = true;
    }

    IEnumerator freezeFastFall(int frames)
    {
        preventFastFall = true;
        for (int i = 0; i < frames; i++)
            yield return new WaitForEndOfFrame();
        preventFastFall = false;
    }

    IEnumerator stickToWall(int frames)
    {
        preventLeftRight = true;
        for (int i = 0; i < frames; i++)
            yield return new WaitForEndOfFrame();
        preventLeftRight = false;
    }

    IEnumerator setHitbox(int frames, GameObject hitbox)
    {
        hitbox.SetActive(true);
        for (int i = 0; i < frames; i++)
            yield return new WaitForEndOfFrame();
        hitbox.SetActive(false);
    }

    IEnumerator Dashing(int frames)
    {
        isDashing = true;
        dashHitBox.SetActive(true);
        _player.SetColor(Color.yellow, frames);
        for (int i = 0; i < frames; i++)
        {
            yield return new WaitForEndOfFrame();
        }
        dashState = DashState.Cooldown;
        isDashing = false;
    }

    IEnumerator CooldownDash(int frames)
    {
        isCoolingDownDash = true;
        shouldDash = false;
        dashHitBox.SetActive(false);
        for (int i = 0; i < frames; i++)
            yield return new WaitForEndOfFrame();
        dashState = DashState.Ready;
        isCoolingDownDash = false;
    }

    private bool holdingTowardsWall()
    {
        if (transform.localScale.x > 0)
            return (collidingWithWall && isHoldingLeft(-0.01));
        else
            return (collidingWithWall && isHoldingRight(0.01));
    }

    private bool LanceFacingWall()
    {
        return ((transform.localScale.x < 0 &&
               ((lanceAngle >= 0 && lanceAngle < 90) || (lanceAngle > -90 && lanceAngle < 0))) ||
               (transform.localScale.x > 0 &&
               ((lanceAngle > 90 && lanceAngle < 180) || (lanceAngle < -90 && lanceAngle > -180))
        ));
    }

    private bool movingFowards()
    {
        //fowards is relative to the direction the player is facing
        return (
            (transform.localScale.x > 0 && _velocity.x > 0) ||
            (transform.localScale.x < 0 && _velocity.x < 0)
        );
    }

    private bool isHoldingRight(double limit = 0.5)
    {
        if (_player.useKeyboard)
        {
            return Input.GetKey("right");
        }
        else
        {
            return Input.GetAxis(HorizontalControl) > limit;
        }
    }

    private bool isHoldingLeft(double limit = -0.5)
    {
        if (_player.useKeyboard)
        {
            return Input.GetKey("left");
        }
        else
        {
            return Input.GetAxis(HorizontalControl) < limit;
        }
    }

    private bool isHoldingDown()
    {
        if (_player.useKeyboard)
        {
            return Input.GetKey("down");
        }
        else
        {
            return Input.GetAxis(VerticalControl) > 0.5;
        }
    }

    private bool isHoldingCharge()
    {
        if (_player.useKeyboard)
        {
            return Input.GetKey(KeyCode.LeftShift);
        }
        else
        {
            return Input.GetAxis(ChargeAxis) < -0.5;
        }
    }

    private bool isPressingDownJump()
    {
        if (_player.useKeyboard)
        {
            return (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown("up"));
        }
        else
        {
            return Input.GetButtonDown(JumpButton);
        }
    }

    private bool isPressingDownDebug()
    {
        if (_player.useKeyboard)
        {
            return Input.GetKeyDown(KeyCode.Tab);
        }
        else
        {
            return Input.GetButtonDown(DebugButton);
        }
    }


    private float GetJoystickAngle()
    {
        return Mathf.Atan2(-Input.GetAxisRaw(VerticalControl), Input.GetAxisRaw(HorizontalControl));
    }

    private bool JoystickInNeutral()
    {
        return (Input.GetAxis(HorizontalControl) < 0.05 &&
                Input.GetAxis(HorizontalControl) > -0.05 &&
                Input.GetAxis(VerticalControl) < 0.05 &&
                Input.GetAxis(VerticalControl) > -0.05);
    }

    private void Turnaround(bool ignorePreventTurnaround = false)
    {
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        _lanceRotation.Flip();
    }
}
