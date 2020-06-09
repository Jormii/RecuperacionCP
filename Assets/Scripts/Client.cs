using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : Human
{
    public enum ClientState
    {
        Init,
        DeInit,
        Error,
        Evaluating,
        Moving,
        Buying,
        WanderingAround
    };

    [SerializeField] private ClientState currentState = ClientState.Init;
    private ClientKnowledge knowledge;
    private ClientResources resources;

    protected new void Start()
    {
        base.Start();

        Init();

        knowledge = new ClientKnowledge();
        resources = GetComponent<ClientResources>();
    }

    void Update()
    {
        PerformCurrentState();
    }

    #region States functions

    private void PerformCurrentState()
    {
        switch (currentState)
        {
            case ClientState.Buying:
                // TODO:
                break;
            case ClientState.DeInit:
                DeInit();
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
            default:
            case ClientState.Init:
            case ClientState.Error:
                Debug.LogErrorFormat("Something wrong happened in client {0}'s state machine. Destroying", name);
                DeInit();
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

        List<Product> products = resources.GetProductsNotBoughtYet();

        bool knowsStore = false;
        int i = 0;
        while (!knowsStore && i < products.Count)
        {
            Product p = products[i];
            if (knowledge.KnowsStoreThatSellsProduct(p))
            {
                knowsStore = true;
                continue;
            }

            i += 1;
        }

        if (knowsStore)
        {
            Product p = products[i];
            StoreKnowledge storeKnowledge = knowledge.GetStoreThatSellsProduct(p);

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
        Debug.LogFormat("Client {0} is going to store {1}", name, storeKnowledge.POSITION);

        if (storeKnowledge.FLOOR != currentFloor)
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

        List<Product> productsClientIsBuying = store.GetProductsWanted(resources);
        for (int i = 0; i < productsClientIsBuying.Count; ++i)
        {
            Product product = productsClientIsBuying[i];
            int price = store.GetPriceOfProduct(product);
            int productStock = store.GetStockOfProduct(product);
            int amount = resources.HowManyCanAfford(product, price, productStock);
            if (amount == 0)
            {
                continue;
            }

            resources.Buy(product, amount, price);
            store.Sell(product, amount);

            Debug.LogFormat("Client {0} buys {1} of {2}", name, amount, product.name);
        }
    }

    private void LeaveMall()
    {
        Debug.LogFormat("Client {0} is leaving the mall", name);

        // TODO: Use knowledge
        /*
        if (!knowledge.KnowsAnyExit())
        {
            // THIS SHOULDN'T HAPPEN
            Debug.LogWarningFormat("Client {0} knows no exists. This shouldn't happen", name);
            WanderAround();
            return;
        }
        */

        Vector2 clientPosition = transform.position;
        // ExitKnowledge closestExit = knowledge.GetClosestExit(clientPosition, currentFloor);
        ExitKnowledge closestExit = new ExitKnowledge(0, 0, Mall.INSTANCE.exit.transform.position); // TODO: Use knowledge
        if (closestExit.FLOOR != currentFloor)
        {
            Vector2 stairsPosition = new Vector2();   // TODO: Obtain stairs position
            IAction moveToStairs = new MoveAction(navigation, stairsPosition, MoveAction.Destination.Stairs);
            AddActionToQueue(moveToStairs);
        }

        Vector2 exitPosition = closestExit.POSITION;
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

    public override void Init()
    {
        currentState = ClientState.Evaluating;
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
            switch (moveAction.GetDestination)
            {
                case MoveAction.Destination.Stairs:
                    // TODO
                    break;
                case MoveAction.Destination.Exit:
                    currentState = ClientState.DeInit;
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
                List<Product> products = store.GetProductsWanted(resources);
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

    public override void UponReachingDestination()
    {
        switch (currentState)
        {
            case ClientState.Moving:
                // TODO
                break;
            case ClientState.WanderingAround:
                currentState = ClientState.Evaluating;
                break;
            default:
                Debug.LogWarningFormat("UponReachingDestination called in unvalid state {0}", currentState);
                break;
        }

        base.UponReachingDestination();
    }

    #endregion

}
