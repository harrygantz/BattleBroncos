using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LanceRotation : MonoBehaviour {
    float currentAngle;
    float angleToRotate;

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        //transform.rotation = Quaternion.AngleAxis(angleToRotate, Vector3.forward);
        //angleToRotate = 0;
	}

    public void RotateLance(float angle)
    {
        transform.rotation = Quaternion.identity;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        //if (angle >= 0 && angle < 180)
        //{
        //    angleToRotate = currentAngle - angle;
        //}
        //else
        //{
        //    angleToRotate = currentAngle + angle;
        //}
        //currentAngle = angle;
    }

    public void Flip()
    {
        //angleToRotate = -currentAngle;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
}
