﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClientResources : MonoBehaviour
{
    [SerializeField] public List<Product> wantedProducts;
    [SerializeField] public List<int> wantedAmount;
    public int money;

    private Dictionary<Product, int> shoppingList;

    private void Start()
    {
        int nProducts = wantedProducts.Count;
        int mAmount = wantedAmount.Count;

        if (nProducts != mAmount)
        {
            Debug.LogErrorFormat("ERROR: Mismatch in client resources in gameobject {0}", name);
            Destroy(gameObject);
            return;
        }

        shoppingList = new Dictionary<Product, int>();
        for (int i = 0; i < nProducts; ++i)
        {
            Product p = wantedProducts[i];
            int a = wantedAmount[i];

            shoppingList.Add(p, a);
        }
    }

    public bool ThereAreThingsLeftToBuy()
    {
        bool thingsLeft = false;
        foreach (KeyValuePair<Product, int> entry in shoppingList)
        {
            Product product = entry.Key;
            int amount = entry.Value;

            if (amount != 0)
            {
                thingsLeft = true;
            }
        }

        return thingsLeft;
    }

    public List<Product> GetProductsNotBoughtYet()
    {
        List<Product> products = new List<Product>();
        foreach (KeyValuePair<Product, int> entry in shoppingList)
        {
            Product product = entry.Key;
            int amount = entry.Value;

            if (amount != 0)
            {
                products.Add(product);
            }
        }

        return products;
    }

    public int HowManyCanAfford(Product product, int price, int stock)
    {
        int wantsToBuy = (int)shoppingList[product];
        int canBuy = Math.Min((int)(money / price), stock);

        int actualBuy = Math.Min(wantsToBuy, canBuy);
        return actualBuy;
    }

    public void Buy(Product product, int price, int stock)
    {
        int amount = HowManyCanAfford(product, price, stock);
        int moneySpent = amount * price;

        money -= moneySpent;
        shoppingList[product] -= amount;

        // TODO: Remove. Debug purposes
        int index = wantedProducts.IndexOf(product);
        wantedAmount[index] -= amount;
    }

}
