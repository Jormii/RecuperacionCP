using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stock : MonoBehaviour
{
    public List<StockData> initialStock;

    private Dictionary<int, StockData> stock;

    private void Awake()
    {
        stock = new Dictionary<int, StockData>();

        for (int i = 0; i < initialStock.Count; ++i)
        {
            StockData stockData = initialStock[i];
            stock.Add(stockData.Product.ID, stockData);
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
        return stock[productID].Sell(amount);
    }

    public bool NeedsReStocking()
    {
        // TODO: Use a set to indicate products that need restocking
        foreach (StockData stockData in stock.Values)
        {
            int currentStock = stockData.CurrentStock;
            int maxStock = stockData.MaximumStock;
            int reStockMargin = stockData.ReStockMargin;

            if ((maxStock - currentStock) > reStockMargin)
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

            int unnecessaryStock = stock[productID].UpdateStock(amount);
            if (unnecessaryStock != 0)
            {
                overStock.Add(productID, unnecessaryStock);
            }
        }

        return overStock;
    }

    #region Properties

    public List<StockData> StockSold
    {
        get => new List<StockData>(stock.Values);
    }

    #endregion
}
