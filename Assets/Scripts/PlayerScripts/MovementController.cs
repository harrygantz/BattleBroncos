using UnityEngine;
using System.Collections;
using Prime31;


public class MovementController : MonoBehaviour
{
	// movement config
	public float gravity = -25f;
    public float walkSpeed;
	public float runSpeed;
    public float airSpeed;
	public float groundDamping = 20f; // how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
    public float jumpHeight = 3f;
    private float framesSpentWalking;



    public string JumpButton = "Jump_P1";
    public string HorizontalControl = "Horizontal_P1";
    public string VerticalControl = "Vertical_P1";
    //FixedUpdate Update bools
    private bool shouldJump;
    private bool hasStartedRunning;
    private bool shouldRun;
    private bool canDoubleJump;
    private bool shouldMoveRight;
    private bool shouldMoveLeft;
    private bool shouldFallThroughOneWay;
    private bool shouldResetVelocityX;
    private bool shouldResetVelocityY;
    private bool isBeingKnockedBack;
    private bool preventMovement;
	[HideInInspector]

	private float normalizedHorizontalSpeed = 0;
	public CharacterController2D _controller;
	public Animator _animator;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector3 _velocity;
    public Player _player;




	void Awake()
	{
		_animator = GetComponent<Animator>();
		_controller = GetComponent<CharacterController2D>();
        _player = GetComponent<Player>();

        shouldJump = false;
		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}

    #region Event Listeners

    void onControllerCollider( RaycastHit2D hit )
	{
		// bail out on plain old ground hits cause they arent very interesting
		if( hit.normal.y == 1f )
			return;

		// logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
		//Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
	}


	void onTriggerEnterEvent( Collider2D col )
	{
	}


	void onTriggerExitEvent( Collider2D col )
	{
	}

	#endregion

	void Update()
	{
        if (!_player.preventInput)
        {
            if (_controller.isGrounded)
            {
                canDoubleJump = true;
                shouldResetVelocityY = true;
                _animator.SetBool("playerJumping", false);
            }
            if (Input.GetAxis(HorizontalControl) > 0.5)
            {
                isBeingKnockedBack = false;
                shouldMoveRight = true;
                if (_controller.isGrounded)
                    _animator.SetBool("playerWalking", true);
            }
            else if (Input.GetAxis(HorizontalControl) < -0.5)
            {
                isBeingKnockedBack = false;
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
            if (Input.GetButtonDown(JumpButton) && canDoubleJump)
            {
                shouldJump = true;
                if (!_controller.isGrounded)
                {
                    canDoubleJump = false;
                }
                _animator.SetBool("playerJumping", true);
            }


            if (_controller.isGrounded && (Input.GetAxis(VerticalControl) < 0.5) && Input.GetButtonDown(JumpButton))
                shouldFallThroughOneWay = true;
        }
	}

    void FixedUpdate()
    {
        float speed;
        if (_controller.isGrounded)
        {
            framesSpentWalking = (_velocity.x < -1 || _velocity.x > 1) ? framesSpentWalking + 1 : 0;
            if (framesSpentWalking > 30)
            {
                shouldRun = true;
                transform.Find("Hitboxes").Find("Charge").gameObject.SetActive(true);
            }
            if (framesSpentWalking == 0)
            {
                shouldRun = false;
                transform.Find("Hitboxes").Find("Charge").gameObject.SetActive(false);
            }
            if (_velocity.x < -runSpeed || _velocity.x > runSpeed)
            {
                hasStartedRunning = true;
            }
            if (_velocity.x > -runSpeed && _velocity.x < runSpeed && hasStartedRunning)
            {
                framesSpentWalking = 0;
                hasStartedRunning = false;
            }
        }
        speed = shouldRun ? runSpeed : walkSpeed;
        if (shouldMoveRight)
        {
            normalizedHorizontalSpeed = 1;
            if (transform.localScale.x < 0f && _controller.isGrounded)
            {
                framesSpentWalking = 0;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }

            shouldMoveRight = false;
        }
        else if (shouldMoveLeft)
        {
            shouldMoveLeft = true;
            normalizedHorizontalSpeed = -1;
            if (transform.localScale.x > 0f && _controller.isGrounded)
            {
                framesSpentWalking = 0;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
            shouldMoveLeft = false;
        }
        else if (isBeingKnockedBack)
        {
            if (_controller.isGrounded)
                normalizedHorizontalSpeed = 0;
            else
                normalizedHorizontalSpeed = _velocity.x < 0 ? -1 : 1;
        }
        else
        {
            normalizedHorizontalSpeed = 0;
        }
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
        if (shouldFallThroughOneWay)
        {
            _velocity.y *= 1f;
            _controller.ignoreOneWayPlatformsThisFrame = true;
            shouldFallThroughOneWay = false;
        }

         
        // apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
        var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
        _velocity.x = Mathf.Lerp(_velocity.x, normalizedHorizontalSpeed * speed, Time.deltaTime * smoothedMovementFactor);

        // apply gravity before moving
        _velocity.y += gravity * Time.deltaTime;
        _controller.move(_velocity * Time.deltaTime);

        // grab our current _velocity to use as a base for all calculations
        _velocity = _controller.velocity;

  
    }

    public void resetVelocity()
    {
        _velocity = new Vector3(0, 0, 0);
    }

    public void movePlayer(float xAcceleration, float yAcceleration, float time)
    {
        //StartCoroutine(accellerateX(xAcceleration, time));
        //StartCoroutine(accellerateY(yAcceleration, time/2));
    }

    //IEnumerator accellerateX(float x, float time)
    //{

    //}

    //IEnumerator accellerateY(float y, float time)
    //{

    //}
    public void knockBack(Vector3 knockBackAmt)
    {
        isBeingKnockedBack = true;
        _velocity = knockBackAmt;
    }


}
