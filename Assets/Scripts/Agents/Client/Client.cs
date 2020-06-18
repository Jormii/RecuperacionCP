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

    private Animator animator;
    private StoreKnowledge storeInterestedIn;
    private Employee employeeFound;
    private Dictionary<int, float> timeSpentPerFloor;
    private bool hasToLeave = false;

    protected override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
        currentState = ClientState.Evaluating;
        knowledge = new ClientKnowledge();
        resources = new ClientResources();
        storesIgnored = new Dictionary<int, float>();
        employeesAsked = new HashSet<int>();
        timeSpentPerFloor = new Dictionary<int, float>();
    }

    protected override void Update()
    {
        base.Update();

        UpdateIgnoredStores();
    }

    public override void Reset(LocationData location)
    {
        base.Reset(location);

        currentState = ClientState.WanderingAround;
        storesIgnored?.Clear();
        employeesAsked?.Clear();
        timeSpentPerFloor?.Clear();
    }

    public void MakeLeave()
    {
        hasToLeave = true;
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
        CancelInvoke();

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

        employeesAsked.Add(employeeFound.GetInstanceID());
        Invoke("WaitBeforeContinuing", 1.5f);
    }

    private void WaitBeforeContinuing()
    {
        ChangeState(ClientState.Evaluating);
        employeeFound.ContinueTasks();
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

        IgnoreStoreTemporarily();
        Invoke("LeaveStore", 1f);
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

            Invoke("LeaveStore", 1f);
        }
    }

    private void LeaveStore()
    {
        animator.SetBool("enteringStore", false);
        animator.SetBool("leavingStore", true);
        MakeInteractable(true);
        ChangeState(ClientState.Evaluating);
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

        if (!resources.ThereAreThingsLeftToBuy() || hasToLeave)
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

            StoreKnowledge cheapestStore = GetCheapestStoreThatSellsWantedProducts(productsIDs);
            if (cheapestStore.STORE_ID == -1)
            {
                if (debug)
                {
                    Debug.LogFormat("Client {0} is ignoring all stores they know", name);
                }

                ChangeState(ClientState.WanderingAround);
            }
            else
            {
                storeInterestedIn = cheapestStore;
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

    private StoreKnowledge GetCheapestStoreThatSellsWantedProducts(List<int> productsIDs)
    {
        StoreKnowledge cheapestStore = new StoreKnowledge(-1, new LocationData());
        int cheapestPrice = int.MaxValue;
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

                int knownPrice = storeKnown.GetPriceOfProduct(productID);
                if (knownPrice < cheapestPrice)
                {
                    cheapestStore = storeKnown;
                    cheapestPrice = knownPrice;
                }
            }
        }

        return cheapestStore;
    }

    #endregion

    #region Leaving Related

    private void Leaving()
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} is leaving the mall", name);
        }

        LocationData closestExit = Mall.INSTANCE.GetClosestExit(Location);
        MoveTo(closestExit, MoveAction.Destination.Exit);
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

        employeeFound.Interrupt(this);

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
        float mallTotalDistance = Mall.MALL_RIGHT_LIMIT - Mall.MALL_LEFT_LIMIT;
        float xPercentage = transform.position.x / mallTotalDistance;

        int xDirection = (xPercentage < Random.Range(0f, 1f)) ? 1 : -1;

        float distanceToTravel = Random.Range(0.125f, 0.5f) * mallTotalDistance;

        Vector2 wanderDestination = new Vector2(
            Mathf.Clamp(transform.position.x + distanceToTravel * xDirection, Mall.MALL_LEFT_LIMIT, Mall.MALL_RIGHT_LIMIT),
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

        animator.SetBool("enteringStore", true);
        animator.SetBool("leavingStore", false);
        ClientsManager.INSTANCE.ClientLeavesMall(this);
    }

    private void OnNoDestinationReached(MoveAction moveAction)
    {
        Invoke("WaitBeforeProceeding", Random.Range(1f, 1.5f));
    }

    private void WaitBeforeProceeding()
    {
        ChangeState(ClientState.Evaluating);
    }

    private void OnStairsReached(MoveAction moveAction)
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} has reached the stairs", name);
        }

        MakeInteractable(false);
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

        MakeInteractable(true);
    }

    private void OnStoreReached(MoveAction moveAction)
    {
        if (debug)
        {
            Debug.LogFormat("Client {0} has reached the store {1}", name, storeInterestedIn.STORE_ID);
        }

        Store store = Mall.INSTANCE.GetStoreByID(storeInterestedIn.STORE_ID);
        if (store.IsOpen)
        {
            ChangeState(ClientState.CheckingStock);

            animator.SetBool("enteringStore", true);
            animator.SetBool("leavingStore", false);
            MakeInteractable(false);
        }
        else
        {
            ChangeState(ClientState.Evaluating);
        }
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
                ChangeState(ClientState.MovingTowardsEmployee);
            }
        }
    }

    #endregion

    #endregion

    public override List<Sprite> GetSpritesToDisplay()
    {
        List<Sprite> sprites = new List<Sprite>();

        switch (currentState)
        {
            case ClientState.AskingForInformation:
            case ClientState.MovingTowardsEmployee:
                sprites.Add(SpriteManager.INSTANCE.GetAskingEmployeeSprite());
                break;
            case ClientState.Buying:
            case ClientState.MovingToStore:
                sprites.Add(SpriteManager.INSTANCE.GetStoreSprite(storeInterestedIn.STORE_ID));
                break;
            case ClientState.Leaving:
                sprites.Add(SpriteManager.INSTANCE.GetLeaveSprite());
                break;
            case ClientState.WanderingAround:
                sprites.Add(SpriteManager.INSTANCE.GetQuestionMarkSprite());
                break;
            default:
                break;
        }

        return sprites;
    }
}
