using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour {

    public static GameMaster gm;
    CameraManager cm;
   // StockManager sm;
    public bool playerDead;
    private static int alivePlayers;


    void Start()
    {
        //sm = StockManager.GetInstanceStockManager();
        cm = CameraManager.GetInstanceCameraManager();
        alivePlayers = 4;
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

    public static void MinusPlayerCount()
    {
        alivePlayers -= 1;
        if (alivePlayers == 1)
            GameOver();
    }

    private static void GameOver()
    {
        Debug.Log("GAME OVER");
        CameraManager cm = CameraManager.GetInstanceCameraManager();
        GameObject go = cm.GetComponent<GameOver>().GameOverScreen.gameObject;
        Player alivePlayer = (
            GameObject.FindGameObjectWithTag("Player1") ??
            GameObject.FindGameObjectWithTag("Player2") ??
            GameObject.FindGameObjectWithTag("Player3") ??
            GameObject.FindGameObjectWithTag("Player4")
        ).GetComponent<Player>();
        go.transform.Find("Horse").GetComponent<Image>().color = alivePlayer.getSpriteColor();
        go.transform.Find("Lance").GetComponent<Image>().color = alivePlayer.getLanceColor();
        go.transform.Find("Win Text").GetComponent<Text>().text = (alivePlayer.name + " Wins!");
        cm.GetComponent<GameOver>().SetGameOver();
    }


}
