using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoesDamage : MonoBehaviour
{

    public int damage;
    public float angle;
    public float intensity;
    private Player player;
    // Use this for initialization

    void Start()
    {
        player = GetComponentInParent<Player>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Player otherPlayer = col.gameObject.GetComponentInParent<Player>();
        if (otherPlayer != null)
        {
            float horzVelocity = Mathf.Cos(angle * Mathf.Deg2Rad) * intensity;
            float vertVelocity = Mathf.Sin(angle * Mathf.Deg2Rad) * intensity;
            if (horzVelocity == -1)
                horzVelocity = 0;
            if (vertVelocity == -1)
                vertVelocity = 0;
            horzVelocity = horzVelocity < 0 ? Mathf.Ceil(horzVelocity) : Mathf.Floor(horzVelocity);
            vertVelocity = vertVelocity < 0 ? Mathf.Ceil(vertVelocity) : Mathf.Floor(vertVelocity);

            otherPlayer.takeDamage(damage, new Vector3(horzVelocity * player.transform.localScale.x, vertVelocity, 0));
        }
    }

}
