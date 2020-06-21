using System.Collections.Generic;
using System.Linq;

public struct SalesReport
{
    public readonly int STORE_ID;
    public readonly int PROFIT;
    public readonly Dictionary<int, int> PRODUCTS_SOLD;
    public SalesReport(int storeID, int profit, Dictionary<int, int> productsSold)
    {
        this.STORE_ID = storeID;
        this.PROFIT = profit;
        this.PRODUCTS_SOLD = productsSold;
    }

    public List<int> GetTwoMostPopularProducts()
    {
        List<int> orderedKeys = new List<int>(from entry in PRODUCTS_SOLD orderby entry.Value descending select entry.Key);

        List<int> mostPopular = new List<int>();
        int maxIndex = System.Math.Min(2, orderedKeys.Count);
        for (int i = 0; i < maxIndex; ++i)
        {
            mostPopular.Add(orderedKeys[i]);
        }

        return mostPopular;
    }
}
