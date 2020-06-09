using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClientResources : MonoBehaviour
{
    [SerializeField] private List<Product> wantedProducts;
    [SerializeField] private List<int> wantedAmount;
    [SerializeField] private int money;

    private Dictionary<Product, int> shoppingList;
    private Dictionary<Product, int> productsBought;
    private int amountLeftToBuy = 0;

    private void Awake()
    {
        InitializeShoppingList();
    }

    private void InitializeShoppingList()
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
        productsBought = new Dictionary<Product, int>();
        for (int i = 0; i < nProducts; ++i)
        {
            Product p = wantedProducts[i];
            int a = wantedAmount[i];

            shoppingList.Add(p, a);
            productsBought.Add(p, 0);
            amountLeftToBuy += a;
        }

        wantedProducts.Clear();
        wantedAmount.Clear();
        wantedProducts = null;
        wantedAmount = null;
    }

    public bool ThereAreThingsLeftToBuy()
    {
        return amountLeftToBuy != 0;
    }

    public List<Product> GetProductsNotBoughtYet()
    {
        List<Product> products = new List<Product>();
        foreach (KeyValuePair<Product, int> entry in productsBought)
        {
            Product product = entry.Key;
            int amountBought = entry.Value;
            int amountWanted = shoppingList[product];

            if ((amountWanted - amountBought) != 0)
            {
                products.Add(product);
            }
        }

        return products;
    }

    public int HowManyCanAfford(Product product, int price, int stock)
    {
        int wantsToBuy = shoppingList[product] - productsBought[product];
        int canBuy = Math.Min((int)(money / price), stock);

        int canAfford = Math.Min(wantsToBuy, canBuy);
        return canAfford;
    }

    public void Buy(Product product, int amount, int price)
    {
        int moneySpent = amount * price;

        money -= moneySpent;
        productsBought[product] += amount;
        amountLeftToBuy -= amount;
    }

    public bool ProductIsInShoppingList(Product product)
    {
        if (!shoppingList.ContainsKey(product))
        {
            return false;
        }

        return GetAmountLeftToBuy(product) != 0;
    }

    public int GetAmountLeftToBuy(Product product)
    {
        int wanted = shoppingList[product];
        int bought = productsBought[product];

        return wanted - bought;
    }

}
