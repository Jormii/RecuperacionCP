using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Stock))]
public class Store : MonoBehaviour
{
    public readonly int ID = IDProvider.GetID();

    public int floor = 0;

    private Stock stock;
    private LocationData location;
    private bool open = true;
    private int profit = 0;
    private Dictionary<int, int> productsSoldInLastHour;

    private void Start()
    {
        stock = GetComponent<Stock>();
        location = new LocationData(transform.position, floor);
        productsSoldInLastHour = new Dictionary<int, int>();

        Mall.INSTANCE.AddStore(this);
    }

    public void Sell(int productID, int amount)
    {
        int profitObtained = stock.Sell(productID, amount);
        profit += profitObtained;

        if (productsSoldInLastHour.ContainsKey(productID))
        {
            productsSoldInLastHour[productID] += amount;
        }
        else
        {
            productsSoldInLastHour.Add(productID, amount);
        }
    }

    public void OnNewHour()
    {
        Dictionary<int, int> timesReStockAsked = stock.GetSalesReport();
        SalesReport salesReport = new SalesReport(ID, profit, productsSoldInLastHour, timesReStockAsked);
        StockChanges stockChanges = Boss.INSTANCE.SendSalesReport(salesReport);

        stock.ModifyStock(stockChanges);

        profit = 0;
        productsSoldInLastHour.Clear();
    }

    public void Close()
    {
        open = false;
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
