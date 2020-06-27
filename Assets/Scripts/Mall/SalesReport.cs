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

    public int GetMostPopularProduct()
    {
        List<int> orderedKeys = new List<int>(from entry in PRODUCTS_SOLD orderby entry.Value descending select entry.Key);
        return orderedKeys[0];
    }
}
