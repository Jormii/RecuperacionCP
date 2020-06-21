using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClientResources
{
    public const int MIN_MONEY = 50;
    public const int MAX_MONEY = 100;
    public const int MAX_PRODUCTS_IN_SHOPPING_LIST = 3;

    public List<ShoppingList> inspectorList;
    private Dictionary<int, ShoppingList> shoppingList;
    [SerializeField] private int money;

    public ClientResources()
    {
        this.money = 0;
        this.inspectorList = new List<ShoppingList>();
        this.shoppingList = new Dictionary<int, ShoppingList>();
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

        // Update inspector list
        inspectorList.Clear();
        foreach (ShoppingList list in shoppingList.Values)
        {
            inspectorList.Add(list);
        }
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

    public void Randomize()
    {
        System.Random rng = new System.Random();

        this.money = rng.Next(MIN_MONEY, MAX_MONEY);

        List<Product> products = ProductsManager.INSTANCE.GetRandomProducts(MAX_PRODUCTS_IN_SHOPPING_LIST);
        for (int i = 0; i < MAX_PRODUCTS_IN_SHOPPING_LIST; ++i)
        {
            Product product = products[i];
            int quantityWanted = rng.Next(1, 6);

            ShoppingList productShoppingList = new ShoppingList(product, quantityWanted);
            shoppingList.Add(product.ID, productShoppingList);
            inspectorList.Add(productShoppingList);
        }
    }
}
