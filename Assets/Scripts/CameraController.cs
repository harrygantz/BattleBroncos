using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public Transform targetToFollow;
    public bool isBounded;

    public Vector3 minCameraPos;
    public Vector3 maxCameraPos;


	void Start () {

		
	}
	
	// Update is called once per frame
	void Update () {

        if (targetToFollow == null)
            return;

        transform.position = new Vector3(targetToFollow.position.x, targetToFollow.position.y, transform.position.z);

        if (isBounded)
        {
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, minCameraPos.x, maxCameraPos.x),
                Mathf.Clamp(transform.position.y, minCameraPos.y, maxCameraPos.y),
                Mathf.Clamp(transform.position.z, minCameraPos.z, maxCameraPos.z)
            );
        }
	}
}
