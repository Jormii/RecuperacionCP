using System.Collections.Generic;
using UnityEngine;

public class ClientResources
{
    public const int MIN_MONEY = 50;
    public const int MAX_MONEY = 100;
    public const int MAX_PRODUCTS_IN_SHOPPING_LIST = 3;

    private Dictionary<int, ShoppingList> shoppingList;
    private int money;

    public ClientResources()
    {
        this.money = Random.Range(MIN_MONEY, MAX_MONEY);
        this.shoppingList = new Dictionary<int, ShoppingList>();

        List<Product> products = Product.GetRandomProducts(MAX_PRODUCTS_IN_SHOPPING_LIST);
        for (int i = 0; i < MAX_PRODUCTS_IN_SHOPPING_LIST; ++i)
        {
            Product product = products[i];
            int quantityWanted = Random.Range(1, 5);

            ShoppingList productShoppingList = new ShoppingList(product, quantityWanted);
            shoppingList.Add(product.ID, productShoppingList);
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
        int wantsToBuy = shoppingList[productID].AmountLeftToBuy();
        int canAfford = Mathf.Min((int)(money / price), stock);

        int canBuy = Mathf.Min(wantsToBuy, canAfford);
        return canBuy;
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
