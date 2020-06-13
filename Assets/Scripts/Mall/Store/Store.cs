using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

[RequireComponent(typeof(Stock))]
public class Store : MonoBehaviour
{
    public readonly int ID = IDProvider.GetID();

    public int floor;

    private Stock stock;
    private LocationData location;
    private int profit = 0;

    private void Start()
    {
        stock = GetComponent<Stock>();
        location = new LocationData(transform.position, floor);

        Mall.INSTANCE.AddStore(this);
    }

    public void Sell(int productID, int amount)
    {
        int profitObtained = stock.Sell(productID, amount);
        profit += profitObtained;
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

    #endregion

}
