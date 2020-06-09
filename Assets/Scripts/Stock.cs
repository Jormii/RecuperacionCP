using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stock : MonoBehaviour
{
    public List<Product> productsInStock = new List<Product>();
    public List<int> initialStock = new List<int>();

    public Dictionary<Product, int> productsPrice;
    public Dictionary<Product, int> productsStock;

    void Start()
    {
        productsPrice = new Dictionary<Product, int>();
        productsStock = new Dictionary<Product, int>();

        if (productsInStock.Count != initialStock.Count)
        {
            Debug.LogError("Mismatch in products' lists");
            Destroy(gameObject);
            return;
        }

        for (int i = 0; i < productsInStock.Count; ++i)
        {
            Product p = productsInStock[i];
            int price = p.defaultPrice;
            int stock = initialStock[i];

            productsPrice.Add(p, price);
            productsStock.Add(p, stock);
        }
    }

    public void Sell(Product product, int amount)
    {
        productsStock[product] -= amount;
    }

    public List<Product> GetProductsWanted(ClientResources clientResources)
    {
        List<Product> products = new List<Product>();
        foreach (KeyValuePair<Product, int> entry in productsStock)
        {
            Product product = entry.Key;
            int amount = entry.Value;

            if (amount == 0)
            {
                continue;
            }

            if (clientResources.ProductIsInShoppingList(product))
            {
                products.Add(product);
            }
        }

        return products;
    }

    public int GetPriceOfProduct(Product product)
    {
        return productsPrice[product];
    }

    public int GetStockOfProduct(Product product)
    {
        return productsStock[product];
    }

}
