using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    public static GameMaster gm;
    public bool playerDead;
    CameraManager camManager;

    void Start()
    {
        camManager = CameraManager.GetInstance();
        if (gm == null)
        {
            gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        }
    }

    public Transform player1Prefab;
    public Transform player2Prefab;
    public Transform SpawnPoint;

  
 
    public void RespawnPlayer (Player player)
    {
        Transform spawnPoint = GameObject.FindGameObjectWithTag("Level").GetComponent<Level>().spawnPoints[player.playerIndex];
        player.gameObject.transform.position = spawnPoint.position;
        player.gameObject.SetActive(true);

        //Connect camera to new spawned players transform
        gm.playerDead = false;
        camManager.players.Add(player.transform);
    }

    public static void KillPlayer(Player player)
    {
        gm.playerDead = true;
        gm.RemoveCameraTransform(player);
        player.gameObject.SetActive(false);
        gm.RespawnPlayer(player);
    }

    public void RemoveCameraTransform(Player player)
    {
        if (playerDead)
        {
            camManager.players.Remove(player.transform);
        }
      
    }

    public static void UpdateHealth(string healthUiName, int playerHealth)
    {
        GameObject healthDisplay = GameObject.Find(healthUiName);
        healthDisplay.GetComponent<NumberRenderer>().RenderNumber(playerHealth);
    }

}
