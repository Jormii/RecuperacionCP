using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Employee : Human
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
    private Dictionary<int, Dictionary<int, int>> productsToRefill;
    private Dictionary<int, int> productsBeingCarried;

    private Store lastStoreSeen;

    protected override void Start()
    {
        base.Start();

        currentState = EmployeeState.WanderingAround;
        productsToRefill = new Dictionary<int, Dictionary<int, int>>();
        productsBeingCarried = new Dictionary<int, int>();
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
                GoingToStorage();
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

    #region GoingToStorage related

    private void GoingToStorage()
    {
        if (debug)
        {
            Debug.LogFormat("Employee {0} is heading to storage", name);
        }

        LocationData currentLocation = new LocationData(transform.position, currentFloor);
        LocationData storageLocation = Mall.INSTANCE.GetClosestStorage(currentLocation);
        if (currentLocation.FLOOR != storageLocation.FLOOR)
        {
            // TODO: Consider different floors
            Vector2 stairsPosition = new Vector2();
            IAction moveToStairs = new MoveAction(navigation, stairsPosition, MoveAction.Destination.Stairs);
            AddActionToQueue(moveToStairs);
        }

        IAction moveToStorageAction = new MoveAction(navigation, storageLocation.POSITION, MoveAction.Destination.Storage);
        AddActionToQueue(moveToStorageAction);
        ExecuteActionQueue();
    }

    #endregion

    #region MovingToStore related

    private void MovingToStore()
    {
        if (debug)
        {
            Debug.LogFormat("Employee {0} is heading to store {1}", name, lastStoreSeen.name);
        }

        LocationData currentLocation = new LocationData(transform.position, currentFloor);
        LocationData storeLocation = lastStoreSeen.Location;
        if (currentLocation.FLOOR != storeLocation.FLOOR)
        {
            Vector2 stairsPosition = new Vector2();  // TODO: Use Mall
            IAction moveToStairs = new MoveAction(navigation, stairsPosition, MoveAction.Destination.Stairs);
            AddActionToQueue(moveToStairs);
        }

        IAction moveToStore = new MoveAction(navigation, storeLocation.POSITION, MoveAction.Destination.Store);
        AddActionToQueue(moveToStore);
        ExecuteActionQueue();
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

        ChangeState(EmployeeState.WanderingAround);
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

        // TODO: Consider changing floors
        Vector2 wanderDirection = new Vector2(
            (Random.Range(0f, 1f) > 0.5f) ? 1 : -1,
            0f
        );

        Vector2 wanderDestination = new Vector2(
            (wanderDirection.x < 0) ? Mall.MIN_X : Mall.MAX_X,
            transform.position.y
        );

        IAction wander = new MoveAction(navigation, wanderDestination, MoveAction.Destination.NoDestination);
        AddActionToQueue(wander);
        ExecuteActionQueue();
    }

    #endregion

    #endregion

    #region Human Functions

    public override void OnActionCompleted(IAction action)
    {
        if (action is MoveAction)
        {
            MoveAction moveAction = action as MoveAction;
            switch (moveAction.GetDestination)
            {
                case MoveAction.Destination.NoDestination:
                    ChangeState(EmployeeState.WanderingAround);
                    break;
                case MoveAction.Destination.Stairs:
                    break;
                case MoveAction.Destination.Storage:
                    OnStorageReached();
                    break;
                case MoveAction.Destination.Store:
                    OnStoreReached();
                    break;
                case MoveAction.Destination.Exit:
                default:
                    Debug.LogErrorFormat("Error in employee {0}. An employee's moving action can't have {1} as destination", name, moveAction.GetDestination);
                    break;
            }
        }

        base.OnActionCompleted(action);
    }

    private void OnStorageReached()
    {
        ChangeState(EmployeeState.MovingToStore);
    }

    private void OnStoreReached()
    {
        ChangeState(EmployeeState.ReStocking);
    }

    public override void OnActionQueueCompleted(IAction lastAction)
    {
        base.OnActionQueueCompleted(lastAction);
    }

    public override void OnStoreSeen(Store store)
    {
        if (currentState != EmployeeState.WanderingAround)
        {
            return;
        }

        ChangeState(EmployeeState.ObservingStock);
        lastStoreSeen = store;
    }

    #endregion

}
