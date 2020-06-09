using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mall
{
    public const float MIN_X = -12.3f;
    public const float MAX_X = 12.3f;

    public static readonly Mall INSTANCE = new Mall();
    public Dictionary<int, Store> allStores;
    public Dictionary<int, List<Store>> storesInFloors;
    public Dictionary<Product, List<Store>> storesThatSellProduct;

    public GameObject exit; // TODO: Remove

    public Mall()
    {
        allStores = new Dictionary<int, Store>();
        storesInFloors = new Dictionary<int, List<Store>>();
        storesThatSellProduct = new Dictionary<Product, List<Store>>();

        exit = GameObject.FindGameObjectWithTag("Exit");
    }

    public bool AddStore(Store store)
    {
        if (allStores.ContainsKey(store.ID))
        {
            return false;
        }

        allStores.Add(store.ID, store);

        if (storesInFloors.ContainsKey(store.Floor))
        {
            storesInFloors[store.Floor].Add(store);
        }
        else
        {
            List<Store> stores = new List<Store>();
            stores.Add(store);
            storesInFloors.Add(store.Floor, stores);
        }

        List<Product> productsInStock = store.GetProductsInStock();
        for (int i = 0; i < productsInStock.Count; ++i)
        {
            Product p = productsInStock[i];
            if (storesThatSellProduct.ContainsKey(p))
            {
                storesThatSellProduct[p].Add(store);
            }
            else
            {
                List<Store> stores = new List<Store>();
                stores.Add(store);
                storesThatSellProduct.Add(p, stores);
            }
        }

        return true;
    }

    public bool ExistsStoreWithID(int id)
    {
        return allStores.ContainsKey(id);
    }

    public Store GetStoreByID(int id)
    {
        return allStores[id];
    }

}
