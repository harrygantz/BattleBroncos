using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour {

    public string Stab = "Stab_P1";
    public Animator _animator;

    // Use this for initialization
    void Start () {
        _animator = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.X) || Input.GetButton(Stab))
        {
            _animator.SetBool("playerStabbing", true);
        }
        else
            _animator.SetBool("playerStabbing", false);

    }

}
