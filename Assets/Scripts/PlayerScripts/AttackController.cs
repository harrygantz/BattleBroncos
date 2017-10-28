using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour {

    public string StabButton = "Stab_P1";
    public Animator _animator;
    public GameObject stabHitbox;

    //Private
    private Player _player;

    void Start () {
        _animator = GetComponent<Animator>();
        _player = GetComponent<Player>();
    }
	
	void Update () {
        if (!_player.preventInput)
        {
            if (isPressingDownStab())
            {
                _animator.SetBool("playerStabbing", true);
                StartCoroutine(Stab(2, 8, 5));
            }
            else
                _animator.SetBool("playerStabbing", false);
        }

    }

    private bool isPressingDownStab()
    {
        if (_player.useKeyboard)
        {
            return Input.GetKeyDown(KeyCode.X);
        }
        else
        {
            return Input.GetButtonDown(StabButton);
        }
    }

    IEnumerator Stab(int startFrames, int activeFrames, int endFrames) //Brittle AF
    {
        for (int i = 0; i < startFrames; i++)
            yield return new WaitForEndOfFrame();
        _player.FlashPlayer(Color.blue, activeFrames);
        StartCoroutine(setHitbox(activeFrames, stabHitbox));
        _player.stopInput(endFrames);
    }

    IEnumerator setHitbox(int frames, GameObject hitbox)
    {
        hitbox.SetActive(true);
        for (int i = 0; i < frames; i++)
            yield return new WaitForEndOfFrame();
        hitbox.SetActive(false);
    }

}
