using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientStateMachine : MonoBehaviour, IStateMachine
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
    public int currentFloor;

    private Store store;
    private Product product;

    void Start()
    {
        Init();

        knowledge = new ClientKnowledge();
        resources = GetComponent<ClientResources>();
        navigation = GetComponent<Navigation>();
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

    public void Init()
    {
        currentState = ClientState.Evaluating;
    }

    public void DeInit()
    {
    }

    private void Evaluate()
    {
        if (resources.ThereAreThingsLeftToBuy())
        {
            List<Product> products = resources.GetProductsNotBoughtYet();

            int randomIndex = Random.Range(0, products.Count - 1);
            Product randomProduct = products[randomIndex];
            StoreData knownStore = knowledge.GetStoreThatSellsProduct(randomProduct);
            if (knownStore == null)
            {
                currentState = ClientState.WanderingAround;
            }
            else
            {
                // TODO: Consider different floors
                navigation.MoveTo(knownStore.position);

                currentState = ClientState.MovingToStore;
                product = randomProduct;
            }
        }
        else
        {
            // TODO
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

        // TODO: Obtain the store somehow
        Store store = GameObject.FindGameObjectWithTag("Store").GetComponent<Store>();
        Stock storeStock = store.stock;

        int price = storeStock.productsPrice[product];
        int stock = 10;
        int amountAbleToBuy = resources.HowManyCanAfford(product, price, stock);
        if (amountAbleToBuy != 0)
        {
            resources.Buy(product, price, stock);
            store.Sell(product, amountAbleToBuy);

            currentState = ClientState.Evaluating;
        }
        else
        {
            // TODO: Look for another store
        }
    }

}
