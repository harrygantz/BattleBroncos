using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {

    public float yBlastZoneOffset = 5f;
    public float xBlastZoneOffset = 5f;
    public Bounds bounds;

    public List<Transform> spawnPoints;
    public Level level;

	void Start () {
        foreach (GameObject spawnPoint in GameObject.FindGameObjectsWithTag("Respawn"))
        {
            bounds = transform.Find("background").gameObject.GetComponent<Renderer>().bounds;
            if (level == null);
            {
                level = GameObject.FindGameObjectWithTag("Level").GetComponent<Level>();
            }
            if(level.spawnPoints.Count == 0)
            {
                level.spawnPoints.Add(spawnPoint.transform);
            }
        }
	}
	
	void Update () {
		
	}

    public bool isBlasted(Transform t)
    {
        return (
                 t.position.y <= (bounds.center.y - bounds.extents.y - yBlastZoneOffset) ||
                 t.position.y >= (bounds.center.y + bounds.extents.y + yBlastZoneOffset) ||
                 t.position.x <= (bounds.center.x - bounds.extents.x - xBlastZoneOffset) ||
                 t.position.x >= (bounds.center.x + bounds.extents.x + xBlastZoneOffset)
        );

    }
}
