using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoesDamage : MonoBehaviour
{

    public int damage;
    public float angle;
    public float intensity;
    public bool useVelocity;
    public bool shouldStick;
    private Player _player;
    private MovementController _movement;
    // Use this for initialization

    void Start()
    {
        _movement = GetComponentInParent<MovementController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        _player = GetComponentInParent<Player>(); //hack fix
        Player otherPlayer = col.gameObject.GetComponentInParent<Player>();
        if (otherPlayer != null)
        {
            float knockback = useVelocity ? _movement._velocity.x : intensity;
            float realAngle = angle;
            if (useVelocity)
            {
                if (knockback > -1 && knockback < 1)
                {
                    knockback = 10;
                    realAngle = 70f;
                }
                if (_movement._velocity.y < 0)
                    realAngle = 290;
            }
            float horzVelocity = Mathf.Cos(realAngle * Mathf.Deg2Rad) * Mathf.Abs(knockback);
            float vertVelocity = Mathf.Sin(realAngle * Mathf.Deg2Rad) * Mathf.Abs(knockback);

            if (horzVelocity == -1)
                horzVelocity = 0;
            if (vertVelocity == -1)
                vertVelocity = 0;
            horzVelocity = horzVelocity < 0 ? Mathf.Ceil(horzVelocity) : Mathf.Floor(horzVelocity);
            vertVelocity = vertVelocity < 0 ? Mathf.Ceil(vertVelocity) : Mathf.Floor(vertVelocity);
            Vector3 KnockbackVector = new Vector3(horzVelocity * _player.transform.localScale.x, vertVelocity, 0);
            otherPlayer.takeDamage(damage, KnockbackVector, gameObject, shouldStick);
        }
    }

}
