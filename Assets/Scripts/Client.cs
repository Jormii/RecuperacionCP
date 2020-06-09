using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : Human
{
    public enum ClientState
    {
        Init,
        Evaluating,
        MovingToStore,
        MovingToOtherDestination,
        InFrontOfStore,
        Buying,
        AskingEmployee,
        WanderingAround,
        FillingComplaint,
        Destroy,
        Error
    };

    public ClientState currentState = ClientState.Init;
    public ClientKnowledge knowledge;
    public ClientResources resources;
    public Navigation navigation;
    public Vision vision;
    public int currentFloor;

    // TODO: Improve these
    private Product productDestination;
    private StoreKnowledge storeKnowledgeDestination;

    void Start()
    {
        Init();

        knowledge = new ClientKnowledge();
        resources = GetComponent<ClientResources>();
        navigation = GetComponent<Navigation>();
        vision = GetComponent<Vision>();
    }

    void Update()
    {
        PerformCurrentState();
    }

    private void PerformCurrentState()
    {
        switch (currentState)
        {
            case ClientState.AskingEmployee:
                break;
            case ClientState.Buying:
                break;
            case ClientState.Destroy:
                DeInit();
                break;
            case ClientState.Evaluating:
                Evaluate();
                break;
            case ClientState.FillingComplaint:
                break;
            case ClientState.InFrontOfStore:
                break;
            case ClientState.MovingToOtherDestination:
                if (navigation.hasReachedItsDestination)
                {
                    currentState = ClientState.Destroy;
                }
                break;
            case ClientState.MovingToStore:
                if (navigation.hasReachedItsDestination)
                {
                    UponGettingToStore();
                }
                break;
            case ClientState.WanderingAround:
                if (navigation.hasReachedItsDestination)
                {
                    currentState = ClientState.Evaluating;
                }
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

    public override void Init()
    {
        currentState = ClientState.Evaluating;
    }

    public override void DeInit()
    {
    }

    private void Evaluate()
    {
        if (resources.ThereAreThingsLeftToBuy())
        {
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

                navigation.MoveTo(storeKnowledge.POSITION);
                currentState = ClientState.MovingToStore;
                productDestination = p;
                storeKnowledgeDestination = storeKnowledge;
            }
            else
            {
                Vector2 position;
                // Was moving right
                if (transform.position.x > 0)
                {
                    position = new Vector2(Mall.MIN_X, transform.position.y);
                }
                else
                {
                    position = new Vector2(Mall.MAX_X, transform.position.y);
                }

                navigation.MoveTo(position);
                currentState = ClientState.WanderingAround;
            }
        }
        else
        {
            // TODO: Recurrir al conocimiento
            Debug.Log("Nothing else to buy. Leaving");
            GameObject exit = Mall.INSTANCE.exit;
            navigation.MoveTo(exit.transform.position);

            currentState = ClientState.MovingToOtherDestination;
        }
    }

    private void UponGettingToStore()
    {
        // TODO: Check if store still exists or sells wanted items
        currentState = ClientState.InFrontOfStore;

        Store store = Mall.INSTANCE.GetStoreByID(storeKnowledgeDestination.STORE_ID);
        Stock storeStock = store.stock;

        int price = storeStock.productsPrice[productDestination];
        int stock = storeStock.productsStock[productDestination];
        int amountAbleToBuy = resources.HowManyCanAfford(productDestination, price, stock);
        if (amountAbleToBuy != 0)
        {
            resources.Buy(productDestination, price, stock);
            store.Sell(productDestination, amountAbleToBuy);

            currentState = ClientState.Evaluating;
        }
        else
        {
            // TODO: Look for another store
        }
    }

    public override void OnStoreSeen(Store store)
    {
        if (!knowledge.KnowsStore(store))
        {
            knowledge.CreateKnowledge(store);
        }
        else
        {
            knowledge.UpdateKnowledge(store);
        }
    }

}
