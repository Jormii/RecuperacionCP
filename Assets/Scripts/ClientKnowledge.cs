using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientKnowledge
{

    public Dictionary<Product, List<StoreData>> knownStores;

    public ClientKnowledge()
    {
        this.knownStores = new Dictionary<Product, List<StoreData>>();
    }

    public StoreData GetStoreThatSellsProduct(Product product)
    {
        // TODO: Remove
        GameObject store = GameObject.FindGameObjectWithTag("Store");
        return store.GetComponent<Store>().storeData;
        // END TODO

        if (!knownStores.ContainsKey(product))
        {
            return null;
        }

        List<StoreData> stores = knownStores[product];
        return stores[0];
    }

}
