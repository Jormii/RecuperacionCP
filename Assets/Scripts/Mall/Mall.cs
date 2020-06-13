using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mall
{
    public const float MIN_X = -12.3f;
    public const float MAX_X = 12.3f;

    public static readonly Mall INSTANCE = new Mall();

    private Dictionary<int, Store> allStores;
    private Dictionary<int, List<Store>> storesInFloors;
    private Dictionary<int, List<Store>> storesThatSellProduct;
    private Dictionary<int, LocationData> exits;
    private Dictionary<int, LocationData> storages;
    private Dictionary<int, LocationData> allStairs;    // TODO: Index by floor

    private Mall()
    {
        this.allStores = new Dictionary<int, Store>();
        this.storesInFloors = new Dictionary<int, List<Store>>();
        this.storesThatSellProduct = new Dictionary<int, List<Store>>();
        this.exits = new Dictionary<int, LocationData>();
        this.storages = new Dictionary<int, LocationData>();
        this.allStairs = new Dictionary<int, LocationData>();
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

        Stock stock = store.StoreStock;
        List<StockData> productsStock = stock.StockSold;
        for (int i = 0; i < productsStock.Count; ++i)
        {
            StockData productStock = productsStock[i];
            int productID = productStock.Product.ID;
            if (storesThatSellProduct.ContainsKey(productID))
            {
                storesThatSellProduct[productID].Add(store);
            }
            else
            {
                List<Store> stores = new List<Store>();
                stores.Add(store);
                storesThatSellProduct.Add(productID, stores);
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

    public List<Store> GetStoresThatSellProduct(int productID)
    {
        return new List<Store>(storesThatSellProduct[productID]);
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

    public bool AddStairs(Stairs stairs)
    {
        if (allStairs.ContainsKey(stairs.ID))
        {
            return false;
        }

        Vector2 position = stairs.transform.position;
        int floor = stairs.Floor;
        LocationData location = new LocationData(position, floor);
        allStairs.Add(stairs.ID, location);
        return true;
    }

    public LocationData GetClosestStairs(LocationData location)
    {
        foreach (LocationData stairLocation in allStairs.Values)
        {
            if (stairLocation.FLOOR == location.FLOOR)
            {
                return stairLocation;
            }
        }

        Debug.LogError("Couldn't find  stairs. This can't happen");
        return new LocationData(location.POSITION, location.FLOOR);
    }

    public LocationData GetStairsOnFloor(int floor)
    {
        foreach (LocationData stairsLocation in allStairs.Values)
        {
            if (stairsLocation.FLOOR == floor)
            {
                return stairsLocation;
            }
        }

        Debug.LogError("Couldn't find  stairs. This can't happen");
        return new LocationData(new Vector2(), floor);
    }

}
