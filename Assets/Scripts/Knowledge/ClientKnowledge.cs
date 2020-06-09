using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientKnowledge
{
    public Dictionary<int, StoreKnowledge> knownStores;
    public Dictionary<Product, List<StoreKnowledge>> knownStoresByProduct;
    public Dictionary<int, ExitKnowledge> knownExits;

    public ClientKnowledge()
    {
        this.knownStores = new Dictionary<int, StoreKnowledge>();
        this.knownStoresByProduct = new Dictionary<Product, List<StoreKnowledge>>();
        this.knownExits = new Dictionary<int, ExitKnowledge>();
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
        StoreKnowledge knowledge = new StoreKnowledge(store.ID, store.floor, store.transform.position);
        knownStores.Add(store.ID, knowledge);

        UpdateKnowledge(store);
    }

    public void UpdateKnowledge(Store store)
    {
        knownStores[store.ID].Update(store);

        foreach (Product product in store.stock.productsStock.Keys)
        {
            if (knownStoresByProduct.ContainsKey(product))
            {
                List<StoreKnowledge> knowledges = knownStoresByProduct[product];
                StoreKnowledge knowledge = GetKnowledge(store);
                if (!knowledges.Contains(knowledge))
                {
                    knowledges.Add(knowledge);
                }
            }
            else
            {
                List<StoreKnowledge> list = new List<StoreKnowledge>();
                StoreKnowledge knowledge = new StoreKnowledge(store.ID, store.floor, store.transform.position);

                list.Add(knowledge);
                knownStoresByProduct.Add(product, list);
            }

        }
    }

    public bool KnowsStoreThatSellsProduct(Product product)
    {
        return knownStoresByProduct.ContainsKey(product);
    }

    public StoreKnowledge GetStoreThatSellsProduct(Product product)
    {
        List<StoreKnowledge> stores = knownStoresByProduct[product];
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
        // TODO
        return new ExitKnowledge(-1, 0, position);
    }

    #endregion

}
