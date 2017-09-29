using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    public static GameMaster gm;

    void Start()
    {
        if (gm == null)
        {
            gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        }
    }

    public void RespawnPlayer (Player player)
    {
        Transform spawnPoint = GameObject.FindGameObjectWithTag("Level").GetComponent<Level>().spawnPoints[player.playerIndex];
        player.gameObject.transform.position = spawnPoint.position;
        player.gameObject.SetActive(true);
    }

    public static void KillPlayer(Player player)
    {
        player.gameObject.SetActive(false);
        gm.RespawnPlayer(player);
    }

}
