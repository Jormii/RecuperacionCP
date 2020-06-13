﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Employee : Agent
{

    private enum EmployeeState
    {
        WanderingAround,
        ObservingStock,
        MovingToStorage,
        MovingToStore,
        ReStocking,
        Error
    }

    [SerializeField] private EmployeeState currentState = EmployeeState.WanderingAround;
    [SerializeField] private List<int> floorsInCharge;
    private Dictionary<int, Dictionary<int, int>> productsToRefill;
    private Dictionary<int, int> productsBeingCarried;

    private Store lastStoreSeen;
    private bool hasVisitedStorage;
    private bool interrupted;

    protected override void Start()
    {
        base.Start();

        currentState = EmployeeState.WanderingAround;
        productsToRefill = new Dictionary<int, Dictionary<int, int>>();
        productsBeingCarried = new Dictionary<int, int>();

        Boss.INSTANCE.AddEmployee(this);
    }

    public bool CanBeInterrupted()
    {
        switch (currentState)
        {
            case EmployeeState.MovingToStorage:
            case EmployeeState.MovingToStore:
            case EmployeeState.WanderingAround:
                return !interrupted;
            default:
                return false;
        }
    }

    public void Interrupt()
    {
        interrupted = true;
        if (ExecutingActionQueue)
        {
            PauseActionQueue();
        }
    }

    public void ContinueTasks()
    {
        interrupted = false;
        if (ThereAreActionsLeft())
        {
            ExecuteActionQueue();
        }
    }

    public void SendToReStock(Store store, Dictionary<int, int> reStock)
    {
        if (debug)
        {
            Debug.LogFormat("Employee {0}: Boss has asked them to restock store {1}", name, store.name);
        }

        if (productsToRefill.ContainsKey(store.ID))
        {
            return;
        }

        lastStoreSeen = store;
        StopExecutingActionQueue();
        MoveProductsFromCarriedToRefill(store.ID, reStock);
        if (AlreadyHasProductsWanted(store.ID, reStock))
        {
            ChangeState(EmployeeState.MovingToStore);
        }
        else
        {
            AddRestOfProducts(store.ID, reStock);
            ChangeState(EmployeeState.MovingToStorage);
        }
    }

    private void MoveProductsFromCarriedToRefill(int storeID, Dictionary<int, int> reStock)
    {
        // Store what products to move
        Dictionary<int, int> productsToMove = new Dictionary<int, int>();
        foreach (KeyValuePair<int, int> entry in productsBeingCarried)
        {
            int productID = entry.Key;
            int amount = entry.Value;

            if (reStock.ContainsKey(productID))
            {
                productsToMove.Add(productID, amount);
            }
        }

        // Delete them from carried
        foreach (int productID in productsToMove.Keys)
        {
            productsBeingCarried.Remove(productID);
        }

        // Add them to store's refill
        if (!productsToRefill.ContainsKey(storeID))
        {
            productsToRefill.Add(storeID, new Dictionary<int, int>());
        }

        Dictionary<int, int> refill = productsToRefill[storeID];
        foreach (KeyValuePair<int, int> entry in productsToMove)
        {
            int productID = entry.Key;
            int amount = entry.Value;

            if (refill.ContainsKey(productID))
            {
                refill[productID] += amount;
            }
            else
            {
                refill.Add(productID, amount);
            }
        }
    }

    private bool AlreadyHasProductsWanted(int storeID, Dictionary<int, int> reStock)
    {
        if (productsToRefill.ContainsKey(storeID))
        {
            Dictionary<int, int> reStockAlreadyNoted = productsToRefill[storeID];
            foreach (int productID in reStock.Keys)
            {
                if (!reStockAlreadyNoted.ContainsKey(productID))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void AddRestOfProducts(int storeID, Dictionary<int, int> reStock)
    {
        Dictionary<int, int> refill = productsToRefill[storeID];
        foreach (KeyValuePair<int, int> entry in reStock)
        {
            int productID = entry.Key;
            int desiredAmount = entry.Value;

            if (refill.ContainsKey(productID))
            {
                refill[productID] = Mathf.Max(refill[productID], desiredAmount);
            }
            else
            {
                refill.Add(productID, desiredAmount);
            }
        }
    }

    public List<StoreKnowledge> ShareKnowledge(List<int> productsIDs)
    {
        List<StoreKnowledge> givenKnowledge = new List<StoreKnowledge>();
        for (int i = 0; i < productsIDs.Count; ++i)
        {
            int productID = productsIDs[i];
            List<Store> stores = Mall.INSTANCE.GetStoresThatSellProduct(productID);
            for (int j = 0; j < stores.Count; ++j)
            {
                Store store = stores[j];
                int storeFloor = store.Floor;
                if (!floorsInCharge.Contains(storeFloor))
                {
                    continue;
                }

                StoreKnowledge knowledge = new StoreKnowledge(store.ID, store.Location);
                knowledge.Update(store);

                givenKnowledge.Add(knowledge);
            }
        }

        return givenKnowledge;
    }

    #region States functions

    private void ChangeState(EmployeeState state)
    {
        currentState = state;
        OnStateChanged();
    }

    protected override void PerformCurrentState()
    {
        switch (currentState)
        {
            case EmployeeState.MovingToStorage:
                MovingToStorage();
                break;
            case EmployeeState.MovingToStore:
                MovingToStore();
                break;
            case EmployeeState.ObservingStock:
                ObservingStock();
                break;
            case EmployeeState.ReStocking:
                ReStocking();
                break;
            case EmployeeState.WanderingAround:
                WanderingAround();
                break;
            case EmployeeState.Error:
            default:
                Debug.LogErrorFormat("Something wrong happened in employee {0}'s state machine. Destroying", name);
                Destroy(gameObject);
                break;
        }
    }

    #region MovingToStorage related

    private void MovingToStorage()
    {
        if (debug)
        {
            Debug.LogFormat("Employee {0} is heading to storage", name);
        }

        LocationData currentLocation = new LocationData(transform.position, currentFloor);
        LocationData storageLocation = Mall.INSTANCE.GetClosestStorage(currentLocation);
        MoveTo(storageLocation, MoveAction.Destination.Storage);
    }

    #endregion

    #region MovingToStore related

    private void MovingToStore()
    {
        Store store = ChooseClosestStore();
        lastStoreSeen = store;

        if (debug)
        {
            Debug.LogFormat("Employee {0} is heading to store {1}", name, lastStoreSeen.name);
        }

        LocationData storeLocation = lastStoreSeen.Location;
        MoveToStore(storeLocation, lastStoreSeen.ID);
    }

    private Store ChooseClosestStore()
    {
        if (productsToRefill.Count == 0)
        {
            return lastStoreSeen;
        }

        Vector2 currentPosition = transform.position;
        Store closestStore = null;
        float distanceToClosest = Mathf.Infinity;
        foreach (int storeID in productsToRefill.Keys)
        {
            Store store = Mall.INSTANCE.GetStoreByID(storeID);
            Vector2 storePosition = store.Location.POSITION;
            float manhattanDistance = Utils.ManhattanDistance(currentPosition, storePosition);

            if (manhattanDistance < distanceToClosest)
            {
                closestStore = store;
                distanceToClosest = manhattanDistance;
            }
        }

        return closestStore;
    }

    #endregion

    #region ObservingStock related

    private void ObservingStock()
    {
        if (debug)
        {
            Debug.LogFormat("Employee {0} is observing {1}'s stock", name, lastStoreSeen.name);
        }

        Stock stock = lastStoreSeen.StoreStock;
        if (stock.NeedsReStocking())
        {
            if (EmployeeCarriesUnstockedProducts())
            {
                if (debug)
                {
                    Debug.LogFormat("Employee {0} is carrying products store {1} needs", name, lastStoreSeen.name);
                }

                ChangeState(EmployeeState.MovingToStore);
            }
            else
            {
                if (debug)
                {
                    Debug.LogFormat("Employee {0}: Store {1} needs restocking", name, lastStoreSeen.name);
                }

                int storeID = lastStoreSeen.ID;
                Dictionary<int, int> reStockNeeded = stock.GetProductsToRefill();

                productsToRefill.Add(storeID, reStockNeeded);
                ChangeState(EmployeeState.MovingToStorage);
            }
        }
        else
        {
            if (debug)
            {
                Debug.LogFormat("Employee {0}: Store {1} needs no restocking", name, lastStoreSeen.name);
            }

            // If was wandering before
            if (ExecutingActionQueue)
            {
                // Continue wandering
                currentState = EmployeeState.WanderingAround;
            }
            else
            {
                ChangeState(EmployeeState.WanderingAround);
            }
        }
    }

    private bool EmployeeCarriesUnstockedProducts()
    {
        Stock stock = lastStoreSeen.StoreStock;
        foreach (int productID in productsBeingCarried.Keys)
        {
            if (stock.HasProductInStock(productID) && stock.ProductNeedsReStock(productID))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region ReStocking related

    private void ReStocking()
    {
        if (debug)
        {
            Debug.LogFormat("Employee {0} is restocking store {1}", name, lastStoreSeen.name);
        }

        Dictionary<int, int> restock = new Dictionary<int, int>();

        // Comes from the storage
        if (productsToRefill.ContainsKey(lastStoreSeen.ID))
        {
            restock = productsToRefill[lastStoreSeen.ID];
        }
        // Was wandering and saw a store in need of stock
        else
        {
            restock = productsBeingCarried;
        }

        productsToRefill.Remove(lastStoreSeen.ID);

        Stock stock = lastStoreSeen.StoreStock;
        Dictionary<int, int> overStock = stock.ReStock(restock);
        HandleUnnecessaryStock(overStock);

        // Still has products to restock
        if (productsToRefill.Count != 0)
        {
            if (hasVisitedStorage)
            {
                ChangeState(EmployeeState.MovingToStore);
            }
            else
            {
                ChangeState(EmployeeState.MovingToStorage);
            }
        }
        else
        {
            ChangeState(EmployeeState.WanderingAround);
        }
    }

    private void HandleUnnecessaryStock(Dictionary<int, int> stock)
    {
        foreach (KeyValuePair<int, int> entry in stock)
        {
            int productID = entry.Key;
            int amount = entry.Value;

            if (debug)
            {
                Debug.LogFormat("Employee {0} found overstock of product {1}. Amount: {2}", name, productID, amount);
            }

            if (productsBeingCarried.ContainsKey(productID))
            {
                productsBeingCarried[productID] += amount;
            }
            else
            {
                productsBeingCarried.Add(productID, amount);
            }
        }
    }

    #endregion

    #region WanderingAroundRelated

    private void WanderingAround()
    {
        if (debug)
        {
            Debug.LogFormat("Employee {0} is wandering around", name);
        }

        hasVisitedStorage = false;

        Vector2 wanderDestination = CalculateWanderDestination();
        int floorToGo = currentFloor;
        if (ShouldChangeFloors())
        {
            floorToGo = CalculateFloorToVisit();
        }

        LocationData wanderLocation = new LocationData(wanderDestination, floorToGo);
        MoveTo(wanderLocation, MoveAction.Destination.NoDestination);
    }

    private Vector2 CalculateWanderDestination()
    {
        // TODO
        Vector2 wanderDirection = new Vector2(
            (Random.Range(0f, 1f) > 0.5f) ? 1 : -1,
            0f
        );

        Vector2 wanderDestination = new Vector2(
            (wanderDirection.x < 0) ? Mall.MIN_X : Mall.MAX_X,
            transform.position.y
        );

        return wanderDestination;
    }

    private int CalculateFloorToVisit()
    {
        if (floorsInCharge.Count == 1)
        {
            return currentFloor;
        }

        List<int> floors = new List<int>(floorsInCharge);
        floors.Remove(currentFloor);

        int randomIndex = Random.Range(0, floors.Count - 1);
        return floors[randomIndex];
    }

    #endregion

    #endregion

    #region Agent Functions

    public override void OnActionCompleted(IAction action)
    {
        if (action is MoveAction)
        {
            MoveAction moveAction = action as MoveAction;
            switch (moveAction.GetDestination)
            {
                case MoveAction.Destination.NoDestination:
                    OnNoDestinationReached(moveAction);
                    break;
                case MoveAction.Destination.Stairs:
                    OnStairsReached(moveAction);
                    break;
                case MoveAction.Destination.StairsEnd:
                    OnStairsEndReached(moveAction);
                    break;
                case MoveAction.Destination.Storage:
                    OnStorageReached(moveAction);
                    break;
                case MoveAction.Destination.Store:
                    OnStoreReached(moveAction);
                    break;
                case MoveAction.Destination.Exit:
                default:
                    Debug.LogErrorFormat("Error in employee {0}. An employee's moving action can't have {1} as destination", name, moveAction.GetDestination);
                    break;
            }
        }

        base.OnActionCompleted(action);
    }

    private void OnNoDestinationReached(MoveAction moveAction)
    {
        ChangeState(EmployeeState.WanderingAround);
    }

    private void OnStairsReached(MoveAction moveAction) { }

    private void OnStairsEndReached(MoveAction moveAction)
    {
        int newFloor = moveAction.Location.FLOOR;
        currentFloor = newFloor;
    }

    private void OnStorageReached(MoveAction moveAction)
    {
        hasVisitedStorage = true;
        ChangeState(EmployeeState.MovingToStore);
    }

    private void OnStoreReached(MoveAction moveAction)
    {
        MoveToStoreAction moveToStoreAction = moveAction as MoveToStoreAction;

        lastStoreSeen = Mall.INSTANCE.GetStoreByID(moveToStoreAction.STORE_ID);
        ChangeState(EmployeeState.ReStocking);
    }

    public override void OnActionQueueCompleted(IAction lastAction)
    {
        base.OnActionQueueCompleted(lastAction);
    }

    public override void OnStoreSeen(Store store)
    {
        // Employee already has noted that this store needs restocking
        if (productsToRefill.ContainsKey(store.ID))
        {
            return;
        }

        switch (currentState)
        {
            case EmployeeState.MovingToStorage:
            case EmployeeState.WanderingAround:
                ChangeState(EmployeeState.ObservingStock);
                lastStoreSeen = store;
                break;
            default:
                break;
        }
    }

    public override void OnOtherAgentSeen(Agent agent)
    {
        if (debug)
        {
            Debug.LogFormat("Employee {0} has seen the agent {1}", name, agent.name);
        }
    }

    public override void OnExitSeen(Exit exit) { }

    #endregion
}