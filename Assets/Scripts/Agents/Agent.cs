﻿using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    public bool debug = true;
    public int initialFloor;
    public float maxTimeSpentPerFloor = 10f;

    protected Vision vision;
    protected Navigation navigation;
    protected int currentFloor;
    protected float timeSpentOnThisFloor;
    protected float totalTime;
    protected bool canInteractWith;

    private Queue<IAction> actions;
    private IAction currentAction;
    private bool executingQueue;
    private bool consumedState;

    protected virtual void Start()
    {
        vision = GetComponent<Vision>();
        navigation = GetComponent<Navigation>();

        currentFloor = initialFloor;
        actions = new Queue<IAction>();
        consumedState = false;
        timeSpentOnThisFloor = 0f;
        totalTime = 0f;
        canInteractWith = true;
    }

    protected virtual void Update()
    {
        if (!consumedState)
        {
            consumedState = true;
            PerformCurrentState();
        }

        timeSpentOnThisFloor += Time.deltaTime;
        totalTime += Time.deltaTime;
    }

    public virtual void Reset(LocationData location)
    {
        transform.position = location.POSITION;
        currentFloor = location.FLOOR;
        initialFloor = currentFloor;
        canInteractWith = true;
        consumedState = false;

        actions = new Queue<IAction>();
        executingQueue = false;
    }

    #region State Machine Related

    protected abstract void PerformCurrentState();

    protected void OnStateChanged()
    {
        consumedState = false;
    }

    #endregion

    #region Action Queue Related

    public bool ThereAreActionsLeft()
    {
        return actions.Count != 0;
    }

    public IAction PeekActionQueue()
    {
        return actions.Peek();
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

    public void StopExecutingActionQueue(bool cancelCurrentAction)
    {
        if (!executingQueue)
        {
            return;
        }

        currentAction.Cancel();
        actions.Clear();
        executingQueue = !cancelCurrentAction;

        if (!cancelCurrentAction)
        {
            AddActionToQueue(currentAction);
        }
    }

    public void StopExecutingActionQueue()
    {
        StopExecutingActionQueue(true);
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

    public bool ExecutingActionQueue
    {
        get => executingQueue;
    }

    public IAction CurrentAction
    {
        get => currentAction;
    }

    #endregion

    #region Stimulus Related

    public abstract void OnStoreSeen(Store store);

    public abstract void OnOtherAgentSeen(Agent agent);

    protected void MoveTo(LocationData location, MoveAction.Destination destination)
    {
        MoveAction moveTo = new MoveAction(navigation, location, destination);
        MoveTo(location, moveTo);
    }

    protected void MoveToStore(LocationData location, int storeID)
    {
        MoveToStoreAction moveToStore = new MoveToStoreAction(navigation, location, storeID);
        MoveTo(location, moveToStore);
    }

    private void MoveTo(LocationData location, MoveAction moveAction)
    {
        LocationData currentLocation = new LocationData(transform.position, currentFloor);
        if (currentLocation.FLOOR != location.FLOOR)
        {
            Stairs.Direction direction = (currentLocation.FLOOR < location.FLOOR) ? Stairs.Direction.Up : Stairs.Direction.Down;
            int floorDifference = Mathf.Abs(currentLocation.FLOOR - location.FLOOR);

            LocationData auxLocation = new LocationData(currentLocation.POSITION, currentLocation.FLOOR);
            for (int i = 0; i < floorDifference; ++i)
            {
                Stairs closestStairs = Mall.INSTANCE.GetClosestStairs(auxLocation, direction);
                LocationData stairsLocation = closestStairs.StartingLocation;
                LocationData stairsEndLocation = closestStairs.EndingLocation;

                IAction moveToStairs = new MoveAction(navigation, stairsLocation, MoveAction.Destination.Stairs);
                AddActionToQueue(moveToStairs);

                IAction goUpStairs = new MoveAction(navigation, stairsEndLocation, MoveAction.Destination.StairsEnd);
                AddActionToQueue(goUpStairs);

                auxLocation = stairsEndLocation;
            }
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

        return random < chance;
    }

    #endregion

    public abstract List<Sprite> GetSpritesToDisplay();

    #region Properties

    public LocationData Location
    {
        get => new LocationData(transform.position, currentFloor);
    }

    public bool CanInteractWith
    {
        get => canInteractWith;
        set => canInteractWith = value;
    }

    #endregion
}
