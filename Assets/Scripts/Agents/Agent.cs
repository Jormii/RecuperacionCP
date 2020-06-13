using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Vision)), RequireComponent(typeof(Navigation))]
public abstract class Agent : MonoBehaviour
{
    public bool debug = true;
    public int initialFloor;
    public float maxTimeSpentPerFloor = 10f;

    protected Vision vision;
    protected Navigation navigation;
    protected int currentFloor;

    private Queue<IAction> actions;
    private IAction currentAction;
    private bool executingQueue;
    private bool consumedState;
    private float timeSpentOnThisFloor;

    protected virtual void Start()
    {
        vision = GetComponent<Vision>();
        navigation = GetComponent<Navigation>();

        currentFloor = initialFloor;
        actions = new Queue<IAction>();
        consumedState = false;
        timeSpentOnThisFloor = 0f;
    }

    protected virtual void Update()
    {
        if (!consumedState)
        {
            consumedState = true;
            PerformCurrentState();
        }

        timeSpentOnThisFloor += Time.deltaTime;
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

    public bool ThereAreActionsLeft()
    {
        return actions.Count != 0;
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
        if (!executingQueue)
        {
            return;
        }

        currentAction.Cancel();
        AddActionToHeadOfQueue(currentAction);

        executingQueue = false;
    }

    public void StopExecutingActionQueue()
    {
        if (!executingQueue)
        {
            return;
        }

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

    public abstract void OnStairsSeen(Stairs stairs);

    public abstract void OnExitSeen(Exit exit);

    protected void MoveTo(LocationData location, MoveAction.Destination destination)
    {
        MoveAction moveTo = new MoveAction(navigation, location, destination);
        MoveTo(location, moveTo);
    }

    public void MoveToStore(LocationData location, int storeID)
    {
        MoveToStoreAction moveToStore = new MoveToStoreAction(navigation, location, storeID);
        MoveTo(location, moveToStore);
    }

    private void MoveTo(LocationData location, MoveAction moveAction)
    {
        LocationData currentLocation = new LocationData(transform.position, currentFloor);
        if (currentLocation.FLOOR != location.FLOOR)
        {
            // TODO: Use knowledge to get stairs position
            // TODO: Consider moving between multiple floors
            Stairs closestStairs = Mall.INSTANCE.GetClosestStairs(currentLocation);
            LocationData stairsLocation = closestStairs.StartingLocation;
            LocationData stairsEndLocation = closestStairs.EndingLocation;

            IAction moveToStairs = new MoveAction(navigation, stairsLocation, MoveAction.Destination.Stairs);
            AddActionToQueue(moveToStairs);

            IAction goUpStairs = new MoveAction(navigation, stairsEndLocation, MoveAction.Destination.StairsEnd);
            AddActionToQueue(goUpStairs);

            timeSpentOnThisFloor = 0f;
        }

        AddActionToQueue(moveAction);
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

    protected bool ShouldChangeFloors()
    {
        float ratioTimeSpent = timeSpentOnThisFloor / maxTimeSpentPerFloor;
        float chance = 1f - 1f / (0.15f * ratioTimeSpent + 1f);
        float random = Random.Range(0f, 1f);

        return random <= chance;
    }

    #endregion
}
