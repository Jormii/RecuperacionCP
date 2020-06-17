using System.Collections.Generic;

public struct SalesReport
{
    public readonly int STORE_ID;
    public readonly int PROFIT;
    public readonly Dictionary<int, int> PRODUCTS_SOLD;
    public readonly Dictionary<int, int> TIMES_RESTOCKED;

    public SalesReport(int storeID, int profit, Dictionary<int, int> productsSold, Dictionary<int, int> timesReStocked)
    {
        this.STORE_ID = storeID;
        this.PROFIT = profit;
        this.PRODUCTS_SOLD = productsSold;
        this.TIMES_RESTOCKED = timesReStocked;
    }
}
