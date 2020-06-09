using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct StoreKnowledge
{
    public readonly int STORE_ID;
    public readonly int FLOOR;
    public readonly Vector2 POSITION;
    private Dictionary<Product, int> productsOnSale;

    public StoreKnowledge(int storeID, int floor, Vector2 position)
    {
        this.STORE_ID = storeID;
        this.FLOOR = floor;
        this.POSITION = position;
        this.productsOnSale = new Dictionary<Product, int>();
    }

    public bool KnowsThatSellsProduct(Product product)
    {
        return productsOnSale.ContainsKey(product);
    }

    public void Update(Store store)
    {
        foreach (KeyValuePair<Product, int> entry in store.GetProductsPrices())
        {
            Product product = entry.Key;
            int price = entry.Value;
            UpdateProduct(product, price);
        }
    }

    private void UpdateProduct(Product product, int price)
    {
        if (KnowsThatSellsProduct(product))
        {
            productsOnSale[product] = price;
        }
        else
        {
            productsOnSale.Add(product, price);
        }
    }

    public override int GetHashCode()
    {
        return STORE_ID;
    }

}
