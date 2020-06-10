﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct StoreKnowledge
{
    public readonly int STORE_ID;
    public readonly LocationData LOCATION;
    private Dictionary<int, int> productsOnSale;

    public StoreKnowledge(int storeID, LocationData location)
    {
        this.STORE_ID = storeID;
        this.LOCATION = location;
        this.productsOnSale = new Dictionary<int, int>();
    }

    public bool KnowsThatSellsProduct(int productID)
    {
        return productsOnSale.ContainsKey(productID);
    }

    public void Update(Store store)
    {
        Stock stock = store.StoreStock;
        List<StockData> productsStock = stock.StockSold;
        for (int i = 0; i < productsStock.Count; ++i)
        {
            StockData productStock = productsStock[i];
            int productID = productStock.Product.ID;
            int price = productStock.Price;
            UpdateProduct(productID, price);
        }
    }

    private void UpdateProduct(int productID, int price)
    {
        if (KnowsThatSellsProduct(productID))
        {
            productsOnSale[productID] = price;
        }
        else
        {
            productsOnSale.Add(productID, price);
        }
    }

    public override int GetHashCode()
    {
        return STORE_ID;
    }

}
