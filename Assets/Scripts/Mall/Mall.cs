using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mall
{
    public const float MIN_X = -12.3f;
    public const float MAX_X = 12.3f;

    public static readonly Mall INSTANCE = new Mall();

    private int lowestFloor = int.MaxValue;
    private int highestFloor = int.MinValue;
    private Dictionary<int, Store> allStores;
    private Dictionary<int, List<Store>> storesInFloors;
    private Dictionary<int, List<Store>> storesThatSellProduct;
    private Dictionary<int, LocationData> exits;
    private Dictionary<int, LocationData> storages;
    private Dictionary<int, Stairs> allStairs;
    private Dictionary<int, List<Stairs>> stairsByFloor;

    private Mall()
    {
        this.allStores = new Dictionary<int, Store>();
        this.storesInFloors = new Dictionary<int, List<Store>>();
        this.storesThatSellProduct = new Dictionary<int, List<Store>>();
        this.exits = new Dictionary<int, LocationData>();
        this.storages = new Dictionary<int, LocationData>();
        this.allStairs = new Dictionary<int, Stairs>();
        this.stairsByFloor = new Dictionary<int, List<Stairs>>();
    }

    private void UpdateFloors(int floor)
    {
        lowestFloor = Mathf.Min(lowestFloor, floor);
        highestFloor = Mathf.Max(highestFloor, floor);
    }

    public bool AddStore(Store store)
    {
        if (allStores.ContainsKey(store.ID))
        {
            return false;
        }

        allStores.Add(store.ID, store);
        UpdateFloors(store.Location.FLOOR);

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
        UpdateFloors(exit.Floor);

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
        UpdateFloors(storage.Floor);

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

        LocationData startLocation = stairs.StartingLocation;
        LocationData endLocation = stairs.EndingLocation;

        allStairs.Add(stairs.ID, stairs);

        if (stairsByFloor.ContainsKey(startLocation.FLOOR))
        {
            stairsByFloor[startLocation.FLOOR].Add(stairs);
        }
        else
        {
            List<Stairs> list = new List<Stairs>();
            list.Add(stairs);
            stairsByFloor.Add(startLocation.FLOOR, list);
        }

        UpdateFloors(startLocation.FLOOR);
        UpdateFloors(endLocation.FLOOR);

        return true;
    }

    public Stairs GetClosestStairs(LocationData location, Stairs.Direction direction)
    {
        List<Stairs> stairsInFloor = stairsByFloor[location.FLOOR];

        Stairs closestStairs = null;
        float distanceToClosest = Mathf.Infinity;
        for (int i = 0; i < stairsInFloor.Count; ++i)
        {
            Stairs stairs = stairsInFloor[i];

            if (stairs.StairsDirection != direction)
            {
                continue;
            }

            float manhattanDistance = Utils.ManhattanDistance(location.POSITION, stairs.StartingLocation.POSITION);
            if (manhattanDistance < distanceToClosest)
            {
                closestStairs = stairs;
                distanceToClosest = manhattanDistance;
            }
        }

        return closestStairs;
    }

    public bool FloorExists(int floor)
    {
        return floor >= lowestFloor && floor <= highestFloor;
    }

}
