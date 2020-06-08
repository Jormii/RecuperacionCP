using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mall
{
    public static readonly Mall INSTANCE = new Mall();
    public HashSet<Store> allStores;
    public Dictionary<int, List<Store>> storesInFloors;
    public Dictionary<Product, List<Store>> storesThatSellProduct;

    public GameObject exit; // TODO: Remove

    public Mall()
    {
        allStores = new HashSet<Store>();
        storesInFloors = new Dictionary<int, List<Store>>();
        storesThatSellProduct = new Dictionary<Product, List<Store>>();

        exit = GameObject.FindGameObjectWithTag("Exit");
    }

    public bool AddStore(Store store)
    {
        if (allStores.Contains(store))
        {
            return false;
        }

        StoreData data = store.storeData;
        allStores.Add(store);

        if (storesInFloors.ContainsKey(data.floor))
        {
            storesInFloors[data.floor].Add(store);
        }
        else
        {
            List<Store> stores = new List<Store>();
            stores.Add(store);
            storesInFloors.Add(data.floor, stores);
        }

        Stock stock = store.stock;
        for (int i = 0; i < stock.productsInStock.Count; ++i)
        {
            Product p = stock.productsInStock[i];
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

}
