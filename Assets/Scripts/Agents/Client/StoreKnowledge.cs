using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StoreKnowledge
{
    public List<Tuple> inspectorList;

    [SerializeField] public readonly int STORE_ID;
    [SerializeField] public readonly LocationData LOCATION;
    private Dictionary<int, int> productsOnSale;

    public StoreKnowledge(int storeID, LocationData location)
    {
        this.inspectorList = new List<Tuple>();

        this.STORE_ID = storeID;
        this.LOCATION = location;
        this.productsOnSale = new Dictionary<int, int>();
    }

    public bool KnowsThatSellsProduct(int productID)
    {
        return productsOnSale.ContainsKey(productID);
    }

    public int GetPriceOfProduct(int productID)
    {
        return productsOnSale[productID];
    }

    public void Update(Store store)
    {
        Stock stock = store.StoreStock;
        List<StockData> productsStock = stock.StockSold;
        for (int i = 0; i < productsStock.Count; ++i)
        {
            StockData productStock = productsStock[i];
            int productID = productStock.Product.ID;
            int price = productStock.Price;
            UpdateProduct(productID, price);
        }

        UpdateInspectorList();
    }

    public void Update(StoreKnowledge knowledge)
    {
        Dictionary<int, int> productsToUpdate = new Dictionary<int, int>();
        Dictionary<int, int> knownStock = knowledge.productsOnSale;
        foreach (KeyValuePair<int, int> entry in knownStock)
        {
            int productID = entry.Key;
            int price = entry.Value;
            productsToUpdate.Add(productID, price);
        }

        // Calling UpdateProduct in the loop above modifies the collection
        foreach (KeyValuePair<int, int> entry in productsToUpdate)
        {
            int productID = entry.Key;
            int price = entry.Value;
            UpdateProduct(productID, price);
        }

        UpdateInspectorList();
    }

    private void UpdateProduct(int productID, int price)
    {
        if (KnowsThatSellsProduct(productID))
        {
            productsOnSale[productID] = price;
        }
        else
        {
            productsOnSale.Add(productID, price);
        }
    }

    private void UpdateInspectorList()
    {
        inspectorList.Clear();
        foreach (KeyValuePair<int, int> entry in productsOnSale)
        {
            inspectorList.Add(new Tuple(entry.Key, entry.Value));
        }
    }

    public Dictionary<int, int> KnownStock
    {
        get => new Dictionary<int, int>(productsOnSale);
    }
}
