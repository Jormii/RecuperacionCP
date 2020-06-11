using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : Agent
{
    public enum ClientState
    {
        Evaluating,
        MovingToStore,
        CheckingStock,
        Buying,
        WanderingAround,
        AskingForInformation,
        Leaving,
        Error
    };

    public const float IGNORE_STORE_TIME = 1.5f;

    [SerializeField] private ClientState currentState = ClientState.Evaluating;
    private ClientKnowledge knowledge;
    private ClientResources resources;
    private Dictionary<int, float> storesIgnored;

    private StoreKnowledge storeInterestedIn;
    private Employee employeeFound;

    protected override void Start()
    {
        base.Start();

        currentState = ClientState.Evaluating;
        knowledge = new ClientKnowledge();
        resources = GetComponent<ClientResources>();
        storesIgnored = new Dictionary<int, float>();
    }

    protected override void Update()
    {
        base.Update();

        UpdateIgnoredStores();
    }

    private void IgnoreStoreTemporarily()
    {
        storesIgnored.Add(storeInterestedIn.STORE_ID, IGNORE_STORE_TIME);
    }

    private void UpdateIgnoredStores()
    {
        Dictionary<int, float> newDictionary = new Dictionary<int, float>();
        foreach (KeyValuePair<int, float> entry in storesIgnored)
        {
            int storeID = entry.Key;
            float time = entry.Value;

            float newTime = time - Time.deltaTime;
            if (newTime > 0)
            {
                newDictionary.Add(storeID, newTime);
            }
        }

        storesIgnored.Clear();
        storesIgnored = newDictionary;
    }

    #region States functions

    private void ChangeState(ClientState state)
    {
        currentState = state;
        OnStateChanged();
    }

    protected override void PerformCurrentState()
    {
        switch (currentState)
        {
            case ClientState.AskingForInformation:
                AskingForInformation();
                break;
            case ClientState.Buying:
                Buying();
                break;
            case ClientState.CheckingStock:
                CheckingStock();
                break;
            case ClientState.Evaluating:
                Evaluating();
                break;
            case ClientState.MovingToStore:
                MovingToStore();
                break;
            case ClientState.Leaving:
                Leaving();
                break;
            case ClientState.WanderingAround:
                WanderingAround();
                break;
            case ClientState.Error:
            default:
                Debug.LogErrorFormat("Something wrong happened in client {0}'s state machine. Destroying", name);
                Destroy(gameObject);
                break;
        }
    }

    #region AskingForInformationRelated

    private void AskingForInformation()
    {
        AskForInformation(employeeFound);
        StopExecutingActionQueue();
        ChangeState(ClientState.Evaluating);
    }

    private void AskForInformation(Employee employee)
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} asks employee {1} for information on products they want", name, employee.name);
        }

        List<int> productsIDsNotBoughtYet = resources.GetProductsNotBoughtYet();
        List<StoreKnowledge> employeesKnowledge = employee.ShareKnowledge(productsIDsNotBoughtYet);
        for (int i = 0; i < employeesKnowledge.Count; ++i)
        {
            StoreKnowledge givenKnowledge = employeesKnowledge[i];
            if (knowledge.KnowsStore(givenKnowledge.STORE_ID))
            {
                knowledge.UpdateKnowledge(givenKnowledge);
            }
            else
            {
                knowledge.CreateKnowledge(givenKnowledge);
            }
        }

        ChangeState(ClientState.Evaluating);
        employeeFound.ContinueTasks();
    }

    #endregion

    #region Buying related

    private void Buying()
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} is buying at store {1}", name, storeInterestedIn.STORE_ID);
        }

        Store store = Mall.INSTANCE.GetStoreByID(storeInterestedIn.STORE_ID);
        Stock stock = store.StoreStock;
        List<int> productsClientIsBuying = resources.GetProductsInterestedIn(store);
        for (int i = 0; i < productsClientIsBuying.Count; ++i)
        {
            int productID = productsClientIsBuying[i];
            StockData productStock = stock.GetStockOfProduct(productID);
            int price = productStock.Price;
            int amountInStock = productStock.CurrentStock;
            int amount = resources.HowManyCanAfford(productID, price, amountInStock);

            resources.Buy(productID, amount, price);
            store.Sell(productID, amount);

            if (debug)
            {
                Debug.LogFormat("Client {0} buys {1} of {2}", name, amount, productID);
            }
        }

        ChangeState(ClientState.Evaluating);
        IgnoreStoreTemporarily();
    }

    #endregion

    #region CheckingStock related

    private void CheckingStock()
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} is checking store {1}'s stock", name, storeInterestedIn.STORE_ID);
        }

        Store store = Mall.INSTANCE.GetStoreByID(storeInterestedIn.STORE_ID);
        knowledge.UpdateKnowledge(store);

        if (StoreHasAnyOfWantedProductsInStock())
        {
            ChangeState(ClientState.Buying);
        }
        else
        {
            IgnoreStoreTemporarily();
            ChangeState(ClientState.Evaluating);
        }
    }

    private bool StoreHasAnyOfWantedProductsInStock()
    {
        Store store = Mall.INSTANCE.GetStoreByID(storeInterestedIn.STORE_ID);
        Stock stock = store.StoreStock;
        List<int> productsIDs = resources.GetProductsInterestedIn(store);
        for (int i = 0; i < productsIDs.Count; ++i)
        {
            int productID = productsIDs[i];
            StockData productStock = stock.GetStockOfProduct(productID);
            if (productStock.CurrentStock != 0)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Evaluating related

    private void Evaluating()
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} is evaluating the situation", name);
        }

        if (!resources.ThereAreThingsLeftToBuy())
        {
            ChangeState(ClientState.Leaving);
            return;
        }

        List<int> productsIDs = GetIDsOfProductsFromKnownStores();
        if (productsIDs.Count == 0)
        {
            if (debug)
            {
                Debug.LogFormat("Client {0} knows no stores that sell the products they want", name);
            }

            ChangeState(ClientState.WanderingAround);
        }
        else
        {
            if (debug)
            {
                Debug.LogFormat("Client {0} knows a store that sells products they want", name);
            }

            StoreKnowledge closestStore = GetClosestStoreThatSellsWantedProducts(productsIDs);
            if (storesIgnored.ContainsKey(closestStore.STORE_ID))
            {
                if (debug)
                {
                    Debug.LogFormat("Client{0}: Store {1} is being ignored for the moment", name, closestStore.STORE_ID);
                }

                ChangeState(ClientState.WanderingAround);
            }
            else
            {
                storeInterestedIn = closestStore;
                ChangeState(ClientState.MovingToStore);
            }
        }
    }

    private List<int> GetIDsOfProductsFromKnownStores()
    {
        List<int> productsIDs = new List<int>();
        List<int> productsWanted = resources.GetProductsNotBoughtYet();

        for (int i = 0; i < productsWanted.Count; ++i)
        {
            int productID = productsWanted[i];
            if (knowledge.KnowsStoreThatSellsProduct(productID))
            {
                productsIDs.Add(productID);
            }
        }

        return productsIDs;
    }

    private StoreKnowledge GetClosestStoreThatSellsWantedProducts(List<int> productsIDs)
    {
        // TODO
        for (int i = 0; i < productsIDs.Count; ++i)
        {
            int productID = productsIDs[i];
        }

        List<StoreKnowledge> stores = knowledge.GetStoreThatSellsProduct(productsIDs[0]);
        return stores[0];
    }

    #endregion

    #region MovingToStore related

    private void MovingToStore()
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} is going to store {1}", name, storeInterestedIn.STORE_ID);
        }

        LocationData currentLocation = new LocationData(transform.position, currentFloor);
        LocationData storeLocation = storeInterestedIn.LOCATION;
        if (currentLocation.FLOOR != storeLocation.FLOOR)
        {
            // TODO: Create logic to move between floors
            Vector2 stairsPosition = new Vector2();
            IAction moveToStairs = new MoveAction(navigation, stairsPosition, MoveAction.Destination.Stairs);
            AddActionToQueue(moveToStairs);
        }

        IAction moveToStore = new MoveAction(navigation, storeLocation.POSITION, MoveAction.Destination.Store);
        AddActionToQueue(moveToStore);
        ExecuteActionQueue();
    }

    #endregion

    #region Leaving related

    private void Leaving()
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} is leaving the mall", name);
        }

        if (!knowledge.KnowsAnyExit())
        {
            Debug.LogWarningFormat("Client {0} knows no exists. This shouldn't happen", name);
            ChangeState(ClientState.WanderingAround);
            return;
        }

        ExitKnowledge closestExit = GetClosestExit();

        LocationData currentLocation = new LocationData(transform.position, currentFloor);
        LocationData exitLocation = closestExit.LOCATION;
        if (currentLocation.FLOOR != exitLocation.FLOOR)
        {
            Vector2 stairsPosition = new Vector2();   // TODO: Obtain stairs position
            IAction moveToStairs = new MoveAction(navigation, stairsPosition, MoveAction.Destination.Stairs);
            AddActionToQueue(moveToStairs);
        }

        IAction moveToExit = new MoveAction(navigation, exitLocation.POSITION, MoveAction.Destination.Exit);
        AddActionToQueue(moveToExit);
        ExecuteActionQueue();
    }

    private ExitKnowledge GetClosestExit()
    {
        LocationData currentLocation = new LocationData(transform.position, currentFloor);
        List<ExitKnowledge> knownExits = knowledge.GetKnownExits();

        // TODO
        return knownExits[0];
    }

    #endregion

    #region WanderingAround related

    private void WanderingAround()
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} is wandering", name);
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

    #region Agent Functions

    public override void OnActionCompleted(IAction action)
    {
        if (action is MoveAction)
        {
            MoveAction moveAction = action as MoveAction;
            switch (moveAction.GetDestination)
            {
                case MoveAction.Destination.Exit:
                    OnExitReached();
                    break;
                case MoveAction.Destination.NoDestination:
                    ChangeState(ClientState.Evaluating);
                    break;
                case MoveAction.Destination.Stairs:
                    OnStairsReached();
                    break;
                case MoveAction.Destination.Store:
                    OnStoreReached();
                    break;
                case MoveAction.Destination.Storage:
                default:
                    Debug.LogErrorFormat("Error on OnActionCompleted. A client cannot end their movement with destination {0}", moveAction.GetDestination);
                    break;
            }
        }

        base.OnActionCompleted(action);
    }

    private void OnExitReached()
    {
        if (debug)
        {
            Debug.LogWarningFormat("Client {0} has left the mall", name);
        }

        gameObject.SetActive(false);
    }

    private void OnStairsReached()
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} has reached the stairs", name);
        }
    }

    private void OnStoreReached()
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} has reached the store {1}", name, storeInterestedIn.STORE_ID);
        }

        ChangeState(ClientState.CheckingStock);
    }

    public override void OnActionQueueCompleted(IAction lastAction)
    {
        base.OnActionQueueCompleted(lastAction);
    }

    public override void OnStoreSeen(Store store)
    {
        if (knowledge.KnowsStore(store.ID))
        {
            knowledge.UpdateKnowledge(store);
            return;
        }

        knowledge.CreateKnowledge(store);
        if (currentState == ClientState.WanderingAround)
        {
            List<int> products = resources.GetProductsInterestedIn(store);
            if (products.Count != 0)
            {
                storeInterestedIn = knowledge.GetKnowledge(store.ID);
                StopExecutingActionQueue();
                ChangeState(ClientState.MovingToStore);
            }
        }
    }

    public override void OnOtherAgentSeen(Agent agent)
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} has seen the agent {1}", name, agent.name);
        }

        if (currentState == ClientState.WanderingAround && agent is Employee)
        {
            Employee employee = agent as Employee;
            if (employee.CanBeInterrupted())
            {
                employeeFound = employee;
                employee.Interrupt();
                ChangeState(ClientState.AskingForInformation);
            }
        }
    }

    #endregion
}
