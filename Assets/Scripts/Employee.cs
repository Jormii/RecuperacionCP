using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Employee : Human
{

    private enum EmployeeState
    {
        Init,
        DeInit,
        Error,
        WanderingAround,
        GoingToStorage,
        ReStocking
    }

    [SerializeField] private EmployeeState currentState = EmployeeState.Init;
    private Dictionary<int, Dictionary<Product, int>> productsToRefill;
    private Dictionary<int, Dictionary<Product, int>> productsCarrying;

    protected override void Start()
    {
        base.Start();

        Init();
        productsToRefill = new Dictionary<int, Dictionary<Product, int>>();
        productsCarrying = new Dictionary<int, Dictionary<Product, int>>();
    }

    private void Update()
    {
        PerformCurrentState();
    }

    #region States functions

    private void PerformCurrentState()
    {
        switch (currentState)
        {
            case EmployeeState.DeInit:
                DeInit();
                break;
            case EmployeeState.GoingToStorage:
                GoingToStorage();
                break;
            case EmployeeState.ReStocking:
                ReStocking();
                break;
            case EmployeeState.WanderingAround:
                WanderingAround();
                break;
            case EmployeeState.Error:
            case EmployeeState.Init:
            default:
                Debug.LogErrorFormat("Something wrong happened in employee {0}'s state machine. Destroying", name);
                DeInit();
                Destroy(gameObject);
                break;
        }
    }

    private void GoingToStorage()
    {
        // TODO
    }

    private void ReStocking()
    {
        // TODO
    }

    private void WanderingAround()
    {
        // TODO
    }

    #endregion

    private void GoToStorage()
    {
        Debug.LogFormat("Employee {0} is heading to storage", name);

        LocationData currentLocation = new LocationData(transform.position, currentFloor);
        LocationData storageLocation = Mall.INSTANCE.GetClosestStorage(currentLocation);

        if (storageLocation.FLOOR != currentFloor)
        {
            Vector2 stairsPosition = new Vector2();     // TODO: Obtain from Mall
            IAction moveToStairs = new MoveAction(navigation, stairsPosition, MoveAction.Destination.Stairs);
            AddActionToQueue(moveToStairs);
        }

        IAction moveToStorageAction = new MoveAction(navigation, storageLocation.POSITION, MoveAction.Destination.Storage);
        AddActionToQueue(moveToStorageAction);

        currentState = EmployeeState.GoingToStorage;
        ExecuteActionQueue();
    }

    private void WanderAround()
    {
        Debug.LogFormat("Employee {0} is wandering", name);
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

        currentState = EmployeeState.WanderingAround;
        ExecuteActionQueue();
    }

    #region Human Functions

    public override void Init()
    {
        WanderAround();
    }

    public override void DeInit()
    {
        Debug.LogWarningFormat("{0} deinits", name);
        gameObject.SetActive(false);
    }

    public override void OnActionCompleted(IAction action)
    {
        base.OnActionCompleted(action);
    }

    public override void OnActionQueueCompleted(IAction lastAction)
    {
        base.OnActionQueueCompleted(lastAction);

        if (lastAction is MoveAction)
        {
            MoveAction moveAction = lastAction as MoveAction;
            switch (moveAction.GetDestination)
            {
                case MoveAction.Destination.NoDestination:
                    WanderAround();
                    break;
                case MoveAction.Destination.Stairs:
                    // TODO
                    break;
                case MoveAction.Destination.Storage:
                    // TODO
                    break;
                case MoveAction.Destination.Store:
                    // TODO
                    break;
                default:
                    Debug.LogWarningFormat("Error in employee {0}, destination {1} is impossible for an employee", name, moveAction.GetDestination);
                    break;
            }
        }
    }

    public override void OnStoreSeen(Store store)
    {
        if (currentState == EmployeeState.WanderingAround || currentState == EmployeeState.GoingToStorage)
        {
            if (productsToRefill.ContainsKey(store.ID))
            {
                return;
            }

            if (store.NeedsReStocking())
            {
                Dictionary<Product, int> reStock = store.GetStockToRefill();
                productsToRefill.Add(store.ID, reStock);

                if (ExecutingActionQueue)
                {
                    StopExecutingActionQueue();
                }

                GoToStorage();
            }
        }
    }

    public override void UponReachingDestination()
    {
        switch (currentState)
        {
            case EmployeeState.GoingToStorage:
                Debug.LogFormat("Employee {0} reached the storage", name);
                break;
            case EmployeeState.ReStocking:
                break;
            case EmployeeState.WanderingAround:
                currentState = EmployeeState.WanderingAround;
                break;
            default:
                Debug.LogWarningFormat("UponReachingDestination called in unvalid state {0}", currentState);
                break;
        }

        base.UponReachingDestination();
    }

    #endregion

}
