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
        InStore,
        Buying,
        AskingEmployee,
        WanderingAround,
        FillingComplaint,
        Destroy,
        Error
    };

    public ClientState currentState = ClientState.Init;
    public ClientResources resources;
    public Navigation navigation;

    void Start()
    {
        Init();

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
                break;
            case ClientState.FillingComplaint:
                break;
            case ClientState.InStore:
                break;
            case ClientState.MovingToOtherDestination:
                break;
            case ClientState.MovingToStore:
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

    }

    public void DeInit()
    {

    }

}
