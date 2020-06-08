using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stock : MonoBehaviour
{
    [SerializeField] List<Product> productsInStock = new List<Product>();
    Dictionary<Product, int> productsPrice;

    void Start()
    {
        productsPrice = new Dictionary<Product, int>();
        for (int i = 0; i < productsInStock.Count; ++i)
        {
            Product p = productsInStock[i];
            int price = p.defaultPrice;

            productsPrice.Add(p, price);
        }
    }

}
