using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthController : MonoBehaviour {

    public int currHealth;
    public int maxHealth = 100;

	void Start () {
        currHealth = maxHealth;
        Mathf.Clamp(currHealth, 0, maxHealth);
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
}
