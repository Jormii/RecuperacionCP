﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Store : MonoBehaviour
{
    private static int idCounter = 0;

    public readonly int ID = GetStoreID();

    [SerializeField] private int floor;
    private Stock stock;

    private void Start()
    {
        stock = GetComponent<Stock>();

        Mall.INSTANCE.AddStore(this);
    }

    public void Sell(Product product, int amount)
    {
        stock.Sell(product, amount);
    }

    public List<Product> GetProductsInStock()
    {
        return stock.GetProducts();
    }

    public Dictionary<Product, int> GetProductsPrices()
    {
        return stock.GetProductsPrices();
    }

    public Dictionary<Product, int> GetProductsStocks()
    {
        return stock.GetProductsStocks();
    }

    public int GetPriceOfProduct(Product product)
    {
        return stock.GetPriceOfProduct(product);
    }

    public int GetStockOfProduct(Product product)
    {
        return stock.GetStockOfProduct(product);
    }

    public List<Product> GetProductsWanted(ClientResources resources)
    {
        return stock.GetProductsWanted(resources);
    }

    public bool NeedsReStocking()
    {
        return stock.NeedsReStocking();
    }

    public Dictionary<Product, int> GetStockToRefill()
    {
        return stock.GetStockToRefill();
    }

    public void ReStock(Dictionary<Product, int> reStock)
    {
        stock.ReStock(reStock);
    }

    private static int GetStoreID()
    {
        return Interlocked.Increment(ref idCounter);
    }

    public override int GetHashCode()
    {
        return ID;
    }

    #region Properties

    public int Floor
    {
        get => floor;
    }

    #endregion

}
