using UnityEngine;

[System.Serializable]
public class StockData
{
    [SerializeField] private Product product;
    [SerializeField] private int price;
    [SerializeField] private int currentStock;
    [SerializeField] private int maximumStock;
    [SerializeField] private int reStockMargin;

    public StockData(Product product, int price, int initialStock, int maximumStock, int reStockMargin)
    {
        this.product = product;
        this.price = price;
        this.currentStock = initialStock;
        this.maximumStock = maximumStock;
        this.reStockMargin = reStockMargin;
    }

    public int Sell(int amount)
    {
        currentStock -= amount;
        int profit = price * amount;
        return profit;
    }

    public int UpdateStock(int amount)
    {
        int stockNeccessaryToReachMaximum = maximumStock - currentStock;
        int amountReStocked = Mathf.Min(amount, stockNeccessaryToReachMaximum);
        currentStock += amountReStocked;

        int overStock = amount - amountReStocked;
        return overStock;
    }

    public bool NeedsReStock()
    {
        return (maximumStock - currentStock) > reStockMargin;
    }

    public int ReStockNeeded()
    {
        return maximumStock - currentStock;
    }

    public void ModifyPrice(int amount)
    {
        price = Mathf.Min(1, price + amount);
    }

    public void ModifyMaxStock(int amount)
    {
        maximumStock = Mathf.Min(1, maximumStock + amount);
    }

    #region Properties

    public Product Product
    {
        get => product;
    }

    public int Price
    {
        get => price;
    }

    public int CurrentStock
    {
        get => currentStock;
    }

    public int MaximumStock
    {
        get => maximumStock;
    }

    public int ReStockMargin
    {
        get => reStockMargin;
    }

    #endregion
}
