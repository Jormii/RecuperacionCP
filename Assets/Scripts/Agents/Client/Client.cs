using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : Human
{
    public enum ClientState
    {
        Evaluating,
        Moving,
        Buying,
        WanderingAround,
        Error
    };

    public const float IGNORE_STORE_TIME = 1f;

    [SerializeField] private ClientState currentState = ClientState.Evaluating;
    private ClientKnowledge knowledge;
    private ClientResources resources;
    private Dictionary<int, float> storesIgnored;

    protected override void Start()
    {
        base.Start();

        currentState = ClientState.Evaluating;
        knowledge = new ClientKnowledge();
        resources = GetComponent<ClientResources>();
        storesIgnored = new Dictionary<int, float>();
    }

    void Update()
    {
        UpdateIgnoredStores();
        PerformCurrentState();
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

    private void PerformCurrentState()
    {
        switch (currentState)
        {
            case ClientState.Buying:
                // TODO:
                break;
            case ClientState.Evaluating:
                Evaluate();
                break;
            case ClientState.Moving:
                Moving();
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

    private void Evaluate()
    {
        if (!resources.ThereAreThingsLeftToBuy())
        {
            LeaveMall();
            return;
        }

        List<int> products = resources.GetProductsNotBoughtYet();

        bool knowsStore = false;
        int i = 0;
        while (!knowsStore && i < products.Count)
        {
            int productID = products[i];
            if (knowledge.KnowsStoreThatSellsProduct(productID))
            {
                StoreKnowledge storeKnowledge = knowledge.GetStoreThatSellsProduct(productID);
                if (!storesIgnored.ContainsKey(storeKnowledge.STORE_ID))
                {
                    knowsStore = true;
                    continue;
                }
            }

            i += 1;
        }

        if (knowsStore)
        {
            int productID = products[i];
            StoreKnowledge storeKnowledge = knowledge.GetStoreThatSellsProduct(productID);

            GoToStore(storeKnowledge);
        }
        else
        {
            WanderAround();
        }

    }

    private void Moving()
    {
        // TODO
    }

    private void WanderingAround()
    {
        // TODO
    }

    #endregion

    private void GoToStore(StoreKnowledge storeKnowledge)
    {
        Debug.LogFormat("Client {0} is going to store {1}", name, storeKnowledge.LOCATION.POSITION);

        if (storeKnowledge.LOCATION.FLOOR != currentFloor)
        {
            // TODO: Create logic to move between floors
            Vector2 stairsPosition = new Vector2();
            IAction moveToStairs = new MoveAction(navigation, stairsPosition, MoveAction.Destination.Stairs);
            AddActionToQueue(moveToStairs);
        }

        IAction moveToStore = new MoveToStoreAction(navigation, storeKnowledge);
        AddActionToQueue(moveToStore);

        currentState = ClientState.Moving;
        ExecuteActionQueue();
    }

    private void BuyAtStore(Store store)
    {
        Debug.LogFormat("Client {0} is buying at store {1}", name, store);

        currentState = ClientState.Buying;

        Stock stock = store.StoreStock;
        List<int> productsClientIsBuying = resources.GetProductsInterestedIn(store);
        for (int i = 0; i < productsClientIsBuying.Count; ++i)
        {
            int productID = productsClientIsBuying[i];
            StockData productStock = stock.GetStockOfProduct(productID);
            int price = productStock.Price;
            int amountInStock = productStock.CurrentStock;
            int amount = resources.HowManyCanAfford(productID, price, amountInStock);
            if (amount == 0)
            {
                continue;
            }

            resources.Buy(productID, amount, price);
            store.Sell(productID, amount);

            Debug.LogFormat("Client {0} buys {1} of {2}", name, amount, productID);
        }

        storesIgnored.Add(store.ID, IGNORE_STORE_TIME);
    }

    private void LeaveMall()
    {
        Debug.LogFormat("Client {0} is leaving the mall", name);

        if (!knowledge.KnowsAnyExit())
        {
            // THIS SHOULDN'T HAPPEN
            Debug.LogWarningFormat("Client {0} knows no exists. This shouldn't happen", name);
            WanderAround();
            return;
        }

        Vector2 clientPosition = transform.position;
        ExitKnowledge closestExit = knowledge.GetClosestExit(clientPosition, currentFloor);
        if (closestExit.LOCATION.FLOOR != currentFloor)
        {
            Vector2 stairsPosition = new Vector2();   // TODO: Obtain stairs position
            IAction moveToStairs = new MoveAction(navigation, stairsPosition, MoveAction.Destination.Stairs);
            AddActionToQueue(moveToStairs);
        }

        Vector2 exitPosition = closestExit.LOCATION.POSITION;
        IAction moveToExit = new MoveAction(navigation, exitPosition, MoveAction.Destination.Exit);
        AddActionToQueue(moveToExit);

        currentState = ClientState.Moving;
        ExecuteActionQueue();
    }

    private void WanderAround()
    {
        Debug.LogFormat("Client {0} is wandering", name);
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

        currentState = ClientState.WanderingAround;
        ExecuteActionQueue();
    }

    #region Human Functions

    public override void OnActionCompleted(IAction action)
    {
        base.OnActionCompleted(action);
    }

    public override void OnActionQueueCompleted(IAction lastAction)
    {
        base.OnActionQueueCompleted(lastAction);

        if (lastAction is MoveToStoreAction)
        {
            MoveToStoreAction moveToStoreAction = lastAction as MoveToStoreAction;
            Store store = Mall.INSTANCE.GetStoreByID(moveToStoreAction.Knowledge.STORE_ID);
            BuyAtStore(store);
            currentState = ClientState.Evaluating;  // TODO: This should be driven by animations
        }
        else if (lastAction is MoveAction)
        {
            MoveAction moveAction = lastAction as MoveAction;
            // TODO
            switch (moveAction.GetDestination)
            {
                case MoveAction.Destination.Stairs:
                    break;
                case MoveAction.Destination.Exit:
                    break;
                case MoveAction.Destination.NoDestination:
                    break;
            }
        }

    }

    public override void OnStoreSeen(Store store)
    {
        if (!knowledge.KnowsStore(store))
        {
            knowledge.CreateKnowledge(store);

            if (currentState == ClientState.WanderingAround)
            {
                List<int> products = resources.GetProductsInterestedIn(store);
                if (products.Count != 0)
                {
                    GoToStore(knowledge.GetKnowledge(store));
                }
            }
        }
        else
        {
            knowledge.UpdateKnowledge(store);
        }
    }

    #endregion

}
