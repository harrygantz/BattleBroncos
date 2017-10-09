using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public Transform cameraHolder;
    public List<Transform> players = new List<Transform>();
    public int speed = 5;

    Vector3 midPoint;

    //For orthographic camera
    public float orthoMin = 4;
    public float orthoMax = 8;

    //For perspective camera
    float targetZ;
    public float zMin = 5;
    public float zMax = 10;

    //Switch camera type
    Camera cam;
    public CameraType cType;
    public enum CameraType
    {
        ortho,
        persp
    }

	
	void Start ()
    {
        cam = Camera.main;
        cameraHolder = cam.transform.parent;
        cType = (cam.orthographic) ? CameraType.ortho : CameraType.persp;
        players.Add(GameObject.FindGameObjectWithTag("Player").transform);
        players.Add(GameObject.FindGameObjectWithTag("Player2").transform);
    }
	
	void FixedUpdate ()
    {
        float distance = Vector3.Distance(players[0].position, players[1].position);
        float half = (distance / 2);

        midPoint = (players[1].position - players[0].position).normalized * half;
        midPoint += players[0].position;
        
        switch(cType)
        {
            case CameraType.ortho:

                cam.orthographicSize = 2 * (half / 2);

                if (cam.orthographicSize > orthoMax)
                    cam.orthographicSize = orthoMax;

                if (cam.orthographicSize < orthoMin)
                    cam.orthographicSize = orthoMin;

                break;
            case CameraType.persp:
                
                    targetZ = -(2 * (half / 2));

                if (Mathf.Abs(targetZ) < Mathf.Abs(zMax))
                    targetZ = zMax;

                if (Mathf.Abs(targetZ) > Mathf.Abs(zMin))
                    targetZ = zMin;

                cam.transform.localPosition = new Vector3(0, 0.5f, targetZ);

                break;
         }

        cameraHolder.transform.position = Vector3.Lerp(cameraHolder.transform.position, midPoint, Time.deltaTime * speed);

        }
    public static CameraManager instance;

    public static CameraManager GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }
}
