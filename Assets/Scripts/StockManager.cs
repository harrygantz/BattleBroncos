using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockManager : MonoBehaviour {

    StockManager stockManager;
    private Level thisLevel;
    private static int stocksMax = 6;
    private int stocksStart = 3;
    private int stocksCurrent = 0;

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

    private void FixedUpdate()
    {
        if (thisLevel.isBlasted(transform) == true)
        {
            RemoveStock();
            CheckStockAmount();
        }
    }


    public void CheckStockAmount()
    {

        for (int i = 0; i < stocksMax; i++)
        {
            if (stocksCurrent <= i)
            {
                playerStocks[i].enabled = false;
            } else
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
        stocksCurrent--;
    }

    public static StockManager instance;

    public static StockManager GetInstanceStockManager()
    {
        return instance;
    }
}
