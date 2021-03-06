﻿using System.Collections.Generic;
using UnityEngine;

public class Mall
{
    public const float MALL_LEFT_LIMIT = -7.8f;
    public const float MALL_RIGHT_LIMIT = 7.8f;

    public static readonly Mall INSTANCE = new Mall();

    private int lowestFloor = int.MaxValue;
    private int highestFloor = int.MinValue;
    private bool closed = false;

    // Stores variables
    private Dictionary<int, Store> allStores;
    private Dictionary<int, List<Store>> storesInFloors;
    private Dictionary<int, List<Store>> storesThatSellProduct;

    // Stairs variables
    private Dictionary<int, Stairs> allStairs;
    private Dictionary<int, List<Stairs>> stairsByFloor;

    // Exits, storages and clients
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

    private void UpdateFloors(int floor)
    {
        lowestFloor = Mathf.Min(lowestFloor, floor);
        highestFloor = Mathf.Max(highestFloor, floor);
    }

    public void Close()
    {
        closed = true;

        foreach (Store store in allStores.Values)
        {
            store.Close();
        }
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

    public void UpdateStore(Store store, StockChanges changes)
    {
        int storeID = store.ID;
        foreach (int productID in changes.PRODUCTS_TO_REMOVE)
        {
            storesThatSellProduct[productID].Remove(store);
        }

        Stock stock = store.StoreStock;
        List<StockData> productsStock = stock.StockSold;
        for (int i = 0; i < productsStock.Count; ++i)
        {
            int productID = productsStock[i].Product.ID;
            if (storesThatSellProduct.ContainsKey(productID))
            {
                List<Store> storesList = storesThatSellProduct[productID];
                if (!storesList.Contains(store))
                {
                    storesList.Add(store);
                }
            }
            else
            {
                List<Store> storesList = new List<Store>();
                storesList.Add(store);
                storesThatSellProduct.Add(productID, storesList);
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

    public List<Store> GetAllStores()
    {
        return new List<Store>(allStores.Values);
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

        LocationData locationData = exit.Location;
        exits.Add(exit.ID, locationData);
        UpdateFloors(locationData.FLOOR);
    }

    public LocationData GetClosestExit(LocationData location)
    {
        LocationData closestExit = new LocationData();
        float distanceToClosestExit = Mathf.Infinity;

        foreach (LocationData exits in exits.Values)
        {
            float manhattanDistance = Utils.ManhattanDistance(location.POSITION, exits.POSITION);
            if (manhattanDistance < distanceToClosestExit)
            {
                closestExit = exits;
                distanceToClosestExit = manhattanDistance;
            }
        }

        return closestExit;
    }

    public List<LocationData> GetAllExits()
    {
        List<LocationData> exitsLocations = new List<LocationData>();
        foreach (LocationData location in exits.Values)
        {
            exitsLocations.Add(location);
        }

        return exitsLocations;
    }

    #endregion

    #region Storage Related

    public void AddStorage(Storage storage)
    {
        if (storages.ContainsKey(storage.ID))
        {
            return;
        }

        LocationData locationData = storage.Location;
        storages.Add(storage.ID, locationData);
        UpdateFloors(locationData.FLOOR);
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

    public bool Closed
    {
        get => closed;
    }

    #endregion
}
