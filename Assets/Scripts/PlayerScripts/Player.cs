using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public CameraController mainCamera;
    public Level thisLevel;
    public PlayerStats playerStats = new PlayerStats();
    public int playerIndex;

    void Start()
    {
        playerIndex = 0;
    }

    void Update()
    {
        if (isBlasted())
        {
            DamagePlayer(9999999);
        }
    }

    public void DamagePlayer(int damage)
    {
        playerStats.health -= damage;
        if (playerStats.health <= 0)
        {
            GameMaster.KillPlayer(this);
        }
    }

    public bool isBlasted()
    {
        return (
            transform.position.y <= (mainCamera.minCameraPos.y - thisLevel.yBlastZoneOffset) ||
            transform.position.y >= (mainCamera.maxCameraPos.y + thisLevel.yBlastZoneOffset) ||
            transform.position.x <= (mainCamera.minCameraPos.x - thisLevel.xBlastZoneOffset) ||
            transform.position.x >= (mainCamera.maxCameraPos.x + thisLevel.xBlastZoneOffset)
        );
    }

    [System.Serializable]
    public class PlayerStats
    {
        public int health = 100;
    }
}
