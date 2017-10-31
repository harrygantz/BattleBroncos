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

    public string JumpButton = "Jump_P1";
    public string HorizontalControl = "Horizontal_P1";
    public string VerticalControl = "Vertical_P1";
    public string ChargeAxis;
    public string DebugButton;

    public Vector3 _velocity;
    [HideInInspector]

    private bool isBeingKnockedBack;
    private bool isBouncingOff;
    private bool shouldDeccellerate;
    private bool shouldJump;
    private bool shouldWallJump;
    private bool shouldCharge;
    private bool shouldMoveRight;
    private bool shouldMoveLeft;
    private bool shouldFallThroughOneWay;
    private bool shouldFastFall;
    private bool shouldSlideDownWall;
    private bool shouldResetXVelocity;
    private bool shouldApplyGravity;
    private bool shouldResetYVelocity;
    private bool shouldStickToWall;
    private bool hasStartedRunning;
    private bool canDoubleJump;
    private bool preventMovement;
    private bool collidingWithWall;
    private bool preventLeftRight;
    private bool ifDidLerp;

    private int jumps = 0;

    private float rotateAngle;
    private float normalizedHorizontalSpeed = 0;
    private float currentSpeed;
    private float holdXVelocityForWallJump;

    private CharacterController2D _controller;
    private Animator _animator;
    private RaycastHit2D _lastControllerColliderHit;
    private Player _player;
    private Transform playerTransform;
    private LanceRotation _lanceRotation;

    private Vector3 velocityLastFrame;
    private Vector3 reflectedVelocity;

    private List<float> list = new List<float>();

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController2D>();
        _player = GetComponent<Player>();
        _lanceRotation = transform.Find("Lance").GetComponent<LanceRotation>();

        shouldJump = false;
        shouldApplyGravity = true;
        // listen to some events for illustration purposes
        _controller.onControllerCollidedEvent += onControllerCollider;
        _controller.onTriggerEnterEvent += onTriggerEnterEvent;
        _controller.onTriggerExitEvent += onTriggerExitEvent;
        _controller.onTriggerStayEvent += onTriggerStayEvent;
    }

    #region Event Listeners

    void onControllerCollider(RaycastHit2D hit)
    {
        // bail out on plain old ground hits cause they arent very interesting
        if (isBeingKnockedBack)
        {
            Vector2 n = hit.normal;
            Vector2 v = _velocity;
            reflectedVelocity = -2 * n * Vector2.Dot(v, n) + v;
            isBouncingOff = true;
        }
        if (hit.normal.y == 1f)
            return;

        if (hit.transform.tag == "Wall") //Using this we can't detect when we leave the wall but we want to apply checks when our player actually touches the wall
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
        if (col.transform.tag == "Wall") 
        {
            collidingWithWall = _controller.collisionState.right || _controller.collisionState.left;
            if (Mathf.Abs(velocityLastFrame.x) > Mathf.Abs(holdXVelocityForWallJump))
            {
                holdXVelocityForWallJump = Mathf.Abs(velocityLastFrame.x);
            }

            if (!_controller.isGrounded)
            {
                _player.SetColor(Color.grey, 10);
                //_player.preventTurnaround = true;
            }
            shouldStickToWall = true;
        }
    }

    void onTriggerExitEvent(Collider2D col)
    {
        if (col.transform.tag == "Wall")
        {
            collidingWithWall = false;
            //_player.preventTurnaround = false;
        }
    }

    void onTriggerStayEvent(Collider2D col)
    {
        if (col.transform.tag == "Wall")
        {
            collidingWithWall = true;
            //_player.preventTurnaround = true;
        }
    }
    #endregion

    void Update()
    {
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
        if (collidingWithWall && (Mathf.Abs(velocityLastFrame.x) > Mathf.Abs(holdXVelocityForWallJump)))
        {
            holdXVelocityForWallJump = Mathf.Abs(velocityLastFrame.x);
        }

        //Debug.Log(GetJoystickAngle() * Mathf.Rad2Deg);

        #region Input
        _lanceRotation.RotateLance(GetJoystickAngle() * Mathf.Rad2Deg);
        if (!_player.preventInput)
        {
            //Left Stick
            if (isHoldingRight() && !preventLeftRight) //right
            {
                isBeingKnockedBack = false; //If the player gets out of hitstun while in air they should continue moving until they input
                shouldMoveRight = true;
                if (_controller.isGrounded)
                    _animator.SetBool("playerWalking", true);
            }
            else if (isHoldingLeft() && !preventLeftRight) //left
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
                else
                {
                    shouldFastFall = true;
                }
            }

            //Charge
            if (isHoldingCharge())
            {
                shouldCharge = true;
                //transform.Find("Hitboxes").Find("Charge").gameObject.SetActive(true);
            }
            else
            {
                shouldCharge = false;
                holdXVelocityForWallJump = 0f;
                //transform.Find("Hitboxes").Find("Charge").gameObject.SetActive(false);
            }

            //Jump
            if (!_controller.isGrounded && collidingWithWall && isPressingDownJump()) //wall jump
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
                list.Sort();
                foreach (float f in list)
                {
                    Debug.Log(f);
                }
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
        if(_controller.isGrounded) {
            speedToUse = walkSpeed;
            accelerationToUse = chargeAcceleration;
        }
        else
        {
            speedToUse = airSpeed;
            accelerationToUse = airAcceleration;
        }
         
        if (shouldMoveRight)
        {
            normalizedHorizontalSpeed = 1;
            if (transform.localScale.x < 0f && _controller.isGrounded)
            {
                Turnaround();
            }

            if (collidingWithWall && !_controller.isGrounded && shouldStickToWall)
            {
                StartCoroutine(stickToWall(7));
                shouldStickToWall = false;
            }
            else if (shouldCharge && (_velocity.x += accelerationToUse * Time.deltaTime) > walkSpeed)
            {
                currentSpeed = _velocity.x += accelerationToUse * Time.deltaTime;
            }
            else
            {
                currentSpeed = speedToUse;
            }
            shouldMoveRight = false;
        }
        else if (shouldMoveLeft)
        {
            normalizedHorizontalSpeed = -1;
            if (transform.localScale.x > 0 && _controller.isGrounded )
            {
                Turnaround();
            }

            if (collidingWithWall && !_controller.isGrounded && shouldStickToWall)
            {
                StartCoroutine(stickToWall(7));
                shouldStickToWall = false;
            }
            else if (shouldCharge && (_velocity.x -= accelerationToUse * Time.deltaTime) < -walkSpeed)
            {
                currentSpeed = _velocity.x -= accelerationToUse * Time.deltaTime;
            }
            else
            {
                currentSpeed = -speedToUse;
            }
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
        bool didWallJump = false;
        if (shouldJump)
        {
            _velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            if (isBeingKnockedBack)
            {
                isBeingKnockedBack = false;
                _velocity.x = 0;
            }
            shouldJump = false;
        }
        else if (shouldWallJump)
        {
            float angle = GetJoystickAngle() * Mathf.Rad2Deg;
            if ((angle > 0 && angle <= 90) || (angle < 0 && angle >= -90))
                angle = angle / 3;
            else if (angle > 90 && angle <= 180)
            {
                angle = 180 - ((180 - angle) / 2);
            }
            else if (angle > -180 && angle <= -90)
            {
                angle = -180 + ((-180 + angle) / 2);
                angle = -180 + angle;
            }
            angle = angle * Mathf.Deg2Rad;
            if (angle == 0)
                angle = 20 * transform.localScale.x;
            float horzVelocity = Mathf.Cos(angle) * baseWallJumpSpeed;
            float vertVelocity = Mathf.Sin(angle) * baseWallJumpSpeed;
            if (holdingTowardsWall())
            {
                if (shouldCharge)//just slide up wall
                {
                    horzVelocity = 0f;
                }
                horzVelocity = Mathf.Cos(wallJumpAngle * Mathf.Deg2Rad) * baseWallJumpSpeed * transform.localScale.x;
                vertVelocity = Mathf.Sin(wallJumpAngle * Mathf.Deg2Rad) * baseWallJumpSpeed * 1.3f;
            }
            if (shouldCharge && holdXVelocityForWallJump > baseWallJumpSpeed)
            {
                horzVelocity = Mathf.Cos(angle) * holdXVelocityForWallJump * 1.2f;
                vertVelocity = Mathf.Sin(angle) * holdXVelocityForWallJump * 1.2f;
            }
            if (horzVelocity == -1)
                horzVelocity = 0;
            if (vertVelocity == -1)
                vertVelocity = 0;

            horzVelocity = horzVelocity < 0 ? Mathf.Ceil(horzVelocity) : Mathf.Floor(horzVelocity);
            vertVelocity = vertVelocity < 0 ? Mathf.Ceil(vertVelocity) : Mathf.Floor(vertVelocity);
            currentSpeed = horzVelocity;
            _velocity.y = vertVelocity;


           // _player.StopTurnaround(10);
            StartCoroutine(freezeLeftRight(10));
            StartCoroutine(freezeGravity(10));
            StartCoroutine(setHitbox(5, transform.Find("Hitboxes").Find("Charge").gameObject));
            _player.FlashPlayer(Color.blue, 5);
            holdXVelocityForWallJump = 0;
            didWallJump = true;
            shouldApplyGravity = false;
            shouldWallJump = false;
        }
        if (shouldSlideDownWall && _velocity.y < 6f)
        {
            _velocity.y = -1;
        }
        if (shouldFastFall)
        {
            gravityToUse = 3f * gravity;
            //_velocity.y = 6f * -jumpHeight;
            shouldFastFall = false;
        }

        //Knockback
        if (isBeingKnockedBack) //smooth only if not being knocked back
        {
            if (_controller.isGrounded)
                normalizedHorizontalSpeed = 0;
            else
                normalizedHorizontalSpeed = _velocity.x < 0 ? -1 : 1;

            if (isBouncingOff && _player.preventInput)
            {
                _velocity = reflectedVelocity * bouncinessFactor;
                isBouncingOff = false;
            }
        }
        else
        {
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
            if (didWallJump)
            {
                _velocity.x = currentSpeed;
            }
            else
            {
                _velocity.x = Mathf.Lerp(_velocity.x, currentSpeed, Time.deltaTime * smoothedMovementFactor);
            }
        }

        if (shouldApplyGravity)
            _velocity.y += gravityToUse * Time.deltaTime;

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
        list.Add(knockBackAmt.y);
        isBeingKnockedBack = true;
        _velocity = knockBackAmt;
        _player.SetColor(Color.red, _player.hitStunFrames);
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

    private bool holdingTowardsWall()
    {
        return ((_controller.collisionState.right && isHoldingRight()) || (_controller.collisionState.left && isHoldingLeft()));
    }

    private bool movingFowards()
    {
        //fowards is relative to the direction the player is facing
        return (
            (transform.localScale.x > 0 && _velocity.x > 0) ||
            (transform.localScale.x < 0 && _velocity.x < 0)
        );
    }

    private bool isHoldingRight()
    {
        if (_player.useKeyboard)
        {
            return Input.GetKey("right");
        }
        else
        {
            return Input.GetAxis(HorizontalControl) > 0.5;
        }
    }

    private bool isHoldingLeft()
    {
        if (_player.useKeyboard)
        {
            return Input.GetKey("left");
        }
        else
        {
            return Input.GetAxis(HorizontalControl) < -0.5;
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

    private void Turnaround(bool ignorePreventTurnaround = false)
    {
        if (ignorePreventTurnaround)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        _lanceRotation.Flip();
        
    }
}

