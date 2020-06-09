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
    public Dictionary<int, LocationData> exits;
    public Dictionary<int, LocationData> storages;

    public Mall()
    {
        allStores = new Dictionary<int, Store>();
        storesInFloors = new Dictionary<int, List<Store>>();
        storesThatSellProduct = new Dictionary<Product, List<Store>>();
        exits = new Dictionary<int, LocationData>();
        storages = new Dictionary<int, LocationData>();
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

    public bool AddExit(Exit exit)
    {
        if (exits.ContainsKey(exit.ID))
        {
            return false;
        }

        LocationData locationData = new LocationData(exit.transform.position, exit.Floor);
        exits.Add(exit.ID, locationData);
        return true;
    }

    public bool AddStorage(Storage storage)
    {
        if (storages.ContainsKey(storage.ID))
        {
            return false;
        }

        LocationData locationData = new LocationData(storage.transform.position, storage.Floor);
        storages.Add(storage.ID, locationData);
        return true;
    }

    public LocationData GetClosestStorage(LocationData location)
    {
        // TODO
        return new List<LocationData>(storages.Values)[0];
    }

}
