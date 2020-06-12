using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Vision)), RequireComponent(typeof(Navigation))]
public abstract class Agent : MonoBehaviour
{
    public bool debug = true;

    protected Vision vision;
    protected Navigation navigation;
    protected int currentFloor;

    private Queue<IAction> actions;
    private IAction currentAction;
    private bool executingQueue;
    private bool consumedState;

    protected virtual void Start()
    {
        vision = GetComponent<Vision>();
        navigation = GetComponent<Navigation>();

        actions = new Queue<IAction>();
        consumedState = false;
    }

    protected virtual void Update()
    {
        if (!consumedState)
        {
            consumedState = true;
            PerformCurrentState();
        }
    }

    #region State machine related functions

    protected abstract void PerformCurrentState();

    protected void OnStateChanged()
    {
        consumedState = false;
    }

    #endregion

    #region Action Queue related functions

    public bool ExecutingActionQueue
    {
        get => executingQueue;
    }

    public void AddActionToQueue(IAction action)
    {
        actions.Enqueue(action);
    }

    public void AddActionToHeadOfQueue(IAction action)
    {
        Queue<IAction> newQueue = new Queue<IAction>();
        newQueue.Enqueue(action);

        while (actions.Count != 0)
        {
            newQueue.Enqueue(actions.Dequeue());
        }

        actions = newQueue;
    }

    public void ExecuteActionQueue()
    {
        currentAction = actions.Dequeue();
        currentAction.Execute();

        executingQueue = true;
    }

    public void PauseActionQueue()
    {
        currentAction.Cancel();
        AddActionToHeadOfQueue(currentAction);

        executingQueue = false;
    }

    public void StopExecutingActionQueue()
    {
        currentAction.Cancel();
        actions.Clear();
        executingQueue = false;
    }

    public virtual void OnActionCompleted(IAction action)
    {
        if (actions.Count == 0)
        {
            OnActionQueueCompleted(currentAction);
        }
        else
        {
            currentAction = actions.Dequeue();
            currentAction.Execute();
        }
    }

    public virtual void OnActionQueueCompleted(IAction lastAction)
    {
        executingQueue = false;
    }

    #endregion

    #region Stimulus related functions

    public abstract void OnStoreSeen(Store store);

    public abstract void OnOtherAgentSeen(Agent agent);

    protected void MoveTo(LocationData location, MoveAction.Destination destination)
    {
        LocationData currentLocation = new LocationData(transform.position, currentFloor);
        if (currentLocation.FLOOR != location.FLOOR)
        {
            // TODO: Use knowledge to get stairs position
            LocationData stairsLocation = Mall.INSTANCE.GetClosestStairs(currentLocation);
            LocationData spawnLocation = Mall.INSTANCE.GetStairsOnFloor(location.FLOOR);

            IAction moveToStairs = new MoveToStairsAction(navigation, stairsLocation, spawnLocation);
            AddActionToQueue(moveToStairs);
        }

        IAction moveTo = new MoveAction(navigation, location, destination);
        AddActionToQueue(moveTo);
        ExecuteActionQueue();
    }

    public void UponReachingDestination()
    {
        if (currentAction is MoveAction)
        {
            OnActionCompleted(currentAction);
        }
        else
        {
            Debug.LogErrorFormat("Error in agent {0}", name);
        }
    }

    #endregion
}
