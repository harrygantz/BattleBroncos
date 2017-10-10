using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    public static GameMaster gm;
    CameraManager cm;
   // StockManager sm;
    public bool playerDead;

    void Start()
    {
        //sm = StockManager.GetInstanceStockManager();
        cm = CameraManager.GetInstanceCameraManager();
        if (gm == null)
        {
            gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        }
    }

    public Transform SpawnPoint;
 
    public void RespawnPlayer (Player player)
    {
        Transform spawnPoint = GameObject.FindGameObjectWithTag("Level").GetComponent<Level>().spawnPoints[player.playerIndex];
        player.gameObject.transform.position = spawnPoint.position;
        player.gameObject.SetActive(true);
        //Connect camera to new spawned players transform
        gm.playerDead = false;
        cm.players.Add(player.transform);
    }

    public static void KillPlayer(Player player)
    {
        gm.playerDead = true;
        gm.RemoveCameraTransform(player);
        player.gameObject.SetActive(false);
        gm.RespawnPlayer(player);
        player.GetComponent<StockManager>().RemoveStock();
    }

    public void RemoveCameraTransform(Player player)
    {
        if (playerDead)
        {
            cm.players.Remove(player.transform);
        } 
    }


    public static void UpdateHealth(string healthUiName, int playerHealth)
    {
        GameObject healthDisplay = GameObject.Find(healthUiName);
        healthDisplay.GetComponent<NumberRenderer>().RenderNumber(playerHealth);
    }

    public static void GameOver()
    {
        CameraManager cm = CameraManager.GetInstanceCameraManager();
        cm.GetComponent<GameOver>().SetGameOver();
    }

}
