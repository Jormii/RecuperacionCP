using System.Collections.Generic;
using UnityEngine;

public class Client : Agent
{
    public enum ClientState
    {
        AskingForInformation,
        Buying,
        CheckingStock,
        Evaluating,
        Leaving,
        MovingToStore,
        MovingTowardsEmployee,
        WanderingAround,
        Error
    };

    public const float IGNORE_STORE_TIME = 5f;

    [SerializeField] private ClientState currentState = ClientState.Evaluating;
    private ClientKnowledge knowledge;
    private ClientResources resources;
    private Dictionary<int, float> storesIgnored;
    private HashSet<int> employeesAsked;

    private StoreKnowledge storeInterestedIn;
    private Employee employeeFound;
    private Dictionary<int, float> timeSpentPerFloor;

    protected override void Start()
    {
        base.Start();

        currentState = ClientState.Evaluating;
        knowledge = new ClientKnowledge();
        resources = GetComponent<ClientResources>();
        storesIgnored = new Dictionary<int, float>();
        employeesAsked = new HashSet<int>();
        timeSpentPerFloor = new Dictionary<int, float>();
    }

    protected override void Update()
    {
        base.Update();

        UpdateIgnoredStores();
    }

    #region Ignored Stores Related

    private void IgnoreStoreTemporarily()
    {
        if (storesIgnored.ContainsKey(storeInterestedIn.STORE_ID))
        {
            return;
        }

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

    #endregion

    #region States Functions

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
            case ClientState.MovingTowardsEmployee:
                MovingTowardsEmployee();
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

    #region AskingForInformation Related

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
                knowledge.CreateStoreKnowledge(givenKnowledge);
            }
        }

        ChangeState(ClientState.Evaluating);
        employeeFound.ContinueTasks();
    }

    #endregion

    #region Buying Related

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

    #region CheckingStock Related

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

    #region Evaluating Related

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
                Debug.LogFormat("Client {0} knows stores that sell products they want", name);
            }

            StoreKnowledge closestStore = GetClosestStoreThatSellsWantedProducts(productsIDs);
            if (closestStore.STORE_ID == -1)
            {
                if (debug)
                {
                    Debug.LogFormat("Client {0} is ignoring all stores they know", name);
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
        Vector2 currentPosition = transform.position;
        StoreKnowledge closestStore = new StoreKnowledge(-1, new LocationData());
        float distanceToClosestStore = Mathf.Infinity;
        for (int i = 0; i < productsIDs.Count; ++i)
        {
            int productID = productsIDs[i];
            List<StoreKnowledge> storesThatSellProduct = knowledge.GetStoresThatSellProduct(productID);
            for (int j = 0; j < storesThatSellProduct.Count; ++j)
            {
                StoreKnowledge storeKnown = storesThatSellProduct[j];
                if (storesIgnored.ContainsKey(storeKnown.STORE_ID))
                {
                    continue;
                }

                Vector2 storePosition = storeKnown.LOCATION.POSITION;
                float manhattanDistance = Utils.ManhattanDistance(currentPosition, storePosition);

                if (manhattanDistance < distanceToClosestStore)
                {
                    closestStore = storeKnown;
                    distanceToClosestStore = manhattanDistance;
                }
            }
        }

        return closestStore;
    }

    #endregion

    #region Leaving Related

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
        LocationData exitLocation = closestExit.LOCATION;
        MoveTo(exitLocation, MoveAction.Destination.Exit);
    }

    private ExitKnowledge GetClosestExit()
    {
        List<ExitKnowledge> knownExits = knowledge.GetKnownExits();

        Vector2 currentPosition = transform.position;
        ExitKnowledge closestExit = new ExitKnowledge();
        float distanceToClosestExit = Mathf.Infinity;
        for (int i = 0; i < knownExits.Count; ++i)
        {
            ExitKnowledge exitKnown = knownExits[i];
            Vector2 exitPosition = exitKnown.LOCATION.POSITION;
            float manhattanDistance = Utils.ManhattanDistance(currentPosition, exitPosition);

            if (manhattanDistance < distanceToClosestExit)
            {
                closestExit = exitKnown;
                distanceToClosestExit = manhattanDistance;
            }
        }

        return closestExit;
    }

    #endregion

    #region MovingToStore Related

    private void MovingToStore()
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} is going to store {1}", name, storeInterestedIn.STORE_ID);
        }

        LocationData storeLocation = storeInterestedIn.LOCATION;
        MoveToStore(storeLocation, storeInterestedIn.STORE_ID);
    }

    #endregion

    #region MovingTowardsEmployee Related

    private void MovingTowardsEmployee()
    {
        // In case something interrupted them
        if (!employeeFound.CanBeInterrupted())
        {
            StopExecutingActionQueue();
            ChangeState(ClientState.WanderingAround);
            return;
        }

        employeeFound.Interrupt();

        // TODO once sprites are done: Tweak to not end on top of the employee when asking
        Vector2 employeePosition = employeeFound.transform.position;
        Vector2 vector = new Vector2(employeePosition.x - transform.position.x, 0f).normalized;
        Vector2 destinationPosition = employeePosition - 1f * vector;
        LocationData destinationLocation = new LocationData(destinationPosition, currentFloor);

        MoveTo(destinationLocation, MoveAction.Destination.Agent);
    }

    #endregion

    #region WanderingAround Related

    private void WanderingAround()
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} is wandering", name);
        }

        Vector2 wanderDestination = CalculateWanderDestination();
        int newFloor = currentFloor;

        if (ShouldChangeFloors())
        {
            newFloor = CalculateNewFloor();
        }

        LocationData wanderLocation = new LocationData(wanderDestination, newFloor);
        MoveTo(wanderLocation, MoveAction.Destination.NoDestination);
    }

    private Vector2 CalculateWanderDestination()
    {
        // TODO: Improve
        Vector2 wanderDirection = new Vector2(
            (Random.Range(0f, 1f) > 0.5f) ? 1 : -1,
            0f
        );

        Vector2 wanderDestination = new Vector2(
            (wanderDirection.x < 0) ? Mall.MALL_LEFT_LIMIT : Mall.MALL_RIGHT_LIMIT,
            transform.position.y
        );

        return wanderDestination;
    }

    private int CalculateNewFloor()
    {
        List<int> allFloors = new List<int>();
        int minFloor = Mall.INSTANCE.LowestFloor;
        int maxFloor = Mall.INSTANCE.HighestFloor;
        for (int i = minFloor; i <= maxFloor; ++i)
        {
            if (i != currentFloor)
            {
                allFloors.Add(i);
            }
        }

        List<float> inverseTimeSpent = new List<float>();
        for (int i = 0; i < allFloors.Count; ++i)
        {
            int floor = allFloors[i];
            if (timeSpentPerFloor.ContainsKey(floor))
            {
                inverseTimeSpent.Add(totalTime - timeSpentPerFloor[floor]);
            }
            else
            {
                inverseTimeSpent.Add(totalTime);
            }
        }

        int randomIndex = Utils.RandomFromWeights(inverseTimeSpent);
        return allFloors[randomIndex];
    }

    #endregion

    #endregion

    #region Agent Functions

    #region OnActionCompleted Related

    public override void OnActionCompleted(IAction action)
    {
        if (action is MoveAction)
        {
            MoveAction moveAction = action as MoveAction;
            switch (moveAction.GetDestination)
            {
                case MoveAction.Destination.Agent:
                    OnAgentReached(moveAction);
                    break;
                case MoveAction.Destination.Exit:
                    OnExitReached(moveAction);
                    break;
                case MoveAction.Destination.NoDestination:
                    OnNoDestinationReached(moveAction);
                    break;
                case MoveAction.Destination.Stairs:
                    OnStairsReached(moveAction);
                    break;
                case MoveAction.Destination.StairsEnd:
                    OnStairsEndReached(moveAction);
                    break;
                case MoveAction.Destination.Store:
                    OnStoreReached(moveAction);
                    break;
                case MoveAction.Destination.Storage:
                default:
                    Debug.LogErrorFormat("Error on OnActionCompleted. A client cannot end their movement with destination {0}", moveAction.GetDestination);
                    break;
            }
        }

        base.OnActionCompleted(action);
    }

    private void OnAgentReached(MoveAction moveAction)
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} is next to another agent", name);
        }

        ChangeState(ClientState.AskingForInformation);
    }

    private void OnExitReached(MoveAction moveAction)
    {
        if (debug)
        {
            Debug.LogWarningFormat("Client {0} has left the mall", name);
        }

        gameObject.SetActive(false);
    }

    private void OnNoDestinationReached(MoveAction moveAction)
    {
        ChangeState(ClientState.Evaluating);
    }

    private void OnStairsReached(MoveAction moveAction)
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} has reached the stairs", name);
        }
    }

    private void OnStairsEndReached(MoveAction moveAction)
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} has arrived to a need floor", name);
        }

        if (timeSpentPerFloor.ContainsKey(currentFloor))
        {
            timeSpentPerFloor[currentFloor] += timeSpentOnThisFloor;
        }
        else
        {
            timeSpentPerFloor.Add(currentFloor, timeSpentOnThisFloor);
        }

        int newFloor = moveAction.Location.FLOOR;
        currentFloor = newFloor;
        timeSpentOnThisFloor = 0f;
    }

    private void OnStoreReached(MoveAction moveAction)
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} has reached the store {1}", name, storeInterestedIn.STORE_ID);
        }

        ChangeState(ClientState.CheckingStock);
    }

    #endregion

    #region Vision Related

    public override void OnStoreSeen(Store store)
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} has seen store {1}", name, store.name);
        }

        if (knowledge.KnowsStore(store.ID))
        {
            knowledge.UpdateKnowledge(store);
            return;
        }

        knowledge.CreateStoreKnowledge(store);
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
            if (employee.CanBeInterrupted() && !employeesAsked.Contains(employee.GetInstanceID()))
            {
                employeeFound = employee;
                employeesAsked.Add(employee.GetInstanceID());
                ChangeState(ClientState.MovingTowardsEmployee);
            }
        }
    }

    public override void OnExitSeen(Exit exit)
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} has seen an exit", name);
        }

        if (knowledge.KnowsExit(exit.ID))
        {
            return;
        }

        if (debug)
        {
            Debug.LogFormat("Client {0} knows a new exit", name);
        }

        knowledge.CreateExitKnowledge(exit);
    }

    #endregion

    #endregion
}
