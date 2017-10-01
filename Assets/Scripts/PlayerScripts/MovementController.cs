using UnityEngine;
using System.Collections;
using Prime31;


public class MovementController : MonoBehaviour
{
	// movement config
	public float gravity = -25f;
	public float runSpeed = 8f;
	public float groundDamping = 20f; // how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
	public float jumpHeight = 3f;
    //FixedUpdate Update bools
    private bool shouldJump;
    private bool shouldMoveRight;
    private bool shouldMoveLeft;
    private bool shouldFallThroughOneWay;
    private bool shouldResetVelocityX;
    private bool shouldResetVelocityY;
    private bool preventMovement;
	[HideInInspector]

	private float normalizedHorizontalSpeed = 0;
	private CharacterController2D _controller;
	private Animator _animator;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector3 _velocity;
    private Player _player;



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
                shouldResetVelocityY = true;
                _animator.SetBool("playerJumping", false);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                shouldMoveRight = true;
                if (_controller.isGrounded)
                    _animator.SetBool("playerWalking", true);
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                shouldMoveLeft = true;
                if (_controller.isGrounded)
                    _animator.SetBool("playerWalking", true);
            }
            else
            {
                normalizedHorizontalSpeed = 0;
                if (_controller.isGrounded)
                    _animator.SetBool("playerWalking", false);
            }

            if (_controller.isGrounded && Input.GetKeyDown(KeyCode.UpArrow))
            {
                shouldJump = true;
                _animator.SetBool("playerJumping", true);
            }

            if (_controller.isGrounded && Input.GetKey(KeyCode.DownArrow))
                shouldFallThroughOneWay = true;
        }
	}

    void FixedUpdate()
    {
 
        if (shouldMoveRight)
        {
            normalizedHorizontalSpeed = 1;
            if (transform.localScale.x < 0f)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            shouldMoveRight = false;
        }
        if (shouldMoveLeft)
        {
            shouldMoveLeft = true;
            normalizedHorizontalSpeed = -1;
            if (transform.localScale.x > 0f)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            shouldMoveLeft = false;
        }
        if (shouldJump)
        {
            _velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            shouldJump = false;
        }
        if (shouldFallThroughOneWay)
        {
            _velocity.y *= 3f;
            _controller.ignoreOneWayPlatformsThisFrame = true;
            shouldFallThroughOneWay = false;
        }

        // apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
        var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
        _velocity.x = Mathf.Lerp(_velocity.x, normalizedHorizontalSpeed * runSpeed, Time.deltaTime * smoothedMovementFactor);

        // apply gravity before moving
        _velocity.y += gravity * Time.deltaTime;
        _controller.move(_velocity * Time.deltaTime);

        // grab our current _velocity to use as a base for all calculations
        _velocity = _controller.velocity;
    }

    public void knockBack(float knockBackAmt)
    {
        Vector2 knockback = new Vector2(knockBackAmt, 1.7f);
        _controller.move(knockback);
    }


}
