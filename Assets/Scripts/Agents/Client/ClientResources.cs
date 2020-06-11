using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClientResources : MonoBehaviour
{
    public List<ShoppingList> initialShoppingList;

    private Dictionary<int, ShoppingList> shoppingList;
    [SerializeField] private int money;

    private void Awake()
    {
        shoppingList = new Dictionary<int, ShoppingList>();
        for (int i = 0; i < initialShoppingList.Count; ++i)
        {
            ShoppingList wantedProduct = initialShoppingList[i];
            int productID = wantedProduct.Product.ID;
            shoppingList.Add(productID, wantedProduct);
        }
    }

    public bool ThereAreThingsLeftToBuy()
    {
        foreach (ShoppingList productWanted in shoppingList.Values)
        {
            if (!productWanted.PurchaseComplete())
            {
                return true;
            }
        }

        return false;
    }

    public List<int> GetProductsNotBoughtYet()
    {
        List<int> products = new List<int>();
        foreach (ShoppingList productWanted in shoppingList.Values)
        {
            if (!productWanted.PurchaseComplete())
            {
                int productID = productWanted.Product.ID;
                products.Add(productID);
            }
        }

        return products;
    }

    public int HowManyCanAfford(int productID, int price, int stock)
    {
        int wantsToBuy = shoppingList[productID].LeftToBuy();
        int canBuy = Math.Min((int)(money / price), stock);

        int canAfford = Math.Min(wantsToBuy, canBuy);
        return canAfford;
    }

    public void Buy(int productID, int amount, int price)
    {
        int moneySpent = amount * price;

        money -= moneySpent;
        shoppingList[productID].Buy(amount);
    }

    public bool ProductIsInShoppingList(int productID)
    {
        if (!shoppingList.ContainsKey(productID))
        {
            return false;
        }

        return !shoppingList[productID].PurchaseComplete();
    }

    public List<int> GetProductsInterestedIn(Store store)
    {
        List<int> products = new List<int>();
        Stock stock = store.StoreStock;
        List<StockData> productsStock = stock.StockSold;
        for (int i = 0; i < productsStock.Count; ++i)
        {
            StockData productStock = productsStock[i];
            int productID = productStock.Product.ID;
            if (ProductIsInShoppingList(productID))
            {
                products.Add(productID);
            }
        }

        return products;
    }
}
