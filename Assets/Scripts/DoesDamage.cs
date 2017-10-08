﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoesDamage : MonoBehaviour {

    public int damage;
    public Vector2 knockBackAmt;
    private Player player;
    // Use this for initialization

    void Start () {
        player = GetComponentInParent<Player>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter2D(Collider2D col)
    {

        Player otherPlayer = col.gameObject.GetComponentInParent<Player>();
        if (otherPlayer != null)
        {
            otherPlayer.takeDamage(damage, new Vector3(knockBackAmt.x * player.transform.localScale.x, knockBackAmt.y, 0));
        }
    }

}
