using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Human : MonoBehaviour, IStateMachine
{
    protected Vision vision;
    protected Navigation navigation;
    protected int currentFloor;

    private Queue<IAction> actions;
    private IAction currentAction;
    private bool executingQueue;

    protected void Start()
    {
        vision = GetComponent<Vision>();
        navigation = GetComponent<Navigation>();

        actions = new Queue<IAction>();
    }

    #region Action Queue related functions

    public bool ExecutingActionQueue
    {
        get => executingQueue;
    }

    public void AddActionToQueue(IAction action)
    {
        actions.Enqueue(action);
    }

    public void ExecuteActionQueue()
    {
        currentAction = actions.Dequeue();
        currentAction.Execute();

        executingQueue = true;
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

    public virtual void UponReachingDestination()
    {
        if (currentAction is MoveAction)
        {
            OnActionCompleted(currentAction);
        }
    }

    #endregion

    #region IStateMachine functions

    public abstract void Init();
    public abstract void DeInit();

    #endregion

}
