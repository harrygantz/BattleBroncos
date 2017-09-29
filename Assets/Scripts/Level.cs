using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {

    public float yBlastZoneOffset = 5f;
    public float xBlastZoneOffset = 5f;

    public List<Transform> spawnPoints;
    public Level level;

	void Start () {
        foreach (GameObject spawnPoint in GameObject.FindGameObjectsWithTag("Respawn"))
        {
            if(level == null)
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
}
