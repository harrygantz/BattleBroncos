using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class Player : MonoBehaviour {

    private Level thisLevel;
    public PlayerStats playerStats = new PlayerStats();
    public CharacterController2D _controller;
    public MovementController _movement;
    public int playerIndex;
    public bool preventInput;
    public bool invulerable;
    public string healthUIName;
    public float invulnFramesOnHit;


    void Start()
    {
        playerIndex = 0;
    }

    void Awake()
    {
        invulerable = false;
        preventInput = false;
        thisLevel = GameObject.FindGameObjectsWithTag("Level")[0].GetComponent<Level>();

        _controller = GetComponent<CharacterController2D>();
        _controller.onControllerCollidedEvent += onControllerCollider;
        _controller.onTriggerEnterEvent += onTriggerEnterEvent;
        _controller.onTriggerExitEvent += onTriggerExitEvent;
        _movement = GetComponent<MovementController>();

    }

    #region Event Listeners

    void onControllerCollider(RaycastHit2D hit)
    {
        // bail out on plain old ground hits cause they arent very interesting
        if (hit.normal.y == 1f)
            return;

        // logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
        //Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
    }


    void onTriggerEnterEvent(Collider2D col)
    {
    }


    void onTriggerExitEvent(Collider2D col)
    {
    }

    #endregion


    void Update()
    {
        GameMaster.UpdateHealth(healthUIName, playerStats.health);
        if (thisLevel.isBlasted(transform))
        {
            GameMaster.KillPlayer(this);
        }
    }

    public void takeDamage(int damage, Vector3 knockBackAmt)
    {
        if (!invulerable)
        {
            playerStats.health += damage;
            stopInput(1f);
            _movement.knockBack(0.03f * playerStats.health * knockBackAmt);
            StartCoroutine(setInvulnerable(invulnFramesOnHit));
            if (playerStats.health <= 0)
            {
                GameMaster.KillPlayer(this);
            }
        }
        
    }

    public void stopInput(float time)
    {
        StartCoroutine(freezeInput(time));
    }
    IEnumerator setInvulnerable(float invulnFrames)
    {
        invulerable = true;
        yield return new WaitForSeconds(.0166f * invulnFrames);
        invulerable = false;
    }

    IEnumerator freezeInput(float time)
    {
        preventInput = true;
        yield return new WaitForSeconds(time);
        preventInput = false;
    }

    void OnDisable()
    {
        invulerable = false;
        preventInput = false;
        playerStats.health = 0;
        _movement.resetVelocity();
    }

    [System.Serializable]
    public class PlayerStats
    {
        public int health = 0;
        public int lives = 0;
    }
}
