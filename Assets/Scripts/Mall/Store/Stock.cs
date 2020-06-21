using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Store))]
public class Stock : MonoBehaviour
{
    public const int MAX_PRODUCTS = 3;

    public bool debug = false;
    public List<StockData> initialStock = new List<StockData>();
    public List<StockData> inspectorStock = new List<StockData>();
    public float stockTimeout = 5f;

    private Store store;
    private Dictionary<int, StockData> stock;
    private bool reStockingCountdownRunning;
    private float reStockingCountdown;
    private Dictionary<int, int> reStockAsked;

    private void Awake()
    {
        if (initialStock.Count == 0)
        {
            Debug.LogErrorFormat("Error: Store {0} has no products in sale", name);
            Destroy(gameObject);
        }

        stock = new Dictionary<int, StockData>();

        if (initialStock.Count > MAX_PRODUCTS)
        {
            Debug.LogWarning("The maximum number of products a store can sell is {0}. Discarting excessive products");
        }

        int maxIndex = Mathf.Min(initialStock.Count, MAX_PRODUCTS);
        for (int i = 0; i < maxIndex; ++i)
        {
            StockData stockData = initialStock[i];
            stock.Add(stockData.Product.ID, stockData);
        }
    }

    private void Start()
    {
        store = GetComponent<Store>();
        reStockingCountdownRunning = false;
        reStockAsked = new Dictionary<int, int>();

        if (NeedsReStocking())
        {
            StartReStockingCountdown();
        }

        UpdateInspectorList();
    }

    private void Update()
    {
        if (reStockingCountdownRunning)
        {
            UpdateReStockingCountdown();
        }
    }

    #region Product Related

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

        UpdateInspectorList();

        return profit;
    }

    #endregion

    #region ReStocking Related

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

    public bool ProductNeedsReStock(int productID)
    {
        StockData stockData = stock[productID];
        return stockData.NeedsReStock();
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

                // Add to reStock asked
                if (reStockAsked.ContainsKey(productID))
                {
                    reStockAsked[productID] += amount;
                }
                else
                {
                    reStockAsked.Add(productID, amount);
                }
            }
        }

        return productsToRefill;
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

        UpdateInspectorList();

        return overStock;
    }

    #endregion

    #region ReStocking Countdown

    private void StartReStockingCountdown()
    {
        if (reStockingCountdownRunning)
        {
            return;
        }

        reStockingCountdownRunning = true;
        reStockingCountdown = stockTimeout;
    }

    private void StopReStockingCountdown()
    {
        reStockingCountdownRunning = false;
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

    private void OnCountdownFinished()
    {
        if (debug)
        {
            Debug.LogWarningFormat("Stock from store {0} has been in need for a long time", name);
        }

        Dictionary<int, int> reStock = GetProductsToRefill();
        Boss.INSTANCE.RequestReStock(store, reStock);

        reStockingCountdown = 1.5f * stockTimeout;

        if (debug)
        {
            Debug.LogWarningFormat("New countdown is {0} seconds long", reStockingCountdown);
        }
    }

    #endregion

    #region Stock Modification

    public void ModifyStock(StockChanges changes)
    {
        foreach (KeyValuePair<int, int> entry in changes.PRICE_CHANGES)
        {
            int productID = entry.Key;
            int modification = entry.Value;

            stock[productID].ModifyPrice(modification);
        }

        foreach (KeyValuePair<int, int> entry in changes.MAX_STOCK_CHANGES)
        {
            int productID = entry.Key;
            int modification = entry.Value;

            stock[productID].ModifyMaxStock(modification);
        }

        foreach (int productID in changes.PRODUCTS_TO_REMOVE)
        {
            stock.Remove(productID);
        }

        int newProducts = changes.PRODUCTS_TO_REMOVE.Count;
        for (int i = 0; i < newProducts; ++i)
        {
            Product randomProduct = ProductsManager.INSTANCE.GetRandomProduct();
            if (stock.ContainsKey(randomProduct.ID) || changes.PRODUCTS_TO_REMOVE.Contains(randomProduct.ID))
            {
                i -= 1;
            }
            else
            {
                StockData newStockData = new StockData(randomProduct, 5, 5, 5, 0);
                stock.Add(randomProduct.ID, newStockData);
            }
        }

        UpdateInspectorList();
    }

    #endregion

    private void UpdateInspectorList()
    {
        inspectorStock.Clear();
        foreach (StockData stockData in stock.Values)
        {
            inspectorStock.Add(stockData);
        }
    }

    #region Properties

    public List<StockData> StockSold
    {
        get => new List<StockData>(stock.Values);
    }

    #endregion
}
