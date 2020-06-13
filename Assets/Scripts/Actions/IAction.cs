using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAction
{
    void Execute();
    void Cancel();
    bool CanBeCancelled
    {
        get;
    }
}
