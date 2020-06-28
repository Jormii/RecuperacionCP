using System.Collections.Generic;

public struct StockChanges
{
    public readonly int STORE_ID;
    public readonly Dictionary<int, int> PRICE_CHANGES;
    public readonly Dictionary<int, int> MAX_STOCK_CHANGES;
    public readonly HashSet<int> PRODUCTS_TO_REMOVE;

    public StockChanges(int storeID)
    {
        this.STORE_ID = storeID;
        this.PRICE_CHANGES = new Dictionary<int, int>();
        this.MAX_STOCK_CHANGES = new Dictionary<int, int>();
        this.PRODUCTS_TO_REMOVE = new HashSet<int>();
    }

    public void ChangePrice(int productID, int amount)
    {
        PRICE_CHANGES.Add(productID, amount);
    }

    public void ChangeStock(int productID, int amount)
    {
        MAX_STOCK_CHANGES.Add(productID, amount);
    }

    public void RemoveProduct(int productID)
    {
        PRODUCTS_TO_REMOVE.Add(productID);
    }
}