using System.Collections.Generic;
using UnityEngine;

public class Mall
{
    // TODO: Tweak these variables when graphics are in
    public const float MALL_LEFT_LIMIT = -12.3f;
    public const float MALL_RIGHT_LIMIT = 12.3f;

    public static readonly Mall INSTANCE = new Mall();

    private int lowestFloor = int.MaxValue;
    private int highestFloor = int.MinValue;

    // Stores variables
    private Dictionary<int, Store> allStores;
    private Dictionary<int, List<Store>> storesInFloors;
    private Dictionary<int, List<Store>> storesThatSellProduct;

    // Stairs variables
    private Dictionary<int, Stairs> allStairs;
    private Dictionary<int, List<Stairs>> stairsByFloor;

    // Exits and storages
    private Dictionary<int, LocationData> exits;
    private Dictionary<int, LocationData> storages;

    private Mall()
    {
        this.allStores = new Dictionary<int, Store>();
        this.storesInFloors = new Dictionary<int, List<Store>>();
        this.storesThatSellProduct = new Dictionary<int, List<Store>>();

        this.allStairs = new Dictionary<int, Stairs>();
        this.stairsByFloor = new Dictionary<int, List<Stairs>>();

        this.exits = new Dictionary<int, LocationData>();
        this.storages = new Dictionary<int, LocationData>();
    }

    public bool FloorExists(int floor)
    {
        return floor >= lowestFloor && floor <= highestFloor;
    }

    private void UpdateFloors(int floor)
    {
        lowestFloor = Mathf.Min(lowestFloor, floor);
        highestFloor = Mathf.Max(highestFloor, floor);
    }

    #region Store Related

    public void AddStore(Store store)
    {
        if (allStores.ContainsKey(store.ID))
        {
            return;
        }

        allStores.Add(store.ID, store);
        UpdateFloors(store.Location.FLOOR);

        if (storesInFloors.ContainsKey(store.Floor))
        {
            storesInFloors[store.Floor].Add(store);
        }
        else
        {
            List<Store> list = new List<Store>();
            list.Add(store);
            storesInFloors.Add(store.Floor, list);
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
                List<Store> list = new List<Store>();
                list.Add(store);
                storesThatSellProduct.Add(productID, list);
            }
        }
    }

    public Store GetStoreByID(int id)
    {
        return allStores[id];
    }

    public List<Store> GetStoresThatSellProduct(int productID)
    {
        return new List<Store>(storesThatSellProduct[productID]);
    }

    #endregion

    #region Stairs Related

    public void AddStairs(Stairs stairs)
    {
        if (allStairs.ContainsKey(stairs.ID))
        {
            return;
        }

        LocationData startLocation = stairs.StartingLocation;
        LocationData endLocation = stairs.EndingLocation;

        allStairs.Add(stairs.ID, stairs);
        UpdateFloors(startLocation.FLOOR);
        UpdateFloors(endLocation.FLOOR);

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

    #endregion

    #region Exit Related

    public void AddExit(Exit exit)
    {
        if (exits.ContainsKey(exit.ID))
        {
            return;
        }

        LocationData locationData = new LocationData(exit.transform.position, exit.Floor);
        exits.Add(exit.ID, locationData);
        UpdateFloors(exit.Floor);
    }

    // TODO: GetClosestExit?

    #endregion

    #region Storage Related

    public void AddStorage(Storage storage)
    {
        if (storages.ContainsKey(storage.ID))
        {
            return;
        }

        LocationData locationData = new LocationData(storage.transform.position, storage.Floor);
        storages.Add(storage.ID, locationData);
        UpdateFloors(storage.Floor);
    }

    public LocationData GetClosestStorage(LocationData location)
    {
        LocationData closestStorage = new LocationData();
        float distanceToClosestStorage = Mathf.Infinity;

        foreach (LocationData storage in storages.Values)
        {
            float manhattanDistance = Utils.ManhattanDistance(location.POSITION, storage.POSITION);
            if (manhattanDistance < distanceToClosestStorage)
            {
                closestStorage = storage;
                distanceToClosestStorage = manhattanDistance;
            }
        }

        return closestStorage;
    }

    #endregion

    #region Properties

    public int LowestFloor
    {
        get => lowestFloor;
    }

    public int HighestFloor
    {
        get => highestFloor;
    }

    #endregion
}
