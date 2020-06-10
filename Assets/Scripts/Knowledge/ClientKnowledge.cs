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

        // TODO: Remove this
        GameObject exit = GameObject.FindGameObjectWithTag("Exit");
        knownExits.Add(0, new ExitKnowledge(0, new LocationData(exit.transform.position, 0)));
    }

    #region Store Knowledge

    public bool KnowsStore(Store store)
    {
        return knownStores.ContainsKey(store.ID);
    }

    public StoreKnowledge GetKnowledge(Store store)
    {
        return knownStores[store.ID];
    }

    public void CreateKnowledge(Store store)
    {
        StoreKnowledge knowledge = new StoreKnowledge(store.ID, store.Location);
        knownStores.Add(store.ID, knowledge);

        UpdateKnowledge(store);
    }

    public void UpdateKnowledge(Store store)
    {
        StoreKnowledge knowledge = knownStores[store.ID];
        knowledge.Update(store);

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

    public bool KnowsStoreThatSellsProduct(int productID)
    {
        return knownStoresByProduct.ContainsKey(productID);
    }

    public StoreKnowledge GetStoreThatSellsProduct(int productID)
    {
        List<StoreKnowledge> stores = knownStoresByProduct[productID];
        // TODO: Implement some sort of filter
        return stores[0];
    }

    #endregion

    #region Exit Knowledge

    public bool KnowsAnyExit()
    {
        return knownExits.Count != 0;
    }

    public ExitKnowledge GetClosestExit(Vector2 position, int floor)
    {
        // TODO: Implement search by distance
        return knownExits[0];
    }

    #endregion

}
