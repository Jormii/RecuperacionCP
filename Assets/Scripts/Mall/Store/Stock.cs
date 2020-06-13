using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Store))]
public class Stock : MonoBehaviour
{
    public List<StockData> initialStock;
    public float stockTimeout = 5f;

    private Store store;
    private Dictionary<int, StockData> stock;
    private bool reStockingCountdownRunning = false;
    private float reStockingCountdown;

    private void Awake()
    {
        stock = new Dictionary<int, StockData>();

        for (int i = 0; i < initialStock.Count; ++i)
        {
            StockData stockData = initialStock[i];
            stock.Add(stockData.Product.ID, stockData);
        }
    }

    private void Start()
    {
        store = GetComponent<Store>();
        if (NeedsReStocking())
        {
            StartReStockingCountdown();
        }
    }

    private void Update()
    {
        if (reStockingCountdownRunning)
        {
            UpdateReStockingCountdown();
        }
    }

    public bool HasProductInStock(int productID)
    {
        return stock.ContainsKey(productID);
    }

    public StockData GetStockOfProduct(int productID)
    {
        return stock[productID];
    }

    public int Sell(int productID, int amount)
    {
        StockData stockData = stock[productID];
        int profit = stockData.Sell(amount);

        if (stockData.NeedsReStock())
        {
            StartReStockingCountdown();
        }

        return profit;
    }

    public bool NeedsReStocking()
    {
        foreach (StockData stockData in stock.Values)
        {
            if (stockData.NeedsReStock())
            {
                return true;
            }
        }

        return false;
    }

    public Dictionary<int, int> GetProductsToRefill()
    {
        Dictionary<int, int> productsToRefill = new Dictionary<int, int>();
        foreach (StockData stockData in stock.Values)
        {
            if (stockData.NeedsReStock())
            {
                int productID = stockData.Product.ID;
                int amount = stockData.ReStockNeeded();
                productsToRefill.Add(productID, amount);
            }
        }

        return productsToRefill;
    }

    public bool ProductNeedsReStock(int productID)
    {
        StockData stockData = stock[productID];
        return stockData.NeedsReStock();
    }

    public Dictionary<int, int> ReStock(Dictionary<int, int> reStock)
    {
        Dictionary<int, int> overStock = new Dictionary<int, int>();
        foreach (KeyValuePair<int, int> entry in reStock)
        {
            int productID = entry.Key;
            int amount = entry.Value;

            if (!HasProductInStock(productID))
            {
                continue;
            }

            StockData stockData = stock[productID];
            int unnecessaryStock = stockData.UpdateStock(amount);
            if (unnecessaryStock != 0)
            {
                overStock.Add(productID, unnecessaryStock);
            }
        }

        if (!NeedsReStocking())
        {
            StopReStockingCountdown();
        }

        return overStock;
    }


    #region ReStocking countdown

    private void StartReStockingCountdown()
    {
        if (reStockingCountdownRunning)
        {
            return;
        }

        reStockingCountdownRunning = true;
        reStockingCountdown = stockTimeout;
    }

    private void UpdateReStockingCountdown()
    {
        float newTime = reStockingCountdown - Time.deltaTime;
        if (newTime < 0f)
        {
            OnCountdownFinished();
        }
        else
        {
            reStockingCountdown = newTime;
        }
    }

    private void StopReStockingCountdown()
    {
        reStockingCountdownRunning = false;
    }

    private void OnCountdownFinished()
    {
        Debug.LogWarningFormat("Stock from store {0} has been in need for a long time", name);

        Dictionary<int, int> reStock = GetProductsToRefill();
        Boss.INSTANCE.RequestReStock(store, reStock);

        reStockingCountdown = 1.5f * stockTimeout;

        Debug.LogWarningFormat("New countdown is {0} seconds long", reStockingCountdown);
    }

    #endregion


    #region Properties

    public List<StockData> StockSold
    {
        get => new List<StockData>(stock.Values);
    }

    #endregion
}
