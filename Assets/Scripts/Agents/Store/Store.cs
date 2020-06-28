using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Stock))]
public class Store : MonoBehaviour
{
    public readonly int ID = IDProvider.GetID();

    public int floor = 0;
    public Transform entrancePosition;
    public Bubble bubblePrefab;

    private Stock stock;
    private LocationData location;
    private bool open = true;
    private int profit = 0;
    private Dictionary<int, int> productsSoldInLastHour;
    private Bubble storesBubble;

    private void Awake()
    {
        stock = GetComponent<Stock>();
        location = new LocationData(entrancePosition.position, floor);
        productsSoldInLastHour = new Dictionary<int, int>();

        Vector3 bubblePosition = new Vector3(entrancePosition.position.x + 0.6f, entrancePosition.position.y, entrancePosition.position.z);
        storesBubble = GameObject.Instantiate<Bubble>(bubblePrefab, bubblePosition, Quaternion.identity, transform);

        foreach (StockData stockData in stock.StockSold)
        {
            int productID = stockData.Product.ID;
            productsSoldInLastHour.Add(productID, 0);
        }

        Mall.INSTANCE.AddStore(this);
        UpdateBubble();
    }

    public void Sell(int productID, int amount)
    {
        int profitObtained = stock.Sell(productID, amount);
        profit += profitObtained;
        productsSoldInLastHour[productID] += amount;
    }

    public void OnNewHour()
    {
        SalesReport salesReport = new SalesReport(ID, profit, productsSoldInLastHour);
        StockChanges stockChanges = Boss.INSTANCE.SendSalesReport(salesReport);

        stock.ModifyStock(stockChanges);
        Mall.INSTANCE.UpdateStore(this, stockChanges);

        profit = 0;
        productsSoldInLastHour.Clear();
        foreach (StockData stockData in stock.StockSold)
        {
            int productID = stockData.Product.ID;
            productsSoldInLastHour.Add(productID, 0);
        }

        UpdateBubble();
    }

    public void Close()
    {
        open = false;
    }

    private void UpdateBubble()
    {
        List<StockData> stockSold = stock.StockSold;
        List<Sprite> productsSprites = new List<Sprite>();
        for (int i = 0; i < stockSold.Count; ++i)
        {
            Sprite sprite = stockSold[i].Product.GetComponent<SpriteRenderer>().sprite;
            productsSprites.Add(sprite);
        }

        storesBubble.Draw(productsSprites);
    }

    #region Properties

    public int Floor
    {
        get => floor;
    }

    public Stock StoreStock
    {
        get => stock;
    }

    public LocationData Location
    {
        get => location;
    }

    public bool IsOpen
    {
        get => open;
    }

    #endregion
}
