using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthController : MonoBehaviour {

    public int currHealth;
    public int maxHealth = 100;
    public int percentage = 0;
    MovementController _movement;
    Player _player;

	void Start () {
        currHealth = maxHealth;
        Mathf.Clamp(currHealth, 0, maxHealth);
        _movement = GetComponent<MovementController>();
        _player = GetComponent<Player>();
	}
	
	void Update () {
		if(currHealth == 0)
        {   
            Die();
        }
	}

    void Die()
    {
        SceneManager.LoadScene(0);
    }

    public void takeDamage(int knockBackAmt, int damage)
    {
        percentage = percentage + damage;
        _movement.knockBack(knockBackAmt);
    }
}
