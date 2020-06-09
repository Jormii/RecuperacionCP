using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stock : MonoBehaviour
{
    [SerializeField] private List<Product> productsInStock = new List<Product>();
    [SerializeField] private List<int> initialStock = new List<int>();

    private Dictionary<Product, int> productsPrice;
    private Dictionary<Product, int> productsStock;

    private void Awake()
    {
        InitializeStockAndPrices();
    }

    private void InitializeStockAndPrices()
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
            int price = p.DefaultPrice;
            int stock = initialStock[i];

            productsPrice.Add(p, price);
            productsStock.Add(p, stock);
        }

        productsInStock.Clear();
        initialStock.Clear();
        productsInStock = null;
        initialStock = null;
    }

    public void Sell(Product product, int amount)
    {
        productsStock[product] -= amount;
    }

    // TODO: It's weird that stock needs client's resources
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

    public List<Product> GetProducts()
    {
        return new List<Product>(productsStock.Keys);
    }

    public Dictionary<Product, int> GetProductsPrices()
    {
        return new Dictionary<Product, int>(productsPrice);
    }

    public Dictionary<Product, int> GetProductsStocks()
    {
        return new Dictionary<Product, int>(productsStock);
    }

    public bool SellsProduct(Product product)
    {
        return productsStock.ContainsKey(product);
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
