using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour {

    private Animator _animator;

    // Use this for initialization
    void Start () {
        _animator = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
        Debug.Log(_animator.GetBool("playerStabbing"));
        if (Input.GetKey(KeyCode.X))
        {
            _animator.SetBool("playerStabbing", true);
        }
        if (Input.GetKey(KeyCode.Z))
        {
            _animator.SetBool("playerStabbing", false);
        }
    }
}
