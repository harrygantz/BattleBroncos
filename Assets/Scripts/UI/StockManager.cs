using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockManager : MonoBehaviour {

    StockManager stockManager;

    private Level thisLevel;
    public int stocksStart;
    private static int stocksMax = 4;
    public int stocksCurrent = 0;
    public Image[] playerStocks;


    void Awake()
    {
        thisLevel = GameObject.FindGameObjectWithTag("Level").GetComponent<Level>();
        instance = this;
    }

    void Start ()
    {
        stocksCurrent = stocksStart;
        CheckStockAmount();
    }

    public void CheckStockAmount() {
        for (int i = 0; i < stocksMax; i++)
        {
            if (stocksCurrent <= i)
            {
                playerStocks[i].enabled = false;
            }
            else
            {
                playerStocks[i].enabled = true;
            }
        }
    }

    public int GetCurrentStocks()
    {
        return stocksCurrent;
    }

    public void RemoveStock()
    {
        playerStocks[stocksCurrent-1].enabled = false;
        stocksCurrent--;
    }

    public static StockManager instance;

    public static StockManager GetInstanceStockManager()
    {
        return instance;
    }
}
