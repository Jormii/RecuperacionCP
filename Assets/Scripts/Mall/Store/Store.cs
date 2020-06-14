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
    private Dictionary<int, int> productsSouldInLastHour;

    private void Start()
    {
        stock = GetComponent<Stock>();
        location = new LocationData(transform.position, floor);
        productsSouldInLastHour = new Dictionary<int, int>();

        Mall.INSTANCE.AddStore(this);
    }

    public void Sell(int productID, int amount)
    {
        int profitObtained = stock.Sell(productID, amount);
        profit += profitObtained;
    }

    public void OnNewHour()
    {
        SalesReport salesReport = new SalesReport(ID, profit, productsSouldInLastHour);
        Boss.INSTANCE.SendSalesReport(salesReport);

        profit = 0;
        productsSouldInLastHour.Clear();
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
