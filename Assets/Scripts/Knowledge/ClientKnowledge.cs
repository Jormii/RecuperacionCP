using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientKnowledge
{
    private Dictionary<int, StoreKnowledge> knownStores;
    private Dictionary<Product, List<StoreKnowledge>> knownStoresByProduct;
    private Dictionary<int, ExitKnowledge> knownExits;

    public ClientKnowledge()
    {
        this.knownStores = new Dictionary<int, StoreKnowledge>();
        this.knownStoresByProduct = new Dictionary<Product, List<StoreKnowledge>>();
        this.knownExits = new Dictionary<int, ExitKnowledge>();

        // TODO: Remove this
        GameObject exit = GameObject.FindGameObjectWithTag("Exit");
        knownExits.Add(0, new ExitKnowledge(0, 0, exit.transform.position));
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
        StoreKnowledge knowledge = new StoreKnowledge(store.ID, store.Floor, store.transform.position);
        knownStores.Add(store.ID, knowledge);

        UpdateKnowledge(store);
    }

    public void UpdateKnowledge(Store store)
    {
        StoreKnowledge knowledge = knownStores[store.ID];
        knowledge.Update(store);

        foreach (Product product in store.GetProductsInStock())
        {
            if (knownStoresByProduct.ContainsKey(product))
            {
                List<StoreKnowledge> knowledges = knownStoresByProduct[product];
                if (!knowledges.Contains(knowledge))
                {
                    knowledges.Add(knowledge);
                }
            }
            else
            {
                List<StoreKnowledge> list = new List<StoreKnowledge>();
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
