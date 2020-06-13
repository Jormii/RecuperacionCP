using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientKnowledge
{
    private Dictionary<int, StoreKnowledge> knownStores;
    private Dictionary<int, List<StoreKnowledge>> knownStoresByProduct;
    private Dictionary<int, ExitKnowledge> knownExits;

    public ClientKnowledge()
    {
        this.knownStores = new Dictionary<int, StoreKnowledge>();
        this.knownStoresByProduct = new Dictionary<int, List<StoreKnowledge>>();
        this.knownExits = new Dictionary<int, ExitKnowledge>();
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

    public void CreateKnowledge(Store store)
    {
        StoreKnowledge knowledge = new StoreKnowledge(store.ID, store.Location);
        CreateKnowledge(knowledge);
    }

    public void CreateKnowledge(StoreKnowledge storeKnowledge)
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
                if (knowledges.Contains(knowledge))
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
    }

    public bool KnowsStoreThatSellsProduct(int productID)
    {
        return knownStoresByProduct.ContainsKey(productID);
    }

    public List<StoreKnowledge> GetStoresThatSellProduct(int productID)
    {
        return knownStoresByProduct[productID];
    }

    #endregion

    #region Exit Knowledge

    public bool KnowsAnyExit()
    {
        return knownExits.Count != 0;
    }

    public bool KnowsExit(int exitID)
    {
        return knownExits.ContainsKey(exitID);
    }

    public void CreateKnowledge(Exit exit)
    {
        LocationData exitLocation = new LocationData(exit.transform.position, exit.Floor);
        ExitKnowledge newKnowledge = new ExitKnowledge(exit.ID, exitLocation);

        knownExits.Add(exit.ID, newKnowledge);
    }

    public List<ExitKnowledge> GetKnownExits()
    {
        return new List<ExitKnowledge>(knownExits.Values);
    }

    #endregion

}
