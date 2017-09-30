using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoesDamage : MonoBehaviour {

    public int damage;
    public float knockBackAmt;
    // Use this for initialization

    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter2D(Collider2D col)
    {
        Player player = col.gameObject.GetComponentInParent<Player>();
        if (player != null)
        {
            player.takeDamage(damage, knockBackAmt);
        }
    }

}
