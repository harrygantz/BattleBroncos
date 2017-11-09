using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class Player : MonoBehaviour {

    public int playerIndex;
    public int invulnFramesOnHit;
    public int hitStunFrames;

    public bool useKeyboard;
    public bool invulerable;

    public string healthUIName;

    public PlayerStats playerStats = new PlayerStats();
    public bool preventInput;
    public bool preventTurnaround;
    [HideInInspector]
    //Private

    private bool shouldStickToLance;

    private Player _otherPlayer;
    private Color spriteColor;

    private GameOver gameOverScreen;
    private StockManager stockManager;
    private Level thisLevel;
    private CharacterController2D _controller;
    private MovementController _movement;
    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        playerIndex = 0;
        stockManager = GetComponent<StockManager>();
    }

    void Awake()
    {
        invulerable = false;
        preventInput = false;
        thisLevel = GameObject.FindGameObjectsWithTag("Level")[0].GetComponent<Level>();

        _controller = GetComponent<CharacterController2D>();
        _movement = GetComponent<MovementController>();
        _spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();

        spriteColor = _spriteRenderer.color;
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
        if (stockManager.GetCurrentStocks() <= 0)
        {
            gameObject.SetActive(false);
            GameMaster.MinusPlayerCount();
        }

    }

    public void takeDamage(int damage, Vector3 knockBackAmt, GameObject lance, bool shouldStick)
    {
        if (!invulerable)
        {
            playerStats.health += damage;
            hitStunFrames = Mathf.RoundToInt(playerStats.health/3 + 10);
            stopInput(hitStunFrames);
            StartCoroutine(setInvulnerable(10));
                
            if (shouldStick && playerStats.health >= 150)
                StartCoroutine(stickToLance(40, lance));
            else
                _movement.knockBack(0.045f * playerStats.health * knockBackAmt);
        }
    }

    public Color getSpriteColor()
    {
        return spriteColor;
    }

    public Color getLanceColor()
    {
        return transform.Find("Lance").GetComponent<SpriteRenderer>().color;
    }

    public void stopInput(int time)
    {
        StartCoroutine(freezeInput(time));
    }

    public void FlashPlayer(Color color, int frames)
    {
        StartCoroutine(Flash(color, frames));
    }

    public void SetColor(Color color, int frames)
    {
        StartCoroutine(StayColor(color, frames));
    }

    public void StopTurnaround(int frames)
    {
        StartCoroutine(freezeTurnaround(frames));
    }

    IEnumerator setInvulnerable(int frames)
    {
        invulerable = true;
        for (int i = 0; i < frames; i++)
            yield return new WaitForEndOfFrame();
        invulerable = false;
    }

    IEnumerator freezeInput(int time)
    {
        preventInput = true;
        for(int i = 0; i < time; i++)
            yield return new WaitForEndOfFrame();
        preventInput = false;
    }

    IEnumerator Flash(Color color, int frames)
    {
        for (int i = 0; i < frames / 2; i++)
        {
            _spriteRenderer.color = color;
            yield return new WaitForEndOfFrame();
            _spriteRenderer.color = spriteColor;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator StayColor(Color color, int frames)
    {
        _spriteRenderer.color = color;
        for (int i = 0; i < frames; i++)
        {
            yield return new WaitForEndOfFrame();
        }
        _spriteRenderer.color = spriteColor;
    }

    IEnumerator freezeTurnaround(int frames)
    {
        preventTurnaround = true;
        for (int i = 0; i < frames; i++)
        {
            yield return new WaitForEndOfFrame();
        }
        preventTurnaround = false;
    }

    IEnumerator stickToLance(int frames, GameObject lance)
    {
        _movement.isStickingToLance = true;
        Vector3 newLocalScale = new Vector3(lance.transform.parent.parent.localScale.x, transform.localScale.y, transform.localScale.z);
        transform.localScale = newLocalScale;
        for (int i = 0; i < frames; i++)
        {
            for (int j = 0; j < 50; j++)
            {
                transform.position = new Vector3(lance.transform.position.x + (1.5f * newLocalScale.x), lance.transform.position.y, lance.transform.position.z);
            }
            yield return new WaitForEndOfFrame();
        }
        _movement.isStickingToLance = false;
    }

    void OnDisable()
    {
        invulerable = false;
        preventInput = false;
        playerStats.health = 0;
        _movement.isStickingToLance = false;
        _spriteRenderer.color = spriteColor;
        _movement.resetVelocity();
    }

    [System.Serializable]
    public class PlayerStats
    {
        public int health = 0;
    }

}
