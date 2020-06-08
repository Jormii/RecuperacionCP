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

}
