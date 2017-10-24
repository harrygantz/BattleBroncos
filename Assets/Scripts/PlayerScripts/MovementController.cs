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
    private SpriteRenderer _spriteRenderer;
    private Player _player;
    private Color spriteColor;
    private Transform playerTransform;

    private Vector3 velocityLastFrame;
    private Vector3 reflectedVelocity;

    private List<float> list = new List<float>();

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController2D>();
        _player = GetComponent<Player>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        spriteColor = _spriteRenderer.color;

        shouldJump = false;
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
        else
        if (_controller.collisionState.becameGroundedThisFrame)
        {
        }
        if (hit.normal.y == 1f)
            return;

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
                Debug.Log("velocity saved: " + holdXVelocityForWallJump);
            }
            if (((_controller.collisionState.right && transform.localScale.x == 1) || (_controller.collisionState.left && transform.localScale.x == -1)) && !_controller.isGrounded) //player is facing wall
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }

    }

    void onTriggerExitEvent(Collider2D col)
    {
        if (col.transform.tag == "Wall")
        {
            collidingWithWall = false;
        }
    }

    void onTriggerStayEvent(Collider2D col)
    {
        if (col.transform.tag == "Wall")
        {
            collidingWithWall = true;
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

        #region Input
        if (!_player.preventInput) 
        {
            //if (collidingWithWall)
            //{
            //    freezeLeftRight(5);
            //}

            //Left Stick
            if (Input.GetAxis(HorizontalControl) > 0.5 && !preventLeftRight) //right
            {
                isBeingKnockedBack = false; //If the player gets out of hitstun while in air they should continue moving until they input
                shouldMoveRight = true;
                if (_controller.isGrounded)
                    _animator.SetBool("playerWalking", true);
            }
            else if (Input.GetAxis(HorizontalControl) < -0.5 && !preventLeftRight) //left
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

            if (Input.GetAxis(VerticalControl) > 0.5) //down
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
            if (Input.GetAxis(ChargeAxis) < -0.5)
            {
                shouldCharge = true;
                transform.Find("Hitboxes").Find("Charge").gameObject.SetActive(true);
            } else
            {
                shouldCharge = false;
                holdXVelocityForWallJump = 0f;
                //transform.Find("Hitboxes").Find("Charge").gameObject.SetActive(false);
            }

            //Jump
            if (!_controller.isGrounded && collidingWithWall && Input.GetButtonDown(JumpButton)) //wall jump
            {
                shouldWallJump = true;
                canDoubleJump = true;
                _animator.SetBool("canDoubleJump", true);
                _animator.SetBool("playerJumping", true);
            }
            else if (Input.GetButtonDown(JumpButton) && canDoubleJump) //regular jump
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
            if (Input.GetButtonDown(JumpButton))
            {
                _animator.SetInteger("jumpCount", jumps += 1);
            }

            //Charge
            if (Input.GetButtonDown(DebugButton))
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
        var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?

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
            if (transform.localScale.x < 0f && (_controller.isGrounded || !shouldCharge))
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
            if (shouldCharge && (_velocity.x += accelerationToUse * Time.deltaTime) > walkSpeed)
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
            if (transform.localScale.x > 0 && (_controller.isGrounded || !shouldCharge) )
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
            if (shouldCharge && (_velocity.x -= accelerationToUse * Time.deltaTime) < -walkSpeed)
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
            float horzVelocity = baseWallJumpSpeed;
            float vertVelocity = Mathf.Sin(wallJumpAngle * Mathf.Deg2Rad) * baseWallJumpSpeed;
            Debug.Log("Velocity used: " + holdXVelocityForWallJump);
            if (holdingTowardsWall())
            {
                if (shouldCharge)//just slide up wall
                {
                    horzVelocity = 0f;
                }
                vertVelocity = Mathf.Sin(wallJumpAngle * Mathf.Deg2Rad) * baseWallJumpSpeed;

            }
            else
            {
                if (shouldCharge && holdXVelocityForWallJump > baseWallJumpSpeed)
                {
                    horzVelocity = holdXVelocityForWallJump * 1.3f;
                    vertVelocity = Mathf.Sin(wallJumpAngle * Mathf.Deg2Rad) * holdXVelocityForWallJump * 1.3f;
                }
            }
            if (horzVelocity == -1)
                horzVelocity = 0;
            if (vertVelocity == -1)
                vertVelocity = 0;

            horzVelocity = horzVelocity < 0 ? Mathf.Ceil(horzVelocity) : Mathf.Floor(horzVelocity);
            vertVelocity = vertVelocity < 0 ? Mathf.Ceil(vertVelocity) : Mathf.Floor(vertVelocity);
            currentSpeed = horzVelocity * transform.localScale.x;
            _velocity.y = vertVelocity * 1.5f;

            freezeLeftRight(5);
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
            //gravityToUse = 6f * gravity;
            _velocity.y = 6f * -jumpHeight;
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
                Debug.Log(currentSpeed);
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
        shouldApplyGravity = true;
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
        StartCoroutine(FlashPlayer(Color.red, _player.hitStunFrames));
    }


    IEnumerator FlashPlayer(Color color, int frames)
    {
        for (int i = 0; i < frames / 2; i++)
        {
            _spriteRenderer.color = color;
            yield return new WaitForEndOfFrame();
            _spriteRenderer.color = spriteColor;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator freezeLeftRight(int time)
    {
        preventLeftRight = true;
        for (int i = 0; i < time; i++)
            yield return new WaitForEndOfFrame();
        preventLeftRight = false;
    }

    private bool holdingTowardsWall()
    {
        return ((_controller.collisionState.right && Input.GetAxis(HorizontalControl) > 0.5) || (_controller.collisionState.left && Input.GetAxis(HorizontalControl) < -0.5));
    }

    private bool movingFowards()
    {
        //fowards is relative to the direction the player is facing
        return (
            (transform.localScale.x > 0 && _velocity.x > 0) ||
            (transform.localScale.x < 0 && _velocity.x < 0)
        );
    }

}