using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClientKnowledge
{
    public List<StoreKnowledge> inspectorKnowledge;

    private Dictionary<int, StoreKnowledge> knownStores;
    private Dictionary<int, List<StoreKnowledge>> knownStoresByProduct;

    public ClientKnowledge()
    {
        this.inspectorKnowledge = new List<StoreKnowledge>();
        this.knownStores = new Dictionary<int, StoreKnowledge>();
        this.knownStoresByProduct = new Dictionary<int, List<StoreKnowledge>>();
    }

    #region Store Knowledge

    public bool KnowsStore(int storeID)
    {
        return knownStores.ContainsKey(storeID);
    }

    public StoreKnowledge GetKnowledge(int storeID)
    {
        return knownStores[storeID];
    }

    public void CreateStoreKnowledge(Store store)
    {
        StoreKnowledge knowledge = new StoreKnowledge(store.ID, store.Location);
        CreateStoreKnowledge(knowledge);
    }

    public void CreateStoreKnowledge(StoreKnowledge storeKnowledge)
    {
        knownStores.Add(storeKnowledge.STORE_ID, storeKnowledge);
        UpdateKnowledge(storeKnowledge);
    }

    public void UpdateKnowledge(Store store)
    {
        // Update global knowledge
        StoreKnowledge knowledge = knownStores[store.ID];
        knowledge.Update(store);

        // Update knowledge by product
        Stock stock = store.StoreStock;
        List<StockData> productsSold = stock.StockSold;
        for (int i = 0; i < productsSold.Count; ++i)
        {
            StockData productStock = productsSold[i];
            int productID = productStock.Product.ID;
            if (knownStoresByProduct.ContainsKey(productID))
            {
                List<StoreKnowledge> knowledges = knownStoresByProduct[productID];
                if (!knowledges.Contains(knowledge))
                {
                    knowledges.Add(knowledge);
                }
            }
            else
            {
                List<StoreKnowledge> list = new List<StoreKnowledge>();
                list.Add(knowledge);
                knownStoresByProduct.Add(productID, list);
            }
        }

        UpdateInspectorKnowledge();
    }

    public void UpdateKnowledge(StoreKnowledge storeKnowledge)
    {
        // Update global knowledge
        StoreKnowledge knowledge = knownStores[storeKnowledge.STORE_ID];
        knowledge.Update(storeKnowledge);

        // Update knowledge by product
        Dictionary<int, int> knownStock = storeKnowledge.KnownStock;
        foreach (int productID in knownStock.Keys)
        {
            if (knownStoresByProduct.ContainsKey(productID))
            {
                List<StoreKnowledge> knowledges = knownStoresByProduct[productID];
                if (!knowledges.Contains(knowledge))
                {
                    knowledges.Add(knowledge);
                }
            }
            else
            {
                List<StoreKnowledge> list = new List<StoreKnowledge>();
                list.Add(knowledge);
                knownStoresByProduct.Add(productID, list);
            }
        }

        UpdateInspectorKnowledge();
    }

    private void UpdateInspectorKnowledge()
    {
        inspectorKnowledge.Clear();

        foreach (StoreKnowledge knowledge in knownStores.Values)
        {
            inspectorKnowledge.Add(knowledge);
        }
    }

    public bool KnowsStoreThatSellsProduct(int productID)
    {
        return knownStoresByProduct.ContainsKey(productID);
    }

    public List<StoreKnowledge> GetStoresThatSellProduct(int productID)
    {
        return new List<StoreKnowledge>(knownStoresByProduct[productID]);
    }

    #endregion
}
