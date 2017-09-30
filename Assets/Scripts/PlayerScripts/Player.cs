using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class Player : MonoBehaviour {

    public CameraController mainCamera;
    public Level thisLevel;
    public PlayerStats playerStats = new PlayerStats();
    public int playerIndex;
    public bool takingDamage;
    private bool invulerable;
    public float invulnFramesOnHit;
    private CharacterController2D _controller;


    void Start()
    {
        playerIndex = 0;
        takingDamage = false;
        invulerable = false;
    }

    void Awake()
    {
        _controller = GetComponent<CharacterController2D>();
        _controller.onControllerCollidedEvent += onControllerCollider;
        _controller.onTriggerEnterEvent += onTriggerEnterEvent;
        _controller.onTriggerExitEvent += onTriggerExitEvent;
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
        if (isBlasted())
        {
            GameMaster.KillPlayer(this);
        }
    }

    public void takeDamage(int damage, float knockBackAmt)
    {
        if (!invulerable)
        {
            Debug.Log(invulerable);
            playerStats.health += damage;
            Debug.Log(playerStats.health);
            StartCoroutine(setInvulnerable(invulnFramesOnHit));
            if (playerStats.health <= 0)
            {
                GameMaster.KillPlayer(this);
            }
        }

    }

    IEnumerator setInvulnerable(float invulnFrames)
    {
        invulerable = true;
        Debug.Log(invulerable);

        yield return new WaitForSeconds(.0166f * invulnFrames);
        invulerable = false;
    }

    public bool isBlasted()
    {
        if (mainCamera != null)
        {
            return (
                 transform.position.y <= (mainCamera.minCameraPos.y - thisLevel.yBlastZoneOffset) ||
                 transform.position.y >= (mainCamera.maxCameraPos.y + thisLevel.yBlastZoneOffset) ||
                 transform.position.x <= (mainCamera.minCameraPos.x - thisLevel.xBlastZoneOffset) ||
                 transform.position.x >= (mainCamera.maxCameraPos.x + thisLevel.xBlastZoneOffset)
             );
        }
        return false;
 
    }


    [System.Serializable]
    public class PlayerStats
    {
        public int health = 0;
    }
}
